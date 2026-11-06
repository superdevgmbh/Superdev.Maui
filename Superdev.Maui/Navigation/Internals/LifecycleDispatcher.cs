using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Mvvm;

namespace Superdev.Maui.Navigation.Internals
{
    /// <summary>
    /// Invokes the navigation lifecycle callbacks on a page and its view model.
    /// </summary>
    /// <remarks>
    /// Callbacks are delivered to the page first and to its binding context second, so that a page which
    /// implements a lifecycle interface itself is served as well as its view model.
    /// <para>
    /// The dispatcher enforces the balancing rules: the one-shot initialization runs at most once per page,
    /// and <see cref="INavigatedTo"/> and <see cref="INavigatedFrom"/> strictly alternate.
    /// </para>
    /// </remarks>
    internal sealed class LifecycleDispatcher
    {
        private static readonly ConcurrentDictionary<TypedInterfaceKey, Type?> TypedInterfaceCache =
            new ConcurrentDictionary<TypedInterfaceKey, Type?>();

        private static readonly ConcurrentDictionary<(Type InterfaceType, string MethodName), MethodInfo> MethodCache =
            new ConcurrentDictionary<(Type InterfaceType, string MethodName), MethodInfo>();

        private readonly ILogger logger;

        public LifecycleDispatcher(ILogger<LifecycleDispatcher> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Determines whether the page or its view model can receive the given <paramref name="parameter"/>,
        /// i.e. whether it implements a compatible <see cref="IInitialize{TParameter}"/>
        /// or <see cref="INavigatedTo{TParameter}"/>.
        /// </summary>
        public bool CanAcceptParameter(Page page, object parameter)
        {
            var parameterType = parameter.GetType();

            foreach (var target in GetTargets(page))
            {
                if (FindTypedInterface(target.GetType(), typeof(IInitialize<>), parameterType) != null ||
                    FindTypedInterface(target.GetType(), typeof(INavigatedTo<>), parameterType) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Invokes the one-shot initialization callbacks. Does nothing if the page was initialized before.
        /// </summary>
        public async Task InitializeAsync(Page page, object? parameter)
        {
            var context = PageNavigationContext.GetOrAttach(page);
            if (context.IsInitialized)
            {
                return;
            }

            context.IsInitialized = true;

            foreach (var target in GetTargets(page))
            {
                var typedInterface = parameter == null
                    ? null
                    : FindTypedInterface(target.GetType(), typeof(IInitialize<>), parameter.GetType());

                try
                {
                    if (typedInterface != null)
                    {
                        await InvokeAsync(target, typedInterface, nameof(IInitialize<object>.InitializeAsync), parameter);
                    }
                    else if (target is IInitialize initialize)
                    {
                        await initialize.InitializeAsync();
                    }
                }
                finally
                {
                    // BaseViewModel reports IsBusy until it is initialized. The flag is set here rather than
                    // in BaseViewModel itself, so that it is set no matter which initialization callback ran,
                    // and even if the initialization failed.
                    if (target is BaseViewModel baseViewModel)
                    {
                        baseViewModel.IsInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the navigated-to callbacks. Does nothing if the page is already the active page.
        /// </summary>
        public async Task NavigatedToAsync(Page page, NavigationDirection navigationDirection, object? parameter)
        {
            var context = PageNavigationContext.GetOrAttach(page);
            if (context.IsActive)
            {
                return;
            }

            context.IsActive = true;

            foreach (var target in GetTargets(page))
            {
                var typedInterface = parameter == null
                    ? null
                    : FindTypedInterface(target.GetType(), typeof(INavigatedTo<>), parameter.GetType());

                if (typedInterface != null)
                {
                    await InvokeAsync(
                        target, typedInterface, nameof(INavigatedTo<object>.NavigatedToAsync), navigationDirection, parameter);
                }
                else if (target is INavigatedTo navigatedTo)
                {
                    await navigatedTo.NavigatedToAsync(navigationDirection);
                }
            }
        }

        /// <summary>
        /// Invokes the navigated-from callbacks. Does nothing if the page is not the active page.
        /// </summary>
        public async Task NavigatedFromAsync(Page page, NavigationDirection navigationDirection)
        {
            var context = PageNavigationContext.GetOrAttach(page);
            if (!context.IsActive)
            {
                return;
            }

            context.IsActive = false;

            foreach (var target in GetTargets(page))
            {
                if (target is INavigatedFrom navigatedFrom)
                {
                    await navigatedFrom.NavigatedFromAsync(navigationDirection);
                }
            }
        }

        /// <summary>
        /// Invokes the application lifecycle callbacks on the given page.
        /// </summary>
        public async Task ApplicationLifecycleAsync(Page page, bool resumed)
        {
            foreach (var target in GetTargets(page))
            {
                Func<Task>? callback = null;

                if (resumed && target is IApplicationResumed applicationResumed)
                {
                    callback = applicationResumed.OnResumedAsync;
                }
                else if (!resumed && target is IApplicationStopped applicationStopped)
                {
                    callback = applicationStopped.OnStoppedAsync;
                }

                if (callback == null)
                {
                    continue;
                }

                try
                {
                    await callback();
                }
                catch (Exception ex)
                {
                    // Window events are synchronous, so there is no caller which could observe this exception.
                    this.logger.LogError(
                        ex, "{Callback} of {PageType} failed with exception",
                        resumed ? nameof(IApplicationResumed.OnResumedAsync) : nameof(IApplicationStopped.OnStoppedAsync),
                        page.GetType().Name);
                }
            }
        }

        private static IEnumerable<object> GetTargets(Page page)
        {
            yield return page;

            if (page.BindingContext is { } bindingContext && !ReferenceEquals(bindingContext, page))
            {
                yield return bindingContext;
            }
        }

        private static Task InvokeAsync(object target, Type interfaceType, string methodName, params object?[] args)
        {
            var method = MethodCache.GetOrAdd((interfaceType, methodName), static key =>
                key.InterfaceType.GetMethod(key.MethodName)
                ?? throw new NavigationException($"Method \"{key.MethodName}\" not found on {key.InterfaceType.Name}"));

            try
            {
                // Invoking through the interface method honors explicit interface implementations.
                return method.Invoke(target, args) as Task ?? Task.CompletedTask;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is { } callbackException)
            {
                // A callback which throws before its first await surfaces wrapped by reflection, while one which
                // throws after an await, or one reached through the untyped interface, surfaces as itself. Every
                // callback failure reaches the caller in the same shape: as the exception the callback threw.
                return Task.FromException(callbackException);
            }
        }

        /// <summary>
        /// Finds the closed generic interface of <paramref name="openInterface"/> implemented by
        /// <paramref name="targetType"/> whose type argument accepts <paramref name="parameterType"/>.
        /// If several match, the most derived one wins.
        /// </summary>
        /// <exception cref="NavigationParameterException">If two unrelated candidates match.</exception>
        internal static Type? FindTypedInterface(Type targetType, Type openInterface, Type parameterType)
        {
            var key = new TypedInterfaceKey(targetType, openInterface, parameterType);
            return TypedInterfaceCache.GetOrAdd(key, static k => ResolveTypedInterface(k));
        }

        private static Type? ResolveTypedInterface(TypedInterfaceKey key)
        {
            var candidates = key.TargetType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == key.OpenInterface)
                .Where(i => i.GetGenericArguments()[0].IsAssignableFrom(key.ParameterType))
                .ToArray();

            if (candidates.Length == 0)
            {
                return null;
            }

            if (candidates.Length == 1)
            {
                return candidates[0];
            }

            // The most derived candidate is the one whose type argument is assignable to all other candidates'.
            var mostDerived = candidates
                .Where(c => candidates.All(o => o.GetGenericArguments()[0].IsAssignableFrom(c.GetGenericArguments()[0])))
                .ToArray();

            if (mostDerived.Length != 1)
            {
                var interfaceName = key.OpenInterface.Name.Split('`')[0];
                throw new NavigationParameterException(
                    $"Type \"{key.TargetType.Name}\" implements ambiguous {interfaceName} interfaces for parameter " +
                    $"type \"{key.ParameterType.Name}\": {string.Join(", ", candidates.Select(c => c.GetGenericArguments()[0].Name))}");
            }

            return mostDerived[0];
        }

        private readonly record struct TypedInterfaceKey(Type TargetType, Type OpenInterface, Type ParameterType);
    }
}

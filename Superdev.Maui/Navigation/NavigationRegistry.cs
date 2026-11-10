using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Superdev.Maui.Navigation
{
    /// <inheritdoc cref="INavigationRegistry"/>
    /// <remarks>
    /// Lookups happen on the caller's thread, before the navigation pipeline marshals to the UI thread,
    /// so two navigations started from different threads read this registry at the same time. The index
    /// of the explicit registrations is built once and never modified afterwards; everything which is
    /// written later - convention hits, misses and the assemblies they were found in - lives in a
    /// concurrent collection.
    /// </remarks>
    internal sealed class NavigationRegistry : INavigationRegistry
    {
        private const string PageSuffix = "Page";
        private const string ViewModelSuffix = "ViewModel";

        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;
        private readonly NavigationOptions navigationOptions;
        private readonly Lazy<RegistrationIndex> index;

        private int conventionWarningIssued;

        /// <summary>
        /// The assemblies which previously produced a convention match. They are searched first, so that
        /// an application whose pages live in one assembly scans the others at most once.
        /// </summary>
        private readonly ConcurrentDictionary<Assembly, byte> cachedAssemblies = new ConcurrentDictionary<Assembly, byte>();

        /// <summary>
        /// Navigation names which the convention scan already failed to resolve. Without this, every
        /// lookup of an unregistered name walks the types of every loaded assembly again.
        /// </summary>
        private readonly ConcurrentDictionary<string, byte> unresolvableNames = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        public NavigationRegistry(
            ILogger<NavigationRegistry> logger,
            IServiceProvider serviceProvider,
            NavigationOptions navigationOptions)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.navigationOptions = navigationOptions;

            // A registration set which fails to index, e.g. because a name is registered twice with
            // different types, fails on every lookup: the exception is cached with the index.
            this.index = new Lazy<RegistrationIndex>(this.BuildIndex, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IReadOnlyList<PageRegistration> Registrations => this.index.Value.ByName.Values.ToArray();

        public PageRegistration GetRegistration(string pageName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pageName);

            if (this.TryGetRegistration(pageName, out var registration))
            {
                return registration;
            }

            throw new PageResolveException(
                $"Page with name \"{pageName}\" is not registered. " +
                $"Use {nameof(ServiceCollectionExtensions.RegisterForNavigation)} to register it.");
        }

        public PageRegistration GetRegistration(Type viewModelType)
        {
            ArgumentNullException.ThrowIfNull(viewModelType);

            var index = this.index.Value;

            if (index.AmbiguousViewModelTypes.Contains(viewModelType))
            {
                throw new PageResolveException(
                    $"More than one page is registered with view model type \"{viewModelType.Name}\". " +
                    $"Navigate by page name instead.");
            }

            if (index.ByViewModelType.TryGetValue(viewModelType, out var registration))
            {
                return registration;
            }

            throw new PageResolveException(
                $"No page is registered with view model type \"{viewModelType.Name}\". " +
                $"Use {nameof(ServiceCollectionExtensions.RegisterForNavigation)} to register it.");
        }

        public bool TryGetRegistration(string pageName, [NotNullWhen(true)] out PageRegistration? registration)
        {
            var index = this.index.Value;

            if (index.ByName.TryGetValue(pageName, out registration))
            {
                return true;
            }

            if (this.navigationOptions.EnableConventionBasedResolution && !this.unresolvableNames.ContainsKey(pageName))
            {
                registration = this.ResolveByConvention(pageName);
                if (registration != null)
                {
                    if (Interlocked.Exchange(ref this.conventionWarningIssued, 1) == 0)
                    {
                        // Once per process: the scan works, but a trimmed or ahead-of-time compiled build may
                        // not contain the types it looks for, and the miss would only show up on a device.
                        this.logger.LogWarning(
                            "Page \"{PageName}\" is not registered and was resolved by scanning the loaded assemblies. " +
                            "Register pages with {Method}; a trimmed or ahead-of-time compiled build cannot resolve them by name. " +
                            "This warning is issued once.",
                            pageName, nameof(ServiceCollectionExtensions.RegisterForNavigation));
                    }

                    // Cache the convention-based result, so that the assembly scan happens at most once per
                    // name. Two threads which scanned the same name at once found the same type; the first
                    // one to store its result wins and the other one reports that result as well.
                    registration = index.ByName.GetOrAdd(pageName, registration);
                    return true;
                }

                // Remember the miss as well, so that a name which cannot be resolved does not scan the
                // loaded assemblies again on every navigation.
                this.unresolvableNames.TryAdd(pageName, 0);
            }

            registration = null;
            return false;
        }

        private RegistrationIndex BuildIndex()
        {
            var byName = new ConcurrentDictionary<string, PageRegistration>(StringComparer.OrdinalIgnoreCase);
            var byViewModelType = new Dictionary<Type, PageRegistration>();
            var ambiguous = new HashSet<Type>();

            var allRegistrations = this.serviceProvider
                .GetKeyedServices<PageRegistration>(KeyedService.AnyKey)
                .Where(r => r.Name != null)
                .ToArray();

            foreach (var group in allRegistrations.GroupBy(r => r.Name!, StringComparer.OrdinalIgnoreCase))
            {
                var registration = group.First();

                if (group.Count() > 1)
                {
                    var conflicting = group
                        .Where(r => r.PageType != registration.PageType || r.ViewModelType != registration.ViewModelType)
                        .ToArray();

                    if (conflicting.Length > 0)
                    {
                        throw new NavigationException(
                            $"Navigation name \"{group.Key}\" is registered more than once with different types: " +
                            $"{string.Join(", ", group.Select(r => r.ToString()))}. " +
                            $"Navigation names must be unique.")
                        {
                            PageName = group.Key
                        };
                    }

                    this.logger.LogWarning(
                        "Navigation name \"{PageName}\" is registered {Count} times with identical types",
                        group.Key, group.Count());
                }

                byName[group.Key] = registration;

                if (registration.ViewModelType is { } viewModelType)
                {
                    if (byViewModelType.ContainsKey(viewModelType))
                    {
                        byViewModelType.Remove(viewModelType);
                        ambiguous.Add(viewModelType);
                    }
                    else if (!ambiguous.Contains(viewModelType))
                    {
                        byViewModelType[viewModelType] = registration;
                    }
                }
            }

            return new RegistrationIndex(byName, byViewModelType, ambiguous);
        }

        private PageRegistration? ResolveByConvention(string pageName)
        {
            // Only pages qualify: without this filter any type which happens to carry the navigation name
            // would be accepted here and fail much later, when the page factory cannot cast it.
            var pageTypes = this.FindTypesWithName(pageName)
                .Where(t => typeof(Page).IsAssignableFrom(t))
                .ToArray();

            if (pageTypes.Length == 0)
            {
                return null;
            }

            if (pageTypes.Length > 1)
            {
                throw new PageResolveException(
                    $"Multiple pages found for name \"{pageName}\": " +
                    $"{string.Join(", ", pageTypes.Select(t => t.FullName))}");
            }

            var pageType = pageTypes[0];
            var viewModelType = this.FindViewModelTypeByConvention(pageName);

            this.logger.LogDebug(
                "Resolved page \"{PageName}\" by convention to {PageType}", pageName, pageType.FullName);

            return new PageRegistration
            {
                Name = pageName,
                PageType = pageType,
                ViewModelType = viewModelType
            };
        }

        private Type? FindViewModelTypeByConvention(string pageName)
        {
            var pageSuffixIndex = pageName.LastIndexOf(PageSuffix, StringComparison.OrdinalIgnoreCase);
            if (pageSuffixIndex <= 0)
            {
                return null;
            }

            var viewModelName = string.Concat(pageName.AsSpan(0, pageSuffixIndex), ViewModelSuffix);
            var viewModelTypes = this.FindTypesWithName(viewModelName);

            if (viewModelTypes.Length == 1)
            {
                return viewModelTypes[0];
            }

            if (viewModelTypes.Length > 1)
            {
                throw new PageResolveException(
                    $"Multiple view models found for name \"{viewModelName}\": " +
                    $"{string.Join(", ", viewModelTypes.Select(t => t.FullName))}");
            }

            this.logger.LogDebug("View model with name \"{ViewModelName}\" not found", viewModelName);
            return null;
        }

        private Type[] FindTypesWithName(string typeName)
        {
            // Attempt to look the type name up in the assemblies which previously produced a match.
            // Keys is a snapshot, so an assembly added by a concurrent scan does not disturb this one.
            var types = FindTypesWithName(this.cachedAssemblies.Keys, typeName);

            if (types.Length == 0)
            {
                types = FindTypesWithName(AppDomain.CurrentDomain.GetAssemblies(), typeName);

                foreach (var type in types)
                {
                    this.cachedAssemblies.TryAdd(type.Assembly, 0);
                }
            }

            return types;
        }

        private static Type[] FindTypesWithName(IEnumerable<Assembly> assemblies, string typeName)
        {
            return assemblies
                .SelectMany(a => GetTypesSafe(a).Where(t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase)))
                .ToArray();
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }

        /// <summary>
        /// The explicit registrations, indexed once. The name index also receives the convention-based
        /// hits, which is why it is a concurrent dictionary; the other two never change after the build.
        /// </summary>
        private sealed class RegistrationIndex
        {
            public RegistrationIndex(
                ConcurrentDictionary<string, PageRegistration> byName,
                IReadOnlyDictionary<Type, PageRegistration> byViewModelType,
                IReadOnlySet<Type> ambiguousViewModelTypes)
            {
                this.ByName = byName;
                this.ByViewModelType = byViewModelType;
                this.AmbiguousViewModelTypes = ambiguousViewModelTypes;
            }

            public ConcurrentDictionary<string, PageRegistration> ByName { get; }

            public IReadOnlyDictionary<Type, PageRegistration> ByViewModelType { get; }

            public IReadOnlySet<Type> AmbiguousViewModelTypes { get; }
        }
    }
}

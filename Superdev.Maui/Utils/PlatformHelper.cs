namespace Superdev.Maui.Utils
{
    public static class PlatformHelper
    {
        private static readonly string CurrentPlatform = DeviceInfo.Current.Platform.ToString();

        public static T OnPlatform<T>(T ios, T android)
        {
            var deviceInfo = DeviceInfo.Current;
            if (deviceInfo.Platform == DevicePlatform.iOS)
            {
                return ios;
            }

            if (deviceInfo.Platform == DevicePlatform.Android)
            {
                return android;
            }


            return default!;
        }

        /// <summary>
        /// Define functions for platforms which return the specified values for the current <seealso cref="Device.RuntimePlatform"/>.
        /// </summary>
        /// <typeparam name="T">The platform-specific value.</typeparam>
        /// <param name="platformFactories">The value providers for each platform.</param>
        /// <returns>Returns a single platform-specific value.</returns>
        public static T? OnPlatformValue<T>(params (DevicePlatform, Func<T>)[] platformFactories)
        {
            return OnPlatformValues(platformFactories).SingleOrDefault();
        }

        /// <summary>
        /// Define functions for platforms which return the specified values for the current <seealso cref="Device.RuntimePlatform"/>.
        /// </summary>
        /// <typeparam name="T">The platform-specific value.</typeparam>
        /// <param name="platformFactories">The value providers for each platform.</param>
        /// <returns>Returns multiple platform-specific value.</returns>
        public static IEnumerable<T> OnPlatformValues<T>(params (DevicePlatform Platform, Func<T> Function)[] platformFactories)
        {
            var deviceInfo = DeviceInfo.Current;
            var functions = platformFactories
                .Where(pf => pf.Platform == deviceInfo.Platform)
                .Select(pf => pf.Function);

            foreach (var func in functions)
            {
                yield return func();
            }
        }

        /// <summary>
        /// Runs actions on specified platforms.
        /// </summary>
        /// <example>
        /// PlatformHelper.RunOnPlatform(
        /// (Device.iOS, () =&gt;{ iosCalls++; }),
        /// (Device.Android, ()=&gt;{ androidCalls++; }));
        /// </example>
        public static void RunOnPlatform(params (DevicePlatform Platform, Action Action)[] platformActions)
        {
            var deviceInfo = DeviceInfo.Current;
            var actions = platformActions
                .Where(pf => pf.Platform == deviceInfo.Platform)
                .Select(pf => pf.Action);

            foreach (var action in actions)
            {
                action();
            }
        }

        public static T? GetValue<T>(object res)
        {
            var onPlatform = (OnPlatform<T>)res;
            var value = onPlatform.Platforms.FirstOrDefault(p => p.Platform[0] == CurrentPlatform)?.Value;
            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return default;
        }
    }
}
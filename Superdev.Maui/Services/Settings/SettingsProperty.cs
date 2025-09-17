using System.Linq.Expressions;
using Superdev.Maui.Internals;

namespace Superdev.Maui.Services.Settings
{
    public class SettingsProperty<T> : ISettingsProperty
    {
        private readonly IPreferences preferences;
        private readonly string key;
        private readonly T defaultValue;
        
        private CachedValue<T> cachedValue;

        public SettingsProperty(IPreferences preferences, Expression<Func<T>> expression, T defaultValue = default)
            : this(preferences, ((MemberExpression)expression.Body).Member.Name, defaultValue)
        {
        }

        public SettingsProperty(IPreferences preferences, string key, T defaultValue = default)
        {
            Guard.ArgumentNotNull(preferences, nameof(preferences));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));

            if (key.Length > 255)
            {
                throw new ArgumentException($"{nameof(key)} must not exceed length of 255 characters", nameof(key));
            }

            this.preferences = preferences;
            this.key = key;
            this.defaultValue = defaultValue;
        }

        /// <summary>
        ///     Turns property value caching on/off.
        ///     Default: <c>True</c> = on.
        /// </summary>
        public bool CachingEnabled { get; set; } = true;

        public T Value
        {
            get
            {
                if (this.CachingEnabled && this.cachedValue.HasValue)
                {
                    return this.cachedValue.Value;
                }

                var value = this.preferences.Get<T>(this.key, this.defaultValue);

                if (this.CachingEnabled)
                {
                    this.cachedValue.Value = value;
                }

                return value;
            }
            set
            {
                if (this.CachingEnabled)
                {
                    this.cachedValue.Value = value;
                }

                this.preferences.Set<T>(this.key, value);
            }
        }

        object ISettingsProperty.Value
        {
            get => this.Value;
            set => this.Value = (T)value;
        }
    }

    internal struct CachedValue<T>
    {
        private T value;

        public bool HasValue { get; private set; }

        public T Value
        {
            get => this.value;
            set
            {
                this.value = value;
                this.HasValue = true;
            }
        }
    }

    public interface ISettingsProperty
    {
        object Value { get; set; }
    }
}
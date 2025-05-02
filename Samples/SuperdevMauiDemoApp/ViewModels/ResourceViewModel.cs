using Superdev.Maui.Extensions;
using Superdev.Maui.Resources.Styles;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class FontElementResourceViewModel : ResourceViewModel<FontElement>
    {
        public FontElementResourceViewModel(string key, FontElement value)
            : base(key, value)
        {
        }
    }

    public class ColorResourceViewModel : ResourceViewModel<Color>
    {
        public ColorResourceViewModel(string key, Color value)
            : base(key, value)
        {
        }
    }

    public class ObjectResourceViewModel : ResourceViewModel<object>
    {
        public ObjectResourceViewModel(KeyValuePair<string, object> r)
            : base(r.Key, r.Value)
        {
        }
    }

    public class ResourceViewModel<T> : ResourceViewModel where T : class
    {
        public ResourceViewModel(string key, T value)
            : base(key, value)
        {
        }

        public T Value => base.Value as T;
    }

    public class ResourceViewModel
    {
        public ResourceViewModel(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; }

        public object Value { get; }

        public string ResourceType
        {
            get
            {
                if (this.Value != null)
                {
                    var type = this.Value.GetType();
                    if (type.GetInterfaces().Contains(typeof(IValueConverter)))
                    {
                        return "IValueConverter";
                    }

                    return type.GetFormattedName();
                }

                return null;
            }
        }

        public override string ToString()
        {
            return this.Key;
        }
    }
}
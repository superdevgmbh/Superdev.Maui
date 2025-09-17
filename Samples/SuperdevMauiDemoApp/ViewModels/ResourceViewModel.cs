using Superdev.Maui.Extensions;
using Superdev.Maui.Resources.Styles;
using Font = Microsoft.Maui.Font;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class ColorResourceViewModel : ResourceViewModel<Color>
    {
        public ColorResourceViewModel(string key, Color value)
            : base(key, value)
        {
        }
    }

    public class FontResourceViewModel : ResourceViewModel<FontElement>
    {
        public FontResourceViewModel(string key, Font value)
            : this(key, new FontElement { FontAttributes = value.GetFontAttributes(), FontFamily = value.Family, FontSize = value.Size })
        {
        }

        public FontResourceViewModel(string key, FontElement value)
            : base(key, value)
        {
        }
    }

    public class ObjectResourceViewModel : ResourceViewModel<object>
    {
        public ObjectResourceViewModel(KeyValuePair<string, object> r)
            : this(r.Key, r.Value)
        {
        }

        public ObjectResourceViewModel(string key, object value)
            : base(key, value)
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
namespace Superdev.Maui.Platforms.Handlers
{
    public class CustomEditorHandler : EditorHandler
    {
        public CustomEditorHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomEditorHandler()
            : base(Mapper)
        {
        }
    }
}
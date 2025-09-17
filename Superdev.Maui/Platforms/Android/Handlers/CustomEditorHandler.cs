using System.Diagnostics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Effects;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<CustomEditor, CustomEditorHandler>;

    public class CustomEditorHandler : EditorHandler
    {
        public new static readonly PM Mapper = new PM(EditorHandler.Mapper)
        {
            [nameof(CustomEditor.Background)] = MapBackground,
            [nameof(CustomEditor.BackgroundColor)] = MapBackgroundColor,
        };

        public CustomEditorHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public CustomEditorHandler()
            : base(Mapper)
        {
        }

        private static void MapBackground(CustomEditorHandler customEditorHandler, CustomEditor customEditor)
        {
            // Debug.WriteLine("MapBackground");
        }

        private static void MapBackgroundColor(CustomEditorHandler customEditorHandler, CustomEditor customEditor)
        {
            // Debug.WriteLine("MapBackgroundColor");

            var lineColor = LineColorEffect.GetLineColor(customEditor);
            if (lineColor == null)
            {
                customEditorHandler.PlatformView.UpdateBackground(customEditor);
            }
        }
    }
}
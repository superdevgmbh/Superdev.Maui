using System.Diagnostics;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Services;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<ContentPage, ContentPageHandler>;

    public class ContentPageHandler : PageHandler
    {
        private readonly IKeyboardService keyboardService;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.PageHandler.Mapper)
        {
            [PageExtensions.HasKeyboardOffset] = UpdateHasKeyboardOffset
        };

        public ContentPageHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
            this.keyboardService = IKeyboardService.Current;
        }

        public ContentPageHandler()
            : base(Mapper)
        {
            this.keyboardService = IKeyboardService.Current;
        }

        private static void UpdateHasKeyboardOffset(ContentPageHandler contentPageHandler, ContentPage contentPage)
        {
            var hasKeyboardOffset = PageExtensions.GetHasKeyboardOffset(contentPage);
            var keyboardService = contentPageHandler.keyboardService;

            if (hasKeyboardOffset != null)
            {
                var windowSoftInputModeAdjust = hasKeyboardOffset == true
                    ? WindowSoftInputModeAdjust.Resize
                    : WindowSoftInputModeAdjust.Pan;

                keyboardService.UseWindowSoftInputModeAdjust(contentPage, windowSoftInputModeAdjust);
            }
        }

        protected override void DisconnectHandler(ContentViewGroup platformView)
        {
            var contentPage = (ContentPage)this.VirtualView;
            this.keyboardService.ResetWindowSoftInputModeAdjust(contentPage);

            base.DisconnectHandler(platformView);
        }
    }
}
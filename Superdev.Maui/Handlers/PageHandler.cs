using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using Superdev.Maui.Extensions;

namespace Superdev.Maui.Handlers
{
    public class PageHandler : Microsoft.Maui.Handlers.PageHandler
    {
        private readonly ILogger logger;
        private readonly SuperdevMauiOptions options;

        public PageHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
            this.logger = IPlatformApplication.Current.Services.GetService<ILogger<PageHandler>>();
            this.options = IPlatformApplication.Current.Services.GetService<SuperdevMauiOptions>();
        }

        public PageHandler()
            : this(null)
        {
        }

#if ANDROID
        protected override void DisconnectHandler(ContentViewGroup platformView)
#elif IOS
        protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView platformView)
#else
        protected override void DisconnectHandler(object platformView)
#endif
        {
            if (this.options.AutoCleanupPage)
            {
                if (this.VirtualView is ContentPage contentPage)
                {
                    this.Cleanup(contentPage);
                }
            }

            base.DisconnectHandler(platformView);
        }

        private void Cleanup(IVisualTreeElement element)
        {
            try
            {
                if (element is VisualElement visualElement)
                {
                    var behaviorsCount = visualElement.Behaviors.Count;
                    var triggersCount = visualElement.Triggers.Count;
                    var effectsCount = visualElement.Effects.Count;

                    var hasBehaviors = behaviorsCount > 0;
                    var hasTriggers = triggersCount > 0;
                    var hasEffects = effectsCount > 0;

                    if (hasBehaviors || hasTriggers || hasEffects)
                    {
                        var message = $"Cleanup for {visualElement.GetType().GetFormattedName()}: " +
                                      $"{(hasBehaviors ? $"{Environment.NewLine}> Behaviors.Count={behaviorsCount}" : "")}" +
                                      $"{(hasTriggers ? $"{Environment.NewLine}> Triggers.Count={triggersCount}" : "")}" +
                                      $"{(hasEffects ? $"{Environment.NewLine}> Effects.Count={effectsCount}" : "")}";

                        this.logger.LogDebug(message);

                        if (hasBehaviors)
                        {
                            visualElement.Behaviors.Clear();
                        }

                        if (hasTriggers)
                        {
                            visualElement.Triggers.Clear();
                        }

                        if (hasEffects)
                        {
                            visualElement.Effects.Clear();
                        }
                    }
                }

                foreach (var child in element.GetVisualChildren())
                {
                    this.Cleanup(child);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Cleanup failed with exception");
            }
        }
    }
}
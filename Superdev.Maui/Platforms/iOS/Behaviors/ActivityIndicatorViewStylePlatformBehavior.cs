using System.Diagnostics;
using Microsoft.Maui.Platform;
using Superdev.Maui.Behaviors;
using UIKit;

namespace Superdev.Maui.Platforms.Behaviors
{
    public partial class ActivityIndicatorViewStylePlatformBehavior
    {
        private MauiActivityIndicator control;

        /// <inheritdoc/>
        protected override void OnAttachedTo(ActivityIndicator bindable, UIView platformView)
        {
            if (platformView is not MauiActivityIndicator control)
            {
                throw new NotSupportedException(
                    $"Control of type {platformView.GetType().Name} is not supported by {this.GetType().Name}.");
            }

            this.control = control;
            this.UpdateViewStyle();
        }

        /// <inheritdoc/>
        protected override void OnDetachedFrom(ActivityIndicator bindable, UIView platformView)
        {
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (this.control is null)
            {
                return;
            }

            if (propertyName == ViewStyleProperty.PropertyName)
            {
                this.UpdateViewStyle();
            }
        }

        private void UpdateViewStyle()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                return;
            }

            try
            {
                this.control.ActivityIndicatorViewStyle = MapActivityIndicatorViewStyle(this.ViewStyle);
            }
            catch (Exception)
            {
                Trace.WriteLine("UpdateViewStyle failed with exception");
            }
        }

        private static UIActivityIndicatorViewStyle MapActivityIndicatorViewStyle(ActivityIndicatorViewStyle activityIndicatorViewStyle)
        {
            switch (activityIndicatorViewStyle)
            {
                case ActivityIndicatorViewStyle.Large:
                    return UIActivityIndicatorViewStyle.Large;
                default:
                    return UIActivityIndicatorViewStyle.Medium;
            }
        }
    }
}
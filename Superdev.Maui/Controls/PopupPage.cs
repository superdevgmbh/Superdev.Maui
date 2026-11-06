using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Superdev.Maui.Controls.Animations;
using NavigationPage = Microsoft.Maui.Controls.NavigationPage;

namespace Superdev.Maui.Controls
{
    /// <summary>
    /// A page which is displayed as a popup above the current page, e.g. a context menu,
    /// a tooltip or a bottom sheet.
    /// </summary>
    /// <remarks>
    /// A popup page is presented as a transparent modal page, so it covers the navigation bar and the tab bar
    /// of the page below. Set a translucent <see cref="VisualElement.BackgroundColor"/>, e.g. <c>#80000000</c>,
    /// to dim what is behind it, and lay the content out with the usual layout options, e.g.
    /// <c>VerticalOptions="End"</c> for a bottom sheet.
    /// <para>
    /// Popup pages are registered and navigated to like any other page: register them with
    /// <c>RegisterForNavigation</c> and open them with <c>PushAsync</c>, which gives them the same
    /// service scope, navigation parameters, lifecycle callbacks and navigation guards as any other page.
    /// </para>
    /// </remarks>
    [ContentProperty(nameof(Content))]
    public class PopupPage : ContentPage
    {
        /// <summary>
        /// Identifies the <see cref="Animation"/> bindable property.
        /// </summary>
        public static readonly BindableProperty AnimationProperty = BindableProperty.Create(
            nameof(Animation), typeof(IPopupAnimation), typeof(PopupPage), defaultValueCreator: _ => new ScaleAnimation());

        /// <summary>
        /// Identifies the <see cref="IsAnimationEnabled"/> bindable property.
        /// </summary>
        public static readonly BindableProperty IsAnimationEnabledProperty = BindableProperty.Create(
            nameof(IsAnimationEnabled), typeof(bool), typeof(PopupPage), defaultValue: true);

        /// <summary>
        /// Identifies the <see cref="IsBackgroundAnimationEnabled"/> bindable property.
        /// </summary>
        public static readonly BindableProperty IsBackgroundAnimationEnabledProperty = BindableProperty.Create(
            nameof(IsBackgroundAnimationEnabled), typeof(bool), typeof(PopupPage), defaultValue: true);

        /// <summary>
        /// Identifies the <see cref="CloseWhenBackgroundIsClicked"/> bindable property.
        /// </summary>
        public static readonly BindableProperty CloseWhenBackgroundIsClickedProperty = BindableProperty.Create(
            nameof(CloseWhenBackgroundIsClicked), typeof(bool), typeof(PopupPage), defaultValue: true);

        /// <summary>
        /// Identifies the <see cref="BackgroundClickedCommand"/> bindable property.
        /// </summary>
        public static readonly BindableProperty BackgroundClickedCommandProperty = BindableProperty.Create(
            nameof(BackgroundClickedCommand), typeof(ICommand), typeof(PopupPage));

        /// <summary>
        /// Identifies the <see cref="HasSystemPadding"/> bindable property.
        /// </summary>
        public static readonly BindableProperty HasSystemPaddingProperty = BindableProperty.Create(
            nameof(HasSystemPadding), typeof(bool), typeof(PopupPage), defaultValue: true,
            propertyChanged: OnHasSystemPaddingChanged);

        /// <summary>
        /// Identifies the <see cref="HasKeyboardOffset"/> bindable property.
        /// </summary>
        public static readonly BindableProperty HasKeyboardOffsetProperty = BindableProperty.Create(
            nameof(HasKeyboardOffset), typeof(bool), typeof(PopupPage), defaultValue: true,
            propertyChanged: OnHasKeyboardOffsetChanged);

        private readonly Grid rootLayout;
        private readonly BoxView backgroundLayer;
        private const string BackgroundAnimationName = "PopupBackground";

        private Color? backgroundColor;
        private View? popupContent;

        /// <inheritdoc cref="PopupPage"/>
        public PopupPage()
        {
            this.backgroundLayer = new BoxView
            {
                Color = Colors.Transparent,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            this.backgroundLayer.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(this.OnBackgroundClicked)
            });

            this.rootLayout = new Grid();
            this.rootLayout.Add(this.backgroundLayer);

            base.Content = this.rootLayout;

            this.BackgroundColor = Colors.Transparent;
            NavigationPage.SetHasNavigationBar(this, false);

            // A transparent modal page needs this presentation style on iOS, otherwise the page below is removed
            // while the popup is displayed and the popup appears on an empty background.
            this.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.OverFullScreen);

            PageExtensions.SetHasKeyboardOffset(this, this.HasKeyboardOffset);
            this.On<iOS>().SetUseSafeArea(this.HasSystemPadding);
        }

        /// <summary>
        /// Occurs when the user tapped the background of the popup, i.e. outside of its content.
        /// </summary>
        public event EventHandler? BackgroundClicked;

        /// <summary>
        /// Gets or sets the content of this popup page.
        /// </summary>
        public new View? Content
        {
            get => this.popupContent;
            set
            {
                if (ReferenceEquals(this.popupContent, value))
                {
                    return;
                }

                if (this.popupContent != null)
                {
                    this.rootLayout.Remove(this.popupContent);
                }

                this.popupContent = value;

                if (value != null)
                {
                    // The content is laid out above the background layer, so that a tap on the content
                    // does not reach the background.
                    this.rootLayout.Add(value);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the animation which is played when the popup appears and disappears.
        /// Default: a <see cref="ScaleAnimation"/>.
        /// </summary>
        public IPopupAnimation? Animation
        {
            get => (IPopupAnimation?)this.GetValue(AnimationProperty);
            set => this.SetValue(AnimationProperty, value);
        }

        /// <summary>
        /// Enables the <see cref="Animation"/>. Default: <c>true</c>.
        /// </summary>
        public bool IsAnimationEnabled
        {
            get => (bool)this.GetValue(IsAnimationEnabledProperty);
            set => this.SetValue(IsAnimationEnabledProperty, value);
        }

        /// <summary>
        /// Closes the popup when the user taps its background. Default: <c>true</c>.
        /// </summary>
        /// <remarks>
        /// A popup whose view or view model implements <see cref="Navigation.IConfirmNavigation"/>
        /// asks the guard before it closes, just like any other navigation.
        /// </remarks>
        public bool CloseWhenBackgroundIsClicked
        {
            get => (bool)this.GetValue(CloseWhenBackgroundIsClickedProperty);
            set => this.SetValue(CloseWhenBackgroundIsClickedProperty, value);
        }

        /// <summary>
        /// Fades the dimmed background in and out together with the content. Default: <c>true</c>.
        /// </summary>
        public bool IsBackgroundAnimationEnabled
        {
            get => (bool)this.GetValue(IsBackgroundAnimationEnabledProperty);
            set => this.SetValue(IsBackgroundAnimationEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the command which is executed when the user tapped the background of the popup.
        /// </summary>
        public ICommand? BackgroundClickedCommand
        {
            get => (ICommand?)this.GetValue(BackgroundClickedCommandProperty);
            set => this.SetValue(BackgroundClickedCommandProperty, value);
        }

        /// <summary>
        /// Keeps the content within the safe area of the screen. Default: <c>true</c>.
        /// </summary>
        public bool HasSystemPadding
        {
            get => (bool)this.GetValue(HasSystemPaddingProperty);
            set => this.SetValue(HasSystemPaddingProperty, value);
        }

        /// <summary>
        /// Moves the content up when the on-screen keyboard appears. Default: <c>true</c>.
        /// </summary>
        public bool HasKeyboardOffset
        {
            get => (bool)this.GetValue(HasKeyboardOffsetProperty);
            set => this.SetValue(HasKeyboardOffsetProperty, value);
        }

        /// <summary>
        /// Plays the appearing animation. Called by the navigation service after the popup was displayed.
        /// </summary>
        internal Task PlayAppearingAnimationAsync()
        {
            return this.PlayAnimationAsync(appearing: true);
        }

        /// <summary>
        /// Plays the disappearing animation. Called by the navigation service before the popup is removed.
        /// </summary>
        internal Task PlayDisappearingAnimationAsync()
        {
            return this.PlayAnimationAsync(appearing: false);
        }

        private async Task PlayAnimationAsync(bool appearing)
        {
            if (this.GetAnimation() is not { } animation || this.popupContent is not { } content)
            {
                return;
            }

            try
            {
                await this.OnAnimationStartingAsync(appearing);

                await Task.WhenAll(
                    appearing
                        ? animation.AppearingAsync(content, this)
                        : animation.DisappearingAsync(content, this),
                    this.AnimateBackgroundAsync(appearing, animation));

                await this.OnAnimationCompletedAsync(appearing);
            }
            catch (Exception ex)
            {
                // An animation is cosmetic: if it cannot run, the popup must still be shown or closed.
                // The content and the dimmed background are reset, so that they do not stay in the initial
                // state of the animation.
                animation.Reset(content);
                this.ResetBackground();

                var logger = IPlatformApplication.Current?.Services.GetService<ILogger<PopupPage>>();
                logger?.LogWarning(ex, "The popup animation failed and was skipped");
            }
        }

        /// <summary>
        /// Fades the dimmed background along with the content, so that the popup does not appear on top of
        /// a scrim which is already fully dark.
        /// </summary>
        private Task AnimateBackgroundAsync(bool appearing, IPopupAnimation animation)
        {
            if (!this.IsBackgroundAnimationEnabled ||
                animation is not PopupAnimationBase animationBase ||
                this.BackgroundColor is not { } backgroundColor)
            {
                return Task.CompletedTask;
            }

            this.backgroundColor ??= backgroundColor;
            var targetAlpha = this.backgroundColor.Alpha;

            var from = appearing ? 0f : targetAlpha;
            var to = appearing ? targetAlpha : 0f;
            var duration = appearing ? animationBase.DurationIn : animationBase.DurationOut;
            var easing = appearing ? animationBase.EasingIn : animationBase.EasingOut;

            var taskCompletionSource = new TaskCompletionSource();
            var backgroundAnimation = new Animation(
                alpha => this.BackgroundColor = this.backgroundColor.WithAlpha((float)alpha), from, to);

            backgroundAnimation.Commit(
                this, BackgroundAnimationName, length: duration, easing: easing,
                finished: (_, _) => taskCompletionSource.TrySetResult());

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Restores the dimmed background, so that a failed animation does not leave the popup without it.
        /// </summary>
        private void ResetBackground()
        {
            this.AbortAnimation(BackgroundAnimationName);

            if (this.backgroundColor is { } backgroundColor)
            {
                this.BackgroundColor = backgroundColor;
            }
        }

        /// <summary>
        /// Called before the appearing or disappearing animation runs. Override in order to prepare the
        /// content, e.g. to start a related animation of your own.
        /// </summary>
        /// <param name="appearing"><c>true</c> for the appearing animation, <c>false</c> for the disappearing one.</param>
        protected virtual Task OnAnimationStartingAsync(bool appearing)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after the appearing or disappearing animation completed. Not called when the animation
        /// failed or was skipped.
        /// </summary>
        /// <param name="appearing"><c>true</c> for the appearing animation, <c>false</c> for the disappearing one.</param>
        protected virtual Task OnAnimationCompletedAsync(bool appearing)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Puts the content into the initial state of the appearing animation,
        /// so that it is not visible before the animation starts.
        /// </summary>
        internal void PrepareAnimation()
        {
            if (this.GetAnimation() is not { } animation || this.popupContent is not { } content)
            {
                return;
            }

            animation.Prepare(content);

            // The dimmed background is faded in together with the content, so it has to start out
            // transparent. Without this the popup appears on a background which is already fully dark.
            if (this.IsBackgroundAnimationEnabled && this.BackgroundColor is { } backgroundColor)
            {
                this.backgroundColor ??= backgroundColor;
                this.BackgroundColor = this.backgroundColor.WithAlpha(0f);
            }
        }

        private IPopupAnimation? GetAnimation()
        {
            return this.IsAnimationEnabled && !AreSystemAnimationsDisabled() ? this.Animation : null;
        }

        /// <summary>
        /// Determines whether the operating system asks applications not to animate, e.g. because the user
        /// enabled a reduced motion setting.
        /// </summary>
        private static bool AreSystemAnimationsDisabled()
        {
            try
            {
#if ANDROID
                var contentResolver = Android.App.Application.Context.ContentResolver;
                if (contentResolver == null)
                {
                    return false;
                }

                var animatorScale = Android.Provider.Settings.Global.GetFloat(
                    contentResolver, Android.Provider.Settings.Global.AnimatorDurationScale, 1f);

                return animatorScale == 0f;
#elif IOS
                return UIKit.UIAccessibility.IsReduceMotionEnabled;
#else
                return false;
#endif
            }
            catch
            {
                // The setting is a nicety: if it cannot be read, the animation runs as usual.
                return false;
            }
        }

        private void OnBackgroundClicked()
        {
            this.RaiseBackgroundClicked();
        }

        /// <summary>
        /// Raises <see cref="BackgroundClicked"/> and executes <see cref="BackgroundClickedCommand"/>.
        /// </summary>
        protected void RaiseBackgroundClicked()
        {
            this.BackgroundClickedCommand?.Execute(null);
            this.BackgroundClicked?.Invoke(this, EventArgs.Empty);
        }

        private static void OnHasSystemPaddingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PopupPage popupPage && newValue is bool hasSystemPadding)
            {
                popupPage.On<iOS>().SetUseSafeArea(hasSystemPadding);
            }
        }

        private static void OnHasKeyboardOffsetChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PopupPage popupPage && newValue is bool hasKeyboardOffset)
            {
                PageExtensions.SetHasKeyboardOffset(popupPage, hasKeyboardOffset);
            }
        }
    }
}

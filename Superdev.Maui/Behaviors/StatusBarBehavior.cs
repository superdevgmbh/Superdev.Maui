using System.Runtime.CompilerServices;
using Superdev.Maui.Services;

namespace Superdev.Maui.Behaviors
{
    /// <summary>
    /// StatusBarBehavior can be used to set the color and style of the status bar at the top of the page.
    /// </summary>
    /// <remarks>
    /// <see cref="StatusBarColor"/> and <see cref="StatusBarStyle"/> can only be used with static values
    /// or StaticResource bindings. This is due to a limitation in .NET MAUI (https://github.com/dotnet/maui/issues/23578).
    /// Use <c>StatusBar.Color</c> and <c>StatusBar.Style</c> if dynamic resource bindings (DynamicResource) is required.
    /// </remarks>
    public class StatusBarBehavior : BehaviorBase<Page>
    {
        private readonly Queue<string?> propertyChanges = new Queue<string?>();

        /// <summary>
        /// <see cref="BindableProperty"/> that manages the StatusBarColor property.
        /// </summary>
        public static readonly BindableProperty StatusBarColorProperty = BindableProperty.Create(
            nameof(StatusBarColor),
            typeof(Color),
            typeof(StatusBarBehavior));

        /// <summary>
        /// Property that holds the value of the Status bar color.
        /// </summary>
        public Color StatusBarColor
        {
            get => (Color)this.GetValue(StatusBarColorProperty);
            set => this.SetValue(StatusBarColorProperty, value);
        }

        /// <summary>
        /// <see cref="BindableProperty"/> that manages the NavigationBarColor property.
        /// </summary>
        public static readonly BindableProperty NavigationBarColorProperty = BindableProperty.Create(
            nameof(NavigationBarColor),
            typeof(Color),
            typeof(StatusBarBehavior));

        /// <summary>
        /// Property that holds the value of the Status bar color.
        /// </summary>
        public Color NavigationBarColor
        {
            get => (Color)this.GetValue(NavigationBarColorProperty);
            set => this.SetValue(NavigationBarColorProperty, value);
        }

        /// <summary>
        /// <see cref="BindableProperty"/> that manages the StatusBarColor property.
        /// </summary>
        public static readonly BindableProperty StatusBarStyleProperty = BindableProperty.Create(
            nameof(StatusBarStyle),
            typeof(StatusBarStyle),
            typeof(StatusBarBehavior),
            StatusBarStyle.Default);

        /// <summary>
        /// Property that holds the value of the Status bar color.
        /// </summary>
        public StatusBarStyle StatusBarStyle
        {
            get => (StatusBarStyle)this.GetValue(StatusBarStyleProperty);
            set => this.SetValue(StatusBarStyleProperty, value);
        }

        private readonly IStatusBarService statusBarService;

        internal StatusBarBehavior(IStatusBarService statusBarService)
        {
            this.statusBarService = statusBarService;
        }

        public StatusBarBehavior()
            : this(IStatusBarService.Current)
        {
        }

        protected override void OnAttachedTo(Page page)
        {
            base.OnAttachedTo(page);

            // We replicate all property changes which occurred
            // while the AssociatedObject was not set yet.
            while (this.propertyChanges.TryDequeue(out var propertyName))
            {
                this.OnPropertyChanged(propertyName);
            }
        }

        protected override void OnDetachingFrom(Page page)
        {
            this.propertyChanges.Clear();
            base.OnDetachingFrom(page);
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (this.AssociatedObject == null)
            {
                this.propertyChanges.Enqueue(propertyName);
                return;
            }

            if (propertyName == StatusBarColorProperty.PropertyName)
            {
                this.statusBarService.SetStatusBarColor(this.StatusBarColor);
            }
            else if (propertyName == NavigationBarColorProperty.PropertyName)
            {
                this.statusBarService.SetNavigationBarColor(this.NavigationBarColor);
            }
            else if (propertyName == StatusBarStyleProperty.PropertyName)
            {
                this.statusBarService.SetStyle(this.StatusBarStyle);
            }
        }
    }
}
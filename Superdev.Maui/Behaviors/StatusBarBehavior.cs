using System.Runtime.CompilerServices;
using Superdev.Maui.Services;

namespace Superdev.Maui.Behaviors
{
    public class StatusBarBehavior : BehaviorBase<Page>
    {
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

            if (this.StatusBarColor is Color statusBarColor)
            {
                this.statusBarService.SetColor(statusBarColor);
            }

            this.statusBarService.SetStyle(this.StatusBarStyle);
        }

        protected override void OnDetachingFrom(Page page)
        {
            base.OnDetachingFrom(page);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (this.AssociatedObject is null)
            {
                return;
            }

            if (propertyName == StatusBarColorProperty.PropertyName)
            {
                if (this.StatusBarColor is Color statusBarColor)
                {
                    this.statusBarService.SetColor(statusBarColor);
                }
            }
            else if (propertyName == StatusBarStyleProperty.PropertyName)
            {
                this.statusBarService.SetStyle(this.StatusBarStyle);
            }
        }
    }
}
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using AContext = Android.Content.Context;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace Superdev.Maui.Platforms.Android.Handlers
{
    public class CustomViewCellHandler : Microsoft.Maui.Controls.Handlers.Compatibility.ViewCellRenderer
    {
        private AView cellCore;
        private bool isSelected;
        private Drawable originalBackground;

        protected override AView GetCellCore(Cell item, AView convertView, AViewGroup parent, AContext context)
        {
            this.cellCore = base.GetCellCore(item, convertView, parent, context);

            this.isSelected = false;
            this.originalBackground = this.cellCore.Background;

            return this.cellCore;
        }

        protected override void OnCellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnCellPropertyChanged(sender, e);

            if (e.PropertyName == "IsSelected")
            {
                this.isSelected = !this.isSelected;
                if (this.isSelected)
                {
                    var selectedBackgroundColor = ((CustomViewCell)sender).SelectedBackgroundColor;
                    if (selectedBackgroundColor != null)
                    {
                        this.cellCore.SetBackgroundColor(selectedBackgroundColor.ToPlatform());
                    }
                }
                else
                {
                    this.cellCore.SetBackground(this.originalBackground);
                }
            }
        }
    }
}

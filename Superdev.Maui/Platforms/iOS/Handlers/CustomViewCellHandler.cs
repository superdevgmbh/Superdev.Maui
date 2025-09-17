using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    public class CustomViewCellHandler : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);

            var selectedBackgroundColor = ((CustomViewCell)item).SelectedBackgroundColor;
            if (selectedBackgroundColor != null)
            {
                cell.SelectedBackgroundView = new UIView
                {
                    BackgroundColor = selectedBackgroundColor.ToPlatform()
                };
            }
            else
            {
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }

            return cell;
        }
    }
}

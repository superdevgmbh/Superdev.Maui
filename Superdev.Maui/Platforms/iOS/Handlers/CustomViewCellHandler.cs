using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Handlers;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    /// <summary>
    /// Paints the <see cref="CustomViewCell.SelectedBackgroundColor"/> of the selected row. The selection is read from
    /// the owning <see cref="ListView"/>, as on Android, instead of left to UIKit: a list that recycles its rows wraps
    /// each of them in a cell of MAUI's own, and UIKit selects that outer cell, which shows its own grey.
    /// </summary>
    public class CustomViewCellHandler : ViewCellRenderer
    {
        /// <summary>
        /// The handler each row belongs to now. A list that keeps its cells gives a row to another cell when it reuses
        /// it, and the handler of the previous cell must not paint that row any more.
        /// </summary>
        private static readonly ConditionalWeakTable<UITableViewCell, CustomViewCellHandler> Owners = new ConditionalWeakTable<UITableViewCell, CustomViewCellHandler>();

        private UITableViewCell? tableViewCell;
        private Cell? cell;

        /// <summary>
        /// The background MAUI gave the row, which it shows while it is not selected.
        /// </summary>
        private UIColor? background;

        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var tableViewCell = base.GetCell(item, reusableCell, tv);
            this.tableViewCell = tableViewCell;
            this.cell = item;
            Owners.AddOrUpdate(tableViewCell, this);

            // UIKit's selection would draw over the colour, or around it in the outer cell, so it is switched off.
            // MAUI copies this style to the outer cell of a recycling list.
            tableViewCell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (item.Parent is ListView listView)
            {
                ListViewSelectionListener.Register(listView, this, this.UpdateSelectedBackground);
            }

            this.UpdateSelectedBackground();
            return tableViewCell;
        }

        public override void SetBackgroundColor(UITableViewCell tableViewCell, Cell cell, UIColor color)
        {
            base.SetBackgroundColor(tableViewCell, cell, color);
            this.background = color;
        }

        public override void UpdateValue(string property)
        {
            base.UpdateValue(property);

            // The colour is often a dynamic resource, which may be resolved only after the row was created; a recycled
            // row gets another item through its binding context.
            if (property is nameof(CustomViewCell.SelectedBackgroundColor) or nameof(BindableObject.BindingContext))
            {
                this.UpdateSelectedBackground();
            }
        }

        private void UpdateSelectedBackground()
        {
            if (this.tableViewCell is not UITableViewCell tableViewCell || this.cell is not Cell cell ||
                !Owners.TryGetValue(tableViewCell, out var owner) || owner != this)
            {
                return;
            }

            // The content view is painted, not the cell: the list sets the cell's own background whenever it shows the
            // row, which would undo the colour of a selected row that scrolls back into view.
            var isSelected = cell.Parent is ListView { SelectedItem: { } selectedItem } && Equals(selectedItem, cell.BindingContext);
            tableViewCell.ContentView.BackgroundColor = isSelected && cell is CustomViewCell { SelectedBackgroundColor: { } selectedBackgroundColor }
                ? selectedBackgroundColor.ToPlatform()
                : this.background;
        }
    }
}

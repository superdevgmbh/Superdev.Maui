using System.ComponentModel;
using System.Runtime.CompilerServices;
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Handlers;
using AContext = Android.Content.Context;

namespace Superdev.Maui.Platforms.Handlers
{
    /// <summary>
    /// Paints the <see cref="CustomViewCell.SelectedBackgroundColor"/> of the selected row. The selection is read from
    /// the owning <see cref="ListView"/> every time, because list rows are recycled: a flag kept per row would carry the
    /// selection of one item over to the next.
    /// </summary>
    public class CustomViewCellHandler : Microsoft.Maui.Controls.Handlers.Compatibility.ViewCellRenderer
    {
        /// <summary>
        /// What each row view needs to be painted. A recycled row view arrives with the background of its previous
        /// item, so its own is kept from the first time it is seen. A list that keeps its cells gives a row view to
        /// another cell when it reuses it, and the handler of the previous cell must not paint that view any more.
        /// </summary>
        private static readonly ConditionalWeakTable<AView, RowView> RowViews = new ConditionalWeakTable<AView, RowView>();

        private AView? cellCore;
        private Cell? cell;

        protected override AView GetCellCore(Cell item, AView convertView, AViewGroup parent, AContext context)
        {
            var cellCore = base.GetCellCore(item, convertView, parent, context);
            RowViews.GetValue(cellCore, v => new RowView(v.Background)).Owner = this;

            this.cellCore = cellCore;
            this.cell = item;

            if (item.Parent is ListView listView)
            {
                ListViewSelectionListener.Register(listView, this, this.UpdateSelectedBackground);
            }

            this.UpdateSelectedBackground();
            return cellCore;
        }

        protected override void OnCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.OnCellPropertyChanged(sender, e);

            if (e.PropertyName is nameof(BindableObject.BindingContext) or nameof(CustomViewCell.SelectedBackgroundColor))
            {
                this.UpdateSelectedBackground();
            }
        }

        private void UpdateSelectedBackground()
        {
            if (this.cell is not Cell cell || this.cellCore is not AView cellCore ||
                !RowViews.TryGetValue(cellCore, out var rowView) || rowView.Owner != this)
            {
                return;
            }

            var isSelected = cell.Parent is ListView { SelectedItem: { } selectedItem } && Equals(selectedItem, cell.BindingContext);
            if (isSelected && cell is CustomViewCell { SelectedBackgroundColor: { } selectedBackgroundColor })
            {
                cellCore.SetBackgroundColor(selectedBackgroundColor.ToPlatform());
            }
            else
            {
                cellCore.Background = rowView.OriginalBackground;
            }
        }

        private sealed class RowView
        {
            public RowView(Drawable? originalBackground)
            {
                this.OriginalBackground = originalBackground;
            }

            public Drawable? OriginalBackground { get; }

            public CustomViewCellHandler? Owner { get; set; }
        }
    }
}

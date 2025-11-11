using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Controls;
using Superdev.Maui.Platforms.Extensions;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<SearchBar, SearchBarHandler>;

    public class SearchBarHandler : Microsoft.Maui.Handlers.SearchBarHandler
    {
        private MauiDoneAccessoryView inputAccessoryView;

        public new static readonly PM Mapper = new PM(Microsoft.Maui.Handlers.SearchBarHandler.Mapper)
        {
            [nameof(DialogExtensions.DoneButtonText)] = MapDoneButtonText,
            [nameof(SearchBar.CancelButtonColor)] = MapCancelButtonColor,
        };

        public SearchBarHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public SearchBarHandler()
            : base(Mapper)
        {
        }

        protected override MauiSearchBar CreatePlatformView()
        {
            var mauiSearchBar = base.CreatePlatformView();

            this.inputAccessoryView = new MauiDoneAccessoryView();
            this.inputAccessoryView.SetDoneButtonAction(this.OnDoneClicked);

            mauiSearchBar.InputAccessoryView = this.inputAccessoryView;

            return mauiSearchBar;
        }

        protected override void DisconnectHandler(MauiSearchBar platformView)
        {
            platformView.InputAccessoryView = null;
            this.inputAccessoryView?.Dispose();
            this.inputAccessoryView = null;

            base.DisconnectHandler(platformView);
        }

        private new SearchBar VirtualView => (SearchBar)base.VirtualView;

        private static void MapDoneButtonText(SearchBarHandler searchBarHandler, SearchBar searchBar)
        {
            searchBarHandler.DoneButtonText(searchBar);
        }

        private void DoneButtonText(SearchBar searchBar)
        {
            var doneButtonText = DialogExtensions.GetDoneButtonText(searchBar);
            var mauiSearchBar = this.PlatformView;
            mauiSearchBar.InputAccessoryView = MauiDoneAccessoryView.SetDoneButtonText(ref this.inputAccessoryView, doneButtonText);
        }

        private new static void MapCancelButtonColor(ISearchBarHandler searchBarHandler, ISearchBar searchBar)
        {
            var mauiSearchBar = searchBarHandler.PlatformView;

            if (searchBar.CancelButtonColor is Color cancelButtonColor && !Equals(cancelButtonColor, Colors.Transparent))
            {
                mauiSearchBar?.UpdateCancelButton(searchBar);
            }
            else
            {
                mauiSearchBar.SetShowsCancelButton(showsCancelButton: false, animated: false);
            }
        }

        private void OnDoneClicked()
        {
            var mauiSearchBar = this.PlatformView;
            mauiSearchBar.ResignFirstResponder();
        }
    }
}
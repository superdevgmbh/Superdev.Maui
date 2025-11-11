using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Mvvm;
using Superdev.Maui.Services;
using SuperdevMauiDemoApp.Model;
using SuperdevMauiDemoApp.Services;
using Superdev.Maui.Validation;

namespace SuperdevMauiDemoApp.ViewModels
{
    public class TimePickerDemoViewModel : BaseViewModel
    {
        private readonly IViewModelErrorHandler viewModelErrorHandler;
        private readonly IDateTime dateTime;

        private bool isReadonly;
        private DateTime? birthdate;
        private DateTime patentStartDate;
        private TimeSpan patentStartTime;
        private TimeSpan? patentEndTime;
        private DateRange patentValidityRange;
        private IRelayCommand toggleIsReadonlyCommand;

        public TimePickerDemoViewModel(
            IViewModelErrorHandler viewModelErrorHandler,
            IDateTime dateTime)
        {
            this.viewModelErrorHandler = viewModelErrorHandler;
            this.dateTime = dateTime;

            var referenceDate = dateTime.Now;
            this.BirthdateValidityRange = new DateRange(start: referenceDate.AddDays(-2), end: referenceDate.AddDays(2));
            this.Birthdate = referenceDate;

            _ = this.LoadData();
        }

        protected override ViewModelValidation SetupValidation()
        {
            var viewModelValidation = new ViewModelValidation();

            viewModelValidation.AddValidationFor(nameof(this.Birthdate))
                .When(() => this.Birthdate == null)
                .Show(() => "Birthdate must be set");

            return viewModelValidation;
        }

        private async Task LoadData()
        {
            this.IsBusy = true;
            this.ViewModelError = ViewModelError.None;

            try
            {
                var today = this.dateTime.Now.Date;
                var todayInOneMonth = today.AddMonths(1);
                this.PatentValidityRange = new DateRange(today, todayInOneMonth);

                this.PatentStartDate = today.AddDays(1);
                this.PatentStartTime = new TimeSpan(14, 15, 16);
            }
            catch (Exception ex)
            {
                this.ViewModelError = this.viewModelErrorHandler.FromException(ex).WithRetry(this.LoadData);
            }

            this.IsBusy = false;
        }

        public IRelayCommand ToggleIsReadonlyCommand
        {
            get => this.toggleIsReadonlyCommand ??= new RelayCommand(this.ToggleIsReadonly);
        }

        private void ToggleIsReadonly()
        {
            this.IsReadonly = !this.IsReadonly;
        }

        public bool IsReadonly
        {
            get => this.isReadonly;
            set => this.SetProperty(ref this.isReadonly, value);
        }

        public DateTime? Birthdate
        {
            get => this.birthdate;
            set
            {
                if (this.SetProperty(ref this.birthdate, value))
                {
                    _ = this.Validation.IsValidAsync();
                }
            }
        }

        public DateRange BirthdateValidityRange { get; }

        public DateTime PatentStartDate
        {
            get => this.patentStartDate;
            set => this.SetProperty(ref this.patentStartDate, value);
        }

        public TimeSpan PatentStartTime
        {
            get => this.patentStartTime;
            set => this.SetProperty(ref this.patentStartTime, value);
        }

        public TimeSpan? PatentEndTime
        {
            get => this.patentEndTime;
            set => this.SetProperty(ref this.patentEndTime, value);
        }

        public DateRange PatentValidityRange
        {
            get => this.patentValidityRange;
            private set => this.SetProperty(ref this.patentValidityRange, value);
        }
    }
}
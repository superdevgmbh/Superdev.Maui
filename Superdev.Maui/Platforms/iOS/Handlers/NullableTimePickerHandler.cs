using System.Diagnostics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Superdev.Maui.Utils;
using Superdev.Maui.Controls;
using Superdev.Maui.Extensions;
using Superdev.Maui.Platforms.iOS.Utils;
using UIKit;

namespace Superdev.Maui.Platforms.Handlers
{
    using PM = PropertyMapper<NullableTimePicker, NullableTimePickerHandler>;

    public class NullableTimePickerHandler : TimePickerHandler
    {
        public new static readonly PM Mapper = new PM(TimePickerHandler.Mapper)
        {
            [nameof(TimePicker.Format)] = UpdateFormat,
            [nameof(TimePicker.Time)] = UpdateTime,
            [nameof(NullableTimePicker.NullableTime)] = UpdateNullableTime,
            [nameof(NullableTimePicker.Placeholder)] = UpdatePlaceholder,
            [nameof(NullableTimePicker.PlaceholderColor)] = UpdatePlaceholder,
            [nameof(DialogExtensions.NeutralButtonText)] = UpdateNeutralButtonText
        };

        public NullableTimePickerHandler(IPropertyMapper mapper = null, CommandMapper commandMapper = null)
            : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
        {
        }

        public NullableTimePickerHandler()
            : base(Mapper)
        {
        }

        private new NullableTimePicker VirtualView => (NullableTimePicker)base.VirtualView;

        protected override void SetupUIToolbar(MauiTimePicker mauiTimePicker)
        {
            lock (mauiTimePicker.InputAccessoryView)
            {
                var buttonItems = new List<UIBarButtonItem>();
                var clearButton = CreateClearButton(this.VirtualView, mauiTimePicker);
                if (clearButton != null)
                {
                    buttonItems.Add(clearButton);
                }

                buttonItems.Add(new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace));
                buttonItems.Add(UIToolbarHelper.CreateDoneButton(this.VirtualView, this.HandleDoneButton));

                mauiTimePicker.InputAccessoryView = UIToolbarHelper.CreateUIToolbar(buttonItems.ToArray());
                mauiTimePicker.InputAccessoryView.SetNeedsDisplay();
            }
        }

        private static UIBarButtonItem CreateClearButton(NullableTimePicker nullableTimePicker, MauiTimePicker mauiTimePicker)
        {
            var clearButtonText = DialogExtensions.GetNeutralButtonText(nullableTimePicker);
            if (!string.IsNullOrEmpty(clearButtonText))
            {
                var clearButton = new UIBarButtonItem(clearButtonText, UIBarButtonItemStyle.Plain, (_, _) =>
                {
                    HandleClearButton(nullableTimePicker, mauiTimePicker);
                });
                return clearButton;
            }

            return null;
        }

        private void HandleDoneButton(object sender, EventArgs e)
        {
            var time = this.PlatformView.Date.ToDateTime().TimeOfDay;
            this.VirtualView.Time = time;
            this.VirtualView.NullableTime = time;
            SetNullableText(this.PlatformView, this.VirtualView);
            this.PlatformView.ResignFirstResponder();
        }

        private static void UpdatePlaceholder(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            var placeholderText = nullableTimePicker.Placeholder;
            var mauiTimePicker = nullableTimePickerHandler.PlatformView;

            if (nullableTimePicker.PlaceholderColor is Color placeholderColor)
            {
                var foregroundColor = placeholderColor.ToPlatform();
                var attributedPlaceholder = new NSAttributedString(placeholderText, foregroundColor: foregroundColor);
                mauiTimePicker.AttributedPlaceholder = attributedPlaceholder;
            }
            else
            {
                mauiTimePicker.Placeholder = placeholderText;
            }
        }

        protected override void UpdateDoneButton(MauiTimePicker mauiTimePicker)
        {
            this.SetupUIToolbar(this.PlatformView);
        }

        private static void UpdateNeutralButtonText(NullableTimePickerHandler nullableTimePickerHandler, NullableTimePicker nullableTimePicker)
        {
            nullableTimePickerHandler.SetupUIToolbar(nullableTimePickerHandler.PlatformView);
        }

        private static void HandleClearButton(NullableTimePicker nullableTimePicker, MauiTimePicker mauiTimePicker)
        {
            nullableTimePicker.Time = TimeSpan.Zero;
            nullableTimePicker.NullableTime = null;
            SetNullableText(mauiTimePicker, nullableTimePicker);
            mauiTimePicker.ResignFirstResponder();
        }

        private static void UpdateFormat(ITimePickerHandler datePickerHandler, ITimePicker datePicker)
        {
            Debug.WriteLine("UpdateFormat");

            if (datePicker is NullableTimePicker nullableTimePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableTimePicker);
            }
        }

        private static void UpdateTime(ITimePickerHandler datePickerHandler, ITimePicker datePicker)
        {
            Debug.WriteLine("UpdateDate");

            if (datePicker is NullableTimePicker nullableTimePicker &&
                datePickerHandler is NullableTimePickerHandler)
            {
                UpdateNullableTime(datePickerHandler, nullableTimePicker);
            }
        }

        private static void UpdateNullableTime(ITimePickerHandler datePickerHandler, ITimePicker datePicker)
        {
            Debug.WriteLine("UpdateNullableDate");

            if (datePicker is NullableTimePicker nullableTimePicker)
            {
                SetNullableText(datePickerHandler.PlatformView, nullableTimePicker);
            }
        }

        private static void SetNullableText(MauiTimePicker mauiTimePicker, NullableTimePicker nullableTimePicker)
        {
            var originalText = mauiTimePicker.Text;

            try
            {
                mauiTimePicker.Text = nullableTimePicker.NullableTime.ToStringExtended(nullableTimePicker.Format);
            }
            catch (Exception ex)
            {
                mauiTimePicker.Text = ex.Message;
            }

            Debug.WriteLine($"SetNullableText: mauiTimePicker.Text=\"{originalText}\" --> mauiTimePicker.Text=\"{mauiTimePicker.Text}\"");
        }
    }
}
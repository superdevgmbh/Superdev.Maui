using Superdev.Maui.Services;
using Superdev.Maui.Tests.Navigation.TestDoubles;
using IDeviceInfo = Superdev.Maui.Services.IDeviceInfo;

namespace Superdev.Maui.Tests.Services
{
    public class DialogServiceTests
    {
        private readonly TestMainThread mainThread = new TestMainThread();
        private readonly Mock<IDeviceInfo> deviceInfoMock = new Mock<IDeviceInfo>();
        private readonly TestPageDialogs pageDialogs = new TestPageDialogs();
        private readonly ContentPage currentPage;

        public DialogServiceTests()
        {
            TestDispatcherProvider.EnsureInstalled();
            this.currentPage = new ContentPage();
        }

        [Fact]
        public async Task DisplayAlertAsync_WithCancelOnly_ShowsTheAlertOnTheCurrentPage()
        {
            // Arrange
            var dialogService = this.CreateDialogService();

            // Act
            await dialogService.DisplayAlertAsync("Title", "Message", "OK");

            // Assert
            this.pageDialogs.Page.Should().BeSameAs(this.currentPage);
            this.pageDialogs.Dialog.Should().Be("Alert");
            this.pageDialogs.Arguments.Should().Equal("Title", "Message", "OK");
            this.mainThread.InvokeCount.Should().Be(1, "dialogs are shown on the main thread");
        }

        [Fact]
        public async Task DisplayAlertAsync_WithConfirmAndCancel_ReturnsTheAnswer()
        {
            // Arrange
            this.pageDialogs.AlertResult = true;
            var dialogService = this.CreateDialogService();

            // Act
            var confirmed = await dialogService.DisplayAlertAsync("Title", "Message", "Yes", "No");

            // Assert
            confirmed.Should().BeTrue();
            this.pageDialogs.Arguments.Should().Equal("Title", "Message", "Yes", "No");
        }

        [Fact]
        public async Task DisplayDestructiveAlertAsync_OnIOS_ShowsAnActionSheetWithTitleAndMessage()
        {
            // Arrange
            this.deviceInfoMock.SetupGet(d => d.Platform).Returns(DevicePlatform.iOS);
            this.pageDialogs.ActionSheetResult = "Delete";
            var dialogService = this.CreateDialogService();

            // Act
            var confirmed = await dialogService.DisplayDestructiveAlertAsync("Title", "Message", "Delete", "Cancel");

            // Assert
            confirmed.Should().BeTrue();
            this.pageDialogs.Dialog.Should().Be("ActionSheet");
            this.pageDialogs.Arguments.Should().Equal($"Title{Environment.NewLine}Message", "Cancel", "Delete");
        }

        [Fact]
        public async Task DisplayDestructiveAlertAsync_OnIOSWithoutMessage_ShowsOnlyTheTitle()
        {
            // Arrange
            this.deviceInfoMock.SetupGet(d => d.Platform).Returns(DevicePlatform.iOS);
            var dialogService = this.CreateDialogService();

            // Act
            await dialogService.DisplayDestructiveAlertAsync("Title", "", "Delete", "Cancel");

            // Assert
            this.pageDialogs.Arguments.Should().Equal("Title", "Cancel", "Delete");
        }

        [Fact]
        public async Task DisplayDestructiveAlertAsync_OnIOSCancelled_ReturnsFalse()
        {
            // Arrange
            this.deviceInfoMock.SetupGet(d => d.Platform).Returns(DevicePlatform.iOS);
            this.pageDialogs.ActionSheetResult = "Cancel";
            var dialogService = this.CreateDialogService();

            // Act
            var confirmed = await dialogService.DisplayDestructiveAlertAsync("Title", "Message", "Delete", "Cancel");

            // Assert
            confirmed.Should().BeFalse();
        }

        [Fact]
        public async Task DisplayDestructiveAlertAsync_OnAndroid_ShowsAnAlert()
        {
            // Arrange
            this.deviceInfoMock.SetupGet(d => d.Platform).Returns(DevicePlatform.Android);
            this.pageDialogs.AlertResult = true;
            var dialogService = this.CreateDialogService();

            // Act
            var confirmed = await dialogService.DisplayDestructiveAlertAsync("Title", "Message", "Delete", "Cancel");

            // Assert
            confirmed.Should().BeTrue();
            this.pageDialogs.Dialog.Should().Be("Alert");
            this.pageDialogs.Arguments.Should().Equal("Title", "Message", "Delete", "Cancel");
        }

        [Fact]
        public async Task DisplayActionSheetAsync_WithTexts_ReturnsTheChosenText()
        {
            // Arrange
            this.pageDialogs.ActionSheetResult = "B";
            var dialogService = this.CreateDialogService();

            // Act
            var chosen = await dialogService.DisplayActionSheetAsync("Title", "Cancel", "Delete", "A", "B");

            // Assert
            chosen.Should().Be("B");
            this.pageDialogs.Arguments.Should().Equal("Title", "Cancel", "Delete", "A", "B");
        }

        [Fact]
        public async Task DisplayActionSheetAsync_ButtonChosen_PressesTheButton()
        {
            // Arrange
            var pressed = false;
            var button = ActionSheetButton.Create("A", () =>
            {
                pressed = true;
                return Task.CompletedTask;
            });
            this.pageDialogs.ActionSheetResult = "A";
            var dialogService = this.CreateDialogService();

            // Act
            var chosen = await dialogService.DisplayActionSheetAsync("Title", button, ActionSheetButton.CreateCancel("Cancel"));

            // Assert
            chosen.Should().BeSameAs(button);
            pressed.Should().BeTrue();
            this.pageDialogs.Arguments.Should().Equal("Title", "Cancel", null, "A");
        }

        [Fact]
        public async Task DisplayActionSheetAsync_Dismissed_ReturnsNull()
        {
            // Arrange
            var pressed = false;
            var button = ActionSheetButton.Create("A", () =>
            {
                pressed = true;
                return Task.CompletedTask;
            });
            this.pageDialogs.ActionSheetResult = null;
            var dialogService = this.CreateDialogService();

            // Act
            var chosen = await dialogService.DisplayActionSheetAsync("Title", button);

            // Assert
            chosen.Should().BeNull();
            pressed.Should().BeFalse();
        }

        [Fact]
        public async Task DisplayActionSheetAsync_TwoCancelButtons_ThrowsArgumentException()
        {
            // Arrange
            var dialogService = this.CreateDialogService();

            // Act
            var action = () => dialogService.DisplayActionSheetAsync(
                "Title", ActionSheetButton.CreateCancel("Cancel"), ActionSheetButton.CreateCancel("Close"));

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
            this.pageDialogs.Dialog.Should().BeNull();
        }

        [Fact]
        public async Task DisplayActionSheetAsync_NullButtons_ThrowsArgumentNullException()
        {
            // Arrange
            var dialogService = this.CreateDialogService();

            // Act
            var action = () => dialogService.DisplayActionSheetAsync("Title", (IActionSheetButton[])null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task DisplayPromptAsync_WithoutKeyboard_UsesTheDefaultKeyboard()
        {
            // Arrange
            this.pageDialogs.PromptResult = "Answer";
            var dialogService = this.CreateDialogService();

            // Act
            var answer = await dialogService.DisplayPromptAsync("Title", "Message");

            // Assert
            answer.Should().Be("Answer");
            this.pageDialogs.Arguments.Should().Equal("Title", "Message", "OK", "Cancel", null, -1, Keyboard.Default, "");
        }

        private DialogService CreateDialogService()
        {
            return new DialogService(this.mainThread, this.deviceInfoMock.Object, () => this.currentPage, this.pageDialogs);
        }

        /// <summary>
        /// Records the last dialog instead of showing it, and answers it with the configured result.
        /// </summary>
        private sealed class TestPageDialogs : PageDialogs
        {
            public Page? Page { get; private set; }

            public string? Dialog { get; private set; }

            public object?[] Arguments { get; private set; } = [];

            public bool AlertResult { get; set; }

            public string? ActionSheetResult { get; set; }

            public string? PromptResult { get; set; }

            public override Task DisplayAlertAsync(Page page, string title, string message, string cancel)
            {
                this.Record(page, "Alert", title, message, cancel);
                return Task.CompletedTask;
            }

            public override Task<bool> DisplayAlertAsync(Page page, string title, string message, string accept, string cancel)
            {
                this.Record(page, "Alert", title, message, accept, cancel);
                return Task.FromResult(this.AlertResult);
            }

            public override Task<string> DisplayActionSheetAsync(Page page, string title, string? cancel, string? destruction, string[] buttons)
            {
                this.Record(page, "ActionSheet", [title, cancel, destruction, .. buttons]);

                // The platform answers a dismissed sheet with null.
                return Task.FromResult(this.ActionSheetResult!);
            }

            public override Task<string?> DisplayPromptAsync(
                Page page, string title, string message, string accept, string cancel, string? placeholder, int maxLength, Keyboard keyboard, string initialValue)
            {
                this.Record(page, "Prompt", title, message, accept, cancel, placeholder, maxLength, keyboard, initialValue);
                return Task.FromResult(this.PromptResult);
            }

            private void Record(Page page, string dialog, params object?[] arguments)
            {
                this.Page = page;
                this.Dialog = dialog;
                this.Arguments = arguments;
            }
        }
    }
}

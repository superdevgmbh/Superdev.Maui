using System.ComponentModel;
using System.Runtime.CompilerServices;
using Superdev.Maui.Effects;
using Superdev.Maui.Tests.Navigation.TestDoubles;
using Superdev.Maui.Tests.Validation;
using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Controls
{
    public class ValidationTests
    {
        [Fact]
        public void ShouldResolveTheForm_FromABaseViewModel_AValidation_OrAnExplicitSource()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var standalone = new ViewModelValidation();
            var explicitSource = new ViewModelValidation();

            // Act & Assert
            Superdev.Maui.Controls.Validation.ResolveForm(viewModel, null).Should().BeSameAs(viewModel.Validation);
            Superdev.Maui.Controls.Validation.ResolveForm(standalone, null).Should().BeSameAs(standalone);
            Superdev.Maui.Controls.Validation.ResolveForm(viewModel, explicitSource).Should().BeSameAs(explicitSource, "an explicit source wins");
            Superdev.Maui.Controls.Validation.ResolveForm(new object(), null).Should().BeNull();
            Superdev.Maui.Controls.Validation.ResolveForm(null, null).Should().BeNull();
        }

        [Fact]
        public async Task ShouldWireAPlainControl_ToTheFieldOfItsBindingContext()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var entry = new Entry { BindingContext = viewModel };

            // Act
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();

            // Assert
            Superdev.Maui.Controls.Validation.GetField(entry).Should().BeSameAs(viewModel.Validation.Fields["UserName"]);
            SemanticProperties.GetHint(entry).Should().Be("User Name must not be empty.", "a screen reader announces the error with the field");

            viewModel.UserName = "superdev";
            SemanticProperties.GetHint(entry).Should().BeNull();
        }

        [Fact]
        public void ShouldRewire_WhenTheBindingContextChanges()
        {
            // Arrange
            var first = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var second = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var entry = new Entry { BindingContext = first };
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");

            // Act
            entry.BindingContext = second;

            // Assert
            Superdev.Maui.Controls.Validation.GetField(entry).Should().BeSameAs(second.Validation.Fields["UserName"]);
        }

        [Fact]
        public void ShouldUseAnExplicitSource_OverTheBindingContext()
        {
            // Arrange
            var viewModel = new TestFormViewModel();
            var source = new ViewModelValidation();
            source.Property(nameof(viewModel.UserName), () => viewModel.UserName).NotEmpty();
            var entry = new Entry { BindingContext = viewModel };

            // Act
            Superdev.Maui.Controls.Validation.SetSource(entry, source);
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");

            // Assert
            Superdev.Maui.Controls.Validation.GetField(entry).Should().BeSameAs(source.Fields["UserName"]);
        }

        [Fact]
        public void ShouldStayUnwired_WithoutAPropertyName_OrWithoutAForm()
        {
            // Arrange
            var entry = new Entry { BindingContext = new object() };

            // Act
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");

            // Assert
            Superdev.Maui.Controls.Validation.GetField(entry).Should().BeNull();
            SemanticProperties.GetHint(entry).Should().BeNull();
        }

        [Fact]
        public async Task ShouldKeepTheControlsOwnHint_AndAnnounceTheErrorAfterIt()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var entry = new Entry { BindingContext = viewModel };
            SemanticProperties.SetHint(entry, "Your login");

            // Act
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            SemanticProperties.GetHint(entry).Should().Be("Your login", "there is no error yet");
            await viewModel.Validation.ValidateAsync();

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Your login. User Name must not be empty.");
            viewModel.UserName = "superdev";
            SemanticProperties.GetHint(entry).Should().Be("Your login", "the own hint comes back with the error gone");
        }

        [Fact]
        public async Task ShouldRestoreTheOwnHint_WhenTheControlIsUnwired()
        {
            // Arrange
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
            var entry = new Entry { BindingContext = viewModel };
            SemanticProperties.SetHint(entry, "Your login");
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();

            // Act
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, null);

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Your login");
        }

        [Fact]
        public void ShouldLeaveABoundHint_InCharge_WhileTheFieldIsValid()
        {
            // Arrange - the hint is bound, as {s:Translate} binds it
            TestDispatcherProvider.EnsureInstalled();
            var viewModel = CreateUserNameForm();
            var texts = new HintTexts { Hint = "Your login" };
            var entry = new Entry { BindingContext = viewModel };
            entry.SetBinding(SemanticProperties.HintProperty, new Binding(nameof(HintTexts.Hint), source: texts));
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");

            // Act - the language changes
            texts.Hint = "Ihr Login";

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Ihr Login");
        }

        [Fact]
        public async Task ShouldLetABoundHintShowThrough_OnceTheErrorIsGone()
        {
            // Arrange - the language changes while the error is announced
            TestDispatcherProvider.EnsureInstalled();
            var viewModel = CreateUserNameForm();
            var texts = new HintTexts { Hint = "Your login" };
            var entry = new Entry { BindingContext = viewModel };
            entry.SetBinding(SemanticProperties.HintProperty, new Binding(nameof(HintTexts.Hint), source: texts));
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            SemanticProperties.GetHint(entry).Should().Be("Your login. User Name must not be empty.");
            texts.Hint = "Ihr Login";

            // Act
            viewModel.UserName = "superdev";

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Ihr Login", "the binding is in charge again, with its latest value");
        }

        [Fact]
        public async Task ShouldLeaveABoundHint_InCharge_AfterItTurnedNull()
        {
            // Arrange - the hint's binding says null while the error is announced
            TestDispatcherProvider.EnsureInstalled();
            var viewModel = CreateUserNameForm();
            var texts = new HintTexts { Hint = "Your login" };
            var entry = new Entry { BindingContext = viewModel };
            entry.SetBinding(SemanticProperties.HintProperty, new Binding(nameof(HintTexts.Hint), source: texts));
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            texts.Hint = null;
            viewModel.UserName = "superdev";

            // Act
            texts.Hint = "Ihr Login";

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Ihr Login");
        }

        [Fact]
        public async Task ShouldAnnounceTheError_AfterTheLatestBoundHint()
        {
            // Arrange - the language changes while an error is announced
            TestDispatcherProvider.EnsureInstalled();
            var viewModel = CreateUserNameFormWithMinimumLength();
            var texts = new HintTexts { Hint = "Your login" };
            var entry = new Entry { BindingContext = viewModel };
            entry.SetBinding(SemanticProperties.HintProperty, new Binding(nameof(HintTexts.Hint), source: texts));
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            texts.Hint = "Ihr Login";

            // Act - the next message is announced
            viewModel.UserName = "ab";

            // Assert
            SemanticProperties.GetHint(entry).Should().StartWith("Ihr Login. ");
        }

        [Fact]
        public async Task ShouldKeepADynamicResourceHint_FollowingItsResource()
        {
            // Arrange
            var viewModel = CreateUserNameForm();
            var entry = new Entry { BindingContext = viewModel };
            entry.Resources["LoginHint"] = "Your login";
            entry.SetDynamicResource(SemanticProperties.HintProperty, "LoginHint");
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            viewModel.UserName = "superdev";

            // Act
            entry.Resources["LoginHint"] = "Ihr Login";

            // Assert
            SemanticProperties.GetHint(entry).Should().Be("Ihr Login");
        }

        [Fact]
        public async Task ShouldLeaveNoErrorLine_WhenTheFieldIsFixedWhileAVisualStateCoversIt()
        {
            // Arrange - the entry has an underline colour of its own and a focused one from a visual state
            var viewModel = CreateUserNameForm();
            var entry = new Entry { BindingContext = viewModel };
            LineColorEffect.SetLineColor(entry, Colors.Gray);
            AddFocusedLineColor(entry, Colors.Blue);
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            VisualStateManager.GoToState(entry, "Focused");

            // Act - the user fixes the field while it has the focus, then leaves it
            viewModel.UserName = "superdev";
            VisualStateManager.GoToState(entry, "Normal");

            // Assert
            LineColorEffect.GetLineColor(entry).Should().Be(Colors.Gray);
        }

        [Fact]
        public async Task ShouldKeepTheOwnLineColor_WhenTheMessageChangesUnderAVisualState()
        {
            // Arrange - the message changes while the focused colour covers the error line
            var viewModel = CreateUserNameFormWithMinimumLength();
            var entry = new Entry { BindingContext = viewModel };
            LineColorEffect.SetLineColor(entry, Colors.Gray);
            AddFocusedLineColor(entry, Colors.Blue);
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            await viewModel.Validation.ValidateAsync();
            VisualStateManager.GoToState(entry, "Focused");
            viewModel.UserName = "ab";
            VisualStateManager.GoToState(entry, "Normal");

            // Act
            viewModel.UserName = "superdev";

            // Assert
            LineColorEffect.GetLineColor(entry).Should().Be(Colors.Gray, "the focused colour belongs to the visual state, not to the entry");
        }

        [Fact]
        public async Task ShouldColourTheUnderline_OnlyWhileTheFieldHasAnError()
        {
            // Arrange - the entry has an underline colour of its own
            var viewModel = CreateUserNameForm();
            var entry = new Entry { BindingContext = viewModel };
            LineColorEffect.SetLineColor(entry, Colors.Blue);
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            LineColorEffect.GetLineColor(entry).Should().Be(Colors.Blue, "there is no error yet");
            await viewModel.Validation.ValidateAsync();
            LineColorEffect.GetLineColor(entry).Should().Be(Colors.Red);

            // Act
            viewModel.UserName = "superdev";

            // Assert
            LineColorEffect.GetLineColor(entry).Should().Be(Colors.Blue);
        }

        [Theory]
        [InlineData(null, "Required.", "Required.")]
        [InlineData("Your login", null, "Your login")]
        [InlineData("Your login", "Required.", "Your login. Required.")]
        [InlineData("Your login.", "Required.", "Your login. Required.")]
        [InlineData("Who are you?", "Required.", "Who are you? Required.")]
        public void ShouldComposeTheHint_WithoutDoublingThePunctuation(string? ownHint, string? message, string? expected)
        {
            // Act
            var hint = Superdev.Maui.Controls.Validation.ValidationAttachment.ComposeHint(ownHint, message);

            // Assert
            hint.Should().Be(expected);
        }

        [Fact]
        public void ShouldNotKeepAWiredControlAlive_ThroughTheViewModel()
        {
            // Arrange - the view model outlives the page, as a singleton or a parent behind a popup does
            var viewModel = new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });

            // Act
            var control = WireEntry(viewModel);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Assert
            control.IsAlive.Should().BeFalse("the form reaches the control through weak subscriptions only");
            GC.KeepAlive(viewModel);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference WireEntry(TestFormViewModel viewModel)
        {
            var entry = new Entry { BindingContext = viewModel };
            Superdev.Maui.Controls.Validation.SetPropertyName(entry, "UserName");
            return new WeakReference(entry);
        }

        private static TestFormViewModel CreateUserNameForm()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty();
                return v;
            });
        }

        private static TestFormViewModel CreateUserNameFormWithMinimumLength()
        {
            return new TestFormViewModel(vm =>
            {
                var v = new ViewModelValidation();
                v.Property(() => vm.UserName).NotEmpty().MinimumLength(3);
                return v;
            });
        }

        /// <summary>A "Focused" visual state on the entry itself, which ranks above a value set in code.</summary>
        private static void AddFocusedLineColor(Entry entry, Color color)
        {
            var commonStates = new VisualStateGroup { Name = "CommonStates" };
            commonStates.States.Add(new VisualState { Name = "Normal" });
            var focused = new VisualState { Name = "Focused" };
            focused.Setters.Add(new Setter { Property = LineColorEffect.LineColorProperty, Value = color });
            commonStates.States.Add(focused);
            VisualStateManager.SetVisualStateGroups(entry, new VisualStateGroupList { commonStates });
        }

        /// <summary>A binding source for a control's own hint, as a translation source is.</summary>
        private sealed class HintTexts : INotifyPropertyChanged
        {
            private string? hint;

            public event PropertyChangedEventHandler? PropertyChanged;

            public string? Hint
            {
                get => this.hint;
                set
                {
                    this.hint = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Hint)));
                }
            }
        }
    }
}

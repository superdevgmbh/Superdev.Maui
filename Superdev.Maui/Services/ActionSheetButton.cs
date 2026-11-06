namespace Superdev.Maui.Services
{
    /// <inheritdoc cref="IActionSheetButton"/>
    public sealed class ActionSheetButton : IActionSheetButton
    {
        private readonly Func<Task>? action;

        private ActionSheetButton(string text, bool isCancel, bool isDestroy, Func<Task>? action)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            this.Text = text;
            this.IsCancel = isCancel;
            this.IsDestroy = isDestroy;
            this.action = action;
        }

        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />
        public bool IsCancel { get; }

        /// <inheritdoc />
        public bool IsDestroy { get; }

        /// <summary>
        /// Creates an ordinary button.
        /// </summary>
        public static ActionSheetButton Create(string text, Func<Task>? action = null)
        {
            return new ActionSheetButton(text, isCancel: false, isDestroy: false, action);
        }

        /// <summary>
        /// Creates the button which dismisses the action sheet.
        /// </summary>
        public static ActionSheetButton CreateCancel(string text, Func<Task>? action = null)
        {
            return new ActionSheetButton(text, isCancel: true, isDestroy: false, action);
        }

        /// <summary>
        /// Creates the button of a destructive action, which the platform highlights.
        /// </summary>
        public static ActionSheetButton CreateDestroy(string text, Func<Task>? action = null)
        {
            return new ActionSheetButton(text, isCancel: false, isDestroy: true, action);
        }

        /// <inheritdoc />
        public Task PressAsync()
        {
            return this.action?.Invoke() ?? Task.CompletedTask;
        }
    }
}

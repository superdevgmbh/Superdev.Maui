namespace Superdev.Maui.Validation
{
    /// <summary>
    /// Raised through <see cref="ViewModelValidation.FocusRequested"/> when the first invalid field should receive
    /// focus. The control wired to <see cref="PropertyName"/> answers it.
    /// </summary>
    public sealed class FocusRequestedEventArgs : EventArgs
    {
        public FocusRequestedEventArgs(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
}

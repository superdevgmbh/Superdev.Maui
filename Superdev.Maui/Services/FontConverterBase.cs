namespace Superdev.Maui.Services
{
    /// <summary>
    /// <seealso cref="IFontConverter"/> which does not change the scale of font sizes.
    /// </summary>
    public abstract class FontConverterBase : IFontConverter
    {
        public event EventHandler FontScalingChanged;

        protected void RaiseFontScalingChangedEvent()
        {
            this.FontScalingChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual double GetScaledFontSize(double fontSize)
        {
            return fontSize;
        }

        public virtual void Dispose()
        {
        }
    }
}
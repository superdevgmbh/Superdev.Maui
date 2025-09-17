namespace Superdev.Maui.Utils
{
    public class ResettableLazy<T>
    {
        private readonly Func<T> valueFactory;
        private readonly LazyThreadSafetyMode lazyThreadSafetyMode;

        private Lazy<T> lazy;

        public ResettableLazy(Func<T> valueFactory, LazyThreadSafetyMode mode = LazyThreadSafetyMode.None)
        {
            this.valueFactory = valueFactory;
            this.lazyThreadSafetyMode = mode;

            this.Reset();
        }

        public T Value => this.lazy.Value;

        public void Reset()
        {
            this.lazy = new Lazy<T>(this.valueFactory, this.lazyThreadSafetyMode);
        }
    }
}
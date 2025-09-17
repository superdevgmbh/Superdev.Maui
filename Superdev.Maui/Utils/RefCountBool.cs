namespace Superdev.Maui.Utils
{
    public class RefCountBool
    {
        private readonly object syncObj = new object();

        private int refCount;

        public bool Value
        {
            get
            {
                lock (this.syncObj)
                {
                    return this.refCount > 0;
                }
            }
            set
            {
                this.SetValue(value);
            }
        }

        public void Reset()
        {
            lock (this.syncObj)
            {
                this.refCount = 0;
            }
        }

        public bool SetValue(bool value)
        {
            lock (this.syncObj)
            {
                var oldValue = this.Value;

                if (value)
                {
                    this.refCount++;
                }
                else
                {
                    if (this.refCount > 0)
                    {
                        this.refCount--;
                    }
                }

                var newValue = this.Value;
                return oldValue != newValue;
            }
        }

        public static implicit operator bool(RefCountBool d) => d.Value;
    }
}
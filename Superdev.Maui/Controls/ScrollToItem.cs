namespace Superdev.Maui.Controls
{
    public class ScrollToItem : IEquatable<ScrollToItem>
    {
        public required object Item { get; init; }

        public ScrollToPosition Position { get; init; }

        public bool Animated { get; init; } = true;

        public bool Equals(ScrollToItem other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(this.Item, other.Item) && this.Position == other.Position && this.Animated == other.Animated;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((ScrollToItem)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Item, (int)this.Position, this.Animated);
        }

        public static bool operator ==(ScrollToItem left, ScrollToItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScrollToItem left, ScrollToItem right)
        {
            return !Equals(left, right);
        }
    }
}
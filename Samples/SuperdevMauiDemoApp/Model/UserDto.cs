namespace SuperdevMauiDemoApp.Model
{
    internal class UserDto : IEquatable<UserDto?>
    {
        public int Id { get; init; }

        public string? UserName { get; init; }

        public bool Equals(UserDto? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Id == other.Id && string.Equals(this.UserName, other.UserName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object? obj)
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

            return this.Equals((UserDto)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.Id);
            hashCode.Add(this.UserName, StringComparer.InvariantCultureIgnoreCase);
            return hashCode.ToHashCode();
        }

        public static bool operator ==(UserDto? left, UserDto? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UserDto? left, UserDto? right)
        {
            return !Equals(left, right);
        }
    }
}
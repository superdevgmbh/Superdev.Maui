namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Describes a navigation operation. Instances are passed to the navigation events and to the
    /// <see cref="NavigationService"/> hooks, e.g. in order to log or track navigations.
    /// </summary>
    public class NavigationInfo
    {
        /// <summary>
        /// Gets the kind of the navigation operation.
        /// </summary>
        public required NavigationOperation Operation { get; init; }

        /// <summary>
        /// Gets the direction of the navigation operation.
        /// </summary>
        public required NavigationDirection Direction { get; init; }

        /// <summary>
        /// Gets the navigation name or URI of the target, if any.
        /// </summary>
        public string? PageName { get; init; }

        /// <summary>
        /// Gets the navigation parameter or the back navigation result, if any.
        /// </summary>
        public object? Parameter { get; init; }

        /// <summary>
        /// Indicates whether the navigation is animated.
        /// </summary>
        public bool Animated { get; init; } = true;

        /// <summary>
        /// Returns a copy which reports the operation and the target the pipeline resolved this request to.
        /// </summary>
        internal NavigationInfo With(NavigationOperation operation, string? pageName)
        {
            return new NavigationInfo
            {
                Operation = operation,
                Direction = this.Direction,
                PageName = pageName,
                Parameter = this.Parameter,
                Animated = this.Animated
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.PageName is { } pageName
                ? $"{this.Operation} \"{pageName}\""
                : $"{this.Operation}";
        }
    }
}

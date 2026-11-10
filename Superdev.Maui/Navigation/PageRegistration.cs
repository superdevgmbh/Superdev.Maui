using System.Diagnostics.CodeAnalysis;

namespace Superdev.Maui.Navigation
{
    /// <summary>
    /// Associates a navigation name with a page type and an optional view model type.
    /// </summary>
    /// <seealso cref="ServiceCollectionExtensions"/>
    public class PageRegistration
    {
        /// <summary>
        /// Gets the page type which is created when navigating to <see cref="Name"/>.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public required Type PageType { get; init; }

        /// <summary>
        /// Gets the view model type which is assigned to the page's binding context, if any.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
        public Type? ViewModelType { get; init; }

        /// <summary>
        /// Gets the navigation name. If not given, the name of <see cref="PageType"/> is used.
        /// </summary>
        public string? Name { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ViewModelType is { } viewModelType
                ? $"{this.Name} ({this.PageType.Name} + {viewModelType.Name})"
                : $"{this.Name} ({this.PageType.Name})";
        }
    }
}

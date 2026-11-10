namespace Superdev.Maui.Validation
{
    internal enum ErrorKeyTarget
    {
        /// <summary>A field of this form; <see cref="ErrorKeyRoute.Key"/> is the property name in its canonical casing.</summary>
        Own,

        /// <summary>A field of the child named <see cref="ErrorKeyRoute.ChildName"/>; <see cref="ErrorKeyRoute.Key"/> is the key the child routes.</summary>
        Child,

        /// <summary>Nobody owns the key: it is kept as a form-level error rather than dropped.</summary>
        Form,
    }

    internal readonly record struct ErrorKeyRoute(ErrorKeyTarget Target, string? ChildName, string Key)
    {
        public static ErrorKeyRoute Own(string propertyName) => new ErrorKeyRoute(ErrorKeyTarget.Own, null, propertyName);

        public static ErrorKeyRoute Child(string childName, string key) => new ErrorKeyRoute(ErrorKeyTarget.Child, childName, key);

        public static ErrorKeyRoute Form(string key) => new ErrorKeyRoute(ErrorKeyTarget.Form, null, key);
    }

    /// <summary>
    /// What the router needs to know about a form: its fields, their aliases, and its children.
    /// </summary>
    internal interface IErrorKeyRoutingSource
    {
        /// <summary>A field which declared <paramref name="key"/> as an alias, in canonical casing.</summary>
        string? FindFieldByAlias(string key);

        /// <summary>A field whose name is <paramref name="key"/>, compared case-insensitively, in canonical casing.</summary>
        string? FindField(string key);

        IReadOnlyList<KeyValuePair<string, IErrorKeyRoutingSource>> Children { get; }
    }

    /// <summary>
    /// Decides where an incoming error key lands. In order: a field that claimed the key as an alias (own, then a
    /// child's); a prefix that names a child (<c>Address.Street</c>); an own field; a field exactly one child owns;
    /// else the form, so that nothing is lost silently.
    /// </summary>
    internal static class ErrorKeyRouter
    {
        public static ErrorKeyRoute Route(string key, IErrorKeyRoutingSource source)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(source);

            if (source.FindFieldByAlias(key) is { } ownAlias)
            {
                return ErrorKeyRoute.Own(ownAlias);
            }

            foreach (var (childName, child) in source.Children)
            {
                if (ClaimsAlias(child, key))
                {
                    return ErrorKeyRoute.Child(childName, key);
                }
            }

            var separator = key.IndexOf('.');
            if (separator > 0 && separator < key.Length - 1)
            {
                var prefix = key[..separator];
                foreach (var (childName, _) in source.Children)
                {
                    if (string.Equals(childName, prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return ErrorKeyRoute.Child(childName, key[(separator + 1)..]);
                    }
                }
            }

            if (source.FindField(key) is { } ownField)
            {
                return ErrorKeyRoute.Own(ownField);
            }

            string? owner = null;
            var owners = 0;
            foreach (var (childName, child) in source.Children)
            {
                if (child.FindField(key) != null)
                {
                    owner = childName;
                    owners++;
                }
            }

            if (owners == 1)
            {
                return ErrorKeyRoute.Child(owner!, key);
            }

            return ErrorKeyRoute.Form(key);
        }

        private static bool ClaimsAlias(IErrorKeyRoutingSource source, string key)
        {
            if (source.FindFieldByAlias(key) != null)
            {
                return true;
            }

            foreach (var (_, child) in source.Children)
            {
                if (ClaimsAlias(child, key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

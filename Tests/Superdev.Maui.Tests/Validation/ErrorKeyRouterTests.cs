using Superdev.Maui.Validation;

namespace Superdev.Maui.Tests.Validation
{
    public class ErrorKeyRouterTests
    {
        [Fact]
        public void ShouldPreferAnOwnAlias()
        {
            var source = new FakeSource(fields: new[] { "Email" }, aliases: new Dictionary<string, string> { ["DuplicateEmail"] = "Email" });

            ErrorKeyRouter.Route("DuplicateEmail", source).Should().Be(ErrorKeyRoute.Own("Email"));
        }

        [Fact]
        public void ShouldRouteAChildsAlias_ToTheChild()
        {
            var child = new FakeSource(fields: new[] { "Street" }, aliases: new Dictionary<string, string> { ["InvalidStreet"] = "Street" });
            var source = new FakeSource(fields: new[] { "Email" }, children: new[] { ("Address", (IErrorKeyRoutingSource)child) });

            ErrorKeyRouter.Route("InvalidStreet", source).Should().Be(ErrorKeyRoute.Child("Address", "InvalidStreet"));
        }

        [Fact]
        public void ShouldRouteAPrefixedKey_ToTheChild_WithoutThePrefix()
        {
            var child = new FakeSource(fields: new[] { "Street" });
            var source = new FakeSource(fields: new[] { "Email" }, children: new[] { ("Address", (IErrorKeyRoutingSource)child) });

            ErrorKeyRouter.Route("address.Street", source).Should().Be(ErrorKeyRoute.Child("Address", "Street"));
        }

        [Fact]
        public void ShouldRouteAnOwnField_CaseInsensitively_InCanonicalCasing()
        {
            var source = new FakeSource(fields: new[] { "UserName" });

            ErrorKeyRouter.Route("username", source).Should().Be(ErrorKeyRoute.Own("UserName"));
        }

        [Fact]
        public void ShouldPreferAnOwnField_OverAChildsFieldOfTheSameName()
        {
            var child = new FakeSource(fields: new[] { "Email" });
            var source = new FakeSource(fields: new[] { "Email" }, children: new[] { ("Contact", (IErrorKeyRoutingSource)child) });

            ErrorKeyRouter.Route("Email", source).Should().Be(ErrorKeyRoute.Own("Email"));
        }

        [Fact]
        public void ShouldRouteAField_OnlyOneChildOwns_ToThatChild()
        {
            var address = new FakeSource(fields: new[] { "Street" });
            var person = new FakeSource(fields: new[] { "FirstName" });
            var source = new FakeSource(fields: Array.Empty<string>(), children: new[]
            {
                ("PersonalInformation", (IErrorKeyRoutingSource)person),
                ("Address", (IErrorKeyRoutingSource)address),
            });

            ErrorKeyRouter.Route("firstname", source).Should().Be(ErrorKeyRoute.Child("PersonalInformation", "firstname"));
        }

        [Fact]
        public void ShouldFallBackToTheForm_WhenNobodyOwnsTheKey_OrTwoChildrenDo()
        {
            var a = new FakeSource(fields: new[] { "Name" });
            var b = new FakeSource(fields: new[] { "Name" });
            var source = new FakeSource(fields: new[] { "Email" }, children: new[] { ("A", (IErrorKeyRoutingSource)a), ("B", (IErrorKeyRoutingSource)b) });

            ErrorKeyRouter.Route("Name", source).Should().Be(ErrorKeyRoute.Form("Name"));
            ErrorKeyRouter.Route("InvalidToken", source).Should().Be(ErrorKeyRoute.Form("InvalidToken"));
        }

        private sealed class FakeSource : IErrorKeyRoutingSource
        {
            private readonly string[] fields;
            private readonly Dictionary<string, string> aliases;

            public FakeSource(
                string[] fields,
                Dictionary<string, string>? aliases = null,
                (string Name, IErrorKeyRoutingSource Child)[]? children = null)
            {
                this.fields = fields;
                this.aliases = new Dictionary<string, string>(aliases ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
                this.Children = (children ?? Array.Empty<(string, IErrorKeyRoutingSource)>())
                    .Select(c => new KeyValuePair<string, IErrorKeyRoutingSource>(c.Name, c.Child))
                    .ToArray();
            }

            public IReadOnlyList<KeyValuePair<string, IErrorKeyRoutingSource>> Children { get; }

            public string? FindFieldByAlias(string key)
            {
                return this.aliases.TryGetValue(key, out var field) ? field : null;
            }

            public string? FindField(string key)
            {
                return this.fields.FirstOrDefault(f => string.Equals(f, key, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}

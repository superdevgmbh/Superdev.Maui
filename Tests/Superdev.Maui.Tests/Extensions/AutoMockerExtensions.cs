using Microsoft.Extensions.Logging;
using Superdev.Maui.Tests.Utils;
using Xunit.Abstractions;

namespace Superdev.Maui.Tests.Extensions
{
    public static class AutoMockerExtensions
    {
        public static void UseTestOutputHelperLogger<T>(this AutoMocker autoMocker, ITestOutputHelper testOutputHelper)
        {
            autoMocker.Use<ILogger<T>>(new TestOutputHelperLogger<T>(testOutputHelper));
        }
    }
}
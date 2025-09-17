namespace Superdev.Maui.Utils.Reflection
{
    internal static class ActivatorHelper
    {
        internal static T CreateInstance<T>(params object[] paramArray)
        {
            return (T)Activator.CreateInstance(typeof(T), args: paramArray);
        }
    }
}

namespace Superdev.Maui.Utils.Reflection
{
    internal static class ActivatorHelper
    {
        internal static T CreateInstance<T>(params object[] paramArray)
        {
            T? instance;

            try
            {
                instance = (T?)Activator.CreateInstance(typeof(T), args: paramArray);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Activator.CreateInstance failed to create instance of type {typeof(T)}", e);
            }

            return instance ?? throw new InvalidOperationException($"Activator.CreateInstance failed to create instance of type {typeof(T)}");
        }
    }
}
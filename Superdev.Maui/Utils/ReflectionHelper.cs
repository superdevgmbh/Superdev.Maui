using System.Reflection;

namespace Superdev.Maui.Utils
{
    public static class ReflectionHelper
    {
        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            return (T)GetPropertyValue(obj, propertyName);
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
            {
                throw new ArgumentException(nameof(propertyName),
                    $"Couldn't find property {propertyName} in type {objType.FullName}");
            }

            return propInfo.GetValue(obj, null);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName);
            if (propInfo == null)
            {
                throw new ArgumentException(nameof(propertyName),
                    $"Couldn't find property {propertyName} in type {objType.FullName}");
            }

            propInfo.SetValue(obj, value);
        }

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo;
            do
            {
                propInfo = type.GetProperty(propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase);

                type = type.BaseType;
            } while (propInfo == null && type != null);

            return propInfo;
        }

        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            return (T)GetFieldValue(obj, fieldName);
        }

        public static object GetFieldValue(object obj, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
            {
                throw new ArgumentException(nameof(fieldName),
                    $"Couldn't find field {fieldName} in type {objType.FullName}");
            }

            return fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            ArgumentNullException.ThrowIfNull(obj);

            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName);
            if (fieldInfo == null)
            {
                throw new ArgumentException(nameof(fieldName),
                    $"Couldn't find field {fieldName} in type {objType.FullName}");
            }

            fieldInfo.SetValue(obj, value);
        }

        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            FieldInfo fieldInfo;
            do
            {
                fieldInfo = type.GetField(fieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase);

                type = type.BaseType;
            } while (fieldInfo == null && type != null);

            return fieldInfo;
        }

        public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] parameterTypes = null)
        {
            MethodInfo methodInfo;
            do
            {
                methodInfo = type.GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase,
                    null,
                    parameterTypes ?? Type.EmptyTypes,
                    null);

                type = type.BaseType;
            } while (methodInfo == null && type != null);

            return methodInfo;
        }

        public static object RunMethod(object target, string methodName, params object[] parameters)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var paramTypes = parameters?.Select(p => p?.GetType() ?? typeof(object)).ToArray() ?? Type.EmptyTypes;
            var methodInfo = GetMethodInfo(target.GetType(), methodName, paramTypes);

            if (methodInfo == null)
            {
                throw new MissingMethodException($"Method '{methodName}' not found.");
            }

            return methodInfo.Invoke(target, parameters);
        }

        public static TDelegate GetMethodDelegate<TDelegate>(object target, string methodName) where TDelegate : Delegate
        {
            var methodInfo = GetMethodInfo(target.GetType(), methodName);
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target, methodInfo);
        }

        // Generic version to avoid casting manually
        public static T RunMethod<T>(object target, string methodName, params object[] parameters)
        {
            var result = RunMethod(target, methodName, parameters);
            return (T)result;
        }
    }
}
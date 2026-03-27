using System.Reflection;

namespace Superdev.Maui.Utils
{
    public static class ReflectionHelper
    {
        public static T? GetPropertyValue<T>(object obj, string propertyName)
        {
            return (T?)GetPropertyValue(obj, propertyName);
        }

        public static object? GetPropertyValue(object obj, string propertyName)
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

        public static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
        {
            var targetType = type;
            PropertyInfo? propInfo;

            do
            {
                propInfo = targetType.GetProperty(propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase);

                targetType = targetType.BaseType;
            } while (propInfo == null && targetType != null);

            return propInfo;
        }

        public static T? GetFieldValue<T>(object obj, string fieldName)
        {
            return (T?)GetFieldValue(obj, fieldName);
        }

        public static object? GetFieldValue(object obj, string fieldName)
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

        public static FieldInfo? GetFieldInfo(Type type, string fieldName)
        {
            var targetType = type;
            FieldInfo? fieldInfo;

            do
            {
                fieldInfo = targetType.GetField(fieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase);

                targetType = targetType.BaseType;
            } while (fieldInfo == null && targetType != null);

            return fieldInfo;
        }

        public static MethodInfo? GetMethodInfo(Type type, string methodName, Type[]? parameterTypes = null)
        {
            var targetType = type;
            parameterTypes ??= Type.EmptyTypes;
            MethodInfo? methodInfo;

            do
            {
                methodInfo = targetType.GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase,
                    null,
                    parameterTypes,
                    null);

                targetType = targetType.BaseType;
            } while (methodInfo == null && targetType != null);

            return methodInfo;
        }

        public static object? RunMethod(object target, string methodName, params object[] parameters)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            var paramTypes = parameters.Select(p => p.GetType()).ToArray();
            var methodInfo = GetMethodInfo(target.GetType(), methodName, paramTypes);
            if (methodInfo == null)
            {
                throw new MissingMethodException($"Method with name '{methodName}' not found.");
            }

            return methodInfo.Invoke(target, parameters);
        }

        public static TDelegate GetMethodDelegate<TDelegate>(object target, string methodName) where TDelegate : Delegate
        {
            var methodInfo = GetMethodInfo(target.GetType(), methodName);
            if (methodInfo == null)
            {
                throw new MissingMethodException($"Method with name '{methodName}' not found.");
            }

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target, methodInfo);
        }

        public static T? RunMethod<T>(object target, string methodName, params object[] parameters)
        {
            var result = RunMethod(target, methodName, parameters);
            return (T?)result;
        }
    }
}
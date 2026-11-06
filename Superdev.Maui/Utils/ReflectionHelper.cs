using System.Reflection;

namespace Superdev.Maui.Utils
{
    public static class ReflectionHelper
    {
        private const BindingFlags MemberBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

        private const BindingFlags MethodBindingFlags = MemberBindingFlags | BindingFlags.Static;

        public static T? GetPropertyValue<T>(object obj, string propertyName)
        {
            return (T?)GetPropertyValue(obj, propertyName);
        }

        public static object? GetPropertyValue(object obj, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName)
                ?? throw new ArgumentException($"Couldn't find property {propertyName} in type {objType.FullName}", nameof(propertyName));

            return propInfo.GetValue(obj, null);
        }

        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            var objType = obj.GetType();
            var propInfo = GetPropertyInfo(objType, propertyName)
                ?? throw new ArgumentException($"Couldn't find property {propertyName} in type {objType.FullName}", nameof(propertyName));

            propInfo.SetValue(obj, value);
        }

        public static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentException.ThrowIfNullOrEmpty(propertyName);

            var targetType = type;
            PropertyInfo? propInfo;

            do
            {
                propInfo = targetType.GetProperty(propertyName, MemberBindingFlags);
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
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName)
                ?? throw new ArgumentException($"Couldn't find field {fieldName} in type {objType.FullName}", nameof(fieldName));

            return fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue(object obj, string fieldName, object? value)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            var objType = obj.GetType();
            var fieldInfo = GetFieldInfo(objType, fieldName)
                ?? throw new ArgumentException($"Couldn't find field {fieldName} in type {objType.FullName}", nameof(fieldName));

            fieldInfo.SetValue(obj, value);
        }

        public static FieldInfo? GetFieldInfo(Type type, string fieldName)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentException.ThrowIfNullOrEmpty(fieldName);

            var targetType = type;
            FieldInfo? fieldInfo;

            do
            {
                fieldInfo = targetType.GetField(fieldName, MemberBindingFlags);
                targetType = targetType.BaseType;
            } while (fieldInfo == null && targetType != null);

            return fieldInfo;
        }

        public static MethodInfo? GetMethodInfo(Type type, string methodName, Type[]? parameterTypes = null)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentException.ThrowIfNullOrEmpty(methodName);

            var targetType = type;
            parameterTypes ??= Type.EmptyTypes;
            MethodInfo? methodInfo;

            do
            {
                methodInfo = targetType.GetMethod(methodName, MethodBindingFlags, null, parameterTypes, null);
                targetType = targetType.BaseType;
            } while (methodInfo == null && targetType != null);

            return methodInfo;
        }

        /// <summary>
        /// Invokes the method <paramref name="methodName"/> of <paramref name="target"/>.
        /// The parameter types are derived from the arguments, so none of them may be null:
        /// use the overload which takes the parameter types in order to pass a null argument.
        /// </summary>
        public static object? RunMethod(object target, string methodName, params object?[] parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            var parameterTypes = new Type[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i]?.GetType() ?? throw new ArgumentException(
                    $"The parameter type of the null argument at index {i} cannot be derived. Use the overload which takes the parameter types.",
                    nameof(parameters));
            }

            return RunMethod(target, methodName, parameterTypes, parameters);
        }

        /// <summary>
        /// Invokes the method <paramref name="methodName"/> of <paramref name="target"/>
        /// which has the given <paramref name="parameterTypes"/>.
        /// </summary>
        public static object? RunMethod(object target, string methodName, Type[] parameterTypes, params object?[] parameters)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentException.ThrowIfNullOrEmpty(methodName);
            ArgumentNullException.ThrowIfNull(parameterTypes);
            ArgumentNullException.ThrowIfNull(parameters);

            var methodInfo = GetMethodInfo(target.GetType(), methodName, parameterTypes)
                ?? throw new MissingMethodException(target.GetType().FullName, methodName);

            return methodInfo.Invoke(target, parameters);
        }

        /// <inheritdoc cref="RunMethod(object, string, object?[])"/>
        public static T? RunMethod<T>(object target, string methodName, params object?[] parameters)
        {
            return (T?)RunMethod(target, methodName, parameters);
        }

        /// <inheritdoc cref="RunMethod(object, string, Type[], object?[])"/>
        public static T? RunMethod<T>(object target, string methodName, Type[] parameterTypes, params object?[] parameters)
        {
            return (T?)RunMethod(target, methodName, parameterTypes, parameters);
        }

        /// <summary>
        /// Creates a delegate of type <typeparamref name="TDelegate"/> for the method <paramref name="methodName"/>
        /// of <paramref name="target"/>. The method is resolved by the parameter types of the delegate.
        /// </summary>
        public static TDelegate GetMethodDelegate<TDelegate>(object target, string methodName) where TDelegate : Delegate
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentException.ThrowIfNullOrEmpty(methodName);

            var parameterTypes = typeof(TDelegate).GetMethod(nameof(Action.Invoke))!
                .GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var methodInfo = GetMethodInfo(target.GetType(), methodName, parameterTypes)
                ?? throw new MissingMethodException(target.GetType().FullName, methodName);

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target, methodInfo);
        }
    }
}
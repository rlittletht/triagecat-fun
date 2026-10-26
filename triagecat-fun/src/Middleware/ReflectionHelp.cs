using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.Azure.Functions.Worker;
using tt.Extensions;

namespace tt.Middleware;

public class ReflectionHelp
{
    public static List<T> GetCustomAttributesOnClassAndMethod<T>(MethodInfo targetMethod)
        where T : Attribute
    {
        IEnumerable<T> methodAttributes = targetMethod.GetCustomAttributes<T>();
        IEnumerable<T>? classAttributes = targetMethod.DeclaringType?.GetCustomAttributes<T>();

        if (classAttributes != null)
            return methodAttributes.Concat(classAttributes).ToList();

        return methodAttributes.ToList();
    }

    public static List<T> GetCustomAttributesOnClassAndMethod<T>(FunctionContext context)
        where T : Attribute
    {
        MethodInfo? targetMethod = context.GetTargetFunctionMethod();

        if (targetMethod == null)
            return new List<T>();

        return GetCustomAttributesOnClassAndMethod<T>(targetMethod);
    }

    public static List<T> GetCustomAttributesOnParameters<T>(MethodInfo targetMethod)
        where T : Attribute
    {
        List<T> allAttributes = new();

        foreach (ParameterInfo paramInfo in targetMethod.GetParameters())
        {
            IEnumerable<T> attributes = paramInfo.GetCustomAttributes<T>();

            allAttributes.AddRange(attributes);
        }

        return allAttributes;
    }

    public static List<T> GetCustomAttributesOnParameters<T>(FunctionContext context)
        where T : Attribute
    {
        MethodInfo? targetMethod = context.GetTargetFunctionMethod();

        if (targetMethod == null)
            return new List<T>();

        return GetCustomAttributesOnParameters<T>(targetMethod);
    }

}

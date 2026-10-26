using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;

namespace tt.Extensions;

public static class FunctionContextExtensions
{
    public static ImmutableList<Claim>? Claims(this FunctionContext context)
    {
        if (context.Items.TryGetValue("Claims", out object? item))
            return item as ImmutableList<Claim>;

        return null;
    }

    public static MethodInfo? GetTargetFunctionMethod(this FunctionContext context)
    {
        // terrible reflection code..
        // Would be nice if this was available out of the box on FunctionContext

        // This contains the fully qualified name of the method
        // E.g. IsolatedFunctionAuth.TestFunctions.ScopesAndAppRoles
        string entryPoint = context.FunctionDefinition.EntryPoint;

        string assemblyPath = context.FunctionDefinition.PathToAssembly;
        Assembly assembly = Assembly.LoadFrom(assemblyPath);
        string typeName = entryPoint.Substring(0, entryPoint.LastIndexOf('.'));

        Type? type = assembly.GetType(typeName);
        string methodName = entryPoint.Substring(entryPoint.LastIndexOf('.') + 1);

        MethodInfo? method = type?.GetMethod(methodName);
        return method;
    }
}
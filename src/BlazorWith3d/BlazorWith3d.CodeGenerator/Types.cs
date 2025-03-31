using System.Collections.Generic;
using System.Linq;

using static BlazorWith3d.CodeGenerator.HelloSourceGenerator_TypeScript;


namespace BlazorWith3d.CodeGenerator;


internal record AppInfo(
    TypeInfo app,
    TypeInfo eventHandler,
    IReadOnlyList<MethodInfo> methods,
    IReadOnlyList<MethodInfo> events)
{
    public IEnumerable<string> namespacesToInclude => new string[] { app.@namespace }
        .Concat(methods.SelectMany(o => o.namespaces))
        .Concat(events.SelectMany(o => o.namespaces))
        .Where(o => !string.IsNullOrEmpty(o))
        .Distinct();
}

internal record TypeInfo(string typeNameOrig, string @namespace, bool isNullable, bool isMemoryPackTypescriptGenerated, MemoryPackSpecialType? specialType, IReadOnlyList<(TypeInfo type, string name)> properties)
{
    public string typeName => isNullable ? typeNameOrig + "?" : typeNameOrig;
    public string TypeNameWithoutIPrefix=> typeName.StartsWith("I")? typeName.Substring(1) : typeName;
}

internal static class TypeInfoUtils
{

    public static IEnumerable<TypeInfo> FlattenProperties(this TypeInfo type)
    {


        yield return type;

        if (type.properties == null)
        {
            yield break;
        }

        foreach(var prop in type.properties)
        {
            foreach (var propType in prop.type.FlattenProperties())
            {
                yield return propType;
            }
        }

    }
}


internal record MethodInfo(string name, TypeInfo? returnType, (TypeInfo argType, string argName)[] arguments )
{
    public IEnumerable<string> namespaces
    {
        get
        {
            if (returnType != null)
            {
                yield return returnType.@namespace;
            }

            foreach (var arg in arguments)
            {
                yield return arg.argType.@namespace;
            }
        }
    }
}
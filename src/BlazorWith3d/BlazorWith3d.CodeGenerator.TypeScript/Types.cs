using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.CodeGenerator;

internal record TwoWayAppInfo(
    TypeInfo app,
    TypeInfo eventHandler,
    IReadOnlyList<MethodInfo> methods,
    IReadOnlyList<MethodInfo> events)
{
    public IEnumerable<string> NamespacesToInclude => new string?[] { app.@namespace }
        .Concat(methods.SelectMany(o => o.namespaces))
        .Concat(events.SelectMany(o => o.namespaces))
        .Where(o => !string.IsNullOrEmpty(o))
        .Select(o=>o!)
        .Distinct();
}

internal record TwoWayAppInfoWithOwner(
    TypeInfo ownerType,
    TypeInfo app,
    TypeInfo eventHandler,
    IReadOnlyList<MethodInfo> methods,
    IReadOnlyList<MethodInfo> events)
{
    public IEnumerable<string> NamespacesToInclude => new string?[] { app.@namespace, ownerType.@namespace }
        .Concat(methods.SelectMany(o => o.namespaces))
        .Concat(events.SelectMany(o => o.namespaces))
        .Where(o => !string.IsNullOrEmpty(o))
        .Select(o=>o!)
        .Distinct();
}

internal record TypeInfo(string typeNameOrig, string @namespace, bool isNullable, BuiltInType? specialType, IReadOnlyList<(TypeInfo type, string name)> properties)
{
    public string typeName => isNullable ? typeNameOrig + "?" : typeNameOrig;
    public string TypeNameWithoutIPrefix=> typeName.StartsWith("I")? typeName.Substring(1) : typeName;
}


internal record BuiltInType(Type dotnetType, string tsType)
{
    public static IReadOnlyList<BuiltInType> KnownTypes { get; }= new BuiltInType[]
    {
        new (typeof(string),"string | null"),
        
        new (typeof(bool),"boolean"),
        new (typeof(SByte),"number"),
        new (typeof(short),"number"),
        new (typeof(int),"number"),
        new (typeof(long),"bigint"),
        new (typeof(byte),"number"),
        new (typeof(ushort),"number"),
        new (typeof(uint),"number"),
        new (typeof(ulong),"bigint"),
        new (typeof(float),"number"),
        new (typeof(double),"number"),
        new (typeof(decimal),"number"),
        new (typeof(DateTime),"Date"),
        
        new (typeof(Nullable<bool>),"boolean | null"),
        new (typeof(Nullable<SByte>),"number | null"),
        new (typeof(Nullable<short>),"number | null"),
        new (typeof(Nullable<int>),"number | null"),
        new (typeof(Nullable<long>),"bigint | null"),
        new (typeof(Nullable<byte>),"number | null"),
        new (typeof(Nullable<ushort>),"number | null"),
        new (typeof(Nullable<uint>),"number | null"),
        new (typeof(Nullable<ulong>),"bigint | null"),
        new (typeof(Nullable<float>),"number | null"),
        new (typeof(Nullable<double>),"number | null"),
        new (typeof(Nullable<decimal>),"number | null"),
        new (typeof(Nullable<DateTime>),"Date | null"),
    };

    public static bool TryGetBuiltInType(INamedTypeSymbol typeSymbol, out BuiltInType builtInType)
    {
        var typeFullName = $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
        builtInType = KnownTypes.FirstOrDefault(o => o.dotnetType.FullName == typeFullName);
        return builtInType != null;
    }
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
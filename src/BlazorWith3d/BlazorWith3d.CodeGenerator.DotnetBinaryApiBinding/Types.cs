using System.Collections.Generic;
using System.Linq;

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

internal record TypeInfo(string typeNameOrig, string @namespace, bool isNullable)
{
    public string typeName => isNullable ? typeNameOrig + "?" : typeNameOrig;
    public string TypeNameWithoutIPrefix=> typeName.StartsWith("I")? typeName.Substring(1) : typeName;
}

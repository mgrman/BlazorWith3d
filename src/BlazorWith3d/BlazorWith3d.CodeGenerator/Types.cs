using System.Collections.Generic;
using System.Linq;

namespace BlazorWith3d.Unity.CodeGenerator;


internal record AppInfo(
    TypeInfo app,
    string eventHandlerConceptName,
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

internal record TypeInfo(string typeNameOrig, string @namespace, bool isNullable, bool isMemoryPackTypescriptGenerated)
{
    public string typeName => isNullable ? typeNameOrig + "?" : typeNameOrig;
    public string TypeNameWithoutIPrefix=> typeName.StartsWith("I")? typeName.Substring(1) : typeName;
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
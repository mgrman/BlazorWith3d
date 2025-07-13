using System.Collections.Generic;
using System.Linq;

using static BlazorWith3d.CodeGenerator.HelloSourceGenerator_MemoryPackTypeScriptForStructs;


namespace BlazorWith3d.CodeGenerator;


internal record TwoWayAppInfo(
    TypeInfo app,
    TypeInfo eventHandler,
    IReadOnlyList<MethodInfo> methods,
    IReadOnlyList<MethodInfo> events)
{
    public IEnumerable<TypeInfo> AllTypesNonDistinct()
    {
        return this.methods.Concat(this.events)
            .SelectMany(o => o.arguments.Select(a => a.argType).ConcatOptional(o.returnType, o.returnType != null))
            .Concat([app, eventHandler])
            .SelectMany(o => o.FlattenProperties());
    }
}

internal record TypeInfo(string typeNameOrig, string @namespace, bool isNullable, bool isMemoryPackTypescriptGenerated, MemoryPackSpecialType? specialType, IReadOnlyList<(TypeInfo type, string name)> properties, bool isNonSequentialStruct)
{
    public string typeName => isNullable ? typeNameOrig + "?" : typeNameOrig;
    public string TypeNameWithoutIPrefix=> typeName.StartsWith("I")? typeName.Substring(1) : typeName;
    
    
    public IEnumerable<TypeInfo> FlattenProperties()
    {
        yield return this;

        if (this.properties == null)
        {
            yield break;
        }

        foreach(var prop in this.properties)
        {
            foreach (var propType in prop.type.FlattenProperties())
            {
                yield return propType;
            }
        }
    }
}

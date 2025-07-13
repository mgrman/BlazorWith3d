using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.CodeGenerator;

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
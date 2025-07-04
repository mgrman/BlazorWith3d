using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.CodeGenerator;

public static class ContextUtilities
{
    public static void AddSourceFiles(this GeneratorExecutionContext context,
        IEnumerable<(string name, string content)> files)
    {
        foreach (var (name, content) in files)
        {
            context.AddSource(name, content);
        }
    }
}   
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public static void AddSourceFiles(this SourceProductionContext context,
        IEnumerable<(string name, string content)> files)
    {
        foreach (var (name, content) in files)
        {
            context.AddSource(name, content);
        }
    }

    public static bool HasAttribute(this InterfaceDeclarationSyntax syntax, string attributeName)
    {
        return HasAttribute(syntax, attributeName, out _);
    }

    public static bool HasAttribute(this InterfaceDeclarationSyntax syntax, string attributeName, out AttributeSyntax attr)
    {
        return syntax.AttributeLists.SelectMany(e => e.Attributes)
                 .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out attr);
    }
    public static bool HasAttribute(this TypeDeclarationSyntax syntax, string attributeName)
    {
        return HasAttribute(syntax, attributeName, out _);
    }

    public static bool HasAttribute(this TypeDeclarationSyntax syntax, string attributeName, out AttributeSyntax attr)
    {
        return syntax.AttributeLists.SelectMany(e => e.Attributes)
                 .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out attr);
    }
}   
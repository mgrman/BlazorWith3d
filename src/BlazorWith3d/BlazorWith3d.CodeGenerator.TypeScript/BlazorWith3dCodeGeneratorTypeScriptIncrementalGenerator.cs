using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BlazorWith3d.CodeGenerator;

[Generator]
public class BlazorWith3dCodeGeneratorTypeScriptIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<(TwoWayAppInfo? info, string localPath)> tsInteropTypesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InterfaceDeclarationSyntax ids && ids.HasAttribute("GenerateTSTypes"),
                transform: static (ctx, _) => (info:ConvertInterfaceWithAttribute(ctx, "GenerateTSTypes"), localPath: GetAttributeStringValue(ctx, "GenerateTSTypes",1)))
            .Where(static m => m.info is not null);

        var aaa=context.AnalyzerConfigOptionsProvider
            .Select((analyzerConfigOptionsProvider, r) =>
            {
                
                // https://github.com/dotnet/project-system/blob/main/docs/design-time-builds.md
                var isDesignTimeBuild =
                    analyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", out var designTimeBuild) &&
                    designTimeBuild == "true";
                if (isDesignTimeBuild)
                {
                    return null;
                }

                if (!analyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir))
                {
                    return null;
                }

                return projectDir;
            });
        
        context.RegisterSourceOutput(tsInteropTypesToGenerate.Combine(aaa),
            static (context, combined) =>
            {
                if (combined.Right == null)
                {
                    return;
                }

                var baseDir = combined.Right;
                
                var types = combined.Left;
                var generatedTypeScript = HelloSourceGenerator_TypeScript.GenerateTypeScriptClasses(types.localPath,types.info);
                foreach (var (ts, path) in generatedTypeScript)
                {
                    // save to file
                    try
                    {
                        var fullPath= Path.Combine(baseDir, path);
                        if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                        }

                        File.WriteAllText(fullPath, ts, new UTF8Encoding(false));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            });
    }

    static string? GetAttributeStringValue(GeneratorSyntaxContext context, string attributeName, int argumentPosition)
    {
        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;
        
        if (!typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out var attr))
        {
            return null;
        }

        var literalExpressionSyntax = (attr.ArgumentList.Arguments[argumentPosition].Expression as LiteralExpressionSyntax);
        return literalExpressionSyntax.Token.ValueText;
    }

    static TwoWayAppInfo? ConvertInterfaceWithAttribute(GeneratorSyntaxContext context, string attributeName)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        if (!interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
            .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out var attr))
        {
            return null;
        }

        var eventHandlerType = (attr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
        return InterfaceTypeToTwoWayAppInfo(context.SemanticModel, interfaceDeclarationSyntax, eventHandlerType);
    }

    static TwoWayAppInfo InterfaceTypeToTwoWayAppInfo(SemanticModel context, InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)
    {
        var methodHandlerTypeInfo = GetTypeInfo(mainType);
        var eventHandlerTypeInfo = GetTypeInfo(eventHandlerType, context.Compilation);

        var eventHandlerTypeSymbol = context.Compilation.GetSemanticModel(eventHandlerType.SyntaxTree).GetTypeInfo(eventHandlerType).Type as INamedTypeSymbol;

        var methods = GetMethodInfosFromDeclaration(context,mainType);

        var events = GetMethodInfos(context, eventHandlerTypeSymbol);
        return new TwoWayAppInfo(methodHandlerTypeInfo, eventHandlerTypeInfo, methods, events);
    }

    static List<MethodInfo> GetMethodInfosFromDeclaration(SemanticModel context, InterfaceDeclarationSyntax o)
    {
        var methods = o.Members.OfType<MethodDeclarationSyntax>()
            .Select(m => GetMethodInfo(m, context.Compilation))
            .Where(m => m != null)
            .ToList();
        return methods;
    }
    static List<MethodInfo> GetMethodInfos(SemanticModel context, INamedTypeSymbol? o)
    {
        var methods = o.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => GetMethodInfo(m, context.Compilation))
            .Where(m => m != null)
            .ToList();
        return methods;
    }


    private static TypeInfo GetTypeInfo(TypeDeclarationSyntax typeDeclaration)
    {
        var typeName = typeDeclaration.Identifier.ToString();

        var parent = typeDeclaration.Parent;

        string namespaceName;
        if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
        {
            namespaceName = fileScopedNamespaceDeclarationSyntax.Name.ToString();
        }
        else if (parent is NamespaceDeclarationSyntax mamespaceDeclarationSyntax)
        {
            namespaceName = mamespaceDeclarationSyntax.Name.ToString();
        }
        else
        {
            throw new InvalidOperationException();
        }
        
        return new TypeInfo(typeName, namespaceName, false,  null,null);
    }

    private static TypeInfo GetTypeInfo(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var typeName = typeSymbol.Name;
        bool isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        if (typeName == nameof(Nullable))
        {
            isNullable = true;
            typeName = typeSymbol.TypeArguments[0].Name;
        }
        
        if (BuiltInType.TryGetBuiltInType(typeSymbol, out var specialType))
        {
            return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, specialType, null!);
        }

        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Select(o =>
            {
                var typeInfo = GetTypeInfo(o.Type as INamedTypeSymbol, compilation);
                return (typeInfo, o.Name);
            })
            .ToList();

        return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, null, properties);
    }

    private static TypeInfo GetTypeInfo(TypeSyntax typeSyntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type as INamedTypeSymbol;

        return GetTypeInfo(typeInfo, compilation);
    }

    private static MethodInfo GetMethodInfo(IMethodSymbol method, Compilation compilation)
    {
        var name = method.Name;

        INamedTypeSymbol? returnType;
        if (method.ReturnType is INamedTypeSymbol identifierNameSyntax)
        {
            if (identifierNameSyntax.Name != "ValueTask")
            {
                throw new InvalidOperationException();
            }
            if (identifierNameSyntax.IsGenericType  && identifierNameSyntax.TypeArguments.Length==1)
            {
                returnType = identifierNameSyntax.TypeArguments[0] as INamedTypeSymbol;
            }
            else
            {
                returnType = null;
            }
        }
        else
        {
            throw new InvalidOperationException();
        }

        var arguments = new List<(TypeInfo argType, string argName)>();
        foreach (var arg in method.Parameters)
        {
            var argName = arg.Name;
            var argType = GetTypeInfo(arg.Type as INamedTypeSymbol, compilation);
            arguments.Add((argType, argName)); 
        }

        return new MethodInfo(name, returnType==null?null:GetTypeInfo(returnType , compilation), arguments.ToArray());
    }

    private static MethodInfo GetMethodInfo(MethodDeclarationSyntax method, Compilation compilation)
    {
        var name = method.Identifier.Text;

        TypeInfo? returnType;
        if (method.ReturnType is IdentifierNameSyntax identifierNameSyntax)
        {
            if (identifierNameSyntax.Identifier.ValueText != "ValueTask")
            {
                throw new InvalidOperationException();
            }

            returnType = null;

        }
        else if (method.ReturnType is GenericNameSyntax genericNameSyntax)
        {
            if (genericNameSyntax.Identifier.ValueText != "ValueTask")
            {
                throw new InvalidOperationException();
            }

            returnType = GetTypeInfo(genericNameSyntax.TypeArgumentList.Arguments[0], compilation);
        }
        else
        {
            throw new InvalidOperationException();
        }

        var arguments = new List<(TypeInfo argType, string argName)>();
        foreach (var arg in method.ParameterList.Parameters)
        {
            var argName = arg.Identifier.ValueText;
            var argType = GetTypeInfo(arg.Type, compilation);
            arguments.Add((argType, argName));
        }


        return new MethodInfo(name, returnType, arguments.ToArray());
    }


}

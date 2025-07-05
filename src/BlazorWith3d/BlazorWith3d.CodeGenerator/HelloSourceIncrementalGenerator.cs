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
public class EnumGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Attributes.g.cs",
            SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<TwoWayAppInfo?> binaryApiTypesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InterfaceDeclarationSyntax ids && ids.HasAttribute("GenerateBinaryApi"),
                transform: static (ctx, _) => ConvertInterfaceWithAttribute(ctx, "GenerateBinaryApi")) 
            .Where(static m => m is not null); 

        context.RegisterSourceOutput(binaryApiTypesToGenerate,
            static (context, appInfo) =>
            {
                var files = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo!);
                context.AddSourceFiles(files);
            });


        IncrementalValuesProvider<TwoWayAppInfo?> memoryPackTsInteropTypesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InterfaceDeclarationSyntax ids && ids.HasAttribute("GenerateTSTypesWithMemoryPack"),
                transform: static (ctx, _) => ConvertInterfaceWithAttribute(ctx, "GenerateTSTypesWithMemoryPack"))
            .Where(static m => m is not null);


        var tsOptions=HelloSourceGenerator_TypeScript.GetTsOptions(context);

        context.RegisterSourceOutput(memoryPackTsInteropTypesToGenerate.Combine(tsOptions),
            static (context, combined) =>
            {
                var generatedTypeScript = HelloSourceGenerator_TypeScript.GenerateTypeScriptClass(combined.Right, combined.Left);
                foreach (var (ts, path) in generatedTypeScript)
                {
                    // save to file
                    try
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(path)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(path));
                        }

                        File.WriteAllText(path, ts, new UTF8Encoding(false));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                }
            });


        IncrementalValuesProvider<TwoWayAppInfoWithOwner?> blazorBindingTypesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is TypeDeclarationSyntax ids && ids.HasAttribute("GenerateDirectBinding"),
                transform: static (ctx, _) => ConvertTypeWithAttribute(ctx, "GenerateDirectBinding"))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(blazorBindingTypesToGenerate,
            static (context, appInfo) =>
            {
                var files = HelloSourceGenerator_BlazorBinding.GenerateBindingClass(appInfo!);

                context.AddSourceFiles(files);
            });
    }


    static TwoWayAppInfo? ConvertInterfaceWithAttribute(GeneratorSyntaxContext context, string attributeName)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        if (!interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
            .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out var generateBinaryApiAttr))
        {
            return null;
        }

        var eventHandlerType = (generateBinaryApiAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
        return InterfaceTypeToTwoWayAppInfo(context.SemanticModel, interfaceDeclarationSyntax, eventHandlerType);
    }

    static TwoWayAppInfoWithOwner? ConvertTypeWithAttribute(GeneratorSyntaxContext context, string attributeName)
    {

        var typeDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

        if (!typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
            .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName,
                out var generateDirectBindingAttr))
        {

            return null;
        }

        var methodHandlerType = (generateDirectBindingAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
        var eventHandlerType = (generateDirectBindingAttr.ArgumentList.Arguments[1].Expression as TypeOfExpressionSyntax).Type;
        return ConcreteTypeToTwoWayAppInfoWithOwner(context.SemanticModel, typeDeclarationSyntax, methodHandlerType, eventHandlerType);
    }

   static TwoWayAppInfoWithOwner ConcreteTypeToTwoWayAppInfoWithOwner(SemanticModel context, TypeDeclarationSyntax mainType, TypeSyntax methodHandlerType, TypeSyntax eventHandlerType)
    {
        var bindingType = GetTypeInfo(mainType);
        var eventHandlerTypeSymbol = context.Compilation.GetSemanticModel(eventHandlerType.SyntaxTree).GetTypeInfo(eventHandlerType).Type as INamedTypeSymbol;
        var methodHandlerTypeSymbol = context.Compilation.GetSemanticModel(methodHandlerType.SyntaxTree).GetTypeInfo(methodHandlerType).Type as INamedTypeSymbol;

        var events = GetMethodInfos(context, methodHandlerTypeSymbol);

        var methods = GetMethodInfos(context, eventHandlerTypeSymbol);
        return new TwoWayAppInfoWithOwner(bindingType, GetTypeInfo(eventHandlerTypeSymbol, context.Compilation), GetTypeInfo(methodHandlerTypeSymbol, context.Compilation), methods, events);
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
        
        return new TypeInfo(typeName, namespaceName, false, typeDeclaration.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.NormalizeWhitespace().ToFullString().StartsWith("GenerateTypeScript")), null,null);
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

        if (HelloSourceGenerator_TypeScript.TryGetSpecialType(typeSymbol, out var specialType))
        {
            return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, false, specialType, null!);
        }


        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Select(o =>
            {
                var typeInfo = GetTypeInfo(o.Type as INamedTypeSymbol, compilation);
                return (typeInfo, o.Name);
            })
            .ToList();

        return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, typeSymbol.GetAttributes().Any(a=>a.AttributeClass.Name.StartsWith("GenerateTypeScript")), null, properties);
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

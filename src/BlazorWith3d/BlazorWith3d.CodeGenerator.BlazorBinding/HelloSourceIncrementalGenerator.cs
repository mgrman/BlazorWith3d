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
public class HelloSourceIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
        
        return new TypeInfo(typeName, namespaceName, false);
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

        return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable);
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
}

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

namespace BlazorWith3d.CodeGenerator;

[Generator]
public class HelloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context) // Implement Execute method
    {

        // retrieve the populated receiver 
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
        {
            return;
        }

        if (!receiver.ShouldGenerate)
        {
            return;
        }

        List<MethodInfo> GetMethodInfos(INamedTypeSymbol? o)
        {
            var methods = o.GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();
            return methods;
        }
        
        List<MethodInfo> GetMethodInfosFromDeclaration(InterfaceDeclarationSyntax o)
        {
            var methods = o.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();
            return methods;
        }

        TwoWayAppInfo InterfaceTypeToTwoWayAppInfo((InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType) o)
        {
            var methodHandlerTypeInfo = GetTypeInfo(o.mainType);
            var eventHandlerTypeInfo = GetTypeInfo(o.eventHandlerType, context.Compilation);

            var eventHandlerTypeSymbol = context.Compilation.GetSemanticModel(o.eventHandlerType.SyntaxTree).GetTypeInfo(o.eventHandlerType).Type as INamedTypeSymbol;

            var methods = GetMethodInfosFromDeclaration(o.mainType);

            var events = GetMethodInfos(eventHandlerTypeSymbol);
            return new TwoWayAppInfo(methodHandlerTypeInfo, eventHandlerTypeInfo, methods, events);
        }


        TwoWayAppInfoWithOwner ConcreteTypeToTwoWayAppInfoWithOwner((TypeDeclarationSyntax mainType, TypeSyntax methodHandlerType, TypeSyntax eventHandlerType) o)
        {
            var bindingType = GetTypeInfo(o.mainType);
            var eventHandlerTypeSymbol = context.Compilation.GetSemanticModel(o.eventHandlerType.SyntaxTree).GetTypeInfo(o.eventHandlerType).Type as INamedTypeSymbol;
            var methodHandlerTypeSymbol= context.Compilation.GetSemanticModel(o.methodHandlerType.SyntaxTree).GetTypeInfo(o.methodHandlerType).Type as INamedTypeSymbol;

            var events = GetMethodInfos(methodHandlerTypeSymbol);

            var methods = GetMethodInfos(eventHandlerTypeSymbol);
            return new TwoWayAppInfoWithOwner(bindingType,GetTypeInfo(eventHandlerTypeSymbol, context.Compilation), GetTypeInfo(methodHandlerTypeSymbol, context.Compilation), methods, events);
        }

        foreach (var appInfo in receiver.BinaryApiTypes.Select(InterfaceTypeToTwoWayAppInfo))
        {
            var files = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo);
            context.AddSourceFiles(files);
        }
        
        
        foreach (var appInfo in receiver.MemoryPackTsInteropTypes.Select(InterfaceTypeToTwoWayAppInfo))
        {
            var generatedTypeScript = HelloSourceGenerator_TypeScript.GenerateTypeScriptClass(context, appInfo);
            foreach(var (ts,path) in generatedTypeScript)
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
        }

        foreach (var appInfo in receiver.BlazorBindingTypes.Select(ConcreteTypeToTwoWayAppInfoWithOwner))
        {
            var files = HelloSourceGenerator_BlazorBinding.GenerateBindingClass(appInfo);

            context.AddSourceFiles(files);
        }
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


    /// <summary>
    ///     Created on demand before each generation pass
    /// </summary>
    private class SyntaxReceiver : ISyntaxContextReceiver
    {
        public bool ShouldGenerate => BinaryApiTypes.Count > 0 || BlazorBindingTypes.Count>0|| HttpClientTypes.Count>0|| MemoryPackTsInteropTypes.Count>0|| HttpControllerTypes.Count>0;

        public List<(InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)> BinaryApiTypes { get; } = new();
        public List<InterfaceDeclarationSyntax> HttpClientTypes { get; } = new();
        public List<(TypeDeclarationSyntax mainType, TypeSyntax methodHandlerType)> HttpControllerTypes { get; } = new();
        public List<(InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)> MemoryPackTsInteropTypes { get; } = new();

        public List<(TypeDeclarationSyntax mainType, TypeSyntax methodHandlerType, TypeSyntax eventHandlerType)> BlazorBindingTypes { get; } = new();
        /// <summary>
        ///     Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for
        ///     generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
            {
                if (interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "GenerateBinaryApi", out var generateBinaryApiAttr))
                {
                    var eventHandlerType = (generateBinaryApiAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;

                    BinaryApiTypes.Add((interfaceDeclarationSyntax, eventHandlerType));
                }
                
                if (interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "GenerateTSTypesWithMemoryPack", out var generateTSTypesAttr))
                {
                    var eventHandlerType = (generateTSTypesAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;

                    MemoryPackTsInteropTypes.Add((interfaceDeclarationSyntax, eventHandlerType));
                }
                
                if (interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "GenerateHttpClient", out var generateHttpClientAttr))
                {
                    HttpClientTypes.Add(interfaceDeclarationSyntax);
                }
            }
            
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "GenerateDirectBinding",
                        out var generateDirectBindingAttr))
                {

                    var methodHandlerType = (generateDirectBindingAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
                    var eventHandlerType = (generateDirectBindingAttr.ArgumentList.Arguments[1].Expression as TypeOfExpressionSyntax).Type;
                    BlazorBindingTypes.Add((typeDeclarationSyntax,methodHandlerType, eventHandlerType));
                }
                
                if (typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "GenerateHttpController",
                        out var generateHttpControllerAttr))
                {

                    var methodHandlerType = (generateHttpControllerAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
                    HttpControllerTypes.Add((typeDeclarationSyntax, methodHandlerType));
                }
            }
        }
    }
}

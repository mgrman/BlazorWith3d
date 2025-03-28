using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlazorWith3d.Unity.CodeGenerator;

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


        var controllers = receiver.ControllerTypes.Select(typeDeclaration =>
        {

            var controllerType = GetTypeInfo(typeDeclaration.mainType);
            var rendererType = GetTypeInfo(typeDeclaration.eventHandlerType, context.Compilation);



            var methods = typeDeclaration.mainType.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();



            var events = context.Compilation
                .GetSemanticModel(typeDeclaration.eventHandlerType.SyntaxTree)
                .GetTypeInfo(typeDeclaration.eventHandlerType)
                .Type
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();

            return new AppInfo(controllerType, "Renderer", true, rendererType, methods, events);
        });


        var renderers = receiver.RendererTypes.Select(typeDeclaration =>
        {
            var rendererType = GetTypeInfo(typeDeclaration.mainType);
            var controllerType = GetTypeInfo(typeDeclaration.eventHandlerType, context.Compilation);



            var methods = typeDeclaration.mainType.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();


            var events = context.Compilation
                .GetSemanticModel(typeDeclaration.eventHandlerType.SyntaxTree)
                .GetTypeInfo(typeDeclaration.eventHandlerType)
                .Type
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m=>m!=null)
                .ToList();

            return new AppInfo(rendererType, "Controller", false, controllerType, methods, events);

        });


        foreach (var appInfo in controllers)
        {
            var text = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo);
            context.AddSource($"{appInfo.app.typeName}.g.cs", text);

            var generatedTypeScript = HelloSourceGenerator_TypeScript.GenerateTypeScriptClass(context, appInfo);

            if (generatedTypeScript != null)
            {
                var (ts, path) = generatedTypeScript.Value;
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

        foreach (var appInfo in renderers)
        {
            var text = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo);

            context.AddSource($"{appInfo.app.typeName}.g.cs", text);

        }

        foreach (var o in receiver.BlazorBindingTypes)
        {
            var rendererInterface = context.Compilation.GetSemanticModel(o.renderer.SyntaxTree).GetTypeInfo(o.renderer).Type as INamedTypeSymbol;
            if (!rendererInterface.GetAttributes().Any(a => a.AttributeClass.Name == "Blazor3DRendererAttribute"))
            {
                throw new InvalidOperationException();
            }
            var controllerInterface = context.Compilation.GetSemanticModel(o.controller.SyntaxTree).GetTypeInfo(o.controller).Type as INamedTypeSymbol;
            if (!controllerInterface.GetAttributes().Any(a => a.AttributeClass.Name == "Blazor3DControllerAttribute"))
            {
                throw new InvalidOperationException();
            }

            var bindingType = GetTypeInfo(o.mainType);



            var events = controllerInterface.GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();

            var methods = rendererInterface.GetMembers()
                .OfType<IMethodSymbol>()
                .Select(m => GetMethodInfo(m, context.Compilation))
                .Where(m => m != null)
                .ToList();

            var text = HelloSourceGenerator_BlazorBinding.GenerateBindingClass(bindingType, new AppInfo(GetTypeInfo(rendererInterface, context.Compilation), "Controller", false, GetTypeInfo(controllerInterface, context.Compilation), methods, events));

            context.AddSource($"{bindingType.typeName}.g.cs", text);

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

        return new TypeInfo(typeName, namespaceName, false, typeDeclaration.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.NormalizeWhitespace().ToFullString().StartsWith("GenerateTypeScript")));
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


        return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, typeSymbol.GetAttributes().Any(a=>a.AttributeClass.Name.StartsWith("GenerateTypeScript")));
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
        if (name == "SetRenderer" || name == "SetController")
        {
            return null;
        }

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
        public bool ShouldGenerate => ControllerTypes.Count > 0 || RendererTypes.Count>0 || BlazorBindingTypes.Count>0;

        public List<(InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)> ControllerTypes { get; } = new();

        public List<(InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)> RendererTypes { get; } = new();

        public List<(TypeDeclarationSyntax mainType, TypeSyntax controller, TypeSyntax renderer)> BlazorBindingTypes { get; } = new();
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
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DController", out var blazor3DAppAttr))
                {
                    var rendererType = (blazor3DAppAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;

                    ControllerTypes.Add((interfaceDeclarationSyntax, rendererType));
                }

                if (interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DRenderer", out var unity3DAppAttr))
                {
                    var controllerType = (unity3DAppAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
                    RendererTypes.Add((interfaceDeclarationSyntax, controllerType));
                }
            }
            
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DAppBinding",
                        out var blazor3DBindingAttr))
                {

                    var controllerType = (blazor3DBindingAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
                    var rendererType = (blazor3DBindingAttr.ArgumentList.Arguments[1].Expression as TypeOfExpressionSyntax).Type;
                    BlazorBindingTypes.Add((typeDeclarationSyntax,controllerType, rendererType));
                }
            }
        }
    }
}

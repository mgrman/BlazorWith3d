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

        foreach (var o in receiver.BlazorBindingTypes)
        {
            var mainInterfaceType = o.BaseList.Types.OfType<SimpleBaseTypeSyntax>()
                .Select(t =>
                {
                    var semanticModel = context.Compilation.GetSemanticModel(t.SyntaxTree);

                    var mainInterfaceType = semanticModel.GetTypeInfo(t.Type).Type;
                    if (mainInterfaceType.GetAttributes().Any(a => a.AttributeClass.Name=="Blazor3DAppAttribute"))
                    {
                        return mainInterfaceType;
                    }

                    return null;
                })
                .FirstOrDefault(o=>o != null);
            
            var bindingType =GetTypeInfo( o);

            var invokerInterfaceType = mainInterfaceType.Interfaces.First(o => o.Name.EndsWith("_MethodInvoker"));

            var eventHandlerInterfaceType = invokerInterfaceType.ContainingAssembly.GetTypeByMetadataName($"{invokerInterfaceType.ContainingNamespace.ToDisplayString()}.{mainInterfaceType.Name}_EventHandler");



            var text = HelloSourceGenerator_BlazorBinding.GenerateBindingClass(bindingType, mainInterfaceType, invokerInterfaceType, eventHandlerInterfaceType);

            context.AddSource($"{bindingType.typeName}.g.cs", text);

        }


        var messagesToRenderer = receiver.MessagesToRenderer
            .Select(m=>GetMethodInfo(m,context.Compilation)).ToList();

        var messagesToController = receiver.MessagesToController
            .Select(m=>GetMethodInfo(m,context.Compilation)).ToList();

        
        foreach (var typeDeclaration in receiver.ControllerTypes)
        {
            var controllerType = GetTypeInfo(typeDeclaration.mainType);
            var rendererType = GetTypeInfo(typeDeclaration.eventHandlerType, context.Compilation);

            var appInfo= new AppInfo(controllerType,"Renderer", rendererType, messagesToController, messagesToRenderer);
            
            var text = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo);
            context.AddSource($"{controllerType.typeName}.g.cs", text);
        }

        foreach (var typeDeclaration in receiver.RendererTypes)
        {
            var rendererType = GetTypeInfo(typeDeclaration.mainType);
            var controllerType = GetTypeInfo(typeDeclaration.eventHandlerType, context.Compilation);

            var appInfo= new AppInfo(rendererType, "Controller", controllerType, messagesToRenderer, messagesToController);

            var text = HelloSourceGenerator_DotnetApis.GenerateClass(appInfo);

            context.AddSource($"{rendererType.typeName}.g.cs", text);

            var generatedTypeScript= HelloSourceGenerator_TypeScript.GenerateTypeScriptClass(context, appInfo);

            if (generatedTypeScript != null)
            {
                var (path, ts) = generatedTypeScript.Value;
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

        return new TypeInfo(typeName, namespaceName);
    }

    private static TypeInfo GetTypeInfo(TypeSyntax typeSyntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type;

        return new TypeInfo(typeInfo.Name, typeInfo.ContainingNamespace.ToDisplayString());
    }

    private static MethodInfo GetMethodInfo(MethodDeclarationSyntax method, Compilation compilation)
    {
     var name=     method.Identifier.Text;

        TypeInfo? returnType;
        if(method.ReturnType is IdentifierNameSyntax identifierNameSyntax)
        {
            if(identifierNameSyntax.Identifier.ValueText!= "ValueTask")
            {
                throw new InvalidOperationException();
            }
            returnType = null;

        }
        else if( method.ReturnType is GenericNameSyntax genericNameSyntax)
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
        foreach(var arg in method.ParameterList.Parameters)
        {
            var argName = arg.Identifier.ValueText;
            var argType= GetTypeInfo(arg.Type, compilation);
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

        public List<TypeDeclarationSyntax  > BlazorBindingTypes { get; } = new();

        public List<MethodDeclarationSyntax > MessagesToRenderer { get; } = new();

        public List<MethodDeclarationSyntax > MessagesToController { get; } = new();

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

                    foreach (var method in interfaceDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
                    {
                        MessagesToController.Add(method);
                    }
                }

                if (interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DRenderer", out var unity3DAppAttr))
                {
                    var controllerType = (unity3DAppAttr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
                    RendererTypes.Add((interfaceDeclarationSyntax, controllerType));

                    foreach (var method in interfaceDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>())
                    {
                        MessagesToRenderer.Add(method);
                    }
                }
            }
            
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                    .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DAppBinding",
                        out var blazor3DAppAttr))
                {

                    BlazorBindingTypes.Add(typeDeclarationSyntax);
                }
            }
        }
    }
}

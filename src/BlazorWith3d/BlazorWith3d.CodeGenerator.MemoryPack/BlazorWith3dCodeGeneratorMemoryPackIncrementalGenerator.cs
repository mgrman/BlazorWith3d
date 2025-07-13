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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace BlazorWith3d.CodeGenerator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MakeConstAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule = new DiagnosticDescriptor( "BlazorWith3dCodeGeneratorMemoryPack1", "Test Title", "Test MessageFormat", "Test Category", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Test Description");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
         // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InterfaceDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var ids = (InterfaceDeclarationSyntax)context.Node;

        // make sure the declaration isn't already const:
        if (!ids.HasAttribute("GenerateTSTypesWithMemoryPack"))
        {
            return;
        }

        var appInfo = BlazorWith3dCodeGeneratorMemoryPackIncrementalGenerator.ConvertInterfaceWithAttribute(ids, context.SemanticModel, "GenerateTSTypesWithMemoryPack");

        var nonSequentialStructs = appInfo.AllTypesNonDistinct()
            .Where(o => o.isNonSequentialStruct)
            .Select(o => o.typeName)
            .Distinct()
            .ToList();

        if (nonSequentialStructs.Count == 0)
        {
            return;
        }

        var ruleInstance = new DiagnosticDescriptor("BlazorWith3dCodeGeneratorMemoryPack1", $"Non-sequential structs referenced in [GenerateTSTypesWithMemoryPack] interface", $"Please mark the types: {nonSequentialStructs.JoinStringWithComma()} with [StructLayout(LayoutKind.Sequential)]", "BlazorWith3dCodeGeneratorMemoryPack", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Non-sequential structs in any child type cannot be used with [GenerateTSTypesWithMemoryPack] marked types");
        context.ReportDiagnostic(Diagnostic.Create(ruleInstance, context.Node.GetLocation()));
    }
}

[Generator]
public class BlazorWith3dCodeGeneratorMemoryPackIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TwoWayAppInfo?> memoryPackTsInteropTypesToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is InterfaceDeclarationSyntax ids && ids.HasAttribute("GenerateTSTypesWithMemoryPack"),
                transform: static (ctx, _) => ConvertInterfaceWithAttribute(ctx.Node as InterfaceDeclarationSyntax, ctx.SemanticModel ,"GenerateTSTypesWithMemoryPack"))
            .Where(static m => m is not null);
        var tsOptions=HelloSourceGenerator_MemoryPackTypeScriptForStructs.GetTsOptions(context);
        context.RegisterSourceOutput(memoryPackTsInteropTypesToGenerate.Combine(tsOptions),
            static (context, combined) =>
            {
                var generatedTypeScript = HelloSourceGenerator_MemoryPackTypeScriptForStructs.GenerateTypeScriptClasses(combined.Right, combined.Left);
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
    }

    internal static TwoWayAppInfo? ConvertInterfaceWithAttribute(InterfaceDeclarationSyntax interfaceDeclarationSyntax, SemanticModel model, string attributeName)
    {
        if (!interfaceDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
            .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == attributeName, out var attr))
        {
            return null;
        }

        var eventHandlerType = (attr.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type;
        return InterfaceTypeToTwoWayAppInfo(model, interfaceDeclarationSyntax, eventHandlerType);
    }

    internal static TwoWayAppInfo InterfaceTypeToTwoWayAppInfo(SemanticModel context, InterfaceDeclarationSyntax mainType, TypeSyntax eventHandlerType)
    {
        var methodHandlerTypeInfo = GetTypeInfo(mainType);
        var eventHandlerTypeInfo = GetTypeInfo(eventHandlerType, context.Compilation);

        var eventHandlerTypeSymbol = context.Compilation.GetSemanticModel(eventHandlerType.SyntaxTree).GetTypeInfo(eventHandlerType).Type as INamedTypeSymbol;

        var methods = GetMethodInfosFromDeclaration(context,mainType);

        var events = GetMethodInfos(context, eventHandlerTypeSymbol);
        return new TwoWayAppInfo(methodHandlerTypeInfo, eventHandlerTypeInfo, methods, events);
    }

    internal static List<MethodInfo> GetMethodInfosFromDeclaration(SemanticModel context, InterfaceDeclarationSyntax o)
    {
        var methods = o.Members.OfType<MethodDeclarationSyntax>()
            .Select(m => GetMethodInfo(m, context.Compilation))
            .Where(m => m != null)
            .ToList();
        return methods;
    }
    internal static List<MethodInfo> GetMethodInfos(SemanticModel context, INamedTypeSymbol? o)
    {
        var methods = o.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => GetMethodInfo(m, context.Compilation))
            .Where(m => m != null)
            .ToList();
        return methods;
    }


    internal static TypeInfo GetTypeInfo(TypeDeclarationSyntax typeDeclaration)
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

        var isNonSequentialStruct = typeDeclaration is StructDeclarationSyntax structDeclaration && !IsSequentialStruct(structDeclaration);

        return new TypeInfo(typeName, namespaceName, false, typeDeclaration.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.NormalizeWhitespace().ToFullString().StartsWith("GenerateTypeScript")), null,null,isNonSequentialStruct);
    }

    private static bool IsSequentialStruct(StructDeclarationSyntax structDeclarationSyntax)
    {
        if (!structDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes)
                .TryGet(e => e.Name.NormalizeWhitespace().ToFullString() == "StructLayout", out var attr))
        {
            return false;
        }

        var firstArgument = attr.ArgumentList.Arguments[0].Expression;
        var literalExpressionSyntax = (firstArgument as MemberAccessExpressionSyntax);
        return literalExpressionSyntax.Name.Identifier.Value == "Sequential";
    }

    private static bool IsSequentialStruct(INamedTypeSymbol typeSymbol)
    {
        var attr = typeSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name.StartsWith("StructLayout"));

        if (attr == null)
        {
            return false;
        }

        return (int)attr.ConstructorArguments.FirstOrDefault().Value==0;
    }

    internal static TypeInfo GetTypeInfo(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var typeName = typeSymbol.Name;
        bool isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        if (typeName == nameof(Nullable))
        {
            isNullable = true;
            typeName = typeSymbol.TypeArguments[0].Name;
        }

        if (HelloSourceGenerator_MemoryPackTypeScriptForStructs.TryGetSpecialType(typeSymbol, out var specialType))
        {
            return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, false, specialType, null!,false);
        }


        var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>()
            .Select(o =>
            {
                var typeInfo = GetTypeInfo(o.Type as INamedTypeSymbol, compilation);
                return (typeInfo, o.Name);
            })
            .ToList();

        var isNonSequentialStruct = typeSymbol.IsValueType && !IsSequentialStruct(typeSymbol);

        return new TypeInfo(typeName, typeSymbol.ContainingNamespace.ToDisplayString(), isNullable, typeSymbol.GetAttributes().Any(a=>a.AttributeClass.Name.StartsWith("GenerateTypeScript")), null, properties, isNonSequentialStruct);
    }

    internal static TypeInfo GetTypeInfo(TypeSyntax typeSyntax, Compilation compilation)
    {
        var semanticModel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

        var typeInfo = semanticModel.GetTypeInfo(typeSyntax).Type as INamedTypeSymbol;

        return GetTypeInfo(typeInfo, compilation);
    }

    internal static MethodInfo GetMethodInfo(IMethodSymbol method, Compilation compilation)
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

    internal static MethodInfo GetMethodInfo(MethodDeclarationSyntax method, Compilation compilation)
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

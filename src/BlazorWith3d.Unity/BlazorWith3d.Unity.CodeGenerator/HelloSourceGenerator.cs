using System.Collections.Generic;
using System.Linq;
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
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver)) return;

        if (!receiver.ShouldGenerate) return;
        
    
        //#error generate based on Unity3DAppAttribute, so there can be inverse class for Unity

        foreach (var typeDeclaration in receiver.Blazor3DAppTypes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol;
            var interfaceInfo = GetInterfaceInfo(typeSymbol);
            var text = GenerateClass("TypedUnityApi","Task",interfaceInfo);
            context.AddSource($"{interfaceInfo.typeName}.g.cs", text);
        }
        
        foreach (var unityTypeDeclaration in receiver.Unity3DAppTypes)
        {
            var semanticModel = context.Compilation.GetSemanticModel(unityTypeDeclaration.SyntaxTree);
            var unityTypeSymbol = semanticModel.GetDeclaredSymbol(unityTypeDeclaration) as INamedTypeSymbol;

            var interfaceTypeName = unityTypeSymbol.Name;
            var attrDeclaration = unityTypeDeclaration.AttributeLists.SelectMany(e => e.Attributes).FirstOrDefault(e => e.Name.NormalizeWhitespace().ToFullString() == "Unity3DApp");

            var typeDeclaration = (attrDeclaration.ArgumentList.Arguments[0].Expression as TypeOfExpressionSyntax).Type as IdentifierNameSyntax;



            var referencedTypeSymbol = semanticModel.GetTypeInfo(typeDeclaration).Type as INamedTypeSymbol;


            var interfaceInfo = GetInterfaceInfo(referencedTypeSymbol);


            var typeName = interfaceTypeName.ToString().TrimStart('I');
            var namespaceName = unityTypeSymbol.ContainingNamespace.ToDisplayString();
            var invertedTypeInfo = new InterfaceInfo(unityTypeSymbol.Name, typeName, namespaceName,interfaceInfo.events.Select(e=>(e.eventName, e.messageType,"arg")).ToList(),interfaceInfo.methods.Select(m=>(m.methodName, "On"+m.methodName, m.messageType)
            ).ToList(), interfaceInfo.messageTypes);

     

              var text = GenerateClass("TypedBlazorApi","void",invertedTypeInfo);

            context.AddSource($"{typeName}.g.cs", text);
            
            
            var text2 = GenerateInterface("void",invertedTypeInfo);

            context.AddSource($"{interfaceTypeName}.g.cs", text2);
        }
    }

    private record InterfaceInfo(
        string interfaceTypeName,
        string typeName,
        string namespaceName,
        IReadOnlyList<(string methodName, string messageType, string inputTypeVarName)> methods,
        IReadOnlyList<(string eventName, string eventInvokeMethodName, string messageType)> events,
        IEnumerable<(string msgType, IEnumerable<string> intefaces)> messageTypes)
    {
    }

    private static InterfaceInfo GetInterfaceInfo(INamedTypeSymbol typeSymbol)
    {
            
        var interfaceTypeName = typeSymbol.Name;
        var typeName = interfaceTypeName.TrimStart('I');
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

            
        var methods = typeSymbol.GetMembers()
             .OfType<IMethodSymbol>()
             .Where(o=>!o.IsImplicitlyDeclared)
            .Select(methodDeclaration =>
            {
                // if (methodDeclaration.ParameterList.Parameters.Count != 1 ||
                //     methodDeclaration.ReturnType.ToString() != "Task")
                // {
                //     sb.AppendLine($"#error {methodDeclaration.ToString()} is not supported");
                //     continue;
                // }

                

                var inputType = methodDeclaration.Parameters[0];
                var methodName = methodDeclaration.Name.ToString();
                var messageType = inputType.Type.ToString();
                var inputTypeVarName = inputType.Name.ToString();
                    
                return (methodName, messageType, inputTypeVarName);
            }).ToList();


       var  events = typeSymbol.GetMembers()
             .OfType<IEventSymbol>()
            .Select(eventDeclaration =>
            {
                var eventName = eventDeclaration.Name.ToString();

                var eventInvokeMethodName = $"Invoke_{eventName}_Event";
                var messageType = (eventDeclaration.Type as INamedTypeSymbol).TypeArguments[0].ToString();
                        

                return (eventName, eventInvokeMethodName, messageType);
            }).ToList();
       
       
       var typesFromBlazor = methods.Select(o => (o.messageType, i: "IMessageToUnity"));
       var typesToBlazor = events.Select(o => (o.messageType, i: "IMessageToBlazor"));
       var messageTypes = typesFromBlazor.Concat(typesToBlazor).GroupBy(o => o.messageType).Select(o => (msgType: o.Key, intefaces: o.Select(o1 => o1.i)));
        return new InterfaceInfo(interfaceTypeName, typeName, namespaceName, methods, events,messageTypes);
    }

    private static string GenerateClass( string typedApiName, string sendMessageReturnType, InterfaceInfo info)
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        //sb.AppendLines(namespaces.Select(o => $"using {o};"));
        sb.AppendLine("using MemoryPack;");
        sb.AppendLine("using MemoryPack.Formatters;");
        sb.AppendLine("using BlazorWith3d.Unity;");
        sb.AppendLine("using BlazorWith3d.Unity.Shared;");
        sb.AppendLine($"namespace {info.namespaceName}");
        using (sb.IndentWithCurlyBrackets())
        {
            sb.AppendLine($"public partial class {info.typeName}: {info.interfaceTypeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                    
                sb.AppendLine($"static {info.typeName}()");
                using (sb.IndentWithCurlyBrackets())
                {
                    var messageToBlazorTypes = info.messageTypes.Where(o => o.intefaces.Contains("IMessageToBlazor"))
                        .Select(o => o.msgType)
                        .ToList();
                    var messageToUnityTypes = info.messageTypes.Where(o => o.intefaces.Contains("IMessageToUnity"))
                        .Select(o => o.msgType)
                        .ToList();
                        
                    sb.AppendLine("var messageMethodFormatter = new DynamicUnionFormatter<IMessageToBlazor>(");
                    using (sb.Indent())
                    {
                        for (var i = 0; i < messageToBlazorTypes.Count; i++)
                        {
                            var m = messageToBlazorTypes[i];
                            sb.AppendLine($"({i},typeof({m})){(i != messageToBlazorTypes.Count - 1 ? "," : "")}");
                        }
                    }
                        
                    sb.AppendLine(");");
                        
                    sb.AppendLine("var messageToUnityFormatter = new DynamicUnionFormatter<IMessageToUnity>(");
                    using (sb.Indent())
                    {
                        for (var i = 0; i < messageToUnityTypes.Count; i++)
                        {
                            var m = messageToUnityTypes[i];
                            sb.AppendLine($"({i},typeof({m})){(i != messageToUnityTypes.Count - 1 ? "," : "")}");
                        }
                    }
                        
                    sb.AppendLine(");");
                        
                    sb.AppendLine("MemoryPackFormatterProvider.Register(messageMethodFormatter);");
                    sb.AppendLine("MemoryPackFormatterProvider.Register(messageToUnityFormatter);");
                        
                }

                sb.AppendLine($"private readonly {typedApiName} _typedApi;");
                sb.AppendLine();
                sb.AppendLine($"public {info.typeName}({typedApiName} typedApi)");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine("_typedApi = typedApi;");

                    foreach (var e in info.events)
                    {
                        sb.AppendLine(
                            $"_typedApi.AddMessageProcessCallback<{e.messageType}>(this.{e.eventInvokeMethodName});");
                    }
                }

                sb.AppendLine();

                foreach (var e in info.events)
                {
                    sb.AppendLine($"public event Action<{e.messageType}> {e.eventName};");
                }

                sb.AppendLine();
                foreach (var m in info.methods)
                {
                    sb.AppendLine($"public {sendMessageReturnType} {m.methodName}({m.messageType} {m.inputTypeVarName})");

                    using (sb.IndentWithCurlyBrackets())
                    {
                        sb.AppendLine($"{(sendMessageReturnType=="void"?"":"return ")}_typedApi.SendMessage({m.inputTypeVarName});");
                    }
                }
                sb.AppendLine();

                foreach (var e in info.events)
                {
                    sb.AppendLine(
                        $"private void {e.eventInvokeMethodName}({e.messageType} msg)=> {e.eventName}?.Invoke(msg);");
                }
            }
        }
        
        
        foreach (var msgType in info.messageTypes)
        { 
            var @namespace = msgType.msgType.Substring(0, msgType.msgType.LastIndexOf("."));
            var @type = msgType.msgType.Substring(msgType.msgType.LastIndexOf(".")+1 );
            sb.AppendLine($"namespace {@namespace}");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"public partial class {@type}: {string.Join(", ", msgType.intefaces)} {{}}");
            }
        }
            
        return sb.ToString();
    }

    private static string GenerateInterface( string sendMessageReturnType, InterfaceInfo info)
    {
        var sb = new IndentedStringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        //sb.AppendLines(namespaces.Select(o => $"using {o};"));
        sb.AppendLine("using MemoryPack;");
        sb.AppendLine("using MemoryPack.Formatters;");
        sb.AppendLine("using BlazorWith3d.Unity;");
        sb.AppendLine("using BlazorWith3d.Unity.Shared;");
        sb.AppendLine($"namespace {info.namespaceName}");
        using (sb.IndentWithCurlyBrackets())
        {
            sb.AppendLine($"public partial interface {info.interfaceTypeName}");
            using (sb.IndentWithCurlyBrackets())
            {

                foreach (var e in info.events)
                {
                    sb.AppendLine($"event Action<{e.messageType}> {e.eventName};");
                }

                sb.AppendLine();
                foreach (var m in info.methods)
                {
                    sb.AppendLine($"{sendMessageReturnType} {m.methodName}({m.messageType} {m.inputTypeVarName});");

                }

            }
        }
        
            
        return sb.ToString();
    }


    /// <summary>
    ///     Created on demand before each generation pass
    /// </summary>
    private class SyntaxReceiver : ISyntaxContextReceiver
    {
        public bool ShouldGenerate => Blazor3DAppTypes.Count > 0 || Unity3DAppTypes.Count>0;

        public List<InterfaceDeclarationSyntax > Blazor3DAppTypes { get; } = new();

        public List<InterfaceDeclarationSyntax > Unity3DAppTypes { get; } = new();

        /// <summary>
        ///     Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for
        ///     generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is InterfaceDeclarationSyntax typeDeclarationSyntax)
            {
                if ( typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes).Any(e=>e.Name.NormalizeWhitespace().ToFullString() == "Blazor3DApp"))
                {
                    Blazor3DAppTypes.Add(typeDeclarationSyntax);
                }
                if ( typeDeclarationSyntax.AttributeLists.SelectMany(e => e.Attributes).Any(e=>e.Name.NormalizeWhitespace().ToFullString() == "Unity3DApp"))
                {
                    Unity3DAppTypes.Add(typeDeclarationSyntax);
                }
            }
        }
    }
}

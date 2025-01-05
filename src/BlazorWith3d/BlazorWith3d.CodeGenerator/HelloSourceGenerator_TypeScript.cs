using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.Unity.CodeGenerator;

internal static class HelloSourceGenerator_TypeScript
{
    internal static (string text, string path)? GenerateTypeScriptClass(GeneratorExecutionContext context, AppInfo info)
    {
        if (!context.Compilation.ReferencedAssemblyNames.Any(o => o.Name.Contains("MemoryPack")))
        {
            return null;
        }

        var configOptions = context.AnalyzerConfigOptions;

        // https://github.com/dotnet/project-system/blob/main/docs/design-time-builds.md
        var isDesignTimeBuild =
            configOptions.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", out var designTimeBuild) &&
            designTimeBuild == "true";

        if (isDesignTimeBuild)
        {
            return null;
        }

        string? path;
        if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptOutputDirectory",
                out path))
        {
            path = null;
        }

        if (path == null)
        {
            return null;
        }

        string ext;
        if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptImportExtension",
                out ext!))
        {
            ext = ".js";
        }

        string convertProp;
        if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptConvertPropertyName",
                out convertProp!))
        {
            convertProp = "true";
        }

        if (!configOptions.GlobalOptions.TryGetValue("build_property.MemoryPackGenerator_TypeScriptEnableNullableTypes",
                out var enableNullableTypes))
        {
            enableNullableTypes = "false";
        }

        if (!bool.TryParse(convertProp, out var convert)) 
        {
            convert = true; 
        }

        var typeScriptGenerateOptions = new
        {
            OutputDirectory = path,
            ImportExtension = ext,
            ConvertPropertyName = convert,
            EnableNullableTypes =
                bool.TryParse(enableNullableTypes, out var enabledNullableTypesParsed) &&
                enabledNullableTypesParsed,
            IsDesignTimeBuild = isDesignTimeBuild
        };

        var sb = new IndentedStringBuilder();


        sb.AppendLine($"import {{IBinaryApi}} from \"../IBinaryApi\";");
        sb.AppendLine($"import {{IBinaryApiWithResponse}} from \"../IBinaryApiWithResponse\";");
        sb.AppendLine($"import {{MemoryPackWriter}} from \"./MemoryPackWriter\";");
        sb.AppendLine($"import {{MemoryPackReader}} from \"./MemoryPackReader\";");

        var typesToImport = info.methods.SelectMany(o => o.arguments.Select(a=>a.argType.typeName).ConcatOptional(o.returnType?.typeName, o.returnType!=null))
            .Concat(info.events.SelectMany(o => o.arguments.Select(a => a.argType.typeName).ConcatOptional(o.returnType?.typeName, o.returnType != null)))
            .Distinct();

        foreach (var type in typesToImport)
        {
            sb.AppendLine($"import {{{type}}} from \"./{type}\";");
        }


        sb.AppendLine($"export interface {info.eventHandler.typeName}");
        using (sb.IndentWithCurlyBrackets())
        {
            foreach (var e in info.events)
            {
                sb.AppendLine($"{e.name}({e.arguments.Select(a=>$"{a.argName}: {a.argType.typeName}").JoinStringWithComma()}): Promise<{(e.returnType==null?"void":e.returnType.typeName)}>;");
            }
        }

        sb.AppendLine($"export interface {info.app.typeName}");
        using (sb.IndentWithCurlyBrackets())
        {
            sb.AppendLine($"Set{info.eventHandlerConceptName}({info.eventHandlerConceptName.ToCamelCase()}: {info.eventHandler.typeName}):void;");
            foreach (var m in info.methods)
            {
                sb.AppendLine($"{m.name}({m.arguments.Select(a => $"{a.argName}: {a.argType.typeName}").JoinStringWithComma()}): Promise<{(m.returnType == null ? "void" : m.returnType.typeName)}>;");
            }
        }

        sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}_DirectInterop implements {info.app.typeName}");
        using (sb.IndentWithCurlyBrackets())
        {
            sb.AppendLine($"private _dotnetObject: any;");

            sb.AppendLine($"constructor( dotnetObject: any)");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"this._dotnetObject = dotnetObject;");
            }

            sb.AppendLine($"public SetRenderer(_: IBlocksOnGrid3DRenderer):void");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine("// dummy method as the commands are invoked directly for now");
            }
            foreach (var m in info.methods)
            {
                sb.AppendLine($"public {m.name}({m.arguments.Select(a => $"{a.argName}: {a.argType.typeName}").JoinStringWithComma()}): Promise<{(m.returnType == null ? "void" : m.returnType.typeName)}>");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"return this._dotnetObject.invokeMethodAsync(\"{m.name}\", {m.arguments.Select(a => $"{a.argName}").JoinStringWithComma()});");
                }
            }
        }

        sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}_BinaryApiWithResponse implements {info.app.typeName}");
        using (sb.IndentWithCurlyBrackets())
        {
            sb.AppendLine($"private _binaryApi: IBinaryApiWithResponse;");
            sb.AppendLine($"private _eventHandler: {info.eventHandler.typeName};");
            sb.AppendLine($"private _messageHandler:(bytes: Uint8Array) => Promise<void>;");
            sb.AppendLine($"private _messageWithResponseHandler:(bytes: Uint8Array) => Promise<Uint8Array>;");

            sb.AppendLine($"constructor( binaryApi: IBinaryApiWithResponse)");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"this._binaryApi = binaryApi;");
                sb.AppendLine($"this._messageHandler= (msg)=>this.ProcessMessages(msg);");
                sb.AppendLine($"this._messageWithResponseHandler= (msg)=>this.ProcessMessagesWithResponse(msg);");
            }

            sb.AppendLines(
                $@"public Set{info.eventHandlerConceptName}({info.eventHandlerConceptName.ToCamelCase()}: {info.eventHandler.typeName}):void
 {{
     if(this._binaryApi.mainMessageHandler != null && this._binaryApi.mainMessageHandler != this._messageHandler)
     {{
         return;
     }}
     this._eventHandler={info.eventHandlerConceptName.ToCamelCase()};
     this._binaryApi.mainMessageHandler = this._eventHandler==null?null:this._messageHandler;
     this._binaryApi.mainMessageWithResponseHandler = this._eventHandler==null?null:this._messageWithResponseHandler;
 }}".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));


            foreach (var (m, i) in info.methods.EnumerateWithIndex())
            {
                sb.AppendLine($"public async {m.name}({m.arguments.Select(a => $"{a.argName}: {a.argType.typeName}").JoinStringWithComma()}): Promise<{(m.returnType == null ? "void" : m.returnType.typeName)}>");
                using (sb.IndentWithCurlyBrackets())
                {
                    if (m.returnType == null)
                    {
                        sb.AppendLine($"const writer = MemoryPackWriter.getSharedInstance();");
                        sb.AppendLine($"writer.writeInt8({i});");
                        foreach(var a in m.arguments)
                        {
                            sb.AppendLine($"{a.argType.typeName}.serializeCore(writer, {a.argName})");
                        }
                        sb.AppendLine($"const encodedMessage = writer.toArray();");
                        sb.AppendLine($"await this._binaryApi.sendMessage(encodedMessage);");
                    }
                    else
                    {
                        sb.AppendLine($"const writer = MemoryPackWriter.getSharedInstance();");
                        sb.AppendLine($"writer.writeInt8({i});");
                        foreach (var a in m.arguments)
                        {
                            sb.AppendLine($"{a.argType.typeName}.serializeCore(writer, {a.argName})");
                        }
                        sb.AppendLine($"const encodedMessage = writer.toArray();");
                        sb.AppendLine($"var responseMessage=await  this._binaryApi.sendMessageWithResponse(encodedMessage);");

                        sb.AppendLine($"var dst = new ArrayBuffer(responseMessage.byteLength);");
                        sb.AppendLine($"new Uint8Array(dst).set(responseMessage);");
                        sb.AppendLine($"let response= {m.returnType.typeName}.deserialize(dst);");
                        sb.AppendLine($"return response;");
                    }
                }
            }


            sb.AppendLine($"private async ProcessMessages(msg: Uint8Array): Promise<void>");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"try");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"let buffer = msg.slice(1);");
                    sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
                    sb.AppendLine($"new Uint8Array(dst).set(buffer);");

                    sb.AppendLine("switch (msg[0])");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        foreach (var (e, i) in info.events.EnumerateWithIndex())
                        {
                            if (e.returnType != null)
                            {
                                continue;
                            }

                            sb.AppendLine($"case {i}:");
                            using (sb.IndentWithCurlyBrackets())
                            {
                                sb.AppendLine($"const reader=new MemoryPackReader(dst);");

                                foreach(var a in e.arguments)
                                {
                                    sb.AppendLine($"const {a.argName}: {a.argType.typeName} = {a.argType.typeName}.deserializeCore(reader);");
                                }

                                sb.AppendLine($"await this._eventHandler.{e.name}({e.arguments.Select(a=>a.argName).JoinStringWithComma()});");
                                sb.AppendLine($"break;");
                            }
                        }
                    }
                }
                sb.AppendLine("catch (e)");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine("console.log(e);");
                    sb.AppendLine("throw e;");
                }
            }
            sb.AppendLine($"private async ProcessMessagesWithResponse(msg: Uint8Array): Promise<Uint8Array>");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"try");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"let buffer = msg.slice(1);");
                    sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
                    sb.AppendLine($"new Uint8Array(dst).set(buffer);");

                    sb.AppendLine("switch (msg[0])");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        foreach (var (e, i) in info.events.EnumerateWithIndex())
                        {
                            if (e.returnType == null)
                            {
                                continue;
                            }
                            sb.AppendLine($"case {i}:");
                            using (sb.IndentWithCurlyBrackets())
                            {
                                sb.AppendLine($"const reader=new MemoryPackReader(dst);");

                                foreach (var a in e.arguments)
                                {
                                    sb.AppendLine($"const {a.argName}: {a.argType.typeName} = {a.argType.typeName}.deserializeCore(reader);");
                                }

                                sb.AppendLine($"let response=await this._eventHandler.{e.name}({e.arguments.Select(a => a.argName).JoinStringWithComma()});");


                                sb.AppendLine($"return {e.returnType.typeName}.serialize(response);");
                                sb.AppendLine($"break;");
                            }
                        }
                    }

                    sb.AppendLine("throw new Error('Missing handler');");

                }
                sb.AppendLine("catch (e)");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine(" console.log(e);");
                    sb.AppendLine(" throw e;");
                }





            }
        }

        return (sb.ToString(),Path.Combine(typeScriptGenerateOptions.OutputDirectory, $"{info.app.typeName}.ts"));
    }

}
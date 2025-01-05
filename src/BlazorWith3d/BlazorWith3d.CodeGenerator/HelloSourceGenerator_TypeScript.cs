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
//
//
//         sb.AppendLine($"import {{IBinaryApi}} from \"../IBinaryApi\";");
//         sb.AppendLine($"import {{IBinaryApiWithResponse}} from \"../IBinaryApiWithResponse\";");
//         sb.AppendLine($"import {{MemoryPackWriter}} from \"./MemoryPackWriter\";");
//
//         var typesToImport = info.methods.Select(o => o.typeName)
//             .Concat(info.events.Select(o => o.typeName))
//             .Concat(info.eventsWithResponse.SelectMany(o => new[] { o.request.typeName, o.response.typeName }))
//             .Concat(info.methodsWithResponse.SelectMany(o => new[] { o.request.typeName, o.response.typeName }))
//             .Distinct();
//             
//         foreach (var type in typesToImport)
//         {
//             sb.AppendLine($"import {{{type}}} from \"./{type}\";");
//         }
//         
//         
//         sb.AppendLine($"export interface {info.app.typeName}_EventHandler");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             foreach (var @event in info.events)
//             {
//                 sb.AppendLine($"On{@event.typeName}(msg: {@event.typeName}): Promise<void>;");
//             }
//             foreach (var @event in info.eventsWithResponse)
//             {
//                 sb.AppendLine($"On{@event.request.typeName}(msg: {@event.request.typeName}): Promise<{@event.response.typeName}>;");
//             }
//         }
//         
//         sb.AppendLine($"export interface {info.app.typeName}_MethodInvoker");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             foreach (var m in info.methods)
//             {
//                 sb.AppendLine($"Invoke{m.typeName}(msg: {m.typeName}): Promise<void>");
//             }
//             foreach (var m in info.methodsWithResponse)
//             {
//                 sb.AppendLine($"Invoke{m.request.typeName}(msg: {m.request.typeName}): Promise<{m.response.typeName}>;");
//             }
//         }
//         
//         sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}_MethodInvoker_DirectInterop implements {info.app.typeName}_MethodInvoker");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             sb.AppendLine($"private _dotnetObject: any;");
//
//             sb.AppendLine($"constructor( dotnetObject: any)");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"this._dotnetObject = dotnetObject;");
//             }
//             
//             foreach (var m in info.methods)
//             {
//                 sb.AppendLine($"public Invoke{m.typeName}(msg: {m.typeName}): Promise<void>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"return this._dotnetObject.invokeMethodAsync(\"On{m.typeName}\", msg);");
//                 }
//             }
//             
//             foreach (var m in info.methodsWithResponse)
//             {
//                 sb.AppendLine($"public Invoke{m.request.typeName}(msg: {m.request.typeName}): Promise<{m.response.typeName}>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"return this._dotnetObject.invokeMethodAsync(\"On{m.request.typeName}\", msg);");
//                 }
//             }
//         }
//         
//         sb.AppendLine($"export interface {info.app.typeName} extends {info.app.typeName}_MethodInvoker");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             //sb.AppendLine($"OnMessageError: (bytes: Uint8Array, error: Error) => void;");
//             sb.AppendLine($"SetEventHandler(eventHandler: {info.app.typeName}_EventHandler);");
//         }
//
//         sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}_BinaryApiWithResponse implements {info.app.typeName}");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             sb.AppendLine($"private _binaryApi: IBinaryApiWithResponse;");
//             sb.AppendLine($"private _eventHandler: {info.app.typeName}_EventHandler;");
//             sb.AppendLine($"private _messageHandler:(bytes: Uint8Array) => Promise<void>;");
//             sb.AppendLine($"private _messageWithResponseHandler:(bytes: Uint8Array) => Promise<Uint8Array>;");
//
//             sb.AppendLine($"constructor( binaryApi: IBinaryApiWithResponse)");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"this._binaryApi = binaryApi;");
//                 sb.AppendLine($"this._messageHandler= (msg)=>this.ProcessMessages(msg);");
//                 sb.AppendLine($"this._messageWithResponseHandler= (msg)=>this.ProcessMessagesWithResponse(msg);");
//             }
//
//             sb.AppendLines(
//                 $@"public get IsProcessingMessages():boolean 
// {{
//     return this._binaryApi.mainMessageHandler == this._messageHandler;
// }}
// public SetEventHandler(eventHandler: {info.app.typeName}_EventHandler):void
// {{
//     if(this._binaryApi.mainMessageHandler != null && this._binaryApi.mainMessageHandler != this._messageHandler)
//     {{
//         return;
//     }}
//     this._eventHandler=eventHandler;
//     this._binaryApi.mainMessageHandler = eventHandler==null?null:this._messageHandler;
//     this._binaryApi.mainMessageWithResponseHandler = eventHandler==null?null:this._messageWithResponseHandler;
// }}".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
//
//             
//             foreach (var (method, i) in info.methods.EnumerateWithIndex())
//             {
//                 sb.AppendLine($"public Invoke{method.typeName}(msg: {method.typeName}): Promise<void>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"return this.sendMessage({i}, w => {method.typeName}.serializeCore(w, msg));");
//                 }
//             }
//             
//             foreach (var (method, i) in info.methodsWithResponse.EnumerateWithIndex())
//             {
//                 sb.AppendLine($"public async Invoke{method.request.typeName}(msg: {method.request.typeName}): Promise<{method.response.typeName}>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"var responseMessage=await this.sendMessageWithResponse({i}, w => {method.request.typeName}.serializeCore(w, msg));");
//                     
//                     sb.AppendLine($"var dst = new ArrayBuffer(responseMessage.byteLength);");
//                     sb.AppendLine($"new Uint8Array(dst).set(responseMessage);");
//                     sb.AppendLine($"let response= {method.response.typeName}.deserialize(dst);");
//                     
//                     sb.AppendLine($"return response;");
//                     
//                     
//                 }
//             }
//
//             sb.AppendLines(
//                 @"private async sendMessage(messageId: number, messageSerializeCore: (writer: MemoryPackWriter) => any): Promise<void> 
// {
//     try {
//         const writer = MemoryPackWriter.getSharedInstance();
//         writer.writeInt8(messageId);
//         messageSerializeCore(writer);
//         const encodedMessage = writer.toArray();
//
//         return this._binaryApi.sendMessage(encodedMessage);
//     } catch (ex) 
//     {
//         throw ex;
//     }
// }
//
// private sendMessageWithResponse(messageId: number, messageSerializeCore: (writer: MemoryPackWriter) => any): Promise<Uint8Array> 
// {
//     try {
//         const writer = MemoryPackWriter.getSharedInstance();
//         writer.writeInt8(messageId);
//         messageSerializeCore(writer);
//         const encodedMessage = writer.toArray();
//
//         return this._binaryApi.sendMessageWithResponse(encodedMessage);
//     } catch (ex) 
//     {
//         throw ex;
//     }
// }".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
//             
//             
//
//             sb.AppendLine($"private async ProcessMessages(msg: Uint8Array): Promise<void>");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"try");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"let buffer = msg.slice(1);");
//                     sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
//                     sb.AppendLine($"new Uint8Array(dst).set(buffer);");
//                     
//                     sb.AppendLine("switch (msg[0])");
//                     using (sb.IndentWithCurlyBrackets())
//                     {
//                         foreach (var (e,i) in info.events.EnumerateWithIndex())
//                         {
//                             sb.AppendLine($"case {i}:");
//                             using (sb.IndentWithCurlyBrackets())
//                             {
//                                 sb.AppendLine($"const obj: {e.typeName} = {e.typeName}.deserialize(dst);");
//                                 sb.AppendLine($"await this._eventHandler.On{e.typeName}?.(obj);");
//                                 sb.AppendLine($"break;");
//                             }
//                         }
//                     }
//                 }
//                 sb.AppendLine("catch (e)");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine(" console.log(e);");
//                 }
//             }
//             sb.AppendLine($"private async ProcessMessagesWithResponse(msg: Uint8Array): Promise<Uint8Array>");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"try");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"let buffer = msg.slice(1);");
//                     sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
//                     sb.AppendLine($"new Uint8Array(dst).set(buffer);");
//                     
//                     sb.AppendLine("switch (msg[0])");
//                     using (sb.IndentWithCurlyBrackets())
//                     {
//                         foreach (var (e,i) in info.eventsWithResponse.EnumerateWithIndex())
//                         {
//                             sb.AppendLine($"case {i}:");
//                             using (sb.IndentWithCurlyBrackets())
//                             {
//                                 sb.AppendLine($"const obj: {e.request.typeName} = {e.request.typeName}.deserialize(dst);");
//                                 sb.AppendLine($"let response= await this._eventHandler.On{e.request.typeName}(obj);");
//                                 
//                                 sb.AppendLine($"return {e.response.typeName}.serialize(response);");
//                                 sb.AppendLine($"break;");
//                             }
//                         }
//                     }
//                     
//                     sb.AppendLine("throw new Error('Missng handler');");
//                     
//                 }
//                 sb.AppendLine("catch (e)");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine(" console.log(e);");
//                     sb.AppendLine(" throw e;");
//                 }
//         
//         
//
//         
//         
//             }
//         }
//     
//         
//         
//         sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}_BinaryApi implements {info.app.typeName}");
//         using (sb.IndentWithCurlyBrackets())
//         {
//             sb.AppendLine($"private _binaryApi: IBinaryApi;");
//             sb.AppendLine($"private _requestResponseIdCounter: number= 0;");
//             sb.AppendLine($"private _eventHandler: {info.app.typeName}_EventHandler;");
//             sb.AppendLine($"private _messageHandler:(bytes: Uint8Array) => void;");
//             sb.AppendLine($"private _responseTcs:{{ [id: number] : any }} = {{}};");
//
//             sb.AppendLine($"constructor( binaryApi: IBinaryApi)");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"this._binaryApi = binaryApi;");
//                 sb.AppendLine($"this._messageHandler= (msg)=>this.ProcessMessages(msg);");
//             }
//
//             sb.AppendLines(
//                 $@"public get IsProcessingMessages():boolean 
// {{
//     return this._binaryApi.mainMessageHandler == this._messageHandler;
// }}
// public SetEventHandler(eventHandler: {info.app.typeName}_EventHandler):void
// {{
//     if(this._binaryApi.mainMessageHandler != null && this._binaryApi.mainMessageHandler != this._messageHandler)
//     {{
//         return;
//     }}
//     this._eventHandler=eventHandler;
//     this._binaryApi.mainMessageHandler = eventHandler==null?null:this._messageHandler;
// }}".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
//
//             
//             foreach (var (method, i) in info.methods.EnumerateWithIndex())
//             {
//                 sb.AppendLine($"public Invoke{method.typeName}(msg: {method.typeName}): Promise<void>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"return this.sendMessage({i}, null, w => {method.typeName}.serializeCore(w, msg));");
//                 }
//             }
//             
//             foreach (var (method, i) in info.methodsWithResponse.EnumerateWithIndex())
//             {
//                 sb.AppendLine($"public async Invoke{method.request.typeName}(msg: {method.request.typeName}): Promise<{method.response.typeName}>");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine($"let requestId = this._requestResponseIdCounter++;");
//                     sb.AppendLine($"this._requestResponseIdCounter=this._requestResponseIdCounter>255?0:this._requestResponseIdCounter;");
//
//                     
//                     sb.AppendLines($@"
//     var deferred : any = {{}};
//     deferred.promise = new Promise(resolve => {{
//         deferred.resolve = resolve;
//     }});
//     this._responseTcs[requestId]= deferred;".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
//                     
//                     sb.AppendLine($"await this.sendMessage({i+info.methods.Count}, requestId, w => {method.request.typeName}.serializeCore(w, msg));");
//                     
//                     sb.AppendLine($"let response = await deferred.promise;");
//                     sb.AppendLine($"delete   this._responseTcs[requestId];");
//                     
//                     sb.AppendLine($"return response;");
//                     
//                     
//                 }
//             }
//
//             sb.AppendLines(
//                 @"private async sendMessage(messageId: number, requestId: number|null, messageSerializeCore: (writer: MemoryPackWriter) => any): Promise<void> 
// {
//     try {
//         const writer = MemoryPackWriter.getSharedInstance();
//         writer.writeInt8(messageId);
//         if(requestId!=null){
//             writer.writeInt8(requestId);
//         }
//         messageSerializeCore(writer);
//         const encodedMessage = writer.toArray();
//
//         return this._binaryApi.sendMessage(encodedMessage);
//     } catch (ex) 
//     {
//         throw ex;
//     }
// }".Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));
//             
//             
//
//             sb.AppendLine($"private async ProcessMessages(msg: Uint8Array): Promise<void>");
//             using (sb.IndentWithCurlyBrackets())
//             {
//                 sb.AppendLine($"try");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine("switch (msg[0])");
//                     using (sb.IndentWithCurlyBrackets())
//                     {
//                         foreach (var (e,i) in info.events.EnumerateWithIndex())
//                         {
//                             sb.AppendLine($"case {i}:");
//                             using (sb.IndentWithCurlyBrackets())
//                             {
//                                 sb.AppendLine($"let buffer = msg.slice(1);");
//                                 sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
//                                 sb.AppendLine($"new Uint8Array(dst).set(buffer);");
//                                 sb.AppendLine($"const obj: {e.typeName} = {e.typeName}.deserialize(dst);");
//                                 sb.AppendLine($"await this._eventHandler.On{e.typeName}?.(obj);");
//                                 sb.AppendLine($"break;");
//                             }
//                         }
//                         foreach (var (e,i) in info.eventsWithResponse.EnumerateWithIndex())
//                         {
//                             sb.AppendLine($"case {i+info.events.Count}:");
//                             using (sb.IndentWithCurlyBrackets())
//                             {
//                                 sb.AppendLine($"let requestId = msg[1];");
//                                 sb.AppendLine($"let buffer = msg.slice(2);");
//                                 sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
//                                 sb.AppendLine($"new Uint8Array(dst).set(buffer);");
//                                 sb.AppendLine($"const obj: {e.request.typeName} = {e.request.typeName}.deserialize(dst);");
//                                 sb.AppendLine($"let response= await this._eventHandler.On{e.request.typeName}(obj);");
//                                 sb.AppendLine($"await this.sendMessage({i+info.methods.Count+info.methodsWithResponse.Count}, requestId, w => {e.response.typeName}.serializeCore(w, response));");
//                                 sb.AppendLine($"break;");
//                             }
//                         }
//                         foreach (var (m,i) in info.methodsWithResponse.EnumerateWithIndex( ))
//                         {
//                             sb.AppendLine($"case {i+info.events.Count+info.eventsWithResponse.Count}:");
//                             using (sb.IndentWithCurlyBrackets())
//                             {
//                                 sb.AppendLine($"let requestId = msg[1];");
//                                 sb.AppendLine($"let buffer = msg.slice(2);");
//                                 sb.AppendLine($"var dst = new ArrayBuffer(buffer.byteLength);");
//                                 sb.AppendLine($"new Uint8Array(dst).set(buffer);");
//                                 sb.AppendLine($"const obj: {m.response.typeName} = {m.response.typeName}.deserialize(dst);");
//                                 sb.AppendLine($"this._responseTcs[requestId].resolve(obj);");
//                                 sb.AppendLine($"break;");
//                             }
//                         }
//                     }
//                 }
//                 sb.AppendLine("catch (e)");
//                 using (sb.IndentWithCurlyBrackets())
//                 {
//                     sb.AppendLine(" console.log(e);");
//                 }
//             }
//         }

        return (sb.ToString(),Path.Combine(typeScriptGenerateOptions.OutputDirectory, $"{info.app.typeName}.ts"));
    }

}
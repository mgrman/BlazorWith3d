using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.CodeGenerator;

internal static class HelloSourceGenerator_TypeScript
{
    private record TsOptions
    {
        public string OutputDirectory{ get; init; }
        public string ImportExtension { get; init; }
        public bool   ConvertPropertyName { get; init; }
        public bool  EnableNullableTypes { get; init; }
        
        public Func<TypeInfo,TsTypeInfo>GetTsType { get; init; }
    }

    private record TsTypeInfo(string tsType, string serializationFormat, string deserializationFormat, bool needsImport)
    {
    }
    
    internal static IEnumerable<(string text, string path)> GenerateTypeScriptClass(GeneratorExecutionContext context, TwoWayAppInfo info)
    {
        var options = GetTsOptions(context);

        if (options == null)
        {
            yield break;
        }

        var allTypes = info.methods.Concat(info.events).SelectMany(o => o.arguments).Select(o => o.argType).SelectMany(o => o.FlattenProperties())
            .GroupBy(o => (o.typeName, o.@namespace)).Select(o => o.First())
            .ToList();
        var typesToGenerate = allTypes
            .Where(o => o.specialType == null && !o.isMemoryPackTypescriptGenerated)
            .Where(o=>o.typeName!=  info.eventHandler.typeName && o.typeName!=  info.app.typeName )
            .ToList();

        foreach (var t in typesToGenerate)
        {
            var sb = new IndentedStringBuilder();

            sb.AppendLine($"import {{ MemoryPackWriter }} from \"./MemoryPackWriter\";");
            sb.AppendLine($"import {{ MemoryPackReader }} from \"./MemoryPackReader\";");
            foreach (var propType in t.properties.Where(o => o.type.specialType == null).Select(o=>o.type.typeName).Distinct())
            {
                sb.AppendLine($"import {{{propType}}} from \"./{propType}\";");
            }

            sb.AppendLine();

            sb.AppendLine($"export class {t.typeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                foreach (var prop in t.properties)
                {
                    var propTypeName = prop.type.typeName;
                    if (prop.type.specialType != null)
                    {
                        propTypeName = GetNullableTypesAllowedTypeScriptType(prop.type.specialType.Value, prop.type.isNullable).TypeName;
                    }

                    sb.AppendLine($"{GetTsPropName(prop.name)}: {propTypeName};");
                }

                sb.AppendLine($"static deserializeCore(reader: MemoryPackReader): {t.typeName} | null");
                using (sb.IndentWithCurlyBrackets())
                {

                            sb.AppendLine($"const value = new {t.typeName}();");
                    foreach (var prop in t.properties)
                    {
                        var propTypeName = prop.type.typeName;
                        if (prop.type.specialType != null)
                        {
                            //;
                            var memoryPackInfo = GetNullableTypesAllowedTypeScriptType(prop.type.specialType.Value, prop.type.isNullable);
                            sb.AppendLine($"value.{GetTsPropName(prop.name)} = reader.read{memoryPackInfo.BinaryOperationMethod}();");
                        }
                        else
                        {
                            sb.AppendLine($"value.{GetTsPropName(prop.name)} = {propTypeName}.deserializeCore(reader);");
                            //;
                        }

                    }
                    sb.AppendLine($"return value;");
                }

                sb.AppendLine($"static serializeCore(writer: MemoryPackWriter, value: {t.typeName} | null): void ");
                using (sb.IndentWithCurlyBrackets())
                {

                    foreach (var prop in t.properties)
                    {
                        var propTypeName = prop.type.typeName;
                        if (prop.type.specialType != null)
                        {
                            //;
                            var memoryPackInfo = GetNullableTypesAllowedTypeScriptType(prop.type.specialType.Value, prop.type.isNullable);
                            sb.AppendLine($"writer.write{memoryPackInfo.BinaryOperationMethod}(value.{GetTsPropName(prop.name)});");
                        }
                        else
                        {
                            sb.AppendLine($"{propTypeName}.serializeCore(writer, value.{GetTsPropName(prop.name)});");
                            //;
                        }

                    }
                }
            }

            yield return (sb.ToString(), Path.Combine(options.OutputDirectory, $"{t.typeName}.ts"));
        }


        {
            var sb = new IndentedStringBuilder();
            sb.AppendLine($"import {{IBinaryApi}} from \"../IBinaryApi\";");
            sb.AppendLine($"import {{MemoryPackWriter}} from \"./MemoryPackWriter\";");
            sb.AppendLine($"import {{MemoryPackReader}} from \"./MemoryPackReader\";");

            var typesToImport = info.methods.SelectMany(o => o.arguments.Select(a => a.argType).ConcatOptional(o.returnType, o.returnType != null))
                .Concat(info.events.SelectMany(o => o.arguments.Select(a => a.argType).ConcatOptional(o.returnType, o.returnType != null)))
                .Where(o => o != null)
                .Where(o => options.GetTsType(o).needsImport)
                .Select(o => o.typeName)
                .Except(new []{ info.eventHandler.typeName, info.app.typeName })
                .Distinct();

            foreach (var type in typesToImport)
            {
                sb.AppendLine($"import {{{type}}} from \"./{type}\";");
            }

            string TsType(TypeInfo? type)
            {
                if (type == null)
                {
                    return "void";
                }
                return options.GetTsType(type).tsType;
            }

            sb.AppendLine($"export interface {info.eventHandler.typeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"Quit():void;");
                sb.AppendLine($"Initialize(methodInvoker:{info.app.typeName}):void;");
                foreach (var e in info.events)
                {
                    sb.AppendLine($"{e.name}({e.arguments.Select(a => $"{a.argName}: {TsType(a.argType)}").JoinStringWithComma()}): Promise<{TsType(e.returnType)}>;");
                }
            }

            sb.AppendLine($"export interface {info.app.typeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                foreach (var m in info.methods)
                {
                    sb.AppendLine($"{m.name}({m.arguments.Select(a => $"{a.argName}: {TsType(a.argType)}").JoinStringWithComma()}): Promise<{TsType(m.returnType)}>;");
                }
            }

            sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}OverDirectInterop implements {info.app.typeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"private _dotnetObject: any;");

                sb.AppendLine($"constructor( dotnetObject: any)");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"this._dotnetObject = dotnetObject;");
                }

                foreach (var m in info.methods)
                {
                    sb.AppendLine($"public {m.name}({m.arguments.Select(a => $"{(a.argType.typeName == info.eventHandler.typeName?"_":a.argName)}: {TsType(a.argType)}").JoinStringWithComma()}): Promise<{TsType(m.returnType)}>");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        sb.AppendLine($"return this._dotnetObject.invokeMethodAsync(\"{m.name}\", {m.arguments.Where(a=>a.argType.typeName != info.eventHandler.typeName).Select(a => $"{a.argName}").JoinStringWithComma()});");
                    }
                }
            }

            sb.AppendLine($"export class {info.app.TypeNameWithoutIPrefix}OverBinaryApi implements {info.app.typeName}");
            using (sb.IndentWithCurlyBrackets())
            {
                sb.AppendLine($"private _binaryApi: IBinaryApi;");
                sb.AppendLine($"private _eventHandler: {info.eventHandler.typeName};");
                sb.AppendLine($"private _messageHandler:(bytes: Uint8Array) => Promise<void>;");
                sb.AppendLine($"private _messageWithResponseHandler:(bytes: Uint8Array) => Promise<Uint8Array>;");

                sb.AppendLine($"constructor( binaryApi: IBinaryApi, eventHandler: {info.eventHandler.typeName})");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"this._binaryApi = binaryApi;");
                    sb.AppendLine($"this._messageHandler= (msg)=>this.ProcessMessages(msg);");
                    sb.AppendLine($"this._messageWithResponseHandler= (msg)=>this.ProcessMessagesWithResponse(msg);");

                    sb.AppendLine($"this._eventHandler=eventHandler;");
                    sb.AppendLine($"this._binaryApi.mainMessageHandler = this._eventHandler==null?null:this._messageHandler;");
                    sb.AppendLine($"this._binaryApi.mainMessageWithResponseHandler = this._eventHandler==null?null:this._messageWithResponseHandler;");
                }

                foreach (var (m, i) in info.methods.EnumerateWithIndex())
                {
                    sb.AppendLine($"public async {m.name}({m.arguments.Select(a => $"{(a.argType.typeName == info.eventHandler.typeName?"_":a.argName)}: {TsType(a.argType)}").JoinStringWithComma()}): Promise<{TsType(m.returnType)}>");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        if (m.returnType == null)
                        {
                            sb.AppendLine($"const writer = MemoryPackWriter.getSharedInstance();");
                            sb.AppendLine($"writer.writeInt8({i});");
                            foreach (var a in m.arguments)
                            {
                                if (a.argType.typeName == info.eventHandler.typeName)
                                {
                                    continue;
                                }
                                sb.AppendLine(string.Format(options.GetTsType(a.argType).serializationFormat, "writer", a.argName));

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
                                sb.AppendLine(string.Format(options.GetTsType(a.argType).serializationFormat, "writer", a.argName));
                            }
                            sb.AppendLine($"const encodedMessage = writer.toArray();");
                            sb.AppendLine($"var responseMessage=await  this._binaryApi.sendMessageWithResponse(encodedMessage);");

                            sb.AppendLine($"const reader=new MemoryPackReader(responseMessage.buffer);");
                            sb.AppendLine($"const readerAny:any= reader;");
                            sb.AppendLine($"readerAny.offset=responseMessage.byteOffset;");


                            sb.AppendLine($"let response: {TsType(m.returnType)} = {string.Format(options.GetTsType(m.returnType).deserializationFormat, "reader")};");

                            sb.AppendLine($"return response;");
                        }
                    }
                }


                sb.AppendLine($"private async ProcessMessages(_msg: Uint8Array): Promise<void>");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"try");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        sb.AppendLine("switch (_msg[_msg.length-1])");
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
                                    sb.AppendLine($"const reader=new MemoryPackReader(_msg.buffer);");
                                    sb.AppendLine($"const readerAny:any= reader;");
                                    sb.AppendLine($"readerAny.offset=_msg.byteOffset;");

                                    foreach (var a in e.arguments)
                                    {
                                        sb.AppendLine($"const {a.argName}: {TsType(a.argType)} = {string.Format(options.GetTsType(a.argType).deserializationFormat, "reader")};");
                                    }

                                    sb.AppendLine($"await this._eventHandler.{e.name}({e.arguments.Select(a => a.argName).JoinStringWithComma()});");
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
                sb.AppendLine($"private async ProcessMessagesWithResponse(_msg: Uint8Array): Promise<Uint8Array>");
                using (sb.IndentWithCurlyBrackets())
                {
                    sb.AppendLine($"try");
                    using (sb.IndentWithCurlyBrackets())
                    {
                        sb.AppendLine("switch (_msg[_msg.length-1])");
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
                                    sb.AppendLine($"const reader=new MemoryPackReader(_msg.buffer);");
                                    sb.AppendLine($"const readerAny:any= reader;");
                                    sb.AppendLine($"readerAny.offset=_msg.byteOffset;");


                                    foreach (var a in e.arguments)
                                    {
                                        sb.AppendLine($"const {a.argName}: {TsType(a.argType)} = {string.Format(options.GetTsType(a.argType).deserializationFormat, "reader")};");
                                    }

                                    sb.AppendLine($"let response=await this._eventHandler.{e.name}({e.arguments.Select(a => a.argName).JoinStringWithComma()});");

                                    sb.AppendLine($"const writer = MemoryPackWriter.getSharedInstance();");
                                    {
                                        sb.AppendLine(string.Format(options.GetTsType(e.returnType).serializationFormat, "writer", "response"));
                                    }
                                    sb.AppendLine($"return writer.toArray();");

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

            yield return (sb.ToString(), Path.Combine(options.OutputDirectory, $"{info.app.typeName}.ts"));
        }
    }

    private static string GetTsPropName(string name)
    {
        return name.Substring(0, 1).ToLower() + name.Substring(1);
    }

    private static TsOptions? GetTsOptions(GeneratorExecutionContext context)
    {
        var configOptions = context.AnalyzerConfigOptions;
        if (!context.Compilation.ReferencedAssemblyNames.Any(o => o.Name.Contains("MemoryPack")))
        {
            return null;
        }

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

        var allowNullableTypes = bool.TryParse(enableNullableTypes, out var enabledNullableTypesParsed) &&
                                 enabledNullableTypesParsed
            ;
        return new TsOptions()
        {
            OutputDirectory = path,
            ImportExtension = ext,
            ConvertPropertyName = convert,
            EnableNullableTypes =allowNullableTypes,
                
            GetTsType= o =>
            {

                if (Enum.TryParse($"{o.@namespace}_{o.typeNameOrig}", true, out MemoryPackSpecialType specialTypeParsed))
                {
                    var aaa=ConvertFromSpecialType(specialTypeParsed, o.isNullable, allowNullableTypes);
                    return new TsTypeInfo(aaa.TypeName, $"{{0}}.write{(o.isNullable?"Nullable":"")}{aaa.BinaryOperationMethod}({{1}})",$"{{0}}.read{(o.isNullable?"Nullable":"")}{aaa.BinaryOperationMethod}()", false);
                }

                return new TsTypeInfo(o.typeName, $"{o.typeName}.serializeCore({{0}}, {{1}})",
                    $"{o.typeName}.deserializeCore({{0}});", true);

            }
        };
    }
    
    
    #region MemoryPack special types
    internal class TypeScriptTypeCore
    {
        public string TypeName { get; set; } = default!;
        public string DefaultValue { get; set; } = default!;
        public string BinaryOperationMethod { get; set; } = default!;
    }

    internal static bool TryGetSpecialType(TypeInfo o, out MemoryPackSpecialType specialType)
    {
        return Enum.TryParse($"{o.@namespace}_{o.typeNameOrig}", true, out specialType);
    }

    internal static bool TryGetSpecialType(INamedTypeSymbol o, out MemoryPackSpecialType specialType)
    {
        var name = o.Name;
        if(o.NullableAnnotation== NullableAnnotation.Annotated && o.IsValueType)
        {
            name = o.TypeArguments[0].Name;
        }

        return Enum.TryParse($"{o.ContainingNamespace.ToDisplayString()}_{name}", true, out specialType);
    }

    internal enum MemoryPackSpecialType
    {
        System_Boolean, 
        System_String, 
        System_SByte, 
        System_Byte,
        System_Int16,
        System_UInt16,
        System_Int32,
        System_UInt32, 
        System_Single,
        System_Double,
        System_Int64, 
        System_UInt64,
        System_DateTime

    }

   static  TypeScriptTypeCore? ConvertFromSpecialType(
        MemoryPackSpecialType specialType,
        bool isNullable,
        bool allowNullableTypes)
    {
        // NOTE The function to get the TypeScript type was duplicated in order
        //      to keep the old behavior of the code generator.
        return allowNullableTypes
            ? GetNullableTypesAllowedTypeScriptType(specialType, isNullable)
            : GetNonNullableTypesAllowedTypeScriptType(specialType);
    }

    static TypeScriptTypeCore? GetNonNullableTypesAllowedTypeScriptType(MemoryPackSpecialType specialType)
    {
        string typeName;
        string binaryOperationMethod;
        string defaultValue;

        switch (specialType)
        {
            case MemoryPackSpecialType.System_Boolean:
                typeName = "boolean";
                binaryOperationMethod = "Boolean";
                defaultValue = "false";

                break;

            case MemoryPackSpecialType.System_String:
                typeName = "string | null";
                binaryOperationMethod = "String";
                defaultValue = "null";

                break;

            case MemoryPackSpecialType.System_SByte:
                typeName = "number";
                binaryOperationMethod = "Int8";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Byte:
                typeName = "number";
                binaryOperationMethod = "Uint8";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Int16:
                typeName = "number";
                binaryOperationMethod = "Int16";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_UInt16:
                typeName = "number";
                binaryOperationMethod = "Uint16";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Int32:
                typeName = "number";
                binaryOperationMethod = "Int32";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_UInt32:
                typeName = "number";
                binaryOperationMethod = "Uint32";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Single:
                typeName = "number";
                binaryOperationMethod = "Float32";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Double:
                typeName = "number";
                binaryOperationMethod = "Float64";
                defaultValue = "0";

                break;

            case MemoryPackSpecialType.System_Int64:
                typeName = "bigint";
                binaryOperationMethod = "Int64";
                defaultValue = "0n";

                break;

            case MemoryPackSpecialType.System_UInt64:
                typeName = "bigint";
                binaryOperationMethod = "Uint64";
                defaultValue = "0n";

                break;

            case MemoryPackSpecialType.System_DateTime:
                typeName = "Date";
                binaryOperationMethod = "Date";
                defaultValue = "new Date(0)";

                break;

            default:
                return null;
        }

        return new TypeScriptTypeCore
        {
            TypeName = typeName,
            DefaultValue = defaultValue,
            BinaryOperationMethod = binaryOperationMethod
        };
    }

    static TypeScriptTypeCore? GetNullableTypesAllowedTypeScriptType(MemoryPackSpecialType specialType, bool isNullable)
    {
        string typeName;
        string binaryOperationMethod;
        string defaultValue;

        string GetTypeName(string typeName) =>
            isNullable ? $"{typeName} | null" : typeName;

        string GetDefaultValue(string defaultValue) =>
            isNullable ? "null" : defaultValue;

        switch (specialType)
        {
            case MemoryPackSpecialType.System_Boolean:
                typeName = GetTypeName("boolean");
                binaryOperationMethod = "Boolean";
                defaultValue = GetDefaultValue("false");

                break;

            case MemoryPackSpecialType.System_String:
                typeName = GetTypeName("string");
                binaryOperationMethod = "String";
                defaultValue = GetDefaultValue(@"""""");

                break;

            case MemoryPackSpecialType.System_SByte:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int8";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Byte:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint8";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Int16:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int16";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_UInt16:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint16";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Int32:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Int32";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_UInt32:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Uint32";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Single:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Float32";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Double:
                typeName = GetTypeName("number");
                binaryOperationMethod = "Float64";
                defaultValue = GetDefaultValue("0");

                break;

            case MemoryPackSpecialType.System_Int64:
                typeName = GetTypeName("bigint");
                binaryOperationMethod = "Int64";
                defaultValue = GetDefaultValue("0n");

                break;

            case MemoryPackSpecialType.System_UInt64:
                typeName = GetTypeName("bigint");
                binaryOperationMethod = "Uint64";
                defaultValue = GetDefaultValue("0n");

                break;

            case MemoryPackSpecialType.System_DateTime:
                typeName = GetTypeName("Date");
                binaryOperationMethod = "Date";
                defaultValue = GetDefaultValue("new Date(0)");

                break;

            default:
                return null;
        }

        return new TypeScriptTypeCore
        {
            TypeName = typeName,
            DefaultValue = defaultValue,
            BinaryOperationMethod = binaryOperationMethod
        };
    }
    #endregion
    
}
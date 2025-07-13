using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace BlazorWith3d.CodeGenerator;

internal static class HelloSourceGenerator_TypeScript
{
    internal static IEnumerable<(string text, string path)> GenerateTypeScriptClasses(string? localPath, TwoWayAppInfo info)
    {
        var typesToGenerate = info.AllTypesNonDistinct()
            .GroupBy(o => (o.typeName, o.@namespace)).Select(o => o.First())
            .Where(o=>o.typeName!=  info.eventHandler.typeName && o.typeName!=  info.app.typeName && o.specialType==null )
            .ToList();

        foreach (var t in typesToGenerate)
        {
            var sb = new IndentedStringBuilder();

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
                    sb.AppendLine($"{GetTsPropName(prop.name)}: {TsType(prop.type)};");
                }
                
            }

            yield return (sb.ToString(), Path.Combine(localPath, $"{t.typeName}.ts"));
        }


        {
            var sb = new IndentedStringBuilder();

            var typesToImport = info.methods
                .SelectMany(o => o.arguments.Select(a => a.argType).ConcatOptional(o.returnType, o.returnType != null))
                .Concat(info.events
                    .SelectMany(o => o.arguments.Select(a => a.argType).ConcatOptional(o.returnType, o.returnType != null)))
                .Where(o => o != null && o.specialType==null)
                .Select(o => o.typeName)
                .Except(new []{ info.eventHandler.typeName, info.app.typeName })
                .Distinct();

            foreach (var type in typesToImport)
            {
                sb.AppendLine($"import {{{type}}} from \"./{type}\";");
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
            
            
            yield return (sb.ToString(), Path.Combine(localPath, $"{info.app.typeName}.ts"));
        }
        
    }

    private static string TsType(TypeInfo? typeInfo)
    {
        if (typeInfo == null)
        {
            return "any";
        }

        var propTypeName = typeInfo.typeName;
        if (typeInfo.specialType != null)
        {
            propTypeName = typeInfo.specialType.tsType;
        }

        return propTypeName;
    }

    private static string GetTsPropName(string name)
    {
        return name.Substring(0, 1).ToLower() + name.Substring(1);
    }
}
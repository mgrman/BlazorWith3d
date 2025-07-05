using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorWith3d.CodeGenerator;
public static class SourceGenerationHelper
{
    public const string Attribute = @"
using System;

namespace BlazorWith3d.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class GenerateDirectBindingAttribute : Attribute
    {
        public GenerateDirectBindingAttribute(Type methodHandlerType,Type eventHandlerType)
        {

        }
    }
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    internal sealed class GenerateBinaryApiAttribute : Attribute
    {
        public GenerateBinaryApiAttribute(Type eventHandlerType)
        {

        }
    }
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    internal sealed class GenerateTSTypesWithMemoryPackAttribute : Attribute
    {
        public GenerateTSTypesWithMemoryPackAttribute(Type eventHandlerType)
        {

        }
    }
}";
}
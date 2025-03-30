using System;

namespace BlazorWith3d.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GenerateDirectBindingAttribute : Attribute
    {
        public GenerateDirectBindingAttribute(Type methodHandlerType,Type eventHandlerType)
        {

        }
    }
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class GenerateBinaryApiAttribute : Attribute
    {
        public GenerateBinaryApiAttribute(Type eventHandlerType)
        {

        }
    }
}
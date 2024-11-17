using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity.Shared
{
    public interface I3DAppObjectApi
    {
        event Action<(object msg, Type msgType)>? OnMessageObject;
        ValueTask InvokeMessageObject(object msg, Type msgTypeOverride = null);
        
        IEnumerable<Type> SupportedOnMessageTypes { get; }
        IEnumerable<Type> SupportedInvokeMessageTypes { get; }
    }
}
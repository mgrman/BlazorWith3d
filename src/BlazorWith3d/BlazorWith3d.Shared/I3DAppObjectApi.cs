using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface I3DAppObjectApi
    {
        bool IsProcessingMessages { get; }
        void StartProcessingMessages();
        void StopProcessingMessages();

        event Action<byte[], Exception> OnMessageError;
        event Action<(object msg, Type msgType)>? OnMessageObject;
        ValueTask InvokeMessageObject(object msg, Type? msgTypeOverride = null);

        IEnumerable<Type> SupportedOnMessageTypes { get; }
        IEnumerable<Type> SupportedInvokeMessageTypes { get; }
    }
}
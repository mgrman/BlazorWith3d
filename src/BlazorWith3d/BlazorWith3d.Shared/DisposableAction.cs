using System;

namespace BlazorWith3d.Shared;

public record DisposableAction(Action onDispose):IDisposable
{
    public void Dispose()
    {
        onDispose();
    }
}
#if COMMON_DOTNET
using System;
using System.Threading.Tasks;

using BlazorWith3d.Shared;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public interface I3DAppController
    {
        ValueTask<IDisposable> InitializeRenderer(IBlocksOnGrid3DApp rendererApi, Func<ValueTask>? afterEventHandlerIsSet);
    }
}
#endif
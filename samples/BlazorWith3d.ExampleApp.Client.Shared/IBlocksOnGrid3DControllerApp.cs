#if COMMON_DOTNET
using System;
using System.Threading.Tasks;

using BlazorWith3d.Shared;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public interface IBlocksOnGrid3DControllerApp
    {
        ValueTask<IBlocksOnGrid3DController> AddRenderer(IBlocksOnGrid3DRenderer rendererApi);
        ValueTask RemoveRenderer(IBlocksOnGrid3DRenderer rendererApi);
    }
}
#endif
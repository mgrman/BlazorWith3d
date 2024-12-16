#if COMMON_DOTNET
using BlazorWith3d.Shared;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public interface I3DAppController
    {
        void InitializeRenderer(IBlocksOnGrid3DApp? appApi);
    }
}
#endif
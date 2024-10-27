#if COMMON_DOTNET
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity
{
    public interface I3DAppController
    {
        void InitializeRenderer(IBlocksOnGrid3DApp appApi);
    }
}
#endif
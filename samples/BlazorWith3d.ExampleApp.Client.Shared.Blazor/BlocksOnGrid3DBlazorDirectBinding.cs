
using BlazorWith3d.JsApp;
using BlazorWith3d.Shared;

namespace BlazorWith3d.ExampleApp.Client.Shared.Blazor;

[Blazor3DAppBinding(typeof(IBlocksOnGrid3DController),typeof(IBlocksOnGrid3DRenderer))]
public partial class BlocksOnGrid3DBlazorDirectBinding
{
    
}
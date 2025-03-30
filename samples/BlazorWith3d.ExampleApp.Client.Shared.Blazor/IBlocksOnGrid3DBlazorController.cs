using System;
using System.Threading.Tasks;

using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public interface IBlocksOnGrid3DBlazorController : IBlocksOnGrid3DController
    {
        ValueTask AddRenderer(IBlocksOnGrid3DBlazorRenderer rendererApi);
        ValueTask RemoveRenderer(IBlocksOnGrid3DRenderer rendererApi);
    }
    
    public interface IBlocksOnGrid3DBlazorRenderer 
    {
        IBlocksOnGrid3DRenderer RendererApi { get; }
        ElementReference RendererContainer { get; }
        
    }

    public record BlocksOnGrid3DBlazorRenderer(IBlocksOnGrid3DRenderer RendererApi, ElementReference RendererContainer)
        : IBlocksOnGrid3DBlazorRenderer;
}
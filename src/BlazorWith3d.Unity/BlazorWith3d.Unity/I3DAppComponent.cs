using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity;

public interface I3DAppComponent
{
    void InitializeRenderer(IUnityApi unityApi);
}
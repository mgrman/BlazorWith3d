using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Unity;


// TODO double check catching of exceptions as they happen in "native" code and do not always propagate properly

// TODO consider some generic reactive dictionary or patch requests on object support
// e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side
// kinda like flux https://facebookarchive.github.io/flux/docs/in-depth-overview/

// TODO with memory pack the Unity build got slower, investigate why!
// e.g. might be worth having a define or something to switch the serialization libraries (have one for faster compile time and one for faster runtime)

// TODO add benchmarking of speed, e.g. a dedicated project with just benchamarks (method only, string message, binary string message, memory pack message)

// TODO inverse generation, define interface and then generate implementation and messages for it. As the highlevel interface will be the one for interop with other 3d libs (though the messages could be as well, but we should start at interface)

public class TypedMessageUnityComponent : BaseUnityComponent
{
    [Inject] 
    private ILogger<TypedMessageUnityComponent> Logger { get; set; } = null!;

    public BlazorTypedUnityApi TypedUnityApi { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        TypedUnityApi = new BlazorTypedUnityApi(this, Logger); ;
        base.OnInitialized();
    }
}
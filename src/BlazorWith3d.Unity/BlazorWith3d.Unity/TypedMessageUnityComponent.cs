using System.Text;
using BlazorWith3d.Unity.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorWith3d.Unity;

// remark: using Newtonsoft.Json only as Unity needs fields for serialization, which are not supported in System.Text.Json

// TODO add catching of exceptions as they happen in "native" code and do not always propagate properly

// TODO consider some generic reactive dictionary or patch requests on object support
// e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side


// TODO more usefull, consider code generation, where interface is defined and marked
// this generates Blazor code for it, which just needs to be instantiated with Unity Instance
// and generates Unity code which references native message passing methods. 
// +arguments are auto wrapped into a message class


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
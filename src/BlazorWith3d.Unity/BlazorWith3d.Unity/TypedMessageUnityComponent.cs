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

//TODO consider using structs with attributes for response with better binary serialization (e.g. https://neuecc.medium.com/how-to-make-the-fastest-net-serializer-with-net-7-c-11-case-of-memorypack-ad28c0366516 )

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
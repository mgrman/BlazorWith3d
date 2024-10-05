using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorWith3d.Unity;

public class BlazorTypedUnityApi:TypedUnityApi
{
    private readonly ILogger _logger;

    public BlazorTypedUnityApi(IUnityApi unityApi, ILogger logger) : base(unityApi)
    {
        _logger = logger;
    }

    protected override void LogError(Exception exception, string msg)
    {
        _logger.LogError(exception, msg);
    }

    protected override void LogError(string msg)
    {
        _logger.LogError( msg);
    }

    protected override void LogWarning(string msg)
    {
        _logger.LogWarning( msg);
    }

    protected override void Log(string msg)
    {
        _logger.LogInformation( msg);
    }

    protected override string SerializeObject<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    protected override T DeserializeObject<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
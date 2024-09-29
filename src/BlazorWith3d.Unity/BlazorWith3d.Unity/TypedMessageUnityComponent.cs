using BlazorWith3d.Unity.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorWith3d.Unity;

// remark: using Newtonsoft.Json only as Unity needs fields for serialization, which are not supported in System.Text.Json

public class TypedMessageUnityComponent:BaseUnityComponent
{
    [Inject] 
    private ILogger<TypedMessageUnityComponent> Logger { get; set; } = null!;
    
    private IDictionary<Type, Func<string, ValueTask<string>>> _handlers = new Dictionary<Type, Func<string, ValueTask<string>>>();

    public async ValueTask<TResponse> SendMessage<TMessage, TResponse>(TMessage message) where TMessage : IMessageToUnity<TMessage, TResponse>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();
        MessageTypeCache.AddTypeToCache<TResponse>();

        var messageString = JsonConvert.SerializeObject(message);
        var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

        try
        {
            var responseString = await SendMessageToUnity(encodedMessage);
            var decoded = MessageTypeCache.DecodeMessageJson(responseString);

            if (decoded == null)
            {
                throw new InvalidOperationException(
                    $"Response for {typeof(TMessage).Name} was not encoded correctly! {responseString}");
            }

            if (decoded.Value.type != typeof(TResponse))
            {
                throw new InvalidOperationException(
                    $"Unexpected response type! Expected {typeof(TResponse).Name} and received '{decoded.Value.typeName}'!");
            }

            var response = JsonConvert.DeserializeObject<TResponse>(decoded.Value.objectJson);
            if (response == null)
            {
                throw new InvalidOperationException(
                    $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
            }

            return response;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void AddMessageProcessCallback<TMessage, TResponse>(Func<TMessage, ValueTask<TResponse>> messageHandler) where TMessage : IMessageFromUnity<TMessage, TResponse>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();
        MessageTypeCache.AddTypeToCache<TResponse>();
        _handlers[typeof(TMessage)] = async objectJson =>
        {
            var messageObject = JsonConvert.DeserializeObject<TMessage>(objectJson);
            if (messageObject == null)
            {
                throw new InvalidOperationException($"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
            }

            var responseObject = await messageHandler(messageObject);

            var response = JsonConvert.SerializeObject(responseObject);
            return MessageTypeCache.EncodeMessageJson<TResponse>(response);
        };
    }
    protected override async ValueTask<string> OnMessageReceived(string msg)
    {
        var decoded = MessageTypeCache.DecodeMessageJson(msg);

        if (decoded== null)
        {
            if (TryHandleKnownMessages(msg, out var knownMessageResponse))
            {
                return knownMessageResponse;
            }
            throw new InvalidOperationException($"Non-encoded message received! {msg}");
        }
        
        if (decoded.Value.type == null)
        {
            Logger.LogWarning($"Unknown message type {decoded.Value.typeName}");
            return string.Empty;
        }

        if (!_handlers.TryGetValue(decoded.Value.type, out var handler))
        {
            Logger.LogWarning($"Missing handler for message {decoded.Value.typeName}");
            return string.Empty;
        }


        var response = await handler(decoded.Value.objectJson);
        return response;
    }

    private bool TryHandleKnownMessages(string msg, out string response)
    {
        switch (msg)
        {
            case "UNITY_INITIALIZED":
                Logger.LogWarning($"UNITY_INITIALIZED received");
                response = "ACK";
                return true;
            default:
                response = string.Empty;
                return false;
        }
    }
}
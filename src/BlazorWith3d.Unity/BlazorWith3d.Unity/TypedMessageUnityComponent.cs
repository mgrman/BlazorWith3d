using BlazorWith3d.Unity.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BlazorWith3d.Unity;

// remark: using Newtonsoft.Json only as Unity needs fields for serialization, which are not supported in System.Text.Json

// TODO adapt API to use binary arrays, not strings.
// to prevent utf8-ut16 and back conversions, as unity js interop uses methods indicating utf8 is used for interop, even though JS and C# both use utf16, maybe emscripten uses utf8?
// as then other binary formatters could also be used


// TODO add catching of exceptions as they happen in "native" code and do not always propagate properly

// TODO consider some generic reactive dictionary or patch requests on object support
// e.g. that both sides can instantiate kind of reactive dictionry and through generic messages they both can be kept automatically in sync, with changes always propagating to the other side

// TODO more usefull, consider code generation, where interface is defined and marked
// this generates Blazor code for it, which just needs to be instantiated with Unity Instance
// and generates Unity code which references native message passing methods. 
// +arguments are auto wrapped into a message class


public class TypedMessageUnityComponent:BaseUnityComponent
{
    [Inject] 
    private ILogger<TypedMessageUnityComponent> Logger { get; set; } = null!;
    
    private IDictionary<Type, Func<string, ValueTask<string>>> _handlersWithResponse = new Dictionary<Type, Func<string, ValueTask<string>>>();
    
    private IDictionary<Type, Action<string>> _handlers = new Dictionary<Type,  Action<string>>();

    public async ValueTask SendMessage<TMessage>(TMessage message) where TMessage : IMessageToUnity<TMessage>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();

        var messageString = JsonConvert.SerializeObject(message);
        var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

        try
        {
            await SendMessageToUnityAsync(encodedMessage);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async ValueTask<TResponse> SendMessageWithResponse<TMessage, TResponse>(TMessage message) where TMessage : IMessageToUnity<TMessage, TResponse>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();
        MessageTypeCache.AddTypeToCache<TResponse>();

        var messageString = JsonConvert.SerializeObject(message);
        var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

        try
        {
            var responseString = await SendMessageWithResponseToUnityAsync(encodedMessage);
            
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

    public void AddMessageWithResponseProcessCallback<TMessage, TResponse>(Func<TMessage, ValueTask<TResponse>> messageHandler) where TMessage : IMessageFromUnity<TMessage, TResponse>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();
        MessageTypeCache.AddTypeToCache<TResponse>();
        _handlersWithResponse[typeof(TMessage)] = async objectJson =>
        {
            var messageObject = JsonConvert.DeserializeObject<TMessage>(objectJson);
            if (messageObject == null)
            {
                throw new InvalidOperationException($"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
            }

            var responseObject = await messageHandler(messageObject);

            var response = JsonConvert.SerializeObject(responseObject);
            return MessageTypeCache.EncodeMessageJson<TResponse>(response);
        };
    }

    public void AddMessageProcessCallback<TMessage>(Func<TMessage, ValueTask> messageHandler) where TMessage : IMessageFromUnity<TMessage>
    {
        MessageTypeCache.AddTypeToCache<TMessage>();
        _handlers[typeof(TMessage)] = async objectJson =>
        {
            var messageObject = JsonConvert.DeserializeObject<TMessage>(objectJson);
            if (messageObject == null)
            {
                throw new InvalidOperationException($"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
            }

            await messageHandler(messageObject);
        };
    }
    
    protected override async ValueTask<string> OnMessageReceivedWithResponseAsync(string msg)
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

        if (!_handlersWithResponse.TryGetValue(decoded.Value.type, out var handlerWithResponse))
        {
            Logger.LogWarning($"Missing handler for message {decoded.Value.typeName}");
            return string.Empty;
        }

        var response = await handlerWithResponse(decoded.Value.objectJson);
        return response;
    }
    
    protected override void OnMessageReceived(string msg)
    {
        var decoded = MessageTypeCache.DecodeMessageJson(msg);

        if (decoded== null)
        {
            if (TryHandleKnownMessages(msg, out var knownMessageResponse))
            {
                return;
            }
            throw new InvalidOperationException($"Non-encoded message received! {msg}");
        }
        
        if (decoded.Value.type == null)
        {
            Logger.LogWarning($"Unknown message type {decoded.Value.typeName}");
            return;
        }

        if (!_handlers.TryGetValue(decoded.Value.type, out var handler))
        {
            Logger.LogWarning($"Missing handler for message {decoded.Value.typeName}");
            return;
        }

        handler(decoded.Value.objectJson);
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
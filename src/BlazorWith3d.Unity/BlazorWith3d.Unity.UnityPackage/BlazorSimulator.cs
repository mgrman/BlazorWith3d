#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;
using UnityEditor;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public interface IBlazorSimulatorMessageWithResponseHandler
    {
        Type MessageType { get; }
        Type ResponseType { get; }
        
        object HandleMessage(object message);
    }
    
    public class BlazorSimulator:MonoBehaviour
    {
        private TypedUnityApi _api;
        private Assembly _messsageAssembly;

        private Dictionary<(Type msgType, Type responseType), object> _responses = new();
        
        public void Initialize(TypedUnityApi api, Assembly messsageAssembly)
        {
            _api = api;
            _messsageAssembly=messsageAssembly;
            
            
        
            var messageTypesToHandle = _messsageAssembly.GetTypes().Where(o =>
                o.GetInterfaces().Any(i => i == typeof(IMessageToBlazor)));

            foreach (var messageTypeToHandle in messageTypesToHandle)
            {
                _api.GetType()
                    .GetMethod(nameof(TypedUnityApi.AddMessageProcessCallback))
                    .MakeGenericMethod(messageTypeToHandle)
                    .Invoke(_api, new [] {CreateMessageProcessCallback(o=>Debug.Log($"Received msg {messageTypeToHandle.Name} {JsonUtility.ToJson(o)}"),messageTypeToHandle) });
            }
            
            
            var messageTypesToRespond = _messsageAssembly.GetTypes()
                .Select(o =>
                {
                    var messageWithResponseInterface = o.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageToBlazor<>));

                    if (messageWithResponseInterface != null)
                    {
                        return (messageType:o, responseType: messageWithResponseInterface.GenericTypeArguments[0]);
                    }

                    return (messageType:null, responseType: null);
                })
                .Where(o=>o.messageType!=null);

            var attachedHandlers=gameObject.GetComponents<IBlazorSimulatorMessageWithResponseHandler>()
                .ToDictionary(o=>(o.MessageType,o.ResponseType), o=>(Func<object,object>)o.HandleMessage);
            
            foreach (var (messageType, responseToSendType) in messageTypesToRespond)
            {
                if (!attachedHandlers.TryGetValue((messageType, responseToSendType), out var callback))
                {
                    _responses[(messageType, responseToSendType)]=Activator.CreateInstance(responseToSendType);
                    callback = o =>
                    {
                        Debug.Log(
                            $"Received msg {o.GetType().Name} wanting response of {responseToSendType.Name} {JsonUtility.ToJson(o)}");
                        return _responses[(messageType, responseToSendType)];
                    };
                }
                
                
                _api.GetType()
                    .GetMethod(nameof(TypedUnityApi.AddMessageWithResponseProcessCallback))
                    .MakeGenericMethod(new[]{messageType,responseToSendType})
                    .Invoke(_api, new [] {CreateMessageWithResponseProcessCallback(callback,messageType, responseToSendType) });
            }
        }

        private object CreateMessageWithResponseProcessCallback(Func<object, object> callback, Type msgType, Type responseType)
        {
            return this.GetType().GetMethod(nameof(CreateMessageWithResponseProcessCallbackGeneric), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(msgType, responseType)
                .Invoke(this,new object[] { callback });
        }

        private  Func<TMessage, Task<TResponse>> CreateMessageWithResponseProcessCallbackGeneric<TMessage, TResponse>(Func<object, object> callback)
        {
            return async (m)=>(TResponse) callback(m);
        }

        private object CreateMessageProcessCallback(Action<object> callback, Type type)
        {
            return this.GetType().GetMethod(nameof(CreateMessageProcessCallbackGeneric), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type)
                .Invoke(this,new object[] { callback });
        }

        private Action<TMessage> CreateMessageProcessCallbackGeneric<TMessage>(Action<object> callback)
        {
            return (m)=>callback(m);
        }
        

        [CustomEditor(typeof(BlazorSimulator))]
        public class BlazorSimulatorEditor : Editor
        {
            private Dictionary<Type, object> _editedInstances = new Dictionary<Type, object>();
            
            
            public override void OnInspectorGUI()
            {
                var blazorSimulator = (BlazorSimulator)target;

                if (Application.isPlaying)
                {
                    var messageTypes = blazorSimulator._messsageAssembly
                        .GetTypes()
                        .Select(o =>
                        {
                            var messageInterface = o.GetInterfaces()
                                .FirstOrDefault(i => i == typeof(IMessageToUnity) );

                            if (messageInterface != null)
                            {
                                return (messageType:o, responseType: null);
                            }
                            
                            var messageWithResponseInterface = o.GetInterfaces()
                                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageToUnity<>));

                            if (messageWithResponseInterface != null)
                            {
                                return (messageType:o, responseType: messageWithResponseInterface.GenericTypeArguments[0]);
                            }

                            return (messageType:null, responseType: null);
                        })
                        .Where(o=>o.messageType!=null);

                    foreach (var (messageType, responseType) in messageTypes)
                    {
                        EditorGUILayout.LabelField($"Send message of type {messageType.Name}");
                        if (!_editedInstances.TryGetValue(messageType, out var instance))
                        {
                            instance = Activator.CreateInstance(messageType);
                            _editedInstances[messageType] = instance;
                        }

                          EditMessage(messageType, instance);

                        if (responseType==null)
                        {
                            if (GUILayout.Button("Send message"))
                            {
                                typeof(TypedUnityApi).GetMethod(nameof(TypedUnityApi.SendMessage))
                                    .MakeGenericMethod(messageType).Invoke(blazorSimulator._api, new[] { instance });
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Send message with response"))
                            {
                                var task = typeof(TypedUnityApi)
                                    .GetMethod(nameof(TypedUnityApi.SendMessageWithResponse))
                                    .MakeGenericMethod(new[] { messageType, responseType })
                                    .Invoke(blazorSimulator._api, new[] { instance }) as Task;

                                task.ContinueWith(o =>
                                {
                                    var result = task.GetType().GetProperty("Result").GetValue(task);
                                    Debug.Log($"Got response of {JsonUtility.ToJson(result)}");
                                });

                            }
                        }
                    }
                    
                    
                    foreach (var response in blazorSimulator._responses)
                    {
                        EditorGUILayout.LabelField($"Prepare response for {response.Key.responseType.Name}");
                         EditMessage(response.Key.responseType,response.Value);

                    }
                }
            }

            private void EditMessage(Type messageType, object instance = null)
            {
                foreach (var field in messageType.GetFields())
                {
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValue(instance,EditorGUILayout.IntField(field.Name, (int)field.GetValue(instance)));
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValue(instance,EditorGUILayout.FloatField(field.Name, (float)field.GetValue(instance)));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(instance,EditorGUILayout.TextField(field.Name, (string)field.GetValue(instance)));
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        field.SetValue(instance,EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(instance)));
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"Unsupported type of {field.FieldType.Name}");
                    }
                }
            }
        }
    }
}

#endif
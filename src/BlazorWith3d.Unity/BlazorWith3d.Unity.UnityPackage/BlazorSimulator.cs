#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlazorWith3d.Unity.Shared;
using UnityEditor;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public interface IBlazorSimulatorMessageHandler
    {
        Type MessageType { get; }

        TypedUnityApi UnityApi { set; }

        void HandleMessage(object message);
    }

    public class BlazorSimulator : MonoBehaviour
    {
        private TypedUnityApi _api;
        private Assembly _messsageAssembly;

        public void Initialize(TypedUnityApi api, Assembly messsageAssembly)
        {
            _api = api;
            _messsageAssembly = messsageAssembly;


            var messageTypesToHandle = _messsageAssembly.GetTypes().Where(o =>
                o.GetInterfaces().Any(i => i == typeof(IMessageToBlazor)));

            var handlerCompoenents = gameObject.GetComponents<IBlazorSimulatorMessageHandler>();
            foreach (var handlerCompoenent in handlerCompoenents) handlerCompoenent.UnityApi = _api;

            var attachedHandlers = handlerCompoenents
                .ToDictionary(o => o.MessageType, o => (Action<object>)o.HandleMessage);

            foreach (var messageType in messageTypesToHandle)
            {
                if (!attachedHandlers.TryGetValue(messageType, out var callback))
                    callback = o => { Debug.Log($"Received msg {messageType.Name} {JsonUtility.ToJson(o)}"); };


                _api.GetType()
                    .GetMethod(nameof(TypedUnityApi.AddMessageProcessCallback))
                    .MakeGenericMethod(messageType)
                    .Invoke(_api, new[] { CreateMessageProcessCallback(callback, messageType) });
            }
        }

        private object CreateMessageProcessCallback(Action<object> callback, Type type)
        {
            return GetType()
                .GetMethod(nameof(CreateMessageProcessCallbackGeneric), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(type)
                .Invoke(this, new object[] { callback });
        }

        private Action<TMessage> CreateMessageProcessCallbackGeneric<TMessage>(Action<object> callback)
        {
            return m => callback(m);
        }


        [CustomEditor(typeof(BlazorSimulator))]
        public class BlazorSimulatorEditor : Editor
        {
            private readonly Dictionary<Type, object> _editedInstances = new();


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
                                .FirstOrDefault(i => i == typeof(IMessageToUnity));

                            if (messageInterface != null) return o;

                            return null;
                        })
                        .Where(o => o != null);

                    foreach (var messageType in messageTypes)
                    {
                        EditorGUILayout.LabelField($"Send message of type {messageType.Name}");
                        if (!_editedInstances.TryGetValue(messageType, out var instance))
                        {
                            instance = Activator.CreateInstance(messageType);
                            _editedInstances[messageType] = instance;
                        }

                        EditMessage(messageType, instance);
                        if (GUILayout.Button("Send message"))
                            typeof(TypedUnityApi).GetMethod(nameof(TypedUnityApi.SendMessage))
                                .MakeGenericMethod(messageType).Invoke(blazorSimulator._api, new[] { instance });
                    }
                }
            }

            private void EditMessage(Type messageType, object instance = null)
            {
                foreach (var field in messageType.GetFields())
                    if (field.FieldType == typeof(int))
                        field.SetValue(instance, EditorGUILayout.IntField(field.Name, (int)field.GetValue(instance)));
                    else if (field.FieldType == typeof(float))
                        field.SetValue(instance,
                            EditorGUILayout.FloatField(field.Name, (float)field.GetValue(instance)));
                    else if (field.FieldType == typeof(string))
                        field.SetValue(instance,
                            EditorGUILayout.TextField(field.Name, (string)field.GetValue(instance)));
                    else if (field.FieldType == typeof(bool))
                        field.SetValue(instance, EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(instance)));
                    else
                        EditorGUILayout.LabelField($"Unsupported type of {field.FieldType.Name}");
            }
        }
    }
}

#endif
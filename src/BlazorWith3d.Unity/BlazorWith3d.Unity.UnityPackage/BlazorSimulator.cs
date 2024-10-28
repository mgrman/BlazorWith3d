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

        I3DAppObjectApi ObjectApi { set; }

        void HandleMessage(object message);
    }

    public class BlazorSimulator : MonoBehaviour
    {
        private I3DAppObjectApi _api;

        public void Initialize(I3DAppObjectApi api)
        {
            _api = api;

            var handlerComponents = gameObject.GetComponents<IBlazorSimulatorMessageHandler>();
            foreach (var handlerComponent in handlerComponents)
            {
                handlerComponent.ObjectApi = _api;
            }

            var attachedHandlers = handlerComponents
                .ToDictionary(o => o.MessageType, o => (Action<object>)o.HandleMessage);

            _api.OnMessageObject += o =>
            {
                if (!attachedHandlers.TryGetValue(o.GetType(), out var callback))
                {
                    callback = o => { Debug.Log($"Received msg {o.GetType().Name} {JsonUtility.ToJson(o)}"); };
                }

                callback.Invoke(o);
            };
        }


        [CustomEditor(typeof(BlazorSimulator))]
        public class BlazorSimulatorEditor : Editor
        {
            private readonly Dictionary<Type, object> _editedInstances = new();

            public override void OnInspectorGUI()
            {
                var appApi = ((BlazorSimulator)target)._api;

                if (Application.isPlaying)
                {
                    var messageTypes = appApi
                        .SupportedInvokeMessageTypes;

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
                        {
                            appApi.InvokeMessageObject(instance);
                        }
                    }
                }
            }

            private void EditMessage(Type messageType, object instance = null)
            {
                foreach (var field in messageType.GetFields())
                    if (field.FieldType == typeof(int))
                        field.SetValue(instance, EditorGUILayout.IntField(field.Name, (int)field.GetValue(instance)));
                    else if (field.FieldType == typeof(long))
                        field.SetValue(instance, EditorGUILayout.LongField(field.Name, (long)field.GetValue(instance)));
                    else if (field.FieldType == typeof(float))
                        field.SetValue(instance,
                            EditorGUILayout.FloatField(field.Name, (float)field.GetValue(instance)));
                    else if (field.FieldType == typeof(double))
                        field.SetValue(instance,
                            EditorGUILayout.DoubleField(field.Name, (double)field.GetValue(instance)));
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
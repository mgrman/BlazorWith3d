using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Shared;
using BlazorWith3d.Unity;

using JetBrains.Annotations;

using UnityEngine;


namespace ExampleApp
{
    public class ExampleAppInitializer : MonoBehaviour
    {
        private readonly DisposableContainer _scriptDisposable=new ();
        
        [CanBeNull]
        private DisposableContainer _rendererDisposable;

        public static Uri HostUrl = null;

        [SerializeField] private UnityRenderer _appPrefab;
        

        [Tooltip("If none is set, will use simulator")] 
        [SerializeField]
        private string _backendWebsocketUrl = "ws://localhost:5292/debug-relay-unity-ws";

        private readonly SemaphoreSlim _semaphore= new SemaphoreSlim(1);

        public async void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
            #endif
            Debug.Log($"cmdArgs: {string.Join(" ",Environment.GetCommandLineArgs())}");
            
#if UNITY_EDITOR

            if (string.IsNullOrEmpty(_backendWebsocketUrl))
            {
                throw new InvalidOperationException(
                    "_backendWebsocketUrl is not set! It must be set to run in Editor!");
            }
            else
            {
                var relay = new BlazorWebSocketRelay(_backendWebsocketUrl, async api =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {

                        await Awaitable.MainThreadAsync();

                        if (_rendererDisposable != null)
                        {
                            await _rendererDisposable.DisposeAsync();
                            _rendererDisposable = null;
                        }

                        if (api != null)
                        {
                            var binaryApi = new BinaryApiOverBinaryMessageApi(api);
                            await CreateControllerAndRenderer(binaryApi);


                            var imageCapturer = new CameraImageStreamer(api);
                            _rendererDisposable.TrackDisposable(imageCapturer);

                        }
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
                
                
                _scriptDisposable.TrackDisposable(relay);
                
                var uriBuilder = new UriBuilder(_backendWebsocketUrl);
                uriBuilder.Scheme="http";
                uriBuilder.Path = "";
                HostUrl = uriBuilder.Uri;
            }
#elif UNITY_WEBGL

            HostUrl = new Uri(Application.absoluteURL);
            var binaryApi = UnityBlazorApi.Singleton;
            await CreateControllerAndRenderer(binaryApi);
            UnityBlazorApi.InitializeWebGLInterop();
#endif
        }

        private async ValueTask CreateControllerAndRenderer(IBinaryApi binaryApi)
        {
            var activeRenderer = Instantiate(_appPrefab);

            Console.WriteLine($"{Screen.width},{Screen.height}");
            var controller = new BlocksOnGrid3DControllerOverBinaryApi(binaryApi, new MemoryPackBinaryApiSerializer(),
                new PoolingArrayBufferWriterFactory(),null);
            controller.OnMessageError += (o, e) =>
            {
                Debug.LogException(e);
            };
            
            await controller.SetRenderer(activeRenderer);
            await activeRenderer.SetController(controller);
            
            
            _rendererDisposable = new DisposableContainer();
            
            _rendererDisposable.TrackDisposable(controller);
            _rendererDisposable.TrackDestroy(activeRenderer.gameObject);
        }

        private async Awaitable OnDestroy()
        {
            await _scriptDisposable.DisposeAsync();
        }
    }


#if UNITY_EDITOR

    public class CameraImageStreamer:IDisposable
    {
        private readonly BinaryApiForSocket _relay;
        private readonly CancellationTokenSource _cts;
        private readonly RenderTexture _renderTexture;
        private readonly Texture2D _screenshot;
        private  float _nextScreenshotTime;

        private readonly Awaitable _streamTask;

        public CameraImageStreamer(BinaryApiForSocket relay)
        {
            _relay = relay;
            _cts = new CancellationTokenSource();
            _renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            
            _screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            _nextScreenshotTime = 0;
            _streamTask = Stream();
        }

        private async Awaitable Stream()
        {
            while (!_cts.IsCancellationRequested && Application.isPlaying)  
            {
                try
                {
                    await Awaitable.NextFrameAsync();

                    
                    if (Time.time <= _nextScreenshotTime)
                    {
                        continue;
                    }
                    
                    _nextScreenshotTime= Time.time + 1/30f;

                    if (_cts.IsCancellationRequested)
                    {
                        return;
                    }

                    if (Camera.main != null)
                    {
                        var cam = Camera.main;
                        cam.targetTexture = _renderTexture;
                        cam.Render();
                        cam.targetTexture = null;

                        RenderTexture.active = _renderTexture;
                        _screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                        _screenshot.Apply();
                        RenderTexture.active = null;

                        byte[] bytes = _screenshot.EncodeToJPG();

                        await _relay.UpdateScreen(bytes);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    continue;
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _streamTask.Cancel();
        }
    }
#endif
}
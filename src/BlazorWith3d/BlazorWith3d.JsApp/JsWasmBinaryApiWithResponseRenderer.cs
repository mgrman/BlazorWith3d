// LEFTOVER for WASM based interop, postponed for now

// using System.Runtime.InteropServices.JavaScript;
//
// using BlazorWith3d.Shared;
//
// using Microsoft.AspNetCore.Components;
// using Microsoft.Extensions.Logging;
// using Microsoft.JSInterop;
//
// namespace BlazorWith3d.JsApp;
//
// public partial class JsWasmBinaryApiWithResponseRenderer: IJsBinaryApi
// {
//     private readonly SemaphoreSlim _semaphore = new (1, 1);
//
//     private readonly IJSRuntime _jsRuntime;
//     private readonly ILogger _logger;
//     private IJSObjectReference? _typescriptApp;
//     private static Dictionary<int, ArraySegment<byte>> _memoryViews=new Dictionary<int, ArraySegment<byte>>();
//     private static Dictionary<int, IBufferWriterWithArraySegment<byte>> _responseViews=new Dictionary<int, IBufferWriterWithArraySegment<byte>>();
//
//     public JsWasmBinaryApiWithResponseRenderer(IJSRuntime jsRuntime, ILogger logger)
//      {
//          _jsRuntime = jsRuntime;
//          _logger = logger;
//      }
//
//     public  async Task InitializeJsApp(string jsPath, ElementReference container, string initMethod="InitializeApp", object? extraArg=null )
//     { 
//         var module= await _jsRuntime.LoadModuleAsync(jsPath);
//
//         var instanceId = 123;
//          _typescriptApp=   await module.InvokeAsync<IJSObjectReference>(initMethod, container,extraArg,instanceId);
//     }
//
//     public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }
//     
//     public Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler { get; set; }
//
//     private void OnMessageBytesReceived(byte[] messageBytes)
//     {
//         MainMessageHandler.Invoke(messageBytes);
//     }
//     
//     private async ValueTask<byte[]> OnMessageBytesWithResponseReceived(byte[] messageBytes)
//     {
//        var response= await MainMessageWithResponseHandler.Invoke(messageBytes);
//        var responseByteArray= response.WrittenArray.ToArray();// ToArray() as JS interop only has fast path for byte[] type
//        response.Dispose();
//        return responseByteArray;
//     }
//     
//     public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte>  messageBytes)
//     {
//         if (_typescriptApp == null)
//         {
//             throw new InvalidOperationException();
//         }
//
//         try
//         {
//             await _semaphore.WaitAsync();
//             
//             (byte[] array, int offset, int count) data = GetArraysForInterop(messageBytes);
//             await _typescriptApp.InvokeVoidAsync("ProcessMessage",data.array, data.offset, data.count); // ToArray() as JS interop only has fast path for byte[] type
//             messageBytes.Dispose();
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error sending message");
//         }
//         finally
//         {
//             _semaphore.Release();
//         }
//     }
//
//     protected virtual (byte[] array, int offset, int count) GetArraysForInterop(IBufferWriterWithArraySegment<byte> messageBytes)
//     {
//         return (messageBytes.WrittenArray.ToArray(), 0, messageBytes.WrittenArray.Count);
//     }
//
//     public async ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> messageBytes)
//     {
//         if (_typescriptApp == null)
//         {
//             throw new InvalidOperationException();
//         }
//
//         try
//         {
//             await _semaphore.WaitAsync();
//             (byte[] array, int offset, int count) data= GetArraysForInterop(messageBytes);
//             var response = await _typescriptApp.InvokeAsync<byte[]>("ProcessMessageWithResponse", data.array, data.offset, data.count);// ToArray() as JS interop only has fast path for byte[] type
//             messageBytes.Dispose();
//             return response;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Error sending message");
//             throw;
//         }
//         finally
//         {
//             _semaphore.Release();
//         }
//     }
//
//     public async ValueTask DisposeAsync()
//     {
//         await _typescriptApp.TryInvokeVoidAsync(_logger, "Quit");
//         await _typescriptApp.TryDisposeAsync();
//         _typescriptApp = null;
//     }
//
//
//     [JSImport("globalThis.console.log")]
//     public static partial Task SendMessage([JSMarshalAs<JSType.MemoryView>] ArraySegment<byte> content);
//
//     // return value is count to allocate and call ReadResponse
//     [JSImport("globalThis.console.log")]
//     public static partial Task<int> SendMessageWithResponse([JSMarshalAs<JSType.MemoryView>] ArraySegment<byte> content);
//
//     [JSImport("globalThis.console.log")]
//     public static partial void ReadResponse([ JSMarshalAs<JSType.MemoryView>]ArraySegment<byte> bufferToFill);
//
//     
//     
//     [JSExport]
//     [return: JSMarshalAs<JSType.MemoryView>]
//     public static ArraySegment<byte> AllocateMemoryView([JSMarshalAs<JSType.Number>]int count, int id)
//     {
//         var view =new ArraySegment<byte>(new byte[count]);
//         _memoryViews[id] = view;
//         return view;
//     }
//     
//     
//     [JSExport]
//     public static void OnWasmMessageReceived(int id)
//     {
//         var messageBytes=_memoryViews[id];
//         _memoryViews.Remove(id);
//         MainMessageHandler.Invoke(messageBytes);
//     }
//
//     [JSExport]
//     public static async Task OnWasmMessageWithResponseReceived(int id)
//     {
//         var messageBytes=_memoryViews[id];
//         _memoryViews.Remove(id);
//         var response= await MainMessageWithResponseHandler.Invoke(messageBytes);
//         _responseViews[id]=response;
//     }
//     
//     [JSExport]
//     [return: JSMarshalAs<JSType.MemoryView>]
//     public static ArraySegment<byte> ReturnResponseToJs(int id)
//     {
//         var response=_responseViews[id];
//
//         Task.Run(async () =>
//         {
//             await Task.Yield();
//             response.Dispose();
//         });
//         return response.WrittenArray;
//     }
// }
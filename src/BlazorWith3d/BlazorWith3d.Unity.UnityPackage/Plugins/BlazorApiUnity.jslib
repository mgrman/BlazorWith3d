var BlazorApiUnity = {
    _InitializeApi: async function (readMessageCallback,readMessageWithResponseCallback,readResponseCallback, onConnectedToControllerCallback) {


        Module["BlazorApi_ReadMessageBuffer"] = {};
        Module["BlazorApi_IdCounter"] = 0;
        Module["BlazorApi_ReadResponseCallback"] = readResponseCallback;

        // void BlazorApi_OnConnectedToController()
        Module["BlazorApi_OnConnectedToController"] = function () {

            {{{ makeDynCall('vii', 'onConnectedToControllerCallback') }}}();
        };

        
        // void BlazorApi_SendMessageToUnity(byte[] message)
        Module["BlazorApi_SendMessageToUnity"] = function (bytes) {

            var id = Module["BlazorApi_IdCounter"]++;
            Module["BlazorApi_IdCounter"]=Module["BlazorApi_IdCounter"]>2147483646?0:Module["BlazorApi_IdCounter"];

            Module["BlazorApi_ReadMessageBuffer"][id] = bytes;
            {{{ makeDynCall('vii', 'readMessageCallback') }}}(bytes.length, id);
        };

        // void BlazorApi_SendMessageToUnity(byte[] message)
        Module["BlazorApi_SendMessageWithResponseToUnity"] = function  (bytes) {

            var id = Module["BlazorApi_IdCounter"]++;
            Module["BlazorApi_IdCounter"]=Module["BlazorApi_IdCounter"]>2147483646?0:Module["BlazorApi_IdCounter"];

            Module["BlazorApi_ReadMessageBuffer"][id] = bytes;
            
            var deferred = {};
            var result= new Promise(resolve => {
                deferred.resolve = resolve;
            });

            if(Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"] == null)
            {
                Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"]= {};
            }
            Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"][id] = deferred;
            
            {{{ makeDynCall('vii', 'readMessageWithResponseCallback') }}}(bytes.length, id);
            return result;
        };


        if(Module["BlazorApi_InitPromiseResolve"] != null){
            Module["BlazorApi_InitPromiseResolve"]();
            Module["BlazorApi_InitPromiseResolve"]=null;
        }
    },
    _ReadBytesBuffer: function (id, array) {

        //console.log("_ReadMessage"+Module["BlazorApi_ReadMessageBuffer"][id]+" at "+array)

        HEAPU8.set(Module["BlazorApi_ReadMessageBuffer"][id], array);

        delete Module["BlazorApi_ReadMessageBuffer"][id]
    },

    _SendMessageFromUnity: function (array,offset, size) {
        //console.log("Array at "+array)
        //console.log("size at "+size)

        // the emscripten heap is wrapped with array, so the memory is exposed but not copied 
        var buffer = new Uint8Array(HEAPU8.buffer, array+offset, size);
        //console.log("buffer at "+buffer.length);

        // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
        Module["BlazorApi_OnMessageFromUnityHandler"](buffer)
    },

    _SendMessageWithResponseFromUnity: async function (id, array, offset, size) {
        //console.log("Array at "+array)
        //console.log("size at "+size)

        // the emscripten heap is wrapped with array, so the memory is exposed but not copied 
        var buffer = new Uint8Array(HEAPU8.buffer, array+offset, size);
        //console.log("buffer at "+buffer.length);

        // byte[] BlazorApi_OnMessageWithResponseFromUnityHandler(byte[] message)
        var response=await Module["BlazorApi_OnMessageWithResponseFromUnityHandler"](buffer);

        Module["BlazorApi_ReadMessageBuffer"][id] = response;

        readResponseCallback=Module["BlazorApi_ReadResponseCallback"] ;
        {{{ makeDynCall('vii', 'readResponseCallback') }}}(response.length, id);
        
    },

    _GetNextRequestId:function (){

        var id = Module["BlazorApi_IdCounter"]++;
        Module["BlazorApi_IdCounter"]=Module["BlazorApi_IdCounter"]>2147483646?0:Module["BlazorApi_IdCounter"];
        return id;
    },

    _SendResponseFromUnity: function (id, array, offset, size) {
        //console.log("Array at "+array)
        //console.log("size at "+size)

        // the emscripten heap is wrapped with array, so the memory is exposed but not copied 
        var buffer = new Uint8Array(HEAPU8.buffer, array+offset, size);
        //console.log("buffer at "+buffer.length);

        // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
        Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"][id].resolve(buffer)
        delete Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"][id]
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
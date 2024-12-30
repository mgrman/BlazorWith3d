var BlazorApiUnity = {
    _InitializeApi: async function (readMessageCallback,readMessageWithResponseCallback) {
        
        
        Module["BlazorApi_ReadMessageBuffer"] = {};
        
        var idCounter=0;

        // void BlazorApi_SendMessageToUnity(byte[] message)
        Module["BlazorApi_SendMessageToUnity"] = function (bytes) {

            var id = idCounter++;
            idCounter=idCounter>2147483646?0:idCounter;

            Module["BlazorApi_ReadMessageBuffer"][id] = bytes;
            {{{ makeDynCall('vii', 'readMessageCallback') }}}(bytes.length, id);
        };

        // void BlazorApi_SendMessageToUnity(byte[] message)
        Module["BlazorApi_SendMessageWithResponseToUnity"] = function  (bytes) {

            var id = idCounter++;
            idCounter=idCounter>2147483646?0:idCounter;

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

    _SendMessageFromUnity: function (array, size) {
        //console.log("Array at "+array)
        //console.log("size at "+size)

        // the emscripten heap is wrapped with array, so the memory is exposed but not copied 
        var buffer = new Uint8Array(HEAPU8.buffer, array, size);
        //console.log("buffer at "+buffer.length);

        // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
        Module["BlazorApi_OnMessageFromUnityHandler"](buffer)
    },

    _SendResponseFromUnity: function (id, array, size) {
        //console.log("Array at "+array)
        //console.log("size at "+size)

        // the emscripten heap is wrapped with array, so the memory is exposed but not copied 
        var buffer = new Uint8Array(HEAPU8.buffer, array, size);
        //console.log("buffer at "+buffer.length);

        // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
        Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"][id].resolve(buffer)
        delete Module["BlazorApi_SendMessageWithResponseToUnity_ResponsePromise"][id]
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
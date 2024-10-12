var BlazorApiUnity = {
    
    _InitializeApi: async function (instantiateByteArrayCallback) {
        Module["BlazorApi_MessageBuffer"]={};
        // void BlazorApi_SendMessageToUnity(byte[] message)
        Module["BlazorApi_SendMessageToUnity"] = function (bytes){

            var id=Math.floor(Math.random() * 500);
            
            Module["BlazorApi_MessageBuffer"][id]=bytes;
            {{{ makeDynCall('vii', 'instantiateByteArrayCallback') }}}(bytes.length, id);
        };
    },
    _ReadBytesBuffer: function (id, array){

        //console.log("_ReadBytesBuffer"+Module["BlazorApi_MessageBuffer"][id]+" at "+array)
        
        HEAPU8.set(Module["BlazorApi_MessageBuffer"][id], array);

        delete Module["BlazorApi_MessageBuffer"][id]
    },
    
    _SendMessageFromUnity: function (array, size) {

        // TODO check if this works, maybe a copy will have to be made

        //console.log("Array at "+array)
        //console.log("size at "+size)
        
        var buffer= new Uint8Array(HEAPU8.buffer, array, size);
        //console.log("buffer at "+buffer.length);
        
        if(Module["BlazorApi_OnMessageFromUnityHandler"]==null) {
            if (Module["BlazorApi_OnMessageFromUnityHandler_Buffer"] == null) {
                Module["BlazorApi_OnMessageFromUnityHandler_Buffer"] = []
            }
            
            Module["BlazorApi_OnMessageFromUnityHandler_Buffer"].push(buffer.slice());
        }
        
        else {
            // void BlazorApi_OnMessageFromUnityHandler(byte[] message)
            Module["BlazorApi_OnMessageFromUnityHandler"](buffer)
        }
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
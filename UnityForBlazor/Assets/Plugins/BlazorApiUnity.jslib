var BlazorApiUnity = {
    InitializeApi: async function (onMessageReceivedCallback) {
        
        // string BlazorApi_SendMessageToUnity(string message)
        Module["BlazorApi_SendMessageToUnity"]=function (message){ 

            var buffer = stringToNewUTF8(message);
            var response={{{ makeDynCall('ii', 'onMessageReceivedCallback') }}}(buffer);
            _free(buffer);
            
            var _response = UTF8ToString(response);
            _free(response);
            return _response;
        };

        // void BlazorApi_Initialized()
        var onInitializeFunc=Module["BlazorApi_Initialized"];
        if(onInitializeFunc) {
            onInitializeFunc();
        }
    },
    SendMessageFromUnity: function (msgId,message, responseCallback) {

        var _message = UTF8ToString(message);

        // Task<string> BlazorApi_OnMessageFromUnityHandler(string message)
        Module["BlazorApi_OnMessageFromUnityHandler"](_message) // returns promise
            .then(response => {
                var buffer = stringToNewUTF8(response);

                {{{ makeDynCall('vii', 'responseCallback') }}}(msgId,buffer);
            });
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
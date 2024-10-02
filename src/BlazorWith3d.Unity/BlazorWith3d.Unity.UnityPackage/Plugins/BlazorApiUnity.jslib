var BlazorApiUnity = {
    _InitializeApi: async function (onMessageReceivedWithResponseCallback,onMessageReceivedCallback) {
        
        // string BlazorApi_SendMessageToUnityWithResponse(string message)
        Module["BlazorApi_SendMessageToUnityWithResponse"]=function (message){ 

            var buffer = stringToNewUTF8(message);
            var response={{{ makeDynCall('ii', 'onMessageReceivedWithResponseCallback') }}}(buffer);
            _free(buffer);
            
            var _response = UTF8ToString(response);
            _free(response);
            return _response;
        };
        
        // void BlazorApi_SendMessageToUnity(string message)
        Module["BlazorApi_SendMessageToUnity"] = function (message){

            var buffer = stringToNewUTF8(message);
            {{{ makeDynCall('vi', 'onMessageReceivedCallback') }}}(buffer);
            _free(buffer);
        };

        // void BlazorApi_Initialized()
        var onInitializeFunc=Module["BlazorApi_Initialized"];
        if(onInitializeFunc) {
            onInitializeFunc();
        }
    },
    _SendMessageWithResponseFromUnity: function (msgId,message, responseCallback) {

        var _message = UTF8ToString(message);

        // Task<string> BlazorApi_OnMessageFromUnityWithResponseHandler(string message)
        Module["BlazorApi_OnMessageFromUnityWithResponseHandler"](_message) // returns promise
            .then(response => {
                var buffer = stringToNewUTF8(response);

                {{{ makeDynCall('vii', 'responseCallback') }}}(msgId,buffer);
            });
    },
    _SendMessageFromUnity: function (message,) {

        var _message = UTF8ToString(message);

        // void BlazorApi_OnMessageFromUnityHandler(string message)
        Module["BlazorApi_OnMessageFromUnityHandler"](_message)
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
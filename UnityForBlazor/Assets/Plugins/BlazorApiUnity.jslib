var BlazorApiUnity = {
    InitializeApi: function (onMessageReceivedCallback) {
        
        Module["BlazorApi_SendMessageToUnity"]=function (message){

            var buffer = stringToNewUTF8(message);
            var response={{{ makeDynCall('ii', 'onMessageReceivedCallback') }}}(buffer);
            _free(buffer);
            
            var _response = UTF8ToString(response);
            _free(response);
            return _response;
        };

        var onInitializeFunc=Module["BlazorApi_Initialized"];
        if(onInitializeFunc) {
            onInitializeFunc();
        }
    },
    SendMessageFromUnity: function (message) {

        var _message = UTF8ToString(message);

        var returnStr = Module["BlazorApi_OnMessageFromUnityHandler"](_message);

        var buffer = stringToNewUTF8(returnStr);
        return buffer;
    },
};

mergeInto(LibraryManager.library, BlazorApiUnity);
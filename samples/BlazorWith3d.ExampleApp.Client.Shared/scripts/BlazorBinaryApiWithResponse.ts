import {IBinaryApiWithResponse} from "./IBinaryApiWithResponse";

export class BlazorBinaryApiWithResponse implements IBinaryApiWithResponse {
    private _sendMessage: (msgBytes: Uint8Array) => Promise<void>;
    private _sendMessageWithResponse: (msgBytes: Uint8Array) => Promise<Uint8Array>;


    constructor(sendMessage: (msgBytes: Uint8Array) => Promise<void>, sendMessageWithResponse: (msgBytes: Uint8Array) => Promise<Uint8Array>) {
        this._sendMessage = sendMessage;
        this._sendMessageWithResponse=sendMessageWithResponse;
    }

    public mainMessageHandler : (bytes: Uint8Array) => Promise<void>;
    public mainMessageWithResponseHandler : (bytes: Uint8Array) => Promise<Uint8Array>;


    sendMessage(bytes: Uint8Array): Promise<void> {

       return this._sendMessage(bytes);
    }
    sendMessageWithResponse(bytes: Uint8Array): Promise<Uint8Array> {

        return this._sendMessageWithResponse(bytes);
    }
}
import {IBinaryApiWithResponse} from "./IBinaryApiWithResponse";

export class BlazorBinaryApiWithResponse implements IBinaryApiWithResponse {
    private _sendMessage: (msgBytes: Uint8Array) => Promise<void>;
    private _sendMessageWithResponse: (msgBytes: Uint8Array) => Promise<Uint8Array>;
    private previousMessage: Promise<any> = Promise.resolve();


    constructor(sendMessage: (msgBytes: Uint8Array) => Promise<void>, sendMessageWithResponse: (msgBytes: Uint8Array) => Promise<Uint8Array>) {
        this._sendMessage = sendMessage;
        this._sendMessageWithResponse=sendMessageWithResponse;
    }

    public mainMessageHandler : (bytes: Uint8Array) => Promise<void>;
    public mainMessageWithResponseHandler : (bytes: Uint8Array) => Promise<Uint8Array>;


    async sendMessage(bytes: Uint8Array): Promise<void> {

        await this.previousMessage;
        this.previousMessage = this._sendMessage(bytes);
    }
    async sendMessageWithResponse(bytes: Uint8Array): Promise<Uint8Array> {

        await this.previousMessage;
        var promise= this._sendMessageWithResponse(bytes);
        this.previousMessage =promise;
        return await promise;
    }
}
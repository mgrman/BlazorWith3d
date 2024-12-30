import {IBinaryApi} from "./IBinaryApi";

export class BlazorBinaryApi implements IBinaryApi {
    private _sendMessage: (msgBytes: Uint8Array) => Promise<any>;


    constructor(sendMessage: (msgBytes: Uint8Array) => Promise<any>) {
        this._sendMessage = sendMessage;
    }

    public mainMessageHandler : (bytes: Uint8Array) => Promise<void>;

    sendMessage(bytes: Uint8Array): Promise<void> {

        return this._sendMessage(bytes);
    }

}
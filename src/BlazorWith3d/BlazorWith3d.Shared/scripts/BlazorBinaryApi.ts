import {IBinaryApi} from "./IBinaryApi";

export class BlazorBinaryApi implements IBinaryApi {
    private _mainMessageHandler?: (bytes: Uint8Array) => void;
    private _sendMessage: (msgBytes: Uint8Array) => Promise<any>;
    private previousMessage: Promise<void> = Promise.resolve();
    private bufferedMessages: Array<Uint8Array> = new Array<Uint8Array>();


    constructor(sendMessage: (msgBytes: Uint8Array) => Promise<any>) {
        this._sendMessage = sendMessage;
    }

    public get mainMessageHandler() {
        return this._mainMessageHandler;
    }

    public set mainMessageHandler(handler: (bytes: Uint8Array) => void) {

        this._mainMessageHandler = handler;
        if (handler != null) {
            for (const msg of this.bufferedMessages) {
                handler(msg);
            }
            this.bufferedMessages = new Array<Uint8Array>();
        }
    }

    async sendMessage(bytes: Uint8Array): Promise<void> {

        await this.previousMessage;
        this.previousMessage = this._sendMessage(bytes);
    }

    public onMessageReceived(msg: Uint8Array): void {

        if (this.mainMessageHandler == null) {
            this.bufferedMessages.push(msg);
        } else {
            this.mainMessageHandler(msg);
        }

    }

}
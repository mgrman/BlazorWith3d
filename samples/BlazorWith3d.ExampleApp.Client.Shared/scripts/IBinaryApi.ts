
export interface IBinaryApi {

    mainMessageHandler?: (bytes: Uint8Array) => Promise<void>;

    mainMessageWithResponseHandler?: (bytes: Uint8Array) => Promise<Uint8Array>;

    sendMessage(bytes: Uint8Array): Promise<void>;
    sendMessageWithResponse(bytes: Uint8Array): Promise<Uint8Array>;
}


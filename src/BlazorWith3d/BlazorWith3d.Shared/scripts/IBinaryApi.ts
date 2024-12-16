
export interface IBinaryApi {

    // if null, messages are buffered
    mainMessageHandler?: (bytes: Uint8Array) => void;

    sendMessage(bytes: Uint8Array): Promise<void>;
}


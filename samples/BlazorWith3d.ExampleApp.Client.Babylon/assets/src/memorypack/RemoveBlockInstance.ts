import { MemoryPackWriter } from "./MemoryPackWriter";
import { MemoryPackReader } from "./MemoryPackReader";

export class RemoveBlockInstance {
    blockId: number;

    constructor() {
        this.blockId = 0;

    }

    static serialize(value: RemoveBlockInstance | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: RemoveBlockInstance | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(1);
        writer.writeInt32(value.blockId);

    }

    static serializeArray(value: (RemoveBlockInstance | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (RemoveBlockInstance | null)[] | null): void {
        writer.writeArray(value, (writer, x) => RemoveBlockInstance.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): RemoveBlockInstance | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): RemoveBlockInstance | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new RemoveBlockInstance();
        if (count == 1) {
            value.blockId = reader.readInt32();

        }
        else if (count > 1) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.blockId = reader.readInt32(); if (count == 1) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (RemoveBlockInstance | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (RemoveBlockInstance | null)[] | null {
        return reader.readArray(reader => RemoveBlockInstance.deserializeCore(reader));
    }
}

import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class StartDraggingBlock {
    blockId: number;
    templateId: number;

    constructor() {
        this.blockId = 0;
        this.templateId = 0;

    }

    static serialize(value: StartDraggingBlock | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: StartDraggingBlock | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(2);
        writer.writeInt32(value.blockId);
        writer.writeInt32(value.templateId);

    }

    static serializeArray(value: (StartDraggingBlock | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (StartDraggingBlock | null)[] | null): void {
        writer.writeArray(value, (writer, x) => StartDraggingBlock.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): StartDraggingBlock | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): StartDraggingBlock | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new StartDraggingBlock();
        if (count == 2) {
            value.blockId = reader.readInt32();
            value.templateId = reader.readInt32();

        }
        else if (count > 2) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.blockId = reader.readInt32(); if (count == 1) return value;
            value.templateId = reader.readInt32(); if (count == 2) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (StartDraggingBlock | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (StartDraggingBlock | null)[] | null {
        return reader.readArray(reader => StartDraggingBlock.deserializeCore(reader));
    }
}

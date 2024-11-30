import { MemoryPackWriter } from "./MemoryPackWriter";
import { MemoryPackReader } from "./MemoryPackReader";

export class RemoveBlockTemplate {
    templateId: number;

    constructor() {
        this.templateId = 0;

    }

    static serialize(value: RemoveBlockTemplate | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: RemoveBlockTemplate | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(1);
        writer.writeInt32(value.templateId);

    }

    static serializeArray(value: (RemoveBlockTemplate | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (RemoveBlockTemplate | null)[] | null): void {
        writer.writeArray(value, (writer, x) => RemoveBlockTemplate.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): RemoveBlockTemplate | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): RemoveBlockTemplate | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new RemoveBlockTemplate();
        if (count == 1) {
            value.templateId = reader.readInt32();

        }
        else if (count > 1) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.templateId = reader.readInt32(); if (count == 1) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (RemoveBlockTemplate | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (RemoveBlockTemplate | null)[] | null {
        return reader.readArray(reader => RemoveBlockTemplate.deserializeCore(reader));
    }
}

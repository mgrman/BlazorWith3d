import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class AddBlockInstance {
    blockId: number;
    positionX: number;
    positionY: number;
    rotationZ: number;
    templateId: number;

    constructor() {
        this.blockId = 0;
        this.positionX = 0;
        this.positionY = 0;
        this.rotationZ = 0;
        this.templateId = 0;

    }

    static serialize(value: AddBlockInstance | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: AddBlockInstance | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(5);
        writer.writeInt32(value.blockId);
        writer.writeFloat32(value.positionX);
        writer.writeFloat32(value.positionY);
        writer.writeFloat32(value.rotationZ);
        writer.writeInt32(value.templateId);

    }

    static serializeArray(value: (AddBlockInstance | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (AddBlockInstance | null)[] | null): void {
        writer.writeArray(value, (writer, x) => AddBlockInstance.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): AddBlockInstance | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): AddBlockInstance | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new AddBlockInstance();
        if (count == 5) {
            value.blockId = reader.readInt32();
            value.positionX = reader.readFloat32();
            value.positionY = reader.readFloat32();
            value.rotationZ = reader.readFloat32();
            value.templateId = reader.readInt32();

        }
        else if (count > 5) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.blockId = reader.readInt32(); if (count == 1) return value;
            value.positionX = reader.readFloat32(); if (count == 2) return value;
            value.positionY = reader.readFloat32(); if (count == 3) return value;
            value.rotationZ = reader.readFloat32(); if (count == 4) return value;
            value.templateId = reader.readInt32(); if (count == 5) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (AddBlockInstance | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (AddBlockInstance | null)[] | null {
        return reader.readArray(reader => AddBlockInstance.deserializeCore(reader));
    }
}

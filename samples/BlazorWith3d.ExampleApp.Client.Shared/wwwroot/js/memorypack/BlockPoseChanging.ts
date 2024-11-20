import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class BlockPoseChanging {
    blockId: number;
    changingRequestId: number;
    positionX: number;
    positionY: number;
    rotationZ: number;

    constructor() {
        this.blockId = 0;
        this.changingRequestId = 0;
        this.positionX = 0;
        this.positionY = 0;
        this.rotationZ = 0;

    }

    static serialize(value: BlockPoseChanging | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: BlockPoseChanging | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(5);
        writer.writeInt32(value.blockId);
        writer.writeInt32(value.changingRequestId);
        writer.writeFloat32(value.positionX);
        writer.writeFloat32(value.positionY);
        writer.writeFloat32(value.rotationZ);

    }

    static serializeArray(value: (BlockPoseChanging | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (BlockPoseChanging | null)[] | null): void {
        writer.writeArray(value, (writer, x) => BlockPoseChanging.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): BlockPoseChanging | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): BlockPoseChanging | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new BlockPoseChanging();
        if (count == 5) {
            value.blockId = reader.readInt32();
            value.changingRequestId = reader.readInt32();
            value.positionX = reader.readFloat32();
            value.positionY = reader.readFloat32();
            value.rotationZ = reader.readFloat32();

        }
        else if (count > 5) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.blockId = reader.readInt32(); if (count == 1) return value;
            value.changingRequestId = reader.readInt32(); if (count == 2) return value;
            value.positionX = reader.readFloat32(); if (count == 3) return value;
            value.positionY = reader.readFloat32(); if (count == 4) return value;
            value.rotationZ = reader.readFloat32(); if (count == 5) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (BlockPoseChanging | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (BlockPoseChanging | null)[] | null {
        return reader.readArray(reader => BlockPoseChanging.deserializeCore(reader));
    }
}

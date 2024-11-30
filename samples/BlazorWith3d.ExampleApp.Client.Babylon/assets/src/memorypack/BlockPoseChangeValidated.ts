import { MemoryPackReader } from "./MemoryPackReader";
import {MemoryPackWriter} from "./MemoryPackWriter";

export class BlockPoseChangeValidated {
    blockId: number;
    changingRequestId: number;
    isValid: boolean;
    newPositionX: number;
    newPositionY: number;
    newRotationZ: number;

    constructor() {
        this.blockId = 0;
        this.changingRequestId = 0;
        this.isValid = false;
        this.newPositionX = 0;
        this.newPositionY = 0;
        this.newRotationZ = 0;

    }

    static serialize(value: BlockPoseChangeValidated | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: BlockPoseChangeValidated | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(6);
        writer.writeInt32(value.blockId);
        writer.writeInt32(value.changingRequestId);
        writer.writeBoolean(value.isValid);
        writer.writeFloat32(value.newPositionX);
        writer.writeFloat32(value.newPositionY);
        writer.writeFloat32(value.newRotationZ);

    }

    static serializeArray(value: (BlockPoseChangeValidated | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (BlockPoseChangeValidated | null)[] | null): void {
        writer.writeArray(value, (writer, x) => BlockPoseChangeValidated.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): BlockPoseChangeValidated | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): BlockPoseChangeValidated | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new BlockPoseChangeValidated();
        if (count == 6) {
            value.blockId = reader.readInt32();
            value.changingRequestId = reader.readInt32();
            value.isValid = reader.readBoolean();
            value.newPositionX = reader.readFloat32();
            value.newPositionY = reader.readFloat32();
            value.newRotationZ = reader.readFloat32();

        }
        else if (count > 6) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.blockId = reader.readInt32(); if (count == 1) return value;
            value.changingRequestId = reader.readInt32(); if (count == 2) return value;
            value.isValid = reader.readBoolean(); if (count == 3) return value;
            value.newPositionX = reader.readFloat32(); if (count == 4) return value;
            value.newPositionY = reader.readFloat32(); if (count == 5) return value;
            value.newRotationZ = reader.readFloat32(); if (count == 6) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (BlockPoseChangeValidated | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (BlockPoseChangeValidated | null)[] | null {
        return reader.readArray(reader => BlockPoseChangeValidated.deserializeCore(reader));
    }
}

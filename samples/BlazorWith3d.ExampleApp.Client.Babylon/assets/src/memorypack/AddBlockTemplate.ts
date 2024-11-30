import { MemoryPackWriter } from "./MemoryPackWriter";
import { MemoryPackReader } from "./MemoryPackReader";

export class AddBlockTemplate {
    sizeX: number;
    sizeY: number;
    sizeZ: number;
    templateId: number;
    visualsUri: string | null;

    constructor() {
        this.sizeX = 0;
        this.sizeY = 0;
        this.sizeZ = 0;
        this.templateId = 0;
        this.visualsUri = null;

    }

    static serialize(value: AddBlockTemplate | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: AddBlockTemplate | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(5);
        writer.writeFloat32(value.sizeX);
        writer.writeFloat32(value.sizeY);
        writer.writeFloat32(value.sizeZ);
        writer.writeInt32(value.templateId);
        writer.writeString(value.visualsUri);

    }

    static serializeArray(value: (AddBlockTemplate | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (AddBlockTemplate | null)[] | null): void {
        writer.writeArray(value, (writer, x) => AddBlockTemplate.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): AddBlockTemplate | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): AddBlockTemplate | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new AddBlockTemplate();
        if (count == 5) {
            value.sizeX = reader.readFloat32();
            value.sizeY = reader.readFloat32();
            value.sizeZ = reader.readFloat32();
            value.templateId = reader.readInt32();
            value.visualsUri = reader.readString();

        }
        else if (count > 5) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.sizeX = reader.readFloat32(); if (count == 1) return value;
            value.sizeY = reader.readFloat32(); if (count == 2) return value;
            value.sizeZ = reader.readFloat32(); if (count == 3) return value;
            value.templateId = reader.readInt32(); if (count == 4) return value;
            value.visualsUri = reader.readString(); if (count == 5) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (AddBlockTemplate | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (AddBlockTemplate | null)[] | null {
        return reader.readArray(reader => AddBlockTemplate.deserializeCore(reader));
    }
}

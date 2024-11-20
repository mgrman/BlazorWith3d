import { MemoryPackWriter } from "./MemoryPackWriter.js";
import { MemoryPackReader } from "./MemoryPackReader.js";

export class PerfCheck {
    aaa: number;
    bbb: number;
    ccc: bigint;
    ddd: string | null;
    id: number;

    constructor() {
        this.aaa = 0;
        this.bbb = 0;
        this.ccc = 0n;
        this.ddd = null;
        this.id = 0;

    }

    static serialize(value: PerfCheck | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeCore(writer, value);
        return writer.toArray();
    }

    static serializeCore(writer: MemoryPackWriter, value: PerfCheck | null): void {
        if (value == null) {
            writer.writeNullObjectHeader();
            return;
        }

        writer.writeObjectHeader(5);
        writer.writeFloat32(value.aaa);
        writer.writeFloat64(value.bbb);
        writer.writeInt64(value.ccc);
        writer.writeString(value.ddd);
        writer.writeInt32(value.id);

    }

    static serializeArray(value: (PerfCheck | null)[] | null): Uint8Array {
        const writer = MemoryPackWriter.getSharedInstance();
        this.serializeArrayCore(writer, value);
        return writer.toArray();
    }

    static serializeArrayCore(writer: MemoryPackWriter, value: (PerfCheck | null)[] | null): void {
        writer.writeArray(value, (writer, x) => PerfCheck.serializeCore(writer, x));
    }

    static deserialize(buffer: ArrayBuffer): PerfCheck | null {
        return this.deserializeCore(new MemoryPackReader(buffer));
    }

    static deserializeCore(reader: MemoryPackReader): PerfCheck | null {
        const [ok, count] = reader.tryReadObjectHeader();
        if (!ok) {
            return null;
        }

        const value = new PerfCheck();
        if (count == 5) {
            value.aaa = reader.readFloat32();
            value.bbb = reader.readFloat64();
            value.ccc = reader.readInt64();
            value.ddd = reader.readString();
            value.id = reader.readInt32();

        }
        else if (count > 5) {
            throw new Error("Current object's property count is larger than type schema, can't deserialize about versioning.");
        }
        else {
            if (count == 0) return value;
            value.aaa = reader.readFloat32(); if (count == 1) return value;
            value.bbb = reader.readFloat64(); if (count == 2) return value;
            value.ccc = reader.readInt64(); if (count == 3) return value;
            value.ddd = reader.readString(); if (count == 4) return value;
            value.id = reader.readInt32(); if (count == 5) return value;

        }
        return value;
    }

    static deserializeArray(buffer: ArrayBuffer): (PerfCheck | null)[] | null {
        return this.deserializeArrayCore(new MemoryPackReader(buffer));
    }

    static deserializeArrayCore(reader: MemoryPackReader): (PerfCheck | null)[] | null {
        return reader.readArray(reader => PerfCheck.deserializeCore(reader));
    }
}

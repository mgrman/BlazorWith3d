using System;
using System.Buffers;


namespace BlazorWith3d.Shared
{
    public static class BufferWriterExtensions
    {
        public static void Write(this IBufferWriter<byte> writer, byte value)
        {
            Span<byte> messageIdSpan = stackalloc byte[1];
            messageIdSpan[0] = value;
            writer.Write(messageIdSpan);
        }

        public static void Write(this IBufferWriter<byte> writer, byte value1, byte? optionalValue2)
        {
            Span<byte> messageIdSpan = stackalloc byte[optionalValue2 == null ? 1 : 2];
            messageIdSpan[0] = value1;
            if (optionalValue2 != null) messageIdSpan[1] = optionalValue2.Value;
            writer.Write(messageIdSpan);
        }

        public static void Write(this IBufferWriter<byte> writer, byte? optionalValue)
        {
            if (!optionalValue.HasValue)
            {
                return;
            }

            writer.Write(optionalValue.Value);
        }
    }
}
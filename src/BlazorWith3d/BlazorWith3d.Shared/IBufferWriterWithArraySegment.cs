using System;
using System.Buffers;

public interface IBufferWriterWithArraySegment<T> : IBufferWriter<T>,  IDisposable
{
    ArraySegment<T> WrittenArray { get; }
}

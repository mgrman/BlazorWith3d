using System;
using System.Buffers;

public interface IBufferWriterFactory<T>
{
    IBufferWriterWithArraySegment<T>  Create();
}
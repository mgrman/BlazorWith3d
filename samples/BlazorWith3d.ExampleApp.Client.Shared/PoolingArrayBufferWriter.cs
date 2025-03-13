using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using System;

// Based on https://github.com/dotnet/dotNext/blob/master/src/DotNext/Buffers/PoolingArrayBufferWriter.cs and adapted to be used with .NetStandard 2.1 and langversion 11

public abstract class BufferWriter<T> : IDisposable, IBufferWriter<T>
{
    /// <summary>
    /// Represents position of write cursor.
    /// </summary>
    private protected int position;

    /// <summary>
    /// Initializes a new memory writer.
    /// </summary>
    private protected BufferWriter()
    {
    }

    /// <summary>
    /// Gets the data written to the underlying buffer so far.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public abstract ReadOnlyMemory<T> WrittenMemory { get; }

    /// <summary>
    /// Gets or sets the amount of data written to the underlying memory so far.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <see cref="Capacity"/>.</exception>
    public int WrittenCount
    {
        get => position;
        set
        {
            if ((uint)value > (uint)Capacity)
                ThrowArgumentOutOfRangeException();

            position = value;

            [DoesNotReturn]
            static void ThrowArgumentOutOfRangeException()
                => throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    /// <summary>
    /// Writes single element.
    /// </summary>
    /// <param name="item">The element to write.</param>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public void Add(T item)
    {
        MemoryMarshal.GetReference(GetSpan(1)) = item;
        position += 1;
    }
    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element to retrieve.</param>
    /// <value>The element at the specified index.</value>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> the index is invalid.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public ref readonly T this[int index] => ref WrittenMemory.Span[index];


    /// <summary>
    /// Gets or sets the total amount of space within the underlying memory.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than zero.</exception>
    public abstract int Capacity { get; }

    /// <summary>
    /// Gets the amount of space available that can still be written into without forcing the underlying buffer to grow.
    /// </summary>
    public int FreeCapacity => Capacity - WrittenCount;

    /// <summary>
    /// Clears the data written to the underlying memory.
    /// </summary>
    /// <param name="reuseBuffer"><see langword="true"/> to reuse the internal buffer; <see langword="false"/> to destroy the internal buffer.</param>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public abstract void Clear(bool reuseBuffer = false);

    /// <summary>
    /// Notifies this writer that <paramref name="count"/> of data items were written.
    /// </summary>
    /// <param name="count">The number of data items written to the underlying buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero.</exception>
    /// <exception cref="InvalidOperationException">Attempts to advance past the end of the underlying buffer.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public void Advance(int count)
    {
        var newPosition = position + count;
        if ((uint)newPosition > (uint)Capacity)
            ThrowInvalidOperationException();

        position = newPosition;

        [DoesNotReturn]
        static void ThrowInvalidOperationException()
            => throw new InvalidOperationException();
    }

    /// <summary>
    /// Moves the writer back the specified number of items.
    /// </summary>
    /// <param name="count">The number of items.</param>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than zero or greater than <see cref="WrittenCount"/>.</exception>
    public void Rewind(int count)
    {
        position -= count;
    }

    /// <summary>
    /// Returns the memory to write to that is at least the requested size.
    /// </summary>
    /// <param name="sizeHint">The minimum length of the returned memory.</param>
    /// <returns>The memory block of at least the size <paramref name="sizeHint"/>.</returns>
    /// <exception cref="OutOfMemoryException">The requested buffer size is not available.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public abstract Memory<T> GetMemory(int sizeHint = 0);

    /// <summary>
    /// Returns the memory to write to that is at least the requested size.
    /// </summary>
    /// <param name="sizeHint">The minimum length of the returned memory.</param>
    /// <returns>The memory block of at least the size <paramref name="sizeHint"/>.</returns>
    /// <exception cref="OutOfMemoryException">The requested buffer size is not available.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public virtual Span<T> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

    /// <summary>
    /// Reallocates internal buffer.
    /// </summary>
    /// <param name="newSize">A new size of internal buffer.</param>
    private protected abstract void Resize(int newSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected void CheckAndResizeBuffer(int sizeHint)
    {
        if (GetBufferSize(sizeHint, Capacity, position, out sizeHint))
            Resize(sizeHint);
    }

    private static int MaxLength => 0X7FFFFFC7;
    
    private const int DefaultInitialBufferSize = 128;
    internal static bool GetBufferSize(int sizeHint, int capacity, int writtenCount, out int newSize)
    {
        Debug.Assert(sizeHint >= 0);
        Debug.Assert(capacity >= 0);
        Debug.Assert(writtenCount >= 0);

        if (sizeHint is 0)
            sizeHint = 1;

        if (sizeHint > capacity - writtenCount)
        {
            var growBy = capacity is 0 ? DefaultInitialBufferSize : capacity;
            if ((sizeHint > growBy || (uint)(growBy += capacity) > (uint)MaxLength) && (uint)(growBy = capacity + sizeHint) > (uint)MaxLength)
                throw new InsufficientMemoryException();

            newSize = growBy;
            return true;
        }

        newSize = default;
        return false;
    }
    
    /// <inheritdoc/>
    public virtual void Dispose()
    {
        position = 0;
    }

    /// <summary>
    /// Gets the textual representation of this buffer.
    /// </summary>
    /// <returns>The textual representation of this buffer.</returns>
    public override string ToString() => WrittenMemory.ToString();
}



/// <summary>
/// Represents memory writer that is backed by the array obtained from the pool.
/// </summary>
/// <remarks>
/// This class provides additional methods for access to array segments in contrast to <see cref="PoolingBufferWriter{T}"/>.
/// </remarks>
/// <typeparam name="T">The data type that can be written.</typeparam>
/// <remarks>
/// Initializes a new writer with the default initial capacity.
/// </remarks>
/// <param name="pool">The array pool.</param>
public sealed class PoolingArrayBufferWriter<T> : BufferWriter<T>, IBufferWriterWithArraySegment<T>
{
    private readonly ArrayPool<T> _pool;
    private T[] _buffer = Array.Empty<T>();

    public PoolingArrayBufferWriter(ArrayPool<T>? pool = null)
    {
        this._pool = pool ?? ArrayPool<T>.Shared;
    }

    public PoolingArrayBufferWriter(int capacity, ArrayPool<T>? pool = null)
        :this(pool)
    {
        _buffer = this._pool.Rent(capacity);
    }
    


    /// <inheritdoc />
    public override int Capacity
    {
        get => _buffer.Length;
    }

    /// <summary>
    /// Gets the data written to the underlying buffer so far.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public override ReadOnlyMemory<T> WrittenMemory
    {
        get
        {
            return new(_buffer, 0, position);
        }
    }

    /// <summary>
    /// Gets the data written to the underlying array so far.
    /// </summary>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public ArraySegment<T> WrittenArray
    {
        get
        {
            return new(_buffer, 0, position);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnBuffer() => _pool.Return(_buffer, RuntimeHelpers.IsReferenceOrContainsReferences<T>());

    /// <summary>
    /// Clears the data written to the underlying memory.
    /// </summary>
    /// <param name="reuseBuffer"><see langword="true"/> to reuse the internal buffer; <see langword="false"/> to destroy the internal buffer.</param>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public override void Clear(bool reuseBuffer = false)
    {
        if (_buffer.Length is 0)
        {
            // nothing to do
        }
        else if (!reuseBuffer)
        {
            ReturnBuffer();
            _buffer = Array.Empty<T>();
        }
        else if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_buffer, 0, position);
        }

        position = 0;
    }

    private T[] GetRawArray(int sizeHint)
    {
        CheckAndResizeBuffer(sizeHint);
        return _buffer;
    }

    /// <summary>
    /// Returns the memory to write to that is at least the requested size.
    /// </summary>
    /// <param name="sizeHint">The minimum length of the returned memory.</param>
    /// <returns>The memory block of at least the size <paramref name="sizeHint"/>.</returns>
    /// <exception cref="OutOfMemoryException">The requested buffer size is not available.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public override Memory<T> GetMemory(int sizeHint = 0)
        => GetRawArray(sizeHint).AsMemory(position);

    /// <summary>
    /// Returns the memory to write to that is at least the requested size.
    /// </summary>
    /// <param name="sizeHint">The minimum length of the returned memory.</param>
    /// <returns>The memory block of at least the size <paramref name="sizeHint"/>.</returns>
    /// <exception cref="OutOfMemoryException">The requested buffer size is not available.</exception>
    /// <exception cref="ObjectDisposedException">This writer has been disposed.</exception>
    public override Span<T> GetSpan(int sizeHint = 0)
    {
        var array = GetRawArray(sizeHint);
        Debug.Assert(position <= array.Length);

        return array.AsSpan(position, array.Length - position);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CopyFast(T[] source, T[] destination, int length)
    {
        Debug.Assert(length <= source.Length);
        Debug.Assert(length <= destination.Length);

        var src = source.AsSpan(0, length); 
        var dest = destination.AsSpan(0, length); 
        src.CopyTo(dest);
    }

    /// <inheritdoc/>
    private protected override void Resize(int newSize)
    {
        var newBuffer = _pool.Rent(newSize);
        if (_buffer.Length > 0U)
        {
            CopyFast(_buffer, newBuffer, position);
            ReturnBuffer();
        }

        _buffer = newBuffer;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (_buffer.Length > 0U)
        {
            ReturnBuffer();
            _buffer = Array.Empty<T>();
        }
        base.Dispose();
    }
}
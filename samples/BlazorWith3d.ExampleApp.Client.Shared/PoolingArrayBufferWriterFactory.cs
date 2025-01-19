namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public class PoolingArrayBufferWriterFactory:IBufferWriterFactory<byte>
    {
        public IBufferWriterWithArraySegment<byte> Create()
        {
            return new PoolingArrayBufferWriter<byte>(100);
        }
    }
}
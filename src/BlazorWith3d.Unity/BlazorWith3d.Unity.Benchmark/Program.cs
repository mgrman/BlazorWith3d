using System.Buffers;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity.Shared;
using MemoryPack;

[SimpleJob(RuntimeMoniker.Net90)]
public class Serialization
{
    private ArrayBufferWriter<byte> writer;

    [GlobalSetup]
    public void Setup()
    {
        writer = new ArrayBufferWriter<byte>(100);
    }

    [Benchmark]
    public byte[] ByteArray() => MemoryPackSerializer.Serialize<PerfCheck>(new  PerfCheck 
    {
        Id = 1009,
        Aaa = 12,
        Bbb = 12.12,
        Ccc = 13.13m,
        Ddd = "https://aaaa.cc/erekwn/sdas?adsasd=asda"
    });

    [Benchmark]
    public byte[] ArrayBufferWriter()
    {
        writer.Clear();
        MemoryPackSerializer.Serialize<PerfCheck, ArrayBufferWriter<byte>>(writer, new PerfCheck
        {
            Id = 1009,
            Aaa = 12,
            Bbb = 12.12,
            Ccc = 13.13m,
            Ddd = "https://aaaa.cc/erekwn/sdas?adsasd=asda"
        });
        return writer.WrittenSpan.ToArray();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Serialization>();
    }
}
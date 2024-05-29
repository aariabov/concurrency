using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser]
public class MyClassVsStructBenchmark
{
    private const int Count = 1_000_000;
    
    [Benchmark]
    public long MyClassCollection()
    {
        var list = new List<MyClass>();
        for (int i = 0; i < Count; i++)
        {
            list.Add(new MyClass(i, i));
        }

        return list.Sum(i => i.Value);
    }
    
    [Benchmark]
    public long MyClassFinalizedCollection()
    {
        var list = new List<MyClassFinalized>();
        for (int i = 0; i < Count; i++)
        {
            list.Add(new MyClassFinalized(i, i));
        }

        return list.Sum(i => i.Value);
    }
    
    [Benchmark]
    public long MyStructCollection()
    {
        var list = new List<MyStruct>();
        for (int i = 0; i < Count; i++)
        {
            list.Add(new MyStruct(i, i));
        }

        return list.Sum(i => i.Value);
    }
    
    record struct MyStruct(long Index, long Value);
    record class MyClass(long Index, long Value);

    class MyClassFinalized
    {
        public long Index { get; set; }
        public long Value { get; set; }

        public MyClassFinalized(long index, long value)
        {
            Index = index;
            Value = value;
        }

        ~MyClassFinalized(){}
    }
}
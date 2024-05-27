using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser]
public class MyClassVsStructBenchmark
{
    private const int Count = 1_000_000;
    
    [Benchmark]
    public async Task MyClassCollection()
    {
        var list = new List<MyClass>();
        foreach (var i in Enumerable.Range(0, Count))
        {
            list.Add(new MyClass(i, i+1));
        }
    }
    
    [Benchmark]
    public async Task MyClassFinalizedCollection()
    {
        var list = new List<MyClassFinalized>();
        foreach (var i in Enumerable.Range(0, Count))
        {
            list.Add(new MyClassFinalized(i, i+1));
        }
    }
    
    [Benchmark]
    public async Task MyStructCollection()
    {
        var list = new List<MyStruct>();
        foreach (var i in Enumerable.Range(0, Count))
        {
            list.Add(new MyStruct(i, i+1));
        }
    }
    
    record struct MyStruct(int Index, int Value);
    record class MyClass(int Index, int Value);

    class MyClassFinalized
    {
        public int Index { get; set; }
        public int Value { get; set; }

        public MyClassFinalized(int index, int value)
        {
            Index = index;
            Value = value;
        }

        ~MyClassFinalized(){}
    }
}
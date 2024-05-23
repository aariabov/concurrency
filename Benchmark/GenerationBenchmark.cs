using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Concurrency;
using Concurrency.Checkers;

namespace Benchmark;

[Config(typeof(FastAndDirtyConfig))]
[MemoryDiagnoser]
public class GenerationBenchmark
{
    private string[] _words;
    private const int WordsCount = 10_000_000;
    private const int ErrorCoefficient = 2;
    
    [GlobalSetup]
    public async Task Setup()
    {
        _words = await SpellChecker.LoadSpellingDictionary();
    }
    
    [Benchmark]
    public async Task SyncGenerator()
    {
        var generator = new SyncGenerator();
        var result = generator.GenerateWords(_words, WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task PlinqGenerator()
    {
        var generator = new PlinqGenerator();
        var result = generator.GenerateWords(_words, WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithLockGenerator()
    {
        var generator = new ParallelWithLockGenerator();
        var result = generator.GenerateWords(_words, WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithBlockingCollectionGenerator()
    {
        var generator = new ParallelWithBlockingCollectionGenerator();
        var result = generator.GenerateWords(_words, WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithConcurrentBagGenerator()
    {
        var generator = new ParallelWithConcurrentBagGenerator();
        var result = generator.GenerateWords(_words, WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
}
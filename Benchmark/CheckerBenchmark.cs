using BenchmarkDotNet.Attributes;
using Concurrency;
using Concurrency.Checkers;

namespace Benchmark;

[Config(typeof(FastAndDirtyConfig))]
[MemoryDiagnoser]
public class CheckerBenchmark
{
    private const int WordsCount = 10_000_000;
    private const int ErrorCoefficient = 2;
    
    [Benchmark]
    public async Task SyncArrayChecker()
    {
        var generator = new SyncGenerator();
        var checker = new SyncArrayChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(10_000, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task SyncListChecker()
    {
        var generator = new SyncGenerator();
        var checker = new SyncListChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(10_000, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task SyncChecker()
    {
        var generator = new SyncGenerator();
        var checker = new SyncChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task PlinqChecker()
    {
        var generator = new PlinqGenerator();
        var checker = new PlinqChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithLockChecker()
    {
        var generator = new ParallelWithLockGenerator();
        var checker = new ParallelWithLockChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithConcurrentBagChecker()
    {
        var generator = new ParallelWithConcurrentBagGenerator();
        var checker = new ParallelWithConcurrentBagChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
    
    [Benchmark]
    public async Task ParallelWithBlockingCollectionChecker()
    {
        var generator = new ParallelWithBlockingCollectionGenerator();
        var checker = new ParallelWithBlockingCollectionChecker();
        var spellChecker = new SpellChecker(generator, checker);
        var result = spellChecker.Check(WordsCount, ErrorCoefficient);
        Console.WriteLine($"Generated words count: {result.Length}");
    }
}
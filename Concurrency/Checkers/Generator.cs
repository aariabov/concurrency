using System.Collections.Concurrent;

namespace Concurrency.Checkers;

public abstract class GeneratorBase
{
    private readonly Random _random = new Random();

    public abstract string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient);

    protected string GenerateWord(string[] words, int errorCoefficient)
    {
        var randomIndex = _random.Next(0, words.Length);
        var word = words[randomIndex];
        if (randomIndex % errorCoefficient == 0)
        {
            word = $"{word}error";
        }

        return word;
    }
}

public class SyncGenerator : GeneratorBase
{
    public override string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient)
    {
        var wordsToTest = new List<string>();
        for (int i = 0; i < wordsCount; i++)
        {
            var word = GenerateWord(words, errorCoefficient);
            wordsToTest.Add(word);
        }

        return wordsToTest.ToArray();
    }
}

public class PlinqGenerator : GeneratorBase
{
    public override string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient)
    {
        var wordsToTest = ParallelEnumerable
            .Range(0, wordsCount)
            .Select(i =>
            {
                var word = GenerateWord(words, errorCoefficient);
                return word;
            })
            .ToArray();

        if (wordsToTest.Length != wordsCount)
        {
            throw new Exception($"Ожидалось, что сгенерируется {wordsCount}, но получилось {wordsToTest.Length}");
        }

        return wordsToTest;
    }
}

public class ParallelWithLockGenerator : GeneratorBase
{
    public override string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient)
    {
        var wordsToTest = new List<string>();
        Parallel.For(
            0,
            wordsCount,
            i =>
            {
                var word = GenerateWord(words, errorCoefficient);
                lock (wordsToTest)
                    wordsToTest.Add(word);
            }
        );
        
        if (wordsToTest.Count != wordsCount)
        {
            throw new Exception($"Ожидалось, что сгенерируется {wordsCount}, но получилось {wordsToTest.Count}");
        }

        return wordsToTest.ToArray();
    }
}

public class ParallelWithConcurrentBagGenerator : GeneratorBase
{
    public override string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient)
    {
        var wordsToTest = new ConcurrentBag<string>();
        Parallel.For(
            0,
            wordsCount,
            i =>
            {
                var word = GenerateWord(words, errorCoefficient);
                wordsToTest.Add(word);
            }
        );
        
        if (wordsToTest.Count != wordsCount)
        {
            throw new Exception($"Ожидалось, что сгенерируется {wordsCount}, но получилось {wordsToTest.Count}");
        }
        
        return wordsToTest.ToArray();
    }
}

public class ParallelWithBlockingCollectionGenerator : GeneratorBase
{
    public override string[] GenerateWords(string[] words, int wordsCount, int errorCoefficient)
    {
        var wordsToTest = new BlockingCollection<string>();
        Parallel.For(
            0,
            wordsCount,
            i =>
            {
                var word = GenerateWord(words, errorCoefficient);
                wordsToTest.Add(word);
            }
        );
        
        if (wordsToTest.Count != wordsCount)
        {
            throw new Exception($"Ожидалось, что сгенерируется {wordsCount}, но получилось {wordsToTest.Count}");
        }
        
        return wordsToTest.ToArray();
    }
}

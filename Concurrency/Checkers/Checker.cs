using System.Collections.Concurrent;

namespace Concurrency.Checkers;

public interface IChecker
{
    int[] GetErrorWordIndexes(string[] wordsToTest, string[] words);
}

/// <summary>
/// Медленный чекер, тк array.Contains(word)
/// </summary>
public class SyncArrayChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] words)
    {
        var errorWordIndexes = new List<int>();
        for (var i = 0; i < wordsToTest.Length; i++)
        {
            var word = wordsToTest[i];
            if (!words.Contains(word))
            {
                errorWordIndexes.Add(i);
            }
        }

        return errorWordIndexes.ToArray();
    }
}

/// <summary>
/// Медленный чекер, тк list.Contains(word)
/// </summary>
public class SyncListChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = wordsArr.ToList();
        var errorWordIndexes = new List<int>();
        for (var i = 0; i < wordsToTest.Length; i++)
        {
            var word = wordsToTest[i];
            if (!words.Contains(word))
            {
                errorWordIndexes.Add(i);
            }
        }

        return errorWordIndexes.ToArray();
    }
}

public class SyncChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = new HashSet<string>(wordsArr);
        var errorWordIndexes = new List<int>();
        for (var i = 0; i < wordsToTest.Length; i++)
        {
            var word = wordsToTest[i];
            if (!words.Contains(word))
            {
                errorWordIndexes.Add(i);
            }
        }

        return errorWordIndexes.ToArray();
    }
}

// TODO: проверить правильность индексов
public class PlinqChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = new HashSet<string>(wordsArr);
        return wordsToTest
            .AsParallel()
            .Select((word, idx) => (word, idx))
            .Where(tuple => !words.Contains(tuple.word))
            .Select(tuple => tuple.idx)
            .ToArray();
    }
}

public class ParallelWithLockChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = new HashSet<string>(wordsArr);
        var errorWordIndexes = new List<int>();
        Parallel.For(
            0,
            wordsToTest.Length,
            i =>
            {
                var word = wordsToTest[i];
                if (!words.Contains(word))
                {
                    lock (errorWordIndexes)
                        errorWordIndexes.Add(i);
                }
            }
        );

        return errorWordIndexes.ToArray();
    }
}

public class ParallelWithConcurrentBagChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = new HashSet<string>(wordsArr);
        var errorWordIndexes = new ConcurrentBag<int>();
        Parallel.For(
            0,
            wordsToTest.Length,
            i =>
            {
                var word = wordsToTest[i];
                if (!words.Contains(word))
                {
                    lock (errorWordIndexes)
                        errorWordIndexes.Add(i);
                }
            }
        );

        return errorWordIndexes.ToArray();
    }
}

public class ParallelWithBlockingCollectionChecker : IChecker
{
    public int[] GetErrorWordIndexes(string[] wordsToTest, string[] wordsArr)
    {
        var words = new HashSet<string>(wordsArr);
        var errorWordIndexes = new BlockingCollection<int>();
        Parallel.For(
            0,
            wordsToTest.Length,
            i =>
            {
                var word = wordsToTest[i];
                if (!words.Contains(word))
                {
                    lock (errorWordIndexes)
                        errorWordIndexes.Add(i);
                }
            }
        );

        return errorWordIndexes.ToArray();
    }
}
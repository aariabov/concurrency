using System.Collections.Concurrent;
using Concurrency.Checkers;

namespace Concurrency;

public class SpellChecker
{
    private static string[] words;
    private readonly GeneratorBase _generator;
    private readonly IChecker _checker;

    public SpellChecker(GeneratorBase generator, IChecker checker)
    {
        _generator = generator;
        _checker = checker;
        words = LoadSpellingDictionary().Result;
    }

    public int[] Check(int wordsCount, int errorCoefficient)
    {
        var wordsToTest = _generator.GenerateWords(words, wordsCount, errorCoefficient);
        var errorWords = _checker.GetErrorWordIndexes(wordsToTest, words);
        return errorWords;
    }

    public static async Task<string[]> LoadSpellingDictionary()
    {
        var wordLookupFile = Path.Combine(Path.GetTempPath(), "WordLookup.txt");
        if (File.Exists(wordLookupFile))
        {
            return await File.ReadAllLinesAsync(wordLookupFile);
        }
        
        using var client = new HttpClient();
        const string link = "http://www.albahari.com/ispell/allwords.txt"; // Contains about 150,000 words
        await using var stream = await client.GetStreamAsync(link);
        await using var file = File.Create(wordLookupFile);
        await stream.CopyToAsync(file);
        return await File.ReadAllLinesAsync(wordLookupFile);
    }
}

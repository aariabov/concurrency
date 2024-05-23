using System.Diagnostics;
using System.Net;

namespace Tests.Albahari._22ParallelProgramming;

[TestClass]
public class _1Plinq
{
    [TestMethod]
    public async Task plinq_prime_numbers()
    {
        IEnumerable<int> numbers = Enumerable.Range(3, 1000000 - 3);

        var parallelQuery =
            from n in numbers.AsParallel()
            where Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0)
            select n;

        int[] primes = parallelQuery.ToArray();
        Console.WriteLine($"Total prime numbers: {primes.Length}");
        Console.WriteLine($"First 100: {string.Join(", ", primes.Take(100))}");
    }

    [TestMethod]
    public async Task plinq_prime_numbers_ordered()
    {
        IEnumerable<int> numbers = Enumerable.Range(3, 1000000 - 3);

        var parallelQuery =
            from n in numbers.AsParallel().AsOrdered()
            where Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0)
            select n;

        int[] primes = parallelQuery.ToArray();
        Console.WriteLine($"Total prime numbers: {primes.Length}");
        Console.WriteLine($"First 100: {string.Join(", ", primes.Take(100))}");
    }

    private static readonly string WordLookupFile = Path.Combine(
        Path.GetTempPath(),
        "WordLookup.txt"
    );

    // проверка орфографии по словарю, словарь скачаем, а док сгенерим из слов словаря
    [TestMethod]
    public async Task spell_checker()
    {
        await LoadSpellingDictionary();
        var allWords = await File.ReadAllLinesAsync(WordLookupFile);
        var wordsHashSet = new HashSet<string>(allWords);
        var wordsToTest = new List<string>();
        var random = new Random();
        var watch = Stopwatch.StartNew();
        for (int i = 0; i < 10_000_000; i++)
        {
            var randomIndex = random.Next(0, allWords.Length);
            var word = allWords[randomIndex];
            wordsToTest.Add(word);
        }
        wordsToTest[12345] = "woozsh";
        wordsToTest[23456] = "wubsie";

        for (int i = 0; i < wordsToTest.Count; i++)
        {
            var word = wordsToTest[i];
            if (!wordsHashSet.Contains(word))
            {
                Console.WriteLine(word);
            }
        }
        Console.WriteLine($"time: {watch.ElapsedMilliseconds}");
    }

    private static async Task LoadSpellingDictionary()
    {
        if (!File.Exists(WordLookupFile))
        {
            using var client = new HttpClient();
            const string link = "http://www.albahari.com/ispell/allwords.txt"; // Contains about 150,000 words
            await using var stream = await client.GetStreamAsync(link);
            await using var file = File.Create(WordLookupFile);
            await stream.CopyToAsync(file);
        }
    }

    [TestMethod]
    public async Task parallel_spell_checker()
    {
        string wordLookupFile = Path.Combine(Path.GetTempPath(), "WordLookup.txt");

        if (!File.Exists(wordLookupFile)) // Contains about 150,000 words
            new WebClient().DownloadFile("http://www.albahari.com/ispell/allwords.txt", wordLookupFile);

        var wordLookup = new HashSet<string>(
            File.ReadAllLines(wordLookupFile),
            StringComparer.InvariantCultureIgnoreCase
        );

        string[] wordList = wordLookup.ToArray();

        var localRandom = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));
        var watch = Stopwatch.StartNew();
        string[] wordsToTest = Enumerable
            .Range(0, 10_000_000)
            .AsParallel()
            .Select(i => wordList[localRandom.Value.Next(0, wordList.Length)])
            .ToArray();

        wordsToTest[12345] = "woozsh"; // Introduce a couple
        wordsToTest[23456] = "wubsie"; // of spelling mistakes.

        var query = wordsToTest
            .AsParallel()
            .Select((word, index) => new IndexedWord { Word = word, Index = index })
            .Where(iword => !wordLookup.Contains(iword.Word))
            .OrderBy(iword => iword.Index);

        foreach (var indexedWord in query)
        {
            Console.WriteLine($"{indexedWord.Word} - index = {indexedWord.Index}");
        }
        Console.WriteLine($"time: {watch.ElapsedMilliseconds}");
    }

    struct IndexedWord
    {
        public string Word;
        public int Index;
    }
}

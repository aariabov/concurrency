using System.Collections.Concurrent;
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

    [TestMethod]
    public async Task parallel_unsafe_problem()
    {
        int i = 0;
        (from n in Enumerable.Range(0, 999).AsParallel() select n * i++).ToArray();
        Console.WriteLine(i);
    }

    [TestMethod]
    public async Task parallel_safe()
    {
        var i = (Enumerable.Range(0, 999).AsParallel().Select((n, i) => i)).Max();
        Console.WriteLine(i);
    }

    [TestMethod]
    public async Task WithDegreeOfParallelism()
    {
        var result = "The Quick Brown Fox"
            .AsParallel()
            .AsOrdered()
            .WithDegreeOfParallelism(3) // Forces Merge + Partition
            .Select(c => char.ToUpper(c));
        Console.WriteLine(string.Join(null, result));
    }

    [TestMethod]
    public async Task Cancellation()
    {
        IEnumerable<int> million = Enumerable.Range(3, 1000000);

        var cancelSource = new CancellationTokenSource();

        var primeNumberQuery =
            from n in million.AsParallel().WithCancellation(cancelSource.Token)
            where Enumerable.Range(2, (int)Math.Sqrt(n)).All(i => n % i > 0)
            select n;

        new Thread(() =>
        {
            Thread.Sleep(100); // Cancel query after
            cancelSource.Cancel(); // 100 milliseconds.
        }).Start();
        try
        {
            // Start query running:
            int[] primes = primeNumberQuery.ToArray();
            // We'll never get here because the other thread will cancel us.
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Query canceled");
        }
    }

    [TestMethod]
    public async Task for_all()
    {
        "abcdef".AsParallel().Select(c => char.ToUpper(c)).ForAll(Console.Write);
    }

    [TestMethod]
    public async Task partitioner_example()
    {
        int[] numbers = { 3, 4, 5, 6, 7, 8, 9 };

        var parallelQuery = Partitioner.Create(numbers, true).AsParallel().Where(n => n % 2 == 0);

        Console.WriteLine(string.Join(" ", parallelQuery.ToArray()));
    }

    [TestMethod]
    public async Task aggregate_sync()
    {
        int[] numbers = { 1, 2, 3 };
        int sum = numbers.Aggregate(0, (total, n) => total + n); // 6
        Console.WriteLine(sum);
    }

    [TestMethod]
    public async Task aggregate_parallel()
    {
        int[] numbers = { 1, 2, 3 };
        int sum = numbers
            .AsParallel()
            .Aggregate(
                () => 0, // seedFactory
                (localTotal, n) => localTotal + n, // updateAccumulatorFunc
                (mainTot, localTot) => mainTot + localTot, // combineAccumulatorFunc
                finalResult => finalResult
            );
        Console.WriteLine(sum);
    }

    [TestMethod]
    public async Task letter_frequencies_sync()
    {
        string text = "Let’s suppose this is a really long string";
        var letterFrequencies = new int[26];
        foreach (char c in text)
        {
            int index = char.ToUpper(c) - 'A';
            if (index >= 0 && index < 26)
                letterFrequencies[index]++;
        }

        for (int i = 0; i < letterFrequencies.Length; i++)
        {
            var chr = (char)(i + 'A');
            Console.WriteLine($"{chr} - {letterFrequencies[i]}");
        }
    }

    [TestMethod]
    public async Task letter_frequencies_sync_aggregate()
    {
        string text = "Let’s suppose this is a really long string";

        int[] letterFrequencies = text.Aggregate(
            new int[26], // Create the "accumulator"
            (letterFrequencies, c) => // Aggregate a letter into the accumulator
            {
                int index = char.ToUpper(c) - 'A';
                if (index >= 0 && index < 26)
                    letterFrequencies[index]++;
                return letterFrequencies;
            }
        );

        for (int i = 0; i < letterFrequencies.Length; i++)
        {
            var chr = (char)(i + 'A');
            Console.WriteLine($"{chr} - {letterFrequencies[i]}");
        }
    }

    [TestMethod]
    public async Task letter_frequencies_parallel_aggregate()
    {
        string text = "Let’s suppose this is a really long string";

        int[] letterFrequencies = text.AsParallel()
            .Aggregate(
                () => new int[26], // Create a new local accumulator
                (localFrequencies, c) => // Aggregate into the local accumulator
                {
                    int index = char.ToUpper(c) - 'A';
                    if (index >= 0 && index < 26)
                        localFrequencies[index]++;
                    return localFrequencies;
                },
                // Aggregate local->main accumulator
                (mainFreq, localFreq) => mainFreq.Zip(localFreq, (f1, f2) => f1 + f2).ToArray(),
                finalResult => finalResult // Perform any final transformation
            );

        for (int i = 0; i < letterFrequencies.Length; i++)
        {
            var chr = (char)(i + 'A');
            Console.WriteLine($"{chr} - {letterFrequencies[i]}");
        }
    }
}

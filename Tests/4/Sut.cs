namespace Tests._4;

public class Sut
{
    public long[] FindPrimeNumbers(int[] numbers)
    {
        var result = new List<long>();
        foreach (var number in numbers)
        {
            result.Add(FindPrimeNumber(number));
        }

        return result.ToArray();
    }
    
    public long[] FindPrimeNumbersParallel(int[] numbers)
    {
        var result = new List<long>();
        var threads = new HashSet<int>();
        Parallel.ForEach(numbers, number =>
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            threads.Add(threadId);
            result.Add(FindPrimeNumber(number));
        });
        Console.WriteLine($"Used thread ids: {string.Join(',', threads)}");
        return result.ToArray();
    }
    
    public long[] FindPrimeNumbersParallelWithBreak(int[] numbers)
    {
        var result = new List<long>();
        Parallel.ForEach(numbers, (number, state) =>
        {
            if (number > 1500)
            {
                state.Stop();
            }
            else
            {
                result.Add(FindPrimeNumber(number));
            }
        });
        return result.ToArray();
    }
    
    public long[] FindPrimeNumbersParallelWithCancel(int[] numbers, CancellationToken cancellationToken)
    {
        var result = new List<long>();
        Parallel.ForEach(numbers, new ParallelOptions{CancellationToken = cancellationToken}, number =>
        {
            result.Add(FindPrimeNumber(number));
        });
        return result.ToArray();
    }
    
    public long[] FindPrimeNumbersParallelWithLock(int[] numbers)
    {
        var result = new List<long>();
        var mutex = new object();
        var counter = 0;

        Parallel.ForEach(numbers, number =>
        {
            lock (mutex)
            {
                counter++;
                result.Add(FindPrimeNumber(number));
            }
        });
        Console.WriteLine($"counter: {counter}");
        return result.ToArray();
    }

    /// <summary>
    /// Находит n-ое простое число
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private long FindPrimeNumber(int n)
    {
        int count=0;
        long a = 2;
        while(count<n)
        {
            long b = 2;
            int prime = 1;// to check if found a prime
            while(b * b <= a)
            {
                if(a % b == 0)
                {
                    prime = 0;
                    break;
                }
                b++;
            }
            if(prime > 0)
            {
                count++;
            }
            a++;
        }
        return (--a);
    }
}
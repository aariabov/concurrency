﻿namespace Tests._4;

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
    
    public long SumOfPrimeNumbersParallel(int[] numbers)
    {
        object mutex = new object();
        long result = 0; // общая сумма
        Parallel.ForEach(source: numbers,
            localInit: () => (long)0, // localValue - локальная переменная
            body: (item, state, localValue) => localValue + FindPrimeNumber(item),
            localFinally: localValue =>
            {
                lock (mutex)
                    result += localValue;
            });
        return result;
    }
    
    public long SumOfPrimeNumbersParallelLinq(int[] numbers)
    {
        return numbers.Select(n => FindPrimeNumber(n)).AsParallel().Sum();
    }
    
    public long SumOfPrimeNumbersParallelLinq1(int[] numbers)
    {
        object mutex = new object();
        long result = 0;
        Parallel.Invoke(
            () =>
            {
                var sum = SumOfPrimeNumbersParallelLinq(numbers.Take(numbers.Length / 2).ToArray());
                lock (mutex)
                {
                    result += sum;
                }
            },
            () =>
            {
                var sum = SumOfPrimeNumbersParallelLinq(numbers.Skip(numbers.Length / 2).ToArray());
                lock (mutex)
                {
                    result += sum;
                }
            }
            );
        return result;
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
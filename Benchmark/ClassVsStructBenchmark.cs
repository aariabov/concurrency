using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser]
public class ClassVsStructBenchmark
{
    private readonly string _sampleString = new('a', 1000);
    private const int A = 1;
    private const int B = 10;

    private const int Rows = 100;

    [Benchmark]
    public int SmallStruct()
    {
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            SmallStruct test = new(A, B);
            sum = sum + Calc(test);
        }
        return sum;
    }

    [Benchmark]
    public int MediumStruct()
    {
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            MediumStruct test = new(A, B, _sampleString, _sampleString);
            sum = sum + Calc(test);
        }
        return sum;
    }

    [Benchmark]
    public int SmallClass()
    {
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            SmallClass test = new(A, B);
            sum = sum + Calc(test);
        }
        return sum;
    }

    [Benchmark]
    public int MediumClass()
    {
        int sum = 0;
        for (int i = 0; i < Rows; i++)
        {
            MediumClass test = new(A, B, _sampleString, _sampleString);
            sum = sum + Calc(test);
        }
        return sum;
    }

    private int Calc(SmallStruct input) => input.A + input.B;
    private int Calc(MediumStruct input) => input.A + input.B;
    private int Calc(SmallClass input) => input.A + input.B;
    private int Calc(MediumClass input) => input.A + input.B;
}

public readonly record struct SmallStruct(int A, int B);
public readonly record struct MediumStruct(int A, int B, string X, string Y);

public record class SmallClass(int A, int B);
public record class MediumClass(int A, int B, string X, string Y);
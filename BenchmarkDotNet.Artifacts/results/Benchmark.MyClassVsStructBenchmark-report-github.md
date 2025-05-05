```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.5737/22H2/2022Update)
12th Gen Intel Core i7-12700K, 1 CPU, 20 logical and 12 physical cores
.NET SDK 6.0.428
  [Host]     : .NET 6.0.36 (6.0.3624.51421), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.36 (6.0.3624.51421), X64 RyuJIT AVX2


```
| Method                     | Mean     | Error    | StdDev   | Gen0      | Gen1      | Gen2      | Allocated |
|--------------------------- |---------:|---------:|---------:|----------:|----------:|----------:|----------:|
| MyClassCollection          | 51.63 ms | 0.854 ms | 0.799 ms | 3400.0000 | 2300.0000 | 1300.0000 |  46.52 MB |
| MyClassFinalizedCollection | 88.14 ms | 1.710 ms | 2.100 ms | 3000.0000 | 1833.3333 |  666.6667 |  46.52 MB |
| MyStructCollection         | 16.38 ms | 0.047 ms | 0.036 ms | 1484.3750 | 1484.3750 | 1484.3750 |     32 MB |

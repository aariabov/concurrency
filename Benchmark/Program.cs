// See https://aka.ms/new-console-template for more information

using Benchmark;
using BenchmarkDotNet.Running;
using Concurrency;

Console.WriteLine("Hello, World!");
var summary = BenchmarkRunner.Run<CheckerBenchmark>();
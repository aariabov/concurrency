// See https://aka.ms/new-console-template for more information

using Benchmark;
using BenchmarkDotNet.Running;
using Concurrency;

Console.WriteLine("Hello, World!");
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

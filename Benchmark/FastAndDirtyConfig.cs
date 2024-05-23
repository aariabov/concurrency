using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Benchmark;

public class FastAndDirtyConfig : ManualConfig
{
    public FastAndDirtyConfig()
    {
        AddJob(new Job("FastAndDirtyJob")
            {
                Run =
                {
                    RunStrategy = RunStrategy.ColdStart,
                    LaunchCount = 1,
                    WarmupCount = 0,
                    IterationCount = 1
                }
            }
            .WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}
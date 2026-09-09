using BenchmarkDotNet.Attributes;

namespace UnicornHack.PerformanceTests;

public class CalibrationPerfTest
{
    private const int OperationCount = 4_000_000;

    [Benchmark]
    public ulong Integer_workload()
    {
        var value = 0x9E3779B97F4A7C15UL;
        for (var operation = 0; operation < OperationCount; operation++)
        {
            value = (value * 6364136223846793005UL + (ulong)operation) ^ (value >> 29);
        }

        return value;
    }
}
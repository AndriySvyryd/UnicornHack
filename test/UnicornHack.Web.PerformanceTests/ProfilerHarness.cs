namespace UnicornHack.Web.PerformanceTests;

/// <summary>
///     CPU-profiling harness. Not a benchmark — runs the <c>WaitTurn_Delta</c> scenario
///     in a tight loop so an external profiler (<c>dotnet-trace</c>, PerfView, VS profiler)
///     can attach and capture a sample profile. Invoked from the command line, e.g.
///     <c>dotnet run -c Release -- --profile-delta 5000</c>.
/// </summary>
public static class ProfilerHarness
{
    public static void RunDeltaLoop(int iterations, int npcCount)
    {
        var bench = new ChangeDetectionPerfTest { NpcCount = npcCount };
        bench.Setup();

        // Warmup so JIT doesn't pollute the trace.
        for (var i = 0; i < 100; i++)
        {
            bench.WaitTurn_Delta();
            bench.Setup();
        }

        Console.WriteLine($"[harness] Starting {iterations} Delta iterations (NpcCount={npcCount})");
        var start = Environment.TickCount64;
        for (var i = 0; i < iterations; i++)
        {
            bench.WaitTurn_Delta();
            bench.Setup();
        }
        Console.WriteLine($"[harness] Done in {Environment.TickCount64 - start} ms");
    }

    public static void RunFullSnapshotLoop(int iterations, int npcCount)
    {
        var bench = new ChangeDetectionPerfTest { NpcCount = npcCount };
        bench.Setup();

        for (var i = 0; i < 100; i++)
        {
            bench.WaitTurn_FullSnapshot();
            bench.Setup();
        }

        Console.WriteLine($"[harness] Starting {iterations} FullSnapshot iterations (NpcCount={npcCount})");
        var start = Environment.TickCount64;
        for (var i = 0; i < iterations; i++)
        {
            bench.WaitTurn_FullSnapshot();
            bench.Setup();
        }
        Console.WriteLine($"[harness] Done in {Environment.TickCount64 - start} ms");
    }
}

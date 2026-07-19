using BenchmarkDotNet.Attributes;

namespace UnicornHack.Web.PerformanceTests;

[MemoryDiagnoser]
public class PersistenceReloadPerfTest
{
    private PersistedGame _persisted = null!;

    [GlobalSetup]
    public void Setup()
    {
        if (!PersistedGame.IsLocalDbAvailable)
        {
            throw new InvalidOperationException("SQL Server LocalDB is required for this benchmark.");
        }

        _persisted = PersistedGame.CreateAndSave();
    }

    [Benchmark]
    public void Reload_game()
        => _persisted.Reload();

    [GlobalCleanup]
    public void Cleanup()
        => _persisted.Dispose();
}

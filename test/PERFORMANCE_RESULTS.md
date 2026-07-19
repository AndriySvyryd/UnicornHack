# Performance Results

## Complete suite, three-run stability check

Measured with BenchmarkDotNet 0.15.8, .NET 10.0.11, x64 RyuJIT, concurrent workstation GC, and `ShortRun` (3 warmups, 3 measured iterations, 1 launch). Hardware: Intel Core i9-10850K, 10 physical/20 logical cores, Windows 11 25H2.

Each complete Core and Web suite was run three times sequentially to avoid CPU and GC interference. Times below are BenchmarkDotNet means per benchmark invocation; some legacy benchmarks intentionally perform many operations per invocation. Allocations are the three-run average per invocation.

Commands used for each run:

```powershell
dotnet run -c Release --no-build --project test/UnicornHack.Core.PerformanceTests/UnicornHack.Core.PerformanceTests.csproj -- --filter '*' --job short
dotnet run -c Release --no-build --project test/UnicornHack.Web.PerformanceTests/UnicornHack.Web.PerformanceTests.csproj -- --filter '*'
```

CI checks machine-independent regression ratios after both benchmark suites complete:

```powershell
test/Test-PerformanceThresholds.ps1 `
    -CoreResultsPath <core-artifacts> `
    -WebResultsPath <web-artifacts>
```

The effect checks compare 64 active effects with 16. Property resolution may scale by at most 6x; status application, immunity lookup, and
movement-reactive extension may scale by at most 2x. At 64 NPCs, delta change tracking may take at most 1.25x the corresponding full-snapshot
time and allocate at most 65%; delta allocations may grow by at most 3x from 24 to 64 NPCs. Ratios avoid absolute timing gates that vary with CI
hardware while still catching lost indexing, accidental full serialization, and superlinear allocation regressions.

### Core

| Benchmark | Parameters | Run 1 | Run 2 | Run 3 | 3-run mean | CV | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| BeveledVisibilityCalculator / Mixed terrain directional FOV | - | 120.29 ms | 122.90 ms | 122.43 ms | 121.87 ms | 0.93% | 2702.03 KB |
| BeveledVisibilityCalculator / Mixed terrain LOS | - | 118.34 ms | 119.93 ms | 118.98 ms | 119.08 ms | 0.55% | 2702.86 KB |
| BeveledVisibilityCalculator / Mixed terrain omnidirectional FOV | - | 125.56 ms | 124.81 ms | 124.44 ms | 124.94 ms | 0.37% | 2700.41 KB |
| EffectSystem / Process actor turn | EffectCount=16 | 1.90 us | 2.00 us | 1.90 us | 1.93 us | 2.44% | 0.60 KB |
| EffectSystem / Process actor turn | EffectCount=64 | 1.90 us | 1.90 us | 1.90 us | 1.90 us | 0.00% | 0.60 KB |
| EffectSystem / Status-heavy expiry scan | EffectCount=16 | 100.0 ns | 100.0 ns | 200.0 ns | 133.3 ns | 35.36%* | 0.08 KB |
| EffectSystem / Status-heavy expiry scan | EffectCount=64 | 300.0 ns | 300.0 ns | 300.0 ns | 300.0 ns | 0.00% | 0.08 KB |
| EffectSystem / Resolve property | EffectCount=16 | 300.0 ns | 300.0 ns | 300.0 ns | 300.0 ns | 0.00% | 0 KB |
| EffectSystem / Resolve property | EffectCount=64 | 1.00 us | 1.00 us | 1.10 us | 1.03 us | 4.56% | 0 KB |
| Entity / AddRemoveComponent | EntityCount=1,000,000 | 320.07 ms | 321.54 ms | 328.38 ms | 323.33 ms | 1.12% | 0 KB |
| Entity / FindComponent | EntityCount=1,000,000 | 7.34 ms | 7.70 ms | 7.53 ms | 7.52 ms | 1.98% | 0 KB |
| Entity / HasComponent | EntityCount=1,000,000 | 8.15 ms | 7.47 ms | 7.70 ms | 7.77 ms | 3.66% | 0 KB |
| EntityRelationship / CollectionAdd | EntityCount=10,000 | 17.40 us | 17.80 us | 17.80 us | 17.67 us | 1.07% | 0 KB |
| EntityRelationship / CollectionCompositeUpdate | EntityCount=10,000 | 62.00 us | 62.50 us | 62.30 us | 62.27 us | 0.33% | 0 KB |
| EntityRelationship / CollectionRemove | EntityCount=10,000 | 16.40 us | 16.80 us | 16.80 us | 16.67 us | 1.13% | 0 KB |
| EntityRelationship / CollectionUpdate | EntityCount=10,000 | 23.50 us | 23.40 us | 24.20 us | 23.70 us | 1.50% | 0 KB |
| PathFinder / Multiple paths | Iterations=10,000 | 123.58 ms | 124.56 ms | 125.23 ms | 124.45 ms | 0.54% | 3267.94 KB |
| SoundPropagation / One listener | RoomSize=15 | 7.40 us | 7.40 us | 7.30 us | 7.37 us | 0.64% | 1.13 KB |
| SoundPropagation / One listener | RoomSize=31 | 39.40 us | 39.80 us | 40.50 us | 39.90 us | 1.14% | 2.16 KB |
| SoundPropagation / Eight listeners, one map | RoomSize=15 | 9.10 us | 9.30 us | 9.20 us | 9.20 us | 0.89% | 1.18 KB |
| SoundPropagation / Eight listeners, one map | RoomSize=31 | 44.30 us | 48.80 us | 45.00 us | 46.03 us | 4.29% | 2.20 KB |

`*` The joined CSV exports sub-microsecond means with only 0.1 us precision. The 0.1/0.1/0.2 us values therefore cannot support a meaningful cross-run CV; the individual BenchmarkDotNet reports measured this case with nanosecond precision and low within-run variation.

### Web

| Benchmark | Parameters | Run 1 | Run 2 | Run 3 | 3-run mean | CV | Allocated |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| ChangeDetection / Move delta | NpcCount=8 | 4.41 ms | 4.62 ms | 4.27 ms | 4.43 ms | 3.23% | 332.08 KB |
| ChangeDetection / Move full snapshot | NpcCount=8 | 3.99 ms | 4.17 ms | 3.95 ms | 4.04 ms | 2.40% | 654.98 KB |
| ChangeDetection / Wait delta | NpcCount=8 | 3.22 ms | 3.32 ms | 3.30 ms | 3.28 ms | 1.34% | 177.69 KB |
| ChangeDetection / Wait full snapshot | NpcCount=8 | 4.09 ms | 4.39 ms | 3.99 ms | 4.16 ms | 4.03% | 398.14 KB |
| ChangeDetection / Move delta | NpcCount=24 | 3.52 ms | 3.22 ms | 3.15 ms | 3.29 ms | 4.90% | 671.14 KB |
| ChangeDetection / Move full snapshot | NpcCount=24 | 3.60 ms | 3.69 ms | 3.49 ms | 3.59 ms | 2.32% | 1456.36 KB |
| ChangeDetection / Wait delta | NpcCount=24 | 2.09 ms | 2.25 ms | 1.94 ms | 2.09 ms | 6.02% | 197.42 KB |
| ChangeDetection / Wait full snapshot | NpcCount=24 | 1.58 ms | 1.65 ms | 1.73 ms | 1.65 ms | 3.88% | 402.14 KB |
| PersistenceReload / Reload game | Seeded LocalDB game | 47.69 ms | 48.66 ms | 48.90 ms | 48.42 ms | 1.08% | 55469.82 KB |

### Stability

- 30 parameter-expanded benchmark cases were measured in each run: 21 Core and 9 Web.
- 28/30 cases have cross-run CV at or below 5%.
- The 16-effect expiry scan's reported 35.36% CV is an export-precision artifact described above.
- The only materially unstable timing is `WaitTurn_Delta` with 24 NPCs: 6.02% CV and 15.92% min/max spread. Treat changes smaller than that spread as noise unless confirmed with a longer job.
- Allocation results are stable. The largest relative allocation spread is 0.172%; persistence reload varies by 0.010%.

Raw results and the reproducible aggregation script are stored under [performance-results/2026-08-15](performance-results/2026-08-15) and [Summarize-PerformanceRuns.ps1](performance-results/Summarize-PerformanceRuns.ps1).

## Effect and hearing optimization comparison

These measurements were taken before and after the effect-index and shared-sound-map optimizations using the same runtime and `ShortRun` job.

| Benchmark | Size | Baseline | Baseline alloc | Optimized | Optimized alloc |
| --- | ---: | ---: | ---: | ---: | ---: |
| Resolve property | 16 effects | 0.36 us | 0.04 KB | 0.30 us | 0 KB |
| Resolve property | 64 effects | 1.20 us | 0.04 KB | 1.00 us | 0 KB |
| Status-heavy expiry scan | 16 effects | 0.18 us | 0.39 KB | 0.10 us | 0.08 KB |
| Status-heavy expiry scan | 64 effects | 0.37 us | 0.77 KB | 0.30 us | 0.08 KB |
| Actor turn | 16 effects | 2.04 us | 0.80 KB | 1.90 us | 0.60 KB |
| Actor turn | 64 effects | 2.28 us | 1.12 KB | 1.90 us | 0.60 KB |
| One-listener sound | 15x15 room | 7.36 us | 1.13 KB | 7.40 us | 1.13 KB |
| One-listener sound | 31x31 room | 40.60 us | 2.16 KB | 40.30 us | 2.16 KB |
| Eight listeners, one map | 15x15 room | 58.90 us* | 9.04 KB* | 9.20 us | 1.18 KB |
| Eight listeners, one map | 31x31 room | 324.82 us* | 17.28 KB* | 44.20 us | 2.20 KB |
| Production LocalDB reload | seeded game | 47.80 ms | 55450.10 KB | 47.01 ms | 55453.84 KB |

`*` Eight-listener baselines are eight independent measured one-listener searches, matching the pre-optimization `KnowledgeSystem` loop. The optimized benchmark creates one attenuation map and performs eight lookups.

Optimizations retained after measurement:

- Reverse, mutation-tolerant expiry iteration instead of allocating `PropertyEffects.ToList()`.
- Transient indexes for build-up, resource, movement-extended, and per-property effects.
- One pooled sound attenuation field per emitted sound, reused by all player and sleeping-actor checks.

## Ability collection removal comparison

Measured with BenchmarkDotNet `MediumRun` after changing the runtime ability navigation from `HashSet<GameEntity>`
to `List<GameEntity>` and deactivating abilities by reverse index. Each invocation creates a fresh creature with the
specified number of active continuous abilities, then measures the complete death queue, including deactivation,
entity deletion, and relationship cleanup.

| Ability count | HashSet + restart scan | List + reverse scan | Time change | HashSet alloc | List alloc | Allocation change |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 16 | 467.0 us | 451.0 us | -3.4% | 10.78 KB | 10.08 KB | -6.5% |
| 64 | 850.3 us | 717.1 us | -15.7% | 23.20 KB | 20.63 KB | -11.1% |
| 256 | 1.872 ms | 1.563 ms | -16.5% | 72.75 KB | 62.67 KB | -13.9% |

The list-backed implementation is faster and allocates less at every measured size. CI checks the 256/64 time and
allocation ratios to catch accidental quadratic teardown behavior.

The same `AbilitySystemsPerfTest` suite measures batches of fully initialized Human Warrior creatures, including
processing their queued relationship and activation work:

| Creature count | Mean | Allocated |
| ---: | ---: | ---: |
| 16 | 21.78 ms | 4.19 MB |
| 64 | 54.99 ms | 6.75 MB |
| 256 | 239.21 ms | 20.43 MB |

CI also checks the 256/64 creature-instantiation time and allocation ratios (currently 4.35x and 3.03x).

BenchmarkDotNet's generated working artifacts under `BenchmarkDotNet.Artifacts/` are intentionally not source-controlled.

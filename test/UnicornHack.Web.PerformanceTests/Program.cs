using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Perfolizer.Horology;
using Perfolizer.Metrology;

namespace UnicornHack.Web.PerformanceTests;

public static class Program
{
    public static void Main(string[] args)
    {
        // Profiling mode: bypass BenchmarkDotNet so external profilers (dotnet-trace,
        // PerfView, etc.) capture a clean trace without BDN's measurement infrastructure
        // showing up in the samples.
        if (args.Length >= 1 && args[0] == "--profile-delta")
        {
            var iterations = args.Length >= 2 ? int.Parse(args[1]) : 2000;
            var npcCount = args.Length >= 3 ? int.Parse(args[2]) : 8;
            ProfilerHarness.RunDeltaLoop(iterations, npcCount);
            return;
        }

        if (args.Length >= 1 && args[0] == "--profile-snapshot")
        {
            var iterations = args.Length >= 2 ? int.Parse(args[1]) : 2000;
            var npcCount = args.Length >= 3 ? int.Parse(args[2]) : 8;
            ProfilerHarness.RunFullSnapshotLoop(iterations, npcCount);
            return;
        }

        // The Web project treats warnings as errors, including the transitive NU1902
        // vulnerability warning from OpenTelemetry. BenchmarkDotNet's default toolchain
        // generates an auxiliary csproj that re-restores the dependency graph, which
        // would surface that warning and abort. Using the in-process toolchain runs the
        // benchmarks in this same assembly and avoids the regenerate/restore step.
        var config = ManualConfig.CreateEmpty()
            .WithOptions(ConfigOptions.JoinSummary)
            .AddJob(Job.ShortRun.WithToolchain(InProcessNoEmitToolchain.Instance))
            .AddLogger(DefaultConfig.Instance.GetLoggers().ToArray())
            .AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray())
            .AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray())
            .AddValidator(DefaultConfig.Instance.GetValidators().ToArray())
            .AddDiagnoser(MemoryDiagnoser.Default);

        config = config
            .AddColumn(StatisticColumn.Min)
            .AddColumn(
                new ProjectionMetricColumn("Payload B", "Total serialized payload bytes across player responses.",
                    metrics => metrics.PayloadBytes),
                new ProjectionMetricColumn("DTOs", "Total emitted transport DTOs across player responses.",
                    metrics => metrics.EmittedDtoCount))
            .AddExporter(
                MarkdownExporter.GitHub,
                HtmlExporter.Default,
                new CsvExporter(
                    CsvSeparator.Comma,
                    new SummaryStyle
                    (
                        CultureInfo.InvariantCulture,
                        printUnitsInHeader: true,
                        printUnitsInContent: false,
                        timeUnit: TimeUnit.Millisecond,
                        sizeUnit: SizeUnit.KB
                    )));

        var artifactsOptionIndex = Array.IndexOf(args, "--artifacts");
        if (artifactsOptionIndex >= 0)
        {
            if (artifactsOptionIndex == args.Length - 1)
            {
                throw new ArgumentException("The --artifacts option requires a path.");
            }

            config = config.WithArtifactsPath(args[artifactsOptionIndex + 1]);
            args = [.. args[..artifactsOptionIndex], .. args[(artifactsOptionIndex + 2)..]];
        }

        var benchmarkSwitcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        var summary = args.Length > 0
            ? benchmarkSwitcher.Run(args, config).FirstOrDefault()
            : benchmarkSwitcher.RunAll(config).FirstOrDefault();

        if (summary != null)
        {
            Console.WriteLine();
            Console.WriteLine("See all results in " + summary.ResultsDirectoryPath);
        }
    }
}

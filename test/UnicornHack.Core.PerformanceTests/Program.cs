using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using Perfolizer.Metrology;

namespace UnicornHack.PerformanceTests;

public static class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.JoinSummary)
            .AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().Concat(new[] { MemoryDiagnoser.Default }).ToArray());

        config = config
            .AddColumn(StatisticColumn.Min)
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
            ? benchmarkSwitcher.Run(args, config).FirstOrDefault() // new DebugInProcessConfig()
            : benchmarkSwitcher.RunAll(config).FirstOrDefault();

        if (summary != null)
        {
            Console.WriteLine();
            Console.WriteLine("See all results in " + summary.ResultsDirectoryPath);
        }
    }
}

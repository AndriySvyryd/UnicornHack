using System;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace UnicornHack.PerformanceTests
{
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
                            timeUnit: TimeUnit.Microsecond,
                            sizeUnit: SizeUnit.KB
                        )));

            var benchmarkSwitcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
            var resultsDirectoryPath = args.Length > 0
                ? benchmarkSwitcher.Run(args, config).First().ResultsDirectoryPath // new DebugInProcessConfig()
                : benchmarkSwitcher.RunAll(config).First().ResultsDirectoryPath;

            Console.WriteLine();
            Console.WriteLine("See all results in " + resultsDirectoryPath);
        }
    }
}

using System;
using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace UnicornHack.PerformanceTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var config = DefaultConfig.Instance
                .With(DefaultConfig.Instance.GetDiagnosers().Concat(new[] { MemoryDiagnoser.Default }).ToArray());

            config = config
                .With(StatisticColumn.Min)
                .With(
                    MarkdownExporter.GitHub,
                    HtmlExporter.Default,
                    new CsvExporter(
                        CsvSeparator.Comma,
                        new SummaryStyle
                        {
                            PrintUnitsInHeader = true,
                            PrintUnitsInContent = false,
                            TimeUnit = TimeUnit.Microsecond,
                            SizeUnit = SizeUnit.KB
                        }));

            var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config).First();

            Console.WriteLine();
            Console.WriteLine("See all results in " + summary.ResultsDirectoryPath);
        }
    }
}

using System;
using BenchmarkDotNet.Running;
using UnicornHack.PerformanceTests.Utils;
using UnicornHack.PerformanceTests.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<EntityPerfTest>();
            BenchmarkRunner.Run<EntityRelationshipPerfTest>();
            BenchmarkRunner.Run<BeveledFOVPerfTest>();
            var summary = BenchmarkRunner.Run<PathFinderPerfTest>();

            Console.WriteLine();
            Console.WriteLine("See all results in " + summary.ResultsDirectoryPath);
        }
    }
}

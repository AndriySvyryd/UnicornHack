using System;
using BenchmarkDotNet.Running;
using UnicornHack.PerformanceTests.Utils.MessagingECS;

namespace UnicornHack
{
    public static class Program
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<EntityPerfTest>();

            Console.WriteLine();
            Console.WriteLine("See all results in " + summary.ResultsDirectoryPath);
        }
    }
}

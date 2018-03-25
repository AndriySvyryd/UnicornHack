using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack
{
    public static class TestAssert
    {
        public static void Equal(IEnumerable<Point> expected, IEnumerable<Point> actual, string message)
        {
            var sortedExpected = expected.OrderBy(r => r.X).ThenBy(r => r.Y).ToList();
            var sortedActual = actual.OrderBy(r => r.X).ThenBy(r => r.Y).ToList();

            var match = sortedExpected.Count == sortedActual.Count;
            if (match)
            {
                for (var i = 0; i < sortedExpected.Count; i++)
                {
                    if (!sortedExpected[i].Equals(sortedActual[i]))
                    {
                        match = false;
                    }
                }
            }

            Assert.True(match,
                "Expected:" + Environment.NewLine
                            + ToString(sortedExpected)
                            + "Actual:" + Environment.NewLine
                            + ToString(sortedActual)
                            + message);
        }

        public static string ToString(IEnumerable<Point> points)
        {
            var sb = new StringBuilder();
            foreach (var point in points)
            {
                sb.Append(point).AppendLine(", ");
            }

            return sb.ToString();
        }

        public static void Equal(IEnumerable<Rectangle> expected, IEnumerable<Rectangle> actual, string message)
        {
            var sortedExpected = expected.OrderBy(r => r.TopLeft.X)
                .ThenBy(r => r.TopLeft.Y)
                .ThenBy(r => r.BottomRight.X)
                .ThenBy(r => r.BottomRight.Y)
                .ToList();
            var sortedActual = actual.OrderBy(r => r.TopLeft.X)
                .ThenBy(r => r.TopLeft.Y)
                .ThenBy(r => r.BottomRight.X)
                .ThenBy(r => r.BottomRight.Y)
                .ToList();

            var match = sortedExpected.Count == sortedActual.Count;
            if (match)
            {
                for (var i = 0; i < sortedExpected.Count; i++)
                {
                    if (!sortedExpected[i].Equals(sortedActual[i]))
                    {
                        match = false;
                    }
                }
            }

            Assert.True(match,
                "Expected:" + Environment.NewLine
                            + ToString(sortedExpected)
                            + "Actual:" + Environment.NewLine
                            + ToString(sortedActual)
                            + message);
        }

        public static string ToString(IEnumerable<Rectangle> rectangles)
        {
            var sb = new StringBuilder();
            foreach (var rectangle in rectangles)
            {
                sb.Append(rectangle).AppendLine(", ");
            }

            return sb.ToString();
        }
    }
}

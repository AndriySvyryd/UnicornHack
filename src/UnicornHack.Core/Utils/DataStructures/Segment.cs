using System;

namespace UnicornHack.Utils.DataStructures
{
    /// <summary>
    ///     Represents an inclusive segment
    /// </summary>
    public struct Segment
    {
        public Segment(byte beginning, byte end)
        {
            if (beginning > end)
            {
                throw new ArgumentOutOfRangeException();
            }

            Beginning = beginning;
            End = end;
        }

        public readonly byte Beginning;
        public readonly byte End;
        public byte Length => (byte)(End - Beginning + 1);
        public byte MidPoint => (byte)(Beginning + (Length - 1) / 2);

        public Segment? Intersection(Segment s)
        {
            if (End < s.Beginning || Beginning > s.End)
            {
                return null;
            }

            if (s.Contains(this))
            {
                return this;
            }

            if (Contains(s))
            {
                return s;
            }

            return Beginning >= s.Beginning ? new Segment(Beginning, s.End) : new Segment(s.Beginning, End);
        }

        public bool Overlaps(Segment s) => Intersection(s) != null;

        public bool Encloses(Segment s) => Beginning < s.Beginning && End > s.End;

        public bool Contains(Segment s) => Beginning <= s.Beginning && End >= s.End;

        public bool Contains(byte value) => Contains(new Segment(value, value));

        public byte DistanceTo(Segment s)
        {
            var leftDistance = s.Beginning - End;
            if (leftDistance > 0)
            {
                return (byte)leftDistance;
            }

            var rightDistance = Beginning - s.End;
            if (rightDistance > 0)
            {
                return (byte)rightDistance;
            }

            return 0;
        }

        public byte DistanceTo(byte value) => DistanceTo(new Segment(value, value));

        public byte RandomPoint(SimpleRandom random) => (byte)random.Next(Beginning, End + 1);

        public override string ToString() => $"[{Beginning}, {End}]";
    }
}

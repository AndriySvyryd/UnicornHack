using System.Collections.Generic;
using System.Linq;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class Room
    {
        private byte _x1;
        private byte _x2;
        private byte _y1;
        private byte _y2;

        public Room(
            LevelComponent level,
            Rectangle boundingRectangle,
            IReadOnlyList<Point> doorwayPoints,
            IReadOnlyList<Point> insidePoints,
            IReadOnlyList<(Point, char)> predefinedPoints,
            MapFragment fragment)
        {
            Level = level;

            BoundingRectangle = boundingRectangle;
            DoorwayPoints = doorwayPoints ?? new Point[0];
            InsidePoints = insidePoints ?? new Point[0];
            PredefinedPoints = predefinedPoints;
            Fragment = fragment;
        }

        public LevelComponent Level { get; set; }

        public Rectangle BoundingRectangle
        {
            get => new Rectangle(new Point(_x1, _y1), new Point(_x2, _y2));
            set
            {
                _x1 = value.TopLeft.X;
                _y1 = value.TopLeft.Y;
                _x2 = value.BottomRight.X;
                _y2 = value.BottomRight.Y;
            }
        }

        public IReadOnlyList<Point> DoorwayPoints { get; }
        public IReadOnlyList<Point> InsidePoints { get; }
        public MapFragment Fragment { get; }
        public IReadOnlyList<(Point, char)> PredefinedPoints { get; }

        public Room GetClosest(IEnumerable<Room> rooms)
        {
            Room closest = null;
            var distance = 0;
            foreach (var room in rooms)
            {
                var nextDistance = BoundingRectangle.DistanceTo(room.BoundingRectangle);
                if (nextDistance < distance)
                {
                    distance = nextDistance;
                    closest = room;
                }
            }

            return closest;
        }

        public Room GetOrthogonallyClosest(IEnumerable<Room> rooms)
        {
            Room closest = null;
            var distance = int.MaxValue;
            foreach (var room in rooms)
            {
                var nextDistance = BoundingRectangle.OrthogonalDistanceTo(room.BoundingRectangle);
                if (nextDistance < distance)
                {
                    distance = nextDistance;
                    closest = room;
                }
            }

            return closest;
        }

        public Point GetGoodConnectionPoint(Room room)
        {
            var connectionPointDistances = DoorwayPoints
                .Select(p => (Point: p, Distance: room.BoundingRectangle.DistanceTo(p))).ToList();
            byte maxDistance = 0;
            foreach (var connectionPointDistance in connectionPointDistances)
            {
                if (connectionPointDistance.Distance > maxDistance)
                {
                    maxDistance = connectionPointDistance.Distance;
                }
            }

            // The closer points are much more likely to get chosen
            return Level.GenerationRandom.WeightedOrder(connectionPointDistances, p => (maxDistance - p.Distance) << 1)
                .First().Point;
        }
    }
}

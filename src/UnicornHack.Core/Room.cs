using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Room
    {
        public virtual int Id { get; private set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual string BranchName { get; private set; }
        public virtual byte LevelDepth { get; private set; }
        public virtual Level Level { get; set; }

        public virtual Rectangle BoundingRectangle
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

        private byte _x1;
        private byte _x2;
        private byte _y1;
        private byte _y2;

        public IReadOnlyList<Point> DoorwayPoints { get; }

        public Room()
        {
        }

        public Room(Level level, Rectangle boundingRectangle, IReadOnlyList<Point> doorwayPoints)
        {
            Game = level.Game;
            Level = level;
            level.Rooms.Add(this);
            Id = Level.NextRoomId++;

            BoundingRectangle = boundingRectangle;
            DoorwayPoints = doorwayPoints ?? new Point[0];
        }

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
            var distance = Int32.MaxValue;
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
                .Select(p => (Point: p, Distance: room.BoundingRectangle.DistanceTo(p)))
                .ToList();
            byte maxDistance = 0;
            foreach (var connectionPointDistance in connectionPointDistances)
            {
                if (connectionPointDistance.Distance > maxDistance)
                {
                    maxDistance = connectionPointDistance.Distance;
                }
            }

            // The closer points are much more likely to get chosen
            return Level.GenerationRandom.WeightedOrder(connectionPointDistances,
                    p => (maxDistance - p.Distance) << 1)
                .First()
                .Point;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Utils
{
    // A* implementation
    public class PathFinder
    {
        private readonly int[,] _pointToIndex;
        private readonly Point[] _indexToPoint;

        private readonly PriorityQueue<int> _openNodes;
        private readonly List<Point> _shortestPath = new();
        private readonly PathFinderNode[] _graph;
        private byte _openNodeStatus = 1;
        private byte _closedNodeStatus = 2;

        public PathFinder(int[,] pointToIndex, Point[] indexToPoint)
        {
            _pointToIndex = pointToIndex;
            _indexToPoint = indexToPoint;

            _graph = new PathFinderNode[indexToPoint.Length];
            _openNodes = new PriorityQueue<int>(new PathFinderNodeComparer(_graph));
        }

        // Not thread safe
        public List<Point> FindPath<TState>(Point start, Point target, Direction initialDirection,
            Func<byte, byte, TState, int?> canMoveTo, TState state)
        {
            var found = false;

            // Instead of clearing the status of all nodes we just use different values
            _openNodeStatus += 2;
            _closedNodeStatus += 2;
            _openNodes.Clear();

            // TODO: Perf: Use Jump Point Search for uniform grids
            // https://gamedevelopment.tutsplus.com/tutorials/how-to-speed-up-a-pathfinding-with-the-jump-point-search-algorithm--gamedev-5818

            // TODO: Perf: Precompute Dijkstra Maps for sharing
            // http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps
            var currentLocationIndex = _pointToIndex[start.X, start.Y];
            var targetLocationIndex = _pointToIndex[target.X, target.Y];
            ref var currentLocationNode = ref _graph[currentLocationIndex];
            currentLocationNode.CostFromStart = 0;
            currentLocationNode.EstimatedTotalCost = start.DistanceTo(target);
            currentLocationNode.ArrivalDirection = initialDirection;
            currentLocationNode.PreviousX = start.X;
            currentLocationNode.PreviousY = start.Y;
            currentLocationNode.Status = _openNodeStatus;

            _openNodes.Push(currentLocationIndex);
            while (_openNodes.Count > 0)
            {
                currentLocationIndex = _openNodes.Pop();

                currentLocationNode = ref _graph[currentLocationIndex];
                if (currentLocationNode.Status == _closedNodeStatus)
                {
                    continue;
                }

                var currentLocation = _indexToPoint[currentLocationIndex];

                Debug.Assert(currentLocation.Y * (_pointToIndex.GetUpperBound(dimension: 0) + 1) + currentLocation.X ==
                             currentLocationIndex);

                if (currentLocationIndex == targetLocationIndex)
                {
                    currentLocationNode.Status = _closedNodeStatus;
                    found = true;
                    break;
                }

                var previousDirection = currentLocationNode.ArrivalDirection;
                for (var i = 0; i < 8; i++)
                {
                    var direction = Vector.MovementDirections[i];
                    var newLocationX = (byte)(currentLocation.X + direction.X);
                    var newLocationY = (byte)(currentLocation.Y + direction.Y);

                    var possibleNewLocation = canMoveTo(newLocationX, newLocationY, state);
                    if (possibleNewLocation == null)
                    {
                        continue;
                    }

                    var newLocationIndex = possibleNewLocation.Value;
                    var newCostFromStart = currentLocationNode.CostFromStart + 1;
                    if ((Direction)i != previousDirection)
                    {
                        var octants = ((Direction)i).ClosestOctantsTo(previousDirection);
                        newCostFromStart += octants / 4f;
                    }

                    ref var newLocationNode = ref _graph[newLocationIndex];
                    if ((newLocationNode.Status == _openNodeStatus ||
                         newLocationNode.Status == _closedNodeStatus) &&
                        newLocationNode.CostFromStart <= newCostFromStart)
                    {
                        continue;
                    }

                    var distanceToTarget = (byte)Math.Max(Math.Abs(target.X - newLocationX),
                        Math.Abs(target.Y - newLocationY));
                    newLocationNode.ArrivalDirection = (Direction)i;
                    newLocationNode.PreviousX = currentLocation.X;
                    newLocationNode.PreviousY = currentLocation.Y;
                    newLocationNode.CostFromStart = newCostFromStart;
                    newLocationNode.EstimatedTotalCost = newCostFromStart + distanceToTarget;
                    newLocationNode.Status = _openNodeStatus;

                    _openNodes.Push(newLocationIndex);
                }

                currentLocationNode.Status = _closedNodeStatus;
            }

            if (found)
            {
                _shortestPath.Clear();
                var pointX = target.X;
                var pointY = target.Y;
                var currentNode = _graph[_pointToIndex[pointX, pointY]];

                while (pointX != currentNode.PreviousX || pointY != currentNode.PreviousY)
                {
                    var point = new Point(pointX, pointY);
                    pointX = currentNode.PreviousX;
                    pointY = currentNode.PreviousY;

                    _shortestPath.Add(point);
                    currentNode = _graph[_pointToIndex[pointX, pointY]];
                }

                Debug.Assert(pointX == start.X && pointY == start.Y);

                return _shortestPath;
            }

            return null;
        }

        private struct PathFinderNode
        {
            public float EstimatedTotalCost;
            public float CostFromStart;
            public Direction ArrivalDirection;
            public byte PreviousX;
            public byte PreviousY;
            public byte Status;
        }

        private class PathFinderNodeComparer : IComparer<int>
        {
            private readonly PathFinderNode[] _graph;

            public PathFinderNodeComparer(PathFinderNode[] graph) => _graph = graph;

            public int Compare(int a, int b)
            {
                if (_graph[a].EstimatedTotalCost > _graph[b].EstimatedTotalCost)
                {
                    return 1;
                }

                if (_graph[a].EstimatedTotalCost < _graph[b].EstimatedTotalCost)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Utils
{
    // A* implementation
    public class PathFinder
    {
        private readonly Func<Point, int, int?> _canMoveTo;
        private readonly int[,] _pointToIndex;
        private readonly Point[] _indexToPoint;

        private readonly PriorityQueue<int> _openNodes;
        private readonly List<Point> _shortestPath = new List<Point>();
        private readonly PathFinderNode[] _graph;
        private byte _openNodeStatus = 1;
        private byte _closedNodeStatus = 2;

        public PathFinder(Func<Point, int, int?> canMoveTo, int[,] pointToIndex, Point[] indexToPoint)
        {
            _canMoveTo = canMoveTo;
            _pointToIndex = pointToIndex;
            _indexToPoint = indexToPoint;

            _graph = new PathFinderNode[indexToPoint.Length];
            _openNodes = new PriorityQueue<int>(new PathFinderNodeComparer(_graph));
        }

        // Not thread safe
        public List<Point> FindPath(Point start, Point target, int searchLimit = Int32.MaxValue)
        {
            var closedNodesCount = 0;
            var found = false;

            // Instead of clearing the status of all nodes we just use different values
            _openNodeStatus += 2;
            _closedNodeStatus += 2;
            _openNodes.Clear();

            var currentLocationIndex = _pointToIndex[start.X, start.Y];
            var targetLocation = _pointToIndex[target.X, target.Y];
            _graph[currentLocationIndex].CostFromStart = 0;
            _graph[currentLocationIndex].EstimatedCostToTarget = start.DistanceTo(target);
            _graph[currentLocationIndex].PreviousX = start.X;
            _graph[currentLocationIndex].PreviousY = start.Y;
            _graph[currentLocationIndex].Status = _openNodeStatus;

            _openNodes.Push(currentLocationIndex);
            while (_openNodes.Count > 0)
            {
                currentLocationIndex = _openNodes.Pop();

                if (_graph[currentLocationIndex].Status == _closedNodeStatus)
                {
                    continue;
                }

                var currentLocation = _indexToPoint[currentLocationIndex];

                Debug.Assert(currentLocation.Y * (_pointToIndex.GetUpperBound(dimension: 0) + 1) + currentLocation.X
                             == currentLocationIndex);

                if (currentLocationIndex == targetLocation)
                {
                    _graph[currentLocationIndex].Status = _closedNodeStatus;
                    found = true;
                    break;
                }

                if (closedNodesCount > searchLimit)
                {
                    return null;
                }

                for (var i = 0; i < 8; i++)
                {
                    var possibleNewLocation = _canMoveTo(currentLocation, i);
                    if (possibleNewLocation == null)
                    {
                        continue;
                    }

                    var newLocationIndex = possibleNewLocation.Value;
                    var newCostFromStart = _graph[currentLocationIndex].CostFromStart + 1;
                    if (i > 3)
                    {
                        // Small penalty for diagonal movement
                        newCostFromStart += 0.001f;
                    }
                    if ((_graph[newLocationIndex].Status == _openNodeStatus
                         || _graph[newLocationIndex].Status == _closedNodeStatus)
                        && _graph[newLocationIndex].CostFromStart <= newCostFromStart)
                    {
                        continue;
                    }

                    _graph[newLocationIndex].PreviousX = currentLocation.X;
                    _graph[newLocationIndex].PreviousY = currentLocation.Y;
                    _graph[newLocationIndex].CostFromStart = newCostFromStart;
                    _graph[newLocationIndex].EstimatedCostToTarget =
                        newCostFromStart + _indexToPoint[newLocationIndex].DistanceTo(target);

                    _openNodes.Push(newLocationIndex);
                    _graph[newLocationIndex].Status = _openNodeStatus;
                }

                closedNodesCount++;
                _graph[currentLocationIndex].Status = _closedNodeStatus;
            }

            if (found)
            {
                _shortestPath.Clear();
                var pointX = target.X;
                var pointY = target.Y;
                var currentNode = _graph[_pointToIndex[pointX, pointY]];

                while (pointX != currentNode.PreviousX
                       || pointY != currentNode.PreviousY)
                {
                    Point point;
                    point.X = pointX;
                    point.Y = pointY;
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

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PathFinderNode
        {
            public float EstimatedCostToTarget;
            public float CostFromStart;
            public byte PreviousX;
            public byte PreviousY;
            public byte Status;
        }

        private class PathFinderNodeComparer : IComparer<int>
        {
            private readonly PathFinderNode[] _graph;

            public PathFinderNodeComparer(PathFinderNode[] graph)
            {
                _graph = graph;
            }

            public int Compare(int a, int b)
            {
                if (_graph[a].EstimatedCostToTarget > _graph[b].EstimatedCostToTarget)
                {
                    return 1;
                }
                if (_graph[a].EstimatedCostToTarget < _graph[b].EstimatedCostToTarget)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
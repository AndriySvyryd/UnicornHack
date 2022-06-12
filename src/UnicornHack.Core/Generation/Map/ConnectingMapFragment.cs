using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Data.Fragments;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation.Map
{
    public class ConnectingMapFragment : MapFragment
    {
        public ICollection<LevelConnection> Connections { get; set; } = new HashSet<LevelConnection>();

        private Func<string, int, int, int, ConnectionComponent, float> _weightFunction;

        private static readonly UnicornExpressionVisitor _translator =
            new(new[] { BranchParameter, DepthParameter, InstancesParameter, TagInstancesParameter, ConnectionParameter });

        protected override void ResetWeightFunction() => _weightFunction = null;

        public static Func<string, int, int, int, ConnectionComponent, float> CreateWeightFunction(string expression)
            => _translator.Translate<Func<string, int, int, int, ConnectionComponent, float>, float>(expression);

        public float GetWeight(LevelComponent level, Rectangle boundingRectangle, ConnectionComponent target)
        {
            // TODO: take transformations into account
            if (PayloadArea.Width > boundingRectangle.Width || PayloadArea.Height > boundingRectangle.Height)
            {
                return 0;
            }

            if (_weightFunction == null)
            {
                try
                {
                    _weightFunction = CreateWeightFunction(GenerationWeight ?? DefaultWeight);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
                }
            }

            try
            {
                return _weightFunction(level.Branch.Name, level.Depth, 0, 0, target);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
            }
        }

        public override Room BuildRoom(LevelComponent level, IEnumerable<Point> points, Action<Point> insideAction,
            Action<Point> perimeterAction, Action<Point> outsideAction)
        {
            var room = base.BuildRoom(level, points, insideAction, perimeterAction, outsideAction);
            if (room == null
                || room.InsidePoints.Count == 0)
            {
                return room;
            }

            var connectionDefinitions = Connections;
            if (connectionDefinitions.Count == 0)
            {
                connectionDefinitions = new[] {new LevelConnection()};
            }

            foreach (var levelConnection in connectionDefinitions.Where(c => c.Glyph == null))
            {
                var connectionPoint = level.GenerationRandom.Pick(room.InsidePoints,
                    p => !level.Connections.ContainsKey(p));
                CreateConnection(level, connectionPoint, levelConnection);
            }

            return room;
        }

        protected override void CreateConnection(LevelComponent level, Point point, char? glyph)
            => CreateConnection(level, point, Connections.FirstOrDefault(c => c.Glyph == glyph));

        protected virtual void CreateConnection(
            LevelComponent level, Point point, LevelConnection connectionDefinition)
        {
            var manager = level.Entity.Manager;

            foreach (var connectionEntity in level.IncomingConnections)
            {
                var connection = connectionEntity.Connection;
                if (connection.TargetLevelX == null)
                {
                    var target = manager.FindEntity(connection.TargetLevelId).Level;
                    if ((connectionDefinition?.BranchName == null
                         || connectionDefinition.BranchName == target.BranchName)
                        && (connectionDefinition?.Depth == null
                            || connectionDefinition.Depth == target.Depth)
                        && LevelConnection.AreMatchingDirection(connection.Direction, connectionDefinition?.Direction))
                    {
                        LevelConnection.CreateReceivingConnection(level, point, connection, connectionDefinition?.Direction);
                        return;
                    }
                }
            }

            LevelConnection.CreateSourceConnection(level, point, connectionDefinition);
        }

        public static readonly CSScriptLoader<ConnectingMapFragment> Loader =
            new CSScriptLoader<ConnectingMapFragment>(@"Data\Fragments\Connecting\", typeof(ConnectingMapFragmentData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<ConnectingMapFragment>(GetPropertyConditions<ConnectingMapFragment>());

        protected new static Dictionary<string, Func<TConnectingMapFragment, object, bool>>
            GetPropertyConditions<TConnectingMapFragment>() where TConnectingMapFragment : ConnectingMapFragment
        {
            var propertyConditions = MapFragment.GetPropertyConditions<TConnectingMapFragment>();
            var mapCondition = propertyConditions[nameof(Map)];
            propertyConditions.Remove(nameof(Map));

            propertyConditions.Add(nameof(Connections),
                (o, v) => v != null && ((ICollection<LevelConnection>)v).Count != 0);
            propertyConditions.Add(nameof(Map), mapCondition);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}

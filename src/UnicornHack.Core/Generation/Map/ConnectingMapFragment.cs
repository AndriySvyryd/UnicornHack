using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class ConnectingMapFragment : MapFragment
    {
        public virtual ICollection<LevelConnection> Connections { get; set; } = new HashSet<LevelConnection>();

        #region Actions

        private Func<string, byte, int, int, Connection, float> _weightFunction;

        public virtual float GetWeight(Level level, Rectangle boundingRectangle, Connection target)
        {
            // TODO: take transformations into account
            if (PayloadArea.Width > boundingRectangle.Width
                || PayloadArea.Height > boundingRectangle.Height)
            {
                return 0;
            }

            if (_weightFunction == null)
            {
                _weightFunction = GenerationWeight.CreateConnectingFragmentWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0, 0, target);
        }

        public override float GetWeight(Level level, Rectangle boundingRectangle)
        {
            throw new InvalidOperationException();
        }

        public override Room BuildRoom(Level level, IEnumerable<Point> points,
            Action<Point> insideAction, Action<Point> perimeterAction, Action<Point> outsideAction)
        {
            var room = base.BuildRoom(level, points, insideAction, perimeterAction, outsideAction);

            var connectionDefinitions = Connections;
            if (connectionDefinitions.Count == 0)
            {
                connectionDefinitions = new[] {new LevelConnection()};
            }

            foreach (var levelConnection in connectionDefinitions.Where(c => c.Glyph == null))
            {
                CreateConnection(level,
                    level.GenerationRandom.Pick(room.InsidePoints,
                        p => !level.Connections.Any(c => c.LevelX == p.X && c.LevelY == p.Y)),
                    levelConnection);
            }

            return room;
        }

        protected override void Write(char c, Point point, Level level,
            (List<Point> doorwayPoints, List<Point> perimeterPoints, List<Point> insidePoints, List<Point> points)
                state)
        {
            MapFeature feature;
            switch (c)
            {
                case '<':
                case '{':
                case '[':
                case '>':
                case '}':
                case ']':
                    feature = MapFeature.StoneFloor;
                    state.points.Add(point);
                    CreateConnection(level, point, c);
                    break;
                default:
                    base.Write(c, point, level, state);
                    return;
            }

            level.Terrain[level.PointToIndex[point.X, point.Y]] = (byte)feature;
        }

        protected virtual void CreateConnection(Level level, Point point, char? glyph)
            => CreateConnection(level, point, Connections.FirstOrDefault(c => c.Glyph == glyph));

        protected virtual Connection CreateConnection(Level level, Point point, LevelConnection connectionDefinition)
        {
            var danglingConnection = level.IncomingConnections
                .FirstOrDefault(c => c.TargetLevelX == null
                                     && (connectionDefinition?.BranchName == null
                                         || connectionDefinition.BranchName == c.BranchName)
                                     && (connectionDefinition?.Depth == null
                                         || connectionDefinition.Depth == c.LevelDepth));

            return danglingConnection != null
                ? Connection.CreateReceivingConnection(level, point, danglingConnection)
                : connectionDefinition?.BranchName != null
                    ? Connection.CreateSourceConnection(
                        level, point, connectionDefinition.BranchName, connectionDefinition.Depth ?? 1)
                    : Connection.CreateSourceConnection(
                        level, point, level.BranchName, connectionDefinition?.Depth ?? (byte)(level.Depth + 1));
        }

        #endregion

        #region Serialization

        public static readonly CSScriptLoader<ConnectingMapFragment> ConnectingLoader =
            new CSScriptLoader<ConnectingMapFragment>(@"data\fragments\connecting\");

        public static IReadOnlyList<ConnectingMapFragment> GetAllConnectingMapFragments() => ConnectingLoader.GetAll();

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<ConnectingMapFragment>(
            GetPropertyConditions<ConnectingMapFragment>());

        protected new static Dictionary<string, Func<TConnectingMapFragment, object, bool>> GetPropertyConditions
            <TConnectingMapFragment>()
            where TConnectingMapFragment : ConnectingMapFragment
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

        #endregion
    }
}
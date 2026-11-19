using CSharpScriptSerialization;
using UnicornHack.Systems.Features;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class FeatureGenerator : ICSScriptSerializable
{
    public virtual void Fill(RoomComponent room, IReadOnlyList<Chunk> chunks)
    {
        var points = new Dictionary<Point, RoomFeatures>();
        foreach (var chunk in chunks)
        {
            foreach (var (point, feature) in chunk.FeaturePoints)
            {
                if (feature is { } kind)
                {
                    points[point] = kind;
                }
            }
        }

        foreach (var (point, kind) in points)
        {
            if (room.Features.ContainsKey(point) || room.Connections.ContainsKey(point)
                || room.Terrain[room.PointToIndex![point.X, point.Y]] is MapFeature.Default or MapFeature.RockWall or MapFeature.StoneWall)
            {
                continue;
            }

            if (kind == RoomFeatures.Trapped && !room.Game.Services.Data.Features.GetAll().Any(feature => feature.RoomFeature == kind))
            {
                continue;
            }

            Place(room, point, kind);
        }
    }

    public static GameEntity Place(RoomComponent room, Point cell, RoomFeatures kind)
    {
        var canBlockMovement = kind != RoomFeatures.Trapped || CanBlockCell(room, cell);
        var definition = room.Level.GenerationRandom.WeightedOrder(
                room.Game.Services.Data.Features.GetAsList(), candidate => candidate.GetWeight(room, kind, canBlockMovement))
            .FirstOrDefault() ?? throw new InvalidOperationException($"No eligible feature definition for {kind} at {cell}.");
        if (kind != RoomFeatures.Trapped && (definition.Behavior & FeatureBehavior.BlocksMovement) != 0)
        {
            cell = room.Entity.Manager.TravelSystem.FindCell(room,
                (candidate, currentRoom) => !currentRoom.Features.ContainsKey(candidate)
                    && !currentRoom.Connections.ContainsKey(candidate) && !currentRoom.Actors.ContainsKey(candidate)
                    && !currentRoom.Items.ContainsKey(candidate)
                    && currentRoom.Entity.Manager.TravelSystem.CanMoveTo(candidate, currentRoom)
                    && CanBlockCell(currentRoom, candidate),
                origin: cell, maxRadius: Math.Max(room.Width, room.Height))
                ?? throw new InvalidOperationException($"No connectivity-preserving placement for '{definition.Name}' near {cell}.");
        }
        return definition.Instantiate(room, cell);
    }

    private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<FeatureGenerator>(
        new Dictionary<string, Func<FeatureGenerator, object?, bool>>());

    public virtual ICSScriptSerializer GetSerializer() => Serializer;

    private static bool CanBlockCell(RoomComponent room, Point blocked)
    {
        var remaining = room.IndexToPoint!.Take(room.TileCount)
            .Where(point => point != blocked && room.Entity.Manager.TravelSystem.CanMoveTo(point, room)).ToArray();
        if (remaining.Length < 2)
        {
            return true;
        }
        var start = remaining.FirstOrDefault(point => !room.Connections.ContainsKey(point));
        if (!remaining.Contains(start))
        {
            start = remaining[0];
        }
        var reached = new HashSet<Point> { start };
        var pending = new Queue<Point>();
        pending.Enqueue(start);
        while (pending.TryDequeue(out var current))
        {
            foreach (var vector in Vector.MovementDirections)
            {
                var next = current.Translate(vector);
                if (next == blocked || !room.IsValid(next)
                    || !room.Entity.Manager.TravelSystem.CanMoveTo(next, room) || !reached.Add(next))
                {
                    continue;
                }
                if (!room.Connections.ContainsKey(next))
                {
                    pending.Enqueue(next);
                }
            }
        }
        return remaining.All(reached.Contains);
    }
}

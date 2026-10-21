using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class CreatureGenerator : ICSScriptSerializable
{
    /// <summary>
    ///     No random creature is placed within this many cells (Chebyshev distance) of an
    ///     incoming connection, so the player never arrives surrounded by monsters.
    /// </summary>
    public const int ConnectionExclusionRadius = 5;

    public byte ExpectedInitialCount
    {
        get;
        set;
    } = 8;

    public byte MaximumInitialCount { get; set; } = byte.MaxValue;

    public virtual void Fill(RoomComponent room, IReadOnlyList<Chunk> chunks)
    {
        // Authored spawn points bypass the random budget; we count them up front so the
        // random pass only fills the remaining headroom toward ExpectedInitialCount.
        var actorsCount = room.Actors.Count;
        var creaturesToPlace = Math.Min(Math.Max(0, MaximumInitialCount - actorsCount),
            room.Level.GenerationRandom.NextBinomial(0.5f, Math.Max(0, ExpectedInitialCount - actorsCount) * 2));

        // Cells within ConnectionExclusionRadius of an incoming connection — the player
        // arrives on one of these, so no creature (authored or random) may spawn in the
        // safety buffer. Computed up front so the authored pass below can honour it too.
        var exclusionZone = GetConnectionExclusionZone(room);

        foreach (var (point, creatureName) in chunks.SelectMany(chunk => chunk.CreaturePoints))
        {
            if (creatureName is null
                && (room.Actors.Count >= MaximumInitialCount || room.Actors.ContainsKey(point)))
            {
                continue;
            }

            var spawned = TryResolveAuthoredSpawnPoint(room, chunks, exclusionZone, point, out var spawnPoint)
                          && (creatureName is null
                              ? TrySpawnCreatureAt(room, spawnPoint, MaximumInitialCount)
                              : TrySpawnExact(room, spawnPoint, creatureName, MaximumInitialCount));
            if (!spawned)
            {
                if (creatureName is not null)
                {
                    throw new InvalidOperationException($"Cannot place exact creature '{creatureName}' at {point} in room {room.EntityId}.");
                }

                continue;
            }

            creaturesToPlace = Math.Max(0, creaturesToPlace - 1);
        }

        // Random body: weight ConnectingMapFragment chunks at zero so the weighted iterator
        // never picks them, which prevents random creature placement on chunks that host an
        // inbound level connection unless the fragment placed an authored spawn point above.
        var chunksToFill = chunks.Count;

        foreach (var chunk in room.Level.GenerationRandom.WeightedOrder(
                     chunks,
                     c => c.InsidePoints.Count))
        {
            var creaturesPerRoom = (int)Math.Ceiling((float)creaturesToPlace / chunksToFill);
            for (var i = 0; i < creaturesPerRoom; i++)
            {
                if (!room.Level.GenerationRandom.TryPick(
                        chunk.InsidePoints,
                        p => !room.Actors.ContainsKey(p) && !exclusionZone.Contains(p),
                        out var point))
                {
                    break;
                }

                if (TrySpawnCreatureAt(room, point, MaximumInitialCount))
                {
                    creaturesToPlace--;
                }

                // TODO: Log no creatures of the chosen level
            }

            chunksToFill--;
        }
    }

    private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<CreatureGenerator>(
        new Dictionary<string, Func<CreatureGenerator, object?, bool>>
        {
            [nameof(ExpectedInitialCount)] = (_, value) => (byte)value! != 8,
            [nameof(MaximumInitialCount)] = (_, value) => (byte)value! != byte.MaxValue
        });

    public virtual ICSScriptSerializer GetSerializer() => Serializer;

    /// <summary>
    ///     The set of cells within <see cref="ConnectionExclusionRadius" /> (Chebyshev) of an
    ///     incoming connection's landing cell. Both the room's own connection tiles and the
    ///     target cells of connections arriving from elsewhere are treated as arrival points.
    ///     Returns an empty set when the room has no connections yet (intra-level doorways are
    ///     carved after this pass runs).
    /// </summary>
    private static HashSet<Point> GetConnectionExclusionZone(RoomComponent room)
    {
        var zone = new HashSet<Point>();

        foreach (var incoming in room.IncomingConnections)
        {
            if (incoming.Connection!.TargetCell is not { } targetCell)
            {
                continue;
            }

            var minX = Math.Max(0, targetCell.X - ConnectionExclusionRadius);
            var maxX = Math.Min(room.Width - 1, targetCell.X + ConnectionExclusionRadius);
            var minY = Math.Max(0, targetCell.Y - ConnectionExclusionRadius);
            var maxY = Math.Min(room.Height - 1, targetCell.Y + ConnectionExclusionRadius);
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    zone.Add(new Point((byte)x, (byte)y));
                }
            }
        }

        return zone;
    }

    private static bool TryResolveAuthoredSpawnPoint(
        RoomComponent room, IReadOnlyList<Chunk> chunks, HashSet<Point> exclusionZone,
        Point origin, out Point relocated)
    {
        relocated = origin;
        if (!exclusionZone.Contains(origin))
        {
            return true;
        }

        var bestDistance = int.MaxValue;
        var found = false;
        foreach (var chunk in chunks)
        {
            foreach (var candidate in chunk.InsidePoints)
            {
                if (exclusionZone.Contains(candidate) || room.Actors.ContainsKey(candidate)
                    || !room.Terrain[room.PointToIndex![candidate.X, candidate.Y]].CanMoveTo())
                {
                    continue;
                }

                var distance = candidate.DistanceTo(origin);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    relocated = candidate;
                    found = true;
                }
            }
        }

        return found;
    }

    public static bool TrySpawnExact(
        RoomComponent room, Point point, string name, byte maximumInitialCount = byte.MaxValue)
        => TrySpawnCreatureAt(room, point, room.Game.Services.Data.Creatures.Get(name), maximumInitialCount);

    private static bool TrySpawnCreatureAt(RoomComponent room, Point point, byte maximumInitialCount)
    {
        foreach (var creature in room.Level.GenerationRandom.WeightedOrder(
                     room.Game.Services.Data.Creatures.GetAsList(), candidate => candidate.GetWeight(room)))
        {
            if (TrySpawnCreatureAt(room, point, creature, maximumInitialCount))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TrySpawnCreatureAt(
        RoomComponent room, Point point, Creature creature, byte maximumInitialCount)
    {
        if (!room.IsValid(point) || !room.Terrain[room.PointToIndex![point.X, point.Y]].CanMoveTo()
            || room.Actors.ContainsKey(point)
            || room.Actors.Count >= maximumInitialCount
            || !creature.CanPlaceInstance(room.Level))
        {
            return false;
        }

        foreach (var incoming in room.IncomingConnections)
        {
            if (incoming.Connection!.TargetCell is { } arrival
                && point.DistanceTo(arrival) <= ConnectionExclusionRadius)
            {
                return false;
            }
        }

        if (creature.Instantiate(room, point) == null)
        {
            return false;
        }

        room.Level.GenerationStats.RecordCreatureInstance(creature.Name, creature.GenerationFlags, creature.IsNamedNonBoss);
        room.Game.GenerationStats.RecordCreatureInstance(creature.Name, creature.GenerationFlags, creature.IsNamedNonBoss);
        return true;
    }
}

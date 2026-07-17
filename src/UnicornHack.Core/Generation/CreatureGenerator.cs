using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation;

public class CreatureGenerator
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

    public virtual void Fill(RoomComponent room, IReadOnlyList<Chunk> chunks)
    {
        // Authored spawn points bypass the random budget; we count them up front so the
        // random pass only fills the remaining headroom toward ExpectedInitialCount.
        var actorsCount = room.Actors.Count;
        var creaturesToPlace = room.Level.GenerationRandom.NextBinomial(0.5f, (ExpectedInitialCount - actorsCount) * 2);
        var difficultyFraction = (float)(room.Level.Difficulty - 1) / LevelGenerator.MaxDifficulty;

        // Cells within ConnectionExclusionRadius of an incoming connection — the player
        // arrives on one of these, so no creature (authored or random) may spawn in the
        // safety buffer. Computed up front so the authored pass below can honour it too.
        var exclusionZone = GetConnectionExclusionZone(room);

        // Authored spawn points spawn if the tile is free, regardless of budget or chunk
        // kind — so a fragment can place a guardian by marking the tile in its payload. A
        // point that falls inside the arrival safety buffer is relocated to the nearest free
        // cell outside it rather than dropped, so the fragment's creature still appears without
        // materializing on top of the incoming player.
        foreach (var chunk in chunks)
        {
            for (var j = 0; j < chunk.CreaturePoints.Count; j++)
            {
                var point = chunk.CreaturePoints[j];
                if (room.Actors.ContainsKey(point))
                {
                    continue;
                }

                if (exclusionZone.Contains(point)
                    && !TryFindNearestSpawnCellOutsideZone(room, chunks, exclusionZone, point, out point))
                {
                    continue;
                }

                TrySpawnCreatureAt(room, point, difficultyFraction);
            }
        }

        // Random body: weight ConnectingMapFragment chunks at zero so the weighted iterator
        // never picks them, which prevents random creature placement on chunks that host an
        // inbound level connection unless the fragment placed an authored spawn point above.
        var chunksToFill = 0;
        foreach (var chunk in chunks)
        {
            chunksToFill++;
        }

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

                if (TrySpawnCreatureAt(room, point, difficultyFraction))
                {
                    creaturesToPlace--;
                }

                // TODO: Log no creatures of the chosen level
            }

            chunksToFill--;
        }
    }

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
            if (incoming.Connection!.TargetCell is { } targetCell)
            {
                AddExclusionAround(zone, [targetCell], room.Width, room.Height);
            }
        }

        return zone;
    }

    private static void AddExclusionAround(
        HashSet<Point> zone, IEnumerable<Point> centers, int width, int height)
    {
        foreach (var center in centers)
        {
            var minX = Math.Max(0, center.X - ConnectionExclusionRadius);
            var maxX = Math.Min(width - 1, center.X + ConnectionExclusionRadius);
            var minY = Math.Max(0, center.Y - ConnectionExclusionRadius);
            var maxY = Math.Min(height - 1, center.Y + ConnectionExclusionRadius);
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    zone.Add(new Point((byte)x, (byte)y));
                }
            }
        }
    }

    /// <summary>
    ///     Finds the nearest free walkable cell (a chunk inside point not already occupied and
    ///     outside <paramref name="exclusionZone" />) to <paramref name="origin" />, so an
    ///     authored spawn point that fell inside the arrival safety buffer can be relocated
    ///     instead of dropped. Distance is Chebyshev, matching the exclusion radius. Returns
    ///     <see langword="false" /> when no such cell exists.
    /// </summary>
    private static bool TryFindNearestSpawnCellOutsideZone(
        RoomComponent room, IReadOnlyList<Chunk> chunks, HashSet<Point> exclusionZone,
        Point origin, out Point relocated)
    {
        relocated = origin;
        var bestDistance = int.MaxValue;
        var found = false;
        foreach (var chunk in chunks)
        {
            foreach (var candidate in chunk.InsidePoints)
            {
                if (exclusionZone.Contains(candidate) || room.Actors.ContainsKey(candidate))
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

    private static bool TrySpawnCreatureAt(RoomComponent room, Point point, float difficultyFraction)
    {
        foreach (var creatureLevel in Sequence.GetAlternating(
                     room.Level.GenerationRandom.NextBinomial(difficultyFraction, Creature.MaxLevel - 1), 0,
                     Creature.MaxLevel - 1))
        {
            foreach (var creatureVariant in room.Level.GenerationRandom.WeightedOrder(
                         room.Level.Game.Services.Data.Creatures.GetAllValues((byte)(creatureLevel + 1)),
                         c => c.GetWeight(room)))
            {
                if (creatureVariant.Instantiate(room, point) != null)
                {
                    room.Level.GenerationStats.RecordCreatureInstance(
                        creatureVariant.Name, creatureVariant.GenerationFlags);
                    return true;
                }
            }
        }

        return false;
    }
}

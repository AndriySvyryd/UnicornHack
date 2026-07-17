using CSharpScriptSerialization;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation.Map;

public class LevelConnection : ICSScriptSerializable
{
    public string BranchName
    {
        get;
        set;
    } = null!;

    public byte? Depth
    {
        get;
        set;
    }

    public char? Glyph
    {
        get;
        set;
    }

    public Directionality? Direction
    {
        get;
        set;
    }

    public static ConnectionComponent CreateSourceConnection(
        RoomComponent room, Point cell, LevelConnection? connectionDefinition)
    {
        var sourceLevel = room.Level;
        var sourceBranch = sourceLevel.Branch;
        var game = room.Game;
        var manager = game.Manager;

        string targetBranchName;
        byte targetDepth;
        Branch? resolvedBranch = null;
        if (connectionDefinition?.BranchName != null)
        {
            // The connecting fragment authored an explicit destination — use it verbatim.
            targetBranchName = connectionDefinition.BranchName;
            targetDepth = connectionDefinition.BranchName == sourceBranch.Name
                ? (byte)(sourceLevel.Depth + 1)
                : connectionDefinition.Depth ?? 1;
        }
        else
        {
            // A Shortcut room skips one level ahead (Depth + 2); a normal Exit advances by one.
            var depthDelta = (room.Features & RoomFeatures.Shortcut) != 0 ? 2 : 1;
            var sameBranchDepth = sourceLevel.Depth + depthDelta;
            if (sameBranchDepth <= sourceBranch.Length)
            {
                targetBranchName = sourceBranch.Name;
                targetDepth = connectionDefinition?.Depth ?? (byte)sameBranchDepth;
            }
            else
            {
                // The target depth runs past the end of this branch: skip into the next branch.
                var nextBranch = PickRandomNextBranch(sourceLevel);
                targetBranchName = nextBranch.Name;
                resolvedBranch = nextBranch;
                targetDepth = connectionDefinition?.Depth ?? 1;
            }
        }

        var direction = connectionDefinition?.Direction;
        var branch = game.GetBranch(targetBranchName) ??
                     (resolvedBranch ?? game.Services.Data.Branches.Get(targetBranchName)).Instantiate(game);

        var targetGameLevel = branch.Levels.FirstOrDefault(l => l.Depth == targetDepth);
        // Prefer the target level's entrance room: when the exit (or shortcut) room already
        // sourced a connection to this level, it created that entrance and points at it.
        var targetRoom = targetGameLevel?.Rooms.FirstOrDefault(r => (r.Features & RoomFeatures.Entrance) != 0)
                         ?? targetGameLevel?.Rooms.FirstOrDefault();
        Point? targetCell = null;
        if (targetRoom != null)
        {
            var matchingConnection = targetRoom.IncomingConnections
                .Select(c => c.Connection!)
                .FirstOrDefault(c => c.TargetRoomId == room.EntityId
                                     && c.TargetLevelX == null
                                     && AreMatchingDirectionality(c.Directionality, direction));
            if (matchingConnection != null)
            {
                if (matchingConnection.Directionality == null)
                {
                    matchingConnection.Directionality = GetInverse(direction ?? Directionality.Both);
                }

                if (direction == null)
                {
                    direction = GetInverse(matchingConnection.Directionality.Value);
                }

                matchingConnection.TargetCell = cell;
                targetCell = matchingConnection.Entity.Position!.Cell;
            }
        }
        else
        {
            var newGameLevel = LevelGenerator.CreateEmpty(branch, targetDepth, room.Level.GenerationRandom.Seed, manager);
            // Create a placeholder room so the connection has a target entity ID. The room a
            // cross-level connection lands on is, by definition, the target level's entrance,
            // so mark it now: GetEmptyEntranceRoom reuses this exact room when the level is
            // generated.
            targetRoom = LevelLayout.CreateEmptyRoom(newGameLevel, manager);
            targetRoom.Features = RoomFeatures.Entrance;
        }

        return CreateConnection(room, cell, targetRoom.EntityId, targetCell, direction, Primitives.Direction.Down);
    }

    /// <summary>
    ///     Picks a destination branch for an inter-branch connection sourced from
    ///     <paramref name="sourceLevel" />, weighted by each candidate branch's
    ///     <see cref="Branch.GenerationWeight" /> evaluated for the source branch / depth.
    /// </summary>
    private static Branch PickRandomNextBranch(GameLevel sourceLevel)
    {
        var sourceBranchName = sourceLevel.Branch.Name;
        var sourceDepth = sourceLevel.Depth;
        var candidates = sourceLevel.Game.Services.Data.Branches.GetAsList();
        // WeightedOrder skips zero-weight items and yields nothing when every weight is 0.
        var picked = sourceLevel.GenerationRandom.WeightedOrder(
            candidates,
            b => b.GetWeight(sourceBranchName, sourceDepth)).FirstOrDefault();
        return picked
            ?? throw new InvalidOperationException(
                $"No branch with positive GenerationWeight found for a connection from '{sourceBranchName}' depth {sourceDepth}. "
                + "Ensure at least one branch's GenerationWeight expression returns a positive value for this source.");
    }

    public static void CreateReceivingConnection(
        RoomComponent room, Point cell, ConnectionComponent incoming, Directionality? direction)
    {
        if (incoming.Directionality == null)
        {
            incoming.Directionality = GetInverse(direction ?? Directionality.Both);
        }

        if (direction == null)
        {
            direction = GetInverse(incoming.Directionality.Value);
        }

        incoming.TargetCell = cell;
        var incomingPosition = incoming.Entity.Position!;
        CreateConnection(room, cell, incomingPosition.RoomId, incomingPosition.Cell, direction, Primitives.Direction.Up);
    }

    public static bool AreMatchingDirectionality(Directionality? first, Directionality? second)
        => first == null
           || second == null
           || first == GetInverse(second.Value);

    /// <summary>
    ///     If <paramref name="room" /> has a still-dangling incoming connection
    ///     (<see cref="ConnectionComponent.TargetLevelX" /> still <c>null</c>) whose
    ///     branch / depth / directionality matches <paramref name="connectionDefinition" />,
    ///     wires it up via <see cref="CreateReceivingConnection" /> and returns <c>true</c>.
    ///     Returns <c>false</c> when no matching dangling incoming exists, leaving it to the
    ///     caller to decide whether to source a new connection instead. A <c>null</c>
    ///     <paramref name="connectionDefinition" /> matches the first dangling incoming.
    /// </summary>
    public static bool TryCreateReceivingConnection(
        RoomComponent room, Point cell, LevelConnection? connectionDefinition)
    {
        foreach (var connectionEntity in room.IncomingConnections)
        {
            var connection = connectionEntity.Connection!;
            if (connection.TargetLevelX == null)
            {
                var target = connection.TargetRoomEntity.Room!;
                if ((connectionDefinition?.BranchName == null
                     || connectionDefinition.BranchName == target.Level.BranchName)
                    && (connectionDefinition?.Depth == null
                        || connectionDefinition.Depth == target.Level.Depth)
                    && AreMatchingDirectionality(connection.Directionality, connectionDefinition?.Direction))
                {
                    CreateReceivingConnection(room, cell, connection, connectionDefinition?.Direction);
                    return true;
                }
            }
        }

        return false;
    }

    private static Directionality GetInverse(Directionality direction)
    {
        var inverse = Directionality.None;
        if ((direction & Directionality.Source) != 0)
        {
            inverse |= Directionality.Destination;
        }

        if ((direction & Directionality.Destination) != 0)
        {
            inverse |= Directionality.Source;
        }

        return inverse;
    }

    protected static ConnectionComponent CreateConnection(
        RoomComponent room, Point point, int targetRoomId, Point? targetCell,
        Directionality? cardinality, Direction direction)
    {
        var manager = room.Entity.Manager;
        using var connectionEntityReference = manager.CreateEntity();
        var connectionEntity = connectionEntityReference.Referenced;

        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = room.Entity;
        position.Cell = point;

        connectionEntity.Position = position;

        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomId = targetRoomId;
        connection.TargetCell = targetCell;
        connection.Directionality = cardinality;
        connection.Orientation = direction;

        connectionEntity.Connection = connection;

        return connection;
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<LevelConnection>(
            new Dictionary<string, Func<LevelConnection, object, bool>> { { nameof(Direction), (o, v) => v != null } });

    public ICSScriptSerializer GetSerializer() => Serializer;
}

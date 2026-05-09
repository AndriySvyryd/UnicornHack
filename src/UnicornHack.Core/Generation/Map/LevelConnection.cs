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
            targetBranchName = connectionDefinition.BranchName;
            targetDepth = connectionDefinition.BranchName == sourceBranch.Name
                ? (byte)(sourceLevel.Depth + 1)
                : connectionDefinition.Depth ?? 1;
        }
        else if (sourceLevel.Depth >= sourceBranch.Length)
        {
            var nextBranch = PickRandomNextBranch(game, sourceBranch.Name, sourceLevel.Depth);
            targetBranchName = nextBranch.Name;
            resolvedBranch = nextBranch;
            targetDepth = connectionDefinition?.Depth ?? 1;
        }
        else
        {
            targetBranchName = sourceBranch.Name;
            targetDepth = connectionDefinition?.Depth ?? (byte)(sourceLevel.Depth + 1);
        }

        var direction = connectionDefinition?.Direction;
        var branch = game.GetBranch(targetBranchName) ??
                     (resolvedBranch ?? game.Services.Data.Branches.Get(targetBranchName)).Instantiate(game);

        var targetGameLevel = branch.Levels.FirstOrDefault(l => l.Depth == targetDepth);
        var targetRoom = targetGameLevel?.Rooms.FirstOrDefault();
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
            // Create a placeholder room so the connection has a target entity ID.
            // It will be replaced when the target level is generated (by the layout's Fill).
            targetRoom = LevelLayout.CreateEmptyRoom(newGameLevel, manager);
        }

        return CreateConnection(room, cell, targetRoom.EntityId, targetCell, direction, Primitives.Direction.Down);
    }

    /// <summary>
    ///     Picks a destination branch for an inter-branch connection sourced from
    ///     <paramref name="sourceBranchName" /> at <paramref name="sourceDepth" />, weighted
    ///     by each candidate branch's <see cref="Branch.GenerationWeight" />.
    /// </summary>
    private static Branch PickRandomNextBranch(Game game, string sourceBranchName, int sourceDepth)
    {
        var candidates = game.Services.Data.Branches.GetAsList();
        // WeightedOrder skips zero-weight items and yields nothing when every weight is 0.
        var picked = game.Random.WeightedOrder(
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

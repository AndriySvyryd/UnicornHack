using Microsoft.EntityFrameworkCore;
using UnicornHack.Hubs;
using UnicornHack.Systems.Levels;
using Xunit;

namespace UnicornHack.Persistence;

public class LevelTest
{
    [Fact]
    public async Task LoadGameAsync_materializes_connection_targets()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        await persisted.ReloadAsync();

        var room = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        Assert.NotEmpty(room.Connections);
        foreach (var connectionEntity in room.Connections.Values)
        {
            var connection = connectionEntity.Connection!;
            Assert.Same(
                persisted.Game.Manager.FindEntity(connection.TargetRoomId),
                connection.TargetRoomEntity);
        }

        Assert.NotEmpty(GameQueryProcessor.SerializeLevelMap(persisted.PlayerEntity).Rooms);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Clean_removes_the_game_and_its_entities(bool useAsync)
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var gameId = persisted.Game.Id;
        var cancellationToken = TestContext.Current.CancellationToken;
        persisted.Game.ActingPlayerId = persisted.PlayerEntityId;
        persisted.Context.SaveChanges();
        Assert.Equal(persisted.PlayerEntityId, persisted.Game.ActingPlayerId);

        if (useAsync)
        {
            await persisted.Context.CleanAsync(persisted.Game, cancellationToken);
        }
        else
        {
            persisted.Context.Clean(persisted.Game);
        }

        Assert.Null(persisted.Game.ActingPlayerId);
        Assert.False(await persisted.Context.Games.AnyAsync(game => game.Id == gameId, cancellationToken));
        Assert.False(await persisted.Context.Entities.AnyAsync(entity => entity.GameId == gameId, cancellationToken));
    }

    [Fact]
    public void Partially_loaded_level_deletes_unloaded_room_rows()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var seededLevel = persisted.PlayerEntity.Position!.RoomEntity.Room!.Level;
        int unloadedRoomId;
        using (var roomReference = persisted.Game.Manager.CreateEntity())
        {
            unloadedRoomId = roomReference.Referenced.Id;
            var room = persisted.Game.Manager.CreateComponent<RoomComponent>(EntityComponent.Room);
            room.Level = seededLevel;
            roomReference.Referenced.Room = room;
            seededLevel.Rooms.Add(room);
        }
        persisted.Context.SaveChanges();
        persisted.Reload();

        var level = persisted.PlayerEntity.Position!.RoomEntity.Room!.Level;
        var persistedRoomIds = persisted.Context.RoomComponents
            .Where(
            room => room.GameId == level.GameId
                    && room.BranchName == level.BranchName
                    && room.Depth == level.Depth)
            .Select(room => room.EntityId)
            .ToList();

        Assert.True(level.Rooms.Count < persistedRoomIds.Count);
        Assert.DoesNotContain(unloadedRoomId, level.Rooms.Select(room => room.Entity.Id));

        // Production invokes level cleanup only after the player moved away. Keep the database deliberately
        // stale until SaveChanges so DeleteLevel must protect the locally moved player and inventory.
        var retainedRoom = persisted.Game.Manager.Rooms
            .Select(entity => entity.Room!)
            .First(room => room.Level != level);
        var retainedCell = persisted.Game.Manager.TravelSystem.FindCell(
            retainedRoom, persisted.Game.Manager.TravelSystem.IsActorFreeCell, maxRadius: int.MaxValue)!.Value;
        persisted.PlayerEntity.Position.SetRoomPosition(retainedRoom.Entity.Id, retainedCell);

        persisted.Game.Manager.TravelSystem.DeleteLevelContents(level, persisted.Game.Manager);
        persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);

        Assert.True(persisted.Context.Levels.Any(
            candidate => candidate.GameId == level.GameId
                         && candidate.BranchName == level.BranchName
                         && candidate.Depth == level.Depth));

        persisted.Context.SaveChanges();

        Assert.False(persisted.Context.Levels.Any(
            candidate => candidate.GameId == level.GameId
                         && candidate.BranchName == level.BranchName
                         && candidate.Depth == level.Depth));
        Assert.False(persisted.Context.RoomComponents.Any(
            room => room.GameId == level.GameId
                    && room.BranchName == level.BranchName
                    && room.Depth == level.Depth));
        Assert.True(persisted.Context.PlayerComponents.Any(
            player => player.GameId == level.GameId
                      && player.EntityId == persisted.PlayerEntityId));
    }
}

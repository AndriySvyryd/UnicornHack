using UnicornHack.Hubs;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack.TestUtilities;

public static class WebTestHelper
{
    public static Game CreateGame(uint? seed = null) => TestHelper.CreateGame(seed);

    public static RoomChangeBuilder SetupListeners(GameManager manager)
    {
        var builder = new RoomChangeBuilder();
        builder.RegisterOnGroups(manager);
        return builder;
    }

    public static void RegisterPlayer(
        GameEntity playerEntity, RoomChangeBuilder builder, GameManager manager)
        => builder.RegisterPlayerBuilder(playerEntity, manager);

    public static PlayerChange GetState(GameEntity playerEntity, GameServices services)
        => GameStateManager.GetState(playerEntity, services);

    public static Dictionary<int, List<TurnChangeSet>> PerformAction(
        PlayerComponent player,
        RoomChangeBuilder listener,
        GameServices services,
        ActorActionType action,
        int? target = null,
        int? target2 = null)
    {
        player.NextAction = action;
        player.NextActionTarget = target;
        player.NextActionTarget2 = target2;

        return GameStateManager.Turn(player, listener, services);
    }

    public static PlayerChange PerformAndVerify(
        PlayerComponent player,
        RoomChangeBuilder builder,
        GameServices services,
        ActorActionType action,
        int? target = null,
        int? target2 = null)
    {
        var baseline = ClientStateHelpers.Deserialize(GetState(player.Entity, services));
        var changeSets = PerformAction(player, builder, services, action, target, target2);

        if (changeSets.TryGetValue(player.Entity.Id, out var playerChangeSets))
        {
            ChangeSetAssert.ApplyAndAssertNoRedundantChanges(baseline, playerChangeSets);
        }
        else
        {
            Assert.Fail("Expected change sets for player, but received none");
        }

        var fresh = ClientStateHelpers.Deserialize(GetState(player.Entity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
        return fresh;
    }
}

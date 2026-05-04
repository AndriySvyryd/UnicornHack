using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Hubs;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;
using Xunit;

namespace UnicornHack.TestUtilities;

public static class WebTestHelper
{
    public static Game CreateGame(uint? seed = null)
    {
        var game = new Game
        {
            Random = new TestRandom { Seed = seed ?? 1 },
            InitialSeed = seed ?? 1,
            Repository = new TestRepository(),
            Services = new GameServices(new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()))
        };

        var manager = new GameManager { Game = game };
        manager.Initialize(new SequentialMessageQueue<GameManager>());
        game.Manager = manager;

        return game;
    }

    public static LevelComponent BuildLevel(string map, Game game)
    {
        var fragment = new DefiningMapFragment
        {
            Map = map,
            Layout = new EmptyLayout { Coverage = 0 },
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        var branch = new GameBranch { Game = game, Name = "test branch", Length = 1, Difficulty = 1 };
        game.Branches.Add(branch);

        fragment.EnsureInitialized(game);
        if (fragment.Height != 0)
        {
            fragment.LevelHeight = fragment.Height;
        }

        if (fragment.Width != 0)
        {
            fragment.LevelWidth = fragment.Width;
        }

        var level = LevelGenerator.CreateEmpty(branch, 0, game.Random.Seed, game.Manager);
        LevelGenerator.Generate(level, fragment);

        return level;
    }

    public static LevelChangeBuilder SetupListeners(GameManager manager)
    {
        var builder = new LevelChangeBuilder();
        builder.RegisterOnGroups(manager);
        return builder;
    }

    public static void RegisterPlayer(
        GameEntity playerEntity, LevelChangeBuilder builder, GameManager manager)
        => builder.RegisterPlayerBuilder(playerEntity, manager);

    public static PlayerChange GetState(GameEntity playerEntity, GameServices services)
        => GameStateManager.GetState(playerEntity, services);

    public static Dictionary<int, List<TurnChangeSet>> PerformAction(
        PlayerComponent player,
        LevelChangeBuilder listener,
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
        LevelChangeBuilder builder,
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

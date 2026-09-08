using System.Threading;
using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Faculties;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

using UnicornHack.Systems.Time;

namespace UnicornHack;

public static class TestHelper
{
    private static int TestAbilityId;

    public static void AdvanceToNextPlayerTurn(GameManager manager)
    {
        while (manager.Game.ActingPlayer == null
               && manager.Players.Any(p => p.Being!.IsAlive))
        {
            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);
        }
    }

    /// <summary>
    ///     Advances the clock to the actor's next action tick and performs its turn, mirroring how the time
    ///     system drives a single actor in production (<see cref="AdvanceTurnMessage" />). Using this instead
    ///     of enqueuing <see cref="PerformActionMessage" /> directly keeps the elapsed time between an actor's
    ///     turns realistic, so build-ups fade and statuses decay as they would in a running game rather than
    ///     at a frozen clock.
    /// </summary>
    public static void PerformTurn(GameEntity actor, GameManager manager)
    {
        if (TimeSystem.GetTick(actor) is { } tick)
        {
            manager.Game.CurrentTick = tick;
        }

        if (actor.AI != null)
        {
            PerformActionMessage.EnqueueAI(actor, manager);
        }
        else
        {
            PerformActionMessage.EnqueuePlayer(actor, manager);
        }

        manager.Queue.ProcessQueue(manager);
    }

    public static Game CreateGame(uint? seed = null)
    {
        var game = new Game
        {
            Random = new TestRandom { Seed = seed ?? 1 },
            Options = { InitialSeed = seed ?? 1 },
            Repository = new TestRepository(),
            Services = new GameServices(new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()), new TestDataProvider())
        };

        game.Manager = CreateGameManager(game);

        return game;
    }

    public static GameManager CreateGameManager(Game? game = null)
    {
        var manager = new GameManager { Game = game ?? CreateGame() };
        manager.Initialize(new SequentialMessageQueue<GameEntity>());
        return manager;
    }

    public static RoomComponent BuildRoom(string map = "", Game? game = null, uint? seed = null, byte depth = 0)
        => BuildRoomWithChunks(map, game, seed, depth).Item1;

    public static (GameEntity Target, GameManager Manager) CreateEffectTarget()
    {
        var room = BuildRoom(".");
        var target = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 0));
        var manager = target.Manager;
        manager.Queue.ProcessQueue(manager);
        return (target, manager);
    }

    public static GameEntity CreateSourceAbility(GameEntity owner)
    {
        var template = new Ability
        {
            Name = "test source ability " + Interlocked.Increment(ref TestAbilityId),
            Effects = []
        };
        var ability = template.AddToAffectable(owner);
        owner.Manager.Queue.ProcessQueue(owner.Manager);
        return ability.Entity;
    }

    public static ActiveComponentEffect ApplyAbilityLevelDecrease(
        GameEntity target, int amount = 1,
        string? abilityName = null, AbilityType? abilityType = null,
        EffectDuration duration = EffectDuration.Infinite, int durationAmount = 1)
    {
        var decrease = new DecreaseAbilityLevel
        {
            AbilityName = abilityName,
            AbilityType = abilityType,
            Amount = amount.ToString(CultureInfo.InvariantCulture),
            Duration = duration,
            DurationAmount = duration is EffectDuration.UntilTimeout or EffectDuration.UntilXPGained
                ? durationAmount.ToString(CultureInfo.InvariantCulture)
                : null
        };
        var template = new Ability
        {
            Name = "test ability decrease " + Interlocked.Increment(ref TestAbilityId),
            Activation = ActivationType.Manual,
            Effects = [decrease]
        };
        var ability = template.AddToAffectable(target);
        target.Manager.Queue.ProcessQueue(target.Manager);
        ApplyEffect(Assert.Single(ability.Effects), target, target, target.Manager);
        target.Manager.Queue.ProcessQueue(target.Manager);

        return Assert.Single(
            target.ActiveEffects!.ComponentEffects,
            record => record.EffectType == EffectType.DecreaseAbilityLevel
                      && record.SourceAbilityEntityId == ability.EntityId);
    }

    public static bool IsLevelExpressionGrant(ActiveComponentEffect record)
        => SkillAbilitiesSystem.IsLevelExpressionGrant(record);

    /// <summary>
    ///     Builds a <see cref="RoomParameters" /> sized to
    ///     <paramref name="room" /> with default terrain and trivial generators. Tests that
    ///     construct <see cref="Chunk" />s directly should pass the result so
    ///     <see cref="RoomParameters.ChunkCells" /> mirrors production
    ///     bookkeeping (every authored chunk claims its cells on the room's ownership map).
    /// </summary>
    public static RoomParameters CreateRoomParameters(
        RoomComponent room, MapFeature defaultTerrain = MapFeature.Default)
        => new(
            RoomWidth: room.Width == 0 ? (byte)1 : room.Width,
            RoomHeight: room.Height == 0 ? (byte)1 : room.Height,
            DefaultTerrain: defaultTerrain,
            DefaultPathTerrain: MapFeature.RockFloor,
            DefaultWall: MapFeature.RockWall,
            CreatureGenerator: new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator: new ItemGenerator { ExpectedInitialCount = 0 });

    /// <summary>
    ///     Builds a demand-only anchor list (every direction is a real doorway demand, no
    ///     positioning-only anchors) by unstacking <paramref name="demand" /> into one
    ///     anchor per required doorway. Test convenience for callers that only have a per-direction
    ///     count; production code builds anchored demands directly in <c>LevelLayout</c>.
    /// </summary>
    public static List<RoomDoorwayAnchor> FromDemand(IReadOnlyDictionary<Direction, int> demand)
    {
        var anchors = new List<RoomDoorwayAnchor>();
        foreach (var (direction, count) in demand)
        {
            for (var i = 0; i < count; i++)
            {
                anchors.Add(new RoomDoorwayAnchor(direction, IsDoorwayDemand: true));
            }
        }

        return anchors;
    }

    public static (RoomComponent, IReadOnlyList<Chunk>) BuildRoomWithChunks(
        string map = "", Game? game = null, uint? seed = null, byte depth = 0)
    {
        game ??= CreateGame(seed);

        var fragment = new DefiningMapFragment
        {
            Map = map,
            // Synthetic test rooms aren't part of a connected dungeon — opt out of the
            // doorway-required guard so trivial maps (e.g. ".") that have no walls don't
            // trip MapFragment.TryPlaceStaticMap's connectivity check.
            NoRandomDoorways = true,
            CreatureGenerator = new CreatureGenerator { ExpectedInitialCount = 0 },
            ItemGenerator = new ItemGenerator { ExpectedInitialCount = 0 }
        };

        // Declare a connection definition for any explicit connection glyph present in
        // the map so ConnectingMapFragment.BuildChunk doesn't add an extra anonymous
        // random connection on top of the authored one.
        foreach (var glyph in (ReadOnlySpan<char>)['<', '>'])
        {
            if (map.Contains(glyph))
            {
                fragment.Connections.Add(new LevelConnection { Glyph = glyph });
            }
        }

        return BuildRoom(fragment, game, depth);
    }

    /// <summary>
    ///     Registers <paramref name="fragment" /> on a fresh test branch and generates a
    ///     single-room level. The supplied <paramref name="game" /> gets its own isolated
    ///     <see cref="IDataProvider.DefiningMapFragments" /> loader (via
    ///     <see cref="TestDataProvider" />) so concurrent tests adding fragments don't
    ///     interfere. The fragment's <see cref="DefiningMapFragment.GenerationWeight" /> is
    ///     rewritten to match only the synthetic branch; <see cref="LevelGenerator.PickFragmentForRoom" />
    ///     then picks it for any room in that branch.
    /// </summary>
    public static (RoomComponent, IReadOnlyList<Chunk>) BuildRoom(
        DefiningMapFragment fragment, Game game, byte depth = 0, byte branchLength = 10,
        RoomLayout? roomLayout = null)
    {
        var id = Interlocked.Increment(ref _testBranchCounter);
        var branchName = "test branch " + id;

        var testBranch = new Branch
        {
            Name = branchName,
            Length = branchLength,
            Difficulty = 1,
            Layout = new EmptyLevelLayout()
        };
        if (roomLayout is not null)
        {
            testBranch.RoomLayout = roomLayout;
        }
        var branchEntity = testBranch.Instantiate(game);
        game.Branches.Add(branchEntity);

        fragment.Name = string.IsNullOrEmpty(fragment.Name) ? "test fragment " + id : fragment.Name;
        fragment.GenerationWeight = $"$branch == '{branchName}' ? 1 : 0";
        fragment.EnsureInitialized();
        if (fragment.Height != 0)
        {
            fragment.RoomHeight = fragment.Height;
        }

        if (fragment.Width != 0)
        {
            fragment.RoomWidth = fragment.Width;
        }

        // Add to whichever DefiningMapFragments loader the game's services already point at,
        // installing a fresh isolated TestDataProvider on first use so the addition doesn't
        // leak into parallel tests via the static singleton. Tests that explicitly preserve
        // the default (static) provider keep that behaviour.
        var testProvider = (TestDataProvider)game.Services.Data;
        testProvider.DefiningMapFragments.Add(fragment);
        testProvider.Branches.Add(testBranch);

        var gameLevel = LevelGenerator.CreateEmpty(branchEntity, depth, game.Random.Seed, game.Manager);
        var chunks = LevelGenerator.Generate(gameLevel);
        Assert.NotNull(chunks);

        return (gameLevel.Rooms.First(), chunks);
    }

    private static int _testBranchCounter;

    public static GameEntity ActivateAbility(
        string abilityName, GameEntity activatorEntity, GameManager manager, int? slot = null)
    {
        var ability = manager.AffectableAbilitiesIndex[(activatorEntity.Id, abilityName)]!;
        Assert.NotNull(ability);

        return ActivateAbility(ability, activatorEntity, manager, slot);
    }

    public static GameEntity ActivateAbility(
        GameEntity abilityEntity, GameEntity activatorEntity, GameManager manager, int? slot = null)
    {
        slot ??= abilityEntity.Ability!.Slot;

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = abilityEntity;
        setSlotMessage.Slot = slot ?? 2;
        manager.Enqueue(setSlotMessage);

        var activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = activatorEntity;
        activateAbilityMessage.TargetEntity = activatorEntity;
        activateAbilityMessage.AbilityEntity = abilityEntity;

        manager.Enqueue(activateAbilityMessage);

        return abilityEntity;
    }

    public static void ApplyEffect(
        GameEntity effectEntity, GameEntity activatorEntity, GameEntity targetEntity, GameManager manager,
        Point? targetCell = null)
    {
        var effect = effectEntity.Effect!;
        if (effect.CombinationFunction == ValueCombinationFunction.Default)
        {
            effect.CombinationFunction = Effect.GetMetadata(effect.EffectType).CombinationFunction;
        }

        var createAbility = effectEntity.Effect?.ContainingAbility == null;
        using var abilityReference = createAbility
            ? manager.CreateEntity()
            : default;
        var abilityEntity = effectEntity.Effect?.ContainingAbility ?? abilityReference.Referenced;
        if (abilityEntity.Ability == null)
        {
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = $"test effect {abilityEntity.Id}";
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = activatorEntity;
            abilityEntity.Ability = ability;
        }

        if (effectEntity.Effect!.ContainingAbilityId == null)
        {
            effectEntity.Effect.ContainingAbilityId = abilityEntity.Id;
        }

        if (effectEntity.Effect.ContainingAbility?.Ability == null
            || !abilityEntity.Ability!.Effects.Contains(effectEntity))
        {
            throw new InvalidOperationException("Test effect was not attached to its containing ability.");
        }

        if (createAbility)
        {
            var context = new EffectApplicationContext(
                manager.EffectApplicationSystem, effectEntity, targetEntity, targetCell,
                activatorEntity, result: null, pretend: false);
            Effect.GetMetadata(effectEntity.Effect.EffectType).ApplicationHandler(context);
        }
        else
        {
            var message = AbilityActivatedMessage.Create(manager);
            message.ActivatorEntity = activatorEntity;
            message.TargetEntity = targetEntity;
            message.TargetCell = targetCell;
            message.AbilityEntity = abilityEntity;
            message.EffectsToApply = [effectEntity];
            message.Outcome = ApplicationOutcome.Success;
            manager.Enqueue(message);
        }
    }

    public static void ApplyEffect(
        EffectType effectType, GameEntity activatorEntity, GameEntity targetEntity, GameManager manager,
        Action<EffectComponent> configure, Point? targetCell = null)
    {
        using var effectReference = manager.CreateEntity();
        var effectEntity = effectReference.Referenced;
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = effectType;
        configure(effect);
        effectEntity.Effect = effect;

        ApplyEffect(effectEntity, activatorEntity, targetEntity, manager, targetCell);
    }

    // Attaches a no-effect spell ability to a creature - a stand-in for the removed ArcaneSpell placeholder -
    // for tests that gate on or log spell casting.
    public static void AddSpellAbility(GameEntity casterEntity)
        => new Ability
        {
            Name = "test spell",
            Type = AbilityType.RangedSpell,
            Activation = ActivationType.Targeted,
            Trigger = ActivationType.OnRangedAttack,
            Range = 20,
            Action = AbilityAction.Spell,
            SuccessCondition = AbilitySuccessCondition.NormalAttack,
            Accuracy = "5+PerceptionModifier()",
            Cooldown = 100,
            Delay = "100*SpeedModifier()",
            Effects = new List<Effect> { new Heal { Amount = "0" } }
        }.AddToAffectable(casterEntity);
}

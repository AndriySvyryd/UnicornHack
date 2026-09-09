using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using MessagePack;
using MessagePack.Resolvers;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Knowledge;

namespace UnicornHack.Web.PerformanceTests;

public enum ProjectionScenario
{
    Wait,
    Movement,
    Combat,
    TerrainOnly
}

public readonly record struct ProjectionMetrics(int PayloadBytes, int EmittedDtoCount);

/// <summary>
///     Measures candidate capture, projection materialization, and production MessagePack
///     serialization. One world is reused for all operations in a parameter case.
/// </summary>
[MemoryDiagnoser]
public class ProjectionSerializationPerfTest
{
    private const int DefaultOperationsPerInvocation = 64;

    private const string Map = @"
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................";

    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithSecurity(MessagePackSecurity.UntrustedData)
        .WithResolver(CompositeResolver.Create(
            GeneratedResolver.Instance,
            BuiltinResolver.Instance));

    private static readonly Dictionary<(ProjectionScenario Scenario, int NpcCount, int PlayerCount), ProjectionMetrics>
        Metrics = [];

    [Params(ProjectionScenario.Wait, ProjectionScenario.Movement, ProjectionScenario.Combat,
        ProjectionScenario.TerrainOnly)]
    public ProjectionScenario Scenario;

    [Params(24)]
    public int NpcCount;

    [Params(1)]
    public int PlayerCount;

    private Game _game = null!;
    private RoomComponent _room = null!;
    private RoomChangeBuilder _builder = null!;
    private List<GameEntity> _players = null!;
    private List<GameEntity> _creatures = null!;
    private List<EffectAccumulator> _statusAccumulators = null!;
    private GameEntity _itemKnowledge = null!;

    [GlobalSetup]
    public void Setup()
    {
        Metrics[(Scenario, NpcCount, PlayerCount)] = default;
        SetupState();
    }

    private void SetupState()
    {
        _game = TestHelper.CreateGame();
        _room = TestHelper.BuildRoom(Map, _game);
        var manager = _game.Manager;

        _players = new List<GameEntity>(PlayerCount);
        _statusAccumulators = new List<EffectAccumulator>(PlayerCount);
        for (var i = 0; i < PlayerCount; i++)
        {
            var player = PlayerRace.InstantiatePlayer(
                $"Bench {i + 1}", i == 0 ? Sex.Male : Sex.Female,
                _room, new Point((byte)(9 + i * 2), 7));
            ItemData.LongSword.Instantiate(player);
            _players.Add(player);
        }

        _creatures = new List<GameEntity>(NpcCount);
        var center = new Point(10, 7);
        for (var radius = 1; radius < 7 && _creatures.Count < NpcCount; radius++)
        {
            for (var dx = -radius; dx <= radius && _creatures.Count < NpcCount; dx++)
            {
                for (var dy = -radius; dy <= radius && _creatures.Count < NpcCount; dy++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                    {
                        continue;
                    }

                    var cell = new Point((byte)(center.X + dx), (byte)(center.Y + dy));
                    if (_players.Any(player =>
                            player.Position!.Cell == cell
                            || (player.Position.CellX + 1 == cell.X && player.Position.CellY == cell.Y)))
                    {
                        continue;
                    }

                    _creatures.Add(CreatureData.HumanWarrior.Instantiate(_room, cell));
                }
            }
        }

        var itemCell = new Point(10, 8);
        ItemData.FlaskOfHealing.Instantiate(_room, itemCell);
        manager.Queue.ProcessQueue(manager);
        var itemEntity = _room.Items[itemCell];
        foreach (var creature in _creatures)
        {
            EnsureKnowledge(creature, manager);
        }

        _itemKnowledge = EnsureKnowledge(itemEntity, manager);

        if (Scenario == ProjectionScenario.Combat)
        {
            foreach (var player in _players)
            {
                var accumulator = manager.RentEffectAccumulator(
                    player.GameId, player.Id, EffectType.Stun, value: 10);
                player.ActiveEffects!.Accumulators.Add(accumulator);
                _statusAccumulators.Add(accumulator);
            }
        }

        _builder = WebTestHelper.SetupListeners(manager);
    }

    [Benchmark]
    public ProjectionMetrics BuildAndSerialize()
    {
        var payloadBytes = 0;
        var emittedDtoCount = 0;
        var operationCount = Scenario switch
        {
            ProjectionScenario.Wait => 128,
            ProjectionScenario.Movement => 512,
            ProjectionScenario.TerrainOnly => 1024,
            _ => DefaultOperationsPerInvocation
        };
        for (var operation = 0; operation < operationCount; operation++)
        {
            _builder.Reset(_game.Manager);
            using (_game.Manager.Queue.Suspend())
            {
                StageScenario();
            }

            foreach (var playerBuilder in _builder.PlayerBuilders)
            {
                var changeSets = new List<TurnChangeSet>(1)
                {
                    new()
                    {
                        PlayerState = playerBuilder.BuildChangeSet(_game.Manager, _game.Services),
                        Events = _builder.EventBuilder.GetEvents(playerBuilder.PlayerEntityId)
                    }
                };

                if (operation == 0)
                {
                    emittedDtoCount += CountDtos(changeSets);
                    payloadBytes += MessagePackSerializer.Serialize(changeSets, Options).Length;
                }
                else
                {
                    MessagePackSerializer.Serialize(changeSets, Options);
                }
            }
        }

        var metrics = new ProjectionMetrics(payloadBytes, emittedDtoCount);
        var key = (Scenario, NpcCount, PlayerCount);
        if (Metrics[key] == default)
        {
            Metrics[key] = metrics;
        }

        return metrics;
    }

    internal static bool TryGetMetrics(BenchmarkCase benchmarkCase, out ProjectionMetrics metrics)
    {
        metrics = default;
        if (benchmarkCase.Descriptor.Type != typeof(ProjectionSerializationPerfTest))
        {
            return false;
        }

        var parameters = benchmarkCase.Parameters.Items;
        var scenario = (ProjectionScenario)parameters.Single(parameter => parameter.Name == nameof(Scenario)).Value;
        var npcCount = (int)parameters.Single(parameter => parameter.Name == nameof(NpcCount)).Value;
        var playerCount = (int)parameters.Single(parameter => parameter.Name == nameof(PlayerCount)).Value;
        return Metrics.TryGetValue((scenario, npcCount, playerCount), out metrics);
    }

    private void StageScenario()
    {
        var manager = _game.Manager;
        switch (Scenario)
        {
            case ProjectionScenario.Wait:
                foreach (var creature in _creatures)
                {
                    creature.AI!.NextActionTick = (creature.AI.NextActionTick ?? 0) + 1;
                }

                foreach (var player in _players)
                {
                    player.Player!.NextActionTick = (player.Player.NextActionTick ?? 0) + 1;
                    player.Being!.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot]
                        .Ability!.Noise++;
                }

                _itemKnowledge.Position!.Cell = _itemKnowledge.Position.CellY == 8
                    ? new Point(10, 9)
                    : new Point(10, 8);
                return;
            case ProjectionScenario.Movement:
                var moveEast = _players[0].Position!.CellX == 9;
                foreach (var player in _players)
                {
                    var position = player.Position!;
                    var cell = new Point(
                        (byte)(position.CellX!.Value + (moveEast ? 1 : -1)),
                        position.CellY!.Value);
                    position.Cell = cell;
                    position.Knowledge!.Position!.Cell = cell;
                }

                return;
            case ProjectionScenario.Combat:
                for (var i = 0; i < _players.Count; i++)
                {
                    var player = _players[i];
                    player.Being!.HitPoints = player.Being.HitPoints == player.Being.HitPointMaximum
                        ? GetDamagedHitPoints(player.Being)
                        : player.Being.HitPointMaximum;
                    player.Player!.NextActionTick = (player.Player.NextActionTick ?? 0) + 100 + i;
                    player.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot]
                        .Ability!.CooldownTick = player.Being.SlottedAbilities[
                            AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!.CooldownTick is { } cooldown
                                ? cooldown + 1
                                : manager.Game.CurrentTick + 250 + i;
                    _statusAccumulators[i].Value += 10;
                }

                for (var i = 0; i < _creatures.Count; i++)
                {
                    var creature = _creatures[i];
                    creature.Being!.HitPoints = creature.Being.HitPoints == creature.Being.HitPointMaximum
                        ? GetDamagedHitPoints(creature.Being)
                        : creature.Being.HitPointMaximum;
                    creature.AI!.NextActionTick = (creature.AI.NextActionTick ?? 0) + 100 + i;
                    if (creature.Being.SlottedAbilities.TryGetValue(
                            AbilitySlottingSystem.DefaultMeleeAttackSlot, out var attack))
                    {
                        attack.Ability!.CooldownTick = attack.Ability.CooldownTick is { } cooldown
                            ? cooldown + 1
                            : manager.Game.CurrentTick + 250 + i;
                    }
                }

                return;
            case ProjectionScenario.TerrainOnly:
                _room.KnownTerrainChanges ??= [];
                _room.TerrainChanges ??= [];
                _room.NeighborChanges ??= [];
                _room.VisibleTerrainChanges ??= [];
                _room.KnownTerrainChanges.Clear();
                _room.TerrainChanges.Clear();
                _room.NeighborChanges.Clear();
                _room.VisibleTerrainChanges.Clear();
                for (short i = 0; i < 16; i++)
                {
                    _room.KnownTerrain[i] = MapFeature.StoneFloor;
                    _room.VisibleTerrain[i] = 1;
                    _room.KnownTerrainChanges[i] = MapFeature.StoneFloor;
                    _room.VisibleTerrainChanges[i] = 1;
                }

                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static int GetDamagedHitPoints(BeingComponent being)
        => Math.Max(1, being.HitPoints - Math.Max(1, being.HitPointMaximum / 4));

    private static GameEntity EnsureKnowledge(GameEntity entity, GameManager manager)
    {
        if (entity.Position!.Knowledge is { } existing)
        {
            return existing;
        }

        using var knowledgeReference = manager.CreateEntity();
        var knowledgeEntity = knowledgeReference.Referenced;
        var knowledge = manager.CreateComponent<KnowledgeComponent>(EntityComponent.Knowledge);
        knowledge.KnownEntityId = entity.Id;
        knowledge.SensedType = SenseType.Sight;
        knowledge.IsIdentified = true;
        knowledgeEntity.Knowledge = knowledge;

        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.SetRoomPosition(entity.Position.RoomId, entity.Position.Cell!.Value);
        position.Heading = entity.Position.Heading;
        knowledgeEntity.Position = position;
        return knowledgeEntity;
    }

    private static int CountDtos(List<TurnChangeSet> changeSets)
    {
        var count = 0;
        foreach (var changeSet in changeSets)
        {
            count += 2;
            count += changeSet.Events?.Count ?? 0;
            var player = changeSet.PlayerState;
            count += player.Abilities?.Count ?? 0;
            count += player.Effects?.Count ?? 0;
            count += player.Log?.Count ?? 0;
            if (player.Room is { } room)
            {
                count++;
                count += room.Actors?.Count ?? 0;
                count += room.Items?.Count ?? 0;
                count += room.Connections?.Count ?? 0;
                count += room.LevelMap == null ? 0 : 1;
                count += room.LevelMapChanges == null ? 0 : 1;
            }
        }

        return count;
    }
}

public sealed class ProjectionMetricColumn(
    string columnName,
    string legend,
    Func<ProjectionMetrics, int> getValue) : IColumn
{
    public string Id => $"Projection.{columnName}";
    public string ColumnName => columnName;
    public bool AlwaysShow => false;
    public ColumnCategory Category => ColumnCategory.Metric;
    public int PriorityInCategory => 0;
    public bool IsNumeric => true;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => legend;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        => GetValue(summary, benchmarkCase, summary.Style);

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        => ProjectionSerializationPerfTest.TryGetMetrics(benchmarkCase, out var metrics)
            ? getValue(metrics).ToString(style.CultureInfo)
            : "-";

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public bool IsAvailable(Summary summary)
        => summary.BenchmarksCases.Any(benchmarkCase =>
            benchmarkCase.Descriptor.Type == typeof(ProjectionSerializationPerfTest));
}
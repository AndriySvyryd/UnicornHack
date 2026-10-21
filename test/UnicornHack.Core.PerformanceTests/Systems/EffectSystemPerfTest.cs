using UnicornHack.Systems.Abilities;
using BenchmarkDotNet.Attributes;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests.Systems;

[MemoryDiagnoser]
public class EffectSystemPerfTest
{
    private DiedMessage _deathMessage = null!;

    private const int ApplyStatusOperations = 1536;
    private const int StatusApplicationsPerTarget = 3;
    private const int StatusTargetCount = 16;
    private const int ExpiryScanOperations = 8192;
    private const int MovementExtensionOperations = 256;
    private const int ProcessTurnOperations = 512;
    private const int QueryLevelOperations = 16384;
    private const int ResolvePropertyOperations = 4096;
    private const int ExpressionRefreshOperations = 256;

    private ActivePropertyEffect _bleedEffect = null!;
    private GameEntity _player = null!;
    private GameManager _manager = null!;
    private GameEntity[] _statusTargets = null!;
    private EntityChange<GameEntity> _healthChange;
    private EntityChange<GameEntity> _unrelatedChange;
    private IEntityChangeListener<GameEntity> _abilityLevelListener = null!;

    [Params(16)]
    public int EffectCount { get; set; }

    [GlobalSetup(Targets =
        [nameof(Resolve_property), nameof(Process_status_heavy_expiry_scan), nameof(Process_actor_turn),
            nameof(Query_effective_ability_level), nameof(Extend_movement_reactive_effects)])]
    public void Setup()
    {
        var room = TestHelper.BuildRoom(".");
        _player = PlayerRace.InstantiatePlayer("Benchmark", Sex.Male, room, new Point(0, 0));
        _manager = _player.Manager;
        _manager.Queue.ProcessQueue(_manager);

        for (var i = 0; i < EffectCount; i++)
        {
            _player.ActiveEffects!.AddPropertyEffect(
                EffectType.ChangeProperty, nameof(BeingComponent.Evasion),
                ValueCombinationFunction.Sum,
                magnitudePrimary: i % 3,
                expirationTarget: 100000 + i,
                expirationEvent: EffectDuration.UntilTimeout);
        }

        _bleedEffect = _player.ActiveEffects!.AddPropertyEffect(
            EffectType.BleedDamage,
            expirationTarget: 100000,
            expirationEvent: EffectDuration.UntilTimeout);
        _manager.AbilityLevelSystem.AddAbilityEffect(
            _player, AbilityData.Acrobatics);
        _player.ActiveEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, AbilityData.Acrobatics.Name, level: 1);
    }

    [Benchmark]
    public int Resolve_property()
    {
        var result = 0;
        for (var i = 0; i < ResolvePropertyOperations; i++)
        {
            _player.ActiveEffects!.TryRefresh(
                nameof(BeingComponent.Evasion), isBool: false, out _, out var value);
            result += value;
        }

        return result;
    }

    [Benchmark]
    public void Process_status_heavy_expiry_scan()
    {
        for (var i = 0; i < ExpiryScanOperations; i++)
        {
            _manager.EffectApplicationSystem.ExpireTimedPropertyEffects(_player, _manager);
        }
    }

    [Benchmark]
    public void Process_actor_turn()
    {
        for (var i = 0; i < ProcessTurnOperations; i++)
        {
            _player.Player!.NextAction = ActorActionType.Wait;
            TestHelper.PerformTurn(_player, _manager);
        }
    }

    [GlobalSetup(Target = nameof(Apply_status))]
    public void Setup_status_target()
    {
        const int roomWidth = 16;
        var row = new string('.', roomWidth);
        var room = TestHelper.BuildRoom(row);
        _statusTargets = new GameEntity[StatusTargetCount];
        for (var targetIndex = 0; targetIndex < _statusTargets.Length; targetIndex++)
        {
            var target = PlayerRace.InstantiatePlayer(
                $"Status target {targetIndex}",
                Sex.Male,
                room,
                new Point((byte)(targetIndex % roomWidth), (byte)(targetIndex / roomWidth)));

            for (var i = 0; i < EffectCount; i++)
            {
                target.ActiveEffects!.AddPropertyEffect(
                    EffectType.ChangeProperty, nameof(BeingComponent.Evasion),
                    ValueCombinationFunction.Sum,
                    magnitudePrimary: i % 3,
                    expirationTarget: 100000 + i,
                    expirationEvent: EffectDuration.UntilTimeout);
            }

            _statusTargets[targetIndex] = target;
            if ((targetIndex + 1) % roomWidth == 0)
            {
                room.Entity.Manager.Queue.ProcessQueue(room.Entity.Manager);
            }
        }
    }

    [Benchmark]
    public void Apply_status()
    {
        var rounds = ApplyStatusOperations / (StatusTargetCount * StatusApplicationsPerTarget);
        for (var round = 0; round < rounds; round++)
        {
            foreach (var target in _statusTargets)
            {
                var amount = round + 1;
                target.Manager.EffectApplicationSystem.ApplyStatus(
                    EffectApplicationContext.CreateStatus(EffectType.Slow, target, amount));
                target.Manager.EffectApplicationSystem.ApplyStatus(
                    EffectApplicationContext.CreateStatus(EffectType.Confuse, target, amount));
                target.Manager.EffectApplicationSystem.ApplyStatus(
                    EffectApplicationContext.CreateStatus(EffectType.Stun, target, amount));
            }
        }
    }

    [Benchmark]
    public int Query_effective_ability_level()
    {
        var result = 0;
        for (var i = 0; i < QueryLevelOperations; i++)
        {
            result += AbilityLevelSystem.GetAbilityLevel(_player, AbilityData.Acrobatics.Name);
        }

        return result;
    }

    [GlobalSetup(Targets = [nameof(Refresh_ability_health), nameof(Refresh_ability_level_stats), nameof(Refresh_ability_unrelated_property)])]
    public void Setup_ability_expressions()
    {
        var game = TestHelper.CreateGame();
        var abilities = Enumerable.Range(0, 100)
            .Select(index => new Ability { Name = $"benchmark static ability {index}", Type = AbilityType.Buff, Effects = [] })
            .ToArray();
        var dependent = new LeveledAbility
        {
            Name = "benchmark dynamic ability",
            Type = AbilityType.Buff,
            LevelExpression = "AbilityLevel('benchmark static ability 0')>0?1+($hitPoints<=$hitPointMaximum/2?1:0)+($levelDeaths>0?1:0):0",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [], [3] = [] }
        };
        ((TestDataProvider)game.Services.Data).AddAbilities([.. abilities, dependent]);
        var room = TestHelper.BuildRoom(".", game);
        _player = CreatureData.Boggart.Instantiate(room, new Point(0, 0));
        _manager = game.Manager;
        _manager.Queue.ProcessQueue(_manager);
        foreach (var ability in abilities)
        {
            if (_player.ActiveEffects!.Abilities.Count >= 101)
            {
                break;
            }

            _manager.AbilityLevelSystem.AddAbilityEffect(_player, ability);
            _manager.Queue.ProcessQueue(_manager);
        }

        if (_player.ActiveEffects!.Abilities.Count != 101
            || AbilityLevelSystem.GetAbilityLevel(_player, dependent.Name) != 1)
        {
            throw new InvalidOperationException("Expected 100 static abilities and one dynamic ability.");
        }

        _abilityLevelListener = _manager.AbilityLevelSystem;
        _deathMessage = new DiedMessage { BeingEntity = _player };
        _healthChange = new EntityChange<GameEntity>(_player, IPropertyValueChanges.Create(
            new PropertyValueChange<int>(_player.Being!, nameof(BeingComponent.HitPoints),
                _player.Being!.HitPoints, _player.Being.HitPoints)));
        _unrelatedChange = new EntityChange<GameEntity>(_player, IPropertyValueChanges.Create(
            new PropertyValueChange<int>(_player.Being, nameof(BeingComponent.EnergyPoints),
                _player.Being.EnergyPoints, _player.Being.EnergyPoints)));
    }

    [GlobalCleanup(Targets = [nameof(Refresh_ability_health), nameof(Refresh_ability_level_stats), nameof(Refresh_ability_unrelated_property)])]
    public void Cleanup_ability_expressions()
    {
        _healthChange.PropertyChanges.Return();
        _unrelatedChange.PropertyChanges.Return();
        _deathMessage.Clean();
    }

    [Benchmark(OperationsPerInvoke = ExpressionRefreshOperations)]
    public void Refresh_ability_health()
    {
        for (var iteration = 0; iteration < ExpressionRefreshOperations; iteration++)
        {
            _abilityLevelListener.OnPropertyValuesChanged(_healthChange);
        }
    }

    [Benchmark(OperationsPerInvoke = ExpressionRefreshOperations)]
    public void Refresh_ability_level_stats()
    {
        for (var iteration = 0; iteration < ExpressionRefreshOperations; iteration++)
        {
            _manager.AbilityLevelSystem.Process(_deathMessage, _manager);
        }
    }

    [Benchmark(OperationsPerInvoke = ExpressionRefreshOperations)]
    public void Refresh_ability_unrelated_property()
    {
        for (var iteration = 0; iteration < ExpressionRefreshOperations; iteration++)
        {
            _abilityLevelListener.OnPropertyValuesChanged(_unrelatedChange);
        }
    }

    [IterationSetup(Target = nameof(Extend_movement_reactive_effects))]
    public void Reset_movement_reactive_effect()
        => _bleedEffect.ExpirationTarget = 100000;

    [Benchmark]
    public void Extend_movement_reactive_effects()
    {
        for (var i = 0; i < MovementExtensionOperations; i++)
        {
            _manager.EffectApplicationSystem.ExtendEffectsOnMovement(_player, _manager);
        }
    }
}

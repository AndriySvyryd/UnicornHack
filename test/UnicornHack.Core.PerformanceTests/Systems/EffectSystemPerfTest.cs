using BenchmarkDotNet.Attributes;
using UnicornHack.Data.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;

namespace UnicornHack.PerformanceTests.Systems;

[MemoryDiagnoser]
public class EffectSystemPerfTest
{
    private ActivePropertyEffect _bleedEffect = null!;
    private GameEntity _player = null!;
    private GameManager _manager = null!;
    private GameEntity _statusTarget = null!;

    [Params(16, 64)]
    public int EffectCount { get; set; }

    [GlobalSetup]
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
        _manager.EffectApplicationSystem.AddAbilityEffect(
            _player, AbilityData.Acrobatics);
        _player.ActiveEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, AbilityData.Acrobatics.Name, level: 1);
    }

    [Benchmark]
    public int Resolve_property()
    {
        _player.ActiveEffects!.TryRefresh(
            nameof(BeingComponent.Evasion), isBool: false, out _, out var value);
        return value;
    }

    [Benchmark]
    public void Process_status_heavy_expiry_scan()
        => _manager.EffectApplicationSystem.ExpireTimedPropertyEffects(_player, _manager);

    [Benchmark]
    public void Process_actor_turn()
    {
        _player.Player!.NextAction = ActorActionType.Wait;
        TestHelper.PerformTurn(_player, _manager);
    }

    [IterationSetup(Target = nameof(Apply_status))]
    public void Setup_status_target()
    {
        var room = TestHelper.BuildRoom(".");
        _statusTarget = PlayerRace.InstantiatePlayer("Status target", Sex.Male, room, new Point(0, 0));
        _statusTarget.Manager.Queue.ProcessQueue(_statusTarget.Manager);

        for (var i = 0; i < EffectCount; i++)
        {
            _statusTarget.ActiveEffects!.AddPropertyEffect(
                EffectType.ChangeProperty, nameof(BeingComponent.Evasion),
                ValueCombinationFunction.Sum,
                magnitudePrimary: i % 3,
                expirationTarget: 100000 + i,
                expirationEvent: EffectDuration.UntilTimeout);
        }
    }

    [Benchmark]
    public void Apply_status()
        => _statusTarget.Manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Slow, _statusTarget, amount: 1));

    [Benchmark]
    public int Query_effective_ability_level()
        => EffectApplicationSystem.GetAbilityLevel(_player, AbilityData.Acrobatics.Name);

    [IterationSetup(Target = nameof(Extend_movement_reactive_effects))]
    public void Reset_movement_reactive_effect()
        => _bleedEffect.ExpirationTarget = 100000;

    [Benchmark]
    public void Extend_movement_reactive_effects()
        => _manager.EffectApplicationSystem.ExtendEffectsOnMovement(_player, _manager);
}

using System.Reflection;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilityActivationSystemTest
{
    [Theory]
    [InlineData(Direction.East, 1, 3)]
    [InlineData(Direction.East, 2, 8)]
    [InlineData(Direction.East, 3, 15)]
    [InlineData(Direction.Northeast, 1, 3)]
    [InlineData(Direction.Northeast, 2, 8)]
    [InlineData(Direction.Northeast, 3, 15)]
    public void Cone_targeting_is_a_range_bounded_ninety_degree_area(Direction direction, int range, int expectedCells)
    {
        var room = TestHelper.BuildRoom("""
            .......
            .......
            .......
            .......
            .......
            .......
            .......
            """);
        var manager = room.Entity.Manager;
        var origin = new Point(3, 3);
        var forward = direction.AsVector();
        var cells = manager.AbilityActivationSystem.GetTargetedCells(
            origin, origin.Translate(forward), direction, range, TargetingShape.Cone, 1, room);

        Assert.Equal(expectedCells, cells.Count);
        Assert.All(cells, cell =>
        {
            Assert.InRange(origin.DistanceTo(cell.Item1), 1, range);
            var relative = origin.DifferenceTo(cell.Item1);
            var projection = relative.X * forward.X + relative.Y * forward.Y;
            var cross = relative.X * forward.Y - relative.Y * forward.X;
            Assert.True(projection > 0 && Math.Abs(cross) <= projection);
            Assert.True(cell.Item2 > 0);
        });
        ((List<(Point, byte)>)cells).Clear();
        manager.PointByteListArrayPool.Return((List<(Point, byte)>)cells);
    }

    [Fact]
    public void Cone_targeting_does_not_reveal_targets_behind_solid_cover()
    {
        var room = TestHelper.BuildRoom("""
            .......
            .......
            .......
            ....#..
            .......
            .......
            .......
            """);
        var manager = room.Entity.Manager;
        var cells = manager.AbilityActivationSystem.GetTargetedCells(
            new Point(3, 3), new Point(4, 3), Direction.East, 3, TargetingShape.Cone, 1, room);

        Assert.DoesNotContain(cells, cell => cell.Item1 == new Point(5, 3));
        Assert.DoesNotContain(cells, cell => cell.Item1 == new Point(6, 3));
        ((List<(Point, byte)>)cells).Clear();
        manager.PointByteListArrayPool.Return((List<(Point, byte)>)cells);
    }

    [Fact]
    public void Non_cumulative_leveled_effects_replace_the_granted_stage_ability()
    {
        var room = TestHelper.BuildRoom(".");
        var actor = CreatureData.Boggart.Instantiate(room, new Point(0, 0));
        var manager = actor.Manager;
        var template = new LeveledAbility
        {
            Name = "test leveled stages",
            Type = AbilityType.Buff,
            Cumulative = false,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [new AddAbility
                {
                    TargetActivator = true,
                    Ability = new Ability
                    {
                        Name = "first granted stage",
                        Type = AbilityType.RangedAttack,
                        Activation = ActivationType.Targeted,
                        Action = AbilityAction.Scream,
                        Range = 4,
                        Windup = 100,
                        Effects = [new Daze { Amount = "1" }]
                    }
                }],
                [2] = [new AddAbility
                {
                    TargetActivator = true,
                    Ability = new Ability
                    {
                        Name = "second granted stage",
                        Type = AbilityType.MeleeAttack,
                        Activation = ActivationType.Targeted,
                        Action = AbilityAction.Bite,
                        Range = 1,
                        Windup = 200,
                        Effects = [new PhysicalDamage { Amount = "5" }]
                    }
                }]
            }
        };
        manager.AbilityLevelSystem.AddAbilityEffect(actor, template);
        manager.Queue.ProcessQueue(manager);
        var first = manager.AffectableAbilitiesIndex[(actor.Id, "first granted stage")]!.Ability!;
        Assert.Equal(AbilityAction.Scream, first.Action);
        Assert.Equal(4, first.Range);
        Assert.Equal(100, first.Windup);

        manager.AbilityLevelSystem.AddAbilityEffect(actor, template);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(actor.Id, "first granted stage")]);
        var second = manager.AffectableAbilitiesIndex[(actor.Id, "second granted stage")]!.Ability!;
        Assert.Equal(AbilityAction.Bite, second.Action);
        Assert.Equal(1, second.Range);
        Assert.Equal(200, second.Windup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Teleport_attack_composes_movement_and_damage_without_mutating_the_preview(bool rooted)
    {
        var room = TestHelper.BuildRoom("""
            .....
            .....
            .....
            """);
        var actor = CreatureData.Boggart.Instantiate(room, new Point(0, 1));
        var target = CreatureData.Boggart.Instantiate(room, new Point(4, 1));
        var manager = actor.Manager;
        var ability = new Ability
        {
            Name = "test teleport attack",
            Type = AbilityType.MeleeAttack,
            Activation = ActivationType.Targeted,
            Trigger = ActivationType.OnMeleeAttack,
            Action = AbilityAction.Hit,
            Range = 4,
            SuccessCondition = AbilitySuccessCondition.Always,
            EnergyPointCost = 5,
            Delay = "100",
            Effects = [new Teleport { TargetActivator = true }, new PhysicalDamage { Amount = "5" }]
        }.AddToAffectable(actor);
        if (rooted)
        {
            manager.AbilityLevelSystem.AddAbilityEffect(actor, AbilityData.Bound, 2);
        }

        manager.Queue.ProcessQueue(manager);
        ability.Slot = 2;
        var initialEnergy = actor.Being!.EnergyPoints;
        var initialHealth = target.Being!.HitPoints;
        var activation = ActivateAbilityMessage.Create(ability.Entity, actor, target);
        var error = manager.AbilityActivationSystem.CanActivateAbility(activation);

        Assert.Equal(new Point(0, 1), actor.Position!.Cell);
        Assert.Equal(initialEnergy, actor.Being.EnergyPoints);
        Assert.Equal(initialHealth, target.Being.HitPoints);
        Assert.Null(error);
        manager.Enqueue(activation);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(new Point(3, 1), actor.Position.Cell);
        Assert.Equal(initialEnergy - 5, actor.Being.EnergyPoints);
        Assert.True(target.Being.HitPoints < initialHealth);
    }

    private static readonly FieldInfo ReferenceCountField = typeof(Entity).GetField(
        "_referenceCount", BindingFlags.Instance | BindingFlags.NonPublic)!;

    [Fact]
    public void Pretend_triggered_activations_return_their_cloned_messages()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        using var activatableReference = manager.CreateEntity();
        var activatable = activatableReference.Referenced;
        activatable.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

        using var abilityReference = manager.CreateEntity();
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "pretend message ownership";
        ability.Activation = ActivationType.WhileEquipped;
        ability.OwnerEntity = activatable;
        abilityReference.Referenced.Ability = ability;

        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.ChangeProperty;
        effect.Duration = EffectDuration.Infinite;
        effect.TargetName = nameof(BeingComponent.Armor);
        effect.Amount = "1";
        effect.ContainingAbilityId = abilityReference.Referenced.Id;
        effectReference.Referenced.Effect = effect;
        manager.Queue.ProcessQueue(manager);

        var activationTemplate = ActivateAbilityMessage.Create(manager);
        activationTemplate.ActivatorEntity = playerEntity;
        activationTemplate.TargetEntity = playerEntity;
        var playerReferences = GetReferenceCount(playerEntity);
        var abilityReferences = GetReferenceCount(abilityReference.Referenced);

        Assert.True(manager.AbilityActivationSystem.ActivateAbilities(
            activatable, ActivationType.WhileEquipped, activationTemplate, manager, pretend: true));
        Assert.Equal(playerReferences, GetReferenceCount(playerEntity));
        Assert.Equal(abilityReferences, GetReferenceCount(abilityReference.Referenced));

        ability.IsActive = true;
        Assert.False(manager.AbilityActivationSystem.ActivateAbilities(
            activatable, ActivationType.WhileEquipped, activationTemplate, manager, pretend: true));
        Assert.Equal(playerReferences, GetReferenceCount(playerEntity));
        Assert.Equal(abilityReferences, GetReferenceCount(abilityReference.Referenced));

        manager.ReturnMessage(activationTemplate);
    }

    private static int GetReferenceCount(GameEntity entity)
        => (int)ReferenceCountField.GetValue(entity)!;

    [Fact]
    public void Activating_an_ability_with_insufficient_energy_throws_the_log_message()
    {
        var (player, ability, manager) = CreateEnergyCostAbility();
        player.Being!.EnergyPoints = 0;
        var activation = ActivateAbilityMessage.Create(ability, player, player);
        manager.Enqueue(activation);

        var exception = Assert.Throws<InvalidOperationException>(
            () => manager.Queue.ProcessQueue(manager));

        Assert.Equal(
            manager.Game.Services.Language.NotEnoughEnergy(ability.Ability!),
            exception.Message);
        Assert.Equal(0, player.Being.EnergyPoints);
    }

    [Fact]
    public void Activating_an_ability_spends_energy_once()
    {
        var (player, ability, manager) = CreateEnergyCostAbility();
        player.Being!.EnergyPoints = 20;
        var activation = ActivateAbilityMessage.Create(ability, player, player);
        manager.Enqueue(activation);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(10, player.Being.EnergyPoints);
    }

    private static (GameEntity Player, GameEntity Ability, GameManager Manager) CreateEnergyCostAbility()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        TestHelper.AddSpellAbility(player);
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var ability = player.ActiveEffects!.Abilities.Single(
            candidate => candidate.Ability!.Action == AbilityAction.Spell);
        ability.Ability!.EnergyCost = 10;
        ability.Ability.Slot = 2;
        ability.Ability.SuccessCondition = AbilitySuccessCondition.Always;
        return (player, ability, manager);
    }

    [Fact]
    public void CanActivateAbility_returns_specific_validation_messages()
    {
        var (player, abilityEntity, manager) = CreateEnergyCostAbility();
        var ability = abilityEntity.Ability!;
        var language = manager.Game.Services.Language;
        ability.EnergyCost = 0;

        ability.IsUsable = false;
        Assert.Equal(language.AbilityNotUsable(ability), GetError());

        ability.IsUsable = true;
        ability.Slot = null;
        Assert.Equal(language.AbilityNotSlotted(ability), GetError());

        ability.Slot = 2;
        ability.IsActive = true;
        Assert.Equal(language.AbilityAlreadyActive(ability), GetError());

        ability.IsActive = false;
        ability.CooldownTick = manager.Game.CurrentTick + 100;
        Assert.Equal(language.AbilityOnCooldown(ability), GetError());

        ability.CooldownTick = null;
        ability.Delay = "-1";
        Assert.Equal(language.TooSlowToUseAbility(ability), GetError());

        ability.Delay = "100";
        Assert.Equal(language.AbilityRequiresTarget(ability), GetError(hasTarget: false));

        using var emptyAbilityReference = manager.CreateEntity();
        var emptyAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        emptyAbility.Name = "empty test ability";
        emptyAbility.Activation = ActivationType.Manual;
        emptyAbility.Slot = 3;
        emptyAbility.OwnerEntity = player;
        emptyAbilityReference.Referenced.Ability = emptyAbility;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            language.AbilityHasNoEffects(emptyAbility),
            GetError(emptyAbilityReference.Referenced));

        string? GetError(GameEntity? testedAbility = null, bool hasTarget = true)
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = testedAbility ?? abilityEntity;
            message.ActivatorEntity = player;
            message.TargetEntity = hasTarget ? player : null;
            message.TargetCell = hasTarget ? player.Position!.Cell : null;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }
    }

    [Fact]
    public void CanActivateAbility_returns_specific_capability_messages_without_a_status_source()
    {
        var (player, abilityEntity, manager) = CreateEnergyCostAbility();
        var ability = abilityEntity.Ability!;
        var language = manager.Game.Services.Language;
        ability.EnergyCost = 0;

        AssertBlocked(
            nameof(BeingComponent.CanCastSpells),
            () => language.CannotCastAbility(ability));

        ability.Type = AbilityType.Default;
        ability.Action = AbilityAction.Scream;
        AssertBlocked(
            nameof(BeingComponent.CanUseVerbalAbilities),
            () => language.CannotUseVerbalAbility(ability));

        ability.Type = AbilityType.Miracle;
        ability.Action = AbilityAction.Default;
        AssertBlocked(
            nameof(BeingComponent.CanUseDivineAbilities),
            () => language.CannotUseDivineAbility(ability));

        ability.Type = AbilityType.DefaultMeleeAttack;
        ability.Action = AbilityAction.Bite;
        AssertBlocked(
            nameof(BeingComponent.CanUseDefaultAttacks),
            () => language.CannotUseDefaultAttack(ability));

        ability.Type = AbilityType.Default;
        AssertBlocked(
            nameof(BeingComponent.CanUseNonDefaultAbilities),
            () => language.CannotUseNonDefaultAbility(ability));

        ability.Action = AbilityAction.Breath;
        AssertBlocked(
            nameof(BeingComponent.CanUseComplexAbilities),
            () => language.CannotUseComplexAbility(ability));

        ability.Action = AbilityAction.Drink;
        AssertBlocked(
            nameof(BeingComponent.CanUseNonAttackAbilities),
            () => language.CannotUseNonAttackAbility(ability));

        void AssertBlocked(string property, Func<string> getExpectedError)
        {
            manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
                0, property, player, manager, ValueCombinationFunction.Override);

            var message = ActivateAbilityMessage.Create(abilityEntity, player, player);
            message.TargetCell = player.Position!.Cell;
            Assert.Equal(
                getExpectedError(),
                manager.AbilityActivationSystem.CanActivateAbility(message));
            manager.ReturnMessage(message);

            manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
                1, property, player, manager, ValueCombinationFunction.Override);
        }
    }

    [Fact]
    public void Rejected_sustained_activation_does_not_reserve_energy()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        using var abilityReference = manager.CreateEntity();
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "invalid sustained target";
        ability.Activation = ActivationType.WhileToggled | ActivationType.Targeted;
        ability.EnergyCost = 10;
        ability.Delay = "100";
        ability.Slot = 2;
        ability.OwnerEntity = playerEntity;
        abilityReference.Referenced.Ability = ability;

        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.ChangeProperty;
        effect.Duration = EffectDuration.Infinite;
        effect.TargetName = nameof(BeingComponent.Armor);
        effect.Amount = "1";
        effect.ContainingAbilityId = abilityReference.Referenced.Id;
        effectReference.Referenced.Effect = effect;
        manager.Queue.ProcessQueue(manager);

        var energyBefore = playerEntity.Being!.EnergyPoints;
        var activation = ActivateAbilityMessage.Create(manager);
        activation.AbilityEntity = abilityReference.Referenced;
        activation.ActivatorEntity = playerEntity;
        manager.Enqueue(activation);

        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));
        Assert.Equal(energyBefore, playerEntity.Being.EnergyPoints);
        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);
    }

    private static readonly string TestMap = @"
.#.
.#.
...";

    [Fact]
    public void Exposed_progressively_reduces_evasion()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        playerEntity.Position!.Heading = Direction.West;
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, AbilityData.Conjuration);
        manager.Queue.ProcessQueue(manager);

        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var iceShard = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;
        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShard.Ability!, playerEntity);

        var missChanceBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            iceShard, playerEntity, target, null, accuracy);
        Assert.True(missChanceBefore > 0);
        var baseEvasion = target.Being!.Evasion;
        var baseStatusResistance = target.Being!.PhysicalStatusResistance;

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Expose, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(target, AbilityData.Exposed.Name));
        Assert.Equal(baseStatusResistance - 25, target.Being.PhysicalStatusResistance);
        Assert.Equal(
            missChanceBefore,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Expose, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(target, AbilityData.Exposed.Name));
        Assert.Equal(baseEvasion / 2, target.Being.Evasion);
        Assert.Equal(baseEvasion / 2, AbilityActivationSystem.GetEvasion(target));
        Assert.Contains(
            target.ActiveEffects!.PropertyEffects,
            effect => effect.TargetName == nameof(BeingComponent.Evasion)
                      && effect.CombinationFunction == ValueCombinationFunction.Percent
                      && effect.SourceAbilityType == AbilityType.Debuff
                      && effect.SourceAbilityActivation == ActivationType.Always);
        var missChanceAtLevelTwo = manager.AbilityActivationSystem.GetEvasionProbability(
            iceShard, playerEntity, target, null, accuracy);
        Assert.InRange(missChanceAtLevelTwo, 1, missChanceBefore);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Expose, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, AbilityLevelSystem.GetAbilityLevel(target, AbilityData.Exposed.Name));
        Assert.Equal(0, target.Being.Evasion);
        Assert.Equal(0, AbilityActivationSystem.GetEvasion(target));

        Assert.Equal(
            0,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));
    }

    [Fact]
    public void Only_stunned_level_three_removes_evasion()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, AbilityData.Conjuration);
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var iceShard = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;
        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShard.Ability!, playerEntity);
        var baseline = manager.AbilityActivationSystem.GetEvasionProbability(
            iceShard, playerEntity, target, null, accuracy);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Stun, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            baseline,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Stun, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            baseline,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Stun, target, amount: 134));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            0,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));
    }

    [Fact]
    public void Only_sleepy_level_two_removes_evasion()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, AbilityData.Conjuration);
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var iceShard = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;
        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShard.Ability!, playerEntity);
        var baseline = manager.AbilityActivationSystem.GetEvasionProbability(
            iceShard, playerEntity, target, null, accuracy);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Sedate, target, amount: 200));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            baseline,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Sedate, target, amount: 200));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            0,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));
    }

    [Fact]
    public void Only_paralyzed_level_two_removes_evasion()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, AbilityData.Conjuration);
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var iceShard = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;
        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShard.Ability!, playerEntity);
        var baseline = manager.AbilityActivationSystem.GetEvasionProbability(
            iceShard, playerEntity, target, null, accuracy);

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Paralyze, target, amount: 200));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            baseline,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(EffectType.Paralyze, target, amount: 200));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            0,
            manager.AbilityActivationSystem.GetEvasionProbability(
                iceShard, playerEntity, target, null, accuracy));
    }

    [Theory]
    [InlineData(100)]
    [InlineData(200)]
    public void Unstable_target_is_easier_to_hit_in_melee_and_at_range(int amount)
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Rakshasa.Instantiate(level, new Point(0, 0));
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var melee = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Name == "close-quarters strike");
        var ranged = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Shoot);
        var meleeAccuracy = manager.AbilityActivationSystem.GetAccuracy(melee.Ability!, attacker);
        var rangedAccuracy = manager.AbilityActivationSystem.GetAccuracy(ranged.Ability!, attacker);

        var meleeMissBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            melee, attacker, target, BeveledVisibilityCalculator.MaxVisibility, meleeAccuracy);
        var rangedMissBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            ranged, attacker, target, BeveledVisibilityCalculator.MaxVisibility, rangedAccuracy);
        Assert.True(meleeMissBefore > 0);

        ApplyUnstable(target, amount, manager);

        var meleeMissAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            melee, attacker, target, BeveledVisibilityCalculator.MaxVisibility, meleeAccuracy);
        var rangedMissAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            ranged, attacker, target, BeveledVisibilityCalculator.MaxVisibility, rangedAccuracy);

        // Directly reduced evasion makes the target easier to hit through either attack type.
        Assert.True(meleeMissAfter < meleeMissBefore);
        Assert.True(rangedMissAfter < rangedMissBefore);
    }

    [Fact]
    public void Unstable_level_two_attacker_strikes_less_accurately_in_melee_and_at_range()
    {
        var level = TestHelper.BuildRoom(TestMap);
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Rakshasa.Instantiate(level, new Point(0, 0));
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var melee = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Name == "close-quarters strike");
        var ranged = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Shoot);
        var meleeAccuracy = manager.AbilityActivationSystem.GetAccuracy(melee.Ability!, attacker);
        var rangedAccuracy = manager.AbilityActivationSystem.GetAccuracy(ranged.Ability!, attacker);

        var meleeMissBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            melee, attacker, target, BeveledVisibilityCalculator.MaxVisibility, meleeAccuracy);
        var rangedMissBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            ranged, attacker, target, BeveledVisibilityCalculator.MaxVisibility, rangedAccuracy);
        Assert.True(meleeMissBefore < 100);

        ApplyUnstable(attacker, amount: 200, manager);

        meleeAccuracy = manager.AbilityActivationSystem.GetAccuracy(melee.Ability!, attacker);
        rangedAccuracy = manager.AbilityActivationSystem.GetAccuracy(ranged.Ability!, attacker);

        var meleeMissAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            melee, attacker, target, BeveledVisibilityCalculator.MaxVisibility, meleeAccuracy);
        var rangedMissAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            ranged, attacker, target, BeveledVisibilityCalculator.MaxVisibility, rangedAccuracy);

        // Level 2 directly halves accuracy for either attack type.
        Assert.True(meleeMissAfter > meleeMissBefore);
        Assert.True(rangedMissAfter > rangedMissBefore);
    }

    private static void ApplyUnstable(GameEntity entity, int amount, GameManager manager)
    {
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Topple, entity, amount * 2));

        manager.Queue.ProcessQueue(manager);
    }

    [Fact]
    public void Frightened_attacker_strikes_the_source_of_its_fear_less_accurately()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var ghost = CreatureData.Psukhe.Instantiate(level, new Point(1, 0));
        var other = CreatureData.Manusya.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        var bite = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Bite);
        var biteAccuracy = manager.AbilityActivationSystem.GetAccuracy(bite.Ability!, attacker);
        var ghostGaze = ghost.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Gaze);

        var missVsGhostBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, ghost, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);
        var missVsOtherBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, other, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);

        // Frighten the attacker of the ghost.
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, attacker, amount: 200, ghostGaze.Id));

        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(attacker, AbilityData.Frightened.Name));

        var missVsGhostAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, ghost, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);
        var missVsOtherAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, other, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);

        // The frightened attacker misses the source of its fear more, but attacks on others are unaffected.
        Assert.True(missVsGhostAfter > missVsGhostBefore);
        Assert.Equal(missVsOtherBefore, missVsOtherAfter);
    }

    [Fact]
    public void Taunted_level_two_redirects_attacks_to_the_source()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var taunter = CreatureData.Bugbear.Instantiate(level, new Point(1, 0));
        var other = CreatureData.Manusya.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        var bite = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Bite);
        bite.Ability!.Slot = 0;
        GameEntity? activationTarget = null;
        var listener = new AbilityActivatedListener
        {
            ProcessMessage = activatedMessage => activationTarget = activatedMessage.TargetEntity
        };
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);
        var taunt = taunter.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Scream);
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Taunt, attacker, amount: 300, taunt.Id));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(attacker, AbilityData.Taunted.Name));

        var message = ActivateAbilityMessage.Create(manager);
        message.AbilityEntity = bite;
        message.ActivatorEntity = attacker;
        message.TargetEntity = other;
        message.TargetCell = other.Position!.Cell;

        manager.Process(message);
        manager.Queue.ProcessQueue(manager);

        Assert.Same(taunter, activationTarget);
    }

    [Fact]
    public void Taunted_level_three_blocks_non_attack_abilities()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var taunter = CreatureData.Bugbear.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var bite = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Bite);
        bite.Ability!.Slot = 0;
        bite.Ability.SuccessCondition = AbilitySuccessCondition.Always;
        bite.Ability.Type = AbilityType.Default;
        var taunt = taunter.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Scream);
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Taunt, attacker, amount: 400, taunt.Id));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, AbilityLevelSystem.GetAbilityLevel(attacker, AbilityData.Taunted.Name));

        var message = ActivateAbilityMessage.Create(manager);
        message.AbilityEntity = bite;
        message.ActivatorEntity = attacker;
        message.TargetEntity = taunter;
        message.TargetCell = taunter.Position!.Cell;

        var error = manager.AbilityActivationSystem.CanActivateAbility(message);

        var taunted = manager.AffectableAbilitiesIndex[
            (attacker.Id, AbilityData.Taunted.Name)]!.Ability!;
        Assert.Equal(
            manager.Game.Services.Language.StatusPreventsAbility(taunted, bite.Ability!),
            error);
        manager.ReturnMessage(message);
    }

    [Fact]
    public void Confused_level_one_does_not_remove_the_targets_evasion_bonus()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var target = CreatureData.Geryon.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        // The target is nimble enough to have an agility bonus above the base evasion.
        Assert.True(target.Being!.Evasion > 50);

        var bite = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Bite);
        var biteAccuracy = manager.AbilityActivationSystem.GetAccuracy(bite.Ability!, attacker);

        var missBefore = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, target, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Confuse, target, amount: 100));

        manager.Queue.ProcessQueue(manager);

        var missAfter = manager.AbilityActivationSystem.GetEvasionProbability(
            bite, attacker, target, BeveledVisibilityCalculator.MaxVisibility, biteAccuracy);

        Assert.Equal(missBefore, missAfter);
    }

    [Fact]
    public void Fascinated_level_one_applies_and_reverts_mental_status_resistance()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.Manusya.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var baseResistance = npc.Being!.MentalStatusResistance;

        EffectTestHelper.ApplyStatus(npc, EffectType.Fascinate, duration: 100, manager);

        Assert.True((AbilityLevelSystem.GetAbilityLevel(npc, "fascinated") > 0));
        Assert.Equal(baseResistance - 25, npc.Being.MentalStatusResistance);

        manager.AbilityLevelSystem.RemoveAbility(npc, "fascinated", manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False((AbilityLevelSystem.GetAbilityLevel(npc, "fascinated") > 0));
        Assert.Equal(baseResistance, npc.Being.MentalStatusResistance);
    }

    [Fact]
    public void Can_use_projectile_abilities_on_targets_in_LOS()
    {
        var level = TestHelper.BuildRoom(TestMap);

        var manager = level.Entity.Manager;
        var listener = new AbilityActivatedListener();
        manager.Queue.Register(listener, AbilityActivatedMessage.Name, -1);

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 0));
        playerEntity.Position!.Heading = Direction.West;
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, AbilityData.Conjuration);

        manager.Queue.ProcessQueue(manager);

        ItemData.Shortbow.Instantiate(playerEntity);
        var undine = CreatureData.Undine.Instantiate(level, new Point(2, 2));
        var sylph = CreatureData.Sylph.Instantiate(level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, playerEntity.Being!.Accuracy);
        Assert.Equal(100, playerEntity.Being.Evasion);
        Assert.Equal(50, sylph.Being!.Accuracy);
        Assert.Equal(75, sylph.Being.Evasion);

        var bow = playerEntity.Being.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = bow;
        equipMessage.Slot = EquipmentSlot.GraspBothRanged;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var nymphAbility = undine.ActiveEffects!.Abilities
            .First(a => (a.Ability!.Activation & ActivationType.Slottable) != 0
                        && a.Ability.Template?.Type.IsDefaultAttack() != true);
        Assert.Equal(2, nymphAbility.Ability!.Slot);

        var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.TwoHandedRangedAttack.Name)]!;

        var activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position!.Cell;
        activateAbilityMessage.AbilityEntity = attackAbility;

        manager.Enqueue(activateAbilityMessage);

        var messageCount = 0;
        listener.ProcessMessage = m =>
        {
            if (m.TargetEntity == null)
            {
                return;
            }

            messageCount++;
            Assert.Same(playerEntity, m.ActivatorEntity);
            Assert.Same(undine, m.TargetEntity);
            Assert.Equal(new Point(2, 2), m.TargetCell);
        };

        Assert.Equal(5, playerEntity.Player!.NextActionTick);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, messageCount);
        Assert.Equal(155, playerEntity.Player!.NextActionTick);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = sylph.Position!.Cell;
        activateAbilityMessage.AbilityEntity = attackAbility;

        manager.Enqueue(activateAbilityMessage);

        messageCount = 0;
        listener.ProcessMessage = m =>
        {
            if (m.TargetEntity == null)
            {
                return;
            }

            messageCount++;
        };

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, messageCount);

        undine.Being!.HitPoints = undine.Being.HitPointMaximum;
        var iceShardAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!;

        var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShardAbility.Ability!, playerEntity);
        Assert.Equal(105, accuracy);

        Assert.Equal(21,
            manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, undine, null,
                accuracy));

        Assert.Equal(100,
            manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, sylph, null,
                accuracy));

        var setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = iceShardAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.Cell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability!.CooldownTick);

        activateAbilityMessage = ActivateAbilityMessage.Create(manager);
        activateAbilityMessage.ActivatorEntity = playerEntity;
        activateAbilityMessage.TargetCell = undine.Position.Cell;
        activateAbilityMessage.AbilityEntity = iceShardAbility;
        manager.Enqueue(activateAbilityMessage);

        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

        Assert.Equal(1, messageCount);
        Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);
    }

    [Fact]
    public void Can_use_toggleable_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, manager.Game.Services.Data.PlayerRaces.Get("human").GetRaceAbilityTemplate(), level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, playerEntity.Being!.ReservedEnergyPoints);
        var toggledAbility = playerEntity.ActiveEffects!.Abilities
            .Single(ability => ability.Ability!.Name == "Surge of Luck");
        var setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        setSlotMessage.Slot = 2;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(toggledAbility.Ability!.IsActive);
        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Slow, duration: 100, manager);

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = toggledAbility;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Equal(400, toggledAbility.Ability.CooldownTick);
        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);

        manager.AbilityLevelSystem.RemoveAbility(
            playerEntity, AbilityData.Slowed.Name, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(200, toggledAbility.Ability.CooldownTick);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(200, manager.Game.CurrentTick);

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.Slot);

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.Null(toggledAbility.Ability.CooldownTick);
        Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);
    }

    [Fact]
    public void Targeting_beam_pattern1_diagonal() => TestTargeting(@"
<#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#2..#",
        new byte[]
        {
            0, 254,   0,   0,   0,   0,   0,   0,   0,   0,
          254,  76, 254,   0,   0,   0,   0,   0,   0,   0,
            0, 254, 140, 254,   0,   0,   0,   0,   0,   0,
            0,   0,  22, 190, 254,   0,   0,   0,   0,   0,
            0,   0,   0,  43, 183,  45,   0,   0,   0,   0,
            0,   0,   0,   0, 254, 181,  68,   0,   0,   0,
            0,   0,   0,   0,   0, 254, 189,   0,   0,   0
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.West);

    [Fact]
    public void Targeting_line_pattern1_diagonal() => TestTargeting(@"
<#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#2..#",
        new byte[]
        {
            0, 254,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0, 254,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0, 254,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0, 254,   0,   0,   0,   0,   0,
            0,   0,   0,   0, 183,   0,   0,   0,   0,   0,
            0,   0,   0,   0, 254,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0, 254,   0,   0,   0,   0
        },
        6,
        TargetingShape.Line,
        1,
        Direction.West);

    [Fact]
    public void Targeting_beam_pattern1_diagonal_near() => TestTargeting(@"
<#.......#
#2#......#
.#.#.....#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 254,   0,   0,   0,   0,   0,   0,   0,   0,
          254, 190, 254,   0,   0,   0,   0,   0,   0,   0,
            0, 254, 158, 254,   0,   0,   0,   0,   0,   0,
            0,   0,  66, 198, 254,   0,   0,   0,   0,   0,
            0,   0,   0, 127, 198,  45,   0,   0,   0,   0,
            0,   0,   0,   2, 254, 181,  68,   0,   0,   0,
            0,   0,   0,   0,   0, 254, 189,   0,   0,   0
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.West);

    [Fact]
    public void Targeting_beam_pattern1_diagonal_self() => TestTargeting(@"
<#.......#
#.#......#
.#.#.....#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0, 254,   0,   0,   0,   0,   0,   0,   0,   0,
          254, 190, 254,   0,   0,   0,   0,   0,   0,   0,
            0, 254, 158, 254,   0,   0,   0,   0,   0,   0,
            0,   0,  66, 198, 254,   0,   0,   0,   0,   0,
            0,   0,   0, 127, 198,  45,   0,   0,   0,   0,
            0,   0,   0,   2, 254, 181,  68,   0,   0,   0,
            0,   0,   0,   0,   0, 254, 189,   0,   0,   0
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.Southeast,
        targetGlyph: '<');

    [Fact]
    public void Targeting_beam_pattern2_diagonal_near() => TestTargeting(@"
<........#
.2.......#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0,  42,   0,   0,   0,   0,   0,   0,   0,   0,
           42, 254, 211, 127,  42,   0,   0,   0,   0,   0,
            0, 211, 254, 187, 254, 211, 126,   0,   0,   0,
            0, 127, 187,   4, 254, 158, 253,   0,   0,   0,
            0,  42, 254, 127,   0,   0,  64,   0,   0,   0,
            0,   0, 211, 251, 254,   0,   0,   0,   0,   0,
            0,   0, 126, 254, 177, 254,   0,   0,   0,   0
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.West);

    [Fact]
    public void Targeting_line_pattern2_diagonal_near() => TestTargeting(@"
<........#
.2.......#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0, 254,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0, 254, 187, 254, 211,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0, 253,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0
        },
        6,
        TargetingShape.Line,
        1,
        Direction.West);


    [Fact]
    public void Targeting_beam_pattern2_vertical_self() => TestTargeting(@"
...<.....#
.........#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0, 127, 254, 127,   0,   0,   0,   0,   0,
            0,  79, 254, 254, 254, 127,   0,   0,   0,   0,
          124,  66,  63, 254, 254, 222, 127,   0,   0,   0,
          127,   0, 127, 254,  42, 127, 254, 127,   0,   0,
            2,   0, 190, 254, 254,  15, 238, 254, 127,   0,
            0,   7, 246, 254,  25,   0, 127, 254, 254, 254
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.South,
        targetGlyph: '<');

    [Fact]
    public void Targeting_beam_pattern2_vertical_near() => TestTargeting(@"
..<......#
..2......#
..#......#
....#....#
.........#
....#....#
.....#...#",
        new byte[]
        {
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0, 127, 254, 127,   0,   0,   0,   0,   0,   0,
          127, 238, 254, 238, 127,   0,   0,   0,   0,   0,
          254, 190,   0, 190, 254,  74,   0,   0,   0,   0,
          254, 127,   0, 127, 127,  51, 117,   0,   0,   0,
          254,  63,   0,  63, 254,   0,  85, 125,   0,   0,
          246,   7,   0,   7,  68,   0,   0, 127, 127,   0
        },
        6,
        TargetingShape.Beam,
        1,
        Direction.North);

    private static void TestTargeting(
        string map,
        byte[] expectedExposure,
        int range,
        TargetingShape targetingShape,
        int targetShapeSize,
        Direction heading,
        char targetGlyph = '2')
    {
        var (level, _) = TestHelper.BuildRoomWithChunks(map);
        var origin = level.Connections.Single().Key;
        var target = targetGlyph == '<' ? origin : FindTargetPoint(map, targetGlyph, origin);

        var manager = level.Entity.Manager;
        var abilityActivationSystem = manager.AbilityActivationSystem;
        var targetedCells = abilityActivationSystem.GetTargetedCells(
            origin, target, heading, range, targetingShape, targetShapeSize, level);

        var visibleTerrain = new byte[level.TileCount];
        var originDistance = 0;
        var previousTarget = origin;
        foreach (var (point, exposure) in targetedCells)
        {
            visibleTerrain[level.PointToIndex![point.X, point.Y]] += exposure;
            var distance = origin.DistanceTo(point);
            Assert.False(distance < originDistance, $"Point {point} is out of sequence");
            if (targetingShape == TargetingShape.Line)
            {
                Assert.Equal(1, previousTarget.DistanceTo(point));
                previousTarget = point;
            }

            originDistance = distance;
        }

        ((List<(Point, byte)>)targetedCells).Clear();
        manager.PointByteListArrayPool.Return((List<(Point, byte)>)targetedCells);
        TestAssert.AssertVisibility(level, expectedExposure, visibleTerrain);
    }

    /// <summary>
    ///     Locates <paramref name="glyph" /> in the raw test map and converts its
    ///     position to a world-space <see cref="Point" />. The fragment may be placed
    ///     at a non-zero offset within the level, so the map→world translation is
    ///     derived from the '&lt;' anchor in the map literal compared to the supplied
    ///     <paramref name="origin" /> (the connection's world position).
    /// </summary>
    private static Point FindTargetPoint(string map, char glyph, Point origin)
    {
        var lines = map.Split('\n');
        Point? originMap = null;
        Point? targetMap = null;
        for (var row = 0; row < lines.Length && (originMap is null || targetMap is null); row++)
        {
            var line = lines[row];
            for (var col = 0; col < line.Length; col++)
            {
                var c = line[col];
                if (originMap is null && c == '<')
                {
                    originMap = new Point((byte)col, (byte)row);
                }
                else if (targetMap is null && c == glyph)
                {
                    targetMap = new Point((byte)col, (byte)row);
                }
            }
        }

        if (targetMap is null)
        {
            throw new ArgumentException($"Glyph '{glyph}' not found in map.", nameof(glyph));
        }

        if (originMap is null)
        {
            throw new ArgumentException("Origin glyph '<' not found in map.", nameof(map));
        }

        return new Point(
            (byte)(origin.X + targetMap.Value.X - originMap.Value.X),
            (byte)(origin.Y + targetMap.Value.Y - originMap.Value.Y));
    }

    private class AbilityActivatedListener : IGameSystem<AbilityActivatedMessage>
    {
        public Action<AbilityActivatedMessage>? ProcessMessage
        {
            get;
            set;
        }

        public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager state)
        {
            ProcessMessage?.Invoke(message);

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}

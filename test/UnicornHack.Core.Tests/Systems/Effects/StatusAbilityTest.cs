using System.Globalization;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using static UnicornHack.Systems.Effects.EffectTestHelper;

namespace UnicornHack.Systems.Effects;

public class StatusAbilityTest
{
    [Fact]
    public void Slow_reduces_speed_through_a_linked_sibling_and_reverts_when_it_ends()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseSpeed - 2, npc.Being.Speed);

        // The status wears off, taking its linked property change with it.
        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseSpeed, npc.Being.Speed);
    }

    [Fact]
    public void Slow_buildup_promotes_cumulative_levels()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;
        var baseCooldownFactor = npc.Being.CooldownFactor;

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager, amount: 300);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Slowed.Name));
        Assert.Equal(baseSpeed - 6, npc.Being.Speed);
        Assert.Equal(baseCooldownFactor + 100, npc.Being.CooldownFactor);
        Assert.Equal(0, npc.ActiveEffects!.Accumulators[EffectType.Slow].Value);
    }

    [Fact]
    public void Slowed_rescales_running_tick_and_xp_cooldowns()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var ability = npc.ActiveEffects!.Abilities.First().Ability!;
        var currentTick = manager.Game.CurrentTick;
        ability.CooldownTick = currentTick + 51;
        ability.CooldownXpLeft = 51;

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager);

        Assert.Equal(200, npc.Being!.CooldownFactor);
        Assert.Equal(currentTick + 102, ability.CooldownTick);
        Assert.Equal(102, ability.CooldownXpLeft);

        manager.EffectApplicationSystem.RemoveAbility(
            npc, AbilityData.Slowed.Name, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, npc.Being.CooldownFactor);
        Assert.Equal(currentTick + 51, ability.CooldownTick);
        Assert.Equal(51, ability.CooldownXpLeft);
    }

    [Fact]
    public void Reapplying_a_leveled_status_adds_a_level_record()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseSpeed - 2, npc.Being.Speed);

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Slowed.Name));
        var records = npc.ActiveEffects!.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Slowed.Name)
            .ToList();
        Assert.Equal(2, records.Count);
        Assert.All(records, record => Assert.Equal(1, record.Level));
        Assert.Equal(
            2,
            manager.AffectableAbilitiesIndex[(npc.Id, AbilityData.Slowed.Name)]!.Ability!.Level);
        Assert.Equal(baseSpeed - 4, npc.Being.Speed);
        var slowAccumulator = npc.ActiveEffects!.Accumulators[EffectType.Slow];
        Assert.InRange(slowAccumulator.Value, 0, EffectApplicationSystem.BuildUpThreshold - 1);
    }

    [Fact]
    public void A_timed_property_modifier_applies_immediately_and_expires_on_its_tick()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var baseResistance = being.FireResistance;
        var startTick = manager.Game.CurrentTick;

        // A lingering +25 fire-resistance buff lasting 50 ticks, stored as an ActivePropertyEffect that
        // carries its own expiry instead of a companion effect entity.
        var expirationTick = startTick + 50;
        playerEntity.ActiveEffects!.AddPropertyEffect(CreatePropertyEffect(playerEntity.ActiveEffects!,
            0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.FireResistance),
            combinationFunction: ValueCombinationFunction.Sum,
            magnitudePrimary: 25,
            expirationTarget: expirationTick,
            expirationEvent: EffectDuration.UntilTimeout));
        playerEntity.ActiveEffects!.NextEffectTick = expirationTick;
        playerEntity.ActiveEffects.TryRefresh(
            nameof(BeingComponent.FireResistance), isBool: false, out _, out _);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseResistance + 25, being.FireResistance);
        Assert.Equal(startTick + 50, playerEntity.ActiveEffects!.NextEffectTick);

        // Passing a turn advances time past the 50-tick expiry, so the buff lifts itself through the
        // temporal expiry index with no leftover state.
        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);
        TestHelper.AdvanceToNextPlayerTurn(manager);

        Assert.Equal(baseResistance, being.FireResistance);
        Assert.Null(playerEntity.ActiveEffects.NextEffectTick);
        Assert.DoesNotContain(
            playerEntity.ActiveEffects.PropertyEffects,
            e => e.TargetName == nameof(BeingComponent.FireResistance));
    }

    [Fact]
    public void A_timed_ChangeProperty_effect_is_carried_by_a_self_expiring_modifier_instance()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var baseMight = being.Might;
        var startTick = manager.Game.CurrentTick;

        // A ChangeProperty(-2 Might) effect that lasts 50 ticks, carried by a manual ability.
        GameEntity effectEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            var abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = playerEntity;
            abilityEntity.Ability = ability;

            using var effectReference = manager.CreateEntity();
            effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.TargetName = nameof(BeingComponent.Might);
            effect.Amount = "-2";
            effect.Duration = EffectDuration.UntilTimeout;
            effect.DurationAmount = "50";
            effect.ContainingAbilityId = ability.EntityId;
            effectEntity.Effect = effect;
        }

        TestHelper.ApplyEffect(effectEntity, playerEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        // The debuff took hold as a self-expiring ActivePropertyEffect - no applied ChangeProperty entity
        // was cloned onto the target for it.
        Assert.Equal(baseMight - 2, being.Might);
        Assert.Contains(
            playerEntity.ActiveEffects!.PropertyEffects,
            i => i.TargetName == nameof(BeingComponent.Might) && i.ExpirationTarget == startTick + 50);
        Assert.Equal(startTick + 50, playerEntity.ActiveEffects.NextEffectTick);

        // Passing a turn advances time past the 50-tick expiry, lifting the debuff on its own.
        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);
        TestHelper.AdvanceToNextPlayerTurn(manager);

        Assert.Equal(baseMight, being.Might);
        Assert.Null(playerEntity.ActiveEffects.NextEffectTick);
    }

    [Fact]
    public void A_lingering_ChangeProperty_effect_contributes_through_a_source_linked_instance()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var baseArmor = being.Armor;

        // An Infinite (source-linked) ChangeProperty(+5 Armor) carried by a manual ability.
        GameEntity effectEntity;
        int abilityId;
        using (var abilityReference = manager.CreateEntity())
        {
            var abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = playerEntity;
            abilityEntity.Ability = ability;
            abilityId = ability.EntityId;

            using var effectReference = manager.CreateEntity();
            effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.TargetName = nameof(BeingComponent.Armor);
            effect.Amount = "5";
            effect.Duration = EffectDuration.Infinite;
            effect.ContainingAbilityId = ability.EntityId;
            effectEntity.Effect = effect;
        }

        TestHelper.ApplyEffect(effectEntity, playerEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        // The bonus took hold through a single ActivePropertyEffect keyed to its source ability and
        // property - there is no companion applied effect entity (retirement 2a).
        Assert.Equal(baseArmor + 5, being.Armor);
        Assert.Contains(
            playerEntity.ActiveEffects!.PropertyEffects,
            i => i.SourceAbilityEntityId == abilityId
                 && i.TargetName == nameof(BeingComponent.Armor)
                 && i.MagnitudePrimary == 5);

        // Dropping the source ability's modifiers reverts the bonus.
        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            playerEntity.ActiveEffects, playerEntity,
            instance => instance.SourceAbilityEntityId == abilityId, manager);

        Assert.Equal(baseArmor, being.Armor);
        Assert.DoesNotContain(
            playerEntity.ActiveEffects.PropertyEffects,
            i => i.SourceAbilityEntityId == abilityId
                 && i.TargetName == nameof(BeingComponent.Armor));
    }

    [Fact]
    public void Burning_suppresses_wetted_until_it_ends()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseVisibility = npc.Being!.Visibility;
        var baseFireResistance = npc.Being.FireResistance;

        ApplyDamageOverTimeEffect(npc, EffectType.FireDamage, ratePerAction: 100, duration: 1, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Burning.Name));
        Assert.Equal(baseVisibility + 50, npc.Being.Visibility);
        Assert.Equal(baseFireResistance, npc.Being.FireResistance);

        // Burning suppresses Wetted while the fire persists.
        ApplyStatus(npc, EffectType.Wet, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseVisibility + 50, npc.Being.Visibility);
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Wet));
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Wetted.Name));

        var burningEffect = Assert.Single(
            npc.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.FireDamage);
        manager.Game.CurrentTick = Assert.IsType<int>(burningEffect.ExpirationTarget);
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseVisibility, npc.Being.Visibility);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Wetted.Name));
    }

    [Fact]
    public void Burning_reconciles_to_the_level_supported_by_sources_remaining_at_expiry()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseVisibility = npc.Being!.Visibility;
        var baseFireResistance = npc.Being.FireResistance;

        ApplyDamageOverTimeEffect(npc, EffectType.FireDamage, ratePerAction: 300, duration: 100, manager);
        ApplyDamageOverTimeEffect(npc, EffectType.FireDamage, ratePerAction: 100, duration: 200, manager);
        manager.Queue.ProcessQueue(manager);

        var sources = npc.ActiveEffects!.PropertyEffects
            .Where(effect => effect.EffectType == EffectType.FireDamage)
            .OrderBy(effect => effect.ExpirationTarget)
            .ToList();
        Assert.Equal(2, sources.Count);
        var records = npc.ActiveEffects.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Burning.Name)
            .OrderBy(effect => effect.InstanceId)
            .ToList();
        Assert.Equal(4, records.Count);
        Assert.All(records, record =>
        {
            Assert.Equal(1, record.Level);
            Assert.Null(record.ExpirationEvent);
            Assert.Null(record.ExpirationTarget);
        });
        var firstRecord = records[0];
        Assert.Equal(baseVisibility + 150, npc.Being.Visibility);
        Assert.Equal(baseFireResistance - 25, npc.Being.FireResistance);
        var visibilityEffects = npc.ActiveEffects.PropertyEffects.Where(
            effect => effect is
            {
                SourceAbilityType: AbilityType.Debuff,
                TargetName: nameof(BeingComponent.Visibility)
            }).ToList();
        Assert.Equal(3, visibilityEffects.Count);
        Assert.Equal(150, visibilityEffects.Sum(effect => effect.MagnitudePrimary));

        var firstExpiration = sources[0].ExpirationTarget!.Value;
        npc.AI!.PreviousActionTick = firstExpiration;
        manager.Game.CurrentTick = firstExpiration;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(npc, manager);
        manager.Queue.ProcessQueue(manager);

        var replacementRecord = Assert.Single(
            npc.ActiveEffects.ComponentEffects,
            effect => effect.TargetName == AbilityData.Burning.Name);
        Assert.Equal(firstRecord.InstanceId, replacementRecord.InstanceId);
        Assert.Equal(1, replacementRecord.Level);
        Assert.Null(replacementRecord.ExpirationEvent);
        Assert.Null(replacementRecord.ExpirationTarget);
        Assert.Equal(baseVisibility + 50, npc.Being.Visibility);
        Assert.Equal(baseFireResistance, npc.Being.FireResistance);
    }

    [Fact]
    public void Stun_builds_up_against_resistance_before_it_takes_hold()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // Half stun resistance: a single hit only builds up halfway, so it doesn't take hold.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.StunResistance), "test", npc);
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(npc.Being!.HasEffectOrAccumulator(EffectType.Stun));
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, "stunned") > 0);

        // A second hit pushes the build-up over the threshold and the stun takes hold.
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, "stunned") > 0);
    }

    [Fact]
    public void Stun_takes_hold_on_the_first_hit_without_resistance()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, "stunned") > 0);
    }

    [Fact]
    public void Stun_buildup_promotes_beyond_authored_levels()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 250);
        manager.Queue.ProcessQueue(manager);

        var records = npc.ActiveEffects!.ComponentEffects
            .Where(effect => effect is { EffectType: EffectType.AddAbility, SourceAbilityEntityId: 0 }
                             && effect.TargetName == AbilityData.Stunned.Name)
            .OrderBy(effect => effect.InstanceId)
            .ToList();
        Assert.Equal(2, records.Count);
        Assert.All(records, record => Assert.Equal(1, record.Level));
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name));
        Assert.Equal(50, npc.Being!.UnableToActPercent);
        Assert.Equal(50, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);
        var levelTwoExpiration = records[1].ExpirationTarget;
        Assert.All(records, record => Assert.Equal(levelTwoExpiration, record.ExpirationTarget));

        manager.Game.CurrentTick += 10;
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 50);
        manager.Queue.ProcessQueue(manager);

        records = npc.ActiveEffects.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Stunned.Name)
            .OrderBy(effect => effect.InstanceId)
            .ToList();
        Assert.Equal(3, records.Count);
        Assert.All(records, record => Assert.Equal(1, record.Level));
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name));
        Assert.Equal(100, npc.Being!.UnableToActPercent);
        Assert.Equal(0, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);
        Assert.True(records[2].ExpirationTarget > levelTwoExpiration);
        var maximumExpiration = records[2].ExpirationTarget;
        Assert.All(records, record => Assert.Equal(maximumExpiration, record.ExpirationTarget));
        Assert.Equal(maximumExpiration, npc.ActiveEffects.NextEffectTick);

        manager.Game.CurrentTick += 10;
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        records = npc.ActiveEffects.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Stunned.Name)
            .OrderBy(effect => effect.InstanceId)
            .ToList();
        Assert.Equal(4, records.Count);
        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name));
        Assert.Equal(0, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);
        Assert.All(records, record => Assert.True(record.ExpirationTarget > maximumExpiration));
    }

    [Fact]
    public void Stunned_status_is_carried_by_a_timed_component_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var startTick = manager.Game.CurrentTick;
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        var record = Assert.Single(
            npc.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.AddAbility
                      && effect.SourceAbilityEntityId == 0
                      && effect.TargetName == AbilityData.Stunned.Name);
        Assert.Equal(
            startTick + Effect.GetMetadata(EffectType.Stun).GetStatusAbilityDuration(npc, level: 1),
            record.ExpirationTarget);
        Assert.Equal(EffectDuration.UntilTimeout, record.ExpirationEvent);

        manager.EffectApplicationSystem.RealizeBuildUps(
            npc, TimeSystem.DefaultActionDelay, manager);
        manager.EffectApplicationSystem.RealizeBuildUps(
            npc, TimeSystem.DefaultActionDelay, manager);
        Assert.False(npc.ActiveEffects.Accumulators.ContainsKey(EffectType.Stun));
        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name) > 0);

        npc.AI!.PreviousActionTick = startTick;
        npc.AI.NextActionTick = startTick + TimeSystem.DefaultActionDelay;
        record.ExpirationEvent = EffectDuration.UntilTimeout;
        record.ExpirationTarget = startTick + 1;
        npc.ActiveEffects.NextEffectTick = record.ExpirationTarget;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            npc.ActiveEffects.ComponentEffects,
            effect => effect.TargetName == AbilityData.Stunned.Name);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name) > 0);
    }

    [Theory]
    [InlineData(EffectDuration.Instant)]
    [InlineData(EffectDuration.UntilTimeout)]
    [InlineData(EffectDuration.UntilXPGained)]
    public void Status_ability_duration_uses_default_duration_to_choose_expiration_axis(
        EffectDuration defaultDuration)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var metadata = new EffectMetadata
        {
            EffectType = EffectType.Stun,
            StatusAbilityName = AbilityData.Stunned.Name,
            StatusAbilityDuration = "10",
            Duration = defaultDuration
        };

        var expiration = EffectApplicationSystem.GetStatusAbilityExpiration(metadata, player, level: 1);

        if (defaultDuration == EffectDuration.UntilXPGained)
        {
            Assert.Equal(EffectDuration.UntilXPGained, expiration.Event);
            Assert.Equal(10, expiration.Target);
        }
        else
        {
            Assert.Equal(EffectDuration.UntilTimeout, expiration.Event);
            Assert.Equal(manager.Game.CurrentTick + 10, expiration.Target);
        }
    }

    [Fact]
    public void Bound_status_is_carried_by_a_timed_component_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var startTick = manager.Game.CurrentTick;
        ApplyStatus(npc, EffectType.Bind, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        var record = Assert.Single(
            npc.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.AddAbility
                      && effect.SourceAbilityEntityId == 0
                      && effect.TargetName == AbilityData.Bound.Name);
        Assert.Equal(
            startTick + Effect.GetMetadata(EffectType.Bind).GetStatusAbilityDuration(npc, level: 1),
            record.ExpirationTarget);
        Assert.Equal(EffectDuration.UntilTimeout, record.ExpirationEvent);
    }

    [Fact]
    public void Bound_level_two_reduces_evasion()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseEvasion = npc.Being!.Evasion;

        ApplyStatus(npc, EffectType.Bind, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Bound.Name));
        Assert.Equal(baseEvasion - 25, npc.Being.Evasion);
    }

    [Fact]
    public void Bound_level_three_blocks_default_attacks_but_not_spells()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        TestHelper.AddSpellAbility(player);
        manager.Queue.ProcessQueue(manager);

        var defaultAttack = manager.AbilitySlottingSystem.GetAbility(
            player, AbilitySlottingSystem.DefaultMeleeAttackSlot)!;
        var spell = player.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Action == AbilityAction.Spell);
        Assert.Equal(AbilityType.DefaultMeleeAttack, defaultAttack.Ability!.Type);

        string? TryActivate(GameEntity abilityEntity)
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = abilityEntity;
            message.ActivatorEntity = player;
            message.TargetEntity = player;
            message.TargetCell = player.Position!.Cell;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }

        ApplyStatus(player, EffectType.Bind, duration: 100, manager, amount: 300);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(player, AbilityData.Bound.Name));

        var bound = manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.Bound.Name)]!.Ability!;
        Assert.Equal(
            manager.Game.Services.Language.StatusPreventsAbility(bound, defaultAttack.Ability!),
            TryActivate(defaultAttack));
        Assert.NotEqual(
            manager.Game.Services.Language.StatusPreventsAbility(bound, spell.Ability!),
            TryActivate(spell));
    }

    [Fact]
    public void Paralyzed_level_one_reduces_physical_status_resistance_without_touching_deflection()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseResistance = npc.Being!.PhysicalStatusResistance;
        var baseDeflection = npc.Being.Deflection;

        ApplyStatus(npc, EffectType.Paralyze, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Paralyzed.Name));
        Assert.Equal(baseResistance - 25, npc.Being.PhysicalStatusResistance);
        Assert.Equal(baseDeflection, npc.Being.Deflection);
    }

    [Fact]
    public void Stuck_level_two_reduces_evasion()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseEvasion = npc.Being!.Evasion;

        ApplyStatus(npc, EffectType.Stick, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stuck.Name));
        Assert.Equal(baseEvasion - 25, npc.Being.Evasion);
    }

    [Fact]
    public void Uncapped_stunned_levels_expire_without_a_cap_created_reserve()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var startTick = manager.Game.CurrentTick;
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 300);
        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        var firstRecords = npc.ActiveEffects!.ComponentEffects
            .Where(effect => effect.EffectType == EffectType.AddAbility
                             && effect.SourceAbilityEntityId == 0
                             && effect.TargetName == AbilityData.Stunned.Name)
            .ToList();
        Assert.Equal(5, firstRecords.Count);
        Assert.All(firstRecords, record => Assert.Equal(1, record.Level));
        Assert.Equal(0, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);

        npc.AI!.PreviousActionTick = startTick;
        npc.AI.NextActionTick = startTick + TimeSystem.DefaultActionDelay;
        foreach (var record in firstRecords)
        {
            record.ExpirationEvent = EffectDuration.UntilTimeout;
            record.ExpirationTarget = startTick + 1;
        }
        npc.ActiveEffects.NextEffectTick = startTick + 1;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            npc.ActiveEffects.ComponentEffects,
            effect => effect.TargetName == AbilityData.Stunned.Name);
        Assert.Equal(0, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name));
    }

    [Fact]
    public void Incapacitation_condition_is_imposed_by_the_applied_status_ability()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, "stunned") > 0);

        // The build-up is tracked by a single accumulator instance that carries no condition binding of
        // its own; the applied effect-less "stunned" status ability imposes the condition.
        var accumulators = npc.ActiveEffects!.Accumulators
            .Where(e => e.EffectType == EffectType.Stun)
            .ToList();
        Assert.Single(accumulators);
        Assert.Equal(
            ValueCombinationFunction.BuildUpFixed,
            Effect.GetMetadata(accumulators[0].EffectType).CombinationFunction);
    }

    [Fact]
    public void Status_buildup_threshold_is_fixed()
    {
        var level = TestHelper.BuildRoom(".");
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        npc.AI!.PreviousActionTick = 100;
        npc.AI.NextActionTick = 350;

        Assert.Equal(
            EffectApplicationSystem.BuildUpThreshold,
            EffectApplicationSystem.GetBuildUpThreshold(ValueCombinationFunction.BuildUpFixed, npc));
        Assert.Equal(
            EffectApplicationSystem.BuildUpThreshold,
            EffectApplicationSystem.GetBuildUpThreshold(ValueCombinationFunction.BuildUpPerSource, npc));
        Assert.Equal(
            npc.Being!.HitPoints,
            EffectApplicationSystem.GetBuildUpThreshold(ValueCombinationFunction.BuildUpHealth, npc));
    }

    [Fact]
    public void Attributes_supply_category_status_resistance_and_modifiers_stack_additively()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(10, npc.Being!.Might);
        Assert.Equal(10, npc.Being.Focus);
        Assert.Equal(50, npc.Being.PhysicalStatusResistance);
        Assert.Equal(50, npc.Being.MentalStatusResistance);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.PhysicalStatusResistance), "physical test", npc);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -25, nameof(BeingComponent.MentalStatusResistance), "mental test", npc);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(75, npc.Being.PhysicalStatusResistance);
        Assert.Equal(25, npc.Being.MentalStatusResistance);
    }

    [Fact]
    public void Stun_buildup_decays_over_time_and_is_removed_when_it_reaches_zero()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // Half stun resistance: a single hit builds up to 50, below the threshold, so it doesn't take hold.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.StunResistance), "test", npc);
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            50,
            npc.ActiveEffects!.Accumulators[EffectType.Stun].Value);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, "stunned") > 0);

        // Processing without elapsed time neither decays nor removes the pending build-up.
        manager.EffectApplicationSystem.RealizeBuildUps(npc, elapsed: 0, manager);
        Assert.Equal(
            50,
            npc.ActiveEffects!.Accumulators[EffectType.Stun].Value);

        // With no active status, one action interval decays the residual build-up by 10 points.
        manager.EffectApplicationSystem.RealizeBuildUps(npc, elapsed: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            40,
            npc.ActiveEffects!.Accumulators[EffectType.Stun].Value);

        // Enough elapsed time decays the remaining build-up to nothing, so the effect is removed.
        manager.EffectApplicationSystem.RealizeBuildUps(npc, elapsed: 100 * 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.False(npc.Being!.HasEffectOrAccumulator(EffectType.Stun));
    }

    [Fact]
    public void Active_status_level_scales_buildup_decay_without_promoting_or_refreshing()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Stun, duration: 100, manager, amount: 250);
        manager.Queue.ProcessQueue(manager);

        var records = npc.ActiveEffects!.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Stunned.Name)
            .ToList();
        Assert.Equal(2, records.Count);
        var expirations = records.Select(record => record.ExpirationTarget).ToList();

        manager.EffectApplicationSystem.RealizeBuildUps(
            npc, elapsed: TimeSystem.DefaultActionDelay, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Stunned.Name));
        Assert.Equal(20, npc.ActiveEffects.Accumulators[EffectType.Stun].Value);
        Assert.Equal(expirations, records.Select(record => record.ExpirationTarget));
    }

    // Applies the engulfer's Engulf effect to the victim (grappling it) and drains the queue.
    private static void Engulf(GameEntity engulfer, GameEntity victim, GameManager manager)
    {
        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);

        TestHelper.ApplyEffect(engulfEffect, engulfer, victim, manager);
        manager.Queue.ProcessQueue(manager);
        var level = EffectApplicationSystem.GetAbilityLevel(victim, AbilityData.Engulfed.Name);
        if (level != 0)
        {
            Assert.InRange(level, 2, 3);
            Assert.Same(victim, manager.EngulferToVictimRelationship.GetVictim(engulfer));
            Assert.Same(engulfer, manager.EngulferToVictimRelationship.GetEngulfer(victim));
        }
    }

    [Fact]
    public void Engulfed_levels_add_hindrance_sight_loss_and_evasion_penalty()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var sourceAbilityId = engulfer.ActiveEffects!.Abilities
            .SelectMany(ability => ability.Ability!.Effects)
            .Single(effect => effect.Effect!.EffectType == EffectType.Engulf)
            .Effect!.ContainingAbilityId!.Value;
        var baseHindrance = victim.Being!.Hindrance;
        var baseEvasion = victim.Being.Evasion;
        var primaryVision = victim.Being.PrimaryVisionRange;
        var secondaryVision = victim.Being.SecondaryVisionRange;

        ApplyEngulfBuildup(200);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(victim, AbilityData.Engulfed.Name));
        Assert.Same(victim, manager.EngulferToVictimRelationship.GetVictim(engulfer));
        Assert.Same(engulfer, manager.EngulferToVictimRelationship.GetEngulfer(victim));
        Assert.Equal(baseHindrance + 25, victim.Being.Hindrance);
        Assert.Equal(primaryVision, victim.Being.PrimaryVisionRange);
        Assert.Equal(secondaryVision, victim.Being.SecondaryVisionRange);
        Assert.True(victim.Being.Evasion < baseEvasion);
        var levelOneEvasion = victim.Being.Evasion;

        var travel = TravelMessage.Create(manager);
        travel.ActorEntity = victim;
        travel.TargetHeading = Direction.East;
        travel.TargetCell = new Point(2, 0);
        manager.Enqueue(travel);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(new Point(1, 0), victim.Position!.Cell);

        ApplyEngulfBuildup(200);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(victim, AbilityData.Engulfed.Name));
        Assert.Equal(0, victim.Being.PrimaryVisionRange);
        Assert.Equal(0, victim.Being.SecondaryVisionRange);
        Assert.Equal(levelOneEvasion, victim.Being.Evasion);

        ApplyEngulfBuildup(200);

        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(victim, AbilityData.Engulfed.Name));
        Assert.Equal(levelOneEvasion - 25, victim.Being.Evasion);

        void ApplyEngulfBuildup(int amount)
        {
            manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
                EffectType.Engulf, victim, amount, sourceAbilityId));
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Fact]
    public void Engulfed_creatures_remain_on_map_without_digestion()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        Engulf(engulfer, victim, manager);
        Assert.True(victim.Being!.HasEffectOrAccumulator(EffectType.Engulf));

        Assert.Equal(new Point(1, 0), victim.Position!.Cell);

        var healthBefore = victim.Being!.HitPoints;

        for (var i = 0; i < 10; i++)
        {
            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(healthBefore, victim.Being!.HitPoints);
        Assert.False(victim.Being.HasEffectOrAccumulator(EffectType.ToxinDamage));
    }

    [Fact]
    public void Digestion_always_succeeds_with_a_speed_delay_only_on_the_engulfed_victim()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        var bystander = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);
        Engulf(engulfer, victim, manager);

        var digestion = engulfer.ActiveEffects!.Abilities.Single(
            a => a.Ability!.Action == AbilityAction.Digestion);
        Assert.Equal(ActivationType.Targeted, digestion.Ability!.Activation);
        Assert.Equal(AbilitySuccessCondition.Always, digestion.Ability.SuccessCondition);
        Assert.Null(digestion.Ability.Accuracy);
        Assert.Equal("100*SpeedModifier()", digestion.Ability.Delay);
        var sylphDigestion = CreatureData.Sylph.Abilities.Single(
            ability => ability.Action == AbilityAction.Digestion);
        Assert.Equal(AbilitySuccessCondition.Always, sylphDigestion.SuccessCondition);
        Assert.Null(sylphDigestion.Accuracy);
        Assert.Equal(
            (int)(10000f / (100 + 10 * engulfer.Being!.Speed)),
            manager.AbilityActivationSystem.GetDelay(digestion.Ability, engulfer));

        var invalid = ActivateAbilityMessage.Create(manager);
        invalid.AbilityEntity = digestion;
        invalid.ActivatorEntity = engulfer;
        invalid.TargetEntity = bystander;
        Assert.Equal(
            manager.Game.Services.Language.InvalidDigestionTarget(digestion.Ability!),
            manager.AbilityActivationSystem.CanActivateAbility(invalid));
        manager.ReturnMessage(invalid);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -victim.Being!.Evasion, nameof(BeingComponent.Evasion), "test digestion evasion", victim);
        manager.Queue.ProcessQueue(manager);

        var valid = ActivateAbilityMessage.Create(manager);
        valid.AbilityEntity = digestion;
        valid.ActivatorEntity = engulfer;
        valid.TargetEntity = victim;
        Assert.Null(manager.AbilityActivationSystem.CanActivateAbility(valid));
        manager.Process(valid);
        manager.Queue.ProcessQueue(manager);

        Assert.True(victim.Being.HasEffectOrAccumulator(EffectType.ToxinDamage));
        Assert.True(victim.Being.HasEffectOrAccumulator(EffectType.Corrode));
    }

    [Fact]
    public void Engulfed_creatures_strike_the_engulfer_that_holds_them_whatever_they_aim_at()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.GelatinousCube.Instantiate(level, new Point(1, 0));
        var bystander = CreatureData.GelatinousCube.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        Engulf(engulfer, victim, manager);
        Assert.True(victim.Being!.HasEffectOrAccumulator(EffectType.Engulf));
        Assert.Equal(new Point(1, 0), victim.Position!.Cell);
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            -100, nameof(BeingComponent.ParalyzeResistance), engulfer, manager);

        // A gelatinous cube's touch delivers Paralyze and, being unblockable, lands reliably.
        var attack = victim.ActiveEffects!.Abilities.First(
            a => a.Ability!.SuccessCondition == AbilitySuccessCondition.UnblockableAttack
                && a.Ability!.Effects.Any(e => e.Effect!.EffectType == EffectType.Paralyze));

        // Aiming the strike at a bystander is no longer refused - while engulfed it is redirected onto the
        // engulfer that holds the victim.
        var check = ActivateAbilityMessage.Create(manager);
        check.AbilityEntity = attack;
        check.ActivatorEntity = victim;
        check.TargetEntity = bystander;
        Assert.Null(manager.AbilityActivationSystem.CanActivateAbility(check));
        manager.ReturnMessage(check);

        var strike = ActivateAbilityMessage.Create(manager);
        strike.AbilityEntity = attack;
        strike.ActivatorEntity = victim;
        strike.TargetEntity = bystander;
        manager.Process(strike);
        manager.Queue.ProcessQueue(manager);

        // The paralyzing blow landed on the engulfer, never on the bystander it was aimed at.
        Assert.True(engulfer.Being!.HasEffectOrAccumulator(EffectType.Paralyze));
        Assert.False(bystander.Being!.HasEffectOrAccumulator(EffectType.Paralyze));
    }

    [Fact]
    public void Deleting_a_dead_engulfer_automatically_removes_its_victims_engulf_effect()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var engulferCell = engulfer.Position!.Cell!.Value;

        Engulf(engulfer, victim, manager);
        var victimCell = victim.Position!.Cell;
        Assert.Equal(new Point(1, 0), victimCell);
        Assert.True(victim.Being!.HasEffectOrAccumulator(EffectType.Engulf));

        // The engulfer is slain.
        engulfer.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        // Its victim remains in place and is no longer grappled. The buildup value is independent of its
        // source ability and remains banked, but there is no active source to put it in force.
        Assert.DoesNotContain(victim.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.Engulf);
        Assert.True(victim.ActiveEffects.Accumulators.ContainsKey(EffectType.Engulf));
        Assert.False(EffectApplicationSystem.GetAbilityLevel(victim, AbilityData.Engulfed.Name) > 0);
        Assert.Null(manager.EngulferToVictimRelationship.GetEngulfer(victim));
        Assert.Null(manager.EngulferToVictimRelationship.GetVictim(engulfer));
        Assert.Equal(victimCell, victim.Position!.Cell);
        Assert.True(victim.Position.Cell!.Value.DistanceTo(engulferCell) <= 1);
    }

    [Fact]
    public void An_engulfed_victims_items_fall_on_the_victims_cell_when_the_engulfer_inventory_is_full()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        // Arm the victim with a dagger before it is swallowed.
        var carriedBefore = victim.Being!.Items.Select(i => i.Id).ToHashSet();
        ItemData.Dagger.Instantiate(victim);
        manager.Queue.ProcessQueue(manager);
        var dagger = victim.Being!.Items.Single(i => !carriedBefore.Contains(i.Id));

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            -engulfer.Physical!.Capacity, nameof(PhysicalComponent.Capacity), engulfer, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, engulfer.Physical.Capacity);
        Engulf(engulfer, victim, manager);
        var victimCell = victim.Position!.Cell!.Value;

        victim.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(dagger.Item!.ContainerId);
        Assert.Equal(victimCell, dagger.Position!.Cell);
        Assert.Same(dagger, level.Items.GetValueOrDefault(victimCell));
    }

    [Fact]
    public void An_engulfed_victims_items_move_into_the_engulfer_inventory_when_there_is_room()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        Assert.NotNull(manager.AbilitySlottingSystem.GetFirstFreeSlot(engulfer));
        var carriedBefore = victim.Being!.Items.Select(i => i.Id).ToHashSet();
        ItemData.Dagger.Instantiate(victim);
        manager.Queue.ProcessQueue(manager);
        var dagger = victim.Being.Items.Single(i => !carriedBefore.Contains(i.Id));

        Engulf(engulfer, victim, manager);
        victim.Being.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(engulfer.Id, dagger.Item!.ContainerId);
        Assert.Contains(dagger, engulfer.Being!.Items);
        Assert.Null(dagger.Position!.Cell);
    }

    [Fact]
    public void Engulfed_victim_item_moves_are_deferred_until_the_queue_is_processed()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var victim = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        var carriedBefore = victim.Being!.Items.Select(i => i.Id).ToHashSet();
        ItemData.Dagger.Instantiate(victim);
        manager.Queue.ProcessQueue(manager);
        var dagger = victim.Being.Items.Single(i => !carriedBefore.Contains(i.Id));
        Engulf(engulfer, victim, manager);

        var diedMessage = manager.CreateMessage<DiedMessage>(DiedMessage.Name);
        diedMessage.BeingEntity = victim;
        manager.ItemMovingSystem.Process(diedMessage, manager);
        manager.ReturnMessage(diedMessage);

        Assert.Contains(dagger, victim.Being.Items);
        Assert.Equal(victim.Id, dagger.Item!.ContainerId);

        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(dagger, victim.Being.Items);
        Assert.Equal(engulfer.Id, dagger.Item.ContainerId);
    }

    [Fact]
    public void An_engulfer_cannot_engulf_a_second_creature_while_already_holding_one()
    {
        var level = TestHelper.BuildRoom("...");
        var manager = level.Entity.Manager;

        var engulfer = CreatureData.GelatinousCube.Instantiate(level, new Point(0, 0));
        var first = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        var second = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        manager.Queue.ProcessQueue(manager);

        Engulf(engulfer, first, manager);
        Assert.True(first.Being!.HasEffectOrAccumulator(EffectType.Engulf));

        // A second engulf while already holding one takes no hold: the second creature stays on the map and
        // the engulfer still holds only its first victim.
        Engulf(engulfer, second, manager);
        Assert.False(second.Being!.HasEffectOrAccumulator(EffectType.Engulf));
        Assert.NotNull(second.Position!.Cell);

        Assert.Same(first, manager.EngulferToVictimRelationship.GetVictim(engulfer));
    }

    [Fact]
    public void Giant_spider_has_a_paralyzing_web()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.GiantSpider, EffectType.Paralyze, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Paralyze));
    }

    [Fact]
    public void Giant_spider_weakening_bite_applies_weaken()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var weakeningBite = CreatureData.GiantSpider.Abilities.Single(
            ability => ability.Name == "weakening bite");

        Assert.Contains(weakeningBite.Effects!, effect => effect is Weaken);
        Assert.DoesNotContain(
            weakeningBite.Effects!,
            effect => effect is ChangeProperty<int> change && change.PropertyName == nameof(BeingComponent.Might));

        var target = ApplyCreatureAbilityStatus(
            CreatureData.GiantSpider, EffectType.Weaken, manager, level);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Weakened.Name));
        Assert.Equal(9, target.Being!.Might);
    }

    [Theory]
    [InlineData(100, 1)]
    [InlineData(200, 2)]
    [InlineData(300, 3)]
    [InlineData(400, 3)]
    public void Weakened_reduces_might_by_one_per_authored_level(int amount, int expectedReduction)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var baseMight = npc.Being!.Might;

        ApplyStatus(npc, EffectType.Weaken, duration: 100, manager, amount: amount);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(expectedReduction, Math.Min(3, EffectApplicationSystem.GetAbilityLevel(
            npc, AbilityData.Weakened.Name)));
        Assert.Equal(baseMight - expectedReduction, npc.Being.Might);
    }

    [Fact]
    public void Acid_blobs_splashback_corrodes_armor()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.AcidBlob, EffectType.Corrode, manager, level);

        // The acid damage itself inflicts the lingering Corroded status.
        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Corrode));
    }

    [Fact]
    public void Lich_has_a_doom_spell()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Lich, EffectType.Doom, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Doom));
    }

    [Fact]
    public void Doom_resistant_creatures_shrug_off_doom()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var lich = CreatureData.Lich.Instantiate(level, new Point(0, 0));
        var skeleton = CreatureData.AnimatedSkeleton.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, skeleton.Being!.DoomResistance);

        var doom = lich.ActiveEffects!.Abilities
            .Single(a => a.Ability!.Effects.Any(e => e.Effect!.EffectType == EffectType.Doom))
            .Ability!.Effects.Single(e => e.Effect!.EffectType == EffectType.Doom);

        TestHelper.ApplyEffect(doom, lich, skeleton, manager);
        manager.Queue.ProcessQueue(manager);

        // Full Doom resistance reduces the buildup to zero, so the status never settles.
        Assert.False(skeleton.Being.HasEffectOrAccumulator(EffectType.Doom));
    }

    [Fact]
    public void Mummys_touch_decays()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Mummy, EffectType.Decay, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Decay));
    }

    [Fact]
    public void Vampire_enervates()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Vampire, EffectType.Enervate, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Enervate));
    }

    [Fact]
    public void Disenchanter_builds_overcharge_with_a_magnitude()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Disenchanter, EffectType.Overcharge, manager, level);

        // The timed source lands immediately, but contributes only after an elapsed victim turn.
        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Overcharge));
        Assert.Equal(
            0,
            target.ActiveEffects!.Accumulators.GetValueOrDefault(EffectType.Overcharge)?.Value ?? 0);

        target.AI!.PreviousActionTick = manager.Game.CurrentTick;
        target.AI.NextActionTick = manager.Game.CurrentTick + TimeSystem.DefaultActionDelay;
        TestHelper.PerformTurn(target, manager);

        Assert.True(
            target.ActiveEffects!.Accumulators[EffectType.Overcharge].Value > 0);
    }

    [Fact]
    public void White_dragons_breath_chills()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var dragon = CreatureData.WhiteDragon.Instantiate(level, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        // Give the target plenty of health so it survives the icy blast and the chill can be observed.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", target);
        manager.Queue.ProcessQueue(manager);

        var freeze = dragon.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.ColdDamage);

        TestHelper.ApplyEffect(freeze, dragon, target, manager);
        manager.Queue.ProcessQueue(manager);

        // The frost damage itself inflicts the lingering Chilled status.
        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Chill));
    }

    [Theory]
    [InlineData(EffectType.ShockDamage, EffectType.Paralyze)]
    [InlineData(EffectType.WaterDamage, EffectType.Wet)]
    [InlineData(EffectType.PsychicDamage, EffectType.Confuse)]
    [InlineData(EffectType.SonicDamage, EffectType.Daze)]
    [InlineData(EffectType.LightDamage, EffectType.Blind)]
    public void Damage_types_inflict_their_secondary_status(EffectType damage, EffectType status)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // Plenty of health so the blow doesn't kill the target before the status can be observed.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", npc);
        manager.Queue.ProcessQueue(manager);

        ApplyInstantDamageEffect(npc, damage, 10, manager);

        manager.Queue.ProcessQueue(manager);

        // Landing the damage inflicts its lingering secondary status.
        Assert.True(npc.Being!.HasEffectOrAccumulator(status));
    }

    [Fact]
    public void Damage_does_not_inflict_a_rider_when_its_scaled_amount_is_zero()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyInstantDamageEffect(npc, EffectType.ShockDamage, 1, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False(npc.Being!.HasEffectOrAccumulator(EffectType.Paralyze));
    }

    [Fact]
    public void A_lingering_bleed_realizes_its_damage_from_the_instance_over_time()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var player = playerEntity.Player!;
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", playerEntity);
        manager.Queue.ProcessQueue(manager);
        being.HitPoints = 1000;

        // An authored bleed damage-over-time (as a slashing ability now carries alongside its instant blow)
        // banks on a self-expiring accumulator instance that lingers and realizes its damage over time.
        ApplyDamageOverTimeEffect(playerEntity, EffectType.BleedDamage, ratePerAction: 100, duration: 1000, manager);

        Assert.True(being.HasEffectOrAccumulator(EffectType.BleedDamage));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Drained.Name));
        var hpBefore = being.HitPoints;

        // On the player's next turn the bleed instance realizes its damage over the elapsed interval.
        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);
        TestHelper.AdvanceToNextPlayerTurn(manager);

        Assert.True(being.HitPoints < hpBefore,
            $"Expected the bleed instance to deal damage over time; HP {hpBefore} -> {being.HitPoints}.");
    }

    [Theory]
    [InlineData(nameof(CreatureData.Jaguar))]
    [InlineData(nameof(CreatureData.Tiger))]
    [InlineData(nameof(CreatureData.Shark))]
    public void Slashing_predators_make_their_prey_bleed(string creatureName)
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var predator = creatureName switch
        {
            nameof(CreatureData.Jaguar) => CreatureData.Jaguar,
            nameof(CreatureData.Tiger) => CreatureData.Tiger,
            _ => CreatureData.Shark
        };

        var target = ApplyCreatureAbilityStatus(predator, EffectType.BleedDamage, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.BleedDamage));
    }

    [Fact]
    public void Dire_wolfs_bite_causes_bleeding()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.DireWolf, EffectType.BleedDamage, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.BleedDamage));
    }

    [Fact]
    public void Succubus_has_a_silencing_kiss()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Succubus, EffectType.Mute, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Mute));
    }

    [Fact]
    public void Homunculus_bite_applies_dazed_level_two()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Homunculus, EffectType.Daze, manager, level);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Dazed.Name));
    }

    [Fact]
    public void Ghosts_touch_withers()
    {
        Assert.Contains(
            CreatureData.Ghost.Abilities.SelectMany(ability => ability.Effects ?? []),
            effect => effect is Wither);
    }

    [Fact]
    public void Eyedra_has_a_gaze_that_dazes()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(CreatureData.Eyedra, EffectType.Daze, manager, level);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Dazed.Name));
    }

    [Fact]
    public void Cripple_hinders_movement_without_touching_attack_speed_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;
        var baseHindrance = npc.Being.Hindrance;
        var baseEvasion = npc.Being.Evasion;
        var baseMovementDelay = npc.Position!.MovementDelay;

        ApplyStatus(npc, EffectType.Cripple, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        // Movement is hindered, but the raw speed that drives attack/cast timing is untouched.
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Crippled.Name));
        Assert.Equal(baseHindrance + 25, npc.Being.Hindrance);
        Assert.True(npc.Being.Evasion < baseEvasion);
        Assert.Equal(baseSpeed, npc.Being.Speed);
        Assert.True(npc.Position.MovementDelay > baseMovementDelay);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseHindrance, npc.Being.Hindrance);
        Assert.Equal(baseMovementDelay, npc.Position.MovementDelay);
    }

    [Fact]
    public void Crippled_level_two_adds_hindrance_and_evasion_penalties()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;
        var baseHindrance = npc.Being.Hindrance;
        var baseEvasion = npc.Being.Evasion;

        ApplyStatus(npc, EffectType.Cripple, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Crippled.Name));
        Assert.Equal(baseHindrance + 50, npc.Being.Hindrance);
        Assert.Equal(baseEvasion - 50, npc.Being.Evasion);
        Assert.Equal(baseSpeed, npc.Being.Speed);
    }

    [Fact]
    public void Dazed_reduces_perception_speed_and_evasion_and_reverts_when_it_ends()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var basePerception = npc.Being!.Perception;
        var baseSpeed = npc.Being.Speed;
        var baseEvasion = npc.Being.Evasion;

        ApplyStatus(npc, EffectType.Daze, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Dazed.Name));
        Assert.Equal(basePerception - 3, npc.Being.Perception);
        Assert.Equal(baseSpeed - 3, npc.Being.Speed);
        // Evasion is derived from the other attributes, so it just has to drop.
        Assert.True(npc.Being.Evasion < baseEvasion);

        // The status wears off, taking all of its linked property changes with it.
        ExpireStatusEffects(npc, manager);

        Assert.Equal(basePerception, npc.Being.Perception);
        Assert.Equal(baseSpeed, npc.Being.Speed);
        Assert.Equal(baseEvasion, npc.Being.Evasion);
    }

    [Fact]
    public void Dazed_level_two_deepens_perception_and_speed_penalties()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var basePerception = npc.Being!.Perception;
        var baseSpeed = npc.Being.Speed;

        ApplyStatus(npc, EffectType.Daze, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Dazed.Name));
        Assert.Equal(basePerception - 5, npc.Being.Perception);
        Assert.Equal(baseSpeed - 5, npc.Being.Speed);
    }

    [Fact]
    public void Decay_lowers_the_health_ceiling_and_clamps_current_health()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = npc.Being!;
        being.HitPoints = being.HitPointMaximum;
        var maxHealth = being.HitPointMaximum;

        ApplyStatus(npc, EffectType.Decay, duration: 100, manager, amount: 5);
        manager.Queue.ProcessQueue(manager);

        Assert.False(npc.ActiveEffects!.Accumulators.ContainsKey(EffectType.Decay));
        // Applying the status reserves health and clamps current health to the usable ceiling.
        Assert.Equal(5, being.ReservedHitPoints);
        Assert.Equal(maxHealth - 5, being.HitPoints);
        var decayedAbility = manager.AffectableAbilitiesIndex[(npc.Id, AbilityData.Decayed.Name)];
        Assert.NotNull(decayedAbility);
        Assert.Contains(npc.ActiveEffects!.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.ChangeProperty,
                TargetName: nameof(BeingComponent.ReservedHitPoints)
            }
            && effect.SourceAbilityEntityId == decayedAbility.Id);

        // Once health drops, it can't be healed back past the lowered ceiling.
        being.HitPoints = 1;
        being.HitPoints = maxHealth;
        Assert.Equal(maxHealth - 5, being.HitPoints);

        // The status wears off and the full ceiling is restored.
        ExpireStatusEffects(npc, manager);

        Assert.Equal(0, being.ReservedHitPoints);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Decayed.Name) > 0);
        being.HitPoints = maxHealth;
        Assert.Equal(maxHealth, being.HitPoints);
    }

    [Fact]
    public void Enervated_lowers_the_energy_ceiling_and_releases_it_when_it_ends()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = npc.Being!;
        being.EnergyPoints = being.EnergyPointMaximum;
        var maxEnergy = being.EnergyPointMaximum;

        ApplyStatus(npc, EffectType.Enervate, duration: 100, manager, amount: 5);
        manager.Queue.ProcessQueue(manager);

        Assert.False(npc.ActiveEffects!.Accumulators.ContainsKey(EffectType.Enervate));
        // The usable energy ceiling drops and current energy is pulled down to it.
        Assert.Equal(5, being.ReservedEnergyPoints);
        Assert.Equal(maxEnergy - 5, being.EnergyPoints);
        var enervatedAbility = manager.AffectableAbilitiesIndex[(npc.Id, AbilityData.Enervated.Name)];
        Assert.NotNull(enervatedAbility);
        Assert.Contains(npc.ActiveEffects!.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.ChangeProperty,
                TargetName: nameof(BeingComponent.ReservedEnergyPoints)
            }
            && effect.SourceAbilityEntityId == enervatedAbility.Id);

        // Energy can't be restored past the lowered ceiling.
        being.EnergyPoints = maxEnergy;
        Assert.Equal(maxEnergy - 5, being.EnergyPoints);

        // The status wears off and the full ceiling is restored.
        ExpireStatusEffects(npc, manager);

        Assert.Equal(0, being.ReservedEnergyPoints);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Enervated.Name) > 0);
        being.EnergyPoints = maxEnergy;
        Assert.Equal(maxEnergy, being.EnergyPoints);
    }

    [Fact]
    public void Decayed_status_ability_tracks_live_amounts_without_copying_source_expiration()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyStatus(
            npc, EffectType.Decay, duration: 100, manager, amount: 5);
        EffectTestHelper.ApplyStatus(npc, EffectType.Decay, duration: 200, manager, amount: 7);
        manager.Queue.ProcessQueue(manager);

        var instances = npc.ActiveEffects!.PropertyEffects
            .Where(effect => effect.EffectType == EffectType.Decay)
            .OrderBy(effect => effect.ExpirationTarget)
            .ToList();
        Assert.Equal(2, instances.Count);
        Assert.Equal(manager.Game.CurrentTick + 100, instances[0].ExpirationTarget);
        Assert.Equal(manager.Game.CurrentTick + 200, instances[1].ExpirationTarget);
        Assert.Equal(12, npc.Being!.ReservedHitPoints);
        var record = Assert.Single(
            npc.ActiveEffects.ComponentEffects,
            effect => effect.TargetName == AbilityData.Decayed.Name);
        Assert.Null(record.ExpirationEvent);
        Assert.Null(record.ExpirationTarget);

        manager.Game.CurrentTick = instances[0].ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(7, npc.Being.ReservedHitPoints);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Decayed.Name) > 0);
        Assert.Null(record.ExpirationEvent);
        Assert.Null(record.ExpirationTarget);

        manager.Game.CurrentTick = instances[1].ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, npc.Being.ReservedHitPoints);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Decayed.Name) > 0);
    }

    [Fact]
    public void Xp_expiration_on_a_non_player_is_converted_to_ticks()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var startTick = manager.Game.CurrentTick;
        TestHelper.ApplyEffect(EffectType.Decay, npc, npc, manager, effect =>
        {
            effect.Duration = EffectDuration.UntilXPGained;
            effect.DurationAmount = "100";
            effect.Amount = "5";
        });
        manager.Queue.ProcessQueue(manager);

        var instance = Assert.Single(
            npc.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.Decay);
        Assert.Equal(EffectDuration.UntilTimeout, instance.ExpirationEvent);
        Assert.Equal(startTick + 100, instance.ExpirationTarget);
    }

    [Theory]
    [InlineData(EffectType.Decay, "decayed")]
    [InlineData(EffectType.Enervate, "enervated")]
    public void Source_propagated_status_does_not_copy_source_xp_expiration(
        EffectType effectType, string abilityName)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyXpExpiringStatus(amount: 5, duration: 100);
        ApplyXpExpiringStatus(amount: 7, duration: 200);

        var record = Assert.Single(
            npc.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == abilityName);
        Assert.Null(record.ExpirationEvent);
        Assert.Null(record.ExpirationTarget);
        Assert.Equal(12, GetReservedAmount());
        Assert.Equal("12", GetStatusAbilityAmount());

        manager.EffectApplicationSystem.RealizeXpExpiringEffects(npc, xp: 150, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(7, GetReservedAmount());
        Assert.Equal("7", GetStatusAbilityAmount());
        Assert.Null(record.ExpirationEvent);
        Assert.Null(record.ExpirationTarget);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(npc, abilityName) > 0);

        manager.EffectApplicationSystem.RealizeXpExpiringEffects(npc, xp: 51, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, GetReservedAmount());
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, abilityName) > 0);

        int GetReservedAmount()
            => effectType == EffectType.Decay
                ? npc.Being!.ReservedHitPoints
                : npc.Being!.ReservedEnergyPoints;

        string? GetStatusAbilityAmount()
            => Assert.Single(
                manager.FindEntity(record.CarrierId)!.Ability!.Effects,
                effect => effect.Effect!.EffectType == EffectType.ChangeProperty).Effect!.Amount;

        void ApplyXpExpiringStatus(int amount, int duration)
        {
            using var effectReference = manager.CreateEntity();
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = effectType;
            effect.Duration = EffectDuration.UntilXPGained;
            effect.DurationAmount = duration.ToString();
            effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, npc, npc, manager);
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Fact]
    public void Drained_stops_health_and_energy_regeneration_from_experience()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = player.Being!;
        var baseSpeed = being.Speed;
        being.HitPoints = 1;
        being.EnergyPoints = 1;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            2, nameof(BeingComponent.Regeneration), "test regeneration", player);
        manager.Queue.ProcessQueue(manager);

        ApplyDamageOverTimeEffect(
            player, EffectType.BleedDamage, ratePerAction: 100, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(player, AbilityData.Drained.Name));
        Assert.Equal(baseSpeed, being.Speed);
        Assert.Equal(0, being.Regeneration);
        Assert.Equal(0, being.EnergyRegeneration);

        manager.XPSystem.AddPlayerXP(6, manager);
        manager.Queue.ProcessQueue(manager);

        // Neither health nor energy is restored while drained.
        Assert.Equal(1, being.HitPoints);
        Assert.Equal(1, being.EnergyPoints);
    }

    [Fact]
    public void Drained_level_two_increases_cooldown_factor()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = player.Being!;
        var baseSpeed = being.Speed;
        var baseCooldownFactor = being.CooldownFactor;

        ApplyDamageOverTimeEffect(
            player, EffectType.BleedDamage, ratePerAction: 200, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(player, AbilityData.Drained.Name));
        Assert.Equal(baseSpeed, being.Speed);
        Assert.Equal(baseCooldownFactor + 100, being.CooldownFactor);
    }

    [Fact]
    public void Warded_levels_cumulatively_reduce_every_other_status_ability()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;

        ApplyStatus(npc, EffectType.Stagger, duration: 100, manager, amount: 100);
        ApplyStatus(npc, EffectType.Slow, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Stagger));
        Assert.Equal(baseSpeed - 2, npc.Being.Speed);

        ApplyWard(amount: 100);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Stagger));
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Slow));
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Staggered.Name));
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Slowed.Name));
        Assert.Equal(baseSpeed, npc.Being.Speed);

        // A later positive level can rise above the active suppression.
        ApplyStatus(npc, EffectType.Stagger, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Staggered.Name));

        ApplyWard(amount: 100);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Staggered.Name));
        Assert.Equal(-1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Slowed.Name));
        Assert.Equal(baseSpeed, npc.Being.Speed);

        ApplyStatus(npc, EffectType.Slow, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Slowed.Name));

        ApplyStatus(npc, EffectType.Chill, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Chill));
        Assert.Equal(-1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Chilled.Name));

        ApplyWard(amount: 100);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Chill));
        Assert.Equal(-2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Chilled.Name));

        ApplyStatus(npc, EffectType.Chill, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(-1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Chilled.Name));

        void ApplyWard(int amount)
        {
            manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Warded);
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Fact]
    public void Canceled_reduces_buffs_and_warded_restores_the_suppressed_levels()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Greased, level: 3);
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(EffectType.Cancel);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Canceled.Name));
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Greased.Name));

        ApplyStatus(EffectType.Cancel);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Canceled.Name));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Greased.Name));

        manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Warded);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Canceled.Name));
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Greased.Name));

        manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Warded);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Canceled.Name));
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Greased.Name));

        void ApplyStatus(EffectType effectType)
        {
            manager.EffectApplicationSystem.ApplyStatus(
                EffectApplicationContext.CreateStatus(effectType, npc, amount: 100));
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Theory]
    [InlineData(EffectType.Sedate)]
    [InlineData(EffectType.Fascinate)]
    public void Damage_clears_interruptible_statuses(EffectType status)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var being = npc.Being!;

        ApplyStatus(
            npc, status, duration: 100, manager,
            amount: EffectTestHelper.GetStatusEffectThreshold(npc));
        manager.Queue.ProcessQueue(manager);
        Assert.True(being.HasEffectOrAccumulator(status));

        ApplyInstantDamageEffect(npc, EffectType.PhysicalDamage, 1, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False(being.HasEffectOrAccumulator(status));
    }

    [Theory]
    [InlineData(EffectType.Sedate)]
    [InlineData(EffectType.Fascinate)]
    public void Healing_does_not_clear_interruptible_statuses(EffectType status)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var being = npc.Being!;

        ApplyStatus(
            npc, status, duration: 100, manager,
            amount: EffectTestHelper.GetStatusEffectThreshold(npc));
        manager.Queue.ProcessQueue(manager);
        being.HitPoints--;

        ApplyHeal(npc, amount: 1, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(being.HasEffectOrAccumulator(status));
    }

    [Fact]
    public void Warded_suppression_restores_health_and_energy_reservations_when_removed()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(npc, EffectType.Decay, duration: 1000, manager, amount: 5);
        ApplyStatus(npc, EffectType.Enervate, duration: 1000, manager, amount: 7);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(5, npc.Being!.ReservedHitPoints);
        Assert.Equal(7, npc.Being.ReservedEnergyPoints);

        var wardedRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            npc, AbilityData.Warded, level: 3);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Warded.Name));
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Decay));
        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Enervate));
        Assert.Equal(0, npc.Being.ReservedHitPoints);
        Assert.Equal(0, npc.Being.ReservedEnergyPoints);

        EffectApplicationSystem.DetachComponentCarrier(wardedRecord, npc, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(5, npc.Being.ReservedHitPoints);
        Assert.Equal(7, npc.Being.ReservedEnergyPoints);
    }

    [Fact]
    public void Greased_reduces_physical_immobilization_once_per_level()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var baseAccuracy = npc.Being!.Accuracy;
        var baseQuietness = npc.Being.Quietness;

        manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Bound);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Bound.Name));

        manager.EffectApplicationSystem.AddAbilityEffect(npc, AbilityData.Greased);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Greased.Name));
        Assert.Equal(baseAccuracy, npc.Being.Accuracy);
        Assert.Equal(baseQuietness + 25, npc.Being.Quietness);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Bound.Name));

        var levelTwoNpc = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.AddAbilityEffect(levelTwoNpc, AbilityData.Bound);
        manager.EffectApplicationSystem.AddAbilityEffect(levelTwoNpc, AbilityData.Greased, level: 2);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(levelTwoNpc, AbilityData.Greased.Name));
        Assert.Equal(-1, EffectApplicationSystem.GetAbilityLevel(levelTwoNpc, AbilityData.Bound.Name));
        Assert.Single(
            levelTwoNpc.ActiveEffects!.ComponentEffects,
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: "bound" });

        // Suppression lowers the effective level rather than blocking a later level from being added.
        manager.EffectApplicationSystem.AddAbilityEffect(levelTwoNpc, AbilityData.Bound);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(levelTwoNpc, AbilityData.Bound.Name));

        manager.EffectApplicationSystem.AddAbilityEffect(levelTwoNpc, AbilityData.Blinded);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(levelTwoNpc, AbilityData.Blinded.Name));
    }

    [Fact]
    public void Soaking_puts_out_burning()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyResourceOverTime(npc, EffectType.FireDamage, ratePerAction: 10, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(npc.Being!.HasEffectOrAccumulator(EffectType.FireDamage));
        var burningLevel = EffectApplicationSystem.GetAbilityLevel(
            npc, AbilityData.Burning.Name);
        Assert.Equal(0, burningLevel);

        // Becoming wetted suppresses one Burning level without consuming the fire source.
        ApplyStatus(npc, EffectType.Wet, duration: 100, manager, amount: 100);

        manager.Queue.ProcessQueue(manager);

        Assert.True(npc.Being!.HasEffectOrAccumulator(EffectType.FireDamage));
        Assert.Equal(
            burningLevel - 1,
            EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Burning.Name));
    }

    [Fact]
    public void Chilled_slows_and_shifts_cold_and_fire_resistance_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;
        var baseCold = npc.Being.ColdResistance;
        var baseFire = npc.Being.FireResistance;
        var baseHindrance = npc.Being.Hindrance;

        ApplyStatus(npc, EffectType.Chill, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Chilled.Name));
        Assert.Equal(baseSpeed - 2, npc.Being.Speed);
        Assert.Equal(baseCold - 25, npc.Being.ColdResistance);
        Assert.Equal(baseFire + 25, npc.Being.FireResistance);
        Assert.Equal(baseHindrance, npc.Being.Hindrance);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseSpeed, npc.Being.Speed);
        Assert.Equal(baseCold, npc.Being.ColdResistance);
        Assert.Equal(baseFire, npc.Being.FireResistance);
        Assert.Equal(baseHindrance, npc.Being.Hindrance);
    }

    [Fact]
    public void Chilled_level_three_repeats_resistance_shifts_and_adds_hindrance()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseSpeed = npc.Being!.Speed;
        var baseCold = npc.Being.ColdResistance;
        var baseFire = npc.Being.FireResistance;
        var baseHindrance = npc.Being.Hindrance;

        ApplyStatus(npc, EffectType.Chill, duration: 100, manager, amount: 300);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Chilled.Name));
        Assert.Equal(baseSpeed - 6, npc.Being.Speed);
        Assert.Equal(baseCold - 75, npc.Being.ColdResistance);
        Assert.Equal(baseFire + 75, npc.Being.FireResistance);
        Assert.Equal(baseHindrance + 30, npc.Being.Hindrance);
    }

    [Fact]
    public void Wet_shifts_fire_and_electricity_resistance_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseFire = npc.Being!.FireResistance;
        var baseElectricity = npc.Being.ElectricityResistance;
        var baseCold = npc.Being.ColdResistance;
        var baseHindrance = npc.Being.Hindrance;

        ApplyStatus(npc, EffectType.Wet, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseFire + 30, npc.Being.FireResistance);
        Assert.Equal(baseElectricity - 30, npc.Being.ElectricityResistance);
        Assert.Equal(baseCold - 30, npc.Being.ColdResistance);
        Assert.Equal(baseHindrance, npc.Being.Hindrance);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseFire, npc.Being.FireResistance);
        Assert.Equal(baseElectricity, npc.Being.ElectricityResistance);
        Assert.Equal(baseCold, npc.Being.ColdResistance);
        Assert.Equal(baseHindrance, npc.Being.Hindrance);
    }

    [Fact]
    public void Wetted_level_two_repeats_resistance_shifts_and_adds_hindrance()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseFire = npc.Being!.FireResistance;
        var baseElectricity = npc.Being.ElectricityResistance;
        var baseCold = npc.Being.ColdResistance;
        var baseHindrance = npc.Being.Hindrance;

        ApplyStatus(npc, EffectType.Wet, duration: 100, manager, amount: 200);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Wetted.Name));
        Assert.Equal(baseFire + 60, npc.Being.FireResistance);
        Assert.Equal(baseElectricity - 60, npc.Being.ElectricityResistance);
        Assert.Equal(baseCold - 60, npc.Being.ColdResistance);
        Assert.Equal(baseHindrance + 20, npc.Being.Hindrance);
    }

    [Fact]
    public void Corroded_lowers_armor_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.Gargoyle.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseArmor = npc.Being!.Armor;
        var baseDeflection = npc.Being.Deflection;
        var basePhysicalResistance = npc.Being.PhysicalResistance;

        ApplyStatus(npc, EffectType.Corrode, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Corroded.Name));
        Assert.Equal(baseArmor - 2, npc.Being.Armor);
        Assert.Equal(baseDeflection, npc.Being.Deflection);
        Assert.Equal(basePhysicalResistance, npc.Being.PhysicalResistance);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseArmor, npc.Being.Armor);
    }

    [Fact]
    public void Corroded_levels_above_authored_maximum_retain_the_last_effects()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.Gargoyle.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseArmor = npc.Being!.Armor;
        var baseDeflection = npc.Being.Deflection;
        var basePhysicalResistance = npc.Being.PhysicalResistance;

        ApplyStatus(npc, EffectType.Corrode, duration: 100, manager, amount: 300);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Corroded.Name));
        Assert.Equal(baseArmor - 6, npc.Being.Armor);
        Assert.Equal(baseDeflection - 25, npc.Being.Deflection);
        Assert.Equal(basePhysicalResistance - 25, npc.Being.PhysicalResistance);
    }

    [Fact]
    public void Unstable_level_one_reduces_accuracy_and_evasion()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseAccuracy = npc.Being!.Accuracy;
        var baseEvasion = npc.Being.Evasion;

        ApplyStatus(npc, EffectType.Topple, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Unstable.Name));
        Assert.Equal(baseAccuracy * 3 / 4, npc.Being.Accuracy);
        Assert.Equal(baseEvasion * 3 / 4, npc.Being.Evasion);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseAccuracy, npc.Being.Accuracy);
        Assert.Equal(baseEvasion, npc.Being.Evasion);
    }

    [Fact]
    public void Unstable_reapplication_replaces_the_level_one_carrier()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var baseAccuracy = npc.Being!.Accuracy;
        var baseEvasion = npc.Being.Evasion;
        var baseMovementDelay = npc.Position!.MovementDelay;
        var baseTurningDelay = npc.Position.TurningDelay;

        ApplyStatus(npc, EffectType.Topple, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        var record = Assert.Single(
            npc.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == AbilityData.Unstable.Name);
        Assert.Equal(1, record.Level);
        Assert.Equal(baseMovementDelay, npc.Position.MovementDelay);
        Assert.Equal(baseTurningDelay, npc.Position.TurningDelay);

        ApplyStatus(npc, EffectType.Topple, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        var records = npc.ActiveEffects.ComponentEffects
            .Where(effect => effect.TargetName == AbilityData.Unstable.Name)
            .OrderBy(effect => effect.InstanceId)
            .ToList();
        Assert.Equal(2, records.Count);
        Assert.Same(record, records[0]);
        Assert.All(records, levelRecord => Assert.Equal(1, levelRecord.Level));
        Assert.Equal(0, records[0].CarrierId);
        Assert.NotEqual(0, records[1].CarrierId);
        Assert.Equal(2, manager.FindEntity(records[1].CarrierId)!.Ability!.Level);
        Assert.Equal(baseAccuracy / 2, npc.Being!.Accuracy);
        Assert.Equal(baseEvasion / 2, npc.Being.Evasion);
        Assert.Equal(baseMovementDelay * 3 / 2, npc.Position.MovementDelay);
        Assert.Equal(baseTurningDelay * 3 / 2, npc.Position.TurningDelay);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseAccuracy, npc.Being.Accuracy);
        Assert.Equal(baseEvasion, npc.Being.Evasion);
        Assert.Equal(baseMovementDelay, npc.Position.MovementDelay);
        Assert.Equal(baseTurningDelay, npc.Position.TurningDelay);
    }

    [Fact]
    public void Shocking_sphere_builds_unstable_with_a_concussive_burst()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.ShockingSphere, EffectType.Topple, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Topple));
    }

    [Fact]
    public void Unstable_does_not_reduce_raw_melee_physical_damage()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var bite = attacker.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Bite);
        var effects = bite.Ability!.Effects.ToList();

        var damageBefore = manager.EffectApplicationSystem.GetExpectedDamage(effects, attacker, target);

        ApplyStatus(attacker, EffectType.Topple, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        var damageAfter = manager.EffectApplicationSystem.GetExpectedDamage(effects, attacker, target);

        Assert.Equal(damageBefore, damageAfter);
    }

    [Fact]
    public void Fascinated_level_one_lowers_mental_status_resistance_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseResistance = npc.Being!.MentalStatusResistance;

        ApplyStatus(
            npc, EffectType.Fascinate, EffectTestHelper.GetStatusEffectThreshold(npc), manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));
        Assert.Equal(baseResistance - 25, npc.Being.MentalStatusResistance);

        // Damage snaps the fascination, and its debuffs revert with the condition.
        ApplyInstantDamageEffect(npc, EffectType.PhysicalDamage, 1, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, "fascinated") > 0);
        Assert.Equal(baseResistance, npc.Being.MentalStatusResistance);
    }

    [Fact]
    public void Confused_level_one_lowers_mental_status_resistance_and_reverts()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseResistance = npc.Being!.MentalStatusResistance;

        ApplyStatus(
            npc, EffectType.Confuse, EffectTestHelper.GetStatusEffectThreshold(npc), manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Confused.Name));
        Assert.Equal(baseResistance - 25, npc.Being.MentalStatusResistance);

        manager.EffectApplicationSystem.RemoveAbility(
            npc, AbilityData.Confused.Name, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Confused.Name));
        Assert.Equal(baseResistance, npc.Being.MentalStatusResistance);
    }

    [Fact]
    public void Fascinated_builds_up_before_it_takes_hold()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var threshold = EffectTestHelper.GetStatusEffectThreshold(npc);
        var baseResistance = npc.Being!.MentalStatusResistance;

        // A brief gaze remains below the fixed threshold.
        ApplyStatus(npc, EffectType.Fascinate, duration: threshold * 3 / 5, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(npc.Being.HasEffectOrAccumulator(EffectType.Fascinate));
        Assert.False(EffectApplicationSystem.GetAbilityLevel(npc, "fascinated") > 0);
        Assert.Equal(baseResistance, npc.Being.MentalStatusResistance);

        // A second gaze reaches level 1.
        ApplyStatus(npc, EffectType.Fascinate, duration: threshold * 3 / 5, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Fascinated.Name));
        Assert.Equal(baseResistance - 25, npc.Being.MentalStatusResistance);
    }

    [Fact]
    public void Will_o_wisp_has_a_mesmerizing_gaze()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.WillOWisp, EffectType.Fascinate, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Fascinate));
    }

    [Fact]
    public void Staggered_levels_encumber_through_hindrance_and_revert()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseHindrance = npc.Being!.Hindrance;

        ApplyStatus(npc, EffectType.Stagger, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Staggered.Name));
        Assert.Equal(baseHindrance + 25, npc.Being.Hindrance);

        ApplyStatus(npc, EffectType.Stagger, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Staggered.Name));
        Assert.Equal(baseHindrance + 50, npc.Being.Hindrance);

        ExpireStatusEffects(npc, manager);

        Assert.Equal(baseHindrance, npc.Being.Hindrance);
    }

    [Fact]
    public void Gas_spore_blast_staggers()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.GasSpore, EffectType.Stagger, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Stagger));
    }

    [Fact]
    public void Ghost_has_a_frightening_gaze()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.Ghost, EffectType.Frighten, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Frighten));
    }

    [Fact]
    public void Bugbear_has_a_taunting_bellow()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;

        var target = ApplyCreatureAbilityStatus(
            CreatureData.Bugbear, EffectType.Taunt, manager, level);

        Assert.True(target.Being!.HasEffectOrAccumulator(EffectType.Taunt));
    }

    [Fact]
    public void Blind_status_is_recognized_and_reverts_when_it_ends()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.False(npc.Being!.HasEffectOrAccumulator(EffectType.Blind));

        ApplyStatus(npc, EffectType.Blind, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Blinded.Name));
        Assert.True(npc.Being!.HasEffectOrAccumulator(EffectType.Blind));

        ExpireStatusEffects(npc, manager);
        Assert.False(npc.Being!.HasEffectOrAccumulator(EffectType.Blind));
    }

    [Fact]
    public void Blinded_level_three_reduces_accuracy_and_evasion()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseAccuracy = npc.Being!.Accuracy;
        var baseEvasion = npc.Being.Evasion;

        ApplyStatus(npc, EffectType.Blind, duration: 100, manager, amount: 300);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Blinded.Name));
        Assert.Equal(baseAccuracy - 50, npc.Being.Accuracy);
        Assert.Equal(baseEvasion - 25, npc.Being.Evasion);
    }

    [Fact]
    public void Poison_prevents_health_regeneration_from_experience()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        player.Being!.HitPoints = 1;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            2, nameof(BeingComponent.Regeneration), "test regeneration", player);
        manager.Queue.ProcessQueue(manager);

        ApplyDamageOverTimeEffect(player, EffectType.ToxinDamage, ratePerAction: 100, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(player, AbilityData.Blighted.Name));
        Assert.Equal(0, player.Being.Regeneration);

        manager.XPSystem.AddPlayerXP(6, manager);
        manager.Queue.ProcessQueue(manager);

        // Poison blocks the health that experience would otherwise restore.
        Assert.Equal(1, player.Being.HitPoints);
    }

    [Fact]
    public void Health_regenerates_from_experience_without_poison()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        player.Being!.HitPoints = 1;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            2, nameof(BeingComponent.Regeneration), "test regeneration", player);
        manager.Queue.ProcessQueue(manager);

        manager.XPSystem.AddPlayerXP(6, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(13, player.Being.HitPoints);
    }

    [Theory]
    [InlineData(EffectType.FireDamage)]
    [InlineData(EffectType.ToxinDamage)]
    public void Damage_over_time_deals_no_instant_damage_and_ticks_scaled_by_elapsed_time(EffectType effectType)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var hitPointsBefore = npc.Being!.HitPoints;

        ApplyResourceOverTime(npc, effectType, ratePerAction: 20, manager);
        manager.Queue.ProcessQueue(manager);

        // A damage effect with a duration deals its damage over time, not instantly on application.
        Assert.Equal(hitPointsBefore, npc.Being.HitPoints);

        // A full action delay of elapsed time inflicts the full per-action rate.
        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            npc, TimeSystem.DefaultActionDelay);
        Assert.Equal(hitPointsBefore - 20, npc.Being.HitPoints);

        // Half an action delay inflicts half the rate.
        var hitPointsAfterFullTick = npc.Being.HitPoints;
        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            npc, TimeSystem.DefaultActionDelay / 2);
        Assert.Equal(hitPointsAfterFullTick - 10, npc.Being.HitPoints);
    }

    [Fact]
    public void Withering_drains_energy_over_time_instead_of_health()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var hitPointsBefore = npc.Being!.HitPoints;
        var energyPointsBefore = npc.Being.EnergyPoints;

        ApplyResourceOverTime(npc, EffectType.Wither, ratePerAction: 10, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(hitPointsBefore, npc.Being.HitPoints);
        Assert.Equal(energyPointsBefore, npc.Being.EnergyPoints);

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            npc, TimeSystem.DefaultActionDelay);

        // Health is untouched; only energy is drained.
        Assert.Equal(hitPointsBefore, npc.Being.HitPoints);
        Assert.Equal(energyPointsBefore - 10, npc.Being.EnergyPoints);
    }

    [Fact]
    public void Poison_halves_incoming_healing()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        npc.Being!.HitPoints = 1;

        ApplyDamageOverTimeEffect(npc, EffectType.ToxinDamage, ratePerAction: 200, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Blighted.Name));
        Assert.Equal(50, npc.Being!.HealingResistance);

        ApplyHeal(npc, amount: 20, manager);
        manager.Queue.ProcessQueue(manager);

        // Only half of the 20 healing is restored while poisoned.
        Assert.Equal(1 + 10, npc.Being.HitPoints);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(50, 10)]
    [InlineData(100, 0)]
    [InlineData(150, -10)]
    [InlineData(200, -20)]
    public void Healing_resistance_reduces_and_inverts_healing(
        int resistance, int expectedChange)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test maximum", npc);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            resistance, nameof(BeingComponent.HealingResistance), "test healing resistance", npc);
        manager.Queue.ProcessQueue(manager);
        npc.Being!.HitPoints = 100;

        ApplyHeal(npc, amount: 20, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100 + expectedChange, npc.Being.HitPoints);
    }

    [Theory]
    [InlineData("animated skeleton", 200)]
    [InlineData("demilich", 200)]
    [InlineData("draugr", 150)]
    [InlineData("ghost", 200)]
    [InlineData("ghoul", 150)]
    [InlineData("lemure", 200)]
    [InlineData("lich", 200)]
    [InlineData("mummy", 150)]
    [InlineData("vampire", 150)]
    [InlineData("zombie", 150)]
    public void Undead_have_authored_healing_resistance(string creatureName, int expected)
    {
        var level = TestHelper.BuildRoom(".");
        var creature = Creature.Loader.Find(creatureName)!.Instantiate(level, new Point(0, 0));
        creature.Manager.Queue.ProcessQueue(creature.Manager);

        Assert.Equal(expected, creature.Being!.HealingResistance);
    }

    [Fact]
    public void Withering_halves_incoming_energy_restoration()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.EnergyPointMaximum), "test", npc);
        manager.Queue.ProcessQueue(manager);

        npc.Being!.EnergyPoints = 1;

        ApplyDamageOverTimeEffect(npc, EffectType.Wither, ratePerAction: 300, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Withered.Name));
        Assert.Equal(0, npc.Being.EnergyRegeneration);
        Assert.Equal(50, npc.Being.EnergyRestorationResistance);

        ApplyRecharge(npc, amount: 20, manager);
        manager.Queue.ProcessQueue(manager);

        // Only half of the 20 energy restoration is applied while withering.
        Assert.Equal(1 + 10, npc.Being.EnergyPoints);
    }

    [Fact]
    public void Withering_level_one_stops_energy_regeneration()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ApplyDamageOverTimeEffect(npc, EffectType.Wither, ratePerAction: 200, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Withered.Name));
        Assert.Equal(0, npc.Being!.EnergyRegeneration);
        Assert.Equal(0, npc.Being.EnergyRestorationResistance);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(50, 10)]
    [InlineData(100, 0)]
    [InlineData(150, -10)]
    [InlineData(200, -20)]
    public void Energy_restoration_resistance_reduces_and_inverts_recharge(
        int resistance, int expectedChange)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.EnergyPointMaximum), "test maximum", npc);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            resistance, nameof(BeingComponent.EnergyRestorationResistance), "test energy resistance", npc);
        manager.Queue.ProcessQueue(manager);
        npc.Being!.EnergyPoints = 100;

        ApplyRecharge(npc, amount: 20, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100 + expectedChange, npc.Being.EnergyPoints);
    }

    [Theory]
    [InlineData(EffectType.ToxinDamage)]
    [InlineData(EffectType.Wither)]
    public void Severe_blight_and_withering_block_direct_restoration(EffectType effectType)
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.EnergyPointMaximum), "test", npc);
        manager.Queue.ProcessQueue(manager);

        npc.Being!.HitPoints = 1;
        npc.Being.EnergyPoints = 1;

        ApplyDamageOverTimeEffect(npc, effectType, ratePerAction: 400, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        var abilityName = effectType == EffectType.ToxinDamage
            ? AbilityData.Blighted.Name
            : AbilityData.Withered.Name;
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(npc, abilityName));
        if (effectType == EffectType.ToxinDamage)
        {
            Assert.Equal(100, npc.Being.HealingResistance);
        }
        else
        {
            Assert.Equal(100, npc.Being.EnergyRestorationResistance);
        }

        if (effectType == EffectType.ToxinDamage)
        {
            ApplyHeal(npc, amount: 20, manager);
        }
        else
        {
            ApplyRecharge(npc, amount: 20, manager);
        }
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, effectType == EffectType.ToxinDamage
            ? npc.Being.HitPoints
            : npc.Being.EnergyPoints);
    }

    [Fact]
    public void Muted_level_two_prevents_casting_spells()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var caster = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        TestHelper.AddSpellAbility(caster);
        manager.Queue.ProcessQueue(manager);

        var spell = caster.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Spell);

        string? TryActivate()
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = spell;
            message.ActivatorEntity = caster;
            message.TargetCell = caster.Position!.Cell;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }

        // Without the mute the spell is not blocked for being muted.
        Assert.DoesNotContain("muted", TryActivate() ?? "", StringComparison.OrdinalIgnoreCase);

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));
        var muted = manager.AffectableAbilitiesIndex[
            (caster.Id, AbilityData.Muted.Name)]!.Ability!;
        var mutedError = manager.Game.Services.Language.StatusPreventsAbility(
            muted, spell.Ability!);
        Assert.NotEqual(mutedError, TryActivate());

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));

        Assert.Equal(mutedError, TryActivate());
    }

    [Fact]
    public void Deafened_levels_reduce_hearing_and_revert()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var npc = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(20, npc.Being!.HearingLevel);
        Assert.Equal(200, npc.Being.Quietness);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            10, nameof(BeingComponent.Hindrance), npc, manager);
        Assert.Equal(190, npc.Being.Quietness);

        ApplyStatus(npc, EffectType.Deafen, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Deafened.Name));
        Assert.Equal(10, npc.Being.HearingLevel);
        Assert.Equal(50, npc.Being.FollowerCommandFailurePercent);

        ApplyStatus(npc, EffectType.Deafen, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(npc, AbilityData.Deafened.Name));
        Assert.Equal(0, npc.Being.HearingLevel);
        Assert.Equal(50, npc.Being.FollowerCommandFailurePercent);

        ExpireStatusEffects(npc, manager);
        Assert.Equal(20, npc.Being.HearingLevel);
        Assert.Equal(0, npc.Being.FollowerCommandFailurePercent);
    }

    [Fact]
    public void Muted_level_one_doubles_spell_energy_cost()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var caster = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        var spell = new Ability
        {
            Name = "costed test spell",
            Type = AbilityType.RangedSpell,
            Activation = ActivationType.Targeted,
            Trigger = ActivationType.OnRangedAttack,
            Action = AbilityAction.Spell,
            EnergyPointCost = 10,
            Effects = new List<Effect> { new Heal { Amount = "0" } }
        }.AddToAffectable(caster).Entity;
        manager.Queue.ProcessQueue(manager);

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));
        Assert.Equal(20, AbilityActivationSystem.GetEnergyCost(spell.Ability!));
        caster.Being!.EnergyPoints = caster.Being.EnergyPointMaximum;
        var energyBefore = caster.Being!.EnergyPoints;

        TestHelper.ActivateAbility(spell, caster, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(energyBefore - 20, caster.Being.EnergyPoints);
    }

    [Fact]
    public void Muted_level_two_doubles_verbal_cost_and_level_three_blocks_verbal_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var caster = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        var scream = new Ability
        {
            Name = "costed test scream",
            Activation = ActivationType.Targeted,
            Action = AbilityAction.Scream,
            EnergyPointCost = 10,
            Effects = new List<Effect> { new Heal { Amount = "0" } }
        }.AddToAffectable(caster).Entity;
        var miracle = new Ability
        {
            Name = "costed test miracle",
            Type = AbilityType.Miracle,
            Activation = ActivationType.Targeted,
            EnergyPointCost = 10,
            Effects = new List<Effect> { new Heal { Amount = "0" } }
        }.AddToAffectable(caster).Entity;
        manager.Queue.ProcessQueue(manager);
        caster.Being!.EnergyPoints = caster.Being.EnergyPointMaximum;

        string? TryActivate()
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = scream;
            message.ActivatorEntity = caster;
            message.TargetEntity = caster;
            message.TargetCell = caster.Position!.Cell;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));
        Assert.Equal(10, AbilityActivationSystem.GetEnergyCost(scream.Ability!));
        Assert.Equal(10, AbilityActivationSystem.GetEnergyCost(miracle.Ability!));
        Assert.True(caster.Being!.CanUseComplexFollowerCommands);
        Assert.True(caster.Being.CanUseDivineAbilities);
        Assert.True(caster.Being.CanUseFollowerCommands);
        Assert.Null(TryActivate());

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));
        Assert.Equal(20, AbilityActivationSystem.GetEnergyCost(scream.Ability!));
        Assert.Equal(20, AbilityActivationSystem.GetEnergyCost(miracle.Ability!));
        Assert.False(caster.Being!.CanUseComplexFollowerCommands);
        Assert.True(caster.Being.CanUseDivineAbilities);
        Assert.True(caster.Being.CanUseFollowerCommands);
        Assert.Null(TryActivate());

        ApplyStatus(caster, EffectType.Mute, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Muted.Name));
        Assert.Equal(20, AbilityActivationSystem.GetEnergyCost(scream.Ability!));
        Assert.Equal(20, AbilityActivationSystem.GetEnergyCost(miracle.Ability!));
        Assert.False(caster.Being!.CanUseComplexFollowerCommands);
        Assert.False(caster.Being.CanUseDivineAbilities);
        Assert.False(caster.Being.CanUseFollowerCommands);
        var muted = manager.AffectableAbilitiesIndex[
            (caster.Id, AbilityData.Muted.Name)]!.Ability!;
        Assert.Equal(
            manager.Game.Services.Language.StatusPreventsAbility(muted, scream.Ability!),
            TryActivate());
    }

    [Fact]
    public void Dazed_level_two_prevents_using_complex_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var caster = CreatureData.GnomeWizard.Instantiate(level, new Point(0, 0));
        TestHelper.AddSpellAbility(caster);
        manager.Queue.ProcessQueue(manager);

        var spell = caster.ActiveEffects!.Abilities.Single(a => a.Ability!.Action == AbilityAction.Spell);

        string? TryActivate()
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = spell;
            message.ActivatorEntity = caster;
            message.TargetCell = caster.Position!.Cell;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }

        Assert.DoesNotContain("dazed", TryActivate() ?? "", StringComparison.OrdinalIgnoreCase);

        ApplyStatus(caster, EffectType.Daze, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Dazed.Name));
        var dazed = manager.AffectableAbilitiesIndex[
            (caster.Id, AbilityData.Dazed.Name)]!.Ability!;
        var complexAbilityError = manager.Game.Services.Language.StatusPreventsAbility(
            dazed, spell.Ability!);
        Assert.NotEqual(complexAbilityError, TryActivate());

        ApplyStatus(caster, EffectType.Daze, duration: 100, manager, amount: 100);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(caster, AbilityData.Dazed.Name));

        Assert.Equal(complexAbilityError, TryActivate());
    }

    [Fact]
    public void Dazed_level_three_blocks_non_default_attacks()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var attacker = CreatureData.Naga.Instantiate(level, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var bite = attacker.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Action == AbilityAction.Bite);
        bite.Ability!.Slot = 0;

        string? TryActivate(GameEntity abilityEntity)
        {
            var message = ActivateAbilityMessage.Create(manager);
            message.AbilityEntity = abilityEntity;
            message.ActivatorEntity = attacker;
            message.TargetEntity = target;
            var error = manager.AbilityActivationSystem.CanActivateAbility(message);
            manager.ReturnMessage(message);
            return error;
        }

        ApplyStatus(attacker, EffectType.Daze, duration: 100, manager, amount: 150);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(attacker, AbilityData.Dazed.Name));
        var dazed = manager.AffectableAbilitiesIndex[
            (attacker.Id, AbilityData.Dazed.Name)]!.Ability!;
        var nonDefaultAbilityError = manager.Game.Services.Language.StatusPreventsAbility(
            dazed, bite.Ability!);
        Assert.NotEqual(nonDefaultAbilityError, TryActivate(bite));

        ApplyStatus(attacker, EffectType.Daze, duration: 100, manager, amount: 75);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(attacker, AbilityData.Dazed.Name));

        Assert.Equal(nonDefaultAbilityError, TryActivate(bite));
        bite.Ability.Type = AbilityType.DefaultMeleeAttack;
        Assert.NotEqual(
            nonDefaultAbilityError,
            TryActivate(bite));
    }

    private static void ApplyHeal(GameEntity target, int amount, GameManager manager)
        => TestHelper.ApplyEffect(EffectType.Heal, target, target, manager, effect =>
        {
        effect.Duration = EffectDuration.Instant;
        effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
        });

    private static void ApplyRecharge(GameEntity target, int amount, GameManager manager)
        => TestHelper.ApplyEffect(EffectType.Recharge, target, target, manager, effect =>
        {
        effect.Duration = EffectDuration.Instant;
        effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
        });

    private static void ApplyResourceOverTime(
        GameEntity target, EffectType effectType, int ratePerAction, GameManager manager)
        => ApplyDamageOverTimeEffect(target, effectType, ratePerAction, duration: 100000, manager);

    // Advances past the target's active timed statuses and realizes their expiry, as the temporal
    // index would when the owner surfaces, so a self-expiring status lifts without a lingering effect entity.
    private static void ExpireStatusEffects(GameEntity target, GameManager manager)
    {
        var activeEffects = target.ActiveEffects!;

        // Let the target take turns until all residual build-up has decayed. Timed status carriers remain
        // independent and are lapsed below at their own expiration ticks.
        for (var guard = 0; guard < 1000
                            && activeEffects.Accumulators.Any(
                                i => Effect.GetMetadata(i.EffectType).CombinationFunction.IsBuildUp()); guard++)
        {
            TestHelper.PerformTurn(target, manager);
        }

        // Lapse any remaining timed status carriers or damage-over-time instances at their latest expiry tick.
        var latest = manager.Game.CurrentTick;
        foreach (var instance in activeEffects.PropertyEffects)
        {
            if (instance is { ExpirationEvent: EffectDuration.UntilTimeout, ExpirationTarget: { } tick }
                && tick > latest)
            {
                latest = tick;
            }
        }

        foreach (var instance in activeEffects.ComponentEffects)
        {
            if (instance is { ExpirationEvent: EffectDuration.UntilTimeout, ExpirationTarget: { } tick }
                && tick > latest)
            {
                latest = tick;
            }
        }

        manager.Game.CurrentTick = Math.Max(manager.Game.CurrentTick, latest);
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(target, manager);
        manager.Queue.ProcessQueue(manager);
    }

    /// <summary>
    ///     Instantiates the attacker and a separate target, then fires the status-carrying effect of
    ///     the attacker's ability at the target (as the ability itself would on a hit) and returns the
    ///     target so the test can assert the resulting status was applied.
    /// </summary>
    private static GameEntity ApplyCreatureAbilityStatus(
        Creature attacker, EffectType status, GameManager manager, RoomComponent level)
    {
        var attackerEntity = attacker.Instantiate(level, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var ability = attackerEntity.ActiveEffects!.Abilities.Single(
            a => a.Ability!.Effects.Any(e => e.Effect!.EffectType == status));
        var effectEntity = ability.Ability!.Effects.Single(e => e.Effect!.EffectType == status);

        TestHelper.ApplyEffect(effectEntity, attackerEntity, target, manager);
        manager.Queue.ProcessQueue(manager);

        return target;
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Data;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using Xunit;

namespace UnicornHack.Persistence;

public class PersistenceEffectTest
{
    [Fact]
    public void Ability_noise_penalty_survives_reload_without_a_registered_template()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var ability = new Ability
        {
            Name = "unregistered quiet ability",
            Type = AbilityType.MeleeAttack,
            Action = AbilityAction.Punch,
            Noise = 7
        }.AddToAffectable(persisted.PlayerEntity);
        var abilityId = ability.Entity.Id;
        persisted.Game.Manager.Queue.ProcessQueue(persisted.Game.Manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        var reloadedAbility = persisted.Game.Manager.FindEntity(abilityId)!.Ability!;
        Assert.Equal(7, reloadedAbility.Noise);
        Assert.Null(reloadedAbility.Template);
    }

    [Fact]
    public void Timed_ability_level_decrease_preserves_expiry_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        manager.EffectApplicationSystem.AddAbilityEffect(player, AbilityData.Acrobatics);
        manager.Queue.ProcessQueue(manager);
        var ability = new Ability
        {
            Name = "test timed ability decrease",
            Type = AbilityType.Status,
            Activation = ActivationType.Manual,
            Effects =
            [
                new DecreaseAbilityLevel
                {
                    AbilityName = AbilityData.Acrobatics.Name,
                    Duration = EffectDuration.UntilTimeout,
                    DurationAmount = "100"
                }
            ]
        }.AddToAffectable(player);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ApplyEffect(Assert.Single(ability.Effects), player, player, manager);
        manager.Queue.ProcessQueue(manager);
        var suppressedAbility = manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.Acrobatics.Name)]!.Ability!;
        Assert.Equal(0, suppressedAbility.Level);
        Assert.False(suppressedAbility.IsUsable);
        Assert.NotEqual(
            0,
            Assert.Single(
                player.ActiveEffects!.ComponentEffects,
                effect => effect is
                {
                    EffectType: EffectType.AddAbility,
                    TargetName: var name
                } && name == AbilityData.Acrobatics.Name).CarrierId);
        var expirationTick = manager.Game.CurrentTick + 100;
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        var marker = Assert.Single(
            player.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel);
        Assert.Equal(EffectDuration.UntilTimeout, marker.ExpirationEvent);
        Assert.Equal(expirationTick, marker.ExpirationTarget);
        Assert.Equal(AbilityData.Acrobatics.Name, marker.TargetName);
        var positiveRecord = Assert.Single(
            player.ActiveEffects.ComponentEffects,
            effect => effect is
            {
                EffectType: EffectType.AddAbility,
                TargetName: var name
            } && name == AbilityData.Acrobatics.Name);
        Assert.NotEqual(0, positiveRecord.CarrierId);
        var reloadedSuppressedAbility = player.Manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.Acrobatics.Name)]!.Ability!;
        Assert.Equal(0, reloadedSuppressedAbility.Level);
        Assert.False(reloadedSuppressedAbility.IsUsable);

        player.Manager.Game.CurrentTick = expirationTick;
        player.Manager.EffectApplicationSystem.ExpireTimedPropertyEffects(player, player.Manager);
        player.Manager.Queue.ProcessQueue(player.Manager);

        Assert.DoesNotContain(marker, player.ActiveEffects.ComponentEffects);
        Assert.NotEqual(0, positiveRecord.CarrierId);
        Assert.NotNull(player.Manager.AffectableAbilitiesIndex[(player.Id, AbilityData.Acrobatics.Name)]);
    }

    [Fact]
    public void Room_bound_ability_level_decrease_preserves_expiry_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var ability = new Ability
        {
            Name = "test room-bound ability decrease",
            Type = AbilityType.Status,
            Activation = ActivationType.Manual,
            Effects =
            [
                new DecreaseAbilityLevel
                {
                    AbilityName = AbilityData.Acrobatics.Name,
                    Duration = EffectDuration.UntilNextRoom
                }
            ]
        }.AddToAffectable(player);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ApplyEffect(Assert.Single(ability.Effects), player, player, manager);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        var marker = Assert.Single(
            persisted.PlayerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel);
        Assert.Equal(EffectDuration.UntilNextRoom, marker.ExpirationEvent);
        Assert.Null(marker.ExpirationTarget);
    }

    [Fact]
    public void Learned_level_carriers_preserve_authority_and_templates_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        manager.EffectApplicationSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
        manager.EffectApplicationSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        var record = Assert.Single(
            player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.WaterSourcery.Name));
        Assert.Equal(2, record.Level);
        Assert.NotEqual(0, record.CarrierId);

        var authoritative = manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.WaterSourcery.Name)]!.Ability!;
        Assert.Equal(2, authoritative.Level);
        Assert.Same(AbilityData.WaterSourcery, authoritative.Template);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.WaterSourcery.Name + "#2")]);
    }

    [Fact]
    public void Component_effect_instance_ids_preserve_order_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var firstRecord = player.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, AbilityData.Stunned.Name);
        var secondRecord = player.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, AbilityData.Stunned.Name);
        var firstInstanceId = firstRecord.InstanceId;
        var secondInstanceId = secondRecord.InstanceId;
        manager.EffectApplicationSystem.RelevelAbility(AbilityData.Stunned.Name, player, manager);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        manager.EffectApplicationSystem.RelevelAbility(AbilityData.Stunned.Name, player, manager);
        manager.Queue.ProcessQueue(manager);

        var records = player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.Stunned.Name);
        var reloadedFirstRecord = Assert.Single(
            records, record => record.InstanceId == firstInstanceId);
        var reloadedSecondRecord = Assert.Single(
            records, record => record.InstanceId == secondInstanceId);
        Assert.Equal(
            reloadedFirstRecord.CarrierId,
            manager.AffectableAbilitiesIndex[(player.Id, AbilityData.Stunned.Name)]!.Id);
        Assert.Equal(
            reloadedSecondRecord.CarrierId,
            manager.AffectableAbilitiesIndex[(player.Id, AbilityData.Stunned.Name + "#2")]!.Id);
    }

    [Fact]
    public void Race_level_carriers_preserve_authority_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var race = manager.Game.Services.Data.PlayerRaces.Get("ogre");
        manager.EffectApplicationSystem.AddAbilityEffect(
            player, race.GetRaceAbilityTemplate());
        manager.EffectApplicationSystem.AddAbilityEffect(
            player, race.GetRaceAbilityTemplate());
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        var record = Assert.Single(player.ActiveEffects!.ComponentEffects.GetValues("ogre race"));
        Assert.Equal(2, record.Level);
        Assert.NotEqual(0, record.CarrierId);

        var authoritative = manager.AffectableAbilitiesIndex[(player.Id, "ogre race")]!;
        Assert.Equal(record.CarrierId, authoritative.Id);
        Assert.Equal(2, authoritative.Ability!.Level);
        Assert.False(authoritative.Ability.IsLeveledSupplement);
        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, "ogre race#2")]);
        Assert.Single(player.Being!.Races, carrier => carrier.Id == authoritative.Id);
    }

    [Fact]
    public void Stale_test_database_schema_is_recreated_automatically()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using (PersistedGame.CreateAndSave())
        {
        }

        using (var connection = new SqlConnection(
                   @"Server=(localdb)\MSSQLLocalDB;Database=UnicornHackTest_PersistenceEffectTest;Integrated Security=True;Connect Timeout=60;"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "ALTER TABLE [EffectComponents] DROP COLUMN [SecondaryAmount]";
            command.ExecuteNonQuery();
        }

        using var recreated = PersistedGame.CreateAndSave();
        Assert.NotNull(recreated.PlayerEntity);
    }

    [Fact]
    public void Active_effect_owned_types_use_the_component_backreference_without_mapping_component_base()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelOnly")
            .Options;
        using var context = new GameDbContext(options);
        var entityTypes = context.Model.GetEntityTypes().ToArray();

        Assert.DoesNotContain(
            entityTypes,
            entityType => entityType.ClrType == typeof(Utils.MessagingECS.Component));

        foreach (var itemType in new[]
                 {
                     typeof(ActivePropertyEffect),
                     typeof(ActiveComponentEffect),
                     typeof(EffectAccumulator)
                 })
        {
            var entityType = Assert.Single(entityTypes, candidate => candidate.ClrType == itemType);
            Assert.Equal(
                nameof(NotificationCollectionItem.ContainingComponent),
                entityType.FindOwnership()!.DependentToPrincipal!.Name);
        }
    }

    [Fact]
    public void Temporal_index_is_built_as_entities_are_loaded()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        persisted.Reload();

        var manager = persisted.Game.Manager;
        var expectedCount = 0;
        foreach (var entity in manager.TemporalEntities)
        {
            var actionTick = TimeSystem.GetTick(entity);
            var effectTick = entity.ActiveEffects?.NextEffectTick;
            var tick = actionTick == null
                ? effectTick
                : effectTick == null
                    ? actionTick
                    : Math.Min(actionTick.Value, effectTick.Value);
            if (tick == null)
            {
                continue;
            }

            expectedCount++;
            Assert.Same(entity, manager.TemporalEntitiesIndex[(tick.Value, entity.Id)]);
        }

        Assert.Equal(expectedCount, manager.TemporalEntitiesIndex.Count);
    }

    [Fact]
    public async Task Active_effect_property_changes_are_tracked_by_notifications()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var activeEffects = persisted.PlayerEntity.ActiveEffects!;
        var propertyEffect = new ActivePropertyEffect(0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.AcidResistance))
        {
            ExpirationTarget = 10,
            ExpirationEvent = EffectDuration.UntilTimeout,
            MagnitudePrimary = 1
        };
        var componentEffect = activeEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, targetName: null,
            conditionValue: (int)AbilityType.Race, level: 1,
            expirationTarget: 10, expirationEvent: EffectDuration.UntilTimeout);
        var accumulator = new EffectAccumulator
        {
            EffectType = EffectType.Doom,
            Value = 1
        };
        propertyEffect = activeEffects.AddPropertyEffect(propertyEffect);
        activeEffects.Accumulators.Add(accumulator);

        Assert.Same(activeEffects, propertyEffect.ContainingComponent);
        Assert.Same(activeEffects, componentEffect.ContainingComponent);
        Assert.Same(activeEffects, accumulator.ContainingComponent);
        Assert.Equal(activeEffects.GameId, propertyEffect.GameId);
        Assert.Equal(activeEffects.EntityId, propertyEffect.EntityId);
        Assert.Equal(activeEffects.GameId, componentEffect.GameId);
        Assert.Equal(activeEffects.EntityId, componentEffect.EntityId);
        persisted.Context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, persisted.Context.Entry(propertyEffect).State);
        Assert.Equal(EntityState.Unchanged, persisted.Context.Entry(componentEffect).State);
        Assert.Equal(EntityState.Unchanged, persisted.Context.Entry(accumulator).State);

        propertyEffect.MagnitudePrimary = 2;
        propertyEffect.ExpirationTarget = 12;
        propertyEffect.ExpirationEvent = EffectDuration.UntilXPGained;
        componentEffect.Level = 3;
        componentEffect.ConditionValue = (int)AbilityType.Status;
        componentEffect.ExpirationTarget = 11;
        componentEffect.ExpirationEvent = EffectDuration.UntilXPGained;
        accumulator.Value = 4;

        Assert.Equal(EntityState.Modified, persisted.Context.Entry(propertyEffect).State);
        Assert.Equal(EntityState.Modified, persisted.Context.Entry(componentEffect).State);
        Assert.Equal(EntityState.Modified, persisted.Context.Entry(accumulator).State);

        var propertyEffectId = propertyEffect.InstanceId;
        var componentEffectId = componentEffect.InstanceId;
        persisted.Context.SaveChanges();
        await persisted.ReloadAsync();

        var reloadedActiveEffects = persisted.PlayerEntity.ActiveEffects!;
        var reloadedPropertyEffect = reloadedActiveEffects.PropertyEffects.Single(
            effect => effect.InstanceId == propertyEffectId);
        var reloadedComponentEffect = reloadedActiveEffects.ComponentEffects.Single(
            effect => effect.InstanceId == componentEffectId);
        var reloadedAccumulator = reloadedActiveEffects.Accumulators[EffectType.Doom];
        Assert.Equal(2, reloadedPropertyEffect.MagnitudePrimary);
        Assert.Equal(nameof(BeingComponent.AcidResistance), reloadedPropertyEffect.TargetName);
        Assert.Equal(12, reloadedPropertyEffect.ExpirationTarget);
        Assert.Equal(EffectDuration.UntilXPGained, reloadedPropertyEffect.ExpirationEvent);
        Assert.Equal(3, reloadedComponentEffect.Level);
        Assert.Null(reloadedComponentEffect.TargetName);
        Assert.Equal((int)AbilityType.Status, reloadedComponentEffect.ConditionValue);
        Assert.Equal(11, reloadedComponentEffect.ExpirationTarget);
        Assert.Equal(EffectDuration.UntilXPGained, reloadedComponentEffect.ExpirationEvent);
        Assert.Equal(4, reloadedAccumulator.Value);
        Assert.Same(reloadedActiveEffects, reloadedPropertyEffect.ContainingComponent);
        Assert.Same(reloadedActiveEffects, reloadedComponentEffect.ContainingComponent);
        Assert.Same(reloadedActiveEffects, reloadedAccumulator.ContainingComponent);
        Assert.Equal(reloadedActiveEffects.GameId, reloadedPropertyEffect.GameId);
        Assert.Equal(reloadedActiveEffects.EntityId, reloadedPropertyEffect.EntityId);
        Assert.Equal(reloadedActiveEffects.GameId, reloadedComponentEffect.GameId);
        Assert.Equal(reloadedActiveEffects.EntityId, reloadedComponentEffect.EntityId);
    }

    [Fact]
    public async Task Active_effect_instance_ids_remain_unique_across_save_and_load_after_removal()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var activeEffects = persisted.PlayerEntity.ActiveEffects!;

        var propertyEffects = Enumerable.Range(1, 3)
            .Select(marker => activeEffects.AddPropertyEffect(
                EffectType.ChangeProperty, magnitudeSecondary: 1000 + marker))
            .ToArray();

        var componentEffects = Enumerable.Range(1, 3)
            .Select(marker => activeEffects.AddComponentEffect(
                EffectType.AddAbility, $"id test {marker}"))
            .ToArray();

        activeEffects.PropertyEffects.Remove(propertyEffects[2]);
        activeEffects.ComponentEffects.Remove(componentEffects[2]);

        var replacementProperty = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, magnitudeSecondary: 2000);
        var replacementComponent = activeEffects.AddComponentEffect(
            EffectType.AddAbility, "id test replacement");

        Assert.True(replacementProperty.InstanceId > propertyEffects[2].InstanceId);
        Assert.True(replacementComponent.InstanceId > componentEffects[2].InstanceId);
        var replacementPropertyId = replacementProperty.InstanceId;
        var replacementComponentId = replacementComponent.InstanceId;
        persisted.Context.SaveChanges();
        await persisted.ReloadAsync();

        var reloaded = persisted.PlayerEntity.ActiveEffects!;
        Assert.Equal(
            reloaded.PropertyEffects.Count,
            reloaded.PropertyEffects.Select(effect => effect.InstanceId).Distinct().Count());
        Assert.Equal(
            reloaded.ComponentEffects.Count,
            reloaded.ComponentEffects.Select(effect => effect.InstanceId).Distinct().Count());
        Assert.Contains(reloaded.PropertyEffects,
            effect => effect.MagnitudeSecondary == 2000
                      && effect.InstanceId == replacementPropertyId);
        Assert.Contains(reloaded.ComponentEffects,
            effect => effect.TargetName == "id test replacement"
                      && effect.InstanceId == replacementComponentId);

        var reloadedProperty = reloaded.PropertyEffects.Single(
            effect => effect.InstanceId == replacementPropertyId);
        var reloadedComponent = reloaded.ComponentEffects.Single(
            effect => effect.InstanceId == replacementComponentId);
        reloaded.PropertyEffects.Remove(reloadedProperty);
        reloaded.ComponentEffects.Remove(reloadedComponent);

        var postReloadProperty = reloaded.AddPropertyEffect(
            EffectType.ChangeProperty, magnitudeSecondary: 3000);
        var postReloadComponent = reloaded.AddComponentEffect(
            EffectType.AddAbility, "id test post reload");

        Assert.True(postReloadProperty.InstanceId > replacementPropertyId);
        Assert.True(postReloadComponent.InstanceId > replacementComponentId);
        persisted.Context.SaveChanges();
    }

    [Fact]
    public void Player_active_effects_and_resolved_stats_survive_save_and_load()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        EffectTestHelper.ApplyStatus(
            player, EffectType.Decay, duration: 1000, manager, amount: 5);
        EffectTestHelper.ApplyStatus(
            player, EffectType.Enervate, duration: 1000, manager, amount: 7);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        var effects = player.ActiveEffects!;

        var propertyEffectCount = effects.PropertyEffects.Count;
        var propertyEffectIds = effects.PropertyEffects.Select(effect => effect.InstanceId).ToArray();
        var might = player.Being!.Might;
        var maxHp = player.Being.HitPointMaximum;
        var evasion = player.Being.Evasion;
        var capacity = player.Physical!.Capacity;
        var reservedHp = player.Being.ReservedHitPoints;
        var reservedEp = player.Being.ReservedEnergyPoints;

        Assert.True(propertyEffectCount > 0, "Expected the seeded player to carry innate property effects.");
        Assert.Contains(effects.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.Decay,
                TargetName: null
            });
        Assert.Contains(effects.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.Enervate,
                TargetName: null
            });
        Assert.Contains(effects.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.ChangeProperty,
                TargetName: nameof(BeingComponent.ReservedHitPoints)
            });
        Assert.Contains(effects.PropertyEffects,
            effect => effect is
            {
                EffectType: EffectType.ChangeProperty,
                TargetName: nameof(BeingComponent.ReservedEnergyPoints)
            });

        persisted.Reload();

        var reloadedPlayer = persisted.PlayerEntity;
        var reloadedEffects = reloadedPlayer.ActiveEffects!;

        Assert.Equal(propertyEffectCount, reloadedEffects.PropertyEffects.Count);
        Assert.Equal(propertyEffectIds, reloadedEffects.PropertyEffects.Select(effect => effect.InstanceId));
        Assert.All(
            reloadedEffects.PropertyEffects,
            effect => Assert.Same(Effect.GetMetadata(effect.EffectType), effect.Metadata));
        Assert.Equal(might, reloadedPlayer.Being!.Might);
        Assert.Equal(maxHp, reloadedPlayer.Being.HitPointMaximum);
        Assert.Equal(evasion, reloadedPlayer.Being.Evasion);
        Assert.Equal(reservedHp, reloadedPlayer.Being.ReservedHitPoints);
        Assert.Equal(reservedEp, reloadedPlayer.Being.ReservedEnergyPoints);
        Assert.Equal(capacity, reloadedPlayer.Physical!.Capacity);
        var race = Assert.Single(reloadedPlayer.Being.Races);
        Assert.Equal(AbilityType.Race, race.Ability!.Type);
        Assert.Null(race.Being);
        Assert.Null(race.ActiveEffects);
    }

    [Fact]
    public void Dynamic_magnitudes_continue_tracking_inputs_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        persisted.Reload();

        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        var being = player.Being!;
        var oldMight = being.Might;
        var oldMaximum = being.HitPointMaximum;

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            5, nameof(BeingComponent.Might), "post-reload might", player);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(oldMight + 5, being.Might);
        Assert.Equal(oldMaximum + 50, being.HitPointMaximum);
    }

    [Fact]
    public void Polymorph_survives_reload_and_reverts_on_expiry()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        var being = player.Being!;
        var ownSpecies = being.Species;
        var ownMight = being.Might;
        const int duration = 100;

        manager.EffectApplicationSystem.ApplyPolymorph(
            CreatureData.Gnome, player, duration, manager);
        manager.Queue.ProcessQueue(manager);
        var formSpecies = being.Species;
        Assert.NotEqual(ownSpecies, formSpecies);

        persisted.Context.SaveChanges();
        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = persisted.Game.Manager;
        being = player.Being!;
        Assert.Equal(formSpecies, being.Species);

        manager.Game.CurrentTick += duration;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(ownSpecies, being.Species);
        Assert.Equal(ownMight, being.Might);
        Assert.DoesNotContain(
            player.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.Polymorph);
    }

    [Theory]
    [InlineData(EffectType.Frighten, "frightened")]
    [InlineData(EffectType.Taunt, "taunted")]
    public void Per_source_status_removed_with_its_source_remains_absent_after_reload(
        EffectType effectType, string abilityName)
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;

        GameEntity sourceAbility;
        using (var sourceReference = manager.CreateEntity())
        {
            sourceAbility = sourceReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = $"persistence {effectType} source";
            ability.OwnerEntity = player;
            sourceAbility.Ability = ability;
        }

        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(
                effectType, player, 6 * EffectApplicationSystem.BuildUpThreshold, sourceAbility.Id));
        manager.EffectApplicationSystem.ApplyStatus(
            EffectApplicationContext.CreateStatus(
                effectType, player, 2 * EffectApplicationSystem.BuildUpThreshold, sourceAbility.Id));
        manager.Queue.ProcessQueue(manager);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(player, abilityName) > 0);

        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            player.ActiveEffects!, player,
            instance => instance.SourceAbilityEntityId == sourceAbility.Id, manager);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(player, abilityName));
        Assert.DoesNotContain(
            player.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == abilityName);
        Assert.Equal(0, player.ActiveEffects.Accumulators[effectType].Value);

        persisted.Context.SaveChanges();
        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = persisted.Game.Manager;
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(player, abilityName));
        Assert.DoesNotContain(
            player.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == abilityName);
        Assert.Equal(0, player.ActiveEffects!.Accumulators[effectType].Value);
    }

    [Fact]
    public void Stun_condition_reconstructs_from_the_persisted_buildup_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;

        // Repeated stun applications accumulate past the build-up threshold, putting the condition in
        // force. The build-up accumulator persists; the in-force stun condition is reconstructed from the
        // persisted state on load.
        for (var i = 0; i < 3; i++)
        {
            ApplyStun(manager, player);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.True(
            EffectApplicationSystem.GetAbilityLevel(player, "stunned") > 0,
            "Expected the accumulated stun to be in force before saving.");
        persisted.Context.SaveChanges();

        persisted.Reload();

        var reloadedPlayer = persisted.PlayerEntity;
        Assert.True(
            EffectApplicationSystem.GetAbilityLevel(reloadedPlayer, "stunned") > 0,
            "The stun condition should reconstruct from the persisted build-up accumulator after a reload.");
    }

    [Fact]
    public void An_equip_toggle_modifier_survives_save_and_load_as_a_source_linked_instance()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;
        var baseAcid = player.Being!.AcidResistance;

        // A toggled ability (an equipped item's WhileEquipped / a toggled-on skillbook) contributing a
        // source-linked Infinite ChangeProperty, stored as an ActivePropertyEffect instance (retirement 8).
        GameEntity toggledAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            toggledAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test aegis";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = player;
            toggledAbility.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.AcidResistance);
            effect.Amount = "15";
            effect.SecondaryAmount = "42";
            effect.ContainingAbilityId = toggledAbility.Id;
            effectReference.Referenced.Effect = effect;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);

        var activateMessage = ActivateAbilityMessage.Create(manager);
        activateMessage.ActivatorEntity = player;
        activateMessage.TargetEntity = player;
        activateMessage.AbilityEntity = toggledAbility;
        manager.Enqueue(activateMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseAcid + 15, player.Being.AcidResistance);
        Assert.Contains(player.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.ChangeProperty
                 && i.TargetName == nameof(BeingComponent.AcidResistance)
                 && i.SourceAbilityEntityId == toggledAbility.Id);

        persisted.Context.SaveChanges();
        persisted.Reload();

        // The instance persists (OwnsMany) and its definition is rebuilt on load, so the resolved
        // resistance the toggled ability granted is intact without re-running the apply path.
        var reloadedPlayer = persisted.PlayerEntity;
        Assert.Equal(baseAcid + 15, reloadedPlayer.Being!.AcidResistance);
        Assert.Contains(reloadedPlayer.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.ChangeProperty
                 && i.TargetName == nameof(BeingComponent.AcidResistance));
        var reloadedEffect = reloadedPlayer.ActiveEffects.Abilities
            .Single(entity => entity.Ability!.Name == "test aegis")
            .Ability!.Effects.Single().Effect!;
        Assert.Equal("42", reloadedEffect.SecondaryAmount);
    }

    [Fact]
    public void An_engulfed_player_remains_positioned_across_save_and_load()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;
        var room = player.Position!.RoomEntity.Room!;
        var playerCell = player.Position!.Cell!.Value;

        var engulfer = CreatureData.GelatinousCube.Instantiate(
            room, FindAdjacentWalkableCell(room, playerCell, manager));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(player.Position!.Cell);
        Assert.Equal(1, player.Position.Cell.Value.DistanceTo(engulfer.Position!.Cell!.Value));
        Assert.True(EffectApplicationSystem.GetAbilityLevel(player, "engulfed") > 0);
        var engulferId = engulfer.Id;
        var engulferCell = engulfer.Position!.Cell;
        var playerCellBeforeReload = player.Position.Cell;

        persisted.Context.SaveChanges();
        persisted.Reload();

        var reloadedManager = persisted.Game.Manager;
        var reloadedPlayer = reloadedManager.FindEntity(persisted.PlayerEntityId);
        Assert.NotNull(reloadedPlayer);
        Assert.Equal(playerCellBeforeReload, reloadedPlayer!.Position!.Cell);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(reloadedPlayer, "engulfed") > 0);

        var reloadedEngulfer = reloadedManager.EngulferToVictimRelationship.GetEngulfer(reloadedPlayer);
        Assert.NotNull(reloadedEngulfer);
        Assert.Equal(engulferId, reloadedEngulfer!.Id);
        Assert.Equal(engulferCell, reloadedEngulfer.Position!.Cell);
    }

    private static Point FindAdjacentWalkableCell(RoomComponent room, Point excluded, GameManager manager)
        => manager.TravelSystem.FindCell(
               room, manager.TravelSystem.IsActorFreeCell, origin: excluded, maxRadius: 1, excluded: excluded)
           ?? throw new InvalidOperationException("No walkable cell found in the player's room.");

    private static void ApplyStun(GameManager manager, GameEntity target)
    {
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stun, target, amount: 100));
    }
}

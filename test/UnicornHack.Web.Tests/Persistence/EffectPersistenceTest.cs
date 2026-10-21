using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UnicornHack.Data;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Generation.Map;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.Caching;
using UnicornHack.Utils.MessagingECS;
using Xunit;

namespace UnicornHack.Persistence;

public class EffectPersistenceTest
{
    [Fact]
    public void Non_decreasing_boss_phase_survives_healing_and_reload_but_can_be_removed()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        var room = player.Position!.RoomEntity.Room!;
        var cell = manager.TravelSystem.FindCell(room, manager.TravelSystem.IsActorFreeCell)!.Value;
        var boss = CreatureData.BalorUaNeit.Instantiate(room, cell);
        manager.Queue.ProcessQueue(manager);
        var bossId = boss.Id;
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(boss, "Balor's eye cycle"));
        boss.Being!.HitPoints = boss.Being.HitPointMaximum / 2;
        manager.Queue.ProcessQueue(manager);
        boss.Being.HitPoints = boss.Being.HitPointMaximum;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(boss, "Balor's eye cycle"));
        persisted.Context.SaveChanges();

        persisted.Reload();

        manager = persisted.Game.Manager;
        boss = manager.FindEntity(bossId)!;
        Assert.Equal(boss.Being!.HitPointMaximum, boss.Being.HitPoints);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(boss, "Balor's eye cycle"));
        Assert.NotNull(manager.AffectableAbilitiesIndex[(bossId, "lifted eye")]);
        Assert.Null(manager.AffectableAbilitiesIndex[(bossId, "king's blow")]);

        var record = Assert.Single(boss.ActiveEffects!.ComponentEffects.GetValues("Balor's eye cycle"));
        Assert.NotEqual(0, record.SourceAbilityEntityId);
        AbilityLevelSystem.DetachComponentCarrier(record, boss, manager);
        manager.Queue.ProcessQueue(manager);
        AbilityLevelSystem.RebuildDynamicAbilities(boss, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(boss, "Balor's eye cycle"));
        Assert.Null(manager.AffectableAbilitiesIndex[(bossId, "lifted eye")]);
        Assert.Empty(boss.ActiveEffects.ComponentEffects.GetValues("Balor's eye cycle"));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    public void Ankou_rebuilds_leveled_grants_after_deaths_while_its_room_is_unloaded(int deathCount)
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var manager = persisted.Game.Manager;
        var playerRoom = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        var remote = LevelLayout.CreateEmptyRoom(playerRoom.Level, manager);
        LevelLayout.AllocateTerrain(remote, 3, 1, MapFeature.StoneFloor);
        var ankou = CreatureData.Ankou.Instantiate(remote, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var actorId = ankou.Id;
        var roomId = remote.EntityId;
        var playerRoomId = playerRoom.EntityId;
        var initialArmor = ankou.Being!.Armor;
        var attack = manager.AffectableAbilitiesIndex[(actorId, "reverse scythe")]!.Ability!;
        var initialDamage = manager.EffectApplicationSystem.GetAmount(Assert.Single(attack.Effects).Effect!, ankou, ankou);
        persisted.Context.SaveChanges();

        persisted.ReloadRooms([playerRoomId]);

        manager = persisted.Game.Manager;
        playerRoom = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        Assert.Null(manager.FindEntity(actorId));
        for (var deaths = 0; deaths < deathCount; deaths++)
        {
            var cell = manager.TravelSystem.FindCell(playerRoom, manager.TravelSystem.IsActorFreeCell)!.Value;
            var victim = CreatureData.Boggart.Instantiate(playerRoom, cell);
            manager.Queue.ProcessQueue(manager);
            victim.Being!.HitPoints = 0;
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Null(manager.FindEntity(actorId));
        Assert.Equal(deathCount, playerRoom.Level.Stats.DeathCount);
        persisted.Context.SaveChanges();
        persisted.ReloadRooms([playerRoomId]);

        manager = persisted.Game.Manager;
        persisted.Context.LoadRooms([roomId]);
        manager.Queue.ProcessQueue(manager);
        ankou = manager.FindEntity(actorId)!;
        attack = manager.AffectableAbilitiesIndex[(actorId, deathCount < 4 ? "reverse scythe" : "extended reverse scythe")]!.Ability!;
        Assert.Equal(deathCount < 4 ? 1 : 2, attack.Range);
        Assert.Equal(initialArmor + (deathCount >= 6 ? 2 : 0), ankou.Being!.Armor);
        Assert.Equal(initialDamage + 2 * Math.Min(3, deathCount / 2),
            manager.EffectApplicationSystem.GetAmount(Assert.Single(attack.Effects).Effect!, ankou, ankou));
        Assert.Equal(1 + Math.Min(3, deathCount / 2), AbilityLevelSystem.GetAbilityLevel(ankou, "Ankou trophies"));
        Assert.Equal(deathCount >= 6, manager.AffectableAbilitiesIndex[(actorId, "trophy armor")] != null);
    }

    [Fact]
    public void Level_death_expressions_read_saved_totals_after_reload_without_observer_triggers()
    {
        Assert.SkipUnless(PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");
        using var persisted = PersistedGame.CreateAndSave();
        var manager = persisted.Game.Manager;
        var room = persisted.PlayerEntity.Position!.RoomEntity.Room!;
        var initialDeaths = room.Level.Stats.DeathCount;
        var initialGameDeaths = persisted.Game.Stats.DeathCount;
        var cell = manager.TravelSystem.FindCell(room, manager.TravelSystem.IsActorFreeCell)!.Value;
        var victim = CreatureData.Boggart.Instantiate(room, cell);
        manager.Queue.ProcessQueue(manager);
        victim.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialDeaths + 1, room.Level.Stats.DeathCount);
        Assert.Equal(initialGameDeaths + 1, persisted.Game.Stats.DeathCount);
        persisted.Context.SaveChanges();

        persisted.Reload();

        var player = persisted.PlayerEntity;
        room = player.Position!.RoomEntity.Room!;
        manager = persisted.Game.Manager;
        var expression = EffectApplicationSystem.CreateLevelExpression("$levelDeaths", "test saved deaths").Function;
        Assert.Equal(initialDeaths + 1, room.Level.Stats.DeathCount);
        Assert.Equal(initialGameDeaths + 1, persisted.Game.Stats.DeathCount);
        Assert.Equal(initialDeaths + 1, expression(player));
        cell = manager.TravelSystem.FindCell(room, manager.TravelSystem.IsActorFreeCell)!.Value;
        victim = CreatureData.Boggart.Instantiate(room, cell);
        manager.Queue.ProcessQueue(manager);
        victim.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(initialDeaths + 2, expression(player));
        persisted.Context.SaveChanges();

        persisted.Reload();

        Assert.Equal(initialDeaths + 2, expression(persisted.PlayerEntity));
        Assert.Equal(initialGameDeaths + 2, persisted.Game.Stats.DeathCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Duplicate_item_abilities_preserve_identity_and_slots_after_reload(bool useAsync)
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var playerEntity = persisted.PlayerEntity;
        var manager = persisted.Game.Manager;
        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var expected = playerEntity.Being!.Items
            .Where(item => item.Item!.TemplateName == ItemData.LongSword.Name)
            .Select(item =>
            {
                var abilityEntity = item.ActiveEffects!.Abilities.Single(
                    ability => ability.Ability!.Name == "Equip");
                return (ItemId: item.Id, AbilityId: abilityEntity.Id, Slot: abilityEntity.Ability!.Slot!.Value);
            })
            .OrderBy(value => value.ItemId)
            .ToList();
        Assert.Equal(2, expected.Count);
        Assert.Equal(2, expected.Select(value => value.Slot).Distinct().Count());

        var coolingAbility = manager.FindEntity(expected[0].AbilityId)!.Ability!;
        var expectedCooldownTick = manager.Game.CurrentTick + 100;
        coolingAbility.CooldownTick = expectedCooldownTick;
        persisted.Context.SaveChanges();

        if (useAsync)
        {
            await persisted.ReloadAsync();
        }
        else
        {
            persisted.Reload();
        }

        var reloadedPlayer = persisted.PlayerEntity;
        var reloadedManager = persisted.Game.Manager;
        foreach (var value in expected)
        {
            var itemEntity = reloadedManager.FindEntity(value.ItemId)!;
            var abilityEntity = reloadedManager.FindEntity(value.AbilityId)!;
            Assert.Same(itemEntity, abilityEntity.Ability!.OwnerEntity);
            Assert.Equal(value.Slot, abilityEntity.Ability.Slot);
            Assert.Same(abilityEntity, reloadedPlayer.Being!.SlottedAbilities[value.Slot]);
        }

        Assert.Equal(
            expectedCooldownTick,
            reloadedManager.FindEntity(expected[0].AbilityId)!.Ability!.CooldownTick);
    }

    [Fact]
    public async Task Loading_a_game_ignores_entities_with_the_same_ids_from_other_games()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var firstGameId = persisted.Game.Id;
        var secondGame = new Game
        {
            Options = { InitialSeed = 2 },
            Random = new SimpleRandom { Seed = 2 },
            Services = persisted.Game.Services,
            Repository = persisted.Context
        };
        persisted.Context.Games.Add(secondGame);
        persisted.Context.SaveChanges();

        var secondManager = new GameManager { Game = secondGame };
        secondManager.Initialize(new SequentialMessageQueue<GameEntity>());
        secondGame.Manager = secondManager;
        var secondRoom = TestHelper.BuildRoom("...", secondGame);
        PlayerRace.InstantiatePlayer(
            "Other", Sex.Female, secondRoom, new Point(1, 0));
        secondManager.Queue.ProcessQueue(secondManager);
        persisted.Context.SaveChanges();

        var firstGameEntityIds = persisted.Context.ChangeTracker.Entries<GameEntity>()
            .Where(entry => entry.Entity.GameId == firstGameId)
            .Select(entry => entry.Entity.Id)
            .ToHashSet();
        var secondGameEntityIds = persisted.Context.ChangeTracker.Entries<GameEntity>()
            .Where(entry => entry.Entity.GameId == secondGame.Id)
            .Select(entry => entry.Entity.Id)
            .ToHashSet();
        Assert.NotEmpty(firstGameEntityIds.Intersect(secondGameEntityIds));

        await persisted.ReloadAsync();

        Assert.Equal(firstGameId, persisted.Game.Id);
        Assert.All(
            persisted.Context.ChangeTracker.Entries<GameEntity>(),
            entry => Assert.Equal(firstGameId, entry.Entity.GameId));
    }

    [Fact]
    public void Queried_poolable_objects_are_assigned_to_object_pools()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var activeEffects = persisted.PlayerEntity.ActiveEffects!;
        activeEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, targetName: null,
            conditionValue: (int)AbilityType.Debuff);
        activeEffects.AddAccumulator(EffectType.Doom);
        persisted.Context.SaveChanges();

        persisted.Reload();

        var poolableObjects = persisted.Context.ChangeTracker.Entries()
            .Select(entry => entry.Entity)
            .OfType<IPoolable>()
            .ToList();
        Assert.Contains(poolableObjects, instance => instance is GameEntity);
        Assert.Contains(poolableObjects, instance => instance is Component);
        Assert.Contains(poolableObjects, instance => instance is ActivePropertyEffect);
        Assert.Contains(poolableObjects, instance => instance is ActiveComponentEffect);
        Assert.Contains(poolableObjects, instance => instance is EffectAccumulator);
        Assert.All(poolableObjects, instance => Assert.NotNull(GetObjectPool(instance)));

        static object? GetObjectPool(IPoolable instance)
        {
            var declaringType = instance switch
            {
                Entity => typeof(Entity),
                Component => typeof(Component),
                PooledTrackableCollectionItem => typeof(PooledTrackableCollectionItem),
                _ => throw new InvalidOperationException(instance.GetType().Name)
            };
            return declaringType.GetField("_pool", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(instance);
        }
    }

    [Fact]
    public void Querying_a_poolable_type_requires_a_wired_game_manager()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var context = persisted.Context;
        var manager = context.Manager;
        context.Manager = null!;
        try
        {
            var exception = Assert.Throws<InvalidOperationException>(
                () => context.Entities.AsNoTracking().First());
            Assert.Contains("must be assigned", exception.Message);
        }
        finally
        {
            context.Manager = manager;
        }
    }

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
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.Acrobatics);
        manager.Queue.ProcessQueue(manager);
        var ability = new Ability
        {
            Name = "test timed ability decrease",
            Type = AbilityType.Debuff,
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
            Type = AbilityType.Debuff,
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
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
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
    public void Expression_grants_remain_authoritative_and_reject_manual_level_changes_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        var player = persisted.PlayerEntity;
        var manager = player.Manager;
        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        Assert.Throws<InvalidOperationException>(() => manager.AbilityLevelSystem.AddAbilityEffect(
            player, AbilityData.IceShard, sourceAbilityEntityId: 1000));
        Assert.Equal(1, manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.IceShard.Name)]!.Ability!.Level);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        var records = player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.IceShard.Name);
        Assert.Single(records);
        Assert.Equal(1, manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.IceShard.Name)]!.Ability!.Level);

        Assert.Throws<InvalidOperationException>(() => manager.AbilityLevelSystem.AddAbilityEffect(
            player, AbilityData.IceShard, sourceAbilityEntityId: 1000));
        Assert.Throws<InvalidOperationException>(() => TestHelper.ApplyAbilityLevelDecrease(
            player, abilityName: AbilityData.IceShard.Name));
        Assert.DoesNotContain(records, record => record.EffectType == EffectType.DecreaseAbilityLevel);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.IceShard.Name));

        manager.AbilityLevelSystem.AddAbilityEffect(player, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(AbilityData.IceShard.MaxLevel, manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.IceShard.Name)]!.Ability!.Level);
        Assert.Equal(AbilityData.IceShard.MaxLevel, Assert.Single(records).Level);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        Assert.Equal(AbilityData.IceShard.MaxLevel, AbilityLevelSystem.GetAbilityLevel(player, AbilityData.IceShard.Name));
        Assert.Equal(AbilityData.IceShard.MaxLevel, Assert.Single(
            player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.IceShard.Name)).Level);
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
        manager.AbilityLevelSystem.RelevelAbility(AbilityData.Stunned.Name, player, manager);
        manager.Queue.ProcessQueue(manager);
        persisted.Context.SaveChanges();

        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = player.Manager;
        manager.AbilityLevelSystem.RelevelAbility(AbilityData.Stunned.Name, player, manager);
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
        manager.AbilityLevelSystem.AddAbilityEffect(
            player, race.GetRaceAbilityTemplate());
        manager.AbilityLevelSystem.AddAbilityEffect(
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
    public void Fresh_test_database_contains_the_current_feature_and_reward_schema()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();
        Assert.NotNull(persisted.PlayerEntity);
        Assert.Empty(persisted.Context.FeatureComponents.Include(feature => feature.Rewards).ToList());
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
            var tick = TimeSystem.GetTick(entity);
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
        var propertyEffect = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, nameof(BeingComponent.AcidResistance),
            magnitudePrimary: 1,
            expirationTarget: 10,
            expirationEvent: EffectDuration.UntilTimeout);
        var componentEffect = activeEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, targetName: null,
            conditionValue: (int)AbilityType.Race, level: 1,
            expirationTarget: 10, expirationEvent: EffectDuration.UntilTimeout);
        var accumulator = activeEffects.AddAccumulator(EffectType.Doom);
        accumulator.Value = 1;

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
        componentEffect.ConditionValue = (int)AbilityType.Debuff;
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
        Assert.Equal((int)AbilityType.Debuff, reloadedComponentEffect.ConditionValue);
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

        var removedPropertyId = propertyEffects[2].InstanceId;
        var removedComponentId = componentEffects[2].InstanceId;
        activeEffects.PropertyEffects.Remove(propertyEffects[2]);
        activeEffects.ComponentEffects.Remove(componentEffects[2]);

        var replacementProperty = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, magnitudeSecondary: 2000);
        var replacementComponent = activeEffects.AddComponentEffect(
            EffectType.AddAbility, "id test replacement");

        Assert.True(replacementProperty.InstanceId > removedPropertyId);
        Assert.True(replacementComponent.InstanceId > removedComponentId);
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
            CreatureData.Rat, player, duration, manager);
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
        Assert.True(AbilityLevelSystem.GetAbilityLevel(player, abilityName) > 0);

        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            player.ActiveEffects!, player,
            instance => instance.SourceAbilityEntityId == sourceAbility.Id, manager);
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, abilityName));
        Assert.DoesNotContain(
            player.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == abilityName);
        Assert.Equal(0, player.ActiveEffects.Accumulators[effectType].Value);

        persisted.Context.SaveChanges();
        persisted.Reload();

        player = persisted.PlayerEntity;
        manager = persisted.Game.Manager;
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(player, abilityName));
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
            AbilityLevelSystem.GetAbilityLevel(player, "stunned") > 0,
            "Expected the accumulated stun to be in force before saving.");
        persisted.Context.SaveChanges();

        persisted.Reload();

        var reloadedPlayer = persisted.PlayerEntity;
        Assert.True(
            AbilityLevelSystem.GetAbilityLevel(reloadedPlayer, "stunned") > 0,
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

        var setSlotMessage = SetAbilitySlotMessage.Create(player, manager);
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

        var engulfer = CreatureData.Sylph.Instantiate(
            room, FindAdjacentWalkableCell(room, playerCell, manager));
        manager.Queue.ProcessQueue(manager);

        var engulfEffect = engulfer.ActiveEffects!.Abilities
            .SelectMany(a => a.Ability!.Effects)
            .Single(e => e.Effect!.EffectType == EffectType.Engulf);
        TestHelper.ApplyEffect(engulfEffect, engulfer, player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(player.Position!.Cell);
        Assert.Equal(1, player.Position.Cell.Value.DistanceTo(engulfer.Position!.Cell!.Value));
        Assert.True(AbilityLevelSystem.GetAbilityLevel(player, "engulfed") > 0);
        var engulferId = engulfer.Id;
        var engulferCell = engulfer.Position!.Cell;
        var playerCellBeforeReload = player.Position.Cell;

        persisted.Context.SaveChanges();
        persisted.Reload();

        var reloadedManager = persisted.Game.Manager;
        var reloadedPlayer = reloadedManager.FindEntity(persisted.PlayerEntityId);
        Assert.NotNull(reloadedPlayer);
        Assert.Equal(playerCellBeforeReload, reloadedPlayer!.Position!.Cell);
        Assert.True(AbilityLevelSystem.GetAbilityLevel(reloadedPlayer, "engulfed") > 0);

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

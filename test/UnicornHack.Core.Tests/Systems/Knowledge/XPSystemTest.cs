using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Knowledge;

public class XPSystemTest
{
    [Fact]
    public void Xp_gained_on_exploration()
    {
        var level = TestHelper.BuildRoom(@"
...
>..
..#");
        level.Level.Difficulty = 3;

        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        player.Position!.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(9, player.Being!.ExperiencePoints);

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = player;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = new Point(0, 0);
        manager.Enqueue(travelMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(27, player.Being.ExperiencePoints);
    }

    [Fact]
    public void Xp_gained_on_enemy_death()
    {
        var level = TestHelper.BuildRoom("..");
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        player.Position!.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, player.Being!.ExperiencePoints);

        undine.Being!.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(182, player.Being.ExperiencePoints);
    }

    [Fact]
    public void Race_carrier_exposes_template_identity_and_innate_effects_have_no_carrier()
    {
        var level = TestHelper.BuildRoom("..");
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        // A creature's single race carrier reports the creature template through its Race ability.
        var undineRace = undine.Being!.Races.Single();
        Assert.Equal(CreatureData.Undine.InitialLevel, undineRace.Ability!.Level);
        Assert.Equal(
            CreatureData.Undine.InitialLevel,
            undine.ActiveEffects!.ComponentEffects.Single(effect => effect is
                { EffectType: EffectType.AddAbility, TargetName: "undine race" }).Level);
        Assert.Equal(AbilityType.Race, undineRace.Ability!.Type);
        Assert.Equal("undine race", undineRace.Ability.Template!.Name);
        Assert.Null(undineRace.Being);
        Assert.Null(undineRace.ActiveEffects);

        // A player's starting race is represented by the same bare Race ability carrier.
        var humanRace = player.Being!.Races.Single();
        Assert.Equal(1, humanRace.Ability!.Level);
        Assert.Equal(
            1,
            player.ActiveEffects!.ComponentEffects.Single(effect => effect is
                { EffectType: EffectType.AddAbility, TargetName: "human race" }).Level);
        Assert.Equal(AbilityType.Race, humanRace.Ability!.Type);
        Assert.Equal("human race", humanRace.Ability.Template!.Name);
        Assert.Null(humanRace.Being);
        Assert.Null(humanRace.ActiveEffects);
        Assert.Same(humanRace, manager.AffectableAbilitiesIndex[(player.Id, "human race")]);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (player.Id, EffectApplicationSystem.InnateAbilityName)]);

        var innateEffects = player.ActiveEffects.PropertyEffects
            .Where(effect => effect is
            {
                SourceAbilityEntityId: null,
                SourceAbilityType: AbilityType.Innate
            })
            .ToList();
        Assert.Equal(AbilityData.Innate.Effects!.Count, innateEffects.Count);

        var hitPointMaximumEffect = Assert.Single(innateEffects,
            effect => effect.TargetName == nameof(BeingComponent.HitPointMaximum));
        var initialMagnitude = hitPointMaximumEffect.MagnitudePrimary;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1, nameof(BeingComponent.Might), "test might", player);
        manager.Queue.ProcessQueue(manager);

        Assert.True(hitPointMaximumEffect.MagnitudePrimary > initialMagnitude);
    }

    [Fact]
    public void Player_race_applies_shared_being_properties()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        var movementDelay = player.Position!.MovementDelay;
        var turningDelay = player.Position.TurningDelay;
        var might = player.Being!.Might;
        var regeneration = player.Being.Regeneration;
        var energyRegeneration = player.Being.EnergyRegeneration;
        var hitPointMaximum = player.Being.HitPointMaximum;
        var accuracy = player.Being.Accuracy;
        var armor = player.Being.Armor;
        var physicalResistance = player.Being.PhysicalResistance;
        var fireResistance = player.Being.FireResistance;
        var bindResistance = player.Being.BindResistance;
        var race = new PlayerRace
        {
            Name = "test race",
            Material = Material.Steel,
            MovementDelay = 10,
            TurningDelay = 20,
            MightBonus = 2,
            Regeneration = 3,
            EnergyRegeneration = 8,
            BonusHitPoints = 10,
            Accuracy = 4,
            Armor = 5,
            PhysicalResistance = 6,
            FireResistance = 7,
            BindResistance = 9,
            Infravisible = true,
            Infravision = true,
            InvisibilityDetection = true
        };

        manager.EffectApplicationSystem.AddAbilityEffect(
            player, race.GetRaceAbilityTemplate());
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(Material.Steel, player.Physical!.Material);
        Assert.Equal(movementDelay + 10, player.Position.MovementDelay);
        Assert.Equal(turningDelay + 20, player.Position.TurningDelay);
        Assert.Equal(might + 2, player.Being.Might);
        Assert.Equal(regeneration + 3, player.Being.Regeneration);
        Assert.Equal(energyRegeneration + 8, player.Being.EnergyRegeneration);
        Assert.Equal(hitPointMaximum + 30, player.Being.HitPointMaximum);
        Assert.Equal(accuracy + 4, player.Being.Accuracy);
        Assert.Equal(armor + 5, player.Being.Armor);
        Assert.Equal(physicalResistance + 6, player.Being.PhysicalResistance);
        Assert.Equal(fireResistance + 7, player.Being.FireResistance);
        Assert.Equal(bindResistance + 9, player.Being.BindResistance);
        Assert.True(player.Physical.Infravisible);
        Assert.True(player.Being.Infravision);
        Assert.True(player.Being.InvisibilityDetection);
    }

    [Fact]
    public void Xp_accumulates_without_leveling_races()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var initialXp = playerEntity.Being!.ExperiencePoints;
        var raceLevels = playerEntity.Being.Races.ToDictionary(race => race.Id, race => race.Ability!.Level);

        manager.XPSystem.AddPlayerXP(5000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialXp + 5000, playerEntity.Being.ExperiencePoints);
        Assert.All(playerEntity.Being.Races, race => Assert.Equal(raceLevels[race.Id], race.Ability!.Level));
    }

    [Fact]
    public void Some_abilities_cooldown_with_xp()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        ItemData.PotionOfExperience.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        var flaskAbilityEntity =
            TestHelper.ActivateAbility(ItemData.FlaskOfHealing.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(flaskAbilityEntity.Ability!.CooldownXpLeft);
        Assert.Null(flaskAbilityEntity.Ability.CooldownTick);
        playerEntity.Being!.HitPoints = 50;

        TestHelper.ActivateAbility(flaskAbilityEntity, playerEntity, manager);
        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

        Assert.Equal(50, playerEntity.Being.HitPoints);

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager, slot: 3);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(flaskAbilityEntity.Ability.CooldownXpLeft);
        Assert.Null(flaskAbilityEntity.Ability.CooldownTick);
        Assert.Equal(50, playerEntity.Being.HitPoints);

        TestHelper.ActivateAbility(flaskAbilityEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(50, playerEntity.Being.HitPoints);
    }

    [Fact]
    public void Slowed_doubles_new_xp_cooldowns()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Slow, duration: 100, manager);

        var flaskAbilityEntity =
            TestHelper.ActivateAbility(ItemData.FlaskOfHealing.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(flaskAbilityEntity.Ability!.XPCooldown * 2, flaskAbilityEntity.Ability.CooldownXpLeft);
        Assert.Null(flaskAbilityEntity.Ability.CooldownTick);
    }

    [Fact]
    public void Some_effects_expire_with_xp()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.PotionOfExperience.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        GameEntity debuffAbilityEntity;
        using (var abilityEntityReference = manager.CreateEntity())
        {
            debuffAbilityEntity = abilityEntityReference.Referenced;

            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = playerEntity;

            debuffAbilityEntity.Ability = ability;

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.EffectType = EffectType.ChangeProperty;
                effect.TargetName = nameof(BeingComponent.Might);
                effect.Amount = "-1";
                effect.Duration = EffectDuration.UntilXPGained;
                effect.DurationAmount = "200";
                effect.ContainingAbilityId = ability.EntityId;

                effectEntityReference.Referenced.Effect = effect;
            }
        }

        TestHelper.ActivateAbility(debuffAbilityEntity, playerEntity, manager, 3);
        manager.Queue.ProcessQueue(manager);

        // The debuff is carried by a self-expiring ActivePropertyEffect budgeted in XP, not a cloned
        // applied effect entity.
        var debuff = playerEntity.ActiveEffects!.PropertyEffects
            .Single(i => i.TargetName == nameof(BeingComponent.Might)
                         && i.ExpirationEvent == EffectDuration.UntilXPGained);

        Assert.Equal(200, debuff.ExpirationTarget);
        Assert.Equal(9, playerEntity.Being!.Might);

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            playerEntity.ActiveEffects.PropertyEffects, i => i.TargetName == nameof(BeingComponent.Might));
        Assert.Equal(10, playerEntity.Being.Might);
    }
}

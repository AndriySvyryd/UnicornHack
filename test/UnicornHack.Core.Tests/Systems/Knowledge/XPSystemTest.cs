using System;
using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Knowledge;

public class XPSystemTest
{
    [Fact]
    public void Xp_gained_on_exploration()
    {
        var level = TestHelper.BuildLevel(@"
...
>..
..#");
        level.Difficulty = 3;

        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        player.Position.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(9, player.Being.ExperiencePoints);

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
        var level = TestHelper.BuildLevel("..");
        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        player.Position.Heading = Direction.West;
        var manager = player.Manager;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, player.Being.ExperiencePoints);

        undine.Being.HitPoints = 0;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(182, player.Being.ExperiencePoints);
    }

    [Fact]
    public void Races_level_up()
    {
        var level = TestHelper.BuildLevel(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.PotionOfExperience.Instantiate(playerEntity, 13);
        ItemData.PotionOfElfness.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, player.SkillPoints);
        Assert.Equal(0, player.TraitPoints);
        Assert.Equal(0, player.MutationPoints);
        Assert.Equal(1000, player.NextLevelXP);
        Assert.Equal(15, playerEntity.Being.Abilities.Count);
        Assert.Equal(2, playerEntity.Being.Items.Count);

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, manager.XPSystem.GetXPLevel(playerEntity));
        Assert.Equal(3, player.SkillPoints);
        Assert.Equal(3, player.TraitPoints);
        Assert.Equal(0, player.MutationPoints);
        Assert.Equal(2000, player.NextLevelXP);
        var experienceAbility =
            manager.AffectableAbilitiesIndex[(playerEntity.Id, ItemData.PotionOfExperience.Name + ": Drink")];
        Assert.Equal(4, experienceAbility.Ability.Slot);

        var humanEntity = playerEntity.Being.Races.Single();
        TestHelper.ActivateAbility(ItemData.PotionOfElfness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, playerEntity.Being.Items.Count);
        Assert.DoesNotContain(playerEntity.Being.Abilities,
            a => a.Ability.Name.Contains(ItemData.PotionOfElfness.Name));
        Assert.Equal(2, playerEntity.Being.Races.Count);
        var elfEntity = playerEntity.Being.Races.Single(r => r != humanEntity);
        Assert.Equal(2, humanEntity.Race.Level);
        Assert.Equal(1, elfEntity.Race.Level);
        Assert.Equal(3, manager.XPSystem.GetXPLevel(playerEntity));
        Assert.Equal(3, player.SkillPoints);
        Assert.Equal(3, player.TraitPoints);
        Assert.Equal(0, player.MutationPoints);
        Assert.Equal(3000, player.NextLevelXP);

        ItemData.PotionOfInhumanity.Instantiate(playerEntity);
        for (var i = 0; i < 4; i++)
        {
            TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(4, manager.XPSystem.GetXPLevel(playerEntity));
        Assert.Equal(17, playerEntity.Being.Abilities.Count);
        Assert.Equal(2, humanEntity.Race.Level);
        Assert.Equal(2, elfEntity.Race.Level);
        Assert.Equal(5, player.SkillPoints);
        Assert.Equal(6, player.TraitPoints);
        Assert.Equal(1, player.MutationPoints);
        Assert.Equal(4500, player.NextLevelXP);

        for (var i = 0; i < 4; i++)
        {
            TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(3, humanEntity.Race.Level);
        Assert.Equal(18, playerEntity.Being.Abilities.Count);

        TestHelper.ActivateAbility(ItemData.PotionOfInhumanity.Name + ": Drink", playerEntity, manager);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, playerEntity.Being.Races.Count);
        Assert.Equal(17, playerEntity.Being.Abilities.Count);
        Assert.Equal(2, manager.XPSystem.GetXPLevel(playerEntity));
        Assert.Equal(2, elfEntity.Race.Level);
        Assert.Equal(2, player.SkillPoints);
        Assert.Equal(3, player.TraitPoints);
        Assert.Equal(1, player.MutationPoints);
        Assert.Equal(3000, player.NextLevelXP);

        for (var i = 0; i < 4; i++)
        {
            TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(16, playerEntity.Being.Abilities.Count);
        Assert.Empty(playerEntity.Being.Items);
        Assert.Equal(3, manager.XPSystem.GetXPLevel(playerEntity));
        Assert.Equal(2000, player.NextLevelXP);
    }

    [Fact]
    public void Some_abilities_cooldown_with_xp()
    {
        var level = TestHelper.BuildLevel(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        ItemData.PotionOfExperience.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        var flaskAbilityEntity =
            TestHelper.ActivateAbility(ItemData.FlaskOfHealing.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(flaskAbilityEntity.Ability.CooldownXpLeft);
        Assert.Null(flaskAbilityEntity.Ability.CooldownTick);
        playerEntity.Being.HitPoints = 50;

        TestHelper.ActivateAbility(flaskAbilityEntity, playerEntity, manager);
        Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

        Assert.Equal(50, playerEntity.Being.HitPoints);

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager, slot: 3);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(flaskAbilityEntity.Ability.CooldownXpLeft);
        Assert.Null(flaskAbilityEntity.Ability.CooldownTick);
        Assert.NotEqual(50, playerEntity.Being.HitPoints);
        playerEntity.Being.HitPoints = 50;

        TestHelper.ActivateAbility(flaskAbilityEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(50, playerEntity.Being.HitPoints);
    }

    [Fact]
    public void Some_effects_expire_with_xp()
    {
        var level = TestHelper.BuildLevel(".");
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
                effect.AppliedAmount = -1;
                effect.Duration = EffectDuration.UntilXPGained;
                effect.DurationAmount = "200";
                effect.ContainingAbilityId = ability.EntityId;

                effectEntityReference.Referenced.Effect = effect;
            }
        }

        TestHelper.ActivateAbility(debuffAbilityEntity, playerEntity, manager, 3);
        manager.Queue.ProcessQueue(manager);

        var appliedEffect = manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(debuffAbilityEntity)
            .Single();

        Assert.Equal(200, appliedEffect.Effect.ExpirationXp);
        Assert.Equal(9, playerEntity.Being.Might);

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, appliedEffect.Id);
        Assert.Equal(10, playerEntity.Being.Might);
    }
}

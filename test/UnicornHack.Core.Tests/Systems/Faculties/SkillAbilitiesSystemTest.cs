﻿using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;

namespace UnicornHack.Systems.Faculties;

public class SkillAbilitiesSystemTest
{
    [Fact]
    public void Skills_add_abilities()
    {
        var level = TestHelper.BuildLevel(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]);

        var firstWaterBookAbility =
            manager.AffectableAbilitiesIndex[
                (playerEntity.Id, ItemData.SkillbookOfWaterSourcery.Name + ": Consult")]!;
        var firstWaterBook = playerEntity.Being!.Items.Single();
        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);
        ItemData.SkillbookOfConjuration.Instantiate(playerEntity);
        TestHelper.ActivateAbility(firstWaterBookAbility, playerEntity, manager, 2);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        var conjurationBookAbility = playerEntity.Being.Abilities
            .First(a => a.Ability!.Name == ItemData.SkillbookOfConjuration.Name + ": Consult");
        TestHelper.ActivateAbility(conjurationBookAbility, playerEntity, manager, 3);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);

        playerEntity.Player!.SkillPoints = 5;
        manager.SkillAbilitiesSystem.BuyAbilityLevel(AbilityData.WaterSourcery, playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]!.Ability!.Level);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        TestHelper.DeactivateAbility(conjurationBookAbility, playerEntity, manager);

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = firstWaterBook;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetLevelEntity = level.Entity;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(firstWaterBookAbility.Manager);
        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        var deactivateMessage = DeactivateAbilityMessage.Create(manager);
        deactivateMessage.AbilityEntity =
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!;
        deactivateMessage.ActivatorEntity = playerEntity;
        manager.Enqueue(deactivateMessage);

        manager.Queue.ProcessQueue(manager);

        manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability = null;

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]);
        Assert.False(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]?.Ability
            ?.IsUsable == true);
    }

    [Fact]
    public void Default_attacks_are_slotted_when_weapons_are_equipped()
    {
        var level = TestHelper.BuildLevel(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FireStaff.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Same(AbilityData.DoubleMeleeAttack.Name,
            playerEntity.Being!.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!.Name);
        Assert.False(
            playerEntity.Being.SlottedAbilities.ContainsKey(AbilitySlottingSystem.DefaultRangedAttackSlot));
        Assert.Equal(3, playerEntity.Being.Abilities.Select(a => a.Ability!)
            .Count(a => a.IsUsable && (a.Activation & ActivationType.Slottable) != 0));

        var swordEntity = playerEntity.Being.Items
            .Single(e => e.Item!.TemplateName == ItemData.LongSword.Name);
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);

        var staffEntity = playerEntity.Being.Items
            .Single(e => e.Item!.TemplateName == ItemData.FireStaff.Name);
        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = staffEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothRanged;

        manager.Enqueue(equipMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Same(AbilityData.TwoHandedMeleeAttack.Name,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot]!.Ability!.Name);
        Assert.Same(AbilityData.TwoHandedRangedAttack.Name,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultRangedAttackSlot]!.Ability!.Name);
        Assert.Equal(2, playerEntity.Being.Abilities.Select(a => a.Ability!)
            .Count(a => a.IsUsable && (a.Activation & ActivationType.Slottable) != 0));
    }
}

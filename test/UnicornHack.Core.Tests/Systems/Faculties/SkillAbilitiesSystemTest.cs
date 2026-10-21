using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Data.Players;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Systems.Faculties;

public class SkillAbilitiesSystemTest
{

    [Fact]
    public void Skills_add_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]);

        var firstWaterBook = playerEntity.Being!.Items.Single();
        var firstWaterBookAbility = firstWaterBook.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Consult");
        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);
        ItemData.SkillbookOfConjuration.Instantiate(playerEntity);
        TestHelper.ActivateAbility(firstWaterBookAbility, playerEntity, manager, 2);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        var secondWaterBook = Assert.Single(
            playerEntity.Being.Items,
            item => item != firstWaterBook
                    && item.Item!.TemplateName == ItemData.SkillbookOfWaterSourcery.Name);
        var secondWaterBookAbility = secondWaterBook.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Consult");
        var conjurationBook = Assert.Single(
            playerEntity.Being.Items,
            item => item.Item!.TemplateName == ItemData.SkillbookOfConjuration.Name);
        var conjurationBookAbility = conjurationBook.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Consult");
        TestHelper.ActivateAbility(conjurationBookAbility, playerEntity, manager, 5);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);

        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(4,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, EffectApplicationSystem.InnateAbilityName)]);
        Assert.Equal(1, playerEntity.ActiveEffects!.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name).Count(
                effect => effect.SourceAbilityEntityId == 0));
        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]!.Ability!.Level);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        manager.Enqueue(DeactivateAbilityMessage.Create(
            conjurationBookAbility.Ability!, playerEntity, manager));

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = firstWaterBook;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Contains(secondWaterBook, playerEntity.Being.Items);
        Assert.Contains(secondWaterBookAbility, playerEntity.Being.SlottedAbilities.Values);
        Assert.Equal(3,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.DoesNotContain(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == AbilityData.Conjuration.Name);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

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
        var level = TestHelper.BuildRoom(".");
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
        Assert.Equal(3, playerEntity.Being!.SlottedAbilities.Values.Select(a => a.Ability!)
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
        Assert.Equal(2, playerEntity.ActiveEffects!.Abilities.Select(a => a.Ability!)
            .Count(a => a.IsUsable && (a.Activation & ActivationType.Slottable) != 0));
    }

    [Fact]
    public void Equipping_through_an_item_ability_updates_the_default_attack()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);
        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, PlayerRaceData.Elf.GetRaceAbilityTemplate());
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(
            ExtremityType.GraspingFingers,
            playerEntity.Being!.UpperExtremities);
        Assert.Equal(
            ExtremityType.Fingers,
            playerEntity.Being.LowerExtremities);
        Assert.Equal(
            ExtremityType.None,
            playerEntity.Being.BackExtremities);
        Assert.True(playerEntity.Position!.MovementDelay > 0);

        var sword = playerEntity.Being.Items.Single(
            item => item.Item!.TemplateName == ItemData.LongSword.Name);
        TestHelper.ActivateItemAbility(
            sword, "Equip", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            EquipmentSlot.GraspPrimaryMelee,
            sword.Item!.EquippedSlot);
        Assert.Same(
            AbilityData.DoubleMeleeAttack.Name,
            playerEntity.Being.SlottedAbilities[
                AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!.Name);
    }

    [Fact]
    public void Weapon_attack_targets_the_weapon_slot_and_resolves_natural_weapons()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        // A weaponless wielder fights with natural weapons; the dual default melee attack is slotted and its
        // two Activate effects target the primary- and secondary-hand SLOTS (negative ids) rather than fixed
        // weapon entities, so the same static template works for any wielder.
        var defaultAttack =
            playerEntity.Being!.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!;
        Assert.Same(AbilityData.DoubleMeleeAttack, defaultAttack.Template);

        var activates = defaultAttack.Effects.Select(e => e.Effect!)
            .Where(e => e.EffectType == EffectType.Activate)
            .ToList();
        Assert.Equal(2, activates.Count);

        // Each Activate targets a hand slot (a negative encoded id) and resolves to that hand's fist.
        var primaryActivate = activates.Single(e =>
            e.TargetEntityId == -(int)EquipmentSlot.GraspPrimaryMelee);
        var secondaryActivate = activates.Single(e =>
            e.TargetEntityId == -(int)EquipmentSlot.GraspSecondaryMelee);

        var primaryResolved = AbilityActivationSystem.ResolveActivationTarget(
            primaryActivate.TargetEntityId!.Value, playerEntity, manager);
        Assert.Equal(playerEntity.Being.PrimaryNaturalWeaponId, primaryResolved!.Id);

        var secondaryResolved = AbilityActivationSystem.ResolveActivationTarget(
            secondaryActivate.TargetEntityId!.Value, playerEntity, manager);
        Assert.Equal(playerEntity.Being.SecondaryNaturalWeaponId, secondaryResolved!.Id);
    }
}

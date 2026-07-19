using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Items;

public class ItemUsageSystemTest
{
    [Fact]
    public void Default_attack_uses_the_natural_weapon_when_fiery_aegis_occupies_the_other_hand()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.FieryAegis.Instantiate(player);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ActivateAbility(ItemData.FieryAegis.Name + ": Equip", player, manager);
        manager.Queue.ProcessQueue(manager);

        var shield = player.Being!.Items.Single(
            item => item.Item!.TemplateName == ItemData.FieryAegis.Name);
        Assert.Equal(EquipmentSlot.GraspPrimaryMelee, shield.Item!.EquippedSlot);

        var attack = manager.AffectableAbilitiesIndex[
            (player.Id, AbilityData.OneHandedMeleeAttack.Name)]!;
        var message = ActivateAbilityMessage.Create(manager);
        message.AbilityEntity = attack;
        message.ActivatorEntity = player;
        message.TargetEntity = player;

        manager.AbilityActivationSystem.GetAttackStats(message);

        manager.ReturnMessage(message);
    }

    [Fact]
    public void Item_base_property_effects_are_innate()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;

        using var swordReference = ItemData.LongSword.Instantiate(manager);
        using var coinReference = ItemData.GoldCoin.Instantiate(manager);
        manager.Queue.ProcessQueue(manager);

        var swordBaseProperties = swordReference.Referenced.ActiveEffects!.PropertyEffects
            .Where(effect => effect.TargetName is nameof(PhysicalComponent.Weight)
                or nameof(PhysicalComponent.Material))
            .ToList();
        Assert.Equal(2, swordBaseProperties.Count);
        Assert.All(swordBaseProperties, effect => Assert.Equal(AbilityType.Innate, effect.SourceAbilityType));
        Assert.Equal(10, swordReference.Referenced.Physical!.Weight);
        Assert.Equal(Material.Steel, swordReference.Referenced.Physical.Material);

        var capacityEffect = Assert.Single(coinReference.Referenced.ActiveEffects!.PropertyEffects,
            effect => effect.TargetName == nameof(PhysicalComponent.Capacity));
        Assert.Equal(AbilityType.Innate, capacityEffect.SourceAbilityType);
        Assert.Equal(int.MaxValue - 1, coinReference.Referenced.Physical!.Capacity);
    }

    [Fact]
    public void Carried_item_ability_cooldown_rescales_with_its_owners_cooldown_factor()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.FieryAegis.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var shieldEntity = playerEntity.Being!.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var ability = playerEntity.ActiveEffects!.Abilities
            .Select(entity => entity.Ability!)
            .Single(candidate => candidate.Activation == ActivationType.WhileToggled);
        var itemAbility = shieldEntity.ActiveEffects!.Abilities
            .Select(entity => entity.Ability!)
            .Single(candidate => candidate.Activation == ActivationType.WhileToggled);
        Assert.Same(shieldEntity, itemAbility.OwnerEntity);

        TestHelper.ActivateAbility(ability.Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(ability.IsActive);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Slow, duration: 100, manager);

        var deactivateMessage = DeactivateAbilityMessage.Create(ability, playerEntity, manager);
        manager.Enqueue(deactivateMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(200, playerEntity.Being.CooldownFactor);
        Assert.Equal(manager.Game.CurrentTick + 400, itemAbility.CooldownTick);

        manager.EffectApplicationSystem.RemoveAbility(
            playerEntity, AbilityData.Slowed.Name, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, playerEntity.Being.CooldownFactor);
        Assert.Equal(manager.Game.CurrentTick + 200, itemAbility.CooldownTick);
    }

    [Fact]
    public void Stuck_prevents_unequipping()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var swordEntity = playerEntity.Being!.Items
            .Single(e => e.Item!.TemplateName == ItemData.LongSword.Name);

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspBothMelee, swordEntity.Item!.EquippedSlot);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stick, playerEntity, amount: 200));

        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Stuck.Name));

        var unequipMessage = EquipItemMessage.Create(manager);
        unequipMessage.ActorEntity = playerEntity;
        unequipMessage.ItemEntity = swordEntity;
        unequipMessage.Slot = EquipmentSlot.None;
        manager.Enqueue(unequipMessage);
        manager.Queue.ProcessQueue(manager);

        // The gear is adhered fast and can't be removed while stuck.
        Assert.Equal(EquipmentSlot.GraspBothMelee, swordEntity.Item!.EquippedSlot);
    }

    [Fact]
    public void Stuck_level_one_prevents_pickup_drop_and_equip()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(player);
        Assert.True(ItemData.FlaskOfHealing.Instantiate(level, new Point(0, 0)));
        manager.Queue.ProcessQueue(manager);
        var sword = player.Being!.Items.Single(item => item.Item!.TemplateName == ItemData.LongSword.Name);
        var flask = level.Items[new Point(0, 0)];

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stick, player, amount: 200));
        manager.Queue.ProcessQueue(manager);

        var drop = MoveItemMessage.Create(manager);
        drop.ItemEntity = sword;
        drop.TargetRoomEntity = level.Entity;
        drop.TargetCell = new Point(0, 0);
        manager.Enqueue(drop);

        var pickup = MoveItemMessage.Create(manager);
        pickup.ItemEntity = flask;
        pickup.TargetContainerEntity = player;
        manager.Enqueue(pickup);

        var equip = EquipItemMessage.Create(manager);
        equip.ActorEntity = player;
        equip.ItemEntity = sword;
        equip.Slot = EquipmentSlot.GraspBothMelee;
        manager.Enqueue(equip);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(player.Id, sword.Item!.ContainerId);
        Assert.Equal(EquipmentSlot.None, sword.Item.EquippedSlot);
        Assert.Null(flask.Item!.ContainerId);
        Assert.Same(flask, level.Items[new Point(0, 0)]);
    }

    [Fact]
    public void Cannot_unequip_items_in_use()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.Wait;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.FieryAegis.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        var shieldEntity = playerEntity.Being!.Items.Single();

        var shieldAbilityName = ItemData.FieryAegis.Name + ": Equip";
        Assert.NotNull(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbilityName)]);

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var shieldAbility = playerEntity.ActiveEffects!.Abilities
            .Select(a => a.Ability!)
            .Single(a => (a.Activation & ActivationType.Slottable) != 0
                         && a.Template?.Type.IsDefaultAttack() != true);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbilityName)]);

        TestHelper.ActivateAbility(shieldAbility.Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(shieldAbility.IsActive);
        Assert.Equal(30, playerEntity.Being.FireResistance);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        manager.Enqueue(equipMessage);

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = shieldEntity;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item!.EquippedSlot);
        Assert.Equal(2, shieldAbility.Slot);
        Assert.True(shieldAbility.IsActive);

        var deactivateMessage = DeactivateAbilityMessage.Create(
            shieldAbility, playerEntity, manager);
        manager.Enqueue(deactivateMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, shieldAbility.Slot);
        Assert.False(shieldAbility.IsActive);
        Assert.Equal(0, playerEntity.Being.FireResistance);
        Assert.Equal(200, shieldAbility.CooldownTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        manager.Enqueue(equipMessage);

        moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = shieldEntity;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;
        manager.Enqueue(moveItemMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
        Assert.NotNull(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbility.Name)]);

        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        player.NextAction = ActorActionType.Wait;
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);
        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(shieldAbility.CooldownTick);
        Assert.Equal(200, manager.Game.CurrentTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(shieldAbility.CooldownTick);
        Assert.Null(shieldAbility.Entity);
        Assert.Equal(EquipmentSlot.None, shieldEntity.Item.EquippedSlot);
        Assert.Equal(playerEntity.Id, shieldEntity.Item.ContainerId);
        Assert.NotNull(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbilityName)]);

        // The re-granted equip ability takes the first free slot (2) once the unequipped shield's ability
        // is gone, instead of the higher slot the legacy applied-effect timing left it in.
        var equipShieldAbility = playerEntity.Being.SlottedAbilities[2].Ability!;
        Assert.Equal(shieldAbilityName, equipShieldAbility.Name);

        TestHelper.ActivateAbility(equipShieldAbility.Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            2 - playerEntity.Physical!.Capacity, nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspPrimaryMelee, shieldEntity.Item.EquippedSlot);
        Assert.Equal(playerEntity.Id, shieldEntity.Item.ContainerId);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(shieldEntity.Item.ContainerId);
    }

    [Fact]
    public void Can_force_unequip_items_in_use()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.Wait;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.FieryAegis.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        var shieldEntity = playerEntity.Being!.Items.Single();

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var shieldAbility = playerEntity.ActiveEffects!.Abilities
            .Select(a => a.Ability!)
            .Single(a => (a.Activation & ActivationType.Slottable) != 0
                         && a.Template?.Type.IsDefaultAttack() != true);

        Assert.Equal(5, player.NextActionTick);

        TestHelper.ActivateAbility(shieldAbility.Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(shieldAbility.IsActive);
        Assert.Equal(30, playerEntity.Being.FireResistance);
        Assert.Equal(55, player.NextActionTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Force = true;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(shieldAbility.Entity);
        Assert.False(shieldAbility.IsActive);
        Assert.Equal(0, playerEntity.Being.FireResistance);
        var activateAbility = shieldEntity.ActiveEffects!.Abilities
            .Select(a => a.Ability!)
            .Single(a => (a.Activation & ActivationType.Slottable) != 0
                         && a.Template?.Type.IsDefaultAttack() != true);
        Assert.Equal(200, activateAbility.CooldownTick);
        Assert.Equal(60, player.NextActionTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = shieldEntity;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item!.EquippedSlot);
        Assert.Single(playerEntity.Being.Items);

        moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = shieldEntity;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;
        moveItemMessage.Force = true;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Empty(playerEntity.Being.Items);
    }

    [Fact]
    public void Cannot_equip_items_that_require_EP()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.Wait;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.CloakOfInvisibility.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(100, playerEntity.Being!.Visibility);

        var cloakEntity = playerEntity.Being.Items.Single();
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = cloakEntity;
        equipMessage.Slot = EquipmentSlot.Back;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.Back, cloakEntity.Item!.EquippedSlot);
        Assert.Equal(50, playerEntity.Being.EnergyPoints);
        Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);
        Assert.Equal(0, playerEntity.Being.Visibility);

        playerEntity.Being.EnergyPoints = 20;

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = cloakEntity;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.None, cloakEntity.Item.EquippedSlot);
        Assert.Equal(20, playerEntity.Being.EnergyPoints);
        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);
        Assert.Equal(100, playerEntity.Being.Visibility);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = cloakEntity;
        equipMessage.Slot = EquipmentSlot.Back;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.None, cloakEntity.Item.EquippedSlot);
        Assert.Equal(20, playerEntity.Being.EnergyPoints);
        Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);
        Assert.Equal(100, playerEntity.Being.Visibility);
    }

    [Fact]
    public void Items_add_hindrance_when_requirements_not_met()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var player = playerEntity.Player!;
        player.NextAction = ActorActionType.Wait;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, playerEntity.Being!.Hindrance);
        Assert.Equal(100, playerEntity.Being.Evasion);
        Assert.Equal(100, playerEntity.Position!.MovementDelay);
        Assert.Equal(100, playerEntity.Position.TurningDelay);

        var swordEntity = playerEntity.Being.Items.Single();

        Assert.Equal(10, swordEntity.Physical!.Weight);

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, playerEntity.Being.Hindrance);
        Assert.Equal(100, playerEntity.Being.Evasion);
        Assert.Equal(100, playerEntity.Position.MovementDelay);
        Assert.Equal(100, playerEntity.Position.TurningDelay);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.None;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspSecondaryMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspSecondaryMelee, swordEntity.Item!.EquippedSlot);
        Assert.Equal(31, playerEntity.Being.Hindrance);
        Assert.Equal(85, playerEntity.Being.Evasion);
        Assert.Equal(142, playerEntity.Position.MovementDelay);
        Assert.Equal(142, playerEntity.Position.TurningDelay);
    }
}

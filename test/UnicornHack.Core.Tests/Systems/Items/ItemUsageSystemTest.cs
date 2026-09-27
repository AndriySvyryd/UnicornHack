using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Items;

public class ItemUsageSystemTest
{
    [Fact]
    public void Generated_equip_ability_adds_equipment_delay()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        Assert.True(ItemData.LongSword.Instantiate(playerEntity));
        manager.Queue.ProcessQueue(manager);

        var sword = playerEntity.Being!.Items.Single(
            item => item.Item!.TemplateName == ItemData.LongSword.Name);
        var equipAbility = sword.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Equip");
        Assert.Equal(
            "Long sword: Equip",
            manager.Game.Services.Language.GetString(equipAbility.Ability!));

        var initialTick = playerEntity.Player!.NextActionTick;
        TestHelper.ActivateAbility(equipAbility, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(EquipmentSlot.None, sword.Item!.EquippedSlot);
        Assert.Equal(initialTick + ItemUsageSystem.EquipmentActionDelay, playerEntity.Player.NextActionTick);
    }

    [Fact]
    public void Non_forced_equip_replaces_conflicting_item()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var swords = playerEntity.Being!.Items
            .Where(item => item.Item!.TemplateName == ItemData.LongSword.Name)
            .ToList();
        var initialTick = playerEntity.Player!.NextActionTick;

        foreach (var sword in swords)
        {
            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = sword;
            equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;
            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(EquipmentSlot.None, swords[0].Item!.EquippedSlot);
        Assert.Equal(EquipmentSlot.GraspPrimaryMelee, swords[1].Item!.EquippedSlot);
        Assert.Equal(initialTick + 3 * ItemUsageSystem.EquipmentActionDelay, playerEntity.Player.NextActionTick);
    }

    [Fact]
    public void Forced_equipment_changes_add_delay()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var swords = playerEntity.Being!.Items
            .Where(item => item.Item!.TemplateName == ItemData.LongSword.Name)
            .ToList();
        var initialTick = playerEntity.Player!.NextActionTick;

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swords[0];
        equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;
        equipMessage.Force = true;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialTick + ItemUsageSystem.EquipmentActionDelay, playerEntity.Player.NextActionTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swords[1];
        equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;
        equipMessage.Force = true;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialTick + 3 * ItemUsageSystem.EquipmentActionDelay, playerEntity.Player.NextActionTick);
        Assert.Equal(EquipmentSlot.None, swords[0].Item!.EquippedSlot);
        Assert.Equal(EquipmentSlot.GraspPrimaryMelee, swords[1].Item!.EquippedSlot);

        var unequipMessage = EquipItemMessage.Create(manager);
        unequipMessage.ActorEntity = playerEntity;
        unequipMessage.ItemEntity = swords[1];
        unequipMessage.Slot = EquipmentSlot.None;
        unequipMessage.Force = true;
        manager.Enqueue(unequipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialTick + 4 * ItemUsageSystem.EquipmentActionDelay, playerEntity.Player.NextActionTick);
        Assert.Equal(EquipmentSlot.None, swords[1].Item!.EquippedSlot);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unequipping_with_a_full_hotbar_requires_force(bool force)
    {
        var level = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        ItemData.LongSword.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var sword = playerEntity.Being!.Items.Single(
            item => item.Item!.TemplateName == ItemData.LongSword.Name);
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = sword;
        equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            AbilitySlottingSystem.DefaultRangedAttackSlot + 1 - playerEntity.Physical!.Capacity,
            nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(manager.AbilitySlottingSystem.GetFirstFreeSlot(playerEntity));
        var tickBefore = playerEntity.Player!.NextActionTick;

        var unequipMessage = EquipItemMessage.Create(manager);
        unequipMessage.ActorEntity = playerEntity;
        unequipMessage.ItemEntity = sword;
        unequipMessage.Slot = EquipmentSlot.None;
        unequipMessage.Force = force;
        manager.Enqueue(unequipMessage);
        manager.Queue.ProcessQueue(manager);

        if (force)
        {
            Assert.Equal(EquipmentSlot.None, sword.Item!.EquippedSlot);
            Assert.Null(sword.Item.ContainerId);
            Assert.Equal(new Point(0, 0), sword.Position!.Cell);
            Assert.Equal(
                tickBefore + ItemUsageSystem.EquipmentActionDelay,
                playerEntity.Player.NextActionTick);
        }
        else
        {
            Assert.Equal(EquipmentSlot.GraspPrimaryMelee, sword.Item!.EquippedSlot);
            Assert.Equal(playerEntity.Id, sword.Item.ContainerId);
            Assert.Equal(tickBefore, playerEntity.Player.NextActionTick);
        }
    }

    [Fact]
    public void Default_attack_uses_the_natural_weapon_when_fiery_aegis_occupies_the_other_hand()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.FieryAegis.Instantiate(player);
        manager.Queue.ProcessQueue(manager);

        var equipAbility = player.Being!.Items.Single()
            .ActiveEffects!.Abilities.Single(ability => ability.Ability!.Name == "Equip");
        TestHelper.ActivateAbility(equipAbility, player, manager);
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
    public void Always_active_item_abilities_apply_to_the_item_not_the_possessor()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        var playerCapacity = playerEntity.Physical!.Capacity;
        var template = new Item
        {
            Name = "capacity charm",
            Type = ItemType.Trinket,
            Material = Material.Glass,
            Weight = 1,
            Abilities =
            [
                new Ability
                {
                    Name = "capacity charm ability",
                    Activation = ActivationType.Always,
                    Effects =
                    [
                        new ChangeProperty<int>
                        {
                            Amount = "3",
                            PropertyName = nameof(PhysicalComponent.Capacity)
                        }
                    ]
                }
            ]
        };

        using var itemReference = template.Instantiate(manager);
        var itemEntity = itemReference.Referenced;
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, itemEntity.Physical!.Capacity);
        Assert.Equal(playerCapacity, playerEntity.Physical.Capacity);

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = itemEntity;
        moveMessage.TargetContainerEntity = playerEntity;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, itemEntity.Physical.Capacity);
        Assert.Equal(playerCapacity, playerEntity.Physical.Capacity);

        moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = itemEntity;
        moveMessage.TargetRoomEntity = level.Entity;
        moveMessage.TargetCell = new Point(0, 0);
        moveMessage.Force = true;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, itemEntity.Physical.Capacity);
        Assert.Equal(playerCapacity, playerEntity.Physical.Capacity);
    }

    [Fact]
    public void Identical_items_have_independent_ability_cooldowns()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var abilities = playerEntity.Being!.Items
            .Where(item => item.Item!.TemplateName == ItemData.FlaskOfHealing.Name)
            .Select(item => item.ActiveEffects!.Abilities.Single(
                ability => ability.Ability!.Name == "Drink").Ability!)
            .ToList();
        Assert.Equal(2, abilities.Count);
        Assert.Equal(2, abilities.Select(ability => ability.EntityId).Distinct().Count());

        playerEntity.Being.HitPoints = 1;
        TestHelper.ActivateAbility(abilities[0].Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(abilities[0].CooldownXpLeft);
        Assert.Null(abilities[1].CooldownXpLeft);
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

        var itemAbility = shieldEntity.ActiveEffects!.Abilities
            .Select(entity => entity.Ability!)
            .Single(candidate => candidate.Activation == ActivationType.WhileToggled);
        Assert.Same(shieldEntity, itemAbility.OwnerEntity);

        TestHelper.ActivateAbility(itemAbility.Entity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(itemAbility.IsActive);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Slow, duration: 100, manager);

        var deactivateMessage = DeactivateAbilityMessage.Create(itemAbility, playerEntity, manager);
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

        var equipShieldAbility = shieldEntity.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Equip");
        Assert.NotNull(equipShieldAbility.Ability!.Slot);
        var shieldAbility = shieldEntity.ActiveEffects.Abilities
            .Select(a => a.Ability!)
            .Single(a => a.Name == "Activate");
        Assert.Null(shieldAbility.Slot);

        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(equipShieldAbility.Ability.Slot);
        Assert.Equal(2, shieldAbility.Slot);

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
        var cooldownTarget = shieldAbility.CooldownTick!.Value;

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
        Assert.True(manager.Game.CurrentTick >= cooldownTarget);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(shieldAbility.CooldownTick);
        Assert.NotNull(shieldAbility.Entity);
        Assert.Null(shieldAbility.Slot);
        Assert.Equal(EquipmentSlot.None, shieldEntity.Item.EquippedSlot);
        Assert.Equal(playerEntity.Id, shieldEntity.Item.ContainerId);

        // The item-owned equip action reuses the toggle's slot after unequipping.
        Assert.Same(equipShieldAbility, playerEntity.Being.SlottedAbilities[2]);
        Assert.Equal("Equip", equipShieldAbility.Ability!.Name);

        TestHelper.ActivateAbility(equipShieldAbility, playerEntity, manager);
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

        Assert.Equal(EquipmentSlot.GraspPrimaryMelee, shieldEntity.Item.EquippedSlot);
        Assert.Equal(playerEntity.Id, shieldEntity.Item.ContainerId);
    }

    [Fact]
    public void Cannot_force_unequip_items_in_use()
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

        var shieldAbility = shieldEntity.ActiveEffects!.Abilities
            .Select(a => a.Ability!)
            .Single(a => a.Name == "Activate");

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

        Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item!.EquippedSlot);
        Assert.NotNull(shieldAbility.Entity);
        Assert.True(shieldAbility.IsActive);
        Assert.Equal(30, playerEntity.Being.FireResistance);
        Assert.Equal(55, player.NextActionTick);

        var deactivateMessage = DeactivateAbilityMessage.Create(
            shieldAbility, playerEntity, manager);
        manager.Enqueue(deactivateMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(shieldAbility.IsActive);
        Assert.Equal(200, shieldAbility.CooldownTick);

        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = shieldEntity;
        equipMessage.Force = true;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = shieldEntity;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;
        moveItemMessage.Force = true;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item!.EquippedSlot);
        Assert.Single(playerEntity.Being.Items);
        Assert.NotNull(shieldAbility.Entity);
        Assert.Equal(200, shieldAbility.CooldownTick);
        Assert.Equal(55, player.NextActionTick);
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

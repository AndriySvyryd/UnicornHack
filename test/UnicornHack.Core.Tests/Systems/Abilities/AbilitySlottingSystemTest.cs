using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Abilities;

public class AbilitySlottingSystemTest
{
    [Fact]
    public void Ability_slots_reject_messages_without_an_ability_or_slot()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var message = SetAbilitySlotMessage.Create(playerEntity, manager);

        Assert.Throws<InvalidOperationException>(
            () => manager.AbilitySlottingSystem.Process(message, manager));
        manager.ReturnMessage(message);
    }

    [Fact]
    public void Ability_slots_reject_another_actor_as_owner()
    {
        var level = TestHelper.BuildRoom("..");
        var firstPlayer = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var secondPlayer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(1, 0));
        var manager = firstPlayer.Manager;
        ItemData.FlaskOfHealing.Instantiate(firstPlayer);
        manager.Queue.ProcessQueue(manager);

        var abilityEntity = Assert.Single(firstPlayer.Being!.Items)
            .ActiveEffects!.Abilities.Single(ability => ability.Ability!.Name == "Drink");
        var message = SetAbilitySlotMessage.Create(secondPlayer, manager);
        message.AbilityEntity = abilityEntity;
        message.Slot = 3;

        Assert.Throws<InvalidOperationException>(
            () => manager.AbilitySlottingSystem.Process(message, manager));
        Assert.NotEqual(3, abilityEntity.Ability!.Slot);
        manager.ReturnMessage(message);
    }

    [Fact]
    public void Stacked_items_expose_one_ability_for_the_stack_entity()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        ItemData.PotionOfExperience.Instantiate(playerEntity);
        ItemData.PotionOfExperience.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var stackEntity = Assert.Single(playerEntity.Being!.Items);
        var stackAbility = stackEntity.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Drink");
        Assert.NotNull(stackAbility.Ability!.Slot);
        Assert.Same(
            stackAbility,
            playerEntity.Being.SlottedAbilities[stackAbility.Ability.Slot.Value]);

        Assert.All(
            stackEntity.Item!.Items,
            stackedItem => Assert.All(
                stackedItem.ActiveEffects!.Abilities,
                ability => Assert.Null(ability.Ability!.Slot)));

        using var splitReference = manager.ItemMovingSystem.Split(stackEntity.Item, 1);
        var splitEntity = splitReference.Referenced;
        var splitAbility = splitEntity.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Drink");
        manager.Queue.ProcessQueue(manager);
        Assert.Null(splitAbility.Ability!.Slot);

        var remainingEntity = Assert.Single(playerEntity.Being.Items);
        var remainingAbility = remainingEntity.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "Drink");
        Assert.NotEqual(splitAbility.Id, remainingAbility.Id);
        Assert.NotNull(remainingAbility.Ability!.Slot);
    }

    [Fact]
    public void Item_owned_abilities_are_slotted_on_the_containing_actor()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var itemEntity = Assert.Single(playerEntity.Being!.Items);
        var itemAbilityEntity = itemEntity.ActiveEffects!.Abilities.Single(
            entity => entity.Ability!.Name == "Drink");
        Assert.DoesNotContain(
            playerEntity.ActiveEffects!.Abilities,
            entity => entity.Ability!.Name?.Contains("flask of healing", StringComparison.Ordinal) == true);
        itemAbilityEntity.Ability!.Slot = 3;
        manager.Queue.ProcessQueue(manager);

        Assert.Contains(itemAbilityEntity, playerEntity.ActiveEffects!.AvailableAbilities);
        Assert.Same(itemAbilityEntity, playerEntity.Being.SlottedAbilities[3]);

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = itemEntity;
        moveMessage.TargetRoomEntity = level.Entity;
        moveMessage.TargetCell = new Point(0, 0);
        moveMessage.Force = true;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.False(playerEntity.Being.SlottedAbilities.ContainsKey(3));
        Assert.Null(itemAbilityEntity.Ability.Slot);
        Assert.DoesNotContain(itemAbilityEntity, playerEntity.ActiveEffects.AvailableAbilities);
    }

    [Fact]
    public void Capacity_reduction_unslots_highest_slots_first()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var initialCapacity = playerEntity.Physical!.Capacity;

        var abilities = new List<GameEntity>();
        foreach (var slot in new[] { 3, 5, 7 })
        {
            var ability = new Ability
            {
                Name = "capacity test " + slot,
                Activation = ActivationType.Manual
            }.AddToAffectable(playerEntity).Entity;
            abilities.Add(ability);
            manager.Queue.ProcessQueue(manager);

            var setSlot = SetAbilitySlotMessage.Create(playerEntity, manager);
            setSlot.AbilityEntity = ability;
            setSlot.Slot = slot;
            manager.Enqueue(setSlot);
            manager.Queue.ProcessQueue(manager);
        }

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            7 - initialCapacity,
            nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, abilities[0].Ability!.Slot);
        Assert.Equal(5, abilities[1].Ability!.Slot);
        Assert.Null(abilities[2].Ability!.Slot);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            5 - initialCapacity,
            nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, abilities[0].Ability!.Slot);
        Assert.Null(abilities[1].Ability!.Slot);
    }

    [Fact]
    public void Abilities_can_be_assigned_to_slots()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        GameEntity toggledAbility;
        using (var abilityEntityReference = manager.CreateEntity())
        {
            toggledAbility = abilityEntityReference.Referenced;

            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = playerEntity;

            toggledAbility.Ability = ability;
        }

        using (var effectEntityReference = manager.CreateEntity())
        {
            var bleedingEffectEntity = effectEntityReference.Referenced;

            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.BleedingResistance);
            effect.Amount = "10";
            effect.ContainingAbilityId = toggledAbility.Id;

            bleedingEffectEntity.Effect = effect;
        }

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Same(toggledAbility, playerEntity.Being!.SlottedAbilities[2]);
        Assert.Equal(10, playerEntity.Being.BleedingResistance);

        GameEntity targetedAbility;
        using (var abilityEntityReference = manager.CreateEntity())
        {
            targetedAbility = abilityEntityReference.Referenced;

            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Targeted;
            ability.OwnerEntity = playerEntity;

            targetedAbility.Ability = ability;
        }

        using (var effectEntityReference = manager.CreateEntity())
        {
            var bleedingEffectEntity = effectEntityReference.Referenced;

            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.BleedDamage;
            effect.Duration = EffectDuration.Infinite;
            effect.Amount = "10";
            effect.ContainingAbilityId = targetedAbility.Id;

            bleedingEffectEntity.Effect = effect;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.Slot);
        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(2, targetedAbility.Ability.Slot);
        Assert.Equal(0, playerEntity.Being.BleedingResistance);

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);
        Assert.False(playerEntity.Being.SlottedAbilities.ContainsKey(2));

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, targetedAbility.Ability.Slot);

        targetedAbility.Ability.IsUsable = false;
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(targetedAbility.Ability.Slot);

        targetedAbility.Ability.IsUsable = true;
        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = AbilitySlottingSystem.DefaultMeleeAttackSlot;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.DoubleMeleeAttack.Name)];
        setSlotMessage.AbilityEntity = attackAbility;
        setSlotMessage.Slot = AbilitySlottingSystem.DefaultRangedAttackSlot;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        setSlotMessage.AbilityEntity = attackAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        setSlotMessage.Slot = -3;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        setSlotMessage.Slot = 20;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(targetedAbility.Ability.Slot);

        var alwaysAbility = playerEntity.ActiveEffects!.Abilities
            .First(a => (a.Ability!.Activation & ActivationType.Always) != 0);
        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = alwaysAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(alwaysAbility.Ability!.Slot);

        setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = playerEntity.Physical!.Capacity - 1;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.True(playerEntity.Physical.Capacity > 1);
        Assert.Equal(playerEntity.Physical.Capacity - 1, targetedAbility.Ability.Slot);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            3 - playerEntity.Physical.Capacity, nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);
    }

    [Fact]
    public void Items_are_stored_in_slots()
    {
        var level = TestHelper.BuildRoom(@"
...
...
..#");

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        ItemData.GoldCoin.Instantiate(level, new Point(1, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        Assert.Null(playerEntity.Being!.SlottedAbilities.GetValueOrDefault(2));

        var travelMessage = TravelMessage.Create(manager);
        travelMessage.ActorEntity = playerEntity;
        travelMessage.TargetHeading = Direction.East;
        travelMessage.TargetCell = new Point(1, 0);
        manager.Enqueue(travelMessage);

        manager.Queue.ProcessQueue(manager);

        var coinItem = playerEntity.Being.Items.Single().Item!;
        var dropCoinAbilityEntity = playerEntity.Being.SlottedAbilities.GetValueOrDefault(2);

        var setSlotMessage = SetAbilitySlotMessage.Create(playerEntity, manager);
        setSlotMessage.AbilityEntity = dropCoinAbilityEntity;
        manager.Enqueue(setSlotMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(coinItem.ContainerId);
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position!.Cell);
        Assert.Null(playerEntity.Being.SlottedAbilities.GetValueOrDefault(2));

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = coinItem.Entity;
        moveItemMessage.TargetContainerEntity = playerEntity;
        manager.Enqueue(moveItemMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(playerEntity.Id, coinItem.ContainerId);

        dropCoinAbilityEntity = playerEntity.Being.SlottedAbilities.GetValueOrDefault(2)!;
        TestHelper.ActivateAbility(dropCoinAbilityEntity, playerEntity, manager);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(coinItem.ContainerId);
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position.Cell);

        moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = coinItem.Entity;
        moveItemMessage.TargetContainerEntity = playerEntity;
        manager.Enqueue(moveItemMessage);

        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            2 - playerEntity.Physical!.Capacity, nameof(PhysicalComponent.Capacity), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(coinItem.ContainerId);
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position.Cell);
        Assert.Null(playerEntity.Being.SlottedAbilities.GetValueOrDefault(2));
    }
}

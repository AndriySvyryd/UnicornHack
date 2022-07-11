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
    public void Abilities_can_be_assigned_to_slots()
    {
        var level = TestHelper.BuildLevel(".");
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
            effect.Duration = EffectDuration.Infinite;
            effect.EffectType = EffectType.ChangeProperty;
            effect.TargetName = nameof(BeingComponent.BleedingResistance);
            effect.AppliedAmount = 10;
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
            effect.Duration = EffectDuration.Infinite;
            effect.EffectType = EffectType.Bleed;
            effect.AppliedAmount = 10;
            effect.ContainingAbilityId = targetedAbility.Id;

            bleedingEffectEntity.Effect = effect;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.Slot);
        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(2, targetedAbility.Ability.Slot);
        Assert.Equal(0, playerEntity.Being.BleedingResistance);

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.OwnerEntity = playerEntity;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);
        Assert.False(playerEntity.Being.SlottedAbilities.ContainsKey(2));

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, targetedAbility.Ability.Slot);

        targetedAbility.Ability.IsUsable = false;
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(targetedAbility.Ability.Slot);

        targetedAbility.Ability.IsUsable = true;
        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = AbilitySlottingSystem.DefaultMeleeAttackSlot;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.DoubleMeleeAttack.Name)];
        setSlotMessage.AbilityEntity = attackAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        setSlotMessage.Slot = -3;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

        setSlotMessage.Slot = 20;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(targetedAbility.Ability.Slot);

        var alwaysAbility = playerEntity.Being.Abilities
            .First(a => (a.Ability!.Activation & ActivationType.Always) != 0);
        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = alwaysAbility;
        setSlotMessage.Slot = 2;
        Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
        Assert.Null(alwaysAbility.Ability!.Slot);

        setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = targetedAbility;
        setSlotMessage.Slot = playerEntity.Physical!.Capacity - 1;
        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);
        Assert.True(playerEntity.Physical.Capacity > 1);
        Assert.Equal(playerEntity.Physical.Capacity - 1, targetedAbility.Ability.Slot);

        playerEntity.Physical.Capacity = 3;
        manager.Queue.ProcessQueue(manager);
        Assert.Null(targetedAbility.Ability.Slot);
    }

    [Fact]
    public void Items_are_stored_in_slots()
    {
        var level = TestHelper.BuildLevel(@"
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

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = dropCoinAbilityEntity;
        manager.Enqueue(setSlotMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(coinItem.ContainerId);
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position!.LevelCell);
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
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position.LevelCell);

        moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = coinItem.Entity;
        moveItemMessage.TargetContainerEntity = playerEntity;
        manager.Enqueue(moveItemMessage);

        manager.Queue.ProcessQueue(manager);

        playerEntity.Physical!.Capacity = 2;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(coinItem.ContainerId);
        Assert.Equal(new Point(1, 0), coinItem.Entity.Position.LevelCell);
        Assert.Null(playerEntity.Being.SlottedAbilities.GetValueOrDefault(2));
    }
}

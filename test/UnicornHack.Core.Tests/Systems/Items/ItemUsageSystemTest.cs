using System.Linq;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Items
{
    public class ItemUsageSystemTest
    {
        [Fact]
        public void Cannot_unequip_items_in_use()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            player.NextAction = PlayerAction.Wait;
            var manager = playerEntity.Manager;
            ItemData.FieryAegis.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            var shieldEntity = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();

            var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var shieldAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Select(a => a.Ability)
                .Single(a => (a.Activation & ActivationType.Slottable) != 0
                             && a.Template?.Type != AbilityType.DefaultAttack);

            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = shieldAbility.Entity;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(shieldAbility.IsActive);
            Assert.Equal(30, playerEntity.Being.FireResistance);

            equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            manager.Enqueue(equipMessage);

            var moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0,0);
            moveItemMessage.TargetLevelEntity = level.Entity;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.Equal(0, shieldAbility.Slot);
            Assert.True(shieldAbility.IsActive);

            var deactivateMessage =
                manager.AbilityActivationSystem.CreateDeactivateAbilityMessage(manager);
            deactivateMessage.AbilityEntity = shieldAbility.Entity;
            deactivateMessage.ActivatorEntity = playerEntity;
            manager.Enqueue(deactivateMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(shieldAbility.Slot);
            Assert.False(shieldAbility.IsActive);
            Assert.Equal(0, playerEntity.Being.FireResistance);
            Assert.Equal(200, shieldAbility.CooldownTick);

            equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            manager.Enqueue(equipMessage);

            moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0, 0);
            moveItemMessage.TargetLevelEntity = level.Entity;
            manager.Enqueue(moveItemMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.NotNull(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbility.Name)]);

            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);
            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);
            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(shieldAbility.CooldownTick);
            Assert.Equal(200, manager.Game.CurrentTick);

            equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.None, shieldEntity.Item.EquippedSlot);
            Assert.Null(shieldAbility.Entity);
        }

        [Fact]
        public void Can_force_unequip_items_in_use()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            player.NextAction = PlayerAction.Wait;
            var manager = playerEntity.Manager;
            ItemData.FieryAegis.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            var shieldEntity = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();

            var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var shieldAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Select(a => a.Ability)
                .Single(a => (a.Activation & ActivationType.Slottable) != 0
                             && a.Template?.Type != AbilityType.DefaultAttack);

            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = shieldAbility.Entity;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(shieldAbility.IsActive);
            Assert.Equal(30, playerEntity.Being.FireResistance);

            equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Force = true;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(shieldAbility.Entity);
            Assert.False(shieldAbility.IsActive);
            Assert.Equal(0, playerEntity.Being.FireResistance);
            var activateAbility = manager.AbilitiesToAffectableRelationship[shieldEntity.Id]
                .Select(a => a.Ability)
                .Single(a => (a.Activation & ActivationType.Slottable) != 0
                             && a.Template?.Type != AbilityType.DefaultAttack);
            Assert.Equal(200, activateAbility.CooldownTick);

            equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0, 0);
            moveItemMessage.TargetLevelEntity = level.Entity;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.Equal(1, manager.EntityItemsToContainerRelationship[playerEntity.Id].Count);

            moveItemMessage = manager.ItemMovingSystem.CreateMoveItemMessage(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0, 0);
            moveItemMessage.TargetLevelEntity = level.Entity;
            moveItemMessage.Force = true;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Empty(manager.EntityItemsToContainerRelationship[playerEntity.Id]);
        }
    }
}

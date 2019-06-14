using System.Linq;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Time;
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

            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var shieldAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Select(a => a.Ability)
                .Single(a => (a.Activation & ActivationType.Slottable) != 0
                             && a.Template?.Type != AbilityType.DefaultAttack);

            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = shieldAbility.Entity;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(shieldAbility.IsActive);
            Assert.Equal(30, playerEntity.Being.FireResistance);

            equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            manager.Enqueue(equipMessage);

            var moveItemMessage = MoveItemMessage.Create(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0,0);
            moveItemMessage.TargetLevelEntity = level.Entity;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.Equal(0, shieldAbility.Slot);
            Assert.True(shieldAbility.IsActive);

            var deactivateMessage =
                DeactivateAbilityMessage.Create(manager);
            deactivateMessage.AbilityEntity = shieldAbility.Entity;
            deactivateMessage.ActivatorEntity = playerEntity;
            manager.Enqueue(deactivateMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(shieldAbility.Slot);
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
            moveItemMessage.TargetLevelEntity = level.Entity;
            manager.Enqueue(moveItemMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.NotNull(manager.AffectableAbilitiesIndex[(playerEntity.Id, shieldAbility.Name)]);

            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);

            player.NextAction = PlayerAction.Wait;
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

            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var shieldAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Select(a => a.Ability)
                .Single(a => (a.Activation & ActivationType.Slottable) != 0
                             && a.Template?.Type != AbilityType.DefaultAttack);

            Assert.Equal(50, player.NextActionTick);

            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = shieldAbility.Entity;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(shieldAbility.IsActive);
            Assert.Equal(30, playerEntity.Being.FireResistance);
            Assert.Equal(100, player.NextActionTick);

            equipMessage = EquipItemMessage.Create(manager);
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
            Assert.Equal(150, player.NextActionTick);

            equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = shieldEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var moveItemMessage = MoveItemMessage.Create(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0, 0);
            moveItemMessage.TargetLevelEntity = level.Entity;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.GraspBothMelee, shieldEntity.Item.EquippedSlot);
            Assert.Equal(1, manager.EntityItemsToContainerRelationship[playerEntity.Id].Count);

            moveItemMessage = MoveItemMessage.Create(manager);
            moveItemMessage.ItemEntity = shieldEntity;
            moveItemMessage.TargetCell = new Point(0, 0);
            moveItemMessage.TargetLevelEntity = level.Entity;
            moveItemMessage.Force = true;

            manager.Enqueue(moveItemMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Empty(manager.EntityItemsToContainerRelationship[playerEntity.Id]);
        }

        [Fact]
        public void Cannot_equip_items_that_require_EP()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            player.NextAction = PlayerAction.Wait;
            var manager = playerEntity.Manager;
            ItemData.CloakOfInvisibility.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(100, playerEntity.Being.Visibility);

            var cloakEntity = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();
            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = cloakEntity;
            equipMessage.Slot = EquipmentSlot.Back;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(EquipmentSlot.Back, cloakEntity.Item.EquippedSlot);
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
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            player.NextAction = PlayerAction.Wait;
            var manager = playerEntity.Manager;
            ItemData.LongSword.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, playerEntity.Being.Hindrance);
            Assert.Equal(100, playerEntity.Being.Evasion);
            Assert.Equal(100, playerEntity.Position.MovementDelay);
            Assert.Equal(100, playerEntity.Position.TurningDelay);

            var swordEntity = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();

            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = swordEntity;
            equipMessage.Slot = EquipmentSlot.GraspPrimaryMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, playerEntity.Being.Hindrance);
            Assert.Equal(95, playerEntity.Being.Evasion);
            Assert.Equal(111, playerEntity.Position.MovementDelay);
            Assert.Equal(111, playerEntity.Position.TurningDelay);

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

            Assert.Equal(EquipmentSlot.GraspSecondaryMelee, swordEntity.Item.EquippedSlot);
            Assert.Equal(4, playerEntity.Being.Hindrance);
            Assert.Equal(80, playerEntity.Being.Evasion);
            Assert.Equal(166, playerEntity.Position.MovementDelay);
            Assert.Equal(166, playerEntity.Position.TurningDelay);
        }
    }
}

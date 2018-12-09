using System;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Faculties;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Abilities
{
    public class AbilitySlottingSystemTest
    {
        [Fact]
        public void Abilities_can_be_assigned_to_slots()
        {
            var level = TestHelper.BuildLevel(" ");

            var manager = level.Entity.Manager;

            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            GameEntity toggledAbility = null;
            using (var abilityEntityReference = manager.CreateEntity())
            {
                toggledAbility = abilityEntityReference.Referenced;

                var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Activation = ActivationType.WhileToggled;
                ability.OwnerEntity = player;

                toggledAbility.Ability = ability;
            }

            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.Equal(0, toggledAbility.Ability.Slot);
            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Same(toggledAbility, manager.SlottedAbilitiesIndex[(player.Id, 0)]);

            GameEntity targetedAbility = null;
            using (var abilityEntityReference = manager.CreateEntity())
            {
                targetedAbility = abilityEntityReference.Referenced;

                var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Activation = ActivationType.Targeted;
                ability.OwnerEntity = player;

                targetedAbility.Ability = ability;
            }

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Equal(0, targetedAbility.Ability.Slot);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.OwnerEntity = player;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.Null(targetedAbility.Ability.Slot);
            Assert.Null(manager.SlottedAbilitiesIndex[(player.Id, 0)]);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.Equal(0, targetedAbility.Ability.Slot);

            targetedAbility.Ability.IsUsable = false;
            manager.Queue.ProcessQueue(manager);
            Assert.Null(targetedAbility.Ability.Slot);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = 0;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
            Assert.Null(targetedAbility.Ability.Slot);

            targetedAbility.Ability.IsUsable = true;
            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = AbilitySlottingSystem.DefaultAttackSlot;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            var attackAbility = manager.AbilitiesToAffectableRelationship[player.Id]
                .Single(a => a.Ability.Name == SkillAbilitiesSystem.DoubleMeleeAttackName);
            setSlotMessage.AbilityEntity = attackAbility;
            setSlotMessage.Slot = 0;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            setSlotMessage.Slot = -2;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            setSlotMessage.Slot = 20;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
            Assert.Null(targetedAbility.Ability.Slot);

            var alwaysAbility = manager.AbilitiesToAffectableRelationship[player.Id]
                .First(a => (a.Ability.Activation & ActivationType.Always) != 0);
            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = alwaysAbility;
            setSlotMessage.Slot = 0;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
            Assert.Null(alwaysAbility.Ability.Slot);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = player.Being.AbilitySlotCount - 1;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.True(player.Being.AbilitySlotCount > 1);
            Assert.Equal(player.Being.AbilitySlotCount - 1, targetedAbility.Ability.Slot);

            player.Being.AbilitySlotCount = 1;
            manager.Queue.ProcessQueue(manager);
            Assert.Null(targetedAbility.Ability.Slot);
        }
    }
}

using System;
using System.Linq;
using UnicornHack.Data.Abilities;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Abilities
{
    public class AbilitySlottingSystemTest
    {
        [Fact]
        public void Abilities_can_be_assigned_to_slots()
        {
            var level = TestHelper.BuildLevel(".");
            var manager = level.Entity.Manager;
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));

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
                effect.Amount = 10;
                effect.ContainingAbilityId = toggledAbility.Id;

                bleedingEffectEntity.Effect = effect;
            }

            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, toggledAbility.Ability.Slot);
            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Same(toggledAbility, manager.SlottedAbilitiesIndex[(playerEntity.Id, 0)]);
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
                effect.Amount = 10;
                effect.ContainingAbilityId = targetedAbility.Id;

                bleedingEffectEntity.Effect = effect;
            }

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Equal(0, targetedAbility.Ability.Slot);
            Assert.Equal(0, playerEntity.Being.BleedingResistance);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.OwnerEntity = playerEntity;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.Null(targetedAbility.Ability.Slot);
            Assert.Null(manager.SlottedAbilitiesIndex[(playerEntity.Id, 0)]);

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
            setSlotMessage.Slot = AbilitySlottingSystem.DefaultMeleeAttackSlot;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.DoubleMeleeAttack.Name)];
            setSlotMessage.AbilityEntity = attackAbility;
            setSlotMessage.Slot = 0;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            setSlotMessage.Slot = -3;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));

            setSlotMessage.Slot = 20;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
            Assert.Null(targetedAbility.Ability.Slot);

            var alwaysAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .First(a => (a.Ability.Activation & ActivationType.Always) != 0);
            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = alwaysAbility;
            setSlotMessage.Slot = 0;
            Assert.Throws<InvalidOperationException>(() => manager.AbilitySlottingSystem.Process(setSlotMessage, manager));
            Assert.Null(alwaysAbility.Ability.Slot);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = targetedAbility;
            setSlotMessage.Slot = playerEntity.Being.AbilitySlotCount - 1;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);
            Assert.True(playerEntity.Being.AbilitySlotCount > 1);
            Assert.Equal(playerEntity.Being.AbilitySlotCount - 1, targetedAbility.Ability.Slot);

            playerEntity.Being.AbilitySlotCount = 1;
            manager.Queue.ProcessQueue(manager);
            Assert.Null(targetedAbility.Ability.Slot);
        }
    }
}

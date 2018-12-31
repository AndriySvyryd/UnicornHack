using System;
using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Faculties;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;
using Xunit;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivationSystemTest
    {
        private static readonly string TestMap = @"
.#.
.#.
...";

        [Fact]
        public void Can_use_projectile_abilities_on_targets_in_LOS()
        {
            var level = TestHelper.BuildLevel(TestMap);

            var manager = level.Entity.Manager;
            var listener = new AbilityActivatedListener();
            manager.Queue.Add(listener, AbilityActivationSystem.AbilityActivatedMessageName, -1);

            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(2, 0));
            playerEntity.Position.Heading = Direction.West;
            playerEntity.Player.SkillPoints = 5;
            manager.SkillAbilitiesSystem.ImproveSkill(nameof(PlayerComponent.Conjuration), playerEntity);
            ItemData.Shortbow.Instantiate(playerEntity);
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(2, 2));
            var elemental = CreatureData.AirElemental.Instantiate(level, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            var bow = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();
            var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = bow;
            equipMessage.Slot = EquipmentSlot.GraspBothExtremities;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var nymphAbility = manager.AbilitiesToAffectableRelationship[nymph.Id]
                .First(a => (a.Ability.Activation & ActivationType.Slottable) != 0
                    && !manager.SkillAbilitiesSystem.CanBeDefaultAttack(a.Ability));
            Assert.Equal(0, nymphAbility.Ability.Slot);

            var attackAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Single(a => a.Ability.Name == SkillAbilitiesSystem.PrimaryRangedAttackName);
            var activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = nymph.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = attackAbility;

            manager.Enqueue(activateAbilityMessage);

            var messageCount = 0;
            listener.ProcessMessage = m =>
            {
                if (m.TargetEntity == null)
                {
                    return;
                }

                messageCount++;
                Assert.Same(playerEntity, m.ActivatorEntity);
                Assert.Same(nymph, m.TargetEntity);
                Assert.Equal(new Point(2, 2), m.TargetCell);
            };

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);

            activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = elemental.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = attackAbility;

            manager.Enqueue(activateAbilityMessage);

            messageCount = 0;
            listener.ProcessMessage = m =>
            {
                if (m.TargetEntity == null)
                {
                    return;
                }

                messageCount++;
            };

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, messageCount);

            nymph.Being.HitPoints = nymph.Being.HitPointMaximum;
            var iceShardAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Single(a => a.Ability.Name == "ice shard");
            iceShardAbility.Ability.SuccessCondition = AbilitySuccessCondition.Default;

            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = iceShardAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);

            activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = nymph.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = iceShardAbility;
            manager.Enqueue(activateAbilityMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);
            Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);

            activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = nymph.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = iceShardAbility;
            manager.Enqueue(activateAbilityMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);
            Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);
        }
        
        [Fact]
        public void Can_use_toggleable_abilities()
        {
            var level = TestHelper.BuildLevel(".");
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player = playerEntity.Player;
            var manager = playerEntity.Manager;

            manager.Queue.ProcessQueue(manager);

            manager.XPSystem.AddPlayerXP(player.NextLevelXP * 3, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);
            var toggledAbility = manager.AbilitiesToAffectableRelationship[playerEntity.Id]
                .Single(a => (a.Ability.Activation & ActivationType.WhileToggled) != 0);
            var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 1;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Equal(1, toggledAbility.Ability.Slot);
            Assert.Null(toggledAbility.Ability.CooldownTick);
            Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Equal(1, toggledAbility.Ability.Slot);
            Assert.Equal(200, toggledAbility.Ability.CooldownTick);
            Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);

            player.NextAction = PlayerAction.Wait;
            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);

            player.NextAction = PlayerAction.Wait;
            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);

            player.NextAction = PlayerAction.Wait;
            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);

            manager.TimeSystem.EnqueueAdvanceTurn(manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.CooldownTick);
            Assert.Equal(200, manager.Game.CurrentTick);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.Slot);

            setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 1;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Equal(1, toggledAbility.Ability.Slot);
            Assert.Null(toggledAbility.Ability.CooldownTick);
            Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);
        }

        private class AbilityActivatedListener : IGameSystem<AbilityActivatedMessage>
        {
            public Action<AbilityActivatedMessage> ProcessMessage { get; set; }

            public MessageProcessingResult Process(AbilityActivatedMessage message, GameManager state)
            {
                ProcessMessage?.Invoke(message);

                return MessageProcessingResult.ContinueProcessing;
            }
        }
    }
}

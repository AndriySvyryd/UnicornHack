using System;
using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Faculties;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;
using Xunit;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivationSystemTest
    {
        private readonly static string TestMap = @"
.#.
.#.
...";

        [Fact]
        public void Projectile_can_only_hit_target_in_LOS()
        {
            var level = TestHelper.BuildLevel(TestMap);

            var manager = level.Entity.Manager;
            var listener = new AbilityActivatedListener();
            manager.Queue.Add(listener, AbilityActivationSystem.AbilityActivatedMessageName, -1);

            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(2, 0));
            player.Position.Heading = Direction.West;
            ItemData.Shortbow.Instantiate(player);
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(2, 2));
            var elemental = CreatureData.AirElemental.Instantiate(level, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            var bow = manager.EntityItemsToContainerRelationship[player.Id].Single();
            var equipMessage = manager.ItemUsageSystem.CreateEquipItemMessage(manager);
            equipMessage.ActorEntity = player;
            equipMessage.ItemEntity = bow;
            equipMessage.Slot = EquipmentSlot.GraspBothExtremities;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var nymphAbility = manager.AbilitiesToAffectableRelationship[nymph.Id]
                .First(a => (a.Ability.Activation & ActivationType.Slottable) != 0
                    && !manager.SkillAbilitiesSystem.CanBeDefaultAttack(a.Ability));
            Assert.Equal(0, nymphAbility.Ability.Slot);

            var attackAbility = manager.AbilitiesToAffectableRelationship[player.Id]
                .Single(a => a.Ability.Name == SkillAbilitiesSystem.PrimaryRangedAttackName);
            var activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = player;
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
                Assert.Same(player, m.ActivatorEntity);
                Assert.Same(nymph, m.TargetEntity);
                Assert.Equal(new Point(2, 2), m.TargetCell);
            };

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);

            manager.Queue.ProcessQueue(manager);

            activateAbilityMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
            activateAbilityMessage.ActivatorEntity = player;
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
        }
        
        [Fact]
        public void Toggleable_abilities_reserve_energy_points()
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

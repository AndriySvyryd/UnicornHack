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

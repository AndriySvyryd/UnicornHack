using System;
using System.Linq;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;
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
            manager.Queue.Add(listener, AbilityActivatedMessage.Name, -1);

            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(2, 0));
            playerEntity.Position.Heading = Direction.West;
            playerEntity.Player.SkillPoints = 5;
            manager.SkillAbilitiesSystem.BuyAbilityLevel(AbilityData.Conjuration, playerEntity);
            ItemData.Shortbow.Instantiate(playerEntity);
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(2, 2));
            var elemental = CreatureData.Sylph.Instantiate(level, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(100, playerEntity.Being.Accuracy);
            Assert.Equal(100, playerEntity.Being.Evasion);
            Assert.Equal(50, elemental.Being.Accuracy);
            Assert.Equal(75, elemental.Being.Evasion);

            var bow = manager.EntityItemsToContainerRelationship[playerEntity.Id].Single();
            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = bow;
            equipMessage.Slot = EquipmentSlot.GraspBothRanged;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);

            var nymphAbility = manager.AbilitiesToAffectableRelationship[nymph.Id]
                .First(a => (a.Ability.Activation & ActivationType.Slottable) != 0
                    && a.Ability.Template?.Type != AbilityType.DefaultAttack);
            Assert.Equal(0, nymphAbility.Ability.Slot);

            var attackAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.TwoHandedRangedAttack.Name)];

            var activateAbilityMessage = ActivateAbilityMessage.Create(manager);
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

            Assert.Equal(5, playerEntity.Player.NextActionTick);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);
            Assert.Equal(105, playerEntity.Player.NextActionTick);

            activateAbilityMessage = ActivateAbilityMessage.Create(manager);
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
            var iceShardAbility = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)];

            var accuracy = manager.AbilityActivationSystem.GetAccuracy(iceShardAbility.Ability, playerEntity);
            Assert.Equal(105, accuracy);

            Assert.Equal(34,
                manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, nymph, null, accuracy));

            Assert.Equal(100,
                manager.AbilityActivationSystem.GetEvasionProbability(iceShardAbility, playerEntity, elemental, null, accuracy));

            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = iceShardAbility;
            setSlotMessage.Slot = 0;
            manager.Enqueue(setSlotMessage);

            activateAbilityMessage = ActivateAbilityMessage.Create(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = nymph.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = iceShardAbility;
            manager.Enqueue(activateAbilityMessage);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(1, messageCount);
            Assert.Equal(1100, iceShardAbility.Ability.CooldownTick);

            activateAbilityMessage = ActivateAbilityMessage.Create(manager);
            activateAbilityMessage.ActivatorEntity = playerEntity;
            activateAbilityMessage.TargetCell = nymph.Position.LevelCell;
            activateAbilityMessage.AbilityEntity = iceShardAbility;
            manager.Enqueue(activateAbilityMessage);

            Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));

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
            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 1;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Equal(1, toggledAbility.Ability.Slot);
            Assert.Null(toggledAbility.Ability.CooldownTick);
            Assert.Equal(50, playerEntity.Being.ReservedEnergyPoints);

            setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Null(toggledAbility.Ability.Slot);
            Assert.Equal(200, toggledAbility.Ability.CooldownTick);
            Assert.Equal(0, playerEntity.Being.ReservedEnergyPoints);

            player.NextAction = PlayerAction.Wait;
            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);

            player.NextAction = PlayerAction.Wait;
            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);

            player.NextAction = PlayerAction.Wait;
            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);

            AdvanceTurnMessage.Enqueue(manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.CooldownTick);
            Assert.Equal(200, manager.Game.CurrentTick);

            setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.Slot);

            setSlotMessage = SetAbilitySlotMessage.Create(manager);
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

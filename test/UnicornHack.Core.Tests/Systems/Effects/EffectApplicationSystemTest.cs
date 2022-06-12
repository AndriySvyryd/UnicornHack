using System.Linq;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Effects
{
    public class EffectApplicationSystemTest
    {
        [Fact]
        public void Active_abilities_are_updated_when_contained_effects_change()
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

            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.False(playerEntity.Being.SlottedAbilities.ContainsKey(2));
            Assert.Equal(0, playerEntity.Being.AcidResistance);
            Assert.Equal(0, playerEntity.Being.ColdResistance);

            GameEntity acidEffectEntity;
            using (var effectEntityReference = manager.CreateEntity())
            {
                acidEffectEntity = effectEntityReference.Referenced;

                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.Duration = EffectDuration.Infinite;
                effect.EffectType = EffectType.ChangeProperty;
                effect.TargetName = nameof(BeingComponent.AcidResistance);
                effect.AppliedAmount = 10;
                effect.ContainingAbilityId = toggledAbility.Id;

                acidEffectEntity.Effect = effect;
            }

            TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(2, toggledAbility.Ability.Slot);
            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Same(toggledAbility, playerEntity.Being.SlottedAbilities[2]);
            Assert.Equal(10, playerEntity.Being.AcidResistance);
            Assert.Equal(0, playerEntity.Being.ColdResistance);

            using (var effectEntityReference = manager.CreateEntity())
            {
                var coldEffectEntity = effectEntityReference.Referenced;

                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.Duration = EffectDuration.Infinite;
                effect.EffectType = EffectType.ChangeProperty;
                effect.TargetName = nameof(BeingComponent.ColdResistance);
                effect.AppliedAmount = 10;
                effect.ContainingAbilityId = toggledAbility.Id;

                coldEffectEntity.Effect = effect;
            }

            acidEffectEntity.Effect.AppliedAmount = 20;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(20, playerEntity.Being.AcidResistance);
            Assert.Equal(10, playerEntity.Being.ColdResistance);

            acidEffectEntity.Effect = null;

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, playerEntity.Being.AcidResistance);
            Assert.Equal(10, playerEntity.Being.ColdResistance);

            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Equal(0, playerEntity.Being.AcidResistance);
            Assert.Equal(0, playerEntity.Being.ColdResistance);
        }

        [Fact]
        public void Properties_are_updated_when_races_change()
        {
            var level = TestHelper.BuildLevel(".");
            var manager = level.Entity.Manager;
            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

            manager.Queue.ProcessQueue(manager);

            ItemData.PotionOfDwarfness.Instantiate(playerEntity);
            ItemData.PotionOfElfness.Instantiate(playerEntity);
            ItemData.PotionOfOgreness.Instantiate(playerEntity);
            ItemData.PotionOfInhumanity.Instantiate(playerEntity);

            manager.Queue.ProcessQueue(manager);

            Assert.Equal(6, playerEntity.Physical.Size);
            //Assert.Equal(1000, playerEntity.Physical.Weight);
            Assert.Equal(10, playerEntity.Being.Speed);
            Assert.Equal(10, playerEntity.Being.Might);
            Assert.Equal(10, playerEntity.Being.Perception);
            Assert.Equal(10, playerEntity.Being.Focus);

            TestHelper.ActivateAbility(ItemData.PotionOfElfness.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);

            TestHelper.ActivateAbility(ItemData.PotionOfDwarfness.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);

            TestHelper.ActivateAbility(ItemData.PotionOfOgreness.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);

            TestHelper.ActivateAbility(ItemData.PotionOfInhumanity.Name + ": Drink", playerEntity, manager);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(7, playerEntity.Physical.Size);
            //Assert.Equal(1000, playerEntity.Physical.Weight);
            Assert.Equal(7, playerEntity.Being.Speed);
            Assert.Equal(13, playerEntity.Being.Might);
            Assert.Equal(8, playerEntity.Being.Perception);
            Assert.Equal(9, playerEntity.Being.Focus);
        }
    }
}

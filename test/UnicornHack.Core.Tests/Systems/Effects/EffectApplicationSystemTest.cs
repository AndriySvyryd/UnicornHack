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

            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Null(manager.SlottedAbilitiesIndex[(playerEntity.Id, 0)]);
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

            var setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;
            setSlotMessage.Slot = 0;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(0, toggledAbility.Ability.Slot);
            Assert.True(toggledAbility.Ability.IsActive);
            Assert.Same(toggledAbility, manager.SlottedAbilitiesIndex[(playerEntity.Id, 0)]);
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

            setSlotMessage = SetAbilitySlotMessage.Create(manager);
            setSlotMessage.AbilityEntity = toggledAbility;

            manager.Enqueue(setSlotMessage);
            manager.Queue.ProcessQueue(manager);

            Assert.Null(toggledAbility.Ability.Slot);
            Assert.False(toggledAbility.Ability.IsActive);
            Assert.Equal(0, playerEntity.Being.AcidResistance);
            Assert.Equal(0, playerEntity.Being.ColdResistance);
        }
    }
}

using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Faculties;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public UniqueEntityRelationship<GameEntity> BeingToPrimaryNaturalWeaponRelationship { get; private set; }
        public UniqueEntityRelationship<GameEntity> BeingToSecondaryNaturalWeaponRelationship { get; private set; }
        public SkillAbilitiesSystem SkillAbilitiesSystem { get; private set; }

        private void InitializeFaculties(SequentialMessageQueue<GameManager> queue)
        {
            BeingToPrimaryNaturalWeaponRelationship = new UniqueEntityRelationship<GameEntity>(
                nameof(BeingToPrimaryNaturalWeaponRelationship),
                Beings,
                EntityItems,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((BeingComponent)component).PrimaryNaturalWeaponId,
                    (int)EntityComponent.Being),
                (effectEntity, _, __) => effectEntity.Being.PrimaryNaturalWeaponId = null,
                referencedKeepAlive: true,
                referencingKeepAlive: false);

            BeingToSecondaryNaturalWeaponRelationship = new UniqueEntityRelationship<GameEntity>(
                nameof(BeingToSecondaryNaturalWeaponRelationship),
                Beings,
                EntityItems,
                new SimpleKeyValueGetter<GameEntity, int>(
                    component => ((BeingComponent)component).SecondaryNaturalWeaponId,
                    (int)EntityComponent.Being),
                (effectEntity, _, __) => effectEntity.Being.SecondaryNaturalWeaponId = null,
                referencedKeepAlive: true,
                referencingKeepAlive: false);

            SkillAbilitiesSystem = new SkillAbilitiesSystem();
            queue.Add<ItemEquippedMessage>(SkillAbilitiesSystem, ItemEquippedMessage.Name, 4);
            queue.Add<PropertyValueChangedMessage<GameEntity, ExtremityType>>(SkillAbilitiesSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.UpperExtremities)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.HandWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.ShortWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.MediumWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LongWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.CloseRangeWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.ShortRangeWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.MediumRangeWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LongRangeWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LightArmor)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.HeavyArmor)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Artifice)), 0);
        }
    }
}

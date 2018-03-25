using UnicornHack.Primitives;
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
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((BeingComponent)component).PrimaryNaturalWeaponId,
                    (int)EntityComponent.Being),
                (effectEntity, _, __, ___) => effectEntity.Being.PrimaryNaturalWeaponId = null,
                referencedKeepAlive: true,
                referencingKeepAlive: false);

            BeingToSecondaryNaturalWeaponRelationship = new UniqueEntityRelationship<GameEntity>(
                nameof(BeingToSecondaryNaturalWeaponRelationship),
                Beings,
                EntityItems,
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((BeingComponent)component).SecondaryNaturalWeaponId,
                    (int)EntityComponent.Being),
                (effectEntity, _, __, ___) => effectEntity.Being.SecondaryNaturalWeaponId = null,
                referencedKeepAlive: true,
                referencingKeepAlive: false);

            SkillAbilitiesSystem = new SkillAbilitiesSystem();
            queue.Add<ItemEquippedMessage>(SkillAbilitiesSystem, ItemUsageSystem.ItemEquippedMessageName, 4);
            queue.Add<PropertyValueChangedMessage<GameEntity, ExtremityType>>(SkillAbilitiesSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.UpperExtremeties)), 0);
        }
    }
}

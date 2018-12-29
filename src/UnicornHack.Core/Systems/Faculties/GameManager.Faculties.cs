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
            queue.Add<ItemEquippedMessage>(SkillAbilitiesSystem, ItemUsageSystem.ItemEquippedMessageName, 4);
            queue.Add<PropertyValueChangedMessage<GameEntity, ExtremityType>>(SkillAbilitiesSystem,
                Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.UpperExtremeties)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.OneHanded)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.TwoHanded)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.DualWielding)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.FistWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.ShortWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.MediumWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LongWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.ThrownWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Slingshots)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Bows)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Crossbows)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Armorless)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LightArmor)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.HeavyArmor)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Stealth)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Assessination)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.MeleeMagicWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.RangedMagicWeapons)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.FireSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.AirSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.WaterSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.EarthSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.LifeSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.SpiritSourcery)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Evocation)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Conjuration)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Transmutation)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Enhancement)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Malediction)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Illusion)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Artifice)), 0);
            queue.Add<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
                Players.GetPropertyValueChangedMessageName(nameof(PlayerComponent.Leadership)), 0);
        }
    }
}

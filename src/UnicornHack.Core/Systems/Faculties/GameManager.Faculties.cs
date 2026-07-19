using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Faculties;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack;

public partial class GameManager
{
    public UniqueEntityRelationship<GameEntity> BeingToPrimaryNaturalWeaponRelationship
    {
        get;
        private set;
    } = null!;

    public UniqueEntityRelationship<GameEntity> BeingToSecondaryNaturalWeaponRelationship
    {
        get;
        private set;
    } = null!;

    public SkillAbilitiesSystem SkillAbilitiesSystem
    {
        get;
        private set;
    } = null!;

    private void InitializeFaculties(SequentialMessageQueue<GameManager> queue)
    {
        BeingToPrimaryNaturalWeaponRelationship = new(
            nameof(BeingToPrimaryNaturalWeaponRelationship),
            Beings,
            RoomItems,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((BeingComponent)component).PrimaryNaturalWeaponId,
                (int)EntityComponent.Being),
            (effectEntity, _) => effectEntity.Being!.PrimaryNaturalWeaponId = null,
            keepPrincipalAlive: true,
            keepDependentAlive: false);

        BeingToSecondaryNaturalWeaponRelationship = new(
            nameof(BeingToSecondaryNaturalWeaponRelationship),
            Beings,
            RoomItems,
            new SimpleKeyValueGetter<GameEntity, int>(
                component => ((BeingComponent)component).SecondaryNaturalWeaponId,
                (int)EntityComponent.Being),
            (effectEntity, _) => effectEntity.Being!.SecondaryNaturalWeaponId = null,
            keepPrincipalAlive: true,
            keepDependentAlive: false);

        SkillAbilitiesSystem = new SkillAbilitiesSystem();
        queue.Register<ItemEquippedMessage>(SkillAbilitiesSystem, ItemEquippedMessage.Name, 4);
        queue.Register<PropertyValueChangedMessage<GameEntity, ExtremityType>>(SkillAbilitiesSystem,
            Beings.GetPropertyValueChangedMessageName(nameof(BeingComponent.UpperExtremities)), 0);
        queue.Register<PropertyValueChangedMessage<GameEntity, int>>(SkillAbilitiesSystem,
            AbilitiesToAffectableRelationship.Dependents.GetPropertyValueChangedMessageName(
                nameof(AbilityComponent.Level)),
            0);
    }
}

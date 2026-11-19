using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Services;

public interface ILanguageService
{
    string GetPropertyName(string propertyName, bool abbreviate);

    [return: NotNullIfNotNull("actorEntity")]
    string? GetActorName(GameEntity? actorEntity, bool isIdentified);
    string GetDescription(string id, DescriptionCategory category);
    string GetString(ItemComponent item, int quantity, bool isIdentified);
    string GetTemplateName(AbilityComponent race, bool abbreviate);
    string GetString(AbilityComponent ability);
    string GetString(Direction direction, bool abbreviate);
    string GetString(EquipmentSlot slot, GameEntity actorEntity, bool abbreviate);
    string GetString(in ItemPickUpEvent @event);
    string GetString(in ItemDropEvent @event);
    string GetString(in ItemEquipmentEvent @event);
    string GetString(in ItemActivationEvent @event);
    string? GetString(in AttackEvent @event);
    string? GetString(in ActorActionFailedEvent @event);
    string GetString(in DeathEvent @event);
    string GetString(in StatusActivatedEvent @event);
    string GetString(in LingeringAttackEvent @event);
    string GetString(in ResourceEffectEvent @event);
    string NoDefaultAttack(bool melee);
    string NoAbilityInSlot(int slot);
    string AbilityNotSlotted(AbilityComponent ability);
    string AbilityAlreadyActive(AbilityComponent ability);
    string AbilityOnCooldown(AbilityComponent ability);
    string AbilityNotUsable(AbilityComponent ability);
    string CannotCastAbility(AbilityComponent ability);
    string CannotUseVerbalAbility(AbilityComponent ability);
    string CannotUseDivineAbility(AbilityComponent ability);
    string CannotUseDefaultAttack(AbilityComponent ability);
    string CannotUseNonDefaultAbility(AbilityComponent ability);
    string CannotUseComplexAbility(AbilityComponent ability);
    string CannotUseNonAttackAbility(AbilityComponent ability);
    string InvalidDigestionTarget(AbilityComponent ability);
    string AbilityHasNoEffects(AbilityComponent ability);
    string NotEnoughEnergy(AbilityComponent ability);
    string TooSlowToUseAbility(AbilityComponent ability);
    string AbilityRequiresTarget(AbilityComponent ability);
    string CannotTurnToUseAbility(AbilityComponent ability);
    string StatusPreventsAction(AbilityComponent statusAbility);
    string StatusPreventsAttemptedAction(AbilityComponent statusAbility);
    string StatusPreventsMovement(AbilityComponent statusAbility);
    string StatusPreventsItemManipulation(AbilityComponent statusAbility);
    string StatusPreventsAbility(AbilityComponent statusAbility, AbilityComponent ability);
    string ItemNotInInventory();
    string InventoryFull();
    string ItemDroppedBecauseInventoryFull(ItemComponent item, int quantity);
    string NothingToPickUp();
    string CannotMove(Direction direction);
    string Welcome(GameEntity playerEntity);
    string NoPath();
    string TooAfraidToApproach(GameEntity source);
    string CannotFleeSource(GameEntity source);
    string SoundFrom(Direction direction);
    string PlayerChoiceRequired();
    string InvalidPlayerChoiceSelection();
    string PlayerChoiceApplicationFailed();

    string PlayerAbilityLevelChanged(
        string abilityName,
        AbilityType abilityType,
        int previousLevel,
        int currentLevel);

    string FeatureDestroyed(string featureName, string? rewardName, RewardType rewardType);
    string RewardGranted(string rewardName);
    string GetPlayerChoiceTitle(RewardType choice, string? action = null, string? featureName = null);
    string GetPlayerChoiceOptionTitle(RewardType choice, string? action = null, string? option = null);
    string GetPlayerChoiceOptionDescription(RewardType choice, string? action = null, string? option = null);
    string GetEffectDurationDescription(EffectDuration duration, string? amount = null);
    string NoAvailableRaces();
    string NoRaceToReplace();

    /// <summary>
    ///     Composes the localized hover-tooltip for a single room cell on the map screen.
    ///     Server-side composition keeps tooltip wording consistent with the rest of the
    ///     localized payload and lets translators provide alternative phrasing without
    ///     touching the client.
    /// </summary>
    /// <param name="features">Active <see cref="RoomFeatures" /> on the room.</param>
    /// <param name="isCurrent">Whether this is the player's current room.</param>
    /// <param name="isVisited">Whether the player has visited this room.</param>
    /// <param name="revealedActorCount">
    ///     Revealed creature count, or <c>-1</c> when not revealed at the player's current
    ///     closeness to the room.
    /// </param>
    /// <param name="eliteName">Localized elite identity when room details can be perceived; otherwise <c>null</c>.</param>
    string GetRoomTooltip(
        RoomFeatures features,
        bool isCurrent,
        bool isVisited,
        int revealedActorCount,
        string? eliteName);
}

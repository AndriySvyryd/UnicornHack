using UnicornHack.Utils;

namespace UnicornHack.Systems.Effects;

/// <summary>
///     Common persisted state for a lingering property modifier or structural effect record owned by an
///     <see cref="ActiveEffectsComponent" />.
/// </summary>
public abstract class ActiveEffect : NotificationCollectionItem
{
    private int _gameId;
    private int _entityId;
    private int _instanceId;
    private EffectType _effectType;
    private string? _targetName;
    private int? _expirationTarget;
    private EffectDuration? _expirationEvent;

    protected ActiveEffect(
        int gameId, int entityId, int instanceId, EffectType effectType, string? targetName)
    {
        GameId = gameId;
        EntityId = entityId;
        InstanceId = instanceId;
        EffectType = effectType;
        TargetName = targetName;
    }

    /// <summary>Id of the game containing the owning <see cref="ActiveEffectsComponent" />.</summary>
    public int GameId
    {
        get => _gameId;
        private set => SetWithNotify(value, ref _gameId);
    }

    /// <summary>Id of the entity that owns this effect through its <see cref="ActiveEffectsComponent" />.</summary>
    public int EntityId
    {
        get => _entityId;
        private set => SetWithNotify(value, ref _entityId);
    }

    /// <summary>Stable identifier within the owning <see cref="ActiveEffectsComponent" />.</summary>
    public int InstanceId
    {
        get => _instanceId;
        private set => SetWithNotify(value, ref _instanceId);
    }

    public EffectType EffectType
    {
        get => _effectType;
        private set
        {
            SetWithNotify(value, ref _effectType);
        }
    }

    /// <summary>
    ///     The affected property or ability name, or <c>null</c> when the effect is a marker or targets an
    ///     ability category through a condition value.
    /// </summary>
    public string? TargetName
    {
        get => _targetName;
        private set => SetWithNotify(value, ref _targetName);
    }

    /// <summary>
    ///     The absolute game tick for <see cref="EffectDuration.UntilTimeout" /> or remaining XP for
    ///     <see cref="EffectDuration.UntilXPGained" />; null for transition and permanent effects.
    /// </summary>
    public int? ExpirationTarget
    {
        get => _expirationTarget;
        set => SetWithNotify(value, ref _expirationTarget);
    }

    /// <summary>
    ///     How this record expires. Null denotes a permanent record; active records never store
    ///     <see cref="EffectDuration.Instant" /> or <see cref="EffectDuration.DuringApplication" />.
    /// </summary>
    public EffectDuration? ExpirationEvent
    {
        get => _expirationEvent;
        set => SetWithNotify(value, ref _expirationEvent);
    }

    protected virtual void OnEffectTypeChanged(EffectType effectType)
    {
    }
}

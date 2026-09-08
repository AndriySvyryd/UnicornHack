namespace UnicornHack.Systems.Effects;

public class EffectAccumulator : PooledTrackableCollectionItem
{
    private int _gameId;
    private int _entityId;
    private EffectType _effectType;
    private int _value;

    internal void Initialize(int gameId, int entityId, EffectType effectType)
    {
        if (IsTracked || ContainingComponent != null)
        {
            throw new InvalidOperationException("A tracked or attached effect accumulator cannot be initialized.");
        }

        _gameId = gameId;
        _entityId = entityId;
        _effectType = effectType;
    }

    public int GameId
    {
        get => _gameId;
        set => SetWithNotify(value, ref _gameId);
    }

    public int EntityId
    {
        get => _entityId;
        set => SetWithNotify(value, ref _entityId);
    }

    public EffectType EffectType
    {
        get => _effectType;
        set => SetWithNotify(value, ref _effectType);
    }

    public int Value
    {
        get => _value;
        set => SetWithNotify(value, ref _value);
    }

    protected override void Reset()
    {
        _gameId = default;
        _entityId = default;
        _effectType = default;
        _value = default;
    }
}

using UnicornHack.Utils;

namespace UnicornHack.Systems.Effects;

public class EffectAccumulator : NotificationCollectionItem
{
    private int _gameId;
    private int _entityId;
    private EffectType _effectType;
    private int _value;

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
}

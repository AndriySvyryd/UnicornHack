namespace UnicornHack.Systems.Effects;

public sealed class AttackResultMessage : EffectResultMessage
{
    public const string Name = "AttackResult";

    public static AttackResultMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<AttackResultMessage>(Name);

    public override string MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public override void ReturnMessage(GameManager manager)
        => manager.ReturnMessage(this);

    private GameEntity? _weaponEntity;

    public GameEntity? WeaponEntity
    {
        get => _weaponEntity;
        set
        {
            _weaponEntity?.RemoveReference(this);
            _weaponEntity = value;
            _weaponEntity?.AddReference(this);
        }
    }

    public AbilityAction AbilityAction
    {
        get;
        set;
    }

    public bool Ranged
    {
        get;
        set;
    }

    public bool Hit
    {
        get;
        set;
    }

    public bool RevealsAttacker
    {
        get;
        set;
    }

    public Dictionary<EffectType, int> LingeringEffects
    {
        get;
    } = [];

    public override void Clean()
    {
        WeaponEntity = default;
        AbilityAction = default;
        Ranged = default;
        Hit = default;
        RevealsAttacker = default;
        LingeringEffects.Clear();
        base.Clean();
    }
}

namespace UnicornHack.Systems.Effects;

public sealed class ItemActivationResultMessage : EffectResultMessage
{
    public const string Name = "ItemActivationResult";

    public static ItemActivationResultMessage Create(GameManager manager)
        => manager.Queue.CreateMessage<ItemActivationResultMessage>(Name);

    public override string MessageName
    {
        get => Name;
        set => Debug.Assert(value == Name);
    }

    public override void ReturnMessage(GameManager manager)
        => manager.ReturnMessage(this);

    private GameEntity? _itemEntity;

    internal bool ItemConsumed
    {
        get;
        set;
    }

    public GameEntity ItemEntity
    {
        get => _itemEntity!;
        set
        {
            _itemEntity?.RemoveReference(this);
            _itemEntity = value;
            _itemEntity?.AddReference(this);
        }
    }

    public bool Successful
    {
        get;
        set;
    }

    public override void Clean()
    {
        ItemEntity = default!;
        ItemConsumed = default;
        Successful = default;
        base.Clean();
    }
}

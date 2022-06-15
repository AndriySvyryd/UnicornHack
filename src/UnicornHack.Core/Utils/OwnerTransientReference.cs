namespace UnicornHack.Utils;

public struct OwnerTransientReference<T, TOwner> : ITransientReference<T>
    where T : class, IOwnerReferenceable
{
    public OwnerTransientReference(T referenced, TOwner owner)
    {
        referenced.AddReference(owner);
        Referenced = referenced;
        Owner = owner;
    }

    public T Referenced
    {
        get;
        private set;
    }

    private TOwner Owner
    {
        get;
        set;
    }

    public void Dispose()
    {
        if (Referenced != null)
        {
            Referenced.RemoveReference(Owner);
            Referenced = null;
            Owner = default;
        }
    }
}

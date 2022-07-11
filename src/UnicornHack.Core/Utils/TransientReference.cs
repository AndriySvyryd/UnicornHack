namespace UnicornHack.Utils;

public struct TransientReference<T, TOwner> : ITransientReference<T>
    where T : class, IReferenceable
    where TOwner : notnull
{
    public TransientReference(T referenced, TOwner owner)
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
        if (Referenced != default)
        {
            Referenced.RemoveReference(Owner);
            Referenced = default!;
            Owner = default!;
        }
    }
}

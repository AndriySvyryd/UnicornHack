namespace UnicornHack.Utils
{
    public interface IOwnerReferenceable
    {
        void AddReference(object owner);
        void RemoveReference(object owner);
    }
}

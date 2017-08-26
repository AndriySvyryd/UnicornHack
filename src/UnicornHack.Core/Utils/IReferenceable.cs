namespace UnicornHack.Utils
{
    // TODO: Replace with garbage collection
    public interface IReferenceable
    {
        void AddReference();
        void RemoveReference();
    }
}
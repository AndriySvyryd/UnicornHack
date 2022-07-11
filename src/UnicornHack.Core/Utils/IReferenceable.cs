namespace UnicornHack.Utils;

public interface IReferenceable
{
    void AddReference(object owner);
    void RemoveReference(object owner);
}

namespace UnicornHack.Generation;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ComponentAttribute : Attribute
{
    public int Id
    {
        get;
        set;
    }
}

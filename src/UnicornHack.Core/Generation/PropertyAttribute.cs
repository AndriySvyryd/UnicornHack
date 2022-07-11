using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation;

/// <summary>
///     Indicates a property that can have its value affected by an <see cref="EffectComponent" />
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyAttribute : Attribute
{
    public bool IsCalculated
    {
        get;
        set;
    } = true;

    public object? MinValue
    {
        get;
        set;
    }

    public object? MaxValue
    {
        get;
        set;
    }

    public object? DefaultValue
    {
        get;
        set;
    }
}

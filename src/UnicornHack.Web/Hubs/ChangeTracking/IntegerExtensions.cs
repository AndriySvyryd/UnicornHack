using System.Numerics;

namespace UnicornHack.Hubs.ChangeTracking;

public static class IntegerExtensions
{
    public static bool IsPropertyChanged(this byte? mask, int propertyIndex)
        => !mask.HasValue || (mask.GetValueOrDefault() & (1 << propertyIndex)) != 0;

    public static bool IsPropertyChanged(this ushort? mask, int propertyIndex)
        => !mask.HasValue || (mask.GetValueOrDefault() & (1 << propertyIndex)) != 0;

    public static bool IsPropertyChanged(this uint? mask, int propertyIndex)
        => !mask.HasValue || (mask.GetValueOrDefault() & (1u << propertyIndex)) != 0;

    public static bool IsPropertyChanged(this ulong? mask, int propertyIndex)
        => !mask.HasValue || (mask.GetValueOrDefault() & (1ul << propertyIndex)) != 0;

    public static int GetSerializedLength<T>(this T? changedProperties, int totalProperties)
        where T : struct, IBinaryInteger<T>
    {
        if (!changedProperties.HasValue)
        {
            return totalProperties;
        }

        var mask = changedProperties.GetValueOrDefault();
        var count = int.CreateChecked(T.PopCount(mask));
        return (mask & T.One) == T.Zero ? Math.Max(1, count + 1) : count;
    }
}
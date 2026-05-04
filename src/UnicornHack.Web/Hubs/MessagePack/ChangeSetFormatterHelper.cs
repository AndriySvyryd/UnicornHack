using System.Collections;

namespace UnicornHack.Hubs.MessagePack;

public static class ChangeSetFormatterHelper
{
    public static int GetSerializedLength(BitArray? changedProperties, int totalProperties)
    {
        if (changedProperties == null)
        {
            return totalProperties;
        }

        var count = 0;
        for (var i = 0; i < changedProperties.Length; i++)
        {
            if (changedProperties[i])
            {
                count++;
            }
        }

        if (!changedProperties[0])
        {
            return Math.Max(1, count + 1);
        }

        return count;
    }
}

using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class EffectChangeFormatter : IMessagePackFormatter<EffectChange?>
{
    public static readonly EffectChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, EffectChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, EffectChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values == null || values[1])
        {
            writer.Write((int)value.EffectType);
        }

        if (values == null || values[2])
        {
            writer.Write(value.ExpirationTick);
        }

        if (values == null || values[3])
        {
            writer.Write(value.Magnitude);
        }

        if (values == null || values[4])
        {
            writer.Write(value.InForce);
        }
    }

    public EffectChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new EffectChange();
        BitArray? values = null;

        for (var i = 0; i < length; i++)
        {
            if (values != null && !values[i])
            {
                continue;
            }

            switch (i)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options);
                    if (values != null)
                    {
                        length = values.Length;
                    }

                    result.ChangedProperties = values;
                    break;
                case 1:
                    result.EffectType = (EffectType)reader.ReadInt32();
                    break;
                case 2:
                    result.ExpirationTick = reader.ReadInt32();
                    break;
                case 3:
                    result.Magnitude = reader.ReadInt32();
                    break;
                case 4:
                    result.InForce = reader.ReadBoolean();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

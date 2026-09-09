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
        var length = values.GetSerializedLength(EffectChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<byte?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values.IsPropertyChanged(1))
        {
            writer.Write((int)value.EffectType);
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.ExpirationTick);
        }

        if (values.IsPropertyChanged(3))
        {
            writer.Write(value.Magnitude);
        }

        if (values.IsPropertyChanged(4))
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
        byte? values = null;

        for (var i = 0; i < length; i++)
        {
            if (values.HasValue && !values.IsPropertyChanged(i))
            {
                continue;
            }

            switch (i)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<byte?>().Deserialize(ref reader, options);
                    if (values.HasValue)
                    {
                        length = values.GetValueOrDefault() == 0 ? 1 : EffectChange.PropertyCount;
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

using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class ConnectionChangeFormatter : IMessagePackFormatter<ConnectionChange?>
{
    public static readonly ConnectionChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, ConnectionChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, ConnectionChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values == null || values[1])
        {
            writer.Write(value.LevelX);
        }

        if (values == null || values[2])
        {
            writer.Write(value.LevelY);
        }

        if (values == null || values[3])
        {
            writer.Write(value.IsDown);
        }
    }

    public ConnectionChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new ConnectionChange();
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
                    result.LevelX = reader.ReadInt32();
                    break;
                case 2:
                    result.LevelY = reader.ReadInt32();
                    break;
                case 3:
                    result.IsDown = reader.ReadBoolean();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

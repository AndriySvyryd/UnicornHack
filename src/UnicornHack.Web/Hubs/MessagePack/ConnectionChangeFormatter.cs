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
        var length = values.GetSerializedLength(ConnectionChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<byte?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values.IsPropertyChanged(1))
        {
            writer.Write(value.LevelX);
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.LevelY);
        }

        if (values.IsPropertyChanged(3))
        {
            writer.Write((byte)value.Orientation);
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
                        length = values.GetValueOrDefault() == 0 ? 1 : ConnectionChange.PropertyCount;
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
                    result.Orientation = (Direction)reader.ReadByte();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

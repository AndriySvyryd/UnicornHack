using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class ItemChangeFormatter : IMessagePackFormatter<ItemChange?>
{
    public static readonly ItemChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, ItemChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, ItemChange.PropertyCount);
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
            writer.Write((int)value.Type);
        }

        if (values == null || values[4])
        {
            writer.Write(value.BaseName ?? "");
        }

        if (values == null || values[5])
        {
            writer.Write(value.Name ?? "");
        }

        if (values == null || values[6])
        {
            writer.Write(value.IsCurrentlyPerceived);
        }
    }

    public ItemChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new ItemChange();
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
                    result.Type = (ItemType)reader.ReadInt32();
                    break;
                case 4:
                    result.BaseName = reader.ReadString();
                    break;
                case 5:
                    result.Name = reader.ReadString();
                    break;
                case 6:
                    result.IsCurrentlyPerceived = reader.ReadBoolean();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class RoomChangeFormatter : IMessagePackFormatter<RoomChange?>
{
    public static readonly RoomChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, RoomChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, RoomChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (values == null || values[1])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, ActorChange>?>()
                .Serialize(ref writer, value.Actors, options);
        }

        if (values == null || values[2])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, ItemChange>?>()
                .Serialize(ref writer, value.Items, options);
        }

        if (values == null || values[3])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, ConnectionChange>?>()
                .Serialize(ref writer, value.Connections, options);
        }

        if (values == null || values[4])
        {
            resolver.GetFormatterWithVerify<RoomMap?>().Serialize(ref writer, value.LevelMap, options);
        }

        if (values == null || values[5])
        {
            resolver.GetFormatterWithVerify<RoomMapChanges?>().Serialize(ref writer, value.LevelMapChanges, options);
        }

        if (values == null || values[6])
        {
            writer.Write(value.BranchName ?? "");
        }

        if (values == null || values[7])
        {
            writer.Write(value.Depth);
        }

        if (values == null || values[8])
        {
            writer.Write(value.Width);
        }

        if (values == null || values[9])
        {
            writer.Write(value.Height);
        }

        if (values == null || values[10])
        {
            writer.Write(value.GridX);
        }

        if (values == null || values[11])
        {
            writer.Write(value.GridY);
        }
    }

    public RoomChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new RoomChange();
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
                    result.Actors = resolver.GetFormatterWithVerify<Dictionary<int, ActorChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 2:
                    result.Items = resolver.GetFormatterWithVerify<Dictionary<int, ItemChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 3:
                    result.Connections = resolver.GetFormatterWithVerify<Dictionary<int, ConnectionChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 4:
                    result.LevelMap = resolver.GetFormatterWithVerify<RoomMap?>()
                        .Deserialize(ref reader, options);
                    break;
                case 5:
                    result.LevelMapChanges = resolver.GetFormatterWithVerify<RoomMapChanges?>()
                        .Deserialize(ref reader, options);
                    break;
                case 6:
                    result.BranchName = reader.ReadString();
                    break;
                case 7:
                    result.Depth = reader.ReadInt32();
                    break;
                case 8:
                    result.Width = reader.ReadInt32();
                    break;
                case 9:
                    result.Height = reader.ReadInt32();
                    break;
                case 10:
                    result.GridX = reader.ReadByte();
                    break;
                case 11:
                    result.GridY = reader.ReadByte();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

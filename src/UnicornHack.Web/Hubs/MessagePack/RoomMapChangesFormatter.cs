using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class RoomMapChangesFormatter : IMessagePackFormatter<RoomMapChanges?>
{
    public static readonly RoomMapChangesFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, RoomMapChanges? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, RoomMapChanges.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (values == null || values[1])
        {
            resolver.GetFormatterWithVerify<List<(short, MapFeature)>?>()
                .Serialize(ref writer, value.TerrainChanges, options);
        }

        if (values == null || values[2])
        {
            resolver.GetFormatterWithVerify<List<(short, DirectionFlags)>?>()
                .Serialize(ref writer, value.WallNeighborsChanges, options);
        }

        if (values == null || values[3])
        {
            resolver.GetFormatterWithVerify<List<(short, byte)>?>()
                .Serialize(ref writer, value.VisibleTerrainChanges, options);
        }
    }

    public RoomMapChanges? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var value = new RoomMapChanges();
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

                    value.ChangedProperties = values;
                    break;
                case 1:
                    value.TerrainChanges = resolver.GetFormatterWithVerify<List<(short, MapFeature)>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 2:
                    value.WallNeighborsChanges = resolver.GetFormatterWithVerify<List<(short, DirectionFlags)>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 3:
                    value.VisibleTerrainChanges = resolver.GetFormatterWithVerify<List<(short, byte)>?>()
                        .Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return value;
    }
}

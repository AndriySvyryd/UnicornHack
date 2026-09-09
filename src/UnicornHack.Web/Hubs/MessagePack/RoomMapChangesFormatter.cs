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
        var length = values.GetSerializedLength(RoomMapChanges.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<byte?>().Serialize(ref writer, values, options);
        if (values.IsPropertyChanged(1))
        {
            resolver.GetFormatterWithVerify<List<(short, MapFeature)>?>()
                .Serialize(ref writer, value.TerrainChanges, options);
        }

        if (values.IsPropertyChanged(2))
        {
            resolver.GetFormatterWithVerify<List<(short, DirectionFlags)>?>()
                .Serialize(ref writer, value.WallNeighborsChanges, options);
        }

        if (values.IsPropertyChanged(3))
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
                        length = values.GetValueOrDefault() == 0 ? 1 : RoomMapChanges.PropertyCount;
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

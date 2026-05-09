using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class RoomMapFormatter : IMessagePackFormatter<RoomMap?>
{
    public static readonly RoomMapFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, RoomMap? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        writer.WriteArrayHeader(3);
        resolver.GetFormatterWithVerify<MapFeature[]>().Serialize(ref writer, value.Terrain, options);
        resolver.GetFormatterWithVerify<DirectionFlags[]>().Serialize(ref writer, value.WallNeighbors, options);
        resolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.VisibleTerrain, options);
    }

    public RoomMap? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var count = reader.ReadArrayHeader();
        var value = new RoomMap();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    value.Terrain = resolver.GetFormatterWithVerify<MapFeature[]>().Deserialize(ref reader, options);
                    break;
                case 1:
                    value.WallNeighbors = resolver.GetFormatterWithVerify<DirectionFlags[]>().Deserialize(ref reader, options);
                    break;
                case 2:
                    value.VisibleTerrain = resolver.GetFormatterWithVerify<byte[]>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return value;
    }
}

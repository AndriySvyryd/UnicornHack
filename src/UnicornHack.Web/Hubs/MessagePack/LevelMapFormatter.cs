using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class LevelMapFormatter : IMessagePackFormatter<LevelMap?>
{
    public static readonly LevelMapFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, LevelMap? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        writer.WriteArrayHeader(3);
        resolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.Terrain, options);
        resolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.WallNeighbors, options);
        resolver.GetFormatterWithVerify<byte[]>().Serialize(ref writer, value.VisibleTerrain, options);
    }

    public LevelMap? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var count = reader.ReadArrayHeader();
        var value = new LevelMap();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    value.Terrain = resolver.GetFormatterWithVerify<byte[]>().Deserialize(ref reader, options);
                    break;
                case 1:
                    value.WallNeighbors = resolver.GetFormatterWithVerify<byte[]>().Deserialize(ref reader, options);
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

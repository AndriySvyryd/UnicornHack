using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class RoomEntryFormatter : IMessagePackFormatter<RoomEntry?>
{
    public static readonly RoomEntryFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, RoomEntry? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        writer.WriteArrayHeader(10);
        writer.Write(value.Id);
        writer.Write(value.GridX);
        writer.Write(value.GridY);
        writer.Write((int)value.Features);
        writer.Write(value.IsVisited);
        resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.ConnectedTo, options);
        writer.Write(value.RevealedActorCount);
        writer.Write(value.RevealedItemCount);
        resolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.StrongestActorName, options);
        resolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.Tooltip, options);
    }

    public RoomEntry? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var count = reader.ReadArrayHeader();
        var value = new RoomEntry();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    value.Id = reader.ReadInt32();
                    break;
                case 1:
                    value.GridX = reader.ReadByte();
                    break;
                case 2:
                    value.GridY = reader.ReadByte();
                    break;
                case 3:
                    value.Features = (RoomFeatures)reader.ReadInt32();
                    break;
                case 4:
                    value.IsVisited = reader.ReadBoolean();
                    break;
                case 5:
                    value.ConnectedTo = resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);
                    break;
                case 6:
                    value.RevealedActorCount = reader.ReadInt32();
                    break;
                case 7:
                    value.RevealedItemCount = reader.ReadInt32();
                    break;
                case 8:
                    value.StrongestActorName = resolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, options);
                    break;
                case 9:
                    value.Tooltip = resolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return value;
    }
}

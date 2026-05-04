using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class TurnChangeSetFormatter : IMessagePackFormatter<TurnChangeSet?>
{
    public static readonly TurnChangeSetFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, TurnChangeSet? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        writer.WriteArrayHeader(2);
        resolver.GetFormatterWithVerify<PlayerChange?>().Serialize(ref writer, value.PlayerState, options);
        resolver.GetFormatterWithVerify<List<object?>?>().Serialize(ref writer, value.Events, options);
    }

    public TurnChangeSet? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new TurnChangeSet();

        for (var i = 0; i < length; i++)
        {
            switch (i)
            {
                case 0:
                    result.PlayerState = resolver.GetFormatterWithVerify<PlayerChange?>()
                        .Deserialize(ref reader, options)!;
                    break;
                case 1:
                    result.Events = resolver.GetFormatterWithVerify<List<object?>?>()
                        .Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

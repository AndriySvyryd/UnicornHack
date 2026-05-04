using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class AttackSummaryFormatter : IMessagePackFormatter<AttackSummary?>
{
    public static readonly AttackSummaryFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, AttackSummary? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(4);
        writer.Write(value.Delay);
        writer.Write(value.HitProbabilities);
        writer.Write(value.Damages);
        writer.Write(value.TicksToKill);
    }

    public AttackSummary? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var length = reader.ReadArrayHeader();
        var result = new AttackSummary();
        for (var i = 0; i < length; i++)
        {
            switch (i)
            {
                case 0:
                    result.Delay = reader.ReadInt32();
                    break;
                case 1:
                    result.HitProbabilities = reader.ReadString() ?? "";
                    break;
                case 2:
                    result.Damages = reader.ReadString() ?? "";
                    break;
                case 3:
                    result.TicksToKill = reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

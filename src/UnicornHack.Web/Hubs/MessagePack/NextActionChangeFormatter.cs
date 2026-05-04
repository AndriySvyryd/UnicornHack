using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Hubs.MessagePack;

public sealed class NextActionChangeFormatter : IMessagePackFormatter<ActorActionChange?>
{
    public static readonly NextActionChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, ActorActionChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(5);
        writer.Write((int)value.Type);
        writer.Write(value.Name ?? "");
        writer.Write(value.Target);
        writer.Write((int)value.TargetingShape);
        writer.Write(value.TargetingShapeSize);
    }

    public ActorActionChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var length = reader.ReadArrayHeader();
        var result = new ActorActionChange();
        for (var i = 0; i < length; i++)
        {
            switch (i)
            {
                case 0:
                    result.Type = (ActorActionType)reader.ReadInt32();
                    break;
                case 1:
                    result.Name = reader.ReadString();
                    break;
                case 2:
                    result.Target = reader.ReadInt32();
                    break;
                case 3:
                    result.TargetingShape = (TargetingShape)reader.ReadInt32();
                    break;
                case 4:
                    result.TargetingShapeSize = reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

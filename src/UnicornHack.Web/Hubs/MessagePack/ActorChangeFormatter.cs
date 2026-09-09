using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class ActorChangeFormatter : IMessagePackFormatter<ActorChange?>
{
    public static readonly ActorChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, ActorChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = values.GetSerializedLength(ActorChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<ushort?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values.IsPropertyChanged(1))
        {
            writer.Write(value.LevelX);
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.LevelY);
        }

        if (values.IsPropertyChanged(3))
        {
            writer.Write(value.Heading);
        }

        if (values.IsPropertyChanged(4))
        {
            writer.Write(value.BaseName ?? "");
        }

        if (values.IsPropertyChanged(5))
        {
            writer.Write(value.Name ?? "");
        }

        if (values.IsPropertyChanged(6))
        {
            writer.Write(value.IsCurrentlyPerceived);
        }

        if (values.IsPropertyChanged(7))
        {
            writer.Write(value.Hp);
        }

        if (values.IsPropertyChanged(8))
        {
            writer.Write(value.MaxHp);
        }

        if (values.IsPropertyChanged(9))
        {
            writer.Write(value.Ep);
        }

        if (values.IsPropertyChanged(10))
        {
            writer.Write(value.MaxEp);
        }

        if (values.IsPropertyChanged(11))
        {
            writer.Write(value.NextActionTick);
        }

        if (values.IsPropertyChanged(12))
        {
            resolver.GetFormatterWithVerify<ActorActionChange?>()
                .Serialize(ref writer, value.NextAction, options);
        }

    }

    public ActorChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new ActorChange();
        ushort? values = null;

        for (var i = 0; i < length; i++)
        {
            if (values.HasValue && !values.IsPropertyChanged(i))
            {
                continue;
            }

            switch (i)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<ushort?>().Deserialize(ref reader, options);
                    if (values.HasValue)
                    {
                        length = values.GetValueOrDefault() == 0 ? 1 : ActorChange.PropertyCount;
                    }

                    result.ChangedProperties = values;
                    break;
                case 1:
                    result.LevelX = reader.ReadByte();
                    break;
                case 2:
                    result.LevelY = reader.ReadByte();
                    break;
                case 3:
                    result.Heading = reader.ReadByte();
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
                case 7:
                    result.Hp = reader.ReadInt32();
                    break;
                case 8:
                    result.MaxHp = reader.ReadInt32();
                    break;
                case 9:
                    result.Ep = reader.ReadInt32();
                    break;
                case 10:
                    result.MaxEp = reader.ReadInt32();
                    break;
                case 11:
                    result.NextActionTick = reader.ReadInt32();
                    break;
                case 12:
                    result.NextAction = resolver.GetFormatterWithVerify<ActorActionChange?>()
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

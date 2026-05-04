using System.Collections;
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
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, ActorChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values == null || values[1])
        {
            writer.Write(value.LevelX);
        }

        if (values == null || values[2])
        {
            writer.Write(value.LevelY);
        }

        if (values == null || values[3])
        {
            writer.Write(value.Heading);
        }

        if (values == null || values[4])
        {
            writer.Write(value.BaseName ?? "");
        }

        if (values == null || values[5])
        {
            writer.Write(value.Name ?? "");
        }

        if (values == null || values[6])
        {
            writer.Write(value.IsCurrentlyPerceived);
        }

        if (values == null || values[7])
        {
            writer.Write(value.Hp);
        }

        if (values == null || values[8])
        {
            writer.Write(value.MaxHp);
        }

        if (values == null || values[9])
        {
            writer.Write(value.Ep);
        }

        if (values == null || values[10])
        {
            writer.Write(value.MaxEp);
        }

        if (values == null || values[11])
        {
            writer.Write(value.NextActionTick);
        }

        if (values == null || values[12])
        {
            resolver.GetFormatterWithVerify<ActorActionChange?>().Serialize(ref writer, value.NextAction, options);
        }

        if (values == null || values[13])
        {
            resolver.GetFormatterWithVerify<AttackSummary?>().Serialize(ref writer, value.MeleeAttack, options);
        }

        if (values == null || values[14])
        {
            resolver.GetFormatterWithVerify<AttackSummary?>().Serialize(ref writer, value.RangeAttack, options);
        }

        if (values == null || values[15])
        {
            resolver.GetFormatterWithVerify<AttackSummary?>().Serialize(ref writer, value.MeleeDefense, options);
        }

        if (values == null || values[16])
        {
            resolver.GetFormatterWithVerify<AttackSummary?>().Serialize(ref writer, value.RangeDefense, options);
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
                    result.NextAction = resolver.GetFormatterWithVerify<ActorActionChange?>().Deserialize(ref reader, options);
                    break;
                case 13:
                    result.MeleeAttack = resolver.GetFormatterWithVerify<AttackSummary?>().Deserialize(ref reader, options);
                    break;
                case 14:
                    result.RangeAttack = resolver.GetFormatterWithVerify<AttackSummary?>().Deserialize(ref reader, options);
                    break;
                case 15:
                    result.MeleeDefense = resolver.GetFormatterWithVerify<AttackSummary?>().Deserialize(ref reader, options);
                    break;
                case 16:
                    result.RangeDefense = resolver.GetFormatterWithVerify<AttackSummary?>().Deserialize(ref reader, options);
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

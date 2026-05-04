using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class PlayerChangeFormatter : IMessagePackFormatter<PlayerChange?>
{
    public static readonly PlayerChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, PlayerChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = ChangeSetFormatterHelper.GetSerializedLength(values, PlayerChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, values, options);
        if (values == null || values[1])
        {
            writer.Write(value.Name ?? "");
        }

        if (values == null || values[2])
        {
            writer.Write(value.Id);
        }

        if (values == null || values[3])
        {
            writer.Write(value.PreviousTick);
        }

        if (values == null || values[4])
        {
            writer.Write(value.CurrentTick);
        }

        if (values == null || values[5])
        {
            resolver.GetFormatterWithVerify<LevelChange?>().Serialize(ref writer, value.Level, options);
        }

        if (values == null || values[6])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, RaceChange>?>()
                .Serialize(ref writer, value.Races, options);
        }

        if (values == null || values[7])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>?>()
                .Serialize(ref writer, value.Abilities, options);
        }

        if (values == null || values[8])
        {
            resolver.GetFormatterWithVerify<Dictionary<int, LogEntryChange>?>()
                .Serialize(ref writer, value.Log, options);
        }

        if (values == null || values[9])
        {
            writer.Write(value.NextActionTick);
        }

        if (values == null || values[10])
        {
            writer.Write(value.XP);
        }

        if (values == null || values[11])
        {
            writer.Write(value.Hp);
        }

        if (values == null || values[12])
        {
            writer.Write(value.MaxHp);
        }

        if (values == null || values[13])
        {
            writer.Write(value.Ep);
        }

        if (values == null || values[14])
        {
            writer.Write(value.MaxEp);
        }

        if (values == null || values[15])
        {
            writer.Write(value.ReservedEp);
        }

        if (values == null || values[16])
        {
            writer.Write(value.Fortune);
        }
    }

    public PlayerChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new PlayerChange();
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
                    result.Name = reader.ReadString();
                    break;
                case 2:
                    result.Id = reader.ReadInt32();
                    break;
                case 3:
                    result.PreviousTick = reader.ReadInt32();
                    break;
                case 4:
                    result.CurrentTick = reader.ReadInt32();
                    break;
                case 5:
                    result.Level = resolver.GetFormatterWithVerify<LevelChange?>()
                        .Deserialize(ref reader, options);
                    break;
                case 6:
                    result.Races = resolver.GetFormatterWithVerify<Dictionary<int, RaceChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 7:
                    result.Abilities = resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 8:
                    result.Log = resolver.GetFormatterWithVerify<Dictionary<int, LogEntryChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 9:
                    result.NextActionTick = reader.ReadInt32();
                    break;
                case 10:
                    result.XP = reader.ReadInt32();
                    break;
                case 11:
                    result.Hp = reader.ReadInt32();
                    break;
                case 12:
                    result.MaxHp = reader.ReadInt32();
                    break;
                case 13:
                    result.Ep = reader.ReadInt32();
                    break;
                case 14:
                    result.MaxEp = reader.ReadInt32();
                    break;
                case 15:
                    result.ReservedEp = reader.ReadInt32();
                    break;
                case 16:
                    result.Fortune = reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

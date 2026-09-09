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
        var length = values.GetSerializedLength(PlayerChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<uint?>().Serialize(ref writer, values, options);
        if (values.IsPropertyChanged(1))
        {
            writer.Write(value.Name ?? "");
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.Id);
        }

        if (values.IsPropertyChanged(3))
        {
            writer.Write(value.PreviousTick);
        }

        if (values.IsPropertyChanged(4))
        {
            writer.Write(value.CurrentTick);
        }

        if (values.IsPropertyChanged(5))
        {
            writer.Write(value.NextActionTick);
        }

        if (values.IsPropertyChanged(6))
        {
            resolver.GetFormatterWithVerify<RoomChange?>().Serialize(ref writer, value.Room, options);
        }

        if (values.IsPropertyChanged(7))
        {
            resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>?>()
                .Serialize(ref writer, value.Abilities, options);
        }

        if (values.IsPropertyChanged(8))
        {
            resolver.GetFormatterWithVerify<Dictionary<int, EffectChange>?>()
                .Serialize(ref writer, value.Effects, options);
        }

        if (values.IsPropertyChanged(9))
        {
            resolver.GetFormatterWithVerify<Dictionary<int, LogEntryChange>?>()
                .Serialize(ref writer, value.Log, options);
        }

        if (values.IsPropertyChanged(10))
        {
            writer.Write(value.XP);
        }

        if (values.IsPropertyChanged(11))
        {
            writer.Write(value.Hp);
        }

        if (values.IsPropertyChanged(12))
        {
            writer.Write(value.MaxHp);
        }

        if (values.IsPropertyChanged(13))
        {
            writer.Write(value.Ep);
        }

        if (values.IsPropertyChanged(14))
        {
            writer.Write(value.MaxEp);
        }

        if (values.IsPropertyChanged(15))
        {
            writer.Write(value.ReservedEp);
        }

        if (values.IsPropertyChanged(16))
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
        uint? values = null;

        for (var i = 0; i < length; i++)
        {
            if (values.HasValue && !values.IsPropertyChanged(i))
            {
                continue;
            }

            switch (i)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<uint?>().Deserialize(ref reader, options);
                    if (values.HasValue)
                    {
                        length = values.GetValueOrDefault() == 0 ? 1 : PlayerChange.PropertyCount;
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
                    result.NextActionTick = reader.ReadInt32();
                    break;
                case 6:
                    result.Room = resolver.GetFormatterWithVerify<RoomChange?>()
                        .Deserialize(ref reader, options);
                    break;
                case 7:
                    result.Abilities = resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 8:
                    result.Effects = resolver.GetFormatterWithVerify<Dictionary<int, EffectChange>?>()
                        .Deserialize(ref reader, options);
                    break;
                case 9:
                    result.Log = resolver.GetFormatterWithVerify<Dictionary<int, LogEntryChange>?>()
                        .Deserialize(ref reader, options);
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

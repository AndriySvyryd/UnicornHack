using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class AbilityChangeFormatter : IMessagePackFormatter<AbilityChange?>
{
    public static readonly AbilityChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, AbilityChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = values.GetSerializedLength(AbilityChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<ushort?>().Serialize(ref writer, values, options);
        if (length == 1)
        {
            return;
        }

        if (values.IsPropertyChanged(1))
        {
            writer.Write(value.Name ?? "");
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.Activation);
        }

        if (values.IsPropertyChanged(3))
        {
            if (value.Slot.HasValue)
            {
                writer.Write(value.Slot.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(4))
        {
            if (value.CooldownTick.HasValue)
            {
                writer.Write(value.CooldownTick.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(5))
        {
            if (value.CooldownXpLeft.HasValue)
            {
                writer.Write(value.CooldownXpLeft.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(6))
        {
            writer.Write(value.TargetingShape);
        }

        if (values.IsPropertyChanged(7))
        {
            writer.Write(value.TargetingShapeSize);
        }

        if (values.IsPropertyChanged(8))
        {
            writer.Write(value.Type);
        }

        if (values.IsPropertyChanged(9))
        {
            if (value.Accumulator.HasValue)
            {
                writer.Write(value.Accumulator.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(10))
        {
            writer.Write(value.Level);
        }

        if (values.IsPropertyChanged(11))
        {
            writer.Write(value.LevelReduction);
        }

        if (values.IsPropertyChanged(12))
        {
            if (value.ExpirationTarget.HasValue)
            {
                writer.Write(value.ExpirationTarget.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(13))
        {
            if (value.ExpirationEvent.HasValue)
            {
                writer.Write(value.ExpirationEvent.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }

        if (values.IsPropertyChanged(14))
        {
            if (value.Noise.HasValue)
            {
                writer.Write(value.Noise.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }
    }

    public AbilityChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new AbilityChange();
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
                        length = values.GetValueOrDefault() == 0 ? 1 : AbilityChange.PropertyCount;
                    }

                    result.ChangedProperties = values;
                    break;
                case 1:
                    result.Name = reader.ReadString();
                    break;
                case 2:
                    result.Activation = reader.ReadInt32();
                    break;
                case 3:
                    result.Slot = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 4:
                    result.CooldownTick = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 5:
                    result.CooldownXpLeft = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 6:
                    result.TargetingShape = reader.ReadInt32();
                    break;
                case 7:
                    result.TargetingShapeSize = reader.ReadInt32();
                    break;
                case 8:
                    result.Type = reader.ReadInt32();
                    break;
                case 9:
                    result.Accumulator = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 10:
                    result.Level = reader.ReadInt32();
                    break;
                case 11:
                    result.LevelReduction = reader.ReadInt32();
                    break;
                case 12:
                    result.ExpirationTarget = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 13:
                    result.ExpirationEvent = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                case 14:
                    result.Noise = reader.TryReadNil() ? null : reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

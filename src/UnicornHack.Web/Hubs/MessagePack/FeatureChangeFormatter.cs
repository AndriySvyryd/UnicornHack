using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class FeatureChangeFormatter : IMessagePackFormatter<FeatureChange?>
{
    public static readonly FeatureChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, FeatureChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var mask = value.ChangedProperties;
        writer.WriteArrayHeader(mask.GetSerializedLength(FeatureChange.PropertyCount));
        options.Resolver.GetFormatterWithVerify<ushort?>().Serialize(ref writer, mask, options);
        for (var index = 1; index < FeatureChange.PropertyCount; index++)
        {
            if (!mask.IsPropertyChanged(index))
            {
                continue;
            }

            switch (index)
            {
                case 1:
                    writer.Write(value.Name);
                    break;
                case 2:
                    writer.Write(value.LevelX);
                    break;
                case 3:
                    writer.Write(value.LevelY);
                    break;
                case 4:
                    writer.Write(value.Destroyed);
                    break;
                case 5:
                    writer.Write(value.BlocksMovement);
                    break;
                case 6:
                    options.Resolver.GetFormatterWithVerify<int?>().Serialize(ref writer, value.NextActionTick, options);
                    break;
                case 7:
                    options.Resolver.GetFormatterWithVerify<int?>().Serialize(ref writer, value.CooldownTick, options);
                    break;
                case 8:
                    options.Resolver.GetFormatterWithVerify<ActorActionChange[]?>().Serialize(ref writer, value.NextActions, options);
                    break;
                case 9:
                    options.Resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AffectedCells, options);
                    break;
                case 10:
                    writer.Write(value.IsCurrentlyPerceived);
                    break;
            }
        }
    }

    public FeatureChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var length = reader.ReadArrayHeader();
        var result = new FeatureChange();
        ushort? mask = null;
        for (var index = 0; index < length; index++)
        {
            if (mask.HasValue && !mask.IsPropertyChanged(index))
            {
                continue;
            }

            switch (index)
            {
                case 0:
                    mask = options.Resolver.GetFormatterWithVerify<ushort?>().Deserialize(ref reader, options);
                    result.ChangedProperties = mask;
                    if (mask.HasValue)
                    {
                        length = mask == 0 ? 1 : FeatureChange.PropertyCount;
                    }
                    break;
                case 1:
                    result.Name = reader.ReadString() ?? "";
                    break;
                case 2:
                    result.LevelX = reader.ReadInt32();
                    break;
                case 3:
                    result.LevelY = reader.ReadInt32();
                    break;
                case 4:
                    result.Destroyed = reader.ReadBoolean();
                    break;
                case 5:
                    result.BlocksMovement = reader.ReadBoolean();
                    break;
                case 6:
                    result.NextActionTick = options.Resolver.GetFormatterWithVerify<int?>().Deserialize(ref reader, options);
                    break;
                case 7:
                    result.CooldownTick = options.Resolver.GetFormatterWithVerify<int?>().Deserialize(ref reader, options);
                    break;
                case 8:
                    result.NextActions = options.Resolver.GetFormatterWithVerify<ActorActionChange[]?>().Deserialize(ref reader, options);
                    break;
                case 9:
                    result.AffectedCells = options.Resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
                    break;
                case 10:
                    result.IsCurrentlyPerceived = reader.ReadBoolean();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

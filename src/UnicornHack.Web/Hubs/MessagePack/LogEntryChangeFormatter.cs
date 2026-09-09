using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class LogEntryChangeFormatter : IMessagePackFormatter<LogEntryChange?>
{
    public static readonly LogEntryChangeFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, LogEntryChange? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        var length = values.GetSerializedLength(LogEntryChange.PropertyCount);
        writer.WriteArrayHeader(length);
        resolver.GetFormatterWithVerify<byte?>().Serialize(ref writer, values, options);
        if (values.IsPropertyChanged(1))
        {
            writer.Write(value.Message ?? "");
        }

        if (values.IsPropertyChanged(2))
        {
            writer.Write(value.Ticks);
        }
    }

    public LogEntryChange? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var result = new LogEntryChange();
        byte? values = null;

        for (var i = 0; i < length; i++)
        {
            if (values.HasValue && !values.IsPropertyChanged(i))
            {
                continue;
            }

            switch (i)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<byte?>().Deserialize(ref reader, options);
                    if (values.HasValue)
                    {
                        length = values.GetValueOrDefault() == 0 ? 1 : LogEntryChange.PropertyCount;
                    }

                    result.ChangedProperties = values;
                    break;
                case 1:
                    result.Message = reader.ReadString();
                    break;
                case 2:
                    result.Ticks = reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return result;
    }
}

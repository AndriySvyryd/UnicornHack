using MessagePack;
using MessagePack.Formatters;
using System.Collections;

namespace UnicornHack.Hubs.MessagePack;

public abstract class FixedArrayFormatter<T> : IMessagePackFormatter<T?>
    where T : class, new()
{
    protected abstract int FieldCount { get; }

    public void Serialize(ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        if (GetChangedProperties(value) is { Length: 1 } changedProperties)
        {
            writer.WriteArrayHeader(1);
            options.Resolver.GetFormatterWithVerify<BitArray?>()
                .Serialize(ref writer, changedProperties, options);
            return;
        }

        writer.WriteArrayHeader(FieldCount);
        SerializeFields(ref writer, value, options);
    }

    public T? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        var length = reader.ReadArrayHeader();
        var value = new T();
        for (var i = 0; i < length; i++)
        {
            if (i < FieldCount)
            {
                DeserializeField(ref reader, value, i, options);
            }
            else
            {
                reader.Skip();
            }
        }

        reader.Depth--;
        return value;
    }

    protected abstract void SerializeFields(
        ref MessagePackWriter writer, T value, MessagePackSerializerOptions options);

    protected abstract void DeserializeField(
        ref MessagePackReader reader, T value, int field, MessagePackSerializerOptions options);

    protected virtual BitArray? GetChangedProperties(T value) => null;

    protected static void WriteNullableInt(ref MessagePackWriter writer, int? value)
    {
        if (value is { } intValue)
        {
            writer.Write(intValue);
        }
        else
        {
            writer.WriteNil();
        }
    }

    protected static int? ReadNullableInt(ref MessagePackReader reader)
        => reader.TryReadNil() ? null : reader.ReadInt32();
}

using MessagePack;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class TurnEventFormatter : FixedArrayFormatter<TurnEvent>
{
    public static readonly TurnEventFormatter Instance = new();

    protected override int FieldCount => 3;

    protected override void SerializeFields(
        ref MessagePackWriter writer, TurnEvent value, MessagePackSerializerOptions options)
    {
        writer.Write((int)value.Type);
        writer.Write(value.Value1);
        writer.Write(value.Value2);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, TurnEvent value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.Type = (TurnEventType)reader.ReadInt32(); break;
            case 1: value.Value1 = reader.ReadInt32(); break;
            case 2: value.Value2 = reader.ReadInt32(); break;
        }
    }
}

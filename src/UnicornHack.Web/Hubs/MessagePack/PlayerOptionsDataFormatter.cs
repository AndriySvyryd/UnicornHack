using MessagePack;
using UnicornHack.Hubs.ChangeTracking;
namespace UnicornHack.Hubs.MessagePack;

public sealed class PlayerOptionsDataFormatter : FixedArrayFormatter<PlayerOptionsData>
{
    public static readonly PlayerOptionsDataFormatter Instance = new();
    protected override int FieldCount => 5;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PlayerOptionsData value, MessagePackSerializerOptions options)
    {
        writer.Write(value.Choice);
        writer.Write(value.Action);
        writer.Write(value.Title);
        options.Resolver.GetFormatterWithVerify<List<PlayerOptionData>>()
            .Serialize(ref writer, value.Options, options);
        writer.Write(value.Notice);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, PlayerOptionsData value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0:
                value.Choice = reader.ReadString() ?? "";
                break;
            case 1:
                value.Action = reader.ReadString();
                break;
            case 2:
                value.Title = reader.ReadString() ?? "";
                break;
            case 3:
                value.Options = options.Resolver.GetFormatterWithVerify<List<PlayerOptionData>>()
                    .Deserialize(ref reader, options);
                break;
            case 4:
                value.Notice = reader.ReadString();
                break;
            default:
                reader.Skip();
                break;
        }
    }
}

public sealed class PlayerOptionDataFormatter : FixedArrayFormatter<PlayerOptionData>
{
    public static readonly PlayerOptionDataFormatter Instance = new();
    protected override int FieldCount => 4;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PlayerOptionData value, MessagePackSerializerOptions options)
    {
        writer.Write(value.Identifier);
        writer.Write(value.Title);
        writer.Write(value.Description);
        writer.Write(value.DisabledReason);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, PlayerOptionData value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0:
                value.Identifier = reader.ReadString() ?? "";
                break;
            case 1:
                value.Title = reader.ReadString() ?? "";
                break;
            case 2:
                value.Description = reader.ReadString() ?? "";
                break;
            case 3:
                value.DisabledReason = reader.ReadString();
                break;
            default:
                reader.Skip();
                break;
        }
    }
}

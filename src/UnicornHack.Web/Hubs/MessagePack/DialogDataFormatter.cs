using System.Collections;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class DialogDataFormatter : IMessagePackFormatter<DialogData?>
{
    public static readonly DialogDataFormatter Instance = new();

    public void Serialize(
        ref MessagePackWriter writer, DialogData? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        var resolver = options.Resolver;
        var values = value.ChangedProperties;
        writer.WriteArrayHeader(ChangeSetFormatterHelper.GetSerializedLength(values, 13));
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, value.ChangedProperties, options);
        if (values is { Length: 1 })
        {
            return;
        }

        if (values == null || values[1]) WriteNullableInt(ref writer, value.AbilitySlot);
        if (values == null || values[2]) resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>>().Serialize(ref writer, value.SlottableAbilities, options);
        if (values == null || values[3]) resolver.GetFormatterWithVerify<ActorAttributes?>().Serialize(ref writer, value.PlayerAttributes, options);
        if (values == null || values[4]) resolver.GetFormatterWithVerify<PlayerInventory?>().Serialize(ref writer, value.PlayerInventory, options);
        if (values == null || values[5]) resolver.GetFormatterWithVerify<PlayerAdaptations?>().Serialize(ref writer, value.PlayerAdaptations, options);
        if (values == null || values[6]) resolver.GetFormatterWithVerify<PlayerSkills?>().Serialize(ref writer, value.PlayerSkills, options);
        if (values == null || values[7]) resolver.GetFormatterWithVerify<AbilityAttributes?>().Serialize(ref writer, value.AbilityAttributes, options);
        if (values == null || values[8]) resolver.GetFormatterWithVerify<ActorAttributes?>().Serialize(ref writer, value.ActorAttributes, options);
        if (values == null || values[9]) resolver.GetFormatterWithVerify<ItemAttributes?>().Serialize(ref writer, value.ItemAttributes, options);
        if (values == null || values[10]) writer.Write(value.StaticDescription);
        if (values == null || values[11]) resolver.GetFormatterWithVerify<PostGameStatistics?>().Serialize(ref writer, value.PostGameStatistics, options);
        if (values == null || values[12]) resolver.GetFormatterWithVerify<LevelMap?>().Serialize(ref writer, value.LevelMap, options);
    }

    public DialogData? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var resolver = options.Resolver;
        var length = reader.ReadArrayHeader();
        var value = new DialogData();
        BitArray? values = null;
        for (var field = 0; field < length; field++)
        {
            if (values != null && !values[field])
            {
                continue;
            }

            switch (field)
            {
                case 0:
                    values = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options);
                    if (values != null) length = values.Length;
                    value.ChangedProperties = values;
                    break;
                case 1: value.AbilitySlot = ReadNullableInt(ref reader); break;
                case 2: value.SlottableAbilities = resolver.GetFormatterWithVerify<Dictionary<int, AbilityChange>>().Deserialize(ref reader, options); break;
                case 3: value.PlayerAttributes = resolver.GetFormatterWithVerify<ActorAttributes?>().Deserialize(ref reader, options); break;
                case 4: value.PlayerInventory = resolver.GetFormatterWithVerify<PlayerInventory?>().Deserialize(ref reader, options); break;
                case 5: value.PlayerAdaptations = resolver.GetFormatterWithVerify<PlayerAdaptations?>().Deserialize(ref reader, options); break;
                case 6: value.PlayerSkills = resolver.GetFormatterWithVerify<PlayerSkills?>().Deserialize(ref reader, options); break;
                case 7: value.AbilityAttributes = resolver.GetFormatterWithVerify<AbilityAttributes?>().Deserialize(ref reader, options); break;
                case 8: value.ActorAttributes = resolver.GetFormatterWithVerify<ActorAttributes?>().Deserialize(ref reader, options); break;
                case 9: value.ItemAttributes = resolver.GetFormatterWithVerify<ItemAttributes?>().Deserialize(ref reader, options); break;
                case 10: value.StaticDescription = reader.ReadString(); break;
                case 11: value.PostGameStatistics = resolver.GetFormatterWithVerify<PostGameStatistics?>().Deserialize(ref reader, options); break;
                case 12: value.LevelMap = resolver.GetFormatterWithVerify<LevelMap?>().Deserialize(ref reader, options); break;
                default: reader.Skip(); break;
            }
        }

        return value;
    }

    private static void WriteNullableInt(ref MessagePackWriter writer, int? value)
    {
        if (value.HasValue) writer.Write(value.Value);
        else writer.WriteNil();
    }

    private static int? ReadNullableInt(ref MessagePackReader reader)
        => reader.TryReadNil() ? null : reader.ReadInt32();
}

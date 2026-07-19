using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.MessagePack;

public sealed class ItemAttributesFormatter : FixedArrayFormatter<ItemAttributes>
{
    public static readonly ItemAttributesFormatter Instance = new();
    protected override int FieldCount => 13;

    protected override BitArray? GetChangedProperties(ItemAttributes value)
        => value.ChangedProperties;

    protected override void SerializeFields(
        ref MessagePackWriter writer, ItemAttributes value, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, value.ChangedProperties, options);
        writer.Write(value.Name);
        writer.Write(value.Description);
        writer.Write((int)value.Type);
        writer.Write((int)value.Material);
        writer.Write(value.Weight);
        writer.Write((int)value.Complexity);
        writer.Write(value.RequiredMight);
        writer.Write(value.RequiredSpeed);
        writer.Write(value.RequiredFocus);
        writer.Write(value.RequiredPerception);
        resolver.GetFormatterWithVerify<Dictionary<int, EquippableSlot>>()
            .Serialize(ref writer, value.EquippableSlots, options);
        resolver.GetFormatterWithVerify<List<AbilityAttributes>>()
            .Serialize(ref writer, value.Abilities, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, ItemAttributes value, int field, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        switch (field)
        {
            case 0: value.ChangedProperties = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options); break;
            case 1: value.Name = reader.ReadString(); break;
            case 2: value.Description = reader.ReadString() ?? ""; break;
            case 3: value.Type = (ItemType)reader.ReadInt32(); break;
            case 4: value.Material = (Material)reader.ReadInt32(); break;
            case 5: value.Weight = reader.ReadInt32(); break;
            case 6: value.Complexity = (ItemComplexity)reader.ReadInt32(); break;
            case 7: value.RequiredMight = reader.ReadInt32(); break;
            case 8: value.RequiredSpeed = reader.ReadInt32(); break;
            case 9: value.RequiredFocus = reader.ReadInt32(); break;
            case 10: value.RequiredPerception = reader.ReadInt32(); break;
            case 11: value.EquippableSlots = resolver.GetFormatterWithVerify<Dictionary<int, EquippableSlot>>().Deserialize(ref reader, options); break;
            case 12: value.Abilities = resolver.GetFormatterWithVerify<List<AbilityAttributes>>().Deserialize(ref reader, options); break;
        }
    }
}

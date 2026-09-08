using System.Collections;
using MessagePack;
using DialogItem = UnicornHack.Hubs.Item;

namespace UnicornHack.Hubs.MessagePack;

public sealed class PlayerInventoryFormatter : FixedArrayFormatter<PlayerInventory>
{
    public static readonly PlayerInventoryFormatter Instance = new();
    protected override int FieldCount => 1;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PlayerInventory value, MessagePackSerializerOptions options)
        => options.Resolver.GetFormatterWithVerify<Dictionary<int, DialogItem>>()
            .Serialize(ref writer, value.Items, options);

    protected override void DeserializeField(
        ref MessagePackReader reader, PlayerInventory value, int field, MessagePackSerializerOptions options)
        => value.Items = options.Resolver.GetFormatterWithVerify<Dictionary<int, DialogItem>>()
            .Deserialize(ref reader, options);
}

public sealed class DialogItemFormatter : FixedArrayFormatter<DialogItem>
{
    public static readonly DialogItemFormatter Instance = new();
    protected override int FieldCount => 6;

    protected override void SerializeFields(
        ref MessagePackWriter writer, DialogItem value, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, value.ChangedProperties, options);
        writer.Write((int)value.Type);
        writer.Write(value.BaseName);
        writer.Write(value.Name);
        resolver.GetFormatterWithVerify<Dictionary<int, EquippableSlot>>()
            .Serialize(ref writer, value.EquippableSlots, options);
        resolver.GetFormatterWithVerify<EquippableSlot?>().Serialize(ref writer, value.EquippedSlot, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, DialogItem value, int field, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        switch (field)
        {
            case 0: value.ChangedProperties = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options); break;
            case 1: value.Type = (ItemType)reader.ReadInt32(); break;
            case 2: value.BaseName = reader.ReadString(); break;
            case 3: value.Name = reader.ReadString(); break;
            case 4: value.EquippableSlots = resolver.GetFormatterWithVerify<Dictionary<int, EquippableSlot>>().Deserialize(ref reader, options); break;
            case 5: value.EquippedSlot = resolver.GetFormatterWithVerify<EquippableSlot?>().Deserialize(ref reader, options); break;
        }
    }
}

public sealed class EquippableSlotFormatter : FixedArrayFormatter<EquippableSlot>
{
    public static readonly EquippableSlotFormatter Instance = new();
    protected override int FieldCount => 3;

    protected override void SerializeFields(
        ref MessagePackWriter writer, EquippableSlot value, MessagePackSerializerOptions options)
    {
        writer.Write(value.Id);
        writer.Write(value.ShortName);
        writer.Write(value.Name);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, EquippableSlot value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.Id = reader.ReadInt32(); break;
            case 1: value.ShortName = reader.ReadString() ?? ""; break;
            case 2: value.Name = reader.ReadString() ?? ""; break;
        }
    }
}

public sealed class PlayerAdaptationsFormatter : FixedArrayFormatter<PlayerAdaptations>
{
    public static readonly PlayerAdaptationsFormatter Instance = new();
    protected override int FieldCount => 2;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PlayerAdaptations value, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterWithVerify<Dictionary<string, int>>();
        formatter.Serialize(ref writer, value.Traits, options);
        formatter.Serialize(ref writer, value.Feats, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, PlayerAdaptations value, int field, MessagePackSerializerOptions options)
    {
        var formatter = options.Resolver.GetFormatterWithVerify<Dictionary<string, int>>();
        switch (field)
        {
            case 0: value.Traits = formatter.Deserialize(ref reader, options); break;
            case 1: value.Feats = formatter.Deserialize(ref reader, options); break;
        }
    }
}

public sealed class EffectAttributesFormatter : FixedArrayFormatter<EffectAttributes>
{
    public static readonly EffectAttributesFormatter Instance = new();
    protected override int FieldCount => 8;

    protected override void SerializeFields(
        ref MessagePackWriter writer, EffectAttributes value, MessagePackSerializerOptions options)
    {
        writer.Write((int)value.Type);
        writer.Write(value.ShouldTargetActivator);
        writer.Write(value.Amount);
        writer.Write((int)value.Function);
        writer.Write(value.TargetName);
        writer.Write(value.SecondaryAmount);
        writer.Write((int)value.Duration);
        writer.Write(value.DurationAmount);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, EffectAttributes value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.Type = (EffectType)reader.ReadInt32(); break;
            case 1: value.ShouldTargetActivator = reader.ReadBoolean(); break;
            case 2: value.Amount = reader.ReadString(); break;
            case 3: value.Function = (ValueCombinationFunction)reader.ReadInt32(); break;
            case 4: value.TargetName = reader.ReadString(); break;
            case 5: value.SecondaryAmount = reader.ReadString(); break;
            case 6: value.Duration = (EffectDuration)reader.ReadInt32(); break;
            case 7: value.DurationAmount = reader.ReadString(); break;
        }
    }
}

public sealed class SubAbilityAttributesFormatter : FixedArrayFormatter<SubAbilityAttributes>
{
    public static readonly SubAbilityAttributesFormatter Instance = new();
    protected override int FieldCount => 3;

    protected override void SerializeFields(
        ref MessagePackWriter writer, SubAbilityAttributes value, MessagePackSerializerOptions options)
    {
        writer.Write((int)value.SuccessCondition);
        writer.Write(value.Accuracy);
        options.Resolver.GetFormatterWithVerify<Dictionary<int, EffectAttributes>>()
            .Serialize(ref writer, value.Effects, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, SubAbilityAttributes value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.SuccessCondition = (AbilitySuccessCondition)reader.ReadInt32(); break;
            case 1: value.Accuracy = reader.ReadInt32(); break;
            case 2: value.Effects = options.Resolver.GetFormatterWithVerify<Dictionary<int, EffectAttributes>>()
                .Deserialize(ref reader, options); break;
        }
    }
}

public sealed class ConditionalGrantAttributesFormatter : FixedArrayFormatter<ConditionalGrantAttributes>
{
    public static readonly ConditionalGrantAttributesFormatter Instance = new();
    protected override int FieldCount => 3;

    protected override void SerializeFields(
        ref MessagePackWriter writer, ConditionalGrantAttributes value, MessagePackSerializerOptions options)
    {
        writer.Write(value.Name);
        writer.Write(value.Level);
        writer.Write(value.LevelExpression);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, ConditionalGrantAttributes value, int field,
        MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.Name = reader.ReadString()!; break;
            case 1: value.Level = reader.ReadInt32(); break;
            case 2: value.LevelExpression = reader.ReadString()!; break;
        }
    }
}

public sealed class PostGameStatisticsFormatter : FixedArrayFormatter<PostGameStatistics>
{
    public static readonly PostGameStatisticsFormatter Instance = new();
    protected override int FieldCount => 0;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PostGameStatistics value, MessagePackSerializerOptions options)
    {
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, PostGameStatistics value, int field, MessagePackSerializerOptions options)
        => reader.Skip();
}

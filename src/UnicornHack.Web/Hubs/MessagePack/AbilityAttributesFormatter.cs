using System.Collections;
using MessagePack;

namespace UnicornHack.Hubs.MessagePack;

public sealed class AbilityAttributesFormatter : FixedArrayFormatter<AbilityAttributes>
{
    public static readonly AbilityAttributesFormatter Instance = new();
    protected override int FieldCount => 23;

    protected override void SerializeFields(
        ref MessagePackWriter writer, AbilityAttributes value, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, value.ChangedProperties, options);
        writer.Write(value.Id);
        writer.Write(value.Name);
        writer.Write(value.Level);
        writer.Write((int)value.Activation);
        WriteNullableInt(ref writer, value.ActivationCondition);
        writer.Write((int)value.TargetingShape);
        writer.Write(value.TargetingShapeSize);
        writer.Write(value.Range);
        writer.Write(value.MinHeadingDeviation);
        writer.Write(value.MaxHeadingDeviation);
        writer.Write(value.EnergyCost);
        writer.Write(value.Delay);
        writer.Write(value.Cooldown);
        writer.Write(value.CooldownTicksLeft);
        writer.Write(value.XpCooldown);
        writer.Write(value.XpCooldownLeft);
        resolver.GetFormatterWithVerify<Dictionary<int, EffectAttributes>>()
            .Serialize(ref writer, value.SelfEffects, options);
        resolver.GetFormatterWithVerify<List<SubAbilityAttributes>>()
            .Serialize(ref writer, value.SubAbilities, options);
        writer.Write(value.Description);
        WriteNullableInt(ref writer, value.Accumulator);
        writer.Write(value.Noise);
        resolver.GetFormatterWithVerify<List<ConditionalGrantAttributes>>()
            .Serialize(ref writer, value.ConditionalGrants, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, AbilityAttributes value, int field, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        switch (field)
        {
            case 0: value.ChangedProperties = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options); break;
            case 1: value.Id = reader.ReadInt32(); break;
            case 2: value.Name = reader.ReadString(); break;
            case 3: value.Level = reader.ReadInt32(); break;
            case 4: value.Activation = (ActivationType)reader.ReadInt32(); break;
            case 5: value.ActivationCondition = ReadNullableInt(ref reader); break;
            case 6: value.TargetingShape = (TargetingShape)reader.ReadInt32(); break;
            case 7: value.TargetingShapeSize = reader.ReadInt32(); break;
            case 8: value.Range = reader.ReadInt32(); break;
            case 9: value.MinHeadingDeviation = reader.ReadInt32(); break;
            case 10: value.MaxHeadingDeviation = reader.ReadInt32(); break;
            case 11: value.EnergyCost = reader.ReadInt32(); break;
            case 12: value.Delay = reader.ReadInt32(); break;
            case 13: value.Cooldown = reader.ReadInt32(); break;
            case 14: value.CooldownTicksLeft = reader.ReadInt32(); break;
            case 15: value.XpCooldown = reader.ReadInt32(); break;
            case 16: value.XpCooldownLeft = reader.ReadInt32(); break;
            case 17: value.SelfEffects = resolver.GetFormatterWithVerify<Dictionary<int, EffectAttributes>>().Deserialize(ref reader, options); break;
            case 18: value.SubAbilities = resolver.GetFormatterWithVerify<List<SubAbilityAttributes>>().Deserialize(ref reader, options); break;
            case 19: value.Description = reader.ReadString(); break;
            case 20: value.Accumulator = ReadNullableInt(ref reader); break;
            case 21: value.Noise = reader.ReadInt32(); break;
            case 22: value.ConditionalGrants = resolver.GetFormatterWithVerify<List<ConditionalGrantAttributes>>()
                .Deserialize(ref reader, options); break;
        }
    }
}

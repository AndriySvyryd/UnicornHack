using System.Collections;
using MessagePack;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

public sealed class ActorAttributesFormatter : FixedArrayFormatter<ActorAttributes>
{
    public static readonly ActorAttributesFormatter Instance = new();
    protected override int FieldCount => 46;

    protected override BitArray? GetChangedProperties(ActorAttributes value)
        => value.ChangedProperties;

    protected override void SerializeFields(
        ref MessagePackWriter writer, ActorAttributes value, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        resolver.GetFormatterWithVerify<BitArray?>().Serialize(ref writer, value.ChangedProperties, options);
        writer.Write(value.Name);
        writer.Write(value.Description);
        writer.Write(value.Level);
        writer.Write(value.MovementDelay);
        writer.Write(value.Size);
        writer.Write(value.Weight);
        writer.Write(value.PrimaryFOVQuadrants);
        writer.Write(value.PrimaryVisionRange);
        writer.Write(value.TotalFOVQuadrants);
        writer.Write(value.SecondaryVisionRange);
        writer.Write(value.Infravision);
        writer.Write(value.InvisibilityDetection);
        writer.Write(value.Infravisible);
        writer.Write(value.Visibility);
        writer.Write(value.HitPoints);
        writer.Write(value.HitPointMaximum);
        writer.Write(value.EnergyPoints);
        writer.Write(value.EnergyPointMaximum);
        writer.Write(value.Might);
        writer.Write(value.Speed);
        writer.Write(value.Focus);
        writer.Write(value.Perception);
        writer.Write(value.Regeneration);
        writer.Write(value.EnergyRegeneration);
        writer.Write(value.Armor);
        writer.Write(value.Deflection);
        writer.Write(value.Accuracy);
        writer.Write(value.Evasion);
        writer.Write(value.Hindrance);
        writer.Write(value.PhysicalResistance);
        writer.Write(value.MagicResistance);
        writer.Write(value.BleedingResistance);
        writer.Write(value.AcidResistance);
        writer.Write(value.ColdResistance);
        writer.Write(value.ElectricityResistance);
        writer.Write(value.FireResistance);
        writer.Write(value.PsychicResistance);
        writer.Write(value.ToxinResistance);
        writer.Write(value.VoidResistance);
        writer.Write(value.SonicResistance);
        writer.Write(value.StunResistance);
        writer.Write(value.LightResistance);
        writer.Write(value.WaterResistance);
        resolver.GetFormatterWithVerify<List<AbilityAttributes>>().Serialize(ref writer, value.Abilities, options);
        resolver.GetFormatterWithVerify<Dictionary<int, EffectChange>>().Serialize(ref writer, value.Effects, options);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, ActorAttributes value, int field, MessagePackSerializerOptions options)
    {
        var resolver = options.Resolver;
        switch (field)
        {
            case 0: value.ChangedProperties = resolver.GetFormatterWithVerify<BitArray?>().Deserialize(ref reader, options); break;
            case 1: value.Name = reader.ReadString(); break;
            case 2: value.Description = reader.ReadString() ?? ""; break;
            case 3: value.Level = reader.ReadInt32(); break;
            case 4: value.MovementDelay = reader.ReadInt32(); break;
            case 5: value.Size = reader.ReadInt32(); break;
            case 6: value.Weight = reader.ReadInt32(); break;
            case 7: value.PrimaryFOVQuadrants = reader.ReadInt32(); break;
            case 8: value.PrimaryVisionRange = reader.ReadInt32(); break;
            case 9: value.TotalFOVQuadrants = reader.ReadInt32(); break;
            case 10: value.SecondaryVisionRange = reader.ReadInt32(); break;
            case 11: value.Infravision = reader.ReadBoolean(); break;
            case 12: value.InvisibilityDetection = reader.ReadBoolean(); break;
            case 13: value.Infravisible = reader.ReadBoolean(); break;
            case 14: value.Visibility = reader.ReadInt32(); break;
            case 15: value.HitPoints = reader.ReadInt32(); break;
            case 16: value.HitPointMaximum = reader.ReadInt32(); break;
            case 17: value.EnergyPoints = reader.ReadInt32(); break;
            case 18: value.EnergyPointMaximum = reader.ReadInt32(); break;
            case 19: value.Might = reader.ReadInt32(); break;
            case 20: value.Speed = reader.ReadInt32(); break;
            case 21: value.Focus = reader.ReadInt32(); break;
            case 22: value.Perception = reader.ReadInt32(); break;
            case 23: value.Regeneration = reader.ReadInt32(); break;
            case 24: value.EnergyRegeneration = reader.ReadInt32(); break;
            case 25: value.Armor = reader.ReadInt32(); break;
            case 26: value.Deflection = reader.ReadInt32(); break;
            case 27: value.Accuracy = reader.ReadInt32(); break;
            case 28: value.Evasion = reader.ReadInt32(); break;
            case 29: value.Hindrance = reader.ReadInt32(); break;
            case 30: value.PhysicalResistance = reader.ReadInt32(); break;
            case 31: value.MagicResistance = reader.ReadInt32(); break;
            case 32: value.BleedingResistance = reader.ReadInt32(); break;
            case 33: value.AcidResistance = reader.ReadInt32(); break;
            case 34: value.ColdResistance = reader.ReadInt32(); break;
            case 35: value.ElectricityResistance = reader.ReadInt32(); break;
            case 36: value.FireResistance = reader.ReadInt32(); break;
            case 37: value.PsychicResistance = reader.ReadInt32(); break;
            case 38: value.ToxinResistance = reader.ReadInt32(); break;
            case 39: value.VoidResistance = reader.ReadInt32(); break;
            case 40: value.SonicResistance = reader.ReadInt32(); break;
            case 41: value.StunResistance = reader.ReadInt32(); break;
            case 42: value.LightResistance = reader.ReadInt32(); break;
            case 43: value.WaterResistance = reader.ReadInt32(); break;
            case 44: value.Abilities = resolver.GetFormatterWithVerify<List<AbilityAttributes>>().Deserialize(ref reader, options); break;
            case 45: value.Effects = resolver.GetFormatterWithVerify<Dictionary<int, EffectChange>>().Deserialize(ref reader, options); break;
        }
    }
}

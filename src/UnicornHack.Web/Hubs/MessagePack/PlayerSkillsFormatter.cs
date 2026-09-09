using MessagePack;

namespace UnicornHack.Hubs.MessagePack;

public sealed class PlayerSkillsFormatter : FixedArrayFormatter<PlayerSkills>
{
    public static readonly PlayerSkillsFormatter Instance = new();
    protected override int FieldCount => 31;

    protected override void SerializeFields(
        ref MessagePackWriter writer, PlayerSkills value, MessagePackSerializerOptions options)
    {
        options.Resolver.GetFormatterWithVerify<uint?>()
            .Serialize(ref writer, value.ChangedProperties, options);
        writer.Write(value.HandWeapons);
        writer.Write(value.ShortWeapons);
        writer.Write(value.MediumWeapons);
        writer.Write(value.LongWeapons);
        writer.Write(value.CloseRangeWeapons);
        writer.Write(value.ShortRangeWeapons);
        writer.Write(value.MediumRangeWeapons);
        writer.Write(value.LongRangeWeapons);
        writer.Write(value.OneHanded);
        writer.Write(value.TwoHanded);
        writer.Write(value.DualWielding);
        writer.Write(value.Acrobatics);
        writer.Write(value.LightArmor);
        writer.Write(value.HeavyArmor);
        writer.Write(value.AirSourcery);
        writer.Write(value.BloodSourcery);
        writer.Write(value.EarthSourcery);
        writer.Write(value.FireSourcery);
        writer.Write(value.SpiritSourcery);
        writer.Write(value.WaterSourcery);
        writer.Write(value.Conjuration);
        writer.Write(value.Enchantment);
        writer.Write(value.Evocation);
        writer.Write(value.Malediction);
        writer.Write(value.Illusion);
        writer.Write(value.Transmutation);
        writer.Write(value.Assassination);
        writer.Write(value.Stealth);
        writer.Write(value.Artifice);
        writer.Write(value.Leadership);
    }

    protected override void DeserializeField(
        ref MessagePackReader reader, PlayerSkills value, int field, MessagePackSerializerOptions options)
    {
        switch (field)
        {
            case 0: value.ChangedProperties = options.Resolver.GetFormatterWithVerify<uint?>().Deserialize(ref reader, options); break;
            case 1: value.HandWeapons = reader.ReadInt32(); break;
            case 2: value.ShortWeapons = reader.ReadInt32(); break;
            case 3: value.MediumWeapons = reader.ReadInt32(); break;
            case 4: value.LongWeapons = reader.ReadInt32(); break;
            case 5: value.CloseRangeWeapons = reader.ReadInt32(); break;
            case 6: value.ShortRangeWeapons = reader.ReadInt32(); break;
            case 7: value.MediumRangeWeapons = reader.ReadInt32(); break;
            case 8: value.LongRangeWeapons = reader.ReadInt32(); break;
            case 9: value.OneHanded = reader.ReadInt32(); break;
            case 10: value.TwoHanded = reader.ReadInt32(); break;
            case 11: value.DualWielding = reader.ReadInt32(); break;
            case 12: value.Acrobatics = reader.ReadInt32(); break;
            case 13: value.LightArmor = reader.ReadInt32(); break;
            case 14: value.HeavyArmor = reader.ReadInt32(); break;
            case 15: value.AirSourcery = reader.ReadInt32(); break;
            case 16: value.BloodSourcery = reader.ReadInt32(); break;
            case 17: value.EarthSourcery = reader.ReadInt32(); break;
            case 18: value.FireSourcery = reader.ReadInt32(); break;
            case 19: value.SpiritSourcery = reader.ReadInt32(); break;
            case 20: value.WaterSourcery = reader.ReadInt32(); break;
            case 21: value.Conjuration = reader.ReadInt32(); break;
            case 22: value.Enchantment = reader.ReadInt32(); break;
            case 23: value.Evocation = reader.ReadInt32(); break;
            case 24: value.Malediction = reader.ReadInt32(); break;
            case 25: value.Illusion = reader.ReadInt32(); break;
            case 26: value.Transmutation = reader.ReadInt32(); break;
            case 27: value.Assassination = reader.ReadInt32(); break;
            case 28: value.Stealth = reader.ReadInt32(); break;
            case 29: value.Artifice = reader.ReadInt32(); break;
            case 30: value.Leadership = reader.ReadInt32(); break;
        }
    }
}

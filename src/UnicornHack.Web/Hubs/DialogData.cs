using System.Collections;
using MessagePack;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs;

[MessagePackObject]
public class DialogData
{
    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new(13);

    [Key(1)]
    public int? AbilitySlot { get; set; }

    [Key(2)]
    public Dictionary<int, AbilityChange> SlottableAbilities { get; set; } = [];

    [Key(3)]
    public ActorAttributes? PlayerAttributes { get; set; }

    [Key(4)]
    public PlayerInventory? PlayerInventory { get; set; }

    [Key(5)]
    public PlayerAdaptations? PlayerAdaptations { get; set; }

    [Key(6)]
    public PlayerSkills? PlayerSkills { get; set; }

    [Key(7)]
    public AbilityAttributes? AbilityAttributes { get; set; }

    [Key(8)]
    public ActorAttributes? ActorAttributes { get; set; }

    [Key(9)]
    public ItemAttributes? ItemAttributes { get; set; }

    [Key(10)]
    public string? StaticDescription { get; set; }

    [Key(11)]
    public PostGameStatistics? PostGameStatistics { get; set; }

    [Key(12)]
    public LevelMap? LevelMap { get; set; }

    public static DialogData Deleted() => new() { ChangedProperties = SerializationContext.DeletedBitArray };
}

[MessagePackObject]
public class PlayerInventory
{
    [Key(0)]
    public Dictionary<int, Item> Items { get; set; } = [];
}

[MessagePackObject]
public class Item
{
    [IgnoreMember]
    public int Id { get; set; } = -1;

    [Key(0)]
    public BitArray? ChangedProperties { get; set; } = new(6);

    [Key(1)]
    public ItemType Type { get; set; }

    [Key(2)]
    public string? BaseName { get; set; }

    [Key(3)]
    public string? Name { get; set; }

    [Key(4)]
    public Dictionary<int, EquippableSlot> EquippableSlots { get; set; } = [];

    [Key(5)]
    public EquippableSlot? EquippedSlot { get; set; }
}

[MessagePackObject]
public class EquippableSlot
{
    [Key(0)]
    public int Id { get; set; } = -1;

    [Key(1)]
    public string ShortName { get; set; } = "";

    [Key(2)]
    public string Name { get; set; } = "";
}

[MessagePackObject]
public class PlayerAdaptations
{
    [Key(0)]
    public Dictionary<string, int> Traits { get; set; } = [];

    [Key(1)]
    public Dictionary<string, int> Feats { get; set; } = [];
}

[MessagePackObject]
public class PlayerSkills
{
    [Key(0)] public BitArray? ChangedProperties { get; set; } = new(31);
    [Key(1)] public int HandWeapons { get; set; }
    [Key(2)] public int ShortWeapons { get; set; }
    [Key(3)] public int MediumWeapons { get; set; }
    [Key(4)] public int LongWeapons { get; set; }
    [Key(5)] public int CloseRangeWeapons { get; set; }
    [Key(6)] public int ShortRangeWeapons { get; set; }
    [Key(7)] public int MediumRangeWeapons { get; set; }
    [Key(8)] public int LongRangeWeapons { get; set; }
    [Key(9)] public int OneHanded { get; set; }
    [Key(10)] public int TwoHanded { get; set; }
    [Key(11)] public int DualWielding { get; set; }
    [Key(12)] public int Acrobatics { get; set; }
    [Key(13)] public int LightArmor { get; set; }
    [Key(14)] public int HeavyArmor { get; set; }
    [Key(15)] public int AirSourcery { get; set; }
    [Key(16)] public int BloodSourcery { get; set; }
    [Key(17)] public int EarthSourcery { get; set; }
    [Key(18)] public int FireSourcery { get; set; }
    [Key(19)] public int SpiritSourcery { get; set; }
    [Key(20)] public int WaterSourcery { get; set; }
    [Key(21)] public int Conjuration { get; set; }
    [Key(22)] public int Enchantment { get; set; }
    [Key(23)] public int Evocation { get; set; }
    [Key(24)] public int Malediction { get; set; }
    [Key(25)] public int Illusion { get; set; }
    [Key(26)] public int Transmutation { get; set; }
    [Key(27)] public int Assassination { get; set; }
    [Key(28)] public int Stealth { get; set; }
    [Key(29)] public int Artifice { get; set; }
    [Key(30)] public int Leadership { get; set; }
}

[MessagePackObject]
public class ActorAttributes
{
    [Key(0)] public BitArray? ChangedProperties { get; set; } = new(46);
    [Key(1)] public string? Name { get; set; }
    [Key(2)] public string Description { get; set; } = "";
    [Key(3)] public int Level { get; set; }
    [Key(4)] public int MovementDelay { get; set; }
    [Key(5)] public int Size { get; set; }
    [Key(6)] public int Weight { get; set; }
    [Key(7)] public int PrimaryFOVQuadrants { get; set; }
    [Key(8)] public int PrimaryVisionRange { get; set; }
    [Key(9)] public int TotalFOVQuadrants { get; set; }
    [Key(10)] public int SecondaryVisionRange { get; set; }
    [Key(11)] public bool Infravision { get; set; }
    [Key(12)] public bool InvisibilityDetection { get; set; }
    [Key(13)] public bool Infravisible { get; set; }
    [Key(14)] public int Visibility { get; set; }
    [Key(15)] public int HitPoints { get; set; }
    [Key(16)] public int HitPointMaximum { get; set; }
    [Key(17)] public int EnergyPoints { get; set; }
    [Key(18)] public int EnergyPointMaximum { get; set; }
    [Key(19)] public int Might { get; set; }
    [Key(20)] public int Speed { get; set; }
    [Key(21)] public int Focus { get; set; }
    [Key(22)] public int Perception { get; set; }
    [Key(23)] public int Regeneration { get; set; }
    [Key(24)] public int EnergyRegeneration { get; set; }
    [Key(25)] public int Armor { get; set; }
    [Key(26)] public int Deflection { get; set; }
    [Key(27)] public int Accuracy { get; set; }
    [Key(28)] public int Evasion { get; set; }
    [Key(29)] public int Hindrance { get; set; }
    [Key(30)] public int PhysicalResistance { get; set; }
    [Key(31)] public int MagicResistance { get; set; }
    [Key(32)] public int BleedingResistance { get; set; }
    [Key(33)] public int AcidResistance { get; set; }
    [Key(34)] public int ColdResistance { get; set; }
    [Key(35)] public int ElectricityResistance { get; set; }
    [Key(36)] public int FireResistance { get; set; }
    [Key(37)] public int PsychicResistance { get; set; }
    [Key(38)] public int ToxinResistance { get; set; }
    [Key(39)] public int VoidResistance { get; set; }
    [Key(40)] public int SonicResistance { get; set; }
    [Key(41)] public int StunResistance { get; set; }
    [Key(42)] public int LightResistance { get; set; }
    [Key(43)] public int WaterResistance { get; set; }
    [Key(44)] public List<AbilityAttributes> Abilities { get; set; } = [];
    [Key(45)] public Dictionary<int, EffectChange> Effects { get; set; } = [];
}

[MessagePackObject]
public class AbilityAttributes
{
    [Key(0)] public BitArray? ChangedProperties { get; set; } = new(23);
    [Key(1)] public int Id { get; set; } = -1;
    [Key(2)] public string? Name { get; set; }
    [Key(3)] public int Level { get; set; }
    [Key(4)] public ActivationType Activation { get; set; }
    [Key(5)] public int? ActivationCondition { get; set; }
    [Key(6)] public TargetingShape TargetingShape { get; set; }
    [Key(7)] public int TargetingShapeSize { get; set; }
    [Key(8)] public int Range { get; set; }
    [Key(9)] public int MinHeadingDeviation { get; set; }
    [Key(10)] public int MaxHeadingDeviation { get; set; }
    [Key(11)] public int EnergyCost { get; set; }
    [Key(12)] public int Delay { get; set; }
    [Key(13)] public int Cooldown { get; set; }
    [Key(14)] public int CooldownTicksLeft { get; set; }
    [Key(15)] public int XpCooldown { get; set; }
    [Key(16)] public int XpCooldownLeft { get; set; }
    [Key(17)] public Dictionary<int, EffectAttributes> SelfEffects { get; set; } = [];
    [Key(18)] public List<SubAbilityAttributes> SubAbilities { get; set; } = [];
    [Key(19)] public string? Description { get; set; }
    [Key(20)] public int? Accumulator { get; set; }
    [Key(21)] public int Noise { get; set; }
    [Key(22)] public List<ConditionalGrantAttributes> ConditionalGrants { get; set; } = [];
}

[MessagePackObject]
public class ConditionalGrantAttributes
{
    [Key(0)] public string Name { get; set; } = "";
    [Key(1)] public int Level { get; set; }
    [Key(2)] public string LevelExpression { get; set; } = "";
}

[MessagePackObject]
public class EffectAttributes
{
    [Key(0)] public EffectType Type { get; set; }
    [Key(1)] public bool ShouldTargetActivator { get; set; }
    [Key(2)] public string? Amount { get; set; }
    [Key(3)] public ValueCombinationFunction Function { get; set; }
    [Key(4)] public string? TargetName { get; set; }
    [Key(5)] public string? SecondaryAmount { get; set; }
    [Key(6)] public EffectDuration Duration { get; set; }
    [Key(7)] public string? DurationAmount { get; set; }
}

[MessagePackObject]
public class SubAbilityAttributes
{
    [Key(0)] public AbilitySuccessCondition SuccessCondition { get; set; }
    [Key(1)] public int Accuracy { get; set; }
    [Key(2)] public Dictionary<int, EffectAttributes> Effects { get; set; } = [];
}

[MessagePackObject]
public class ItemAttributes
{
    [Key(0)] public BitArray? ChangedProperties { get; set; } = new(13);
    [Key(1)] public string? Name { get; set; }
    [Key(2)] public string Description { get; set; } = "";
    [Key(3)] public ItemType Type { get; set; }
    [Key(4)] public Material Material { get; set; }
    [Key(5)] public int Weight { get; set; }
    [Key(6)] public ItemComplexity Complexity { get; set; }
    [Key(7)] public int RequiredMight { get; set; }
    [Key(8)] public int RequiredSpeed { get; set; }
    [Key(9)] public int RequiredFocus { get; set; }
    [Key(10)] public int RequiredPerception { get; set; }
    [Key(11)] public Dictionary<int, EquippableSlot> EquippableSlots { get; set; } = [];
    [Key(12)] public List<AbilityAttributes> Abilities { get; set; } = [];
}

[MessagePackObject]
public class PostGameStatistics
{
}

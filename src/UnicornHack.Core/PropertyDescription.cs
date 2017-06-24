using System;

namespace UnicornHack
{
    public class PropertyDescription
    {
        public Type PropertyType { get; private set; }
        public object MinValue { get; private set; }
        public object MaxValue { get; private set; }
        public PropertyCombinationBehavior CombineUsing { get; private set; }

        // Boons
        public static PropertyDescription SleepResistance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription DecayResistance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription StoningResistance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription SlimingResistance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription SicknessResistance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription FireResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription ColdResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription ElectricityResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription PoisonResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription VenomResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription AcidResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription DisintegrationResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription DrainResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription Reflection { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription FreeAction { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Protection { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription ProtectionFromShapeChangers { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription LifeSaving { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Invulnerability { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Impairments
        public static PropertyDescription SilverWeakness { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription WaterWeakness { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription Sleepiness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Stuning { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Confusion { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Hallucination { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Sickness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Stoning { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Sliming { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Suffocating { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Slickness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Clumsiness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Senses
        public static PropertyDescription InvisibilityDetection { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Infravision { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Telepathy { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription Clairvoyance { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription DangerAwareness { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription CreatureSpeciesAwareness { get; } =
            new PropertyDescription {PropertyType = typeof(string)};

        public static PropertyDescription CreatureClassAwareness { get; } =
            new PropertyDescription {PropertyType = typeof(string)};

        public static PropertyDescription Perception { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Blindness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Deafness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Appearance
        public static PropertyDescription Invisibility { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Infravisibility { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Displacement { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Stealthiness { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription Concealment { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Camouflage { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Transportation
        public static PropertyDescription Jumping { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Teleportation { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription TeleportationControl { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Flight { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription FlightControl { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription WaterWalking { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Swimming { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Phasing { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Amorphism { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Clinginess { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Tunneling { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription ToolTunneling { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Behavior alteration
        public static PropertyDescription MonsterAggravation { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Conflict { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Physical attributes
        public static PropertyDescription Regeneration { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription EnergyRegeneration { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription Reanimation { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription PolymorphControl { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Unchanging { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Lycanthropy { get; } =
            new PropertyDescription {PropertyType = typeof(string)};

        public static PropertyDescription Strength { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 50};

        public static PropertyDescription Agility { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 50 };

        public static PropertyDescription Quickness { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 50 };

        public static PropertyDescription Constitution { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 50 };

        public static PropertyDescription Intelligence { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 50 };

        public static PropertyDescription Willpower { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 50 };

        public static PropertyDescription ThickHide { get; } =
            new PropertyDescription { PropertyType = typeof(int), MinValue = 0, MaxValue = 3 };

        public static PropertyDescription Size { get; } =
            new PropertyDescription { PropertyType = typeof(Size), MinValue = UnicornHack.Size.Tiny, MaxValue = UnicornHack.Size.Gigantic };

        public static PropertyDescription MaxHP { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = Int32.MaxValue};

        public static PropertyDescription MaxEP { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = Int32.MaxValue};

        public static PropertyDescription AbilitySustainment { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Amphibiousness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription WaterBreathing { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Breathlessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription SingularInventory { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription NoInventory { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Eyelessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription SingleEyedness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Handlessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Limblessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Headlessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Mindlessness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Humanoidness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription HumanoidTorso { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription AnimalBody { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription SerpentlikeBody { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription NonSolidBody { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription NonAnimal { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Reproduction
        public static PropertyDescription Asexuality { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Maleness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Femaleness { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Oviparity { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Item properties
        public static PropertyDescription Magicproof { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        public static PropertyDescription Erodeproof { get; } =
            new PropertyDescription {PropertyType = typeof(bool)};

        // Rusted, corroded, burnt, rotten, dissolved depending on material
        public static PropertyDescription Erodedness { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static PropertyDescription ArmorClass { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = -127, MaxValue = 20};

        public static PropertyDescription MagicResistance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 100};

        public static PropertyDescription Hindrance { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 10};

        public static PropertyDescription Enchantment { get; } =
            new PropertyDescription {PropertyType = typeof(int), MinValue = -5, MaxValue = 5};
    }
}
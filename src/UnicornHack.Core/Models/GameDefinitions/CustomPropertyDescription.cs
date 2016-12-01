using System;

namespace UnicornHack.Models.GameDefinitions
{
    public class CustomPropertyDescription
    {
        public Type PropertyType { get; private set; }
        public object MinValue { get; private set; }
        public object MaxValue { get; private set; }

        // Boons
        public static CustomPropertyDescription SleepResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription DecayResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription StoningResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SlimingResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SicknessResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription FireResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription ColdResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription ElectricityResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription PoisonResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription VenomResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription AcidResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription DisintegrationResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription DrainResistance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Reflection { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription FreeAction { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Protection { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription ProtectionFromShapeChangers { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription LifeSaving { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Invulnerability { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Impairments
        public static CustomPropertyDescription SilverWeakness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription WaterWeakness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Sleepiness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Stuning { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Confusion { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Hallucination { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Vomiting { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Sickness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Stoning { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Sliming { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Suffocating { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Slickness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Clumsiness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Senses
        public static CustomPropertyDescription InvisibilityDetection { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Infravision { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Telepathy { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Clairvoyance { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription DangerAwareness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Perception { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Blindness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Deafness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Appearance
        public static CustomPropertyDescription Invisibility { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Infravisibility { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Displacement { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Stealthiness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Concealment { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Camouflage { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Transportation
        public static CustomPropertyDescription Jumping { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Teleportation { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription TeleportationControl { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Flight { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription FlightControl { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription WaterWalking { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Swimming { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Phasing { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Amorphism { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Clinginess { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Tunneling { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription ToolTunneling { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Behavior alteration
        public static CustomPropertyDescription MonsterAggravation { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Conflict { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Nutrition
        public static CustomPropertyDescription Carnivorism { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Herbivorism { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Omnivorism { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Metallivorism { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SlowDigestion { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Hunger { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription FoodPoisoning { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Physical attributes
        public static CustomPropertyDescription Regeneration { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription EnergyRegeneration { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription Reanimation { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription PolymorphControl { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Unchanging { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Lycanthropy { get; } =
            new CustomPropertyDescription {PropertyType = typeof(string)};

        public static CustomPropertyDescription ThickHide { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};

        public static CustomPropertyDescription MaxHP { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = Int32.MaxValue};

        public static CustomPropertyDescription MaxEP { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = Int32.MaxValue};

        public static CustomPropertyDescription AbilitySustainment { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Amphibiousness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription WaterBreathing { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Breathlessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SingularInventory { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription NoInventory { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Eyelessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SingleEyedness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Handlessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Limblessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Headlessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Mindlessness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Humanoidness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription HumanoidTorso { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription AnimalBody { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription SerpentlikeBody { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription NonSolidBody { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription NonAnimal { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Reproduction
        public static CustomPropertyDescription Asexuality { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Maleness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Femaleness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Oviparity { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Item properties
        public static CustomPropertyDescription Magicproof { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        public static CustomPropertyDescription Erodeproof { get; } =
            new CustomPropertyDescription {PropertyType = typeof(bool)};

        // Rusted, corroded, burnt, rotten, dissolved depending on material
        public static CustomPropertyDescription Erodedness { get; } =
            new CustomPropertyDescription {PropertyType = typeof(int), MinValue = 0, MaxValue = 3};
    }
}
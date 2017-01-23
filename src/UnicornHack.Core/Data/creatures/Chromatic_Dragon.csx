new Creature
{
    Name = "Chromatic Dragon",
    Species = Species.Dragon,
    SpeciesClass = SpeciesClass.Reptile,
    MagicResistance = 30,
    MovementRate = 12,
    Weight = 4500,
    Size = Size.Gigantic,
    Nutrition = 1500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "ElementalDamage" } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 18 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "InvisibilityDetection",
        "Infravision",
        "AnimalBody",
        "Handlessness",
        "Femaleness",
        "Carnivorism",
        "StoningResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object>
    {
        {
            "FireResistance",
            3
        },
        {
            "ColdResistance",
            3
        },
        {
            "ElectricityResistance",
            3
        },
        {
            "PoisonResistance",
            3
        },
        {
            "AcidResistance",
            3
        },
        {
            "DangerAwareness",
            3
        },
        {
            "ThickHide",
            3
        }
    }
,
    InitialLevel = 16,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.Covetous,
    Alignment = -14,
    Noise = ActorNoiseType.Quest
}

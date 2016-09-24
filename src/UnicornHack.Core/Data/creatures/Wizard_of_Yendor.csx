new CreatureVariant
{
    InitialLevel = 30,
    ArmorClass = -8,
    MagicResistance = 100,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Noise = ActorNoiseType.Cuss,
    Name = "Wizard of Yendor",
    Species = Species.Human,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Teleportation",
        "TeleportationControl",
        "MagicalBreathing",
        "Infravisibility",
        "InvisibilityDetection",
        "Humanoidness",
        "Maleness",
        "Omnivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "EnergyRegeneration", 3 }, { "Telepathy", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new StealAmulet() }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
}

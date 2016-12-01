new CreatureVariant
{
    Name = "Wizard of Yendor",
    Species = Species.Human,
    InitialLevel = 30,
    ArmorClass = -8,
    MagicResistance = 100,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new StealAmulet() }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Teleportation",
        "TeleportationControl",
        "Breathlessness",
        "Infravisibility",
        "InvisibilityDetection",
        "Humanoidness",
        "Maleness",
        "Omnivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "EnergyRegeneration", 3 }, { "Telepathy", 3 } },
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Noise = ActorNoiseType.Cuss
}

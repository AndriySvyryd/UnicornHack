new CreatureVariant
{
    InitialLevel = 106,
    ArmorClass = -8,
    MagicResistance = 95,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.Covetous,
    Alignment = -20,
    Noise = ActorNoiseType.Growl,
    CorpseVariantName = "",
    Name = "Demogorgon",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Infravisibility",
        "Infravision",
        "InvisibilityDetection",
        "Handlessness",
        "Humanoidness",
        "Maleness"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainLife { Amount = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Infect { Strength = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Infect { Strength = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 18 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { Strength = 4 } } }
    }
}

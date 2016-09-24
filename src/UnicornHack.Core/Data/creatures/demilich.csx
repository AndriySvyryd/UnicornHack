new CreatureVariant
{
    InitialLevel = 14,
    ArmorClass = -2,
    MagicResistance = 60,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -12,
    Noise = ActorNoiseType.Mumble,
    CorpseVariantName = "",
    PreviousStageName = "lich",
    NextStageName = "master lich",
    Name = "demilich",
    Species = Species.Lich,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "SicknessResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Regeneration", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 13 } } }
    }
}

new CreatureVariant
{
    InitialLevel = 11,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -9,
    Noise = ActorNoiseType.Mumble,
    CorpseVariantName = "",
    NextStageName = "demilich",
    Name = "lich",
    Species = Species.Lich,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 6,
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
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 5 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 10 } } }
    }
}

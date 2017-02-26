new Creature
{
    Name = "demilich",
    Species = Species.Lich,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = -2,
    MagicResistance = 60,
    MovementDelay = 133,
    Weight = 600,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ColdDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 13 } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 14,
    PreviousStageName = "lich",
    NextStageName = "master lich",
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -12,
    Noise = ActorNoiseType.Mumble
}

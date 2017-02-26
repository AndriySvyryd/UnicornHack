new Creature
{
    Name = "lich",
    Species = Species.Lich,
    SpeciesClass = SpeciesClass.Undead,
    MagicResistance = 30,
    MovementDelay = 200,
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
            Effects = new HashSet<Effect> { new ColdDamage { Damage = 5 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 10 } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 11,
    NextStageName = "demilich",
    CorpseName = "",
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -9,
    Noise = ActorNoiseType.Mumble
}

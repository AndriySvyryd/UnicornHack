new Creature
{
    Name = "kobold zombie",
    Species = Species.Kobold,
    SpeciesClass = SpeciesClass.Undead,
    ArmorClass = 9,
    MovementDelay = 200,
    Weight = 400,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 1,
    CorpseName = "kobold",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -2,
    Noise = ActorNoiseType.Moan
}

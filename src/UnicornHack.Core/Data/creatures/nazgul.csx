new Creature
{
    Name = "nazgul",
    Species = Species.Wraith,
    SpeciesClass = SpeciesClass.Undead,
    MagicResistance = 25,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Medium,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new DrainLife { Amount = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Sleep { Duration = 26 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new DrainLife { Amount = 1 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "SleepResistance",
        "Flight",
        "FlightControl",
        "Infravision",
        "NonSolidBody",
        "Humanoidness",
        "Breathlessness",
        "Maleness",
        "StoningResistance",
        "SlimingResistance",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 13,
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -17,
    Noise = ActorNoiseType.Howl
}

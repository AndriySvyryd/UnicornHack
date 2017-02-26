new Creature
{
    Name = "vampire",
    Species = Species.Vampire,
    SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
    ArmorClass = 1,
    MagicResistance = 25,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new DrainLife { Amount = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Infect { } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravision", "Humanoidness", "Breathlessness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 10,
    NextStageName = "vampire lord",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Speach
}

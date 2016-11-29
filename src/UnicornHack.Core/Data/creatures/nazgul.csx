new CreatureVariant
{
    InitialLevel = 13,
    MagicResistance = 25,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -17,
    Noise = ActorNoiseType.Howl,
    CorpseVariantName = "",
    Name = "nazgul",
    Species = Species.Wraith,
    SpeciesClass = SpeciesClass.Undead,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravision", "NonSolidBody", "Humanoidness", "Breathlessness", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 }, { "StoningResistance", 3 }, { "SlimingResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Sleep { Duration = 26 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new DrainLife { Amount = 1 } } }
    }
}

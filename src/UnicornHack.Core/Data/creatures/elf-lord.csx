new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 10,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 9,
    Noise = ActorNoiseType.Speach,
    Name = "elf-lord",
    Species = Species.Elf,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 800,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

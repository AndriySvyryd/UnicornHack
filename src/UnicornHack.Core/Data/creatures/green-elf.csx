new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 10,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 6,
    Noise = ActorNoiseType.Speach,
    Name = "green-elf",
    Species = Species.Elf,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 800,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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
    }
}

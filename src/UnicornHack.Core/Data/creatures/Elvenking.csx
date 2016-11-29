new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 10,
    MagicResistance = 25,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 10,
    Noise = ActorNoiseType.Speach,
    Name = "Elvenking",
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

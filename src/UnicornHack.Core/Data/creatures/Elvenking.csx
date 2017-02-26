new Creature
{
    Name = "Elvenking",
    Species = Species.Elf,
    ArmorClass = 10,
    MagicResistance = 25,
    MovementDelay = 100,
    Weight = 800,
    Size = Size.Medium,
    Nutrition = 350,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
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
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    InitialLevel = 9,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 10,
    Noise = ActorNoiseType.Speach
}

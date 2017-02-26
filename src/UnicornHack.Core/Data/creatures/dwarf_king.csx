new Creature
{
    Name = "dwarf king",
    Species = Species.Dwarf,
    ArmorClass = 10,
    MagicResistance = 20,
    MovementDelay = 200,
    Weight = 900,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
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
    SimpleProperties = new HashSet<string> { "ToolTunneling", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    InitialLevel = 6,
    PreviousStageName = "dwarf lord",
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 6,
    Noise = ActorNoiseType.Speach
}

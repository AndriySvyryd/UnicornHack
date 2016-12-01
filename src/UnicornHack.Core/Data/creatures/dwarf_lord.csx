new CreatureVariant
{
    Name = "dwarf lord",
    Species = Species.Dwarf,
    PreviousStageName = "dwarf",
    NextStageName = "dwarf king",
    InitialLevel = 4,
    ArmorClass = 10,
    MagicResistance = 10,
    MovementRate = 6,
    Weight = 900,
    Size = Size.Medium,
    Nutrition = 400,
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
,
    SimpleProperties = new HashSet<string> { "ToolTunneling", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 5,
    Noise = ActorNoiseType.Speach
}

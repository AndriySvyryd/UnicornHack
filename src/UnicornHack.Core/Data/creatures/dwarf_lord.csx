new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 10,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 5,
    Noise = ActorNoiseType.Speach,
    PreviousStageName = "dwarf",
    NextStageName = "dwarf king",
    Name = "dwarf lord",
    Species = Species.Dwarf,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 900,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "ToolTunneling", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
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

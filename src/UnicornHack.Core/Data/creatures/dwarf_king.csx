new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 10,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 6,
    Noise = ActorNoiseType.Speach,
    PreviousStageName = "dwarf lord",
    Name = "dwarf king",
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
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

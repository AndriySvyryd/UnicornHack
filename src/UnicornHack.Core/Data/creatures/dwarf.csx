new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 10,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 4,
    Noise = ActorNoiseType.Speach,
    NextStageName = "dwarf lord",
    Name = "dwarf",
    Species = Species.Dwarf,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 900,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "ToolTunneling", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
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

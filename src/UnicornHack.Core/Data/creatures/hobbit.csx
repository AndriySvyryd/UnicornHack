new CreatureVariant
{
    Name = "hobbit",
    Species = Species.Hobbit,
    InitialLevel = 1,
    ArmorClass = 10,
    MagicResistance = 10,
    MovementRate = 9,
    Weight = 500,
    Size = Size.Medium,
    Nutrition = 250,
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
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 6,
    Noise = ActorNoiseType.Speach
}

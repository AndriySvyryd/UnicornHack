new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 10,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.WeaponCollector,
    Alignment = 6,
    Noise = ActorNoiseType.Speach,
    Name = "hobbit",
    Species = Species.Hobbit,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 500,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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

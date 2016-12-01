new CreatureVariant
{
    Name = "bugbear",
    Species = Species.Bugbear,
    SpeciesClass = SpeciesClass.MagicalBeast,
    InitialLevel = 3,
    ArmorClass = 5,
    MovementRate = 9,
    Weight = 1250,
    Size = Size.Large,
    Nutrition = 250,
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
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -6,
    Noise = ActorNoiseType.Growl
}

new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -6,
    Noise = ActorNoiseType.Growl,
    Name = "bugbear",
    Species = Species.Bugbear,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 9,
    Size = Size.Large,
    Weight = 1250,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

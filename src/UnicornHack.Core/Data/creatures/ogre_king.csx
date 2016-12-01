new CreatureVariant
{
    Name = "ogre king",
    Species = Species.Ogre,
    PreviousStageName = "ogre lord",
    InitialLevel = 9,
    ArmorClass = 4,
    MagicResistance = 60,
    MovementRate = 14,
    Weight = 1700,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
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
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Maleness" },
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -7,
    Noise = ActorNoiseType.Grunt
}

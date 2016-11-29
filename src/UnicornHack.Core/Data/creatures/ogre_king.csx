new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 4,
    MagicResistance = 60,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -7,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "ogre lord",
    Name = "ogre king",
    Species = Species.Ogre,
    MovementRate = 14,
    Size = Size.Large,
    Weight = 1700,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Maleness" },
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
}

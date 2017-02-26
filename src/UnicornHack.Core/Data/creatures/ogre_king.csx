new Creature
{
    Name = "ogre king",
    Species = Species.Ogre,
    ArmorClass = 4,
    MagicResistance = 60,
    MovementDelay = 85,
    Weight = 1700,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Maleness" },
    InitialLevel = 9,
    PreviousStageName = "ogre lord",
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -7,
    Noise = ActorNoiseType.Grunt
}

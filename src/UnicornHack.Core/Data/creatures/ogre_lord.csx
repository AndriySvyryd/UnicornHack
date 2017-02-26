new Creature
{
    Name = "ogre lord",
    Species = Species.Ogre,
    ArmorClass = 3,
    MagicResistance = 30,
    MovementDelay = 100,
    Weight = 1650,
    Size = Size.Large,
    Nutrition = 550,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
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
    InitialLevel = 7,
    PreviousStageName = "ogre",
    NextStageName = "ogre king",
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt
}

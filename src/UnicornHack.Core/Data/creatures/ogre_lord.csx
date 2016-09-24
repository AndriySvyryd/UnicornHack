new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 3,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "ogre",
    NextStageName = "ogre king",
    Name = "ogre lord",
    Species = Species.Ogre,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1650,
    Nutrition = 550,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Maleness" },
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
    }
}

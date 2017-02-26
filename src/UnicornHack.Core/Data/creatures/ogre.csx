new Creature
{
    Name = "ogre",
    Species = Species.Ogre,
    ArmorClass = 5,
    MovementDelay = 120,
    Weight = 1600,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 6 } }
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
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Carnivorism" },
    InitialLevel = 5,
    NextStageName = "ogre lord",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Grunt
}

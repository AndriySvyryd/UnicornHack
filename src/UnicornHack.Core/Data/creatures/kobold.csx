new Creature
{
    Name = "kobold",
    Species = Species.Kobold,
    ArmorClass = 10,
    MovementDelay = 200,
    Weight = 400,
    Size = Size.Small,
    Nutrition = 100,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 5 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 1,
    NextStageName = "large kobold",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -2,
    Noise = ActorNoiseType.Grunt
}

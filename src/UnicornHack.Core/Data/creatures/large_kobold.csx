new Creature
{
    Name = "large kobold",
    Species = Species.Kobold,
    ArmorClass = 10,
    MovementDelay = 200,
    Weight = 450,
    Size = Size.Small,
    Nutrition = 150,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 6 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 2,
    PreviousStageName = "kobold",
    NextStageName = "kobold lord",
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -2,
    Noise = ActorNoiseType.Grunt
}

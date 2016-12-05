new Creature
{
    Name = "gremlin",
    Species = Species.Gremlin,
    ArmorClass = 2,
    MagicResistance = 25,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 20,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new HashSet<Effect> { new Curse { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 3 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Swimming", "Infravisibility", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -5,
    Noise = ActorNoiseType.Laugh
}

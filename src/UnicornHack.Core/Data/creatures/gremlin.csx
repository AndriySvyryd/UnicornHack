new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 2,
    MagicResistance = 25,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -5,
    Noise = ActorNoiseType.Laugh,
    Name = "gremlin",
    Species = Species.Gremlin,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 20,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Swimming", "Infravisibility", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new Curse() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 3 } } }
    }
}

new CreatureVariant
{
    Name = "gremlin",
    Species = Species.Gremlin,
    InitialLevel = 5,
    ArmorClass = 2,
    MagicResistance = 25,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 20,
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
,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Swimming", "Infravisibility", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -5,
    Noise = ActorNoiseType.Laugh
}

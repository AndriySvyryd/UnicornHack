new Creature
{
    Name = "master mind flayer",
    Species = Species.Illithid,
    MagicResistance = 90,
    MovementDelay = 100,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
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
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Suck,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ChangeValuedProperty<int> { PropertyName = "Intelligence", Value = -2, Duration = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Suck,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ChangeValuedProperty<int> { PropertyName = "Intelligence", Value = -2, Duration = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "Telepathy", 3 } },
    InitialLevel = 13,
    PreviousStageName = "mind flayer",
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Gurgle
}

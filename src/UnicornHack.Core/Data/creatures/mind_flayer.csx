new Creature
{
    Name = "mind flayer",
    Species = Species.Illithid,
    ArmorClass = 5,
    MagicResistance = 80,
    MovementRate = 12,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 300,
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
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Suck,
            Timeout = 1,
            Effects = new HashSet<Effect> { new DrainIntelligence { Amount = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "Telepathy", 3 } },
    InitialLevel = 9,
    NextStageName = "master mind flayer",
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Gurgle
}

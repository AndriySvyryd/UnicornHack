new CreatureVariant
{
    Name = "master mind flayer",
    Species = Species.Illithid,
    PreviousStageName = "mind flayer",
    InitialLevel = 13,
    MagicResistance = 90,
    MovementRate = 12,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Suck,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainIntelligence { Amount = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Suck,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainIntelligence { Amount = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "Telepathy", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Gurgle
}

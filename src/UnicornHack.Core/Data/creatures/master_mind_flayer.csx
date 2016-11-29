new CreatureVariant
{
    InitialLevel = 13,
    MagicResistance = 90,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Gurgle,
    PreviousStageName = "mind flayer",
    Name = "master mind flayer",
    Species = Species.Illithid,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1200,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Levitation", "FlightControl", "InvisibilityDetection", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "Telepathy", 3 } },
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
}

new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 5,
    MagicResistance = 80,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -8,
    Noise = ActorNoiseType.Gurgle,
    NextStageName = "master mind flayer",
    Name = "mind flayer",
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
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
    }
}

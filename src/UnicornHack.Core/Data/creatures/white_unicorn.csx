new CreatureVariant
{
    Name = "white unicorn",
    Species = Species.Unicorn,
    SpeciesClass = SpeciesClass.Quadrupedal,
    InitialLevel = 4,
    ArmorClass = 2,
    MagicResistance = 70,
    MovementRate = 24,
    Weight = 1300,
    Size = Size.Large,
    Nutrition = 700,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 6 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.RangedPeaceful | MonsterBehavior.Wandering | MonsterBehavior.GemCollector,
    Alignment = 7,
    Noise = ActorNoiseType.Neigh
}

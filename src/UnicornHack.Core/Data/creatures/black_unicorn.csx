new Creature
{
    Name = "black unicorn",
    Species = Species.Unicorn,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 2,
    MagicResistance = 70,
    MovementRate = 24,
    Weight = 1300,
    Size = Size.Large,
    Nutrition = 700,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 6 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 4,
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.RangedPeaceful | MonsterBehavior.Wandering | MonsterBehavior.GemCollector,
    Alignment = -7,
    Noise = ActorNoiseType.Neigh
}

new CreatureVariant
{
    InitialLevel = 20,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Roar,
    Name = "mastodon",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 3800,
    Nutrition = 800,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 18 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 18 } }
        }
    }
}

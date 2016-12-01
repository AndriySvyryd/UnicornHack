new CreatureVariant
{
    Name = "mastodon",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    InitialLevel = 20,
    ArmorClass = 5,
    MovementRate = 12,
    Weight = 3800,
    Size = Size.Large,
    Nutrition = 800,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 18 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 18 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Roar
}

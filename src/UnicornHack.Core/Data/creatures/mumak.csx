new CreatureVariant
{
    InitialLevel = 5,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -2,
    Noise = ActorNoiseType.Roar,
    Name = "mumak",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 9,
    Size = Size.Large,
    Weight = 2500,
    Nutrition = 1000,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}

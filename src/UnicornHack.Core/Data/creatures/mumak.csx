new CreatureVariant
{
    Name = "mumak",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    InitialLevel = 5,
    MovementRate = 9,
    Weight = 2500,
    Size = Size.Large,
    Nutrition = 1000,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -2,
    Noise = ActorNoiseType.Roar
}

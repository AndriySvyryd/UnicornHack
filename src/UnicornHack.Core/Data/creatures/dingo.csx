new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Bark,
    Name = "dingo",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 16,
    Size = Size.Medium,
    Weight = 400,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}

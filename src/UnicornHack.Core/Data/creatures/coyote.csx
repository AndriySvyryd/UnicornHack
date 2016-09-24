new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Bark,
    Name = "coyote",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 300,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

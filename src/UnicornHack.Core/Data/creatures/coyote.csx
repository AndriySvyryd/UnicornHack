new CreatureVariant
{
    Name = "coyote",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    InitialLevel = 1,
    ArmorClass = 7,
    MovementRate = 12,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 250,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Bark
}

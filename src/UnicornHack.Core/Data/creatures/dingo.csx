new CreatureVariant
{
    Name = "dingo",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    InitialLevel = 4,
    ArmorClass = 7,
    MovementRate = 16,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 200,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Bark
}

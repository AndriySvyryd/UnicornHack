new Creature
{
    Name = "jackal",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 7,
    MovementDelay = 100,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 250,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 1,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Bark
}

new Creature
{
    Name = "fox",
    Species = Species.Fox,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 7,
    MovementRate = 15,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 1,
    GenerationFrequency = Frequency.Often,
    Noise = ActorNoiseType.Bark
}

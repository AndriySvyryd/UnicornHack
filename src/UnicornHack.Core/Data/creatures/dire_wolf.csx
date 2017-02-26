new Creature
{
    Name = "dire wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 3,
    MovementDelay = 100,
    Weight = 1200,
    Size = Size.Medium,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 7,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Bark
}

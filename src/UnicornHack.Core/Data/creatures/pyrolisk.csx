new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Hiss,
    Name = "pyrolisk",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 30,
    Nutrition = 30,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "FireResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Gaze,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 7 } }
        }
    }
}

new CreatureVariant
{
    Name = "pyrolisk",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    InitialLevel = 6,
    ArmorClass = 6,
    MagicResistance = 30,
    MovementRate = 6,
    Weight = 30,
    Size = Size.Small,
    Nutrition = 30,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "FireResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Hiss
}

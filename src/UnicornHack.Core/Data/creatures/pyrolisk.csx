new Creature
{
    Name = "pyrolisk",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    ArmorClass = 6,
    MagicResistance = 30,
    MovementRate = 6,
    Weight = 30,
    Size = Size.Small,
    Nutrition = 30,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 1,
            Effects = new HashSet<Effect> { new FireDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "FireResistance", 3 } },
    InitialLevel = 6,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Hiss
}

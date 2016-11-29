new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 4,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Uncommonly,
    PreviousStageName = "baby crocodile",
    Name = "crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 9,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
}

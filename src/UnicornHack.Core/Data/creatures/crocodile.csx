new CreatureVariant
{
    Name = "crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    PreviousStageName = "baby crocodile",
    InitialLevel = 8,
    ArmorClass = 4,
    MagicResistance = 10,
    MovementRate = 9,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 500,
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
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Uncommonly
}

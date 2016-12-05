new Creature
{
    Name = "crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 4,
    MagicResistance = 10,
    MovementRate = 9,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 8,
    PreviousStageName = "baby crocodile",
    GenerationFrequency = Frequency.Uncommonly
}

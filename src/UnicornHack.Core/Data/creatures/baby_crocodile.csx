new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Uncommonly,
    NextStageName = "crocodile",
    Name = "baby crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 200,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 6 } }
        }
    }
}

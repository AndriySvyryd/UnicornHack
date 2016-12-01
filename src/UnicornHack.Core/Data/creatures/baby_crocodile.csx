new CreatureVariant
{
    Name = "baby crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    NextStageName = "crocodile",
    InitialLevel = 6,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 6 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Uncommonly
}

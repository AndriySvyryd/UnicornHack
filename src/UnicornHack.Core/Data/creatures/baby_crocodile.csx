new Creature
{
    Name = "baby crocodile",
    Species = Species.Crocodile,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 6,
    MovementDelay = 200,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 6 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 6,
    NextStageName = "crocodile",
    GenerationFrequency = Frequency.Uncommonly
}

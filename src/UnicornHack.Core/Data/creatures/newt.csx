new CreatureVariant
{
    Name = "newt",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    InitialLevel = 1,
    ArmorClass = 8,
    MovementRate = 6,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Commonly
}

new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 8,
    GenerationFrequency = Frequency.Commonly,
    Name = "newt",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 6,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

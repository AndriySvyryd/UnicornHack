new Creature
{
    Name = "newt",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 8,
    MovementRate = 6,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Amphibiousness", "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    InitialLevel = 1,
    GenerationFrequency = Frequency.Commonly
}

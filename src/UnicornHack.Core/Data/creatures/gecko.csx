new CreatureVariant
{
    Name = "gecko",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    InitialLevel = 2,
    ArmorClass = 8,
    MovementRate = 6,
    Weight = 15,
    Size = Size.Tiny,
    Nutrition = 15,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Sometimes
}

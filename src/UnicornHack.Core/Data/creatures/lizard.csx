new CreatureVariant
{
    Name = "lizard",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    InitialLevel = 4,
    ArmorClass = 6,
    MovementRate = 6,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Sometimes
}

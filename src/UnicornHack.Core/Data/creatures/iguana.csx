new Creature
{
    Name = "iguana",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 7,
    MovementDelay = 200,
    Weight = 50,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Handlessness", "Oviparity", "Carnivorism", "SingularInventory" },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Sometimes
}

new Creature
{
    Name = "gecko",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile,
    ArmorClass = 8,
    MovementDelay = 200,
    Weight = 15,
    Size = Size.Tiny,
    Nutrition = 15,
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
    InitialLevel = 2,
    GenerationFrequency = Frequency.Sometimes
}

new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 6,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Uncommonly,
    Name = "chameleon",
    Species = Species.Lizard,
    SpeciesClass = SpeciesClass.Reptile | SpeciesClass.ShapeChanger,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 50,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Handlessness", "PolymorphControl", "Oviparity", "Carnivorism", "SingularInventory" },
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
}

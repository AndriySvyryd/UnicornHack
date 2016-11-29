new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Sqeek,
    Name = "rabid rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainConstitution { Amount = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 3 } } }
    }
}

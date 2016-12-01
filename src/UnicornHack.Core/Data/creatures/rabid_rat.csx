new CreatureVariant
{
    Name = "rabid rat",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent,
    InitialLevel = 3,
    ArmorClass = 6,
    MovementRate = 12,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 50,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Sqeek
}

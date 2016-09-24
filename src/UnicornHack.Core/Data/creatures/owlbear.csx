new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Roar,
    Name = "owlbear",
    Species = Species.Simian,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1700,
    Nutrition = 700,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Carnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 2 } }
        }
    }
}

new Creature
{
    Name = "owlbear",
    Species = Species.Simian,
    SpeciesClass = SpeciesClass.MagicalBeast,
    ArmorClass = 5,
    MovementDelay = 100,
    Weight = 1700,
    Size = Size.Large,
    Nutrition = 700,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Bind { Duration = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Humanoidness", "Carnivorism" },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Roar
}

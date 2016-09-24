new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss,
    Name = "python",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 3,
    Size = Size.Medium,
    Weight = 250,
    Nutrition = 125,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
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

new CreatureVariant
{
    Name = "python",
    Species = Species.Snake,
    SpeciesClass = SpeciesClass.Reptile,
    InitialLevel = 6,
    ArmorClass = 5,
    MovementRate = 3,
    Weight = 250,
    Size = Size.Medium,
    Nutrition = 125,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Bind { Duration = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravision", "SerpentlikeBody", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    GenerationFrequency = Frequency.Uncommonly,
    Noise = ActorNoiseType.Hiss
}

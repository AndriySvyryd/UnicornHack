new Creature
{
    Name = "baby long worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 5,
    MovementDelay = 400,
    Weight = 600,
    Size = Size.Medium,
    Nutrition = 250,
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
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 2,
    NextStageName = "long worm",
    GenerationFrequency = Frequency.Commonly
}

new Creature
{
    Name = "baby purple worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 5,
    MovementRate = 3,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 4,
    NextStageName = "purple worm",
    GenerationFrequency = Frequency.Commonly
}

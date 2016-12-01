new CreatureVariant
{
    Name = "baby purple worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    NextStageName = "purple worm",
    InitialLevel = 4,
    ArmorClass = 5,
    MovementRate = 3,
    Weight = 600,
    Size = Size.Medium,
    Nutrition = 250,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Commonly
}

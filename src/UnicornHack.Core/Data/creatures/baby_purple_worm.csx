new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Commonly,
    NextStageName = "purple worm",
    Name = "baby purple worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 3,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}

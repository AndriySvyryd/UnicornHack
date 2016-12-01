new CreatureVariant
{
    Name = "long worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    PreviousStageName = "baby long worm",
    InitialLevel = 9,
    ArmorClass = 5,
    MagicResistance = 10,
    MovementRate = 3,
    Weight = 1500,
    Size = Size.Gigantic,
    Nutrition = 500,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Commonly
}

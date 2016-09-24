new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 5,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    PreviousStageName = "baby long worm",
    Name = "long worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 3,
    Size = Size.Gigantic,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}

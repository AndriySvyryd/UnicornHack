new Creature
{
    Name = "long worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 5,
    MagicResistance = 10,
    MovementRate = 3,
    Weight = 1500,
    Size = Size.Gigantic,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 9,
    PreviousStageName = "baby long worm",
    GenerationFrequency = Frequency.Commonly
}

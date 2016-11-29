new CreatureVariant
{
    InitialLevel = 15,
    ArmorClass = 5,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Commonly,
    PreviousStageName = "baby purple worm",
    Name = "purple worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 9,
    Size = Size.Gigantic,
    Weight = 1500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new AcidDamage { Damage = 5 } }
        }
    }
}

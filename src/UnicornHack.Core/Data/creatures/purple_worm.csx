new Creature
{
    Name = "purple worm",
    Species = Species.Worm,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 5,
    MagicResistance = 20,
    MovementDelay = 133,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Engulf { Duration = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new HashSet<Effect> { new AcidDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SerpentlikeBody", "Eyelessness", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 15,
    PreviousStageName = "baby purple worm",
    GenerationFrequency = Frequency.Commonly
}

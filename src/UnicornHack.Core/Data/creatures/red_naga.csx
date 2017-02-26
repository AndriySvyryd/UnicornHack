new Creature
{
    Name = "red naga",
    Species = Species.Naga,
    SpeciesClass = SpeciesClass.Aberration,
    ArmorClass = 4,
    MovementDelay = 100,
    Weight = 1500,
    Size = Size.Huge,
    Nutrition = 600,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spit,
            Timeout = 1,
            Effects = new HashSet<Effect> { new FireDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "SerpentlikeBody", "Limblessness", "Carnivorism", "Oviparity", "SingularInventory", "SlimingResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "ThickHide", 3 } },
    InitialLevel = 6,
    PreviousStageName = "red naga hatchling",
    GenerationFrequency = Frequency.Uncommonly,
    Alignment = -4,
    Noise = ActorNoiseType.Hiss
}

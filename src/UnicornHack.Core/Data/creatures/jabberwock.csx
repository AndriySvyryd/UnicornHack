new CreatureVariant
{
    InitialLevel = 15,
    ArmorClass = -2,
    MagicResistance = 50,
    GenerationFrequency = Frequency.Rarely,
    Noise = ActorNoiseType.Burble,
    Name = "jabberwock",
    Species = Species.Jabberwock,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1300,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility", "AnimalBody", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 11 } }
        }
    }
}

new CreatureVariant
{
    InitialLevel = 15,
    ArmorClass = -1,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Mountable | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
    Alignment = 4,
    Noise = ActorNoiseType.Roar,
    PreviousStageName = "baby red dragon",
    Name = "red dragon",
    Species = Species.Dragon,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 9,
    Size = Size.Gigantic,
    Weight = 4500,
    Nutrition = 1500,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "InvisibilityDetection",
        "Infravision",
        "AnimalBody",
        "Handlessness",
        "Carnivorism",
        "Oviparity",
        "SingularInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "DangerAwareness", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 14 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}

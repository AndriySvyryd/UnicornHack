new Creature
{
    Name = "tengu",
    Species = Species.Tengu,
    SpeciesClass = SpeciesClass.ShapeChanger,
    ArmorClass = 5,
    MagicResistance = 30,
    MovementDelay = 92,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 150,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 2 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Teleport { } } }
    }
,
    SimpleProperties = new HashSet<string> { "Teleportation", "TeleportationControl", "Infravisibility", "Infravision" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 6,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Alignment = 7,
    Noise = ActorNoiseType.Squawk
}

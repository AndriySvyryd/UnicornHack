new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 5,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Alignment = 7,
    Noise = ActorNoiseType.Squawk,
    Name = "tengu",
    Species = Species.Tengu,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 13,
    Size = Size.Small,
    Weight = 300,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "Teleportation", "TeleportationControl", "Infravisibility", "Infravision" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Teleport() } }
    }
}

new CreatureVariant
{
    Name = "tengu",
    Species = Species.Tengu,
    SpeciesClass = SpeciesClass.ShapeChanger,
    InitialLevel = 6,
    ArmorClass = 5,
    MagicResistance = 30,
    MovementRate = 13,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 150,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Teleport() } }
    }
,
    SimpleProperties = new HashSet<string> { "Teleportation", "TeleportationControl", "Infravisibility", "Infravision" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Alignment = 7,
    Noise = ActorNoiseType.Squawk
}

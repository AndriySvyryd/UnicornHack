new Creature
{
    Name = "Medusa",
    Species = Species.Human,
    ArmorClass = 2,
    MagicResistance = 50,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Gaze, Timeout = 1, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 10 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Amphibiousness",
        "Infravisibility",
        "Humanoidness",
        "Femaleness",
        "Omnivorism",
        "StoningResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful,
    Alignment = -15,
    Noise = ActorNoiseType.Hiss
}

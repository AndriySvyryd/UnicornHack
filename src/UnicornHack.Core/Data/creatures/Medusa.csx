new CreatureVariant
{
    InitialLevel = 20,
    ArmorClass = 2,
    MagicResistance = 50,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful,
    Alignment = -15,
    Noise = ActorNoiseType.Hiss,
    Name = "Medusa",
    Species = Species.Human,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Amphibiousness", "Infravisibility", "Humanoidness", "Femaleness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 3 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Gaze, Timeout = 1, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 10 } } }
    }
}

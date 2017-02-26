new Creature
{
    Name = "pit fiend",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = -3,
    MagicResistance = 65,
    MovementDelay = 200,
    Weight = 1600,
    Size = Size.Large,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 6 } }
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
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Hug,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 4 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "InvisibilityDetection", "Infravisibility", "Humanoidness", "SicknessResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 13,
    PreviousStageName = "vrock",
    NextStageName = "balrog",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = -13,
    Noise = ActorNoiseType.Growl
}

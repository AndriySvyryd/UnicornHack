new Creature
{
    Name = "marilith",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = -6,
    MagicResistance = 80,
    MovementDelay = 100,
    Weight = 1200,
    Size = Size.Large,
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
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 3 } } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Infravision",
        "InvisibilityDetection",
        "Infravisibility",
        "HumanoidTorso",
        "SerpentlikeBody",
        "Femaleness",
        "SicknessResistance"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 } },
    InitialLevel = 9,
    PreviousStageName = "hezrou",
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -12,
    Noise = ActorNoiseType.Cuss
}

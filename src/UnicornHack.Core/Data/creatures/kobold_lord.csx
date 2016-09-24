new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -2,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "large kobold",
    Name = "kobold lord",
    Species = Species.Kobold,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 500,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
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
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 6 } } }
    }
}

new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 10,
    GenerationFrequency = Frequency.Usually,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -2,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "kobold",
    NextStageName = "kobold lord",
    Name = "large kobold",
    Species = Species.Kobold,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 450,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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

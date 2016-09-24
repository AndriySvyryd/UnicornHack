new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 10,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach,
    NextStageName = "ranger",
    Name = "hunter",
    Species = Species.Human,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "DangerAwareness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

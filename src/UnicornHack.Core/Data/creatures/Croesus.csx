new CreatureVariant
{
    InitialLevel = 20,
    MagicResistance = 40,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 15,
    Noise = ActorNoiseType.Guard,
    Name = "Croesus",
    Species = Species.Human,
    MovementRate = 15,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "InvisibilityDetection", "Humanoidness", "Maleness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 22 } }
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

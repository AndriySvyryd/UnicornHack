new Creature
{
    Name = "Croesus",
    Species = Species.Human,
    MagicResistance = 40,
    MovementRate = 15,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 22 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "InvisibilityDetection", "Humanoidness", "Maleness", "Omnivorism" },
    InitialLevel = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 15,
    Noise = ActorNoiseType.Guard
}

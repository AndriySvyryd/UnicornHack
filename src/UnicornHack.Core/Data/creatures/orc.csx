new Creature
{
    Name = "orc",
    Species = Species.Orc,
    ArmorClass = 10,
    MovementDelay = 133,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
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
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 1,
    NextStageName = "orc captain",
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -4,
    Noise = ActorNoiseType.Grunt
}

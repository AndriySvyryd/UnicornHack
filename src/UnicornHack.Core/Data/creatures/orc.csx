new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -4,
    Noise = ActorNoiseType.Grunt,
    NextStageName = "orc captain",
    Name = "orc",
    Species = Species.Orc,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

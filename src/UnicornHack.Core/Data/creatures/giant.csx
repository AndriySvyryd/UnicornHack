new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 6,
    GenerationFlags = GenerationFlags.NonGenocidable,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = 2,
    Noise = ActorNoiseType.Boast,
    Name = "giant",
    Species = Species.Giant,
    MovementRate = 6,
    Size = Size.Huge,
    Weight = 2250,
    Nutrition = 750,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 11 } }
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

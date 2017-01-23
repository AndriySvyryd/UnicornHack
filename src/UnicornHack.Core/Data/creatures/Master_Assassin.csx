new Creature
{
    Name = "Master Assassin",
    Species = Species.Human,
    MagicResistance = 30,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new VenomDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Maleness", "Omnivorism", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    InitialLevel = 16,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = 16,
    Noise = ActorNoiseType.Quest
}

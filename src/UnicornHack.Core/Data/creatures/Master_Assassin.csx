new CreatureVariant
{
    InitialLevel = 16,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = 16,
    Noise = ActorNoiseType.Quest,
    Name = "Master Assassin",
    Species = Species.Human,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new VenomDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new StealAmulet() }
        }
    }
}

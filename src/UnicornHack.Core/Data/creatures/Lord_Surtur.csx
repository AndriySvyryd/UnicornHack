new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = 2,
    MagicResistance = 40,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = 12,
    Noise = ActorNoiseType.Quest,
    Name = "Lord Surtur",
    Species = Species.Giant,
    MovementRate = 12,
    Size = Size.Huge,
    Weight = 2200,
    Nutrition = 800,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new StealAmulet() }
        }
    }
}

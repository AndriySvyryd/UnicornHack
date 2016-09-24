new CreatureVariant
{
    InitialLevel = 12,
    MagicResistance = 50,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful | MonsterBehavior.Displacing | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Bribeable,
    Noise = ActorNoiseType.Sell,
    Name = "shopkeeper",
    Species = Species.Human,
    MovementRate = 18,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
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
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}

new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 8,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = -4,
    Noise = ActorNoiseType.Grunt,
    Name = "kobold shaman",
    Species = Species.Kobold,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 450,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 6 } } }
    }
}

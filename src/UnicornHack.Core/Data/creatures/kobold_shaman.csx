new CreatureVariant
{
    Name = "kobold shaman",
    Species = Species.Kobold,
    InitialLevel = 3,
    ArmorClass = 8,
    MagicResistance = 10,
    MovementRate = 6,
    Weight = 450,
    Size = Size.Small,
    Nutrition = 150,
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
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = -4,
    Noise = ActorNoiseType.Grunt
}

new CreatureVariant
{
    Name = "orc shaman",
    Species = Species.Orc,
    InitialLevel = 3,
    ArmorClass = 10,
    MagicResistance = 10,
    MovementRate = 9,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt
}

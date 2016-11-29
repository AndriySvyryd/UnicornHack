new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 10,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt,
    Name = "orc shaman",
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
}

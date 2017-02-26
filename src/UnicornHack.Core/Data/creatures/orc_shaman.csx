new Creature
{
    Name = "orc shaman",
    Species = Species.Orc,
    ArmorClass = 10,
    MagicResistance = 10,
    MovementDelay = 133,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt
}

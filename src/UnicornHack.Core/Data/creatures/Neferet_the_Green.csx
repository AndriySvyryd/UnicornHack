new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = 10,
    MagicResistance = 70,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful | MonsterBehavior.GoldCollector | MonsterBehavior.MagicUser,
    Noise = ActorNoiseType.Quest,
    Name = "Neferet the Green",
    Species = Species.Human,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Femaleness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 }, { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
    }
}

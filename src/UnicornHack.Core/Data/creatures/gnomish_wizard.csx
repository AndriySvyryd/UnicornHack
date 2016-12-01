new CreatureVariant
{
    Name = "gnomish wizard",
    Species = Species.Gnome,
    InitialLevel = 3,
    ArmorClass = 10,
    MagicResistance = 20,
    MovementRate = 8,
    Weight = 700,
    Size = Size.Small,
    Nutrition = 250,
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
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Noise = ActorNoiseType.Speach
}

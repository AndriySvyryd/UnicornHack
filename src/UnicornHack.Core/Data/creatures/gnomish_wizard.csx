new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 10,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Noise = ActorNoiseType.Speach,
    Name = "gnomish wizard",
    Species = Species.Gnome,
    MovementRate = 8,
    Size = Size.Small,
    Weight = 700,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
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

new CreatureVariant
{
    Name = "Oracle",
    Species = Species.Human,
    InitialLevel = 12,
    MagicResistance = 50,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Femaleness" },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful,
    Noise = ActorNoiseType.Oracle
}

new CreatureVariant
{
    InitialLevel = 12,
    MagicResistance = 50,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful,
    Noise = ActorNoiseType.Oracle,
    Name = "Oracle",
    Species = Species.Human,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Femaleness" },
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
}

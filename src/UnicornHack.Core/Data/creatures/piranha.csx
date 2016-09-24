new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 4,
    GenerationFlags = GenerationFlags.SmallGroup,
    Name = "piranha",
    Species = Species.Fish,
    MovementRate = 12,
    Size = Size.Tiny,
    Weight = 60,
    Nutrition = 30,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}

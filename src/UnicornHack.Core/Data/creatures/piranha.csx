new CreatureVariant
{
    Name = "piranha",
    Species = Species.Fish,
    InitialLevel = 5,
    ArmorClass = 4,
    MovementRate = 12,
    Weight = 60,
    Size = Size.Tiny,
    Nutrition = 30,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    GenerationFlags = GenerationFlags.SmallGroup
}

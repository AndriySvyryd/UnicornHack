new Creature
{
    Name = "piranha",
    Species = Species.Fish,
    ArmorClass = 4,
    MovementDelay = 100,
    Weight = 60,
    Size = Size.Tiny,
    Nutrition = 30,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    InitialLevel = 5,
    GenerationFlags = GenerationFlags.SmallGroup
}

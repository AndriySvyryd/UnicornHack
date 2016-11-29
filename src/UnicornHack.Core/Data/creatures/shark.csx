new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 2,
    Name = "shark",
    Species = Species.Fish,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 17 } }
        }
    }
}

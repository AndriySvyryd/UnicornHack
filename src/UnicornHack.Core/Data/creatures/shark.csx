new Creature
{
    Name = "shark",
    Species = Species.Fish,
    ArmorClass = 2,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Large,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 17 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "WaterBreathing", "Limblessness", "Oviparity", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 7
}

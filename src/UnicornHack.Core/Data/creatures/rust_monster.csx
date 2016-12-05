new Creature
{
    Name = "rust monster",
    Species = Species.RustMonster,
    ArmorClass = 2,
    MovementRate = 18,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new WaterDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new WaterDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new WaterDamage { Damage = 10 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Occasionally
}

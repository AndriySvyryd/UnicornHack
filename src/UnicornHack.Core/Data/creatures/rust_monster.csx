new CreatureVariant
{
    Name = "rust monster",
    Species = Species.RustMonster,
    InitialLevel = 5,
    ArmorClass = 2,
    MovementRate = 18,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new WaterDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new WaterDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new WaterDamage { Damage = 10 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Occasionally
}

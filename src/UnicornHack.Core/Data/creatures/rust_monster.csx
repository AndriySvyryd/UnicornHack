new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 2,
    GenerationFrequency = Frequency.Occasionally,
    Name = "rust monster",
    Species = Species.RustMonster,
    MovementRate = 18,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new WaterDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new WaterDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new WaterDamage { Damage = 10 } } }
    }
}

new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
    Name = "woodchuck",
    Species = Species.Woodchuck,
    SpeciesClass = SpeciesClass.Rodent,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Swimming", "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}

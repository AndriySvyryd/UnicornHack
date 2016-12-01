new CreatureVariant
{
    Name = "woodchuck",
    Species = Species.Woodchuck,
    SpeciesClass = SpeciesClass.Rodent,
    InitialLevel = 3,
    ArmorClass = 5,
    MovementRate = 3,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
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
,
    SimpleProperties = new HashSet<string> { "Swimming", "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
}

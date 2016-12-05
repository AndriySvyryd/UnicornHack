new Creature
{
    Name = "woodchuck",
    Species = Species.Woodchuck,
    SpeciesClass = SpeciesClass.Rodent,
    ArmorClass = 5,
    MovementRate = 3,
    Weight = 100,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
}

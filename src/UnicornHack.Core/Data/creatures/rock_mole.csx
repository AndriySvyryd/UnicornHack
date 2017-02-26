new Creature
{
    Name = "rock mole",
    Species = Species.Mole,
    SpeciesClass = SpeciesClass.Rodent,
    MagicResistance = 20,
    MovementDelay = 400,
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
    SimpleProperties = new HashSet<string> { "Tunneling", "AnimalBody", "Infravisibility", "Handlessness", "Metallivorism", "SingularInventory" },
    InitialLevel = 3,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector
}

new CreatureVariant
{
    InitialLevel = 3,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector,
    Name = "rock mole",
    Species = Species.Mole,
    SpeciesClass = SpeciesClass.Rodent,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Tunneling", "AnimalBody", "Infravisibility", "Handlessness", "Metallivorism", "SingularInventory" },
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

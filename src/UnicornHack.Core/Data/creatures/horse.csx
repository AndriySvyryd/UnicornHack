new CreatureVariant
{
    Name = "horse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    PreviousStageName = "pony",
    NextStageName = "warhorse",
    InitialLevel = 5,
    ArmorClass = 5,
    MovementRate = 20,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 1100,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}

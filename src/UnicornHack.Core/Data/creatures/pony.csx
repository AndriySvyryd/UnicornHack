new Creature
{
    Name = "pony",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 6,
    MovementRate = 16,
    Weight = 1300,
    Size = Size.Medium,
    Nutrition = 900,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    InitialLevel = 3,
    NextStageName = "horse",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}

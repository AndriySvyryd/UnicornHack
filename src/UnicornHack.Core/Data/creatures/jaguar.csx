new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Growl,
    Name = "jaguar",
    Species = Species.BigCat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 600,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}

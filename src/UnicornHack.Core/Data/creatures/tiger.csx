new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Growl,
    Name = "tiger",
    Species = Species.BigCat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 14,
    Size = Size.Large,
    Weight = 600,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}

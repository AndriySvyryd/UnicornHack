new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Occasionally,
    Name = "rock piercer",
    Species = Species.Piercer,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 200,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}

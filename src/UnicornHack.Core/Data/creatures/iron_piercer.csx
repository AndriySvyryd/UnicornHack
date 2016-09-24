new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 2,
    GenerationFrequency = Frequency.Occasionally,
    Name = "iron piercer",
    Species = Species.Piercer,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 300,
    Nutrition = 150,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
}

new CreatureVariant
{
    Name = "rock piercer",
    Species = Species.Piercer,
    InitialLevel = 3,
    ArmorClass = 3,
    MovementRate = 1,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 100,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Occasionally
}

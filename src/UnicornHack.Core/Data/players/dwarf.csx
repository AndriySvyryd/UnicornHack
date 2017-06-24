new PlayerRace
{
    Name = "dwarf",
    Species = Species.Dwarf,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Always,
            Effects = new HashSet<Effect>
            {
                new ChangeSimpleProperty { PropertyName = "Infravision" },
                new ChangeSimpleProperty { PropertyName = "Infravisibility" },
                new ChangeSimpleProperty { PropertyName = "Humanoidness" },
                new ChangeValuedProperty<int> { PropertyName = "Strength", Value = 1 },
                new ChangeValuedProperty<int> { PropertyName = "Agility", Value = -1 },
                new ChangeValuedProperty<int> { PropertyName = "Constitution", Value = 1 },
                new ChangeValuedProperty<int> { PropertyName = "Quickness", Value = -1 }
            }
        }
    }
}

new PlayerRace
{
    Name = "ogre",
    Species = Species.Ogre,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Always,
            Effects = new HashSet<Effect>
            {
                new ChangeSimpleProperty { PropertyName = "Infravisibility" },
                new ChangeSimpleProperty { PropertyName = "Humanoidness" },
                new ChangeValuedProperty<int> { PropertyName = "Strength", Value = 2 },
                new ChangeValuedProperty<int> { PropertyName = "Agility", Value = -2 },
                new ChangeValuedProperty<int> { PropertyName = "Constitution", Value = 2 },
                new ChangeValuedProperty<int> { PropertyName = "Quickness", Value = -2 },
                new ChangeValuedProperty<Size> { PropertyName = "Size", Value = Size.Large, IsAbsolute = true }
            }
        }
    }
}

new PlayerRace
{
    Name = "human",
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Always,
            Effects = new HashSet<Effect> { new ChangeSimpleProperty { PropertyName = "Infravisibility" }, new ChangeSimpleProperty { PropertyName = "Humanoidness" } }
        }
    }
}

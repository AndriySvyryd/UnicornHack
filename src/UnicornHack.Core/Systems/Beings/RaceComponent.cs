namespace UnicornHack.Systems.Beings;

[Component(Id = (int)EntityComponent.Race)]
public class RaceComponent : GameComponent
{
    private string _templateName = null!;
    private Species _species;
    private SpeciesClass _speciesClass;
    private int _level;

    public RaceComponent()
    {
        ComponentId = (int)EntityComponent.Race;
    }

    public string TemplateName
    {
        get => _templateName;
        set => SetWithNotify(value, ref _templateName);
    }

    public Species Species
    {
        get => _species;
        set => SetWithNotify(value, ref _species);
    }

    public SpeciesClass SpeciesClass
    {
        get => _speciesClass;
        set => SetWithNotify(value, ref _speciesClass);
    }

    public int Level
    {
        get => _level;
        set => SetWithNotify(value, ref _level);
    }

    protected override void Clean()
    {
        _templateName = default!;
        _species = default;
        _speciesClass = default;
        _level = default;

        base.Clean();
    }
}

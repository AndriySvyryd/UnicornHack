namespace UnicornHack.Systems.Actors;

[Component(Id = (int)EntityComponent.AI)]
public class AIComponent : ActorComponent
{
    private AIBehavior _behavior;
    private ActorNoiseType _noise;

    public AIComponent()
    {
        ComponentId = (int)EntityComponent.AI;
    }

    public AIBehavior Behavior
    {
        get => _behavior;
        set => SetWithNotify(value, ref _behavior);
    }

    public ActorNoiseType Noise
    {
        get => _noise;
        set => SetWithNotify(value, ref _noise);
    }

    public Dictionary<Point, byte>? ThreatenedCells { get; set; }

    protected override void Clean()
    {
        ThreatenedCells?.Clear();
        ThreatenedCells = null;
        _behavior = default;
        _noise = default;

        base.Clean();
    }
}

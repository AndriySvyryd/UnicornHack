using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;

namespace UnicornHack.Services.LogEvents;

public readonly struct ActorActionFailedEvent
{
    public ActorActionFailedEvent(
        GameEntity sensorEntity, GameEntity actorEntity, SenseType actorSensed,
        ActorActionType action, Direction? direction = null, AbilityComponent? ability = null)
    {
        SensorEntity = sensorEntity;
        ActorEntity = actorEntity;
        ActorSensed = actorSensed;
        Action = action;
        Direction = direction;
        Ability = ability;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity ActorEntity
    {
        get;
    }

    public SenseType ActorSensed
    {
        get;
    }

    public ActorActionType Action
    {
        get;
    }

    public Direction? Direction
    {
        get;
    }

    public AbilityComponent? Ability
    {
        get;
    }
}

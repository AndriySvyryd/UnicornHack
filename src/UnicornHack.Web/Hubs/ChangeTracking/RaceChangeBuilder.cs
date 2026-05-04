using System.Collections;
using UnicornHack.Services;

namespace UnicornHack.Hubs.ChangeTracking;

public class RaceChangeBuilder : ChangeBuilder<RaceChange>
{
    private readonly int _playerEntityId;

    public RaceChangeBuilder(int playerEntityId)
    {
        _playerEntityId = playerEntityId;
    }

    public override void RegisterOnGroups(GameManager manager)
    {
        manager.Races.AddListener(this);
        manager.RacesToBeingRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.Races.RemoveListener(this);
        manager.RacesToBeingRelationship.RemoveDependentsListener(this);
    }

    protected override bool IsRelevantPrincipal(GameEntity principal)
        => principal.Id == _playerEntityId;

    protected override RaceChange ProduceFullSnapshot(GameEntity entity, SerializationContext context)
    {
        var snapshot = SerializeRace(entity, context.Services);
        snapshot.ChangedProperties = null;
        return snapshot;
    }

    // Races re-serialize from scratch for deltas — SerializeRace produces a RaceChange with all bits set.
    protected override RaceChange? ProduceDelta(GameEntity entity, RaceChange tracked, SerializationContext context)
    {
        if (entity.Race == null)
        {
            // Stale reference — emit a removal sentinel.
            return new RaceChange { ChangedProperties = new BitArray(1) };
        }

        return SerializeRace(entity, context.Services);
    }

    public static RaceChange SerializeRace(GameEntity raceEntity, GameServices services)
    {
        var race = raceEntity.Race!;
        return new RaceChange
        {
            Name = services.Language.GetString(race, abbreviate: false),
            ShortName = services.Language.GetString(race, abbreviate: true)
        };
    }
}

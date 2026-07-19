using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Effects;

public sealed class EngulferToVictimRelationship : IEntityChangeListener<GameEntity>
{
    private readonly Dictionary<int, GameEntity> _victimsByEngulferId = [];
    private readonly Dictionary<int, int> _engulferIdsByVictimId = [];

    public EngulferToVictimRelationship(IEntityGroup<GameEntity> affectableEntities)
    {
        affectableEntities.AddListener(this);
    }

    public GameEntity? GetVictim(GameEntity engulfer)
        => _victimsByEngulferId.GetValueOrDefault(engulfer.Id);

    public GameEntity? GetEngulfer(GameEntity victim)
        => _engulferIdsByVictimId.TryGetValue(victim.Id, out var engulferId)
            ? victim.Manager.FindEntity(engulferId)
            : null;

    public void Rebuild(GameEntity victim)
        => RefreshVictim(victim);

    public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
        if (!entityChange.Entity.Manager.IsLoading)
        {
            RefreshVictim(entityChange.Entity);
        }
    }

    public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.RemovedComponent is ActiveEffectsComponent)
        {
            RemoveEngulfer(entityChange.Entity);
            if (_victimsByEngulferId.Remove(entityChange.Entity.Id, out var victim))
            {
                _engulferIdsByVictimId.Remove(victim.Id);
            }
        }
    }

    public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Manager.IsLoading)
        {
            return;
        }

        var changes = entityChange.PropertyChanges;
        for (var i = 0; i < changes.Count; i++)
        {
            var component = changes.GetChangedComponent(i);
            var propertyName = changes.GetChangedPropertyName(i);
            if (component.ComponentId == (int)EntityComponent.ActiveEffects
                && propertyName == nameof(ActiveEffectsComponent.PropertyEffects))
            {
                RefreshVictim(entityChange.Entity);
                return;
            }
        }
    }

    private void RefreshVictim(GameEntity victim)
    {
        int? sourceAbilityId = null;
        if (victim.ActiveEffects is { } activeEffects)
        {
            foreach (var effect in activeEffects.PropertyEffects)
            {
                if (effect is { EffectType: EffectType.Engulf, SourceAbilityEntityId: { } sourceId })
                {
                    sourceAbilityId = sourceId;
                    break;
                }
            }
        }

        var engulferId = sourceAbilityId is { } id
            ? victim.Manager.FindEntity(id)?.Ability?.OwnerId
            : null;
        SetEngulfer(victim, engulferId);
    }

    private void SetEngulfer(GameEntity victim, int? engulferId)
    {
        if (_engulferIdsByVictimId.GetValueOrDefault(victim.Id) == engulferId)
        {
            return;
        }

        RemoveEngulfer(victim);
        if (engulferId == null)
        {
            return;
        }

        if (_victimsByEngulferId.TryGetValue(engulferId.Value, out var existingVictim)
            && existingVictim != victim)
        {
            throw new InvalidOperationException(
                $"Entity {engulferId.Value} cannot engulf both {existingVictim.Id} and {victim.Id}.");
        }

        _victimsByEngulferId[engulferId.Value] = victim;
        _engulferIdsByVictimId[victim.Id] = engulferId.Value;
    }

    private void RemoveEngulfer(GameEntity victim)
    {
        if (_engulferIdsByVictimId.Remove(victim.Id, out var engulferId))
        {
            _victimsByEngulferId.Remove(engulferId);
        }
    }
}
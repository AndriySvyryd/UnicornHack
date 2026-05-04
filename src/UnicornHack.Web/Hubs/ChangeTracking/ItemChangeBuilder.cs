using System.Collections;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ItemChangeBuilder : ChangeBuilder<ItemChange>
{
    public override void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownItemsToLevelCellRelationship.AddDependentsListener(this);
    }

    public override void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownItemsToLevelCellRelationship.RemoveDependentsListener(this);
    }

    protected override void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (Changes.TryGetValue(entity.Id, out var tracked))
        {
            TrackPropertyChanges(entity, tracked, entityChange.PropertyChanges);
            return;
        }

        // The entity isn't tracked yet this turn. Check whether the property change
        // warrants creating a new entry — only SensedType changes need this.
        var knowledge = entity.Knowledge;
        if (knowledge == null || knowledge.KnownEntity.Item == null)
        {
            return;
        }

        for (var i = 0; i < entityChange.PropertyChanges.Count; i++)
        {
            if (entityChange.PropertyChanges.GetChangedComponent(i).ComponentId == (int)EntityComponent.Knowledge
                && entityChange.PropertyChanges.GetChangedPropertyName(i) == nameof(KnowledgeComponent.SensedType))
            {
                tracked = GetOrCreateChange(entity, EntityState.Modified);
                TrackPropertyChanges(entity, tracked, entityChange.PropertyChanges);
                return;
            }
        }
    }

    protected override void TrackPropertyChanges(
        GameEntity entity, ItemChange change, IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);

            if (componentId == (int)EntityComponent.Position)
            {
                switch (propertyName)
                {
                    case nameof(PositionComponent.LevelX):
                        change.LevelX = entity.Position!.LevelX;
                        break;
                    case nameof(PositionComponent.LevelY):
                        change.LevelY = entity.Position!.LevelY;
                        break;
                }
            }
            else if (componentId == (int)EntityComponent.Knowledge)
            {
                if (propertyName == nameof(KnowledgeComponent.SensedType))
                {
                    var newSensedType = changes.GetValue<SenseType>(i, Utils.MessagingECS.ValueType.Current);
                    var oldSensedType = changes.GetValue<SenseType>(i, Utils.MessagingECS.ValueType.Old);
                    if (newSensedType.CanIdentify() != oldSensedType.CanIdentify())
                    {
                        change.IsCurrentlyPerceived = newSensedType.CanIdentify();
                    }
                }
                else if (propertyName == nameof(KnowledgeComponent.IsIdentified))
                {
                    var knowledge = entity.Knowledge!;
                    var item = knowledge.KnownEntity.Item!;
                    var canIdentify = knowledge.IsIdentified;
                    change.Type = canIdentify ? item.Type : ItemType.None;
                    change.BaseName = canIdentify ? item.TemplateName : null;
                    change.Name = canIdentify
                        ? entity.Game.Services.Language.GetString(item, item.GetQuantity(), canIdentify)
                        : null;
                }
            }
        }
    }

    protected override ItemChange ProduceFullSnapshot(GameEntity entity, SerializationContext context)
        => SerializeItem(entity, context);

    protected override ItemChange? ProduceDelta(GameEntity entity, ItemChange tracked, SerializationContext context)
    {
        var itemKnowledge = entity.Knowledge;
        var knownEntity = itemKnowledge?.KnownEntity;
        var item = knownEntity?.Item;
        if (itemKnowledge == null || item == null)
        {
            // Stale reference — treat as removal.
            return new ItemChange { ChangedProperties = new BitArray(1) };
        }

        return HasModifiedBits(tracked.ChangedProperties) ? tracked : null;
    }

    public static ItemChange SerializeItem(GameEntity knowledgeEntity, SerializationContext context)
    {
        var itemKnowledge = knowledgeEntity.Knowledge!;
        var knownEntity = itemKnowledge.KnownEntity;
        var item = knownEntity.Item!;
        var position = knowledgeEntity.Position!;
        var currentlyPerceived = itemKnowledge.SensedType.CanIdentify();

        var change = new ItemChange
        {
            LevelX = position.LevelX,
            LevelY = position.LevelY,
            IsCurrentlyPerceived = currentlyPerceived
        };

        if (itemKnowledge.IsIdentified)
        {
            change.Type = item.Type;
            change.BaseName = item.TemplateName;
            change.Name = context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.IsIdentified);
        }

        change.ChangedProperties = null;

        return change;
    }
}

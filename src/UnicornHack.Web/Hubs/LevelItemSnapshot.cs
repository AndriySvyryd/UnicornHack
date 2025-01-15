using System.Collections;
using Microsoft.EntityFrameworkCore;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Hubs;

public class LevelItemSnapshot
{
    private bool CurrentlyPerceived
    {
        get; set;
    }

    private string? NameSnapshot
    {
        get; set;
    }

    public LevelItemSnapshot CaptureState(GameEntity knowledgeEntity, SerializationContext context)
    {
        var itemKnowledge = knowledgeEntity.Knowledge!;
        var knownEntity = itemKnowledge.KnownEntity;
        var position = knowledgeEntity.Position!;
        var item = knownEntity.Item!;
        var manager = context.Manager;
        CurrentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
        NameSnapshot = itemKnowledge.SensedType.CanIdentify()
            ? context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType)
            : null;

        return this;
    }

    public static List<object?>? Serialize(
        GameEntity knowledgeEntity, EntityState? state, LevelItemSnapshot? snapshot, SerializationContext context)
    {
        List<object?> properties;
        var manager = context.Manager;
        var itemKnowledge = knowledgeEntity.Knowledge;
        var knownEntity = itemKnowledge?.KnownEntity;
        var item = knownEntity?.Item;
        var position = knowledgeEntity.Position;
        switch (state)
        {
            case null:
            case EntityState.Added:
            {
                properties = new List<object?>(7) { null };

                Debug.Assert(itemKnowledge != null, nameof(itemKnowledge));
                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(item != null, nameof(item));

                properties.Add(position.LevelX);
                properties.Add(position.LevelY);
                string? name = null;
                if (itemKnowledge.SensedType.CanIdentify())
                {
                    properties.Add((int)item.Type);
                    properties.Add(item.TemplateName);
                    name = context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType);
                    properties.Add(name);
                    var currentlyPerceived = manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell)
                        .CanIdentify();
                    if (snapshot != null)
                    {
                        snapshot.CurrentlyPerceived = currentlyPerceived;
                    }

                    properties.Add(currentlyPerceived);
                }
                else
                {
                    var setValues = new bool[7];
                    setValues[0] = true;
                    setValues[1] = true;
                    setValues[2] = true;
                    properties[0] = new BitArray(setValues);
                }

                if (snapshot != null)
                {
                    snapshot.NameSnapshot = name;
                }

                return properties;
            }
            case EntityState.Deleted:
                return SerializationContext.DeletedBitArray;
            default:
            {
                var i = 0;
                var setValues = new bool[7];
                setValues[i++] = true;
                properties = [null];

                Debug.Assert(itemKnowledge != null, nameof(itemKnowledge));
                Debug.Assert(knownEntity != null, nameof(knownEntity));
                Debug.Assert(position != null, nameof(position));
                Debug.Assert(item != null, nameof(item));

                var positionEntry = context.DbContext.Entry(position);
                if (positionEntry.State != EntityState.Unchanged)
                {
                    var levelX = positionEntry.Property(nameof(PositionComponent.LevelX));
                    if (levelX.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelX);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }

                    var levelY = positionEntry.Property(nameof(PositionComponent.LevelY));
                    if (levelY.IsModified)
                    {
                        setValues[i++] = true;
                        properties.Add(position.LevelY);
                    }
                    else
                    {
                        setValues[i++] = false;
                    }
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                var knowledgeEntry = context.DbContext.Entry(itemKnowledge);
                var sensedType = knowledgeEntry.Property(nameof(KnowledgeComponent.SensedType));
                if (sensedType.IsModified)
                {
                    var canIdentify = itemKnowledge.SensedType.CanIdentify();
                    setValues[i++] = true;
                    properties.Add(!canIdentify
                        ? (int)ItemType.None
                        : item.Type);

                    setValues[i++] = true;
                    properties.Add(!canIdentify
                        ? null
                        : item.TemplateName);
                }
                else
                {
                    setValues[i++] = false;
                    setValues[i++] = false;
                }

                var newName = itemKnowledge.SensedType.CanIdentify()
                    ? context.Services.Language.GetString(item, item.GetQuantity(), itemKnowledge.SensedType)
                    : null;
                if (snapshot!.NameSnapshot != newName)
                {
                    setValues[i++] = true;
                    properties.Add(newName);
                    snapshot.NameSnapshot = newName;
                }
                else
                {
                    setValues[i++] = false;
                }

                var currentlyPerceived =
                    manager.SensorySystem.SensedByPlayer(knownEntity, position.LevelCell).CanIdentify();
                var currentPerceptionChanged = snapshot.CurrentlyPerceived != currentlyPerceived;
                if (currentPerceptionChanged)
                {
                    setValues[i++] = true;
                    properties.Add(currentlyPerceived);
                    snapshot.CurrentlyPerceived = currentlyPerceived;
                }
                else
                {
                    setValues[i++] = false;
                }

                if (properties.Count == 1)
                {
                    return null;
                }

                Debug.Assert(i == 7);
                properties[0] = new BitArray(setValues);
                return properties;
            }
        }
    }
}

using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class ActorChangeBuilder
    : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
{
    private const uint LevelXDirty = 1u << 1;
    private const uint LevelYDirty = 1u << 2;
    private const uint HeadingDirty = 1u << 3;
    private const uint BaseNameDirty = 1u << 4;
    private const uint NameDirty = 1u << 5;
    private const uint IsCurrentlyPerceivedDirty = 1u << 6;
    private const uint HpDirty = 1u << 7;
    private const uint MaxHpDirty = 1u << 8;
    private const uint EpDirty = 1u << 9;
    private const uint MaxEpDirty = 1u << 10;
    private const uint NextActionTickDirty = 1u << 11;
    private const uint NextActionDirty = 1u << 12;
    private const uint MembershipDirty = 1u << 31;
    private const uint AllSharedDirty = (1u << ActorChange.PropertyCount) - 2;
    private const uint IdentificationDirty = BaseNameDirty | NameDirty | IsCurrentlyPerceivedDirty
                                                | HpDirty | MaxHpDirty | EpDirty | MaxEpDirty
                                                | NextActionTickDirty | NextActionDirty;
    private const uint PerceptionDirty = IsCurrentlyPerceivedDirty | HpDirty | MaxHpDirty | EpDirty
                                             | MaxEpDirty | NextActionTickDirty | NextActionDirty;

    private readonly record struct PendingChange(
        GameEntity Entity,
        EntityState State,
        uint DirtyMask,
        int InitialRoomId,
        int FinalRoomId);

    private readonly record struct SharedProjection(
        int RoomId,
        byte LevelX,
        byte LevelY,
        byte Heading,
        string? BaseName,
        string? Name,
        bool IsCurrentlyPerceived,
        int Hp,
        int MaxHp,
        int Ep,
        int MaxEp,
        int NextActionTick,
        ActionProjection? NextAction);

    private readonly record struct ActionProjection(
        ActorActionType Type,
        string? Name,
        int Target,
        TargetingShape TargetingShape,
        int TargetingShapeSize);

    private readonly Dictionary<int, PendingChange> _pending = [];
    private readonly Dictionary<int, SharedProjection> _sharedProjection = [];
    private readonly Dictionary<int, SharedProjection> _preparedShared = [];

    public bool HasChanges => _pending.Count != 0;

    public void RegisterOnGroups(GameManager manager)
    {
        manager.KnownPositions.AddListener(this);
        manager.KnownActorsToCellRelationship.AddDependentsListener(this);
        manager.RoomActors.AddListener(this);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.KnownPositions.RemoveListener(this);
        manager.KnownActorsToCellRelationship.RemoveDependentsListener(this);
        manager.RoomActors.RemoveListener(this);
    }

    public bool HasChangesForRoom(RoomComponent room, GameServices services)
    {
        var roomId = room.EntityId;
        var canPerceiveIntentions = CanPerceiveIntentions(room);
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || TryPrepareShared(
                    id, pending, roomId, services, room.Entity.Game.Manager, canPerceiveIntentions, out _))
            {
                return true;
            }
        }

        return false;
    }

    public Dictionary<int, ActorChange>? BuildCommonChanges(RoomComponent room, GameServices services)
    {
        var roomId = room.EntityId;
        var canPerceiveIntentions = CanPerceiveIntentions(room);
        var outputCount = 0;
        foreach (var (id, pending) in _pending)
        {
            if (ShouldDelete(id, pending, roomId)
                || TryPrepareShared(
                    id, pending, roomId, services, room.Entity.Game.Manager, canPerceiveIntentions, out _))
            {
                outputCount++;
            }
        }

        if (outputCount == 0)
        {
            return null;
        }

        var output = new Dictionary<int, ActorChange>(outputCount);
        foreach (var (id, pending) in _pending)
        {
            ActorChange? change = null;
            if (ShouldDelete(id, pending, roomId))
            {
                change = new ActorChange { ChangedProperties = 0 };
                _sharedProjection.Remove(id);
            }
            else if (TryPrepareShared(
                         id, pending, roomId, services, room.Entity.Game.Manager, canPerceiveIntentions, out var current))
            {
                change = _sharedProjection.TryGetValue(id, out var previous)
                         && previous.RoomId == roomId
                    ? SerializeSharedDelta(current, pending.DirtyMask)
                    : SerializeSharedFull(current);
                _sharedProjection[id] = current;
            }

            if (change != null)
            {
                output.Add(id, change);
            }
        }

        return output;
    }

    private void InvalidateRoomIntentions(RoomComponent? room)
    {
        if (room == null)
        {
            return;
        }

        foreach (var actor in room.KnownActors.Values)
        {
            if (actor.Knowledge?.KnownEntity.AI != null)
            {
                TrackModified(actor, room.EntityId, NextActionDirty);
            }
        }
    }

    public void Reset()
    {
        _pending.Clear();
        _preparedShared.Clear();
    }

    public void ResetProjection(GameEntity observer)
    {
        _sharedProjection.Clear();
        if (observer.Position?.RoomEntity.Room is not { } room)
        {
            return;
        }

        var services = observer.Game.Services;
        var canPerceiveIntentions = CanPerceiveIntentions(room);
        foreach (var actor in room.KnownActors.Values)
        {
            if (TryGetShared(
                    actor,
                    services,
                    observer.Game.Manager,
                    canPerceiveIntentions,
                    AllSharedDirty | MembershipDirty,
                    null,
                    out var shared))
            {
                _sharedProjection[actor.Id] = shared;
            }
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityAdded(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (IsRelevant(entityChange.Entity))
        {
            TrackAdded(entityChange.Entity, principal.Id);
        }
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityRemoved(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (!IsRelevant(entityChange.Entity)
            && !IsRelevantOnRemove(entityChange))
        {
            return;
        }

        var finalRoomId = HasRoomIdChange(entityChange.PropertyChanges)
            ? entityChange.Entity.Position?.RoomId ?? 0
            : 0;
        TrackRemoved(entityChange.Entity, principal.Id, finalRoomId);
    }

    void IDependentEntityChangeListener<GameEntity>.OnPropertyValuesChanged(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (_pending.ContainsKey(entityChange.Entity.Id)
            || IsRelevant(entityChange.Entity))
        {
            TrackModified(
                entityChange.Entity,
                principal.Id,
                GetKnowledgeDirtyMask(entityChange.Entity, entityChange.PropertyChanges));
        }
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Player != null)
        {
            InvalidateRoomIntentions(entityChange.Entity.Position?.RoomEntity.Room);
        }
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (entity.Player == null && entityChange.RemovedComponent is not PlayerComponent)
        {
            return;
        }

        var roomId = entityChange.RemovedComponent is PositionComponent removedPosition
            ? removedPosition.RoomId
            : entity.Position?.RoomId;
        if (roomId != null)
        {
            InvalidateRoomIntentions(entity.Game.Manager.FindEntity(roomId.Value)?.Room);
        }
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (entity.Being != null)
        {
            TrackPlayerIntentionVisibility(entity, entityChange.PropertyChanges);
            if (entity.Position?.Knowledge is { } knowledge)
            {
                TrackModified(
                    knowledge,
                    knowledge.Position!.RoomId,
                    GetBeingDirtyMask(entity, entityChange.PropertyChanges));
            }

            return;
        }

        if (IsRelevant(entity))
        {
            TrackModified(
                entity,
                entity.Position!.RoomId,
                GetKnowledgeDirtyMask(entity, entityChange.PropertyChanges));
        }
    }

    private void TrackPlayerIntentionVisibility(GameEntity entity, IPropertyValueChanges changes)
    {
        if (entity.Player == null)
        {
            return;
        }

        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);
            if (componentId == (int)EntityComponent.Being
                && propertyName == nameof(BeingComponent.CanPerceiveIntention))
            {
                InvalidateRoomIntentions(entity.Position?.RoomEntity.Room);
            }
            else if (componentId == (int)EntityComponent.Position
                     && propertyName == nameof(PositionComponent.RoomId))
            {
                var oldRoomId = changes.GetValue<int>(i, Utils.MessagingECS.ValueType.Old);
                InvalidateRoomIntentions(entity.Game.Manager.FindEntity(oldRoomId)?.Room);
                InvalidateRoomIntentions(entity.Position?.RoomEntity.Room);
            }
        }
    }

    private bool TryPrepareShared(
        int id,
        PendingChange pending,
        int roomId,
        GameServices services,
        GameManager manager,
        bool canPerceiveIntentions,
        out SharedProjection current)
    {
        if (pending.FinalRoomId != roomId)
        {
            current = default;
            return false;
        }

        if (_preparedShared.TryGetValue(id, out current))
        {
            return true;
        }

        var hasPrevious = _sharedProjection.TryGetValue(id, out var previous)
                          && previous.RoomId == roomId;
        if (!TryGetShared(
                pending.Entity,
                services,
                manager,
                canPerceiveIntentions,
                pending.DirtyMask,
                hasPrevious ? previous : null,
                out current))
        {
            return false;
        }

        _preparedShared[id] = current;
        return true;
    }

    private static bool TryGetShared(
        GameEntity actor,
        GameServices services,
        GameManager manager,
        bool canPerceiveIntentions,
        uint dirtyMask,
        SharedProjection? previous,
        out SharedProjection projection)
    {
        if (actor is not
            {
                Knowledge: { } knowledge,
                Position: { CellX: { } levelX, CellY: { } levelY, Heading: { } heading } position
            }
            || knowledge.KnownEntity.Being is not { } being)
        {
            projection = default;
            return false;
        }

        var refreshAll = previous == null || (dirtyMask & MembershipDirty) != 0;
        var prior = previous.GetValueOrDefault();
        var identified = knowledge.IsIdentified;
        var perceived = identified && knowledge.SensedType.CanIdentify();
        var knownEntity = knowledge.KnownEntity;
        var ai = knownEntity.AI;

        projection = new SharedProjection(
            position.RoomId,
            refreshAll || (dirtyMask & LevelXDirty) != 0 ? levelX : prior.LevelX,
            refreshAll || (dirtyMask & LevelYDirty) != 0 ? levelY : prior.LevelY,
            refreshAll || (dirtyMask & HeadingDirty) != 0 ? (byte)heading : prior.Heading,
            refreshAll || (dirtyMask & BaseNameDirty) != 0
                ? !identified ? null : ai != null
                    ? services.Language.GetTemplateName(being.Races.First().Ability!, abbreviate: false)
                    : "player"
                : prior.BaseName,
            refreshAll || (dirtyMask & NameDirty) != 0
                ? !identified ? null : services.Language.GetActorName(knownEntity, identified)
                : prior.Name,
            refreshAll || (dirtyMask & IsCurrentlyPerceivedDirty) != 0
                ? perceived
                : prior.IsCurrentlyPerceived,
            refreshAll || (dirtyMask & HpDirty) != 0 ? identified ? being.HitPoints : 0 : prior.Hp,
            refreshAll || (dirtyMask & MaxHpDirty) != 0 ? identified ? being.HitPointMaximum : 0 : prior.MaxHp,
            refreshAll || (dirtyMask & EpDirty) != 0 ? identified ? being.EnergyPoints : 0 : prior.Ep,
            refreshAll || (dirtyMask & MaxEpDirty) != 0 ? identified ? being.EnergyPointMaximum : 0 : prior.MaxEp,
            (refreshAll || (dirtyMask & NextActionTickDirty) != 0) && identified && perceived
                ? ai?.NextActionTick ?? knownEntity.Player?.NextActionTick ?? 0
                : refreshAll || (dirtyMask & NextActionTickDirty) != 0 ? 0 : prior.NextActionTick,
            refreshAll || (dirtyMask & NextActionDirty) != 0
                ? identified && perceived && canPerceiveIntentions && ai != null
                    ? ToProjection(SerializeNextAction(ai, manager, services))
                    : null
                : prior.NextAction);
        return true;
    }

    private static ActionProjection? ToProjection(ActorActionChange? action)
        => action == null
            ? null
            : new ActionProjection(
                action.Type, action.Name, action.Target, action.TargetingShape, action.TargetingShapeSize);

    internal static bool CanPerceiveIntentions(RoomComponent room)
    {
        foreach (var actor in room.Actors.Values)
        {
            if (actor.Player != null && actor.Being!.CanPerceiveIntention)
            {
                return true;
            }
        }

        return false;
    }

    private static ActorChange SerializeSharedFull(SharedProjection current)
        => new()
        {
            ChangedProperties = null,
            LevelX = current.LevelX,
            LevelY = current.LevelY,
            Heading = current.Heading,
            BaseName = current.BaseName,
            Name = current.Name,
            IsCurrentlyPerceived = current.IsCurrentlyPerceived,
            Hp = current.Hp,
            MaxHp = current.MaxHp,
            Ep = current.Ep,
            MaxEp = current.MaxEp,
            NextActionTick = current.NextActionTick,
            NextAction = ToChange(current.NextAction)
        };

    private static ActorChange SerializeSharedDelta(SharedProjection current, uint dirtyMask)
    {
        var change = new ActorChange();
        if ((dirtyMask & LevelXDirty) != 0) change.LevelX = current.LevelX;
        if ((dirtyMask & LevelYDirty) != 0) change.LevelY = current.LevelY;
        if ((dirtyMask & HeadingDirty) != 0) change.Heading = current.Heading;
        if ((dirtyMask & BaseNameDirty) != 0) change.BaseName = current.BaseName;
        if ((dirtyMask & NameDirty) != 0) change.Name = current.Name;
        if ((dirtyMask & IsCurrentlyPerceivedDirty) != 0) change.IsCurrentlyPerceived = current.IsCurrentlyPerceived;
        if ((dirtyMask & HpDirty) != 0) change.Hp = current.Hp;
        if ((dirtyMask & MaxHpDirty) != 0) change.MaxHp = current.MaxHp;
        if ((dirtyMask & EpDirty) != 0) change.Ep = current.Ep;
        if ((dirtyMask & MaxEpDirty) != 0) change.MaxEp = current.MaxEp;
        if ((dirtyMask & NextActionTickDirty) != 0) change.NextActionTick = current.NextActionTick;
        if ((dirtyMask & NextActionDirty) != 0) change.NextAction = ToChange(current.NextAction);
        return change;
    }

    private static ActorActionChange? ToChange(ActionProjection? action)
        => action is not { } value
            ? null
            : new ActorActionChange
            {
                Type = value.Type,
                Name = value.Name,
                Target = value.Target,
                TargetingShape = value.TargetingShape,
                TargetingShapeSize = value.TargetingShapeSize
            };

    public static ActorChange SerializeActor(
        GameEntity knowledgeEntity, in SerializationContext context, bool canPerceiveIntentions)
    {
        var actorKnowledge = knowledgeEntity.Knowledge!;
        var knownEntity = actorKnowledge.KnownEntity;
        var ai = knownEntity.AI;
        var position = knowledgeEntity.Position!;
        var being = knownEntity.Being!;

        if (!actorKnowledge.IsIdentified)
        {
            return new ActorChange
            {
                ChangedProperties = null,
                LevelX = position.CellX!.Value,
                LevelY = position.CellY!.Value,
                Heading = (byte)position.Heading!
            };
        }

        var currentlyPerceived = actorKnowledge.SensedType.CanIdentify();
        var change = new ActorChange
        {
            ChangedProperties = null,
            LevelX = position.CellX!.Value,
            LevelY = position.CellY!.Value,
            Heading = (byte)position.Heading!,
            BaseName = ai != null
                ? context.Services.Language.GetTemplateName(
                    being.Races.First().Ability!, abbreviate: false)
                : "player",
            Name = context.Services.Language.GetActorName(knownEntity, actorKnowledge.IsIdentified),
            IsCurrentlyPerceived = currentlyPerceived,
            Hp = being.HitPoints,
            MaxHp = being.HitPointMaximum,
            Ep = being.EnergyPoints,
            MaxEp = being.EnergyPointMaximum
        };

        if (ai == null)
        {
            change.NextActionTick = currentlyPerceived ? knownEntity.Player?.NextActionTick ?? 0 : 0;
            return change;
        }

        change.NextActionTick = currentlyPerceived ? ai.NextActionTick ?? 0 : 0;
        change.NextAction = currentlyPerceived && canPerceiveIntentions
            ? SerializeNextAction(ai, context.Manager, context.Services)
            : null;
        return change;
    }

    private static ActorActionChange? SerializeNextAction(
        AIComponent ai, GameManager manager, GameServices services)
    {
        if (ai.NextAction == null)
        {
            return null;
        }

        switch (ai.NextAction)
        {
            case ActorActionType.UseAbilitySlot:
                var slot = ai.NextActionTarget;
                if (!slot.HasValue)
                {
                    return null;
                }

                var abilityEntity = manager.AbilitySlottingSystem.GetAbility(ai.Entity, slot.Value);
                if (abilityEntity == null)
                {
                    return null;
                }

                var ability = abilityEntity.Ability!;
                return new ActorActionChange
                {
                    Type = ai.NextAction.Value,
                    Name = services.Language.GetString(ability),
                    Target = ai.NextActionTarget2 ?? 0,
                    TargetingShape = ability.TargetingShape,
                    TargetingShapeSize = ability.TargetingShapeSize
                };
            case ActorActionType.MoveOneCell:
            case ActorActionType.ChangeHeading:
                var direction = (Direction)ai.NextActionTarget!;
                var actionLabel = ai.NextAction == ActorActionType.MoveOneCell ? "Move " : "Turn ";
                return new ActorActionChange
                {
                    Type = ai.NextAction.Value,
                    Name = actionLabel + services.Language.GetString(direction, abbreviate: true),
                    Target = ai.Entity.Position!.Cell!.Value.Translate(direction.AsVector()).ToInt32(),
                    TargetingShape = TargetingShape.Line,
                    TargetingShapeSize = 0
                };
            default:
                return new ActorActionChange
                {
                    Type = ai.NextAction ?? ActorActionType.Wait,
                    Name = ai.NextAction?.ToString(),
                    Target = ai.NextActionTarget ?? 0,
                    TargetingShape = TargetingShape.Line,
                    TargetingShapeSize = 0
                };
        }
    }

    private void TrackAdded(GameEntity entity, int roomId)
    {
        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            var wasKnownInRoom = _sharedProjection.TryGetValue(entity.Id, out var previous)
                                 && previous.RoomId == roomId
                                 && pending.InitialRoomId == roomId;
            var dirtyMask = wasKnownInRoom
                ? pending.DirtyMask & ~MembershipDirty
                : pending.DirtyMask | MembershipDirty;
            if (dirtyMask == 0)
            {
                _pending.Remove(entity.Id);
                return;
            }

            _pending[entity.Id] = pending with
            {
                Entity = entity,
                State = wasKnownInRoom ? EntityState.Modified : EntityState.Added,
                DirtyMask = dirtyMask,
                FinalRoomId = roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _sharedProjection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            MembershipDirty | AllSharedDirty,
            InitialRoomId: 0,
            FinalRoomId: roomId);
    }

    private void TrackRemoved(GameEntity entity, int initialRoomId, int finalRoomId)
    {
        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            if (pending.State == EntityState.Added
                && !_sharedProjection.ContainsKey(entity.Id)
                && finalRoomId == 0)
            {
                _pending.Remove(entity.Id);
                return;
            }

            _pending[entity.Id] = pending with
            {
                Entity = entity,
                State = EntityState.Deleted,
                DirtyMask = pending.DirtyMask | MembershipDirty,
                FinalRoomId = finalRoomId
            };
            return;
        }

        if (_sharedProjection.ContainsKey(entity.Id))
        {
            _pending[entity.Id] = new PendingChange(
                entity, EntityState.Deleted, MembershipDirty, initialRoomId, finalRoomId);
        }
    }

    private void TrackModified(GameEntity entity, int roomId, uint dirtyMask)
    {
        if (dirtyMask == 0)
        {
            return;
        }

        if (_pending.TryGetValue(entity.Id, out var pending))
        {
            _pending[entity.Id] = pending with
            {
                Entity = entity,
                DirtyMask = pending.DirtyMask | dirtyMask,
                FinalRoomId = pending.State == EntityState.Deleted ? pending.FinalRoomId : roomId
            };
            return;
        }

        _pending[entity.Id] = new PendingChange(
            entity,
            _sharedProjection.ContainsKey(entity.Id) ? EntityState.Modified : EntityState.Added,
            dirtyMask,
            roomId,
            roomId);
    }

    private bool ShouldDelete(int id, PendingChange pending, int roomId)
        => pending.InitialRoomId == roomId
            && pending.FinalRoomId != roomId
            && _sharedProjection.TryGetValue(id, out var previous)
            && previous.RoomId == roomId;

    private static uint GetKnowledgeDirtyMask(GameEntity entity, IPropertyValueChanges changes)
    {
        var dirtyMask = 0u;
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);
            if (componentId == (int)EntityComponent.Position
                && propertyName == nameof(PositionComponent.CellX))
            {
                dirtyMask |= LevelXDirty;
                dirtyMask |= NextActionDirty;
            }
            else if (componentId == (int)EntityComponent.Position
                     && propertyName == nameof(PositionComponent.CellY))
            {
                dirtyMask |= LevelYDirty;
                dirtyMask |= NextActionDirty;
            }
            else if (componentId == (int)EntityComponent.Position
                     && propertyName == nameof(PositionComponent.Heading))
            {
                dirtyMask |= HeadingDirty;
            }
            else if (componentId == (int)EntityComponent.Knowledge)
            {
                if (propertyName == nameof(KnowledgeComponent.SensedType))
                {
                    dirtyMask |= PerceptionDirty;
                }
                else if (propertyName is nameof(KnowledgeComponent.IsIdentified)
                         or nameof(KnowledgeComponent.KnownEntityId))
                {
                    dirtyMask |= IdentificationDirty;
                }
            }
        }

        return dirtyMask;
    }

    private static uint GetBeingDirtyMask(GameEntity entity, IPropertyValueChanges changes)
    {
        var dirtyMask = 0u;
        var knowledge = entity.Position?.Knowledge?.Knowledge;
        var isIdentified = knowledge is { IsIdentified: true };
        var canIdentify = knowledge is
            {
                IsIdentified: true,
                SensedType: var sensedType
            }
            && sensedType.CanIdentify();
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);
            if (componentId == (int)EntityComponent.Being)
            {
                if (propertyName is nameof(BeingComponent.HitPoints)
                    or nameof(BeingComponent.HitPointMaximum)
                    or nameof(BeingComponent.EnergyPoints)
                    or nameof(BeingComponent.EnergyPointMaximum)
                    && isIdentified)
                {
                    dirtyMask |= propertyName switch
                    {
                        nameof(BeingComponent.HitPoints) => HpDirty,
                        nameof(BeingComponent.HitPointMaximum) => MaxHpDirty,
                        nameof(BeingComponent.EnergyPoints) => EpDirty,
                        nameof(BeingComponent.EnergyPointMaximum) => MaxEpDirty,
                        _ => 0
                    };
                }

            }
            else if (componentId == (int)EntityComponent.AI)
            {
                if (canIdentify && propertyName == nameof(AIComponent.NextActionTick))
                {
                    dirtyMask |= NextActionTickDirty;
                }
                else if (canIdentify
                         && propertyName is nameof(AIComponent.NextAction)
                             or nameof(AIComponent.NextActionTarget)
                             or nameof(AIComponent.NextActionTarget2))
                {
                    dirtyMask |= NextActionDirty;
                }
            }
            else if (componentId == (int)EntityComponent.Player
                     && canIdentify
                     && propertyName == nameof(PlayerComponent.NextActionTick))
            {
                dirtyMask |= NextActionTickDirty;
            }
            else if (componentId == (int)EntityComponent.Position)
            {
                if (canIdentify)
                {
                    dirtyMask |= NextActionDirty;
                }
            }
        }

        return dirtyMask;
    }

    private static bool IsRelevant(GameEntity entity)
        => entity.Knowledge?.KnownEntity.Being != null;

    private static bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange)
        => entityChange.RemovedComponent is KnowledgeComponent knowledge
            && knowledge.KnownEntity.Being != null;

    private static bool HasRoomIdChange(IPropertyValueChanges changes)
    {
        for (var i = 0; i < changes.Count; i++)
        {
            if (changes.GetChangedComponent(i).ComponentId == (int)EntityComponent.Position
                && changes.GetChangedPropertyName(i) == nameof(PositionComponent.RoomId))
            {
                return true;
            }
        }

        return false;
    }
}
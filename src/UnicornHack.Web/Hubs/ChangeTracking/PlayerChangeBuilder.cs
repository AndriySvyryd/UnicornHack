using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class PlayerChangeBuilder : IEntityChangeListener<GameEntity>
{
    private readonly int _playerEntityId;
    private readonly AbilityChangeBuilder _abilityBuilder;
    private readonly EffectChangeBuilder _effectBuilder;
    private readonly LogChangeBuilder _logBuilder = new();

    private PlayerChange _pendingChange = new();

    public RoomChangeBuilder RoomChangeBuilder
    {
        get;
    }

    public int PlayerEntityId => _playerEntityId;

    public int LastSentTick
    {
        get; set;
    }

    /// <summary>
    ///     Id of the level entity the client last received in a full snapshot.
    ///     Used to detect cross-level transitions so a full <see cref="RoomChange" /> snapshot
    ///     is emitted instead of a delta (which carries only entity/terrain changes, not
    ///     <see cref="RoomChange.Width" /> / <see cref="RoomChange.Height" /> /
    ///     <see cref="RoomChange.BranchName" /> / <see cref="RoomChange.Depth" />).
    /// </summary>
    private int LastSentRoomId
    {
        get; set;
    }

    public PlayerChangeBuilder(GameEntity playerEntity, RoomChangeBuilder roomChangeBuilder)
    {
        _playerEntityId = playerEntity.Id;
        LastSentTick = playerEntity.Game.CurrentTick;
        LastSentRoomId = playerEntity.Position?.RoomId ?? 0;
        RoomChangeBuilder = roomChangeBuilder;

        _abilityBuilder = new AbilityChangeBuilder(playerEntity.Id);
        _abilityBuilder.ResetAccumulatorPlaceholders(playerEntity);
        _effectBuilder = new EffectChangeBuilder(playerEntity.Id);
        _logBuilder.Reset(playerEntity);
    }

    public bool HasObservableChanges(GameManager manager)
    {
        var playerEntity = manager.FindEntity(_playerEntityId);
        var position = playerEntity?.Position;
        // The room the player occupies is tracked so the client keeps the actor list up to date.
        var currentRoom = position?.RoomEntity.Room;
        if (RoomChangeBuilder.HasChanges(currentRoom)
            || HasScalarChanges() || _abilityBuilder.HasChanges
            || _abilityBuilder.HasAccumulatorChanges
            || _effectBuilder.HasChanges)
        {
            return true;
        }

        if (playerEntity != null && _logBuilder.HasChanges(playerEntity))
        {
            return true;
        }

        // A move to a different room is a change even when nothing else is; no position maps to room 0.
        var currentRoomId = position?.RoomId ?? 0;
        return currentRoomId != LastSentRoomId;
    }

    public PlayerChange BuildChangeSet(GameManager manager, GameServices services)
    {
        var playerEntity = manager.FindEntity(_playerEntityId);
        if (playerEntity == null)
        {
            Debug.Assert(false, $"Player entity with id {_playerEntityId} not found");

            return new PlayerChange
            {
                PreviousTick = LastSentTick,
                CurrentTick = manager.Game.CurrentTick
            };
        }

        var change = new PlayerChange
        {
            PreviousTick = LastSentTick,
            CurrentTick = manager.Game.CurrentTick
        };
        LastSentTick = manager.Game.CurrentTick;

        var position = playerEntity.Position;
        var currentRoomEntity = position?.RoomEntity;
        var currentRoomId = currentRoomEntity?.Id ?? 0;
        if (currentRoomEntity == null)
        {
            // The player has no position at all (e.g. being torn down); there is no room to send.
            LastSentRoomId = 0;
        }
        else if (currentRoomId != LastSentRoomId)
        {
            // Player moved to a different level. Emit a full snapshot.
            change.Room = RoomChangeBuilder.SerializeRoom(
                currentRoomEntity, new SerializationContext(null!, playerEntity, services));
            RoomChangeBuilder.ResetActorBaseline(playerEntity);
            LastSentRoomId = currentRoomId;
        }
        else
        {
            var levelChangeSet = RoomChangeBuilder.GetSerializedRoom(
                currentRoomEntity, playerEntity, services);
            if (levelChangeSet != null)
            {
                change.Room = levelChangeSet;
            }
        }

        Dictionary<int, AbilityChange>? serializedAbilities = null;
        if (_abilityBuilder.HasChanges || _abilityBuilder.HasAccumulatorChanges)
        {
            serializedAbilities = new Dictionary<int, AbilityChange>();
            if (_abilityBuilder.HasChanges)
            {
                _abilityBuilder.WriteTo(serializedAbilities, new SerializationContext(null!, playerEntity, services));
            }

            _abilityBuilder.WriteAccumulatorPlaceholdersTo(serializedAbilities, playerEntity);
            change.Abilities = serializedAbilities;
        }

        if (_effectBuilder.HasChanges)
        {
            var effectChanges = new Dictionary<int, EffectChange>();
            _effectBuilder.WriteTo(effectChanges);
            change.Effects = effectChanges;
        }

        var serializedLog = _logBuilder.GetSerializedLogEntries(playerEntity);
        if (serializedLog != null)
        {
            change.Log = serializedLog;
        }

        var pendingBits = _pendingChange.ChangedProperties!;
        if (pendingBits[5])
        {
            change.NextActionTick = _pendingChange.NextActionTick;
        }

        if (pendingBits[10])
        {
            change.XP = _pendingChange.XP;
        }

        if (pendingBits[11])
        {
            change.Hp = _pendingChange.Hp;
        }

        if (pendingBits[12])
        {
            change.MaxHp = _pendingChange.MaxHp;
        }

        if (pendingBits[13])
        {
            change.Ep = _pendingChange.Ep;
        }

        if (pendingBits[14])
        {
            change.MaxEp = _pendingChange.MaxEp;
        }

        if (pendingBits[15])
        {
            change.ReservedEp = _pendingChange.ReservedEp;
        }

        if (pendingBits[16])
        {
            change.Fortune = _pendingChange.Fortune;
        }

        return change;
    }

    public void Reset(GameEntity playerEntity)
    {
        _pendingChange = new PlayerChange();
        _abilityBuilder.Reset();
        _abilityBuilder.ResetAccumulatorPlaceholders(playerEntity);
        _effectBuilder.Reset();

        LastSentRoomId = playerEntity.Position?.RoomId ?? 0;
        _logBuilder.Reset(playerEntity);
    }

    public static PlayerChange SerializePlayer(
        GameEntity playerEntity, SerializationContext context)
    {
        var player = playerEntity.Player!;
        var being = playerEntity.Being!;

        var position = playerEntity.Position;
        var onMap = position is { Cell: not null };
        var serializedLevel = position == null
            ? null
            : RoomChangeBuilder.SerializeRoom(position.RoomEntity, context);

        var abilities = new Dictionary<int, AbilityChange>();
        foreach (var ability in being.Abilities
                     .Select(a => a.Ability!)
                     .Where(a => !a.IsLeveledSupplement
                                 && AbilityChangeBuilder.IsClientVisible(a))
                     .Select(a => a.Entity))
        {
            abilities.Add(ability.Id, AbilityChangeBuilder.SerializeAbility(ability, context.Services));
        }

        foreach (var (id, placeholder) in AbilityChangeBuilder.GetAccumulatorPlaceholders(playerEntity))
        {
            abilities.Add(id, placeholder);
        }

        var logEntries = new Dictionary<int, LogEntryChange>();
        foreach (var logEntry in player.LogEntries
                     .OrderBy(e => e, LogEntry.Comparer)
                     .Skip(Math.Max(0, player.LogEntries.Count - 10)))
        {
            logEntries.Add(logEntry.Id, LogChangeBuilder.SerializeLogEntry(logEntry));
        }

        return new PlayerChange
        {
            ChangedProperties = null,
            Name = player.ProperName,
            Id = onMap ? position!.Knowledge?.Id ?? -1 : -1,
            PreviousTick = 0,
            CurrentTick = player.Game.CurrentTick,
            Room = serializedLevel,
            Abilities = abilities,
            Effects = EffectChangeBuilder.SerializeSnapshot(playerEntity),
            Log = logEntries,
            NextActionTick = player.NextActionTick ?? 0,
            XP = being.ExperiencePoints,
            Hp = being.HitPoints,
            MaxHp = being.HitPointMaximum,
            Ep = being.EnergyPoints,
            MaxEp = being.EnergyPointMaximum,
            ReservedEp = being.ReservedEnergyPoints,
            Fortune = being.EntropyState
        };
    }

    public void RegisterOnGroups(GameManager manager)
    {
        manager.RoomActors.AddListener(this);
        _abilityBuilder.RegisterOnGroups(manager);
        _effectBuilder.RegisterOnGroups(manager);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.RoomActors.RemoveListener(this);
        _abilityBuilder.UnregisterFromGroups(manager);
        _effectBuilder.UnregisterFromGroups(manager);
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        => HandleScalarPropertyChanges(entityChange);

    private bool HasScalarChanges()
    {
        var bits = _pendingChange.ChangedProperties!;
        for (var i = 5; i < PlayerChange.PropertyCount; i++)
        {
            if (bits[i])
            {
                return true;
            }
        }

        return false;
    }

    private bool HandleScalarPropertyChanges(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Id != _playerEntityId)
        {
            return false;
        }

        var changes = entityChange.PropertyChanges;
        var handled = false;
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);

            if (componentId == (int)EntityComponent.Player)
            {
                if (propertyName == nameof(PlayerComponent.NextActionTick))
                {
                    _pendingChange.NextActionTick = entityChange.Entity.Player!.NextActionTick ?? 0;
                    handled = true;
                }
            }
            else if (componentId == (int)EntityComponent.Being)
            {
                switch (propertyName)
                {
                    case nameof(BeingComponent.ExperiencePoints):
                        _pendingChange.XP = entityChange.Entity.Being!.ExperiencePoints;
                        handled = true;
                        break;
                    case nameof(BeingComponent.HitPoints):
                        _pendingChange.Hp = entityChange.Entity.Being!.HitPoints;
                        handled = true;
                        break;
                    case nameof(BeingComponent.HitPointMaximum):
                        _pendingChange.MaxHp = entityChange.Entity.Being!.HitPointMaximum;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EnergyPoints):
                        _pendingChange.Ep = entityChange.Entity.Being!.EnergyPoints;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EnergyPointMaximum):
                        _pendingChange.MaxEp = entityChange.Entity.Being!.EnergyPointMaximum;
                        handled = true;
                        break;
                    case nameof(BeingComponent.ReservedEnergyPoints):
                        _pendingChange.ReservedEp = entityChange.Entity.Being!.ReservedEnergyPoints;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EntropyState):
                        _pendingChange.Fortune = entityChange.Entity.Being!.EntropyState;
                        handled = true;
                        break;
                    case nameof(BeingComponent.CanPerceiveIntention):
                        RoomChangeBuilder.TrackObserverTelegraphVisibility(entityChange.Entity);
                        handled = true;
                        break;
                }
            }
        }

        return handled;
    }
}

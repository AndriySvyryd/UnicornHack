using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class PlayerChangeBuilder : IEntityChangeListener<GameEntity>
{
    private readonly int _playerEntityId;
    private readonly RaceChangeBuilder _raceBuilder;
    private readonly AbilityChangeBuilder _abilityBuilder;
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

        _raceBuilder = new RaceChangeBuilder(playerEntity.Id);
        _abilityBuilder = new AbilityChangeBuilder(playerEntity.Id);
        _logBuilder.Initialize(playerEntity);
    }

    public bool HasObservableChanges(GameManager manager)
    {
        if (HasScalarChanges() || _raceBuilder.HasChanges || _abilityBuilder.HasChanges)
        {
            return true;
        }

        var playerEntity = manager.FindEntity(_playerEntityId);
        if (playerEntity != null && _logBuilder.HasChanges(playerEntity))
        {
            return true;
        }

        var currentRoomId = playerEntity?.Position?.RoomId ?? 0;
        if (currentRoomId != LastSentRoomId)
        {
            return true;
        }

        return RoomChangeBuilder.HasChanges(playerEntity?.Position?.RoomEntity.Room);
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

        var currentRoomEntity = playerEntity.Position?.RoomEntity;
        var currentRoomId = currentRoomEntity?.Id ?? 0;
        if (currentRoomId != LastSentRoomId && currentRoomEntity != null)
        {
            // Player moved to a different level. Emit a full snapshot.
            change.Room = RoomChangeBuilder.SerializeRoom(
                currentRoomEntity, new SerializationContext(null!, playerEntity, services));
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

        if (_raceBuilder.HasChanges)
        {
            var serializedRaces = new Dictionary<int, RaceChange>();
            _raceBuilder.WriteTo(serializedRaces, new SerializationContext(null!, playerEntity, services));
            change.Races = serializedRaces;
        }

        if (_abilityBuilder.HasChanges)
        {
            var serializedAbilities = new Dictionary<int, AbilityChange>();
            _abilityBuilder.WriteTo(serializedAbilities, new SerializationContext(null!, playerEntity, services));
            change.Abilities = serializedAbilities;
        }

        var serializedLog = _logBuilder.GetSerializedLogEntries(playerEntity);
        if (serializedLog != null)
        {
            change.Log = serializedLog;
        }

        var pendingBits = _pendingChange.ChangedProperties!;
        if (pendingBits[9])
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

    public void Clear()
    {
        _raceBuilder.Clear();
        _abilityBuilder.Clear();
        _pendingChange = new PlayerChange();
    }

    public static PlayerChange SerializePlayer(
        GameEntity playerEntity, SerializationContext context)
    {
        var player = playerEntity.Player!;
        var being = playerEntity.Being!;

        var serializedLevel = RoomChangeBuilder.SerializeRoom(
            playerEntity.Position!.RoomEntity, context);

        var races = new Dictionary<int, RaceChange>();
        foreach (var race in being.Races)
        {
            var change = RaceChangeBuilder.SerializeRace(race, context.Services);
            change.ChangedProperties = null;
            races.Add(race.Id, change);
        }

        var abilities = new Dictionary<int, AbilityChange>();
        foreach (var ability in being.Abilities
                     .Select(a => a.Ability!)
                     .Where(a => a.Slot != null)
                     .Select(a => a.Entity))
        {
            abilities.Add(ability.Id, AbilityChangeBuilder.SerializeAbility(ability, context.Services));
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
            Id = playerEntity.Position.Knowledge!.Id,
            PreviousTick = 0,
            CurrentTick = player.Game.CurrentTick,
            Room = serializedLevel,
            Races = races,
            Abilities = abilities,
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
        _raceBuilder.RegisterOnGroups(manager);
        _abilityBuilder.RegisterOnGroups(manager);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.RoomActors.RemoveListener(this);
        _raceBuilder.UnregisterFromGroups(manager);
        _abilityBuilder.UnregisterFromGroups(manager);
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        HandleScalarPropertyChanges(entityChange);
    }

    private bool HasScalarChanges()
    {
        var bits = _pendingChange.ChangedProperties!;
        for (var i = 9; i < PlayerChange.PropertyCount; i++)
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
                }
            }
        }

        return handled;
    }
}

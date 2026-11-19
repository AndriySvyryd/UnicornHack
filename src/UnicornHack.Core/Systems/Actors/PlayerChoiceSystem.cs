using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Features;

namespace UnicornHack.Systems.Actors;

public class PlayerChoiceSystem
{
    public const string AddAction = "Add";
    public const string ReplaceAction = "Replace";
    public const string RemoveAction = "Remove";
    public const string DestroyAction = "Destroy";
    public const int DefaultOptionCount = 3;

    public static byte GetPlayerUseMask(GameEntity playerEntity)
    {
        var index = 0;
        foreach (var player in playerEntity.Manager.Players)
        {
            if (player.Id < playerEntity.Id)
            {
                index++;
            }
        }

        return checked((byte)(1 << index));
    }

    public static bool HasUsedFeature(FeatureComponent feature, GameEntity playerEntity)
        => (feature.UsedByPlayers & GetPlayerUseMask(playerEntity)) != 0;

    private bool TryPresentRaceChoice(GameEntity playerEntity, FeatureComponent feature)
    {
        var player = playerEntity.Player!;
        var ownedRaces = GetPlayerRaces(playerEntity);
        var canAdd = GetAvailableRaces(playerEntity, feature, ownedRaces).Count != 0;
        var actions = new List<string>();
        if (canAdd)
        {
            actions.Add(AddAction);
            if (ownedRaces.Count != 0)
            {
                actions.Add(RemoveAction);
            }
        }
        if (feature.Destroyed == false)
        {
            actions.Add(DestroyAction);
        }
        if (actions.Count == 0)
        {
            return false;
        }

        player.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.Race,
            SourceFeatureId = feature.EntityId,
            PendingChoiceOptions = actions
        };
        return true;
    }

    public PlayerOptionSelectionResult Select(
        GameEntity playerEntity,
        int optionIndex,
        GameManager manager)
    {
        var player = playerEntity.Player
            ?? throw new InvalidOperationException($"Entity {playerEntity.Id} is not a player.");
        var language = manager.Game.Services.Language;
        if (player.PendingChoice is not { } choice)
        {
            return PlayerOptionSelectionResult.Reject(language.InvalidPlayerChoiceSelection());
        }

        FeatureComponent? source = null;
        if (choice.SourceFeatureId != null && !TryGetRewardSource(playerEntity, choice, out source))
        {
            return PlayerOptionSelectionResult.Reject(language.PlayerChoiceApplicationFailed());
        }

        if (source == null || choice.ChoiceType != RewardType.Race)
        {
            return SelectReward(playerEntity, choice, source, optionIndex, manager);
        }

        if (choice.PendingChoiceAction == null)
        {
            return SelectRaceMenu(playerEntity, choice, source, optionIndex, manager);
        }

        if (optionIndex < 0
            || optionIndex > choice.PendingChoiceOptions.Count)
        {
            return PlayerOptionSelectionResult.Reject(language.InvalidPlayerChoiceSelection());
        }

        if (optionIndex == choice.PendingChoiceOptions.Count)
        {
            return ResolveDestroyed(playerEntity, source, manager);
        }

        return choice.PendingChoiceAction switch
        {
            AddAction => CompleteReward(playerEntity, source,
                choice.PendingChoiceOptions[optionIndex], destroyed: false, manager),
            ReplaceAction => PresentRemoveOptions(playerEntity, choice.PendingChoiceOptions[optionIndex], source),
            RemoveAction => CompleteReward(playerEntity, source, choice.SelectedOption!, destroyed: false, manager,
                choice.PendingChoiceOptions[optionIndex]),
            _ => throw new InvalidOperationException($"Unknown race choice action '{choice.PendingChoiceAction}'.")
        };
    }

    public PlayerChoicePresentation? GetPresentation(GameEntity playerEntity)
    {
        if (playerEntity.Player?.PendingChoice is not { } choice)
        {
            return null;
        }

        FeatureComponent? feature = null;
        if (choice.SourceFeatureId != null && !TryGetRewardSource(playerEntity, choice, out feature))
        {
            return null;
        }

        if (feature == null || choice.ChoiceType != RewardType.Race)
        {
            return GetRewardPresentation(playerEntity, choice, feature);
        }

        var language = playerEntity.Game.Services.Language;
        var options = new List<PlayerChoiceOptionPresentation>();
        if (choice.PendingChoiceAction == null)
        {
            var canAdd = choice.PendingChoiceOptions.Contains(AddAction);
            foreach (var action in new[] { AddAction, RemoveAction })
            {
                options.Add(new PlayerChoiceOptionPresentation(action,
                    language.GetPlayerChoiceOptionTitle(RewardType.Race, action),
                    language.GetPlayerChoiceOptionDescription(RewardType.Race, action),
                    choice.PendingChoiceOptions.Contains(action) ? null
                        : action == RemoveAction && canAdd ? language.NoRaceToReplace() : language.NoAvailableRaces()));
            }
        }
        else
        {
            foreach (var raceName in choice.PendingChoiceOptions)
            {
                options.Add(new PlayerChoiceOptionPresentation(raceName,
                    language.GetPlayerChoiceOptionTitle(RewardType.Race, choice.PendingChoiceAction, raceName),
                    language.GetPlayerChoiceOptionDescription(RewardType.Race, choice.PendingChoiceAction, raceName)));
            }
        }
        if (feature.Destroyed == false)
        {
            options.Add(new PlayerChoiceOptionPresentation(DestroyAction,
                language.GetPlayerChoiceOptionTitle(RewardType.Race, DestroyAction, feature.Name),
                language.GetPlayerChoiceOptionDescription(RewardType.Race, DestroyAction, feature.Name)));
        }

        return new PlayerChoicePresentation(RewardType.Race, choice.PendingChoiceAction,
            language.GetPlayerChoiceTitle(RewardType.Race, choice.PendingChoiceAction, feature.Name), options,
            language.GetEffectDurationDescription(EffectDuration.Infinite));
    }

    public bool TryPresentReward(GameEntity playerEntity, RoomComponent room, Point cell)
    {
        room.Features.TryGetValue(cell, out var featureEntity);
        var feature = featureEntity?.Feature;
        if (playerEntity.Player is not { PendingChoice: null } player
            || playerEntity.Position?.RoomId != room.EntityId
            || playerEntity.Position.Cell is not { } actorCell || actorCell.DistanceTo(cell) != 1
            || feature == null || HasUsedFeature(feature, playerEntity))
        {
            return false;
        }

        if (feature.Destroyed == true)
        {
            return false;
        }

        if (feature.Template is not { RewardType: not RewardType.None } definition)
        {
            return false;
        }

        if (definition.RewardType == RewardType.Race)
        {
            return TryPresentRaceChoice(playerEntity, feature);
        }

        var options = GetRewardOptions(feature)
            .Where(option => CanApplyReward(playerEntity, definition.RewardType, option.Name)).ToList();
        if (options.Count == 0 && feature.Destroyed != false)
        {
            return false;
        }

        var optionCount = Math.Min(options.Count, DefaultOptionCount);
        var selectedOptions = options.Count == optionCount ? options
            : feature.RewardRandom.WeightedOrder(options,
            option => option.Weight).Take(optionCount).ToList();

        player.PendingChoice = new PlayerOptions
        {
            ChoiceType = definition.RewardType,
            SourceFeatureId = feature.EntityId,
            PendingChoiceOptions = selectedOptions.Select(option => option.Name).ToList()
        };
        return true;
    }

    public virtual IEnumerable<(string Name, float Weight)> GetRewardOptions(Feature definition, RoomComponent room)
    {
        var data = room.Game.Services.Data;
        var pool = definition.RewardType switch
        {
            RewardType.Race => data.PlayerRaces.GetAll().Select(race => race.Name),
            RewardType.Ritual => data.Abilities.GetAllValues(AbilityType.Boon)
                .Concat(data.Abilities.GetAllValues(AbilityType.Curse)).Select(ability => ability.Name),
            RewardType.Item => data.Items.GetAll()
                .Where(item => definition.ItemRewardType == ItemType.None || (item.Type!.Value & definition.ItemRewardType) != 0)
                .Select(item => item.Name),
            RewardType.Effect when definition.AbilityRewardType == AbilityType.Default => data.Abilities.GetAllValues(AbilityType.Default)
                .Concat(data.Abilities.GetAllValues(AbilityType.Buff)).Select(ability => ability.Name),
            _ => data.Abilities.GetAllValues(definition.AbilityRewardType).Select(ability => ability.Name)
        };

        return pool.Select(name => (Name: name, Weight: GetRewardWeight(definition, room, name)))
            .Where(option => option.Weight > 0);
    }

    public virtual float GetRewardWeight(Feature definition, RoomComponent room, string name)
        => definition.RewardType switch
        {
            RewardType.Item => room.Game.Services.Data.Items.Get(name).GetWeight(room, definition.Name),
            RewardType.Race => room.Game.Services.Data.PlayerRaces.Get(name).GetWeight(room, definition.Name),
            RewardType.Ritual or RewardType.Ability or RewardType.Effect => room.Game.Services.Data.Abilities.Get(name)
                .GetWeight(room, definition.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(definition.RewardType))
        };

    private IEnumerable<(string Name, float Weight)> GetRewardOptions(FeatureComponent feature)
    {
        var room = feature.Entity.Position!.RoomEntity.Room!;
        return GetRewardOptions(feature.Template!, room);
    }

    private static bool CanApplyReward(GameEntity playerEntity, RewardType type, string name, bool destroy = false)
        => type switch
        {
            RewardType.Race => destroy || !AbilityLevelSystem.GetPlayerRaceAbilityNames(playerEntity)
                .Contains(playerEntity.Game.Services.Data.PlayerRaces.Get(name).GetRaceAbilityName()),
            RewardType.Item => CanReceiveItem(playerEntity, name),
            RewardType.Ritual or RewardType.Ability => CanGrantRewardAbility(playerEntity,
                playerEntity.Game.Services.Data.Abilities.Get(name), EffectDuration.Infinite),
            RewardType.Effect => CanApplyRewardAbility(playerEntity, name),
            _ => false
        };

    private static bool CanGrantRewardAbility(GameEntity playerEntity, Ability template, EffectDuration duration)
    {
        var level = AbilityLevelSystem.GetAbilityLevel(playerEntity, template.Name);
        if (level >= template.MaxLevel)
        {
            return false;
        }

        if (template.Type != AbilityType.Boon || level == 0)
        {
            return true;
        }

        return duration == EffectDuration.Infinite
            && !playerEntity.ActiveEffects!.ComponentEffects.GetValues(template.Name).Any(record =>
                record.EffectType == EffectType.AddAbility && record.Level > 0
                && record.ExpirationEvent is not (null or EffectDuration.Infinite));
    }

    private static bool CanReceiveItem(GameEntity playerEntity, string name)
    {
        var manager = playerEntity.Manager;
        using var reference = manager.Game.Services.Data.Items.Get(name).Instantiate(manager);
        var message = MoveItemMessage.Create(manager);
        message.ItemEntity = reference.Referenced;
        message.TargetContainerEntity = playerEntity;
        var canReceive = manager.ItemMovingSystem.CanMoveItem(message, manager);
        if (!canReceive)
        {
            message.TargetContainerEntity = null;
            message.TargetRoomEntity = playerEntity.Position!.RoomEntity;
            message.TargetCell = playerEntity.Position.Cell;
            canReceive = manager.ItemMovingSystem.CanMoveItem(message, manager);
        }

        manager.ReturnMessage(message);
        return canReceive;
    }

    private static bool CanApplyRewardAbility(GameEntity playerEntity, string name)
    {
        foreach (var effect in playerEntity.Game.Services.Data.Abilities.Get(name).Effects ?? [])
        {
            if (effect is DecreaseAbilityLevel { AbilityName: null, AbilityType: { } type }
                && AbilityLevelSystem.GetRemovableAbilities(playerEntity, type).Count == 0)
            {
                return false;
            }

            if (effect is AddAbility added && !CanGrantRewardAbility(playerEntity,
                added.Ability ?? playerEntity.Game.Services.Data.Abilities.Get(added.Name),
                added.Duration ?? EffectDuration.Infinite))
            {
                return false;
            }
        }

        return true;
    }

    private PlayerChoicePresentation GetRewardPresentation(
        GameEntity playerEntity, PlayerOptions choice, FeatureComponent? feature)
    {
        var language = playerEntity.Game.Services.Language;
        var options = choice.PendingChoiceOptions.Select(name => new PlayerChoiceOptionPresentation(
            name, language.GetPlayerChoiceOptionTitle(choice.ChoiceType, choice.PendingChoiceAction, name),
            language.GetPlayerChoiceOptionDescription(choice.ChoiceType, choice.PendingChoiceAction, name))).ToList();
        if (feature?.Destroyed == false)
        {
            options.Add(new PlayerChoiceOptionPresentation(DestroyAction,
                language.GetPlayerChoiceOptionTitle(choice.ChoiceType, DestroyAction, feature.Name),
                language.GetPlayerChoiceOptionDescription(choice.ChoiceType, DestroyAction, feature.Name)));
        }

        return new PlayerChoicePresentation(choice.ChoiceType, choice.PendingChoiceAction,
            language.GetPlayerChoiceTitle(choice.ChoiceType, choice.PendingChoiceAction, featureName: feature?.Template?.Name),
            options, Notice: null);
    }

    private PlayerOptionSelectionResult SelectReward(
        GameEntity playerEntity, PlayerOptions choice, FeatureComponent? source, int optionIndex, GameManager manager)
    {
        if (source != null && optionIndex == choice.PendingChoiceOptions.Count)
        {
            return ResolveDestroyed(playerEntity, source, manager);
        }

        if (optionIndex < 0 || optionIndex >= choice.PendingChoiceOptions.Count)
        {
            return PlayerOptionSelectionResult.Reject(manager.Game.Services.Language.InvalidPlayerChoiceSelection());
        }

        var name = choice.PendingChoiceOptions[optionIndex];
        if (source == null && !CanApplyReward(playerEntity, choice.ChoiceType, name))
        {
            return PlayerOptionSelectionResult.Reject(manager.Game.Services.Language.PlayerChoiceApplicationFailed());
        }
        if (source != null)
        {
            return CompleteReward(playerEntity, source, name, destroyed: false, manager);
        }
        ApplyReward(playerEntity, choice.ChoiceType, playerEntity, name, manager, raceToRemove: null);
        playerEntity.Player!.PendingChoice = null;
        if (choice.ChoiceType != RewardType.Race)
        {
            manager.LoggingSystem.WriteLog(manager.Game.Services.Language.RewardGranted(name), playerEntity, manager);
        }
        return PlayerOptionSelectionResult.Complete();
    }

    private static bool TryGetRewardSource(GameEntity playerEntity, PlayerOptions choice, [NotNullWhen(true)] out FeatureComponent? source)
    {
        source = null;
        var entity = playerEntity.Manager.FindEntity(choice.SourceFeatureId);
        if (entity?.Position is not { Cell: { } cell } position
            || playerEntity.Position is not { Cell: { } playerCell } playerPosition
            || position.RoomId != playerPosition.RoomId || playerCell.DistanceTo(cell) != 1
            || entity.Feature is not { Destroyed: not true } feature
            || feature.Template?.RewardType != choice.ChoiceType
            || HasUsedFeature(feature, playerEntity))
        {
            return false;
        }

        source = feature;
        return true;
    }

    private static PlayerOptionSelectionResult CompleteReward(
        GameEntity playerEntity, FeatureComponent feature, string name, bool destroyed, GameManager manager,
        string? raceToRemove = null)
    {
        var type = feature.Template!.RewardType;
        ApplyReward(playerEntity, type, feature.Entity, name, manager, raceToRemove);
        CompleteFeatureUse(playerEntity, feature);
        if (destroyed || type != RewardType.Race)
        {
            var language = manager.Game.Services.Language;
            manager.LoggingSystem.WriteLog(destroyed
                ? language.FeatureDestroyed(feature.Name, name, type)
                : language.RewardGranted(name), playerEntity, manager);
        }
        return PlayerOptionSelectionResult.Complete();
    }

    private static void CompleteFeatureUse(GameEntity playerEntity, FeatureComponent feature)
    {
        feature.UsedByPlayers |= GetPlayerUseMask(playerEntity);
        if (playerEntity.Player!.PendingChoice?.SourceFeatureId == feature.EntityId)
        {
            playerEntity.Player.PendingChoice = null;
        }
    }

    private static void ApplyReward(
        GameEntity playerEntity, RewardType type, GameEntity source, string name, GameManager manager, string? raceToRemove)
    {
        switch (type)
        {
            case RewardType.Ritual:
            case RewardType.Ability:
                var boon = manager.Game.Services.Data.Abilities.Get(name);
                manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, boon);
                break;
            case RewardType.Effect:
                source.ActiveEffects ??= manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
                var ability = manager.AffectableAbilitiesIndex[(source.Id, name)]?.Ability
                    ?? manager.Game.Services.Data.Abilities.Get(name).AddToAffectable(source);
                var activation = ActivateAbilityMessage.Create(manager);
                activation.AbilityEntity = ability.Entity;
                activation.ActivatorEntity = source;
                activation.TargetEntity = playerEntity;
                manager.Process(activation);
                break;
            case RewardType.Item:
                manager.Game.Services.Data.Items.Get(name).Instantiate(playerEntity);
                break;
            case RewardType.Race:
                ApplyRace(playerEntity, manager.Game.Services.Data.PlayerRaces.Get(name), manager, raceToRemove);
                break;
            default:
                throw new InvalidOperationException($"Unknown reward type {type}.");
        }
    }

    private PlayerOptionSelectionResult ResolveDestroyed(
        GameEntity playerEntity, FeatureComponent feature, GameManager manager)
    {
        if (feature.Destroyed != false)
        {
            return PlayerOptionSelectionResult.Reject(manager.Game.Services.Language.PlayerChoiceApplicationFailed());
        }
        var pool = GetRewardOptions(feature).ToList();
        var options = GetDestroyOptions(playerEntity, feature, pool);
        var name = feature.RewardRandom.WeightedOrder(options,
            option => option.Weight).Select(option => option.Name).FirstOrDefault();

        var results = new List<(GameEntity Player, string? Name)> { (playerEntity, name) };
        foreach (var player in manager.Players)
        {
            if (player == playerEntity || HasUsedFeature(feature, player))
            {
                continue;
            }

            var playerOptions = GetDestroyOptions(player, feature, pool);
            var playerResult = feature.RewardRandom.WeightedOrder(playerOptions,
                option => option.Weight).Select(option => option.Name).FirstOrDefault();
            results.Add((player, playerResult));
        }

        feature.Destroyed = true;
        foreach (var (player, result) in results)
        {
            if (result != null)
            {
                CompleteReward(player, feature, result, destroyed: true, manager);
            }
            else
            {
                CompleteFeatureUse(player, feature);
                manager.LoggingSystem.WriteLog(manager.Game.Services.Language.FeatureDestroyed(
                    feature.Name, rewardName: null, feature.Template!.RewardType),
                    player, manager);
            }
        }

        return PlayerOptionSelectionResult.Complete();
    }

    private static List<(string Name, float Weight)> GetDestroyOptions(
        GameEntity playerEntity, FeatureComponent feature, IReadOnlyList<(string Name, float Weight)> pool)
    {
        // TODO: disregard preconditions for choices when presenting destroy options
        var choice = playerEntity.Player?.PendingChoice;
        var displayed = choice?.SourceFeatureId == feature.EntityId ? choice.PendingChoiceOptions : [];
        var excluded = displayed.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return pool.Where(option => !excluded.Contains(option.Name)
            && CanApplyReward(playerEntity, feature.Template!.RewardType, option.Name, destroy: true)).ToList();
    }

    public void Clear(GameEntity playerEntity)
    {
        if (playerEntity.Player != null)
        {
            playerEntity.Player.PendingChoice = null;
        }
    }

    private PlayerOptionSelectionResult SelectRaceMenu(
        GameEntity playerEntity,
        PlayerOptions choice,
        FeatureComponent source,
        int optionIndex,
        GameManager manager)
    {
        var language = manager.Game.Services.Language;
        var action = optionIndex switch
        {
            0 => AddAction,
            1 => RemoveAction,
            2 => DestroyAction,
            _ => null
        };
        if (action == null || !choice.PendingChoiceOptions.Contains(action))
        {
            return PlayerOptionSelectionResult.Reject(language.InvalidPlayerChoiceSelection());
        }

        return action == DestroyAction
            ? ResolveDestroyed(playerEntity, source, manager)
            : PresentAddOptions(playerEntity, source, replace: action == RemoveAction);
    }

    private static void ApplyRace(
        GameEntity playerEntity,
        PlayerRace race,
        GameManager manager,
        string? raceToRemove)
    {
        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, race.GetRaceAbilityTemplate());
        if (raceToRemove != null && !string.Equals(raceToRemove, race.Name, StringComparison.OrdinalIgnoreCase))
        {
            manager.AbilityLevelSystem.RemoveAbility(playerEntity,
                manager.Game.Services.Data.PlayerRaces.Get(raceToRemove).GetRaceAbilityName(), manager,
                sourceAbilityEntityId: null);
        }
    }

    private static PlayerOptionSelectionResult PresentRemoveOptions(
        GameEntity playerEntity, string raceName, FeatureComponent source)
    {
        var newRaceAbility = playerEntity.Game.Services.Data.PlayerRaces.Get(raceName).GetRaceAbilityName();
        playerEntity.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.Race,
            SourceFeatureId = source.EntityId,
            PendingChoiceAction = RemoveAction,
            SelectedOption = raceName,
            PendingChoiceOptions = GetPlayerRaces(playerEntity)
                .Where(race => !string.Equals(race.GetRaceAbilityName(), newRaceAbility, StringComparison.OrdinalIgnoreCase))
                .OrderBy(race => race.Name, StringComparer.OrdinalIgnoreCase)
                .Select(race => race.Name).ToList()
        };
        return PlayerOptionSelectionResult.Complete();
    }

    private PlayerOptionSelectionResult PresentAddOptions(
        GameEntity playerEntity, FeatureComponent feature, bool replace)
    {
        var availableRaces = GetAvailableRaces(playerEntity, feature);
        if (availableRaces.Count == 0)
        {
            return PlayerOptionSelectionResult.Reject(playerEntity.Game.Services.Language.NoAvailableRaces());
        }

        var options = feature.RewardRandom.WeightedOrder(
                availableRaces, option => option.Weight)
            .Take(DefaultOptionCount).Select(option => option.Race.Name).ToList();

        playerEntity.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.Race,
            PendingChoiceAction = replace ? ReplaceAction : AddAction,
            SourceFeatureId = feature.EntityId,
            PendingChoiceOptions = options
        };
        return PlayerOptionSelectionResult.Complete();
    }

    private List<(PlayerRace Race, float Weight)> GetAvailableRaces(
        GameEntity playerEntity, FeatureComponent source, IReadOnlyCollection<PlayerRace>? ownedRaces = null)
    {
        ownedRaces ??= GetPlayerRaces(playerEntity);
        var ownedRaceAbilityNames = ownedRaces
            .Select(race => race.GetRaceAbilityName())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return GetRewardOptions(source)
            .Select(option => (Race: playerEntity.Game.Services.Data.PlayerRaces.Get(option.Name), option.Weight))
            .Where(option => !ownedRaceAbilityNames.Contains(option.Race.GetRaceAbilityName()))
            .ToList();
    }

    private static List<PlayerRace> GetPlayerRaces(GameEntity playerEntity)
    {
        var result = new List<PlayerRace>();
        var playerRaces = playerEntity.Game.Services.Data.PlayerRaces;
        foreach (var abilityName in AbilityLevelSystem.GetPlayerRaceAbilityNames(playerEntity))
        {
            if (playerRaces.FindByIndex(abilityName) is { } race)
            {
                result.Add(race);
            }
        }

        return result;
    }
}

using UnicornHack.Systems.Abilities;
using UnicornHack.Services;
using UnicornHack.Systems.Features;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public class PlayerChoiceSystem
{
    public const string AddAction = "Add";
    public const string RemoveAction = "Remove";
    public const string DestroyAction = "Destroy";
    public const int DefaultOptionCount = 3;

    public bool TryPresent(GameEntity playerEntity, RewardType choiceType)
    {
        var player = playerEntity.Player;
        if (player == null
            || player.PendingChoice != null
            || choiceType != RewardType.RacePedestal)
        {
            return false;
        }

        var ownedRaces = GetPlayerRaces(playerEntity);
        var canAdd = GetAvailableRaces(playerEntity, ownedRaces).Count != 0;
        player.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.RacePedestal,
            PendingChoiceOptions = !canAdd
                ? [DestroyAction]
                : ownedRaces.Count == 0
                    ? [AddAction, DestroyAction]
                    : [AddAction, RemoveAction, DestroyAction]
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
        if (player.PendingChoice is { ChoiceType: RewardType.Boon or RewardType.Item or RewardType.Race } rewardChoice)
        {
            return SelectReward(playerEntity, rewardChoice, optionIndex, manager);
        }

        if (player.PendingChoice is not { ChoiceType: RewardType.RacePedestal } choice)
        {
            return PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.NoPendingChoice));
        }

        var position = playerEntity.Position!;
        var room = position.RoomEntity.Room!;
        var pedestalCell = position.Cell!.Value.Translate(position.Heading!.Value.AsVector());
        if (choice.PendingChoiceAction == null)
        {
            return SelectRaceMenu(
                playerEntity, optionIndex, room, pedestalCell, manager);
        }

        if (optionIndex < 0
            || optionIndex > choice.PendingChoiceOptions.Length)
        {
            return PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.StaleSelection));
        }

        if (optionIndex == choice.PendingChoiceOptions.Length)
        {
            return ApplyRandomRaceResult(
                playerEntity,
                choice.PendingChoiceOptions,
                room,
                pedestalCell,
                manager);
        }

        return choice.PendingChoiceAction switch
        {
            AddAction => GrantRace(
                playerEntity,
                manager.Game.Services.Data.PlayerRaces.Get(choice.PendingChoiceOptions[optionIndex]),
                room,
                pedestalCell,
                manager),
            RemoveAction => RemoveRace(
                playerEntity, choice.PendingChoiceOptions[optionIndex], manager),
            _ => PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.UnknownAction))
        };
    }

    public PlayerChoicePresentation? GetPresentation(GameEntity playerEntity)
    {
        if (playerEntity.Player?.PendingChoice is { ChoiceType: RewardType.Boon or RewardType.Item or RewardType.Race } rewardChoice)
        {
            return GetRewardPresentation(playerEntity, rewardChoice);
        }

        if (playerEntity.Player?.PendingChoice is not { ChoiceType: RewardType.RacePedestal } choice)
        {
            return null;
        }

        if (choice.PendingChoiceAction == null)
        {
            var canAdd = choice.PendingChoiceOptions.Contains(AddAction);
            var canReplace = choice.PendingChoiceOptions.Contains(RemoveAction);
            var language = playerEntity.Game.Services.Language;
            return new PlayerChoicePresentation(
                RewardType.RacePedestal,
                Action: null,
                language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.Title),
                [
                    new PlayerChoiceOptionPresentation(
                        AddAction,
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionTitle, AddAction),
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionDescription, AddAction),
                        canAdd
                            ? null
                            : language.GetPlayerChoiceText(
                                RewardType.RacePedestal, PlayerChoiceText.NoAvailableOptions)),
                    new PlayerChoiceOptionPresentation(
                        RemoveAction,
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionTitle, RemoveAction),
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionDescription, RemoveAction),
                        canReplace
                            ? null
                            : language.GetPlayerChoiceText(
                                RewardType.RacePedestal,
                                canAdd
                                    ? PlayerChoiceText.NoOptionToReplace
                                    : PlayerChoiceText.NoAvailableOptions)),
                    new PlayerChoiceOptionPresentation(
                        DestroyAction,
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionTitle, DestroyAction),
                        language.GetPlayerChoiceText(
                            RewardType.RacePedestal, PlayerChoiceText.OptionDescription, DestroyAction))
                ],
                Notice: language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.Notice));
        }

        var choiceLanguage = playerEntity.Game.Services.Language;
        return new PlayerChoicePresentation(
            RewardType.RacePedestal,
            choice.PendingChoiceAction,
            choiceLanguage.GetPlayerChoiceText(
                RewardType.RacePedestal, PlayerChoiceText.Title, choice.PendingChoiceAction),
            choice.PendingChoiceOptions.Select(raceName => new PlayerChoiceOptionPresentation(
                    raceName,
                    choiceLanguage.GetPlayerChoiceText(
                        RewardType.RacePedestal,
                        PlayerChoiceText.OptionTitle,
                        choice.PendingChoiceAction,
                        raceName),
                    choiceLanguage.GetPlayerChoiceText(
                        RewardType.RacePedestal,
                        PlayerChoiceText.OptionDescription,
                        choice.PendingChoiceAction,
                        raceName)))
                .Append(new PlayerChoiceOptionPresentation(
                    DestroyAction,
                    choiceLanguage.GetPlayerChoiceText(
                        RewardType.RacePedestal, PlayerChoiceText.OptionTitle, DestroyAction),
                    choiceLanguage.GetPlayerChoiceText(
                        RewardType.RacePedestal, PlayerChoiceText.OptionDescription, DestroyAction)))
                .ToList(),
            Notice: choiceLanguage.GetPlayerChoiceText(
                RewardType.RacePedestal, PlayerChoiceText.Notice));
    }

    public static Reward CreateRewardFeature(
        RoomComponent room, Point cell, string title, string[] names, MapFeature feature = MapFeature.SmallFountain,
        RewardType rewardType = RewardType.Boon)
    {
        if (!room.IsValid(cell) || !room.Terrain[room.PointToIndex![cell.X, cell.Y]].CanMoveTo()
            || room.Actors.ContainsKey(cell) || room.Items.ContainsKey(cell) || room.Connections.ContainsKey(cell)
            || room.Features.ContainsKey(cell)
            || names.Length == 0)
        {
            throw new InvalidOperationException($"Invalid reward placement in room {room.EntityId} at {cell}.");
        }

        var data = room.Game.Services.Data;
        foreach (var name in names)
        {
            var valid = rewardType switch
            {
                RewardType.Boon => data.Abilities.Find(name)?.Type == AbilityType.Boon,
                RewardType.Item => data.Items.Find(name) != null,
                RewardType.Race => data.PlayerRaces.Find(name) != null,
                _ => throw new ArgumentOutOfRangeException(nameof(rewardType))
            };
            if (!valid)
            {
                throw new InvalidOperationException($"Reward '{name}' is not a valid {rewardType}.");
            }
        }

        var manager = room.Entity.Manager;
        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        entity.Feature = manager.CreateComponent<FeatureComponent>(EntityComponent.Feature);
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = room.Entity;
        position.Cell = cell;
        entity.Position = position;
        var reward = new Reward
        {
            Id = entity.Id,
            FeatureId = entity.Id,
            Title = title,
            Type = rewardType,
            Names = (string[])names.Clone(),
            ActiveFeature = feature
        };
        entity.Feature.Rewards.Add(reward);
        TerrainSystem.SetTerrain(feature, cell, room);
        return reward;
    }

    public bool TryPresentReward(GameEntity playerEntity, RoomComponent room, Point cell)
    {
        room.Features.TryGetValue(cell, out var featureEntity);
        var reward = featureEntity?.Feature?.Rewards.FirstOrDefault(candidate => candidate.ClaimantEntityId == null);
        if (playerEntity.Player is not { PendingChoice: null } player
            || playerEntity.Position?.RoomId != room.EntityId
            || playerEntity.Position.Cell is not { } actorCell || actorCell.DistanceTo(cell) != 1
            || reward == null
            || room.Terrain[room.PointToIndex![cell.X, cell.Y]] != reward.ActiveFeature)
        {
            return false;
        }

        var ownedRaces = reward.Type == RewardType.Race
            ? AbilityLevelSystem.GetPlayerRaceAbilityNames(playerEntity)
            : null;
        var options = reward.Names.Where(name => reward.Type switch
        {
            RewardType.Race => !ownedRaces!.Contains(playerEntity.Game.Services.Data.PlayerRaces.Get(name).GetRaceAbilityName()),
            RewardType.Item => CanReceiveItem(playerEntity, name),
            _ => true
        }).ToArray();
        if (options.Length == 0)
        {
            return false;
        }

        player.PendingChoice = new PlayerOptions
        {
            ChoiceType = reward.Type,
            SourceFeatureId = reward.FeatureId,
            PendingChoiceOptions = options
        };
        return true;
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

    private static PlayerChoicePresentation GetRewardPresentation(GameEntity playerEntity, PlayerOptions choice)
    {
        var feature = playerEntity.Manager.FindEntity(choice.SourceFeatureId)!.Feature!;
        var reward = feature.Rewards.First(candidate => candidate.Type == choice.ChoiceType);
        var category = choice.ChoiceType switch
        {
            RewardType.Item => DescriptionCategory.Item,
            RewardType.Race => DescriptionCategory.Race,
            _ => DescriptionCategory.Ability
        };
        var options = choice.PendingChoiceOptions.Select(name => new PlayerChoiceOptionPresentation(
            name, name,
            playerEntity.Game.Services.Language.GetDescription(name, category))).ToList();
        return new PlayerChoicePresentation(choice.ChoiceType, choice.PendingChoiceAction,
            reward.Title, options,
            choice.ChoiceType == RewardType.Boon ? "Lasts until entering another branch." : null);
    }

    private static PlayerOptionSelectionResult SelectReward(
        GameEntity playerEntity, PlayerOptions choice, int optionIndex, GameManager manager)
    {
        if (optionIndex < 0 || optionIndex >= choice.PendingChoiceOptions.Length)
        {
            return PlayerOptionSelectionResult.Reject("That reward choice is no longer available.");
        }

        var name = choice.PendingChoiceOptions[optionIndex];
        if (choice.PendingChoiceAction != "Confirm")
        {
            playerEntity.Player!.PendingChoice = new PlayerOptions
            {
                ChoiceType = choice.ChoiceType,
                PendingChoiceAction = "Confirm",
                SourceFeatureId = choice.SourceFeatureId,
                PendingChoiceOptions = [name]
            };
            return PlayerOptionSelectionResult.Complete();
        }

        var featureEntity = manager.FindEntity(choice.SourceFeatureId)!;
        var reward = featureEntity.Feature!.Rewards.First(candidate => candidate.Type == choice.ChoiceType);
        switch (choice.ChoiceType)
        {
            case RewardType.Boon:
                var boon = manager.Game.Services.Data.Abilities.Get(name);
                var granted = manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, boon);
                granted.ExpirationEvent = EffectDuration.UntilNextBranch;
                break;
            case RewardType.Item:
                manager.Game.Services.Data.Items.Get(name).Instantiate(playerEntity);
                break;
            case RewardType.Race:
                GrantRace(playerEntity, manager.Game.Services.Data.PlayerRaces.Get(name), room: null, pedestalCell: null, manager);
                break;
            default:
                throw new InvalidOperationException($"Unknown reward type {reward.Type}.");
        }

        reward.ClaimantEntityId = playerEntity.Id;
        foreach (var option in featureEntity.Feature!.Rewards.ToArray())
        {
            featureEntity.Feature.Rewards.Remove(option);
        }

        var featurePosition = featureEntity.Position!;
        TerrainSystem.DeactivateFeature(reward.ActiveFeature, featurePosition.Cell!.Value, featurePosition.RoomEntity.Room!);
        playerEntity.Player!.PendingChoice = null;
        manager.LoggingSystem.WriteLog($"You gain {name}.", playerEntity, manager);
        return PlayerOptionSelectionResult.Complete(playerEntity.Position!.MovementDelay);
    }

    public void Clear(GameEntity playerEntity)
    {
        if (playerEntity.Player != null)
        {
            playerEntity.Player.PendingChoice = null;
        }
    }

    private static PlayerOptionSelectionResult SelectRaceMenu(
        GameEntity playerEntity,
        int optionIndex,
        RoomComponent room,
        Point pedestalCell,
        GameManager manager)
    {
        var player = playerEntity.Player!;
        var language = manager.Game.Services.Language;
        var action = optionIndex switch
        {
            0 => AddAction,
            1 => RemoveAction,
            2 => DestroyAction,
            _ => null
        };
        if (action == null || !player.PendingChoice!.PendingChoiceOptions.Contains(action))
        {
            return PlayerOptionSelectionResult.Reject(language.InvalidPlayerChoiceSelection());
        }

        switch (optionIndex)
        {
            case 0:
                return PresentAddOptions(playerEntity, manager);
            case 1:
                player.PendingChoice = new PlayerOptions
                {
                    ChoiceType = RewardType.RacePedestal,
                    PendingChoiceAction = RemoveAction,
                    PendingChoiceOptions = GetPlayerRaces(playerEntity).OrderBy(race => race.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(race => race.Name)
                        .ToArray()
                };
                return PlayerOptionSelectionResult.Complete();
            case 2:
                return ApplyRandomRaceResult(
                    playerEntity, [], room, pedestalCell, manager);
            default:
                return PlayerOptionSelectionResult.Reject(
                    language.GetPlayerChoiceText(
                        RewardType.RacePedestal, PlayerChoiceText.UnknownAction));
        }
    }

    private static PlayerOptionSelectionResult GrantRace(
        GameEntity playerEntity,
        PlayerRace race,
        RoomComponent? room,
        Point? pedestalCell,
        GameManager manager)
    {
        manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, race.GetRaceAbilityTemplate());
        if (room != null
            && pedestalCell.HasValue)
        {
            TerrainSystem.DeactivateFeature(
                MapFeature.RacePedestal, pedestalCell.Value, room);
        }
        playerEntity.Player!.PendingChoice = null;
        return PlayerOptionSelectionResult.Complete();
    }

    private static PlayerOptionSelectionResult RemoveRace(
        GameEntity playerEntity,
        string raceName,
        GameManager manager)
    {
        var race = manager.Game.Services.Data.PlayerRaces.Get(raceName);
        manager.AbilityLevelSystem.RemoveAbility(
            playerEntity, race.GetRaceAbilityName(), manager,
            sourceAbilityEntityId: null);
        return PresentAddOptions(playerEntity, manager);
    }

    private static PlayerOptionSelectionResult PresentAddOptions(
        GameEntity playerEntity, GameManager manager)
    {
        var availableRaces = GetAvailableRaces(playerEntity);
        if (availableRaces.Count == 0)
        {
            return PlayerOptionSelectionResult.Reject(
                manager.Game.Services.Language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.NoAvailableOptions));
        }

        var selectedRaces = manager.Game.Random.WeightedOrder(
                availableRaces, _ => 1f)
            .Take(DefaultOptionCount);

        playerEntity.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = RewardType.RacePedestal,
            PendingChoiceAction = AddAction,
            PendingChoiceOptions = selectedRaces
                .Select(race => race.Name)
                .ToArray()
        };
        return PlayerOptionSelectionResult.Complete();
    }

    private static PlayerOptionSelectionResult ApplyRandomRaceResult(
        GameEntity playerEntity,
        IReadOnlyCollection<string> presentedOptions,
        RoomComponent? room,
        Point? pedestalCell,
        GameManager manager)
    {
        var excluded = presentedOptions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        // TODO: This will select a race/origin even if the conditions don't normally allow it.
        // e.g. it could select another origin for the same base race
        var race = manager.Game.Random.WeightedOrder(
                playerEntity.Game.Services.Data.PlayerRaces.GetAsList(),
                candidate => excluded.Contains(candidate.Name) ? 0f : 1f)
            .FirstOrDefault();
        if (race == null)
        {
            return PlayerOptionSelectionResult.Reject(
                manager.Game.Services.Language.GetPlayerChoiceText(
                    RewardType.RacePedestal, PlayerChoiceText.NoAvailableOptions));
        }

        return GrantRace(playerEntity, race, room, pedestalCell, manager);
    }

    private static List<PlayerRace> GetAvailableRaces(
        GameEntity playerEntity, IReadOnlyCollection<PlayerRace>? ownedRaces = null)
    {
        ownedRaces ??= GetPlayerRaces(playerEntity);
        var ownedRaceAbilityNames = ownedRaces
            .Select(race => race.GetRaceAbilityName())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return playerEntity.Game.Services.Data.PlayerRaces.GetAll()
            .Where(race => !ownedRaceAbilityNames.Contains(race.GetRaceAbilityName()))
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

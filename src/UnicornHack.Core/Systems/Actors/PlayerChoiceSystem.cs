using UnicornHack.Services;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public class PlayerChoiceSystem
{
    public const string RaceChoice = "Race";
    public const string AddAction = "Add";
    public const string RemoveAction = "Remove";
    public const string DestroyAction = "Destroy";
    public const int DefaultOptionCount = 3;

    public bool TryPresent(GameEntity playerEntity, string choice)
    {
        var player = playerEntity.Player;
        if (player == null
            || player.PendingChoice != null
            || choice != RaceChoice)
        {
            return false;
        }

        player.PendingChoice = new PlayerOptions
        {
            ChoiceType = RaceChoice
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
        if (player.PendingChoice is not { ChoiceType: RaceChoice } choice)
        {
            return PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.NoPendingChoice));
        }

        var hasPedestal = TerrainSystem.TryFindAdjacentFeature(
            playerEntity, MapFeature.RacePedestal, out var room, out var pedestalCell);
        if (!hasPedestal
            && choice.PendingChoiceAction != AddAction)
        {
            Clear(playerEntity);
            return PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.SourceUnavailable));
        }

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
                    RaceChoice, PlayerChoiceText.StaleSelection));
        }

        if (optionIndex == choice.PendingChoiceOptions.Length)
        {
            return ApplyRandomRaceResult(
                playerEntity,
                choice.PendingChoiceOptions,
                hasPedestal ? room : null,
                hasPedestal ? pedestalCell : null,
                manager);
        }

        return choice.PendingChoiceAction switch
        {
            AddAction => AddRace(
                playerEntity,
                choice.PendingChoiceOptions[optionIndex],
                hasPedestal ? room : null,
                hasPedestal ? pedestalCell : null,
                manager),
            RemoveAction => RemoveRace(
                playerEntity, choice.PendingChoiceOptions[optionIndex], manager),
            _ => PlayerOptionSelectionResult.Reject(
                language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.UnknownAction))
        };
    }

    public PlayerChoicePresentation? GetPresentation(GameEntity playerEntity)
    {
        if (playerEntity.Player?.PendingChoice is not { ChoiceType: RaceChoice } choice)
        {
            return null;
        }

        if (choice.PendingChoiceAction == null)
        {
            var ownedRaces = GetPlayerRaces(playerEntity);
            var availableRaces = GetAvailableRaces(playerEntity, ownedRaces);
            var canAdd = availableRaces.Count != 0;
            var canReplace = canAdd && ownedRaces.Count != 0;
            var language = playerEntity.Game.Services.Language;
            return new PlayerChoicePresentation(
                RaceChoice,
                Action: null,
                language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.Title),
                [
                    new PlayerChoiceOptionPresentation(
                        AddAction,
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionTitle, AddAction),
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionDescription, AddAction),
                        canAdd
                            ? null
                            : language.GetPlayerChoiceText(
                                RaceChoice, PlayerChoiceText.NoAvailableOptions)),
                    new PlayerChoiceOptionPresentation(
                        RemoveAction,
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionTitle, RemoveAction),
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionDescription, RemoveAction),
                        canReplace
                            ? null
                            : language.GetPlayerChoiceText(
                                RaceChoice,
                                canAdd
                                    ? PlayerChoiceText.NoOptionToReplace
                                    : PlayerChoiceText.NoAvailableOptions)),
                    new PlayerChoiceOptionPresentation(
                        DestroyAction,
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionTitle, DestroyAction),
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.OptionDescription, DestroyAction))
                ],
                Notice: language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.Notice));
        }

        var choiceLanguage = playerEntity.Game.Services.Language;
        return new PlayerChoicePresentation(
            RaceChoice,
            choice.PendingChoiceAction,
            choiceLanguage.GetPlayerChoiceText(
                RaceChoice, PlayerChoiceText.Title, choice.PendingChoiceAction),
            choice.PendingChoiceOptions.Select(raceName => new PlayerChoiceOptionPresentation(
                    raceName,
                    choiceLanguage.GetPlayerChoiceText(
                        RaceChoice,
                        PlayerChoiceText.OptionTitle,
                        choice.PendingChoiceAction,
                        raceName),
                    choiceLanguage.GetPlayerChoiceText(
                        RaceChoice,
                        PlayerChoiceText.OptionDescription,
                        choice.PendingChoiceAction,
                        raceName)))
                .Append(new PlayerChoiceOptionPresentation(
                    DestroyAction,
                    choiceLanguage.GetPlayerChoiceText(
                        RaceChoice, PlayerChoiceText.OptionTitle, DestroyAction),
                    choiceLanguage.GetPlayerChoiceText(
                        RaceChoice, PlayerChoiceText.OptionDescription, DestroyAction)))
                .ToList(),
            Notice: choiceLanguage.GetPlayerChoiceText(
                RaceChoice, PlayerChoiceText.Notice));
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
        var ownedRaces = GetPlayerRaces(playerEntity);
        var language = manager.Game.Services.Language;
        var availableRaces = GetAvailableRaces(playerEntity, ownedRaces);
        switch (optionIndex)
        {
            case 0:
                if (availableRaces.Count == 0)
                {
                    return PlayerOptionSelectionResult.Reject(
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.NoAvailableOptions));
                }

                return PresentAddOptions(playerEntity, manager);
            case 1:
                if (availableRaces.Count == 0)
                {
                    return PlayerOptionSelectionResult.Reject(
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.NoAvailableOptions));
                }
                if (ownedRaces.Count == 0)
                {
                    return PlayerOptionSelectionResult.Reject(
                        language.GetPlayerChoiceText(
                            RaceChoice, PlayerChoiceText.NoOptionToReplace));
                }

                player.PendingChoice = new PlayerOptions
                {
                    ChoiceType = RaceChoice,
                    PendingChoiceAction = RemoveAction,
                    PendingChoiceOptions = ownedRaces.OrderBy(race => race.Name, StringComparer.OrdinalIgnoreCase)
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
                        RaceChoice, PlayerChoiceText.UnknownAction));
        }
    }

    private static PlayerOptionSelectionResult AddRace(
        GameEntity playerEntity,
        string raceName,
        RoomComponent? room,
        Point? pedestalCell,
        GameManager manager)
    {
        var ownedRaces = GetPlayerRaces(playerEntity);
        var race = manager.Game.Services.Data.PlayerRaces.Find(raceName);
        if (race == null
            || ownedRaces.Any(
                owned => string.Equals(
                    owned.GetRaceAbilityName(),
                    race.GetRaceAbilityName(),
                    StringComparison.OrdinalIgnoreCase)))
        {
            playerEntity.Player!.PendingChoice = null;
            return PlayerOptionSelectionResult.Reject(
                manager.Game.Services.Language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.OptionUnavailable));
        }

        manager.EffectApplicationSystem.AddAbilityEffect(
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
        var ownedRaces = GetPlayerRaces(playerEntity);
        if (ownedRaces.Count == 0)
        {
            playerEntity.Player!.PendingChoice = null;
            return PlayerOptionSelectionResult.Reject(
                manager.Game.Services.Language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.NoOptionToReplace));
        }

        var race = manager.Game.Services.Data.PlayerRaces.Find(raceName);
        if (race == null
            || !ownedRaces.Any(
                owned => string.Equals(
                    owned.GetRaceAbilityName(),
                    race.GetRaceAbilityName(),
                    StringComparison.OrdinalIgnoreCase)))
        {
            playerEntity.Player!.PendingChoice = null;
            return PlayerOptionSelectionResult.Reject(
                manager.Game.Services.Language.GetPlayerChoiceText(
                    RaceChoice, PlayerChoiceText.OptionUnavailable));
        }

        manager.EffectApplicationSystem.RemoveAbility(
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
                    RaceChoice, PlayerChoiceText.NoAvailableOptions));
        }

        var ownedRaceAbilityNames =
            EffectApplicationSystem.GetPlayerRaceAbilityNames(playerEntity);
        var selectedRaces = manager.Game.Random.WeightedOrder(
                playerEntity.Game.Services.Data.PlayerRaces.GetAsList(),
                race => ownedRaceAbilityNames.Contains(race.GetRaceAbilityName())
                    ? 0f
                    : 1f)
            .Take(DefaultOptionCount);

        playerEntity.Player!.PendingChoice = new PlayerOptions
        {
            ChoiceType = RaceChoice,
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
                    RaceChoice, PlayerChoiceText.NoAvailableOptions));
        }

        manager.EffectApplicationSystem.AddAbilityEffect(
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
        foreach (var abilityName in EffectApplicationSystem.GetPlayerRaceAbilityNames(playerEntity))
        {
            if (playerRaces.FindByIndex(abilityName) is { } race)
            {
                result.Add(race);
            }
        }

        return result;
    }

}

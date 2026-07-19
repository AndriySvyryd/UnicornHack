using CSharpScriptSerialization;
using UnicornHack.Data.Players;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class PlayerRace : Being, ICSScriptSerializable, ILoadable
{
    public IReadOnlyDictionary<int, IReadOnlyList<Effect>> Effects
    {
        get;
        set;
    } = new Dictionary<int, IReadOnlyList<Effect>>();

    public override PlayerRace? BaseBeing => BaseName == null ? null : Loader.Find(BaseName);

    public static GameEntity InstantiatePlayer(string name, Sex sex, RoomComponent room, Point cell)
    {
        var manager = room.Entity.Manager;
        using (var playerEntityReference = manager.CreateEntity())
        {
            var playerEntity = playerEntityReference.Referenced;

            var player = manager.CreateComponent<PlayerComponent>(EntityComponent.Player);
            player.NextActionTick = manager.Game.CurrentTick;
            player.PreviousActionTick = manager.Game.CurrentTick;
            player.ProperName = name;

            playerEntity.Player = player;

            var being = manager.CreateComponent<BeingComponent>(EntityComponent.Being);
            being.Sex = sex;

            playerEntity.Being = being;

            var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
            playerEntity.Physical = physical;

            playerEntity.ActiveEffects =
                manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

            var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
            position.RoomId = room.EntityId;
            position.Cell = cell;
            position.Heading = Direction.South;

            playerEntity.Position = position;

            room.IsVisited = true;

            var defaultRace = PlayerRaceData.Human;

            manager.EffectApplicationSystem.ApplyAbilityWithoutCarrier(AbilityData.Innate, playerEntity);

            manager.EffectApplicationSystem.AddAbilityEffect(
                playerEntity, defaultRace.GetRaceAbilityTemplate());

            return playerEntity;
        }
    }

    public override int GetTemplateId() => Loader.GetId(Name);

    protected override Dictionary<int, IReadOnlyList<Effect>> GetRaceLeveledEffects()
        => (Dictionary<int, IReadOnlyList<Effect>>)Effects;

    public static readonly IndexedCSScriptLoader<string, PlayerRace> Loader =
        new(@"Data\Players\", typeof(PlayerRaceData), race => race.GetRaceAbilityName(),
            StringComparer.OrdinalIgnoreCase);

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<PlayerRace>(GetPropertyConditions<PlayerRace>());

    protected static new Dictionary<string, Func<TPlayerRace, object?, bool>> GetPropertyConditions<TPlayerRace>()
        where TPlayerRace : PlayerRace
    {
        var propertyConditions = Being.GetPropertyConditions<TPlayerRace>();
        propertyConditions.Add(
            nameof(Effects), (_, value) => ((IReadOnlyDictionary<int, IReadOnlyList<Effect>>)value!).Count != 0);
        return propertyConditions;
    }

    ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
}

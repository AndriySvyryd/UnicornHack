using CSharpScriptSerialization;
using UnicornHack.Data.Features;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Features;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Feature : ILoadable, ICSScriptSerializable
{
    private Func<string, int, int, string, float>? _generationWeightFunction;

    public string Name { get; set; } = null!;
    public string GenerationWeight
    {
        get;
        set
        {
            field = value;
            _generationWeightFunction = null;
        }
    } = "1.0";

    public string? EnglishDescription
    {
        get; set;
    }
    public RoomFeatures RoomFeature
    {
        get; set;
    }
    public FeatureBehavior Behavior { get; set; } = FeatureBehavior.BlocksMovement | FeatureBehavior.AllowDestroy;
    public RewardType RewardType
    {
        get; set;
    }
    public AbilityType AbilityRewardType
    {
        get; set;
    }
    public ItemType ItemRewardType
    {
        get; set;
    }
    public List<Ability> Abilities { get; set; } = [];

    public float GetWeight(RoomComponent room, RoomFeatures kind, bool canBlockMovement)
        => RoomFeature != kind || !canBlockMovement && (Behavior & FeatureBehavior.BlocksMovement) != 0
            ? 0
            : RewardGenerationWeight.Evaluate(ref _generationWeightFunction, GenerationWeight, Name, room, Name);

    public GameEntity Instantiate(RoomComponent room, Point cell)
    {
        if (!room.IsValid(cell) || room.Features.ContainsKey(cell)
            || !room.Terrain[room.PointToIndex![cell.X, cell.Y]].CanMoveTo())
        {
            throw new InvalidOperationException($"Invalid placement for feature '{Name}' at {cell}.");
        }

        if (Behavior.HasFlag(FeatureBehavior.TriggerOnInterval) && Abilities.Count == 0)
        {
            throw new InvalidOperationException($"Timed feature '{Name}' requires abilities.");
        }

        AbilityLoader.Prepare(Abilities);
        var manager = room.Entity.Manager;
        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var feature = manager.CreateComponent<FeatureComponent>(EntityComponent.Feature);
        feature.Name = Name;
        feature.Template = this;
        feature.RewardRandom.Seed = room.Level.GenerationRandom.Seed;
        feature.Destroyed = Behavior.HasFlag(FeatureBehavior.AllowDestroy) ? false : null;
        entity.Feature = feature;
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = room.Entity;
        position.Cell = cell;
        position.Heading = (Direction)room.Level.GenerationRandom.Next(Vector.MovementDirections.Length);
        entity.Position = position;

        if (Abilities.Count != 0)
        {
            entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
            foreach (var template in Abilities)
            {
                template.AddToAffectable(entity);
            }
        }

        if (Behavior.HasFlag(FeatureBehavior.TriggerOnInterval))
        {
            feature.NextActionTick = checked(manager.Game.CurrentTick + manager.FeatureSystem.GetActionDelay(entity));
        }

        return entity;
    }

    public static readonly CSScriptLoader<Feature> Loader = new(@"Data\Features\", typeof(FeatureData));

    private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<Feature>(
        new Dictionary<string, Func<Feature, object?, bool>>
    {
        { nameof(Name), (_, value) => value != null },
        { nameof(GenerationWeight), (_, value) => (string)value! != "1.0" },
        { nameof(EnglishDescription), (_, value) => value != null },
        { nameof(RoomFeature), (_, value) => (RoomFeatures)value! != default },
        { nameof(Behavior), (_, value) => (FeatureBehavior)value! != (FeatureBehavior.BlocksMovement | FeatureBehavior.AllowDestroy) },
        { nameof(RewardType), (_, value) => (RewardType)value! != default },
        { nameof(AbilityRewardType), (_, value) => (AbilityType)value! != default },
        { nameof(ItemRewardType), (_, value) => (ItemType)value! != default },
        { nameof(Abilities), (_, value) => ((IReadOnlyList<Ability>)value!).Count != 0 }
    });

    public ICSScriptSerializer GetSerializer() => Serializer;
}

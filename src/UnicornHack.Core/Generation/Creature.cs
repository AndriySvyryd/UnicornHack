using CSharpScriptSerialization;
using UnicornHack.Data.Creatures;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class Creature : Being, ICSScriptSerializable, ILoadable
{
    private ActorNoiseType? _noise;
    private string? _englishDescription;
    private Sex? _sex;
    private string? _lycanthropy;
    public List<Ability> Abilities
    {
        get;
        set;
    } = new List<Ability>();

    public override Creature? BaseBeing => BaseName == null ? null : Loader.Find(BaseName);

    public string? PreviousStageName
    {
        get;
        set;
    }

    public string? NextStageName
    {
        get;
        set;
    }

    public byte InitialLevel
    {
        get;
        set;
    }

    public int XP
    {
        get;
        set;
    }

    private string? _generationWeight;

    public string? GenerationWeight
    {
        get => _generationWeight;
        set
        {
            _generationWeight = value;
            _weightFunction = null;
        }
    }

    public CreatureGenerationFlags GenerationFlags
    {
        get;
        set;
    }

    public bool IsNamed
        => IsBoss
           || Name.Length > 0 && char.IsUpper(Name[0]);

    public bool IsBoss
        => (GenerationFlags & (CreatureGenerationFlags.LevelBoss
                               | CreatureGenerationFlags.BranchBoss)) != 0;

    public bool IsNamedNonBoss => IsNamed && !IsBoss;

    public AIBehavior Behavior
    {
        get;
        set;
    }

    public ActorNoiseType? Noise
    {
        get => _noise ?? BaseBeing?.Noise ?? ActorNoiseType.Silent;
        set => _noise = value;
    }

    public Sex? Sex
    {
        get => _sex ?? BaseBeing?.Sex;
        set => _sex = value;
    }

    public override int GetTemplateId() => Loader.GetId(Name);

    protected override Dictionary<int, IReadOnlyList<Effect>> GetRaceLeveledEffects()
    {
        if (Abilities.Count == 0)
        {
            return [];
        }

        AbilityLoader.Prepare(Abilities);
        var granted = new Effect[Abilities.Count];
        for (var index = 0; index < Abilities.Count; index++)
        {
            granted[index] = new AddAbility { Ability = Abilities[index] };
        }

        return new Dictionary<int, IReadOnlyList<Effect>> { [1] = granted };
    }

    // TODO: Move to LanguageService
    public string? EnglishDescription
    {
        get => _englishDescription ?? BaseBeing?.EnglishDescription;
        set => _englishDescription = value;
    }

    public string? Lycanthropy
    {
        get => _lycanthropy ?? BaseBeing?.Lycanthropy;
        set => _lycanthropy = value;
    }

    public GameEntity Instantiate(RoomComponent room, Point cell)
    {
        var manager = room.Entity.Manager;
        using var creatureEntityReference = manager.CreateEntity();
        var creatureEntity = creatureEntityReference.Referenced;

        var ai = manager.CreateComponent<AIComponent>(EntityComponent.AI);
        ai.NextActionTick = manager.Game.CurrentTick;
        ai.PreviousActionTick = manager.Game.CurrentTick;
        ai.Behavior = Behavior;
        if (Noise != null)
        {
            ai.Noise = Noise.Value;
        }

        creatureEntity.AI = ai;

        var being = manager.CreateComponent<BeingComponent>(EntityComponent.Being);
        being.Sex = Sex ?? (room.Level.GenerationRandom.Roll(1, 2) > 1
            ? Primitives.Sex.Female
            : Primitives.Sex.Male);

        being.ExperiencePoints = XP != 0 ? XP : 100 + InitialLevel * 10;
        creatureEntity.Being = being;

        var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
        creatureEntity.Physical = physical;

        creatureEntity.ActiveEffects =
            manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

        // TODO: return null or try a different cell if cell occupied
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomId = room.EntityId;
        position.Cell = cell;
        position.Heading = (Direction)room.Level.GenerationRandom.Next(7);
        creatureEntity.Position = position;

        manager.EffectApplicationSystem.ApplyAbilityWithoutCarrier(AbilityData.Innate, creatureEntity);

        manager.AbilityLevelSystem.AddAbilityEffect(
            creatureEntity, GetRaceAbilityTemplate(), InitialLevel);

        return creatureEntity;
    }

    /// <summary>
    ///     Applies this creature template's stats to <paramref name="entity" /> as timed
    ///     Status-layer <see cref="ValueCombinationFunction.Override" /> ChangeProperty instances that force
    ///     each stat to the form's value for <paramref name="duration" /> ticks and then revert on their own (via
    ///     <see cref="EffectApplicationSystem.ExpireTimedPropertyEffects" />). Used to
    ///     transform a polymorphed being into this creature form. The aliveness-bootstrap field
    ///     <see cref="BeingComponent.HitPointMaximum" /> is left to the being's own attribute-driven
    ///     recomputation rather than overridden.
    /// </summary>
    public void ApplyFormOverride(GameEntity entity, int duration, GameManager manager)
        => ForEachBaseStat((property, value) =>
        {
            if (value == null
                || property == nameof(BeingComponent.HitPointMaximum))
            {
                return;
            }

            manager.EffectApplicationSystem.AddTimedPropertyOverride(value.Value, property, entity, duration, manager);
        });

    /// <summary>
    ///     Enumerates every base stat this creature template contributes, invoking <paramref name="apply" />
    ///     with the target property name and the template's value (null when the stat is unset).
    /// </summary>
    private void ForEachBaseStat(Action<string, int?> apply)
    {
        ForEachBeingStat(apply);

        // TODO: Populate the remaining sensory/locomotion/behavior traits once their components gain the
        // matching properties: RespirationType, LocomotionType, EyeCount, Quietness, VisibilityLevel,
        // AcuityLevel, HearingLevel, Telepathic, Clairvoyant, Clingy, Displaced, Reflective, Mindless,
        // NonAnimal, Reanimation.
    }

    private Func<string, int, int, float>? _weightFunction;

    protected static readonly string DefaultWeight = "1.0";

    public static Func<string, int, int, float> CreateWeightFunction(string expression)
        => UnicornExpressionVisitor.CreateGenerationWeightFunction(expression);

    public bool CanPlaceInstance(GameLevel level)
        => !IsNamed
           || level.Game.GenerationStats.CreatureInstancesByName.GetValueOrDefault(Name) <= 0
           && (!IsNamedNonBoss || level.GenerationStats.NamedNonBossCreatureCount <= 0);

    public float GetWeight(RoomComponent room)
    {
        var levelStats = room.Level.GenerationStats;
        if (!CanPlaceInstance(room.Level))
        {
            return 0;
        }

        return UnicornExpressionVisitor.EvaluateGenerationWeight(
            ref _weightFunction, GenerationWeight ?? DefaultWeight, Name, room.Level.Branch.Name, room.Level.Depth,
            levelStats.CreatureInstancesByName.GetValueOrDefault(Name));
    }

    public static Creature? GetTemplate(GameEntity entity)
    {
        if (entity.Player != null
            || entity.Being is not { } being)
        {
            return null;
        }

        Creature? firstTemplate = null;
        var firstTemplateEntityId = int.MaxValue;
        Creature? namedTemplate = null;
        var namedTemplateEntityId = int.MaxValue;
        foreach (var raceEntity in being.Races)
        {
            var raceName = raceEntity.Ability?.Template?.Name ?? raceEntity.Ability?.Name;
            if (raceName != null
                && entity.Manager.Game.Services.Data.Creatures.FindByIndex(raceName) is { } creature)
            {
                if (raceEntity.Id < firstTemplateEntityId)
                {
                    firstTemplate = creature;
                    firstTemplateEntityId = raceEntity.Id;
                }

                if (creature.IsNamed
                    && raceEntity.Id < namedTemplateEntityId)
                {
                    namedTemplate = creature;
                    namedTemplateEntityId = raceEntity.Id;
                }
            }
        }

        return namedTemplate ?? firstTemplate;
    }

    public static readonly IndexedGroupedCSScriptLoader<byte, string, Creature> Loader =
        new(@"Data\Creatures\", c => c.InitialLevel, typeof(CreatureData),
            creature => creature.GetRaceAbilityName(), StringComparer.OrdinalIgnoreCase);

    private static byte? _maxLevel;

    public static byte MaxLevel
    {
        get
        {
            _maxLevel ??= Loader.GetAllKeys().Max();
            return _maxLevel.Value;
        }
    }

    private static readonly CSScriptSerializer Serializer =
        new PropertyCSScriptSerializer<Creature>(GetPropertyConditions<Creature>());

    protected static new Dictionary<string, Func<TCreatureVariant, object?, bool>>
        GetPropertyConditions<TCreatureVariant>() where TCreatureVariant : Creature
    {
        var propertyConditions = Being.GetPropertyConditions<TCreatureVariant>();
        propertyConditions.Add(nameof(Abilities), (_, value) => ((IReadOnlyList<Ability>)value!).Count != 0);
        propertyConditions.Add(
            nameof(EnglishDescription),
            (o, v) => (string?)v != o.BaseBeing?.EnglishDescription);
        propertyConditions.Add(nameof(InitialLevel), (_, v) => (byte)v! != 0);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        propertyConditions.Add(
            nameof(GenerationWeight),
            (_, v) => v != null && (string)v != DefaultWeight);
        propertyConditions.Add(nameof(PreviousStageName), (_, v) => v != null);
        propertyConditions.Add(nameof(NextStageName), (_, v) => v != null);
        propertyConditions.Add(
            nameof(GenerationFlags),
            (_, v) => (CreatureGenerationFlags)v! != CreatureGenerationFlags.None);
        propertyConditions.Add(nameof(Behavior), (_, v) => (AIBehavior)v! != AIBehavior.None);
        propertyConditions.Add(
            nameof(Noise),
            (o, v) => (ActorNoiseType)v! != (o.BaseBeing?.Noise ?? ActorNoiseType.Silent));
        propertyConditions.Add(nameof(Sex), (o, v) => (Sex?)v != o.BaseBeing?.Sex);
        propertyConditions.Add(nameof(Lycanthropy), (o, v) => (string?)v != o.BaseBeing?.Lycanthropy);

        return propertyConditions;
    }

    public ICSScriptSerializer GetSerializer() => Serializer;
}

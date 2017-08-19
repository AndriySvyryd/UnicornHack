using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Properties;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class CreatureVariant : ICSScriptSerializable, ILoadable
    {
        private string _corpseVariantName;
        private ActorNoiseType? _noise;
        private Species? _species;
        private SpeciesClass? _speciesClass;
        private int? _movementDelay;
        private int? _weight;
        private Material? _material;
        private ISet<string> _simpleProperties;
        private IDictionary<string, object> _valuedProperties;

        public virtual string Name { get; set; }

        public virtual Species Species
        {
            get => _species ?? BaseActor?.Species ?? Species.Default;
            set => _species = value;
        }

        public virtual SpeciesClass SpeciesClass
        {
            get => _speciesClass ?? BaseActor?.SpeciesClass ?? SpeciesClass.None;
            set => _speciesClass = value;
        }

        public virtual int MovementDelay
        {
            get => _movementDelay ?? BaseActor?.MovementDelay ?? 0;
            set => _movementDelay = value;
        }

        /// <summary> 100g units </summary>
        public virtual int Weight
        {
            get => _weight ?? BaseActor?.Weight ?? 0;
            set => _weight = value;
        }

        public virtual Material Material
        {
            get => _material ?? BaseActor?.Material ?? Material.Flesh;
            set => _material = value;
        }

        public virtual ISet<string> SimpleProperties
        {
            get
            {
                if (_simpleProperties != null)
                {
                    return _simpleProperties;
                }
                if (BaseActor != null)
                {
                    return BaseActor.SimpleProperties;
                }
                return _simpleProperties = new HashSet<string>();
            }
            set => _simpleProperties = value;
        }

        public virtual IDictionary<string, object> ValuedProperties
        {
            get
            {
                if (_valuedProperties != null)
                {
                    return _valuedProperties;
                }
                if (BaseActor != null)
                {
                    return BaseActor.ValuedProperties;
                }
                return _valuedProperties = new Dictionary<string, object>();
            }
            set => _valuedProperties = value;
        }

        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public byte InitialLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        public GenerationFlags GenerationFlags { get; set; }
        public MonsterBehavior Behavior { get; set; }

        public ActorNoiseType Noise
        {
            get => _noise ?? BaseActor?.Noise ?? ActorNoiseType.Silent;
            set => _noise = value;
        }

        public string CorpseName
        {
            get => _corpseVariantName ?? BaseActor?.CorpseName;
            set => _corpseVariantName = value;
        }

        public virtual string BaseName { get; set; }
        public CreatureVariant BaseActor => BaseName == null ? null : Loader.Get(BaseName);

        public string PreviousStageName { get; set; }
        public string NextStageName { get; set; }

        public virtual Creature Instantiate(Level level, byte x, byte y)
        {
            var creature = new Creature(level, x, y)
            {
                BaseName = Name,
                Species = Species,
                SpeciesClass = SpeciesClass,
                MovementDelay = MovementDelay,
                Weight = Weight,
                Material = Material,
                Noise = Noise,
                Behavior = Behavior,
                CorpseVariantName = CorpseName,
                PreviousStageName = PreviousStageName,
                NextStageName = NextStageName,
                DifficultyLevel = InitialLevel,
                XP = XP
            };

            var innateAbility =
                new Ability(level.Game) {Name = Actor.InnateName, Activation = AbilityActivation.Always};

            var sexSet = false;
            foreach (var simpleProperty in SimpleProperties)
            {
                switch (simpleProperty)
                {
                    case nameof(PropertyData.Asexuality):
                        creature.Sex = Sex.None;
                        sexSet = true;
                        break;
                    case nameof(PropertyData.Maleness):
                        creature.Sex = Sex.Male;
                        sexSet = true;
                        break;
                    case nameof(PropertyData.Femaleness):
                        creature.Sex = Sex.Female;
                        sexSet = true;
                        break;
                    default:
                        innateAbility.Effects.Add(
                            new ChangeProperty<bool>(level.Game) {PropertyName = simpleProperty, Value = true});
                        break;
                }
            }

            if (!sexSet)
            {
                creature.Sex = level.Game.Random.Roll(1, 2) > 1 ? Sex.Female : Sex.Male;
            }

            foreach (var valuedProperty in ValuedProperties)
            {
                innateAbility.Effects.Add(
                    Effect.CreateChangeValuedProperty(level.Game, valuedProperty.Key, valuedProperty.Value));
            }

            foreach (var ability in Abilities)
            {
                creature.Abilities.Add(ability.Instantiate(level.Game));
            }

            creature.MaxHP = 50 + creature.DifficultyLevel * 5;
            creature.MaxHP = creature.MaxHP < 1 ? 1 : creature.MaxHP;
            creature.HP = creature.MaxHP;

            creature.RecalculateEffectsAndAbilities();
            return creature;
        }

        private Func<string, byte, int, float> _weightFunction;

        public virtual float GetWeight(Level level)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateCreatureWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0);
        }

        public static readonly GroupedCSScriptLoader<byte, CreatureVariant> Loader =
            new GroupedCSScriptLoader<byte, CreatureVariant>(@"Data\Creatures\", c => c.InitialLevel,
                typeof(CreatureData));

        private static byte? _maxLevel;

        public static byte MaxLevel
        {
            get
            {
                if (_maxLevel == null)
                {
                    _maxLevel = Loader.GetAllKeys().Max();
                }
                return _maxLevel.Value;
            }
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<CreatureVariant>(GetPropertyConditions<CreatureVariant>());

        protected static Dictionary<string, Func<TCreatureVariant, object, bool>>
            GetPropertyConditions<TCreatureVariant>() where TCreatureVariant : CreatureVariant =>
            new Dictionary<string, Func<TCreatureVariant, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(BaseName), (o, v) => v != null},
                {nameof(Species), (o, v) => (Species)v != (o.BaseActor?.Species ?? Species.Default)},
                {nameof(SpeciesClass), (o, v) => (SpeciesClass)v != (o.BaseActor?.SpeciesClass ?? SpeciesClass.None)},
                {nameof(MovementDelay), (o, v) => (int)v != (o.BaseActor?.MovementDelay ?? 0)},
                {nameof(Weight), (o, v) => (int)v != (o.BaseActor?.Weight ?? 0)},
                {nameof(Material), (o, v) => (Material)v != (o.BaseActor?.Material ?? Material.Flesh)},
                {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0},
                {nameof(SimpleProperties), (o, v) => ((ICollection<string>)v).Count != 0},
                {nameof(ValuedProperties), (o, v) => ((IDictionary<string, object>)v).Keys.Count != 0},
                {nameof(InitialLevel), (o, v) => (byte)v != 0},
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                {
                    nameof(GenerationWeight),
                    (o, v) => (Weight)v != null && (!(v is DefaultWeight def) || def.Multiplier != 1)
                },
                {nameof(PreviousStageName), (o, v) => v != null},
                {nameof(NextStageName), (o, v) => v != null},
                {nameof(CorpseName), (o, v) => (string)v != o.BaseActor?.CorpseName},
                {nameof(GenerationFlags), (o, v) => (GenerationFlags)v != GenerationFlags.None},
                {nameof(Behavior), (o, v) => (MonsterBehavior)v != MonsterBehavior.None},
                {nameof(Noise), (o, v) => (ActorNoiseType)v != (o.BaseActor?.Noise ?? ActorNoiseType.Silent)}
            };

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}
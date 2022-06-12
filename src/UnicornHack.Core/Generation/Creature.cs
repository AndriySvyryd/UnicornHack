using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CSharpScriptSerialization;
using UnicornHack.Data.Creatures;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation
{
    public class Creature : Being, ICSScriptSerializable, ILoadable
    {
        private ActorNoiseType? _noise;
        private Species? _species;
        private SpeciesClass? _speciesClass;
        private string _englishDescription;
        private int? _movementDelay;
        private int? _turningDelay;
        private Material? _material;
        private Sex? _sex;
        private int? _size;
        private int? _weight;
        private int? _perception;
        private int? _speed;
        private int? _might;
        private int? _focus;
        private int? _energyRegeneration;
        private int? _regeneration;
        private int? _bonusHitPoints;
        private int? _bonusEnergyPoints;
        private int? _accuracy;
        private int? _evasion;
        private int? _magicResistance;
        private int? _armor;
        private int? _deflection;
        private int? _physicalResistance;
        private int? _acidResistance;
        private int? _bleedingResistance;
        private int? _coldResistance;
        private int? _electricityResistance;
        private int? _fireResistance;
        private int? _lightResistance;
        private int? _psychicResistance;
        private int? _sonicResistance;
        private int? _stunResistance;
        private int? _voidResistance;
        private int? _toxinResistance;
        private int? _waterResistance;
        private bool? _reflective;
        private bool? _slimingImmune;
        private bool? _stoningImmune;
        private HeadType? _headType;
        private TorsoType? _torsoType;
        private ExtremityType? _upperExtremities;
        private ExtremityType? _lowerExtremities;
        private ExtremityType? _backExtremities;
        private RespirationType? _respirationType;
        private LocomotionType? _locomotionType;
        private int? _slotCapacity;
        private int? _eyeCount;
        private int? _noiseLevel;
        private int? _visibilityLevel;
        private int? _acuityLevel;
        private int? _hearingLevel;
        private int? _telepathic;
        private bool? _infravisible;
        private bool? _infravision;
        private bool? _invisibilityDetection;
        private bool? _clairvoyant;
        private bool? _clingy;
        private bool? _displaced;
        private string _lycanthropy;
        private bool? _mindless;
        private bool? _nonAnimal;
        private bool? _reanimation;

        public string BaseName { get; set; }
        public Creature BaseCreature => BaseName == null ? null : Loader.Find(BaseName);

        public string PreviousStageName { get; set; }
        public string NextStageName { get; set; }

        public byte InitialLevel { get; set; }
        public int XP { get; set; }

        private string _generationWeight;
        public string GenerationWeight
        {
            get => _generationWeight;
            set
            {
                _generationWeight = value;
                _weightFunction = null;
            }
        }

        public GenerationFlags GenerationFlags { get; set; }
        public AIBehavior Behavior { get; set; }

        public ActorNoiseType? Noise
        {
            get => _noise ?? BaseCreature?.Noise ?? ActorNoiseType.Silent;
            set => _noise = value;
        }

        public Sex? Sex
        {
            get => _sex ?? BaseCreature?.Sex;
            set => _sex = value;
        }

        public override Species? Species
        {
            get => _species ?? BaseCreature?.Species;
            set => _species = value;
        }

        public override SpeciesClass? SpeciesClass
        {
            get => _speciesClass ?? BaseCreature?.SpeciesClass;
            set => _speciesClass = value;
        }

        public string EnglishDescription
        {
            get => _englishDescription ?? BaseCreature?.EnglishDescription;
            set => _englishDescription = value;
        }

        public Material? Material
        {
            get => _material ?? BaseCreature?.Material;
            set => _material = value;
        }

        public int? MovementDelay
        {
            get => _movementDelay ?? BaseCreature?.MovementDelay;
            set => _movementDelay = value;
        }

        public int? TurningDelay
        {
            get => _turningDelay ?? BaseCreature?.TurningDelay;
            set => _turningDelay = value;
        }

        public int? Size
        {
            get => _size ?? BaseCreature?.Size;
            set => _size = value;
        }

        public int? Weight
        {
            get => _weight ?? BaseCreature?.Weight;
            set => _weight = value;
        }

        public int? Perception
        {
            get => _perception ?? BaseCreature?.Perception;
            set => _perception = value;
        }

        public int? Might
        {
            get => _might ?? BaseCreature?.Might;
            set => _might = value;
        }

        public int? Speed
        {
            get => _speed ?? BaseCreature?.Speed;
            set => _speed = value;
        }

        public int? Focus
        {
            get => _focus ?? BaseCreature?.Focus;
            set => _focus = value;
        }

        public int? EnergyRegeneration
        {
            get => _energyRegeneration ?? BaseCreature?.EnergyRegeneration;
            set => _energyRegeneration = value;
        }

        public int? Regeneration
        {
            get => _regeneration ?? BaseCreature?.Regeneration;
            set => _regeneration = value;
        }

        public int? BonusHitPoints
        {
            get => _bonusHitPoints ?? BaseCreature?.BonusHitPoints;
            set => _bonusHitPoints = value;
        }

        public int? BonusEnergyPoints
        {
            get => _bonusEnergyPoints ?? BaseCreature?.BonusEnergyPoints;
            set => _bonusEnergyPoints = value;
        }

        public int? Accuracy
        {
            get => _accuracy ?? BaseCreature?.Accuracy;
            set => _accuracy = value;
        }

        public int? Evasion
        {
            get => _evasion ?? BaseCreature?.Evasion;
            set => _evasion = value;
        }

        public int? Deflection
        {
            get => _deflection ?? BaseCreature?.Deflection;
            set => _deflection = value;
        }

        public int? Armor
        {
            get => _armor ?? BaseCreature?.Armor;
            set => _armor = value;
        }

        public int? PhysicalResistance
        {
            get => _physicalResistance ?? BaseCreature?.PhysicalResistance;
            set => _physicalResistance = value;
        }

        public int? MagicResistance
        {
            get => _magicResistance ?? BaseCreature?.MagicResistance;
            set => _magicResistance = value;
        }

        public int? AcidResistance
        {
            get => _acidResistance ?? BaseCreature?.AcidResistance;
            set => _acidResistance = value;
        }

        public int? BleedingResistance
        {
            get => _bleedingResistance ?? BaseCreature?.BleedingResistance;
            set => _bleedingResistance = value;
        }

        public int? ColdResistance
        {
            get => _coldResistance ?? BaseCreature?.ColdResistance;
            set => _coldResistance = value;
        }

        public int? ElectricityResistance
        {
            get => _electricityResistance ?? BaseCreature?.ElectricityResistance;
            set => _electricityResistance = value;
        }

        public int? FireResistance
        {
            get => _fireResistance ?? BaseCreature?.FireResistance;
            set => _fireResistance = value;
        }

        public int? LightResistance
        {
            get => _lightResistance ?? BaseCreature?.LightResistance;
            set => _lightResistance = value;
        }

        public int? PsychicResistance
        {
            get => _psychicResistance ?? BaseCreature?.PsychicResistance;
            set => _psychicResistance = value;
        }

        public int? SonicResistance
        {
            get => _sonicResistance ?? BaseCreature?.SonicResistance;
            set => _sonicResistance = value;
        }

        public int? StunResistance
        {
            get => _stunResistance ?? BaseCreature?.StunResistance;
            set => _stunResistance = value;
        }

        public int? ToxinResistance
        {
            get => _toxinResistance ?? BaseCreature?.ToxinResistance;
            set => _toxinResistance = value;
        }

        public int? VoidResistance
        {
            get => _voidResistance ?? BaseCreature?.VoidResistance;
            set => _voidResistance = value;
        }

        public int? WaterResistance
        {
            get => _waterResistance ?? BaseCreature?.WaterResistance;
            set => _waterResistance = value;
        }

        public bool? Reflective
        {
            get => _reflective ?? BaseCreature?.Reflective;
            set => _reflective = value;
        }

        public bool? SlimingImmune
        {
            get => _slimingImmune ?? BaseCreature?.SlimingImmune;
            set => _slimingImmune = value;
        }

        public bool? StoningImmune
        {
            get => _stoningImmune ?? BaseCreature?.StoningImmune;
            set => _stoningImmune = value;
        }

        public HeadType? HeadType
        {
            get => _headType ?? BaseCreature?.HeadType;
            set => _headType = value;
        }

        public TorsoType? TorsoType
        {
            get => _torsoType ?? BaseCreature?.TorsoType;
            set => _torsoType = value;
        }

        public ExtremityType? UpperExtremities
        {
            get => _upperExtremities ?? BaseCreature?.UpperExtremities;
            set => _upperExtremities = value;
        }

        public ExtremityType? LowerExtremities
        {
            get => _lowerExtremities ?? BaseCreature?.LowerExtremities;
            set => _lowerExtremities = value;
        }

        public ExtremityType? BackExtremities
        {
            get => _backExtremities ?? BaseCreature?.BackExtremities;
            set => _backExtremities = value;
        }

        public RespirationType? RespirationType
        {
            get => _respirationType ?? BaseCreature?.RespirationType;
            set => _respirationType = value;
        }

        public LocomotionType? LocomotionType
        {
            get => _locomotionType ?? BaseCreature?.LocomotionType;
            set => _locomotionType = value;
        }

        public int? SlotCapacity
        {
            get => _slotCapacity ?? BaseCreature?.SlotCapacity;
            set => _slotCapacity = value;
        }

        public int? EyeCount
        {
            get => _eyeCount ?? BaseCreature?.EyeCount;
            set => _eyeCount = value;
        }

        public int? NoiseLevel
        {
            get => _noiseLevel ?? BaseCreature?.NoiseLevel;
            set => _noiseLevel = value;
        }

        public int? VisibilityLevel
        {
            get => _visibilityLevel ?? BaseCreature?.VisibilityLevel;
            set => _visibilityLevel = value;
        }

        public int? AcuityLevel
        {
            get => _acuityLevel ?? BaseCreature?.AcuityLevel;
            set => _acuityLevel = value;
        }

        public int? HearingLevel
        {
            get => _hearingLevel ?? BaseCreature?.HearingLevel;
            set => _hearingLevel = value;
        }

        public int? Telepathic
        {
            get => _telepathic ?? BaseCreature?.Telepathic;
            set => _telepathic = value;
        }

        public bool? Infravisible
        {
            get => _infravisible ?? BaseCreature?.Infravisible;
            set => _infravisible = value;
        }

        public bool? Infravision
        {
            get => _infravision ?? BaseCreature?.Infravision;
            set => _infravision = value;
        }

        public bool? InvisibilityDetection
        {
            get => _invisibilityDetection ?? BaseCreature?.InvisibilityDetection;
            set => _invisibilityDetection = value;
        }

        public bool? Clairvoyant
        {
            get => _clairvoyant ?? BaseCreature?.Clairvoyant;
            set => _clairvoyant = value;
        }

        public bool? Clingy
        {
            get => _clingy ?? BaseCreature?.Clingy;
            set => _clingy = value;
        }

        public bool? Displaced
        {
            get => _displaced ?? BaseCreature?.Displaced;
            set => _displaced = value;
        }

        public string Lycanthropy
        {
            get => _lycanthropy ?? BaseCreature?.Lycanthropy;
            set => _lycanthropy = value;
        }

        public bool? Mindless
        {
            get => _mindless ?? BaseCreature?.Mindless;
            set => _mindless = value;
        }

        public bool? NonAnimal
        {
            get => _nonAnimal ?? BaseCreature?.NonAnimal;
            set => _nonAnimal = value;
        }

        public bool? Reanimation
        {
            get => _reanimation ?? BaseCreature?.Reanimation;
            set => _reanimation = value;
        }

        public GameEntity Instantiate(LevelComponent level, Point cell)
        {
            var manager = level.Entity.Manager;
            using var creatureEntityReference = manager.CreateEntity();
            var creatureEntity = creatureEntityReference.Referenced;

            var ai = manager.CreateComponent<AIComponent>(EntityComponent.AI);
            ai.NextActionTick = manager.Game.CurrentTick;
            ai.Behavior = Behavior;
            if (Noise != null)
            {
                ai.Noise = Noise.Value;
            }

            creatureEntity.AI = ai;

            var being = manager.CreateComponent<BeingComponent>(EntityComponent.Being);
            being.Sex = Sex ?? (level.GenerationRandom.Roll(1, 2) > 1
                ? Primitives.Sex.Female
                : Primitives.Sex.Male);

            being.ExperiencePoints = XP;
            creatureEntity.Being = being;

            var sensor = manager.CreateComponent<SensorComponent>(EntityComponent.Sensor);
            creatureEntity.Sensor = sensor;

            var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
            creatureEntity.Physical = physical;

            var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
            position.LevelId = level.EntityId;
            position.LevelCell = cell;
            position.Heading = (Direction)level.GenerationRandom.Next(7);
            creatureEntity.Position = position;

            using var innateAbilityReference = manager.CreateEntity();
            var innateAbilityEntity = innateAbilityReference.Referenced;

            var innateEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            innateEffect.EffectType = EffectType.AddAbility;
            innateEffect.Duration = EffectDuration.Infinite;

            innateAbilityEntity.Effect = innateEffect;

            var innateAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            innateAbility.Name = EffectApplicationSystem.InnateAbilityName;
            innateAbility.Activation = ActivationType.Always;
            innateAbility.SuccessCondition = AbilitySuccessCondition.Always;

            innateAbilityEntity.Ability = innateAbility;

            AddPropertyEffect(
                nameof(PositionComponent.MovementDelay), MovementDelay, innateAbilityEntity.Id, manager);
            AddPropertyEffect(
                nameof(PositionComponent.TurningDelay), TurningDelay, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(PhysicalComponent.Size), Size, innateAbilityEntity.Id, manager,
                ValueCombinationFunction.MeanRoundDown);
            AddPropertyEffect(nameof(PhysicalComponent.Weight), Weight, innateAbilityEntity.Id, manager,
                ValueCombinationFunction.MeanRoundDown);
            AddPropertyEffect(nameof(PhysicalComponent.Material), (int?)(Material ?? Primitives.Material.Flesh),
                innateAbilityEntity.Id, manager, ValueCombinationFunction.Override);
            AddPropertyEffect(nameof(PhysicalComponent.Capacity),
                (SlotCapacity ?? AbilitySlottingSystem.DefaultSlotCapacity) + 2, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Perception), Perception, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Might), Might, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Speed), Speed, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Focus), Focus, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.EnergyRegeneration), EnergyRegeneration, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.Regeneration), Regeneration, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.HitPointMaximum), BonusHitPoints, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.EnergyPointMaximum), BonusEnergyPoints, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.Accuracy), Accuracy, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Evasion), Evasion, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Deflection), Deflection, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.Armor), Armor, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.PhysicalResistance), PhysicalResistance, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.MagicResistance), MagicResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.AcidResistance), AcidResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.BleedingResistance), BleedingResistance, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.ColdResistance), ColdResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.ElectricityResistance), ElectricityResistance, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.FireResistance), FireResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.LightResistance), LightResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.PsychicResistance), PsychicResistance, innateAbilityEntity.Id,
                manager);
            AddPropertyEffect(nameof(BeingComponent.SonicResistance), SonicResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.StunResistance), StunResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.ToxinResistance), ToxinResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.VoidResistance), VoidResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.WaterResistance), WaterResistance, innateAbilityEntity.Id, manager);
            AddPropertyEffect(nameof(BeingComponent.UpperExtremities), (int?)UpperExtremities, innateAbilityEntity.Id,
                manager);

            // TODO: Populate other properties

            innateAbility.OwnerEntity = creatureEntity;
            innateEffect.AffectedEntity = creatureEntity;

            using var raceEffectEntityReference = manager.CreateEntity();
            var raceEffectEntity = raceEffectEntityReference.Referenced;
            var raceEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            raceEffect.EffectType = EffectType.ChangeRace;
            raceEffect.Duration = EffectDuration.Infinite;

            raceEffectEntity.Effect = raceEffect;

            var race = AddToAppliedEffect(raceEffectEntity, creatureEntity);
            race.Level = InitialLevel;

            raceEffect.AffectedEntity = creatureEntity;

            return creatureEntity;
        }

        private Func<string, int, int, float> _weightFunction;

        protected static readonly string DefaultWeight = "1.0";

        protected static readonly ParameterExpression BranchParameter =
            Expression.Parameter(typeof(string), name: "branch");

        protected static readonly ParameterExpression DepthParameter =
            Expression.Parameter(typeof(int), name: "depth");

        protected static readonly ParameterExpression InstancesParameter =
            Expression.Parameter(typeof(int), name: "instances");

        private static readonly UnicornExpressionVisitor _translator =
            new(new[] { BranchParameter, DepthParameter, InstancesParameter });

        public static Func<string, int, int, float> CreateWeightFunction(string expression)
            => _translator.Translate<Func<string, int, int, float>, float>(expression);

        public float GetWeight(LevelComponent level)
        {
            if (_weightFunction == null)
            {
                try
                {
                    _weightFunction = CreateWeightFunction(GenerationWeight ?? DefaultWeight);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Error while parsing the GenerationWeight for " + Name, e);
                }
            }

            try
            {
                return _weightFunction(level.Branch.Name, level.Depth, 0);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error while evaluating the Weight for " + Name, e);
            }
        }

        public static readonly GroupedCSScriptLoader<byte, Creature> Loader =
            new GroupedCSScriptLoader<byte, Creature>(@"Data\Creatures\", c => c.InitialLevel,
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
            new PropertyCSScriptSerializer<Creature>(GetPropertyConditions<Creature>());

        protected static Dictionary<string, Func<TCreatureVariant, object, bool>>
            GetPropertyConditions<TCreatureVariant>() where TCreatureVariant : Creature =>
            new()
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(BaseName), (o, v) => v != null},
                {nameof(Species), (o, v) => (Species?)v != o.BaseCreature?.Species},
                {nameof(SpeciesClass), (o, v) => (SpeciesClass?)v != o.BaseCreature?.SpeciesClass},
                {nameof(EnglishDescription), (o, v) => (string)v != o.BaseCreature?.EnglishDescription},
                {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0},
                {nameof(InitialLevel), (o, v) => (byte)v != 0},
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                {
                    nameof(GenerationWeight),
                    (o, v) => v != null && (string)v != DefaultWeight
                },
                {nameof(PreviousStageName), (o, v) => v != null},
                {nameof(NextStageName), (o, v) => v != null},
                {nameof(GenerationFlags), (o, v) => (GenerationFlags)v != GenerationFlags.None},
                {nameof(Behavior), (o, v) => (AIBehavior)v != AIBehavior.None},
                {nameof(Noise), (o, v) => (ActorNoiseType)v != (o.BaseCreature?.Noise ?? ActorNoiseType.Silent)},
                {nameof(Sex), (o, v) => (Sex?)v != o.BaseCreature?.Sex},
                {nameof(Size), (o, v) => (int?)v != o.BaseCreature?.Size},
                {nameof(Weight), (o, v) => (int?)v != o.BaseCreature?.Weight},
                {nameof(MovementDelay), (o, v) => (int?)v != o.BaseCreature?.MovementDelay},
                {nameof(TurningDelay), (o, v) => (int?)v != o.BaseCreature?.TurningDelay},
                {nameof(Material), (o, v) => (Material?)v != o.BaseCreature?.Material},
                {nameof(Perception), (o, v) => (int?)v != o.BaseCreature?.Perception},
                {nameof(Might), (o, v) => (int?)v != o.BaseCreature?.Might},
                {nameof(Speed), (o, v) => (int?)v != o.BaseCreature?.Speed},
                {nameof(Focus), (o, v) => (int?)v != o.BaseCreature?.Focus},
                {nameof(EnergyRegeneration), (o, v) => (int?)v != o.BaseCreature?.EnergyRegeneration},
                {nameof(Regeneration), (o, v) => (int?)v != o.BaseCreature?.Regeneration},
                {nameof(BonusHitPoints), (o, v) => (int?)v != o.BaseCreature?.BonusHitPoints},
                {nameof(BonusEnergyPoints), (o, v) => (int?)v != o.BaseCreature?.BonusEnergyPoints},
                {nameof(Accuracy), (o, v) => (int?)v != o.BaseCreature?.Accuracy},
                {nameof(Evasion), (o, v) => (int?)v != o.BaseCreature?.Evasion},
                {nameof(Deflection), (o, v) => (int?)v != o.BaseCreature?.Deflection},
                {nameof(Armor), (o, v) => (int?)v != o.BaseCreature?.Armor},
                {nameof(PhysicalResistance), (o, v) => (int?)v != o.BaseCreature?.PhysicalResistance},
                {nameof(MagicResistance), (o, v) => (int?)v != o.BaseCreature?.MagicResistance},
                {nameof(AcidResistance), (o, v) => (int?)v != o.BaseCreature?.AcidResistance},
                {nameof(BleedingResistance), (o, v) => (int?)v != o.BaseCreature?.BleedingResistance},
                {nameof(ColdResistance), (o, v) => (int?)v != o.BaseCreature?.ColdResistance},
                {nameof(ElectricityResistance), (o, v) => (int?)v != o.BaseCreature?.ElectricityResistance},
                {nameof(FireResistance), (o, v) => (int?)v != o.BaseCreature?.FireResistance},
                {nameof(LightResistance), (o, v) => (int?)v != o.BaseCreature?.LightResistance},
                {nameof(PsychicResistance), (o, v) => (int?)v != o.BaseCreature?.PsychicResistance},
                {nameof(SonicResistance), (o, v) => (int?)v != o.BaseCreature?.SonicResistance},
                {nameof(StunResistance), (o, v) => (int?)v != o.BaseCreature?.StunResistance},
                {nameof(ToxinResistance), (o, v) => (int?)v != o.BaseCreature?.ToxinResistance},
                {nameof(VoidResistance), (o, v) => (int?)v != o.BaseCreature?.VoidResistance},
                {nameof(WaterResistance), (o, v) => (int?)v != o.BaseCreature?.WaterResistance},
                {nameof(Reflective), (o, v) => (bool?)v != o.BaseCreature?.Reflective},
                {nameof(SlimingImmune), (o, v) => (bool?)v != o.BaseCreature?.SlimingImmune},
                {nameof(StoningImmune), (o, v) => (bool?)v != o.BaseCreature?.StoningImmune},
                {nameof(HeadType), (o, v) => (HeadType?)v != o.BaseCreature?.HeadType},
                {nameof(TorsoType), (o, v) => (TorsoType?)v != o.BaseCreature?.TorsoType},
                {nameof(UpperExtremities), (o, v) => (ExtremityType?)v != o.BaseCreature?.UpperExtremities},
                {nameof(LowerExtremities), (o, v) => (ExtremityType?)v != o.BaseCreature?.LowerExtremities},
                {nameof(BackExtremities), (o, v) => (ExtremityType?)v != o.BaseCreature?.BackExtremities},
                {nameof(RespirationType), (o, v) => (RespirationType?)v != o.BaseCreature?.RespirationType},
                {nameof(LocomotionType), (o, v) => (LocomotionType?)v != o.BaseCreature?.LocomotionType},
                {nameof(SlotCapacity), (o, v) => (int?)v != o.BaseCreature?.SlotCapacity},
                {nameof(EyeCount), (o, v) => (int?)v != o.BaseCreature?.EyeCount},
                {nameof(NoiseLevel), (o, v) => (int?)v != o.BaseCreature?.NoiseLevel},
                {nameof(VisibilityLevel), (o, v) => (int?)v != o.BaseCreature?.VisibilityLevel},
                {nameof(AcuityLevel), (o, v) => (int?)v != o.BaseCreature?.AcuityLevel},
                {nameof(HearingLevel), (o, v) => (int?)v != o.BaseCreature?.HearingLevel},
                {nameof(Telepathic), (o, v) => (int?)v != o.BaseCreature?.Telepathic},
                {nameof(Infravisible), (o, v) => (bool?)v != o.BaseCreature?.Infravisible},
                {nameof(Infravision), (o, v) => (bool?)v != o.BaseCreature?.Infravision},
                {nameof(InvisibilityDetection), (o, v) => (bool?)v != o.BaseCreature?.InvisibilityDetection},
                {nameof(Clairvoyant), (o, v) => (bool?)v != o.BaseCreature?.Clairvoyant},
                {nameof(Clingy), (o, v) => (bool?)v != o.BaseCreature?.Clingy},
                {nameof(Displaced), (o, v) => (bool?)v != o.BaseCreature?.Displaced},
                {nameof(Lycanthropy), (o, v) => (string)v != o.BaseCreature?.Lycanthropy},
                {nameof(Mindless), (o, v) => (bool?)v != o.BaseCreature?.Mindless},
                {nameof(NonAnimal), (o, v) => (bool?)v != o.BaseCreature?.NonAnimal},
                {nameof(Reanimation), (o, v) => (bool?)v != o.BaseCreature?.Reanimation}
            };

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}

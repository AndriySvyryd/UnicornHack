using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Data.Creatures;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
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
        private int? _movementDelay;
        private Material? _material;
        private Sex? _sex;
        private int? _size;
        private int? _weight;
        private int? _agility;
        private int? _constitution;
        private int? _intelligence;
        private int? _quickness;
        private int? _strength;
        private int? _willpower;
        private int? _energyRegeneration;
        private int? _regeneration;
        private int? _magicAbsorption;
        private int? _magicDeflection;
        private int? _magicResistance;
        private int? _physicalAbsorption;
        private int? _physicalDeflection;
        private int? _physicalResistance;
        private int? _acidResistance;
        private int? _bleedingResistance;
        private int? _blightResistance;
        private int? _coldResistance;
        private int? _disintegrationResistance;
        private int? _electricityResistance;
        private int? _fireResistance;
        private int? _waterResistance;
        private bool? _reflective;
        private bool? _slimingImmune;
        private bool? _stoningImmune;
        private HeadType? _headType;
        private TorsoType? _torsoType;
        private ExtremityType? _upperExtremeties;
        private ExtremityType? _lowerExtremeties;
        private ExtremityType? _backExtremeties;
        private RespirationType? _respirationType;
        private LocomotionType? _locomotionType;
        private int? _inventorySize;
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
        public Weight GenerationWeight { get; set; }
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

        public int? Agility
        {
            get => _agility ?? BaseCreature?.Agility;
            set => _agility = value;
        }

        public int? Constitution
        {
            get => _constitution ?? BaseCreature?.Constitution;
            set => _constitution = value;
        }

        public int? Intelligence
        {
            get => _intelligence ?? BaseCreature?.Intelligence;
            set => _intelligence = value;
        }

        public int? Quickness
        {
            get => _quickness ?? BaseCreature?.Quickness;
            set => _quickness = value;
        }

        public int? Strength
        {
            get => _strength ?? BaseCreature?.Strength;
            set => _strength = value;
        }

        public int? Willpower
        {
            get => _willpower ?? BaseCreature?.Willpower;
            set => _willpower = value;
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

        public int? MagicAbsorption
        {
            get => _magicAbsorption ?? BaseCreature?.MagicAbsorption;
            set => _magicAbsorption = value;
        }

        public int? MagicDeflection
        {
            get => _magicDeflection ?? BaseCreature?.MagicDeflection;
            set => _magicDeflection = value;
        }

        public int? MagicResistance
        {
            get => _magicResistance ?? BaseCreature?.MagicResistance;
            set => _magicResistance = value;
        }

        public int? PhysicalAbsorption
        {
            get => _physicalAbsorption ?? BaseCreature?.PhysicalAbsorption;
            set => _physicalAbsorption = value;
        }

        public int? PhysicalDeflection
        {
            get => _physicalDeflection ?? BaseCreature?.PhysicalDeflection;
            set => _physicalDeflection = value;
        }

        public int? PhysicalResistance
        {
            get => _physicalResistance ?? BaseCreature?.PhysicalResistance;
            set => _physicalResistance = value;
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

        public int? BlightResistance
        {
            get => _blightResistance ?? BaseCreature?.BlightResistance;
            set => _blightResistance = value;
        }

        public int? ColdResistance
        {
            get => _coldResistance ?? BaseCreature?.ColdResistance;
            set => _coldResistance = value;
        }

        public int? DisintegrationResistance
        {
            get => _disintegrationResistance ?? BaseCreature?.DisintegrationResistance;
            set => _disintegrationResistance = value;
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

        public ExtremityType? UpperExtremeties
        {
            get => _upperExtremeties ?? BaseCreature?.UpperExtremeties;
            set => _upperExtremeties = value;
        }

        public ExtremityType? LowerExtremeties
        {
            get => _lowerExtremeties ?? BaseCreature?.LowerExtremeties;
            set => _lowerExtremeties = value;
        }

        public ExtremityType? BackExtremeties
        {
            get => _backExtremeties ?? BaseCreature?.BackExtremeties;
            set => _backExtremeties = value;
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

        public int? InventorySize
        {
            get => _inventorySize ?? BaseCreature?.InventorySize;
            set => _inventorySize = value;
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
            using (var creatureEntityReference = manager.CreateEntity())
            {
                var creatureEntity = creatureEntityReference.Referenced;

                var ai = manager.CreateComponent<AIComponent>(EntityComponent.AI);
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
                if (XP == 0)
                {
                    being.ExperiencePoints = InitialLevel * 100;
                }
                else
                {
                    being.ExperiencePoints = XP;
                }

                creatureEntity.Being = being;

                var sensor = manager.CreateComponent<SensorComponent>(EntityComponent.Sensor);

                creatureEntity.Sensor = sensor;

                var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
                physical.Material = Material ?? Primitives.Material.Flesh;
                physical.Capacity = InventorySize ?? ItemMovingSystem.DefaultInventorySize;

                creatureEntity.Physical = physical;

                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.LevelId = level.EntityId;
                position.LevelCell = cell;
                position.Heading = (Direction)level.GenerationRandom.Next(7);

                creatureEntity.Position = position;

                using (var appliedEffectEntityReference = manager.CreateEntity())
                {
                    var appliedEffectEntity = appliedEffectEntityReference.Referenced;
                    var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    appliedEffect.DurationTicks = (int)EffectDuration.Infinite;
                    appliedEffect.EffectType = EffectType.ChangeRace;

                    appliedEffectEntity.Effect = appliedEffect;

                    var race = AddRace(appliedEffectEntity);
                    race.Level = InitialLevel;

                    using (var abilityEntityReference = manager.CreateEntity())
                    {
                        var abilityEntity = abilityEntityReference.Referenced;

                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.AddAbility;
                        effect.DurationTicks = (int)EffectDuration.Infinite;
                        effect.ContainingAbilityId = appliedEffectEntity.Id;

                        abilityEntity.Effect = effect;

                        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                        ability.Name = LivingSystem.InnateAbilityName;
                        ability.Activation = ActivationType.Always;

                        abilityEntity.Ability = ability;

                        CreatePropertyEffect(nameof(PositionComponent.MovementDelay), MovementDelay, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(PhysicalComponent.Size), Size, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(PhysicalComponent.Weight), Weight, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.Agility), Agility, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.Constitution), Constitution, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.Intelligence), Intelligence, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.Quickness), Quickness, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.Strength), Strength, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.Willpower), Willpower, abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.EnergyRegeneration), EnergyRegeneration,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.Regeneration), Regeneration, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.MagicAbsorption), MagicAbsorption, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.MagicDeflection), MagicDeflection, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.MagicResistance), MagicResistance, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.PhysicalAbsorption), PhysicalAbsorption,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.PhysicalDeflection), PhysicalDeflection,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.PhysicalResistance), PhysicalResistance,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.AcidResistance), AcidResistance, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.BleedingResistance), BleedingResistance,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.BlightResistance), BlightResistance,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.ColdResistance), ColdResistance, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.DisintegrationResistance), DisintegrationResistance,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.ElectricityResistance), ElectricityResistance,
                            abilityEntity.Id, manager);
                        CreatePropertyEffect(nameof(BeingComponent.FireResistance), FireResistance, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.WaterResistance), WaterResistance, abilityEntity.Id,
                            manager);
                        CreatePropertyEffect(nameof(BeingComponent.UpperExtremeties), (int?)UpperExtremeties,
                            abilityEntity.Id, manager);
                    }

                    appliedEffectEntity.Ability.OwnerEntity = creatureEntity;
                    appliedEffect.AffectedEntityId = creatureEntity.Id;
                }

                return creatureEntity;
            }
        }

        private Func<string, byte, int, float> _weightFunction;

        public float GetWeight(LevelComponent level)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateCreatureWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0);
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
            new Dictionary<string, Func<TCreatureVariant, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(BaseName), (o, v) => v != null},
                {nameof(Species), (o, v) => (Species?)v != (o.BaseCreature?.Species)},
                {nameof(SpeciesClass), (o, v) => (SpeciesClass?)v != (o.BaseCreature?.SpeciesClass)},
                {nameof(MovementDelay), (o, v) => (int?)v != (o.BaseCreature?.MovementDelay)},
                {nameof(Material), (o, v) => (Material?)v != (o.BaseCreature?.Material)},
                {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0},
                {nameof(InitialLevel), (o, v) => (byte)v != 0},
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                {
                    nameof(GenerationWeight),
                    (o, v) => (Weight)v != null && (!(v is DefaultWeight def) || def.Multiplier != 1)
                },
                {nameof(PreviousStageName), (o, v) => v != null},
                {nameof(NextStageName), (o, v) => v != null},
                {nameof(GenerationFlags), (o, v) => (GenerationFlags)v != GenerationFlags.None},
                {nameof(Behavior), (o, v) => (AIBehavior)v != AIBehavior.None},
                {nameof(Noise), (o, v) => (ActorNoiseType)v != (o.BaseCreature?.Noise ?? ActorNoiseType.Silent)},
                {nameof(Sex), (o, v) => (Sex?)v != (o.BaseCreature?.Sex)},
                {nameof(Size), (o, v) => (int?)v != (o.BaseCreature?.Size)},
                {nameof(Weight), (o, v) => (int?)v != (o.BaseCreature?.Weight)},
                {nameof(Agility), (o, v) => (int?)v != (o.BaseCreature?.Agility)},
                {nameof(Constitution), (o, v) => (int?)v != (o.BaseCreature?.Constitution)},
                {nameof(Intelligence), (o, v) => (int?)v != (o.BaseCreature?.Intelligence)},
                {nameof(Quickness), (o, v) => (int?)v != (o.BaseCreature?.Quickness)},
                {nameof(Strength), (o, v) => (int?)v != (o.BaseCreature?.Strength)},
                {nameof(Willpower), (o, v) => (int?)v != (o.BaseCreature?.Willpower)},
                {nameof(EnergyRegeneration), (o, v) => (int?)v != (o.BaseCreature?.EnergyRegeneration)},
                {nameof(Regeneration), (o, v) => (int?)v != (o.BaseCreature?.Regeneration)},
                {nameof(MagicAbsorption), (o, v) => (int?)v != (o.BaseCreature?.MagicAbsorption)},
                {nameof(MagicDeflection), (o, v) => (int?)v != (o.BaseCreature?.MagicDeflection)},
                {nameof(MagicResistance), (o, v) => (int?)v != (o.BaseCreature?.MagicResistance)},
                {nameof(PhysicalAbsorption), (o, v) => (int?)v != (o.BaseCreature?.PhysicalAbsorption)},
                {nameof(PhysicalDeflection), (o, v) => (int?)v != (o.BaseCreature?.PhysicalDeflection)},
                {nameof(PhysicalResistance), (o, v) => (int?)v != (o.BaseCreature?.PhysicalResistance)},
                {nameof(AcidResistance), (o, v) => (int?)v != (o.BaseCreature?.AcidResistance)},
                {nameof(BleedingResistance), (o, v) => (int?)v != (o.BaseCreature?.BleedingResistance)},
                {nameof(BlightResistance), (o, v) => (int?)v != (o.BaseCreature?.BlightResistance)},
                {nameof(ColdResistance), (o, v) => (int?)v != (o.BaseCreature?.ColdResistance)},
                {nameof(DisintegrationResistance), (o, v) => (int?)v != (o.BaseCreature?.DisintegrationResistance)},
                {nameof(ElectricityResistance), (o, v) => (int?)v != (o.BaseCreature?.ElectricityResistance)},
                {nameof(FireResistance), (o, v) => (int?)v != (o.BaseCreature?.FireResistance)},
                {nameof(WaterResistance), (o, v) => (int?)v != (o.BaseCreature?.WaterResistance)},
                {nameof(Reflective), (o, v) => (bool?)v != (o.BaseCreature?.Reflective)},
                {nameof(SlimingImmune), (o, v) => (bool?)v != (o.BaseCreature?.SlimingImmune)},
                {nameof(StoningImmune), (o, v) => (bool?)v != (o.BaseCreature?.StoningImmune)},
                {nameof(HeadType), (o, v) => (HeadType?)v != (o.BaseCreature?.HeadType)},
                {nameof(TorsoType), (o, v) => (TorsoType?)v != (o.BaseCreature?.TorsoType)},
                {nameof(UpperExtremeties), (o, v) => (ExtremityType?)v != (o.BaseCreature?.UpperExtremeties)},
                {nameof(LowerExtremeties), (o, v) => (ExtremityType?)v != (o.BaseCreature?.LowerExtremeties)},
                {nameof(BackExtremeties), (o, v) => (ExtremityType?)v != (o.BaseCreature?.BackExtremeties)},
                {nameof(RespirationType), (o, v) => (RespirationType?)v != (o.BaseCreature?.RespirationType)},
                {nameof(LocomotionType), (o, v) => (LocomotionType?)v != (o.BaseCreature?.LocomotionType)},
                {nameof(InventorySize), (o, v) => (int?)v != (o.BaseCreature?.InventorySize)},
                {nameof(EyeCount), (o, v) => (int?)v != (o.BaseCreature?.EyeCount)},
                {nameof(NoiseLevel), (o, v) => (int?)v != (o.BaseCreature?.NoiseLevel)},
                {nameof(VisibilityLevel), (o, v) => (int?)v != (o.BaseCreature?.VisibilityLevel)},
                {nameof(AcuityLevel), (o, v) => (int?)v != (o.BaseCreature?.AcuityLevel)},
                {nameof(HearingLevel), (o, v) => (int?)v != (o.BaseCreature?.HearingLevel)},
                {nameof(Telepathic), (o, v) => (int?)v != (o.BaseCreature?.Telepathic)},
                {nameof(Infravisible), (o, v) => (bool?)v != (o.BaseCreature?.Infravisible)},
                {nameof(Infravision), (o, v) => (bool?)v != (o.BaseCreature?.Infravision)},
                {nameof(InvisibilityDetection), (o, v) => (bool?)v != (o.BaseCreature?.InvisibilityDetection)},
                {nameof(Clairvoyant), (o, v) => (bool?)v != (o.BaseCreature?.Clairvoyant)},
                {nameof(Clingy), (o, v) => (bool?)v != (o.BaseCreature?.Clingy)},
                {nameof(Displaced), (o, v) => (bool?)v != (o.BaseCreature?.Displaced)},
                {nameof(Lycanthropy), (o, v) => (string)v != (o.BaseCreature?.Lycanthropy)},
                {nameof(Mindless), (o, v) => (bool?)v != (o.BaseCreature?.Mindless)},
                {nameof(NonAnimal), (o, v) => (bool?)v != (o.BaseCreature?.NonAnimal)},
                {nameof(Reanimation), (o, v) => (bool?)v != (o.BaseCreature?.Reanimation)}
            };

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}

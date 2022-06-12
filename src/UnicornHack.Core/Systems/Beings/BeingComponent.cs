using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    [Component(Id = (int)EntityComponent.Being)]
    public class BeingComponent : GameComponent
    {
        private Sex _sex;
        private int _energyPointMaximum;
        private int _energyPoints;
        private int _reservedEnergyPoints;
        private int _hitPointMaximum;
        private int _hitPoints;
        private int _perception;
        private int _might;
        private int _speed;
        private int _focus;
        private int _energyRegeneration;
        private int _regeneration;
        private int _hindrance;
        private int _accuracy;
        private int _evasion;
        private int _deflection;
        private int _armor;
        private int _physicalResistance;
        private int _magicResistance;
        private int _acidResistance;
        private int _bleedingResistance;
        private int _coldResistance;
        private int _electricityResistance;
        private int _fireResistance;
        private int _lightResistance;
        private int _psychicResistance;
        private int _sonicResistance;
        private int _stunResistance;
        private int _toxinResistance;
        private int _voidResistance;
        private int _waterResistance;
        private HeadType _headType;
        private TorsoType _torsoType;
        private ExtremityType _upperExtremities;
        private ExtremityType _lowerExtremities;
        private ExtremityType _backExtremities;
        private int _visibility;
        private int _experiencePoints;
        private float _leftoverHPRegenerationXP;
        private float _leftoverEPRegenerationXP;
        private int? _primaryNaturalWeaponId;
        private int? _secondaryNaturalWeaponId;
        private int _entropyState;
        private IReadOnlyCollection<GameEntity> _abilities;
        private IReadOnlyCollection<GameEntity> _appliedEffects;
        private IReadOnlyCollection<GameEntity> _items;

        public BeingComponent()
            => ComponentId = (int)EntityComponent.Being;

        public Sex Sex
        {
            get => _sex;
            set => SetWithNotify(value, ref _sex);
        }

        [Property]
        public int EnergyPointMaximum
        {
            get => _energyPointMaximum;
            set => SetWithNotify(value, ref _energyPointMaximum);
        }

        public int EnergyPoints
        {
            get => _energyPoints;
            set
            {
                var newEP = value;
                if (newEP < 0)
                {
                    newEP = 0;
                }

                var maxEP = EnergyPointMaximum - ReservedEnergyPoints;
                if (newEP > maxEP)
                {
                    newEP = maxEP;
                }

                SetWithNotify(newEP, ref _energyPoints);
            }
        }

        public int ReservedEnergyPoints
        {
            get => _reservedEnergyPoints;
            set
            {
                var newReservedEP = value;
                if (newReservedEP < 0)
                {
                    newReservedEP = 0;
                }

                if (newReservedEP > EnergyPointMaximum)
                {
                    newReservedEP = EnergyPointMaximum;
                }

                SetWithNotify(newReservedEP, ref _reservedEnergyPoints);

                var maxEP = EnergyPointMaximum - newReservedEP;
                if (EnergyPoints > maxEP)
                {
                    EnergyPoints = maxEP;
                }
            }
        }

        [Property]
        public int HitPointMaximum
        {
            get => _hitPointMaximum;
            set => SetWithNotify(value, ref _hitPointMaximum);
        }

        public int HitPoints
        {
            get => _hitPoints;
            set
            {
                var newHP = value;
                if (newHP > HitPointMaximum)
                {
                    newHP = HitPointMaximum;
                }

                SetWithNotify(newHP, ref _hitPoints);
            }
        }

        public bool IsAlive => HitPoints > 0;

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Perception
        {
            get => _perception;
            set => SetWithNotify(value, ref _perception);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Might
        {
            get => _might;
            set => SetWithNotify(value, ref _might);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Speed
        {
            get => _speed;
            set => SetWithNotify(value, ref _speed);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Focus
        {
            get => _focus;
            set => SetWithNotify(value, ref _focus);
        }

        [Property]
        public int EnergyRegeneration
        {
            get => _energyRegeneration;
            set => SetWithNotify(value, ref _energyRegeneration);
        }

        [Property]
        public int Regeneration
        {
            get => _regeneration;
            set => SetWithNotify(value, ref _regeneration);
        }

        [Property(MinValue = 0)]
        public int Hindrance
        {
            get => _hindrance;
            set => SetWithNotify(value, ref _hindrance);
        }

        [Property]
        public int Accuracy
        {
            get => _accuracy;
            set => SetWithNotify(value, ref _accuracy);
        }

        [Property]
        public int Evasion
        {
            get => _evasion;
            set => SetWithNotify(value, ref _evasion);
        }

        [Property]
        public int Deflection
        {
            get => _deflection;
            set => SetWithNotify(value, ref _deflection);
        }

        [Property(MinValue = 0)]
        public int Armor
        {
            get => _armor;
            set => SetWithNotify(value, ref _armor);
        }

        [Property]
        public int PhysicalResistance
        {
            get => _physicalResistance;
            set => SetWithNotify(value, ref _physicalResistance);
        }

        [Property]
        public int MagicResistance
        {
            get => _magicResistance;
            set => SetWithNotify(value, ref _magicResistance);
        }

        [Property]
        public int AcidResistance
        {
            get => _acidResistance;
            set => SetWithNotify(value, ref _acidResistance);
        }

        [Property]
        public int BleedingResistance
        {
            get => _bleedingResistance;
            set => SetWithNotify(value, ref _bleedingResistance);
        }

        [Property]
        public int ColdResistance
        {
            get => _coldResistance;
            set => SetWithNotify(value, ref _coldResistance);
        }

        [Property]
        public int ElectricityResistance
        {
            get => _electricityResistance;
            set => SetWithNotify(value, ref _electricityResistance);
        }

        [Property]
        public int FireResistance
        {
            get => _fireResistance;
            set => SetWithNotify(value, ref _fireResistance);
        }

        [Property(DefaultValue = 100)]
        public int LightResistance
        {
            get => _lightResistance;
            set => SetWithNotify(value, ref _lightResistance);
        }

        [Property]
        public int PsychicResistance
        {
            get => _psychicResistance;
            set => SetWithNotify(value, ref _psychicResistance);
        }

        [Property]
        public int SonicResistance
        {
            get => _sonicResistance;
            set => SetWithNotify(value, ref _sonicResistance);
        }

        [Property]
        public int StunResistance
        {
            get => _stunResistance;
            set => SetWithNotify(value, ref _stunResistance);
        }

        [Property]
        public int ToxinResistance
        {
            get => _toxinResistance;
            set => SetWithNotify(value, ref _toxinResistance);
        }

        [Property]
        public int VoidResistance
        {
            get => _voidResistance;
            set => SetWithNotify(value, ref _voidResistance);
        }

        [Property(DefaultValue = 100)]
        public int WaterResistance
        {
            get => _waterResistance;
            set => SetWithNotify(value, ref _waterResistance);
        }

        [Property]
        public HeadType HeadType
        {
            get => _headType;
            set => SetWithNotify(value, ref _headType);
        }

        [Property]
        public TorsoType TorsoType
        {
            get => _torsoType;
            set => SetWithNotify(value, ref _torsoType);
        }

        [Property]
        public ExtremityType UpperExtremities
        {
            get => _upperExtremities;
            set => SetWithNotify(value, ref _upperExtremities);
        }

        [Property]
        public ExtremityType LowerExtremities
        {
            get => _lowerExtremities;
            set => SetWithNotify(value, ref _lowerExtremities);
        }

        [Property]
        public ExtremityType BackExtremities
        {
            get => _backExtremities;
            set => SetWithNotify(value, ref _backExtremities);
        }

        [Property(DefaultValue = 100, MinValue = 0)]
        public int Visibility
        {
            get => _visibility;
            set => SetWithNotify(value, ref _visibility);
        }

        public int ExperiencePoints
        {
            get => _experiencePoints;
            set => SetWithNotify(value, ref _experiencePoints);
        }

        public float LeftoverHPRegenerationXP
        {
            get => _leftoverHPRegenerationXP;
            set => SetWithNotify(value, ref _leftoverHPRegenerationXP);
        }

        public float LeftoverEPRegenerationXP
        {
            get => _leftoverEPRegenerationXP;
            set => SetWithNotify(value, ref _leftoverEPRegenerationXP);
        }

        public int? PrimaryNaturalWeaponId
        {
            get => _primaryNaturalWeaponId;
            set => SetWithNotify(value, ref _primaryNaturalWeaponId);
        }

        public int? SecondaryNaturalWeaponId
        {
            get => _secondaryNaturalWeaponId;
            set => SetWithNotify(value, ref _secondaryNaturalWeaponId);
        }

        public int EntropyState
        {
            get => _entropyState;
            set => SetWithNotify(value, ref _entropyState);
        }

        public IReadOnlyCollection<GameEntity> Abilities
        {
            get
            {
                if (_abilities == null)
                {
                    _abilities = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    var abilities = Entity.Item?.Abilities
                                     ?? Entity.Physical?.Abilities
                                     ?? Entity.Sensor?.Abilities;
                    if (abilities != null)
                    {
                        _abilities = abilities;
                    }
                }

                return _abilities;
            }
        }

        public IReadOnlyCollection<GameEntity> AppliedEffects
        {
            get
            {
                if (_appliedEffects == null)
                {
                    _appliedEffects = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    var appliedEffects = Entity.Item?.AppliedEffects
                                          ?? Entity.Physical?.AppliedEffects
                                          ?? Entity.Sensor?.AppliedEffects;
                    if (appliedEffects != null)
                    {
                        _appliedEffects = appliedEffects;
                    }
                }

                return _appliedEffects;
            }
        }

        public IReadOnlyDictionary<int, GameEntity> SlottedAbilities { get; private set; }
        public IReadOnlyCollection<GameEntity> Races { get; private set; }

        public IReadOnlyCollection<GameEntity> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                    var items = Entity.Item?.Items
                            ?? Entity.Physical?.Items
                            ?? Entity.Sensor?.Items;
                    if (items != null)
                    {
                        _items = items;
                    }
                }

                return _items;
            }
        }

        protected override void Clean()
        {
            _sex = default;
            _energyPointMaximum = default;
            _energyPoints = default;
            _hitPointMaximum = default;
            _hitPoints = default;
            _perception = default;
            _might = default;
            _speed = default;
            _focus = default;
            _energyRegeneration = default;
            _regeneration = default;
            _hindrance = default;
            _accuracy = default;
            _evasion = default;
            _deflection = default;
            _armor = default;
            _physicalResistance = default;
            _magicResistance = default;
            _acidResistance = default;
            _bleedingResistance = default;
            _coldResistance = default;
            _electricityResistance = default;
            _fireResistance = default;
            _lightResistance = default;
            _psychicResistance = default;
            _sonicResistance = default;
            _stunResistance = default;
            _toxinResistance = default;
            _voidResistance = default;
            _waterResistance = default;
            _headType = default;
            _torsoType = default;
            _upperExtremities = default;
            _lowerExtremities = default;
            _backExtremities = default;
            _visibility = default;
            _experiencePoints = default;
            _leftoverHPRegenerationXP = default;
            _primaryNaturalWeaponId = default;
            _secondaryNaturalWeaponId = default;
            _entropyState = default;
            ((Dictionary<int, GameEntity>)SlottedAbilities)?.Clear();
            ((HashSet<GameEntity>)Races)?.Clear();
            _abilities = default;
            _appliedEffects = default;
            _items = default;

            base.Clean();
        }
    }
}

using UnicornHack.Generation;
using UnicornHack.Primitives;

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
        private int _agility;
        private int _constitution;
        private int _intelligence;
        private int _quickness;
        private int _strength;
        private int _willpower;
        private int _energyRegeneration;
        private int _regeneration;
        private int _magicAbsorption;
        private int _magicDeflection;
        private int _magicResistance;
        private int _physicalAbsorption;
        private int _physicalDeflection;
        private int _physicalResistance;
        private int _acidResistance;
        private int _bleedingResistance;
        private int _blightResistance;
        private int _coldResistance;
        private int _disintegrationResistance;
        private int _electricityResistance;
        private int _fireResistance;
        private int _waterResistance;
        private HeadType _headType;
        private TorsoType _torsoType;
        private ExtremityType _upperExtremeties;
        private ExtremityType _lowerExtremeties;
        private ExtremityType _backExtremeties;
        private int _experiencePoints;
        private float _leftoverHPRegenerationXP;
        private float _leftoverEPRegenerationXP;
        private int? _primaryNaturalWeaponId;
        private int? _secondaryNaturalWeaponId;
        private int _abilitySlotCount;

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

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Agility
        {
            get => _agility;
            set => SetWithNotify(value, ref _agility);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Constitution
        {
            get => _constitution;
            set => SetWithNotify(value, ref _constitution);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Intelligence
        {
            get => _intelligence;
            set => SetWithNotify(value, ref _intelligence);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Quickness
        {
            get => _quickness;
            set => SetWithNotify(value, ref _quickness);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Strength
        {
            get => _strength;
            set => SetWithNotify(value, ref _strength);
        }

        [Property(DefaultValue = 10, MinValue = 0)]
        public int Willpower
        {
            get => _willpower;
            set => SetWithNotify(value, ref _willpower);
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

        [Property]
        public int MagicAbsorption
        {
            get => _magicAbsorption;
            set => SetWithNotify(value, ref _magicAbsorption);
        }

        [Property]
        public int MagicDeflection
        {
            get => _magicDeflection;
            set => SetWithNotify(value, ref _magicDeflection);
        }

        [Property]
        public int MagicResistance
        {
            get => _magicResistance;
            set => SetWithNotify(value, ref _magicResistance);
        }

        [Property]
        public int PhysicalAbsorption
        {
            get => _physicalAbsorption;
            set => SetWithNotify(value, ref _physicalAbsorption);
        }

        [Property]
        public int PhysicalDeflection
        {
            get => _physicalDeflection;
            set => SetWithNotify(value, ref _physicalDeflection);
        }

        [Property]
        public int PhysicalResistance
        {
            get => _physicalResistance;
            set => SetWithNotify(value, ref _physicalResistance);
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
        public int BlightResistance
        {
            get => _blightResistance;
            set => SetWithNotify(value, ref _blightResistance);
        }

        [Property]
        public int ColdResistance
        {
            get => _coldResistance;
            set => SetWithNotify(value, ref _coldResistance);
        }

        [Property]
        public int DisintegrationResistance
        {
            get => _disintegrationResistance;
            set => SetWithNotify(value, ref _disintegrationResistance);
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
        public ExtremityType UpperExtremeties
        {
            get => _upperExtremeties;
            set => SetWithNotify(value, ref _upperExtremeties);
        }

        [Property]
        public ExtremityType LowerExtremeties
        {
            get => _lowerExtremeties;
            set => SetWithNotify(value, ref _lowerExtremeties);
        }

        [Property]
        public ExtremityType BackExtremeties
        {
            get => _backExtremeties;
            set => SetWithNotify(value, ref _backExtremeties);
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

        [Property(DefaultValue = 8)]
        public int AbilitySlotCount
        {
            get => _abilitySlotCount;
            set => SetWithNotify(value, ref _abilitySlotCount);
        }

        // Unmapped properties
        public bool IsAlive => HitPoints > 0;

        protected override void Clean()
        {
            _sex = default;
            _energyPointMaximum = default;
            _energyPoints = default;
            _hitPointMaximum = default;
            _hitPoints = default;
            _agility = default;
            _constitution = default;
            _intelligence = default;
            _quickness = default;
            _strength = default;
            _willpower = default;
            _energyRegeneration = default;
            _regeneration = default;
            _magicAbsorption = default;
            _magicDeflection = default;
            _magicResistance = default;
            _physicalAbsorption = default;
            _physicalDeflection = default;
            _physicalResistance = default;
            _acidResistance = default;
            _bleedingResistance = default;
            _blightResistance = default;
            _coldResistance = default;
            _disintegrationResistance = default;
            _electricityResistance = default;
            _fireResistance = default;
            _waterResistance = default;
            _headType = default;
            _torsoType = default;
            _upperExtremeties = default;
            _lowerExtremeties = default;
            _backExtremeties = default;
            _experiencePoints = default;
            _leftoverHPRegenerationXP = default;
            _primaryNaturalWeaponId = default;
            _secondaryNaturalWeaponId = default;
            _abilitySlotCount = default;

            base.Clean();
        }
    }
}

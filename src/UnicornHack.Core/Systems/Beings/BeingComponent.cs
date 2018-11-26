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
        private float _leftoverRegenerationXP;
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

        [Property(IsCalculated = true)]
        public int EnergyPointMaximum
        {
            get => _energyPointMaximum;
            set => SetWithNotify(value, ref _energyPointMaximum);
        }

        public int EnergyPoints
        {
            get => _energyPoints;
            set => SetWithNotify(value, ref _energyPoints);
        }

        [Property(IsCalculated = true)]
        public int HitPointMaximum
        {
            get => _hitPointMaximum;
            set => SetWithNotify(value, ref _hitPointMaximum);
        }

        public int HitPoints
        {
            get => _hitPoints;
            set => SetWithNotify(value, ref _hitPoints);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Agility
        {
            get => _agility;
            set => SetWithNotify(value, ref _agility);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Constitution
        {
            get => _constitution;
            set => SetWithNotify(value, ref _constitution);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Intelligence
        {
            get => _intelligence;
            set => SetWithNotify(value, ref _intelligence);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Quickness
        {
            get => _quickness;
            set => SetWithNotify(value, ref _quickness);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Strength
        {
            get => _strength;
            set => SetWithNotify(value, ref _strength);
        }

        [Property(IsCalculated = true, DefaultValue = 10, MinValue = 0)]
        public int Willpower
        {
            get => _willpower;
            set => SetWithNotify(value, ref _willpower);
        }

        [Property(IsCalculated = true)]
        public int EnergyRegeneration
        {
            get => _energyRegeneration;
            set => SetWithNotify(value, ref _energyRegeneration);
        }

        [Property(IsCalculated = true)]
        public int Regeneration
        {
            get => _regeneration;
            set => SetWithNotify(value, ref _regeneration);
        }

        [Property(IsCalculated = true)]
        public int MagicAbsorption
        {
            get => _magicAbsorption;
            set => SetWithNotify(value, ref _magicAbsorption);
        }

        [Property(IsCalculated = true)]
        public int MagicDeflection
        {
            get => _magicDeflection;
            set => SetWithNotify(value, ref _magicDeflection);
        }

        [Property(IsCalculated = true)]
        public int MagicResistance
        {
            get => _magicResistance;
            set => SetWithNotify(value, ref _magicResistance);
        }

        [Property(IsCalculated = true)]
        public int PhysicalAbsorption
        {
            get => _physicalAbsorption;
            set => SetWithNotify(value, ref _physicalAbsorption);
        }

        [Property(IsCalculated = true)]
        public int PhysicalDeflection
        {
            get => _physicalDeflection;
            set => SetWithNotify(value, ref _physicalDeflection);
        }

        [Property(IsCalculated = true)]
        public int PhysicalResistance
        {
            get => _physicalResistance;
            set => SetWithNotify(value, ref _physicalResistance);
        }

        [Property(IsCalculated = true)]
        public int AcidResistance
        {
            get => _acidResistance;
            set => SetWithNotify(value, ref _acidResistance);
        }

        [Property(IsCalculated = true)]
        public int BleedingResistance
        {
            get => _bleedingResistance;
            set => SetWithNotify(value, ref _bleedingResistance);
        }

        [Property(IsCalculated = true)]
        public int BlightResistance
        {
            get => _blightResistance;
            set => SetWithNotify(value, ref _blightResistance);
        }

        [Property(IsCalculated = true)]
        public int ColdResistance
        {
            get => _coldResistance;
            set => SetWithNotify(value, ref _coldResistance);
        }

        [Property(IsCalculated = true)]
        public int DisintegrationResistance
        {
            get => _disintegrationResistance;
            set => SetWithNotify(value, ref _disintegrationResistance);
        }

        [Property(IsCalculated = true)]
        public int ElectricityResistance
        {
            get => _electricityResistance;
            set => SetWithNotify(value, ref _electricityResistance);
        }

        [Property(IsCalculated = true)]
        public int FireResistance
        {
            get => _fireResistance;
            set => SetWithNotify(value, ref _fireResistance);
        }

        [Property(IsCalculated = true, DefaultValue = 100)]
        public int WaterResistance
        {
            get => _waterResistance;
            set => SetWithNotify(value, ref _waterResistance);
        }

        [Property(IsCalculated = true)]
        public HeadType HeadType
        {
            get => _headType;
            set => SetWithNotify(value, ref _headType);
        }

        [Property(IsCalculated = true)]
        public TorsoType TorsoType
        {
            get => _torsoType;
            set => SetWithNotify(value, ref _torsoType);
        }

        [Property(IsCalculated = true)]
        public ExtremityType UpperExtremeties
        {
            get => _upperExtremeties;
            set => SetWithNotify(value, ref _upperExtremeties);
        }

        [Property(IsCalculated = true)]
        public ExtremityType LowerExtremeties
        {
            get => _lowerExtremeties;
            set => SetWithNotify(value, ref _lowerExtremeties);
        }

        [Property(IsCalculated = true)]
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

        public float LeftoverRegenerationXP
        {
            get => _leftoverRegenerationXP;
            set => SetWithNotify(value, ref _leftoverRegenerationXP);
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

        [Property(IsCalculated = true, DefaultValue = 8)]
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
            _leftoverRegenerationXP = default;
            _primaryNaturalWeaponId = default;
            _secondaryNaturalWeaponId = default;
            _abilitySlotCount = default;

            base.Clean();
        }
    }
}

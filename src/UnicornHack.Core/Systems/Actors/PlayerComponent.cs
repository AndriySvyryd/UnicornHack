using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Actors
{
    [Component(Id = (int)EntityComponent.Player)]
    public class PlayerComponent : GameComponent, IKeepAliveComponent
    {
        private string _properName;
        private PlayerAction? _nextAction;
        private int? _nextActionTarget;
        private int? _nextActionTarget2;
        private bool _queuedAction;
        private int? _nextActionTick;
        private int _maxLevel;
        private int _nextLevelXP;
        private int _skillPoints;
        private int _traitPoints;
        private int _mutationPoints;
        private int _oneHanded;
        private int _twoHanded;
        private int _dualWielding;
        private int _fistWeapons;
        private int _shortWeapons;
        private int _mediumWeapons;
        private int _longWeapons;
        private int _thrownWeapons;
        private int _slingshots;
        private int _bows;
        private int _crossbows;
        private int _armorless;
        private int _lightArmor;
        private int _heavyArmor;
        private int _stealth;
        private int _assessination;
        private int _meleeMagicWeapons;
        private int _rangedMagicWeapons;
        private int _fireSourcery;
        private int _airSourcery;
        private int _waterSourcery;
        private int _earthSourcery;
        private int _lifeSourcery;
        private int _spiritSourcery;
        private int _evocation;
        private int _conjuration;
        private int _transmutation;
        private int _enhancement;
        private int _malediction;
        private int _illusion;
        private int _artifice;
        private int _leadership;

        public PlayerComponent()
            => ComponentId = (int)EntityComponent.Player;

        public ObservableHashSet<LogEntry> LogEntries { get; } = new ObservableHashSet<LogEntry>();
        public ICollection<PlayerCommand> CommandHistory { get; } = new ObservableHashSet<PlayerCommand>();

        public string ProperName
        {
            get => _properName;
            set => SetWithNotify(value, ref _properName);
        }

        public PlayerAction? NextAction
        {
            get => _nextAction;
            set => SetWithNotify(value, ref _nextAction);
        }

        public int? NextActionTarget
        {
            get => _nextActionTarget;
            set => SetWithNotify(value, ref _nextActionTarget);
        }

        public int? NextActionTarget2
        {
            get => _nextActionTarget2;
            set => SetWithNotify(value, ref _nextActionTarget2);
        }

        public bool QueuedAction
        {
            get => _queuedAction;
            set => SetWithNotify(value, ref _queuedAction);
        }

        public int? NextActionTick
        {
            get => _nextActionTick;
            set => SetWithNotify(value, ref _nextActionTick);
        }

        /// <summary>
        ///     The maximum level ever reached
        /// </summary>
        public int MaxLevel
        {
            get => _maxLevel;
            set => SetWithNotify(value, ref _maxLevel);
        }

        public int NextLevelXP
        {
            get => _nextLevelXP;
            set => SetWithNotify(value, ref _nextLevelXP);
        }

        public int SkillPoints
        {
            get => _skillPoints;
            set => SetWithNotify(value, ref _skillPoints);
        }

        public int TraitPoints
        {
            get => _traitPoints;
            set => SetWithNotify(value, ref _traitPoints);
        }

        public int MutationPoints
        {
            get => _mutationPoints;
            set => SetWithNotify(value, ref _mutationPoints);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int OneHanded
        {
            get => _oneHanded;
            set => SetWithNotify(value, ref _oneHanded);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int TwoHanded
        {
            get => _twoHanded;
            set => SetWithNotify(value, ref _twoHanded);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int DualWielding
        {
            get => _dualWielding;
            set => SetWithNotify(value, ref _dualWielding);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int FistWeapons
        {
            get => _fistWeapons;
            set => SetWithNotify(value, ref _fistWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int ShortWeapons
        {
            get => _shortWeapons;
            set => SetWithNotify(value, ref _shortWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int MediumWeapons
        {
            get => _mediumWeapons;
            set => SetWithNotify(value, ref _mediumWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int LongWeapons
        {
            get => _longWeapons;
            set => SetWithNotify(value, ref _longWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int ThrownWeapons
        {
            get => _thrownWeapons;
            set => SetWithNotify(value, ref _thrownWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Slingshots
        {
            get => _slingshots;
            set => SetWithNotify(value, ref _slingshots);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Bows
        {
            get => _bows;
            set => SetWithNotify(value, ref _bows);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Crossbows
        {
            get => _crossbows;
            set => SetWithNotify(value, ref _crossbows);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Armorless
        {
            get => _armorless;
            set => SetWithNotify(value, ref _armorless);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int LightArmor
        {
            get => _lightArmor;
            set => SetWithNotify(value, ref _lightArmor);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int HeavyArmor
        {
            get => _heavyArmor;
            set => SetWithNotify(value, ref _heavyArmor);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Stealth
        {
            get => _stealth;
            set => SetWithNotify(value, ref _stealth);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Assessination
        {
            get => _assessination;
            set => SetWithNotify(value, ref _assessination);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int MeleeMagicWeapons
        {
            get => _meleeMagicWeapons;
            set => SetWithNotify(value, ref _meleeMagicWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int RangedMagicWeapons
        {
            get => _rangedMagicWeapons;
            set => SetWithNotify(value, ref _rangedMagicWeapons);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int FireSourcery
        {
            get => _fireSourcery;
            set => SetWithNotify(value, ref _fireSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int AirSourcery
        {
            get => _airSourcery;
            set => SetWithNotify(value, ref _airSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int WaterSourcery
        {
            get => _waterSourcery;
            set => SetWithNotify(value, ref _waterSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int EarthSourcery
        {
            get => _earthSourcery;
            set => SetWithNotify(value, ref _earthSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int LifeSourcery
        {
            get => _lifeSourcery;
            set => SetWithNotify(value, ref _lifeSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int SpiritSourcery
        {
            get => _spiritSourcery;
            set => SetWithNotify(value, ref _spiritSourcery);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Evocation
        {
            get => _evocation;
            set => SetWithNotify(value, ref _evocation);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Conjuration
        {
            get => _conjuration;
            set => SetWithNotify(value, ref _conjuration);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Transmutation
        {
            get => _transmutation;
            set => SetWithNotify(value, ref _transmutation);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Enhancement
        {
            get => _enhancement;
            set => SetWithNotify(value, ref _enhancement);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Malediction
        {
            get => _malediction;
            set => SetWithNotify(value, ref _malediction);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Illusion
        {
            get => _illusion;
            set => SetWithNotify(value, ref _illusion);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Artifice
        {
            get => _artifice;
            set => SetWithNotify(value, ref _artifice);
        }

        [Property(MinValue = 0, MaxValue = 3)]
        public int Leadership
        {
            get => _leadership;
            set => SetWithNotify(value, ref _leadership);
        }

        protected override void Clean()
        {
            _properName = default;
            _nextAction = default;
            _nextActionTarget = default;
            _nextActionTarget2 = default;
            _queuedAction = default;
            _nextActionTick = default;
            _maxLevel = default;
            _nextLevelXP = default;
            _skillPoints = default;
            _traitPoints = default;
            _mutationPoints = default;
            _oneHanded = default;
            _twoHanded = default;
            _dualWielding = default;
            _fistWeapons = default;
            _shortWeapons = default;
            _mediumWeapons = default;
            _longWeapons = default;
            _thrownWeapons = default;
            _slingshots = default;
            _bows = default;
            _crossbows = default;
            _armorless = default;
            _lightArmor = default;
            _heavyArmor = default;
            _stealth = default;
            _assessination = default;
            _meleeMagicWeapons = default;
            _rangedMagicWeapons = default;
            _fireSourcery = default;
            _airSourcery = default;
            _waterSourcery = default;
            _earthSourcery = default;
            _lifeSourcery = default;
            _spiritSourcery = default;
            _evocation = default;
            _conjuration = default;
            _transmutation = default;
            _enhancement = default;
            _malediction = default;
            _illusion = default;
            _artifice = default;
            _leadership = default;

            base.Clean();
        }
    }
}

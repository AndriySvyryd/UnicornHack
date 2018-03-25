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
        private int? _defaultAttackId;
        private int _maxXPLevel;
        private int _unspentSkillPoints;
        private int _oneHanded;
        private int _twoHanded;
        private int _dualWielding;
        private int _fistWeapons;
        private int _shortWeapons;
        private int _mediumWeapons;
        private int _longWeapons;
        private int _thrown;
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
        private int _survival;
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

        public int? DefaultAttackId
        {
            get => _defaultAttackId;
            set => SetWithNotify(value, ref _defaultAttackId);
        }

        /// <summary>
        ///     The maximum level ever reached by an associated race
        /// </summary>
        public int MaxXPLevel
        {
            get => _maxXPLevel;
            set => SetWithNotify(value, ref _maxXPLevel);
        }

        // Skills
        public int UnspentSkillPoints
        {
            get => _unspentSkillPoints;
            set => SetWithNotify(value, ref _unspentSkillPoints);
        }

        public int OneHanded
        {
            get => _oneHanded;
            set => SetWithNotify(value, ref _oneHanded);
        }

        public int TwoHanded
        {
            get => _twoHanded;
            set => SetWithNotify(value, ref _twoHanded);
        }

        public int DualWielding
        {
            get => _dualWielding;
            set => SetWithNotify(value, ref _dualWielding);
        }

        public int FistWeapons
        {
            get => _fistWeapons;
            set => SetWithNotify(value, ref _fistWeapons);
        }

        public int ShortWeapons
        {
            get => _shortWeapons;
            set => SetWithNotify(value, ref _shortWeapons);
        }

        public int MediumWeapons
        {
            get => _mediumWeapons;
            set => SetWithNotify(value, ref _mediumWeapons);
        }

        public int LongWeapons
        {
            get => _longWeapons;
            set => SetWithNotify(value, ref _longWeapons);
        }

        public int Thrown
        {
            get => _thrown;
            set => SetWithNotify(value, ref _thrown);
        }

        public int Slingshots
        {
            get => _slingshots;
            set => SetWithNotify(value, ref _slingshots);
        }

        public int Bows
        {
            get => _bows;
            set => SetWithNotify(value, ref _bows);
        }

        public int Crossbows
        {
            get => _crossbows;
            set => SetWithNotify(value, ref _crossbows);
        }

        public int Armorless
        {
            get => _armorless;
            set => SetWithNotify(value, ref _armorless);
        }

        public int LightArmor
        {
            get => _lightArmor;
            set => SetWithNotify(value, ref _lightArmor);
        }

        public int HeavyArmor
        {
            get => _heavyArmor;
            set => SetWithNotify(value, ref _heavyArmor);
        }

        public int Stealth
        {
            get => _stealth;
            set => SetWithNotify(value, ref _stealth);
        }

        public int Assessination
        {
            get => _assessination;
            set => SetWithNotify(value, ref _assessination);
        }

        public int MeleeMagicWeapons
        {
            get => _meleeMagicWeapons;
            set => SetWithNotify(value, ref _meleeMagicWeapons);
        }

        public int RangedMagicWeapons
        {
            get => _rangedMagicWeapons;
            set => SetWithNotify(value, ref _rangedMagicWeapons);
        }

        public int FireSourcery
        {
            get => _fireSourcery;
            set => SetWithNotify(value, ref _fireSourcery);
        }

        public int AirSourcery
        {
            get => _airSourcery;
            set => SetWithNotify(value, ref _airSourcery);
        }

        public int WaterSourcery
        {
            get => _waterSourcery;
            set => SetWithNotify(value, ref _waterSourcery);
        }

        public int EarthSourcery
        {
            get => _earthSourcery;
            set => SetWithNotify(value, ref _earthSourcery);
        }

        public int LifeSourcery
        {
            get => _lifeSourcery;
            set => SetWithNotify(value, ref _lifeSourcery);
        }

        public int SpiritSourcery
        {
            get => _spiritSourcery;
            set => SetWithNotify(value, ref _spiritSourcery);
        }

        public int Evocation
        {
            get => _evocation;
            set => SetWithNotify(value, ref _evocation);
        }

        public int Conjuration
        {
            get => _conjuration;
            set => SetWithNotify(value, ref _conjuration);
        }

        public int Transmutation
        {
            get => _transmutation;
            set => SetWithNotify(value, ref _transmutation);
        }

        public int Enhancement
        {
            get => _enhancement;
            set => SetWithNotify(value, ref _enhancement);
        }

        public int Malediction
        {
            get => _malediction;
            set => SetWithNotify(value, ref _malediction);
        }

        public int Illusion
        {
            get => _illusion;
            set => SetWithNotify(value, ref _illusion);
        }

        public int Survival
        {
            get => _survival;
            set => SetWithNotify(value, ref _survival);
        }

        public int Artifice
        {
            get => _artifice;
            set => SetWithNotify(value, ref _artifice);
        }

        public int Leadership
        {
            get => _leadership;
            set => SetWithNotify(value, ref _leadership);
        }
    }
}

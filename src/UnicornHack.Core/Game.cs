using System.Collections.Generic;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Services;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack
{
    public class Game : NotificationEntity
    {
        private int _id;
        private int _currentTick;
        private int _nextEntityId;
        private int _nextLogId;
        private int _nextCommandId;
        private uint? _initialSeed;
        private SimpleRandom _random;
        private int? _actingPlayerId;
        private GameEntity _actingPlayer;

        public int Id
        {
            get => _id;
            // ReSharper disable once UnusedMember.Local
            private set => SetWithNotify(value, ref _id);
        }

        public int CurrentTick
        {
            get => _currentTick;
            set => SetWithNotify(value, ref _currentTick);
        }

        public int NextEntityId
        {
            get => _nextEntityId;
            set => SetWithNotify(value, ref _nextEntityId);
        }

        public int NextLogId
        {
            get => _nextLogId;
            set => SetWithNotify(value, ref _nextLogId);
        }

        public int NextCommandId
        {
            get => _nextCommandId;
            set => SetWithNotify(value, ref _nextCommandId);
        }

        public uint? InitialSeed
        {
            get => _initialSeed;
            set => SetWithNotify(value, ref _initialSeed);
        }

        public SimpleRandom Random
        {
            get => _random;
            set => SetWithNotify(value, ref _random);
        }

        public int? ActingPlayerId
        {
            get => _actingPlayerId;
            set => SetWithNotify(value, ref _actingPlayerId);
        }

        public GameEntity ActingPlayer
        {
            get => _actingPlayer;
            set => SetWithNotify(value, ref _actingPlayer);
        }

        public ICollection<GameBranch> Branches { get; set; } = new ObservableHashSet<GameBranch>();

        // Unmapped properties
        public GameServices Services { get; set; }
        public GameManager Manager { get; set; }
        public IRepository Repository { get; set; }

        public GameBranch GetBranch(string branchName)
            => Branches.FirstOrDefault(b => b.Name == branchName) ??
               Repository.Find<GameBranch>(Id, branchName);

        public GameEntity LoadLevel(int levelId)
        {
            var levelEntity = Manager.FindEntity(levelId);
            if (!LevelGenerator.EnsureGenerated(levelEntity.Level))
            {
                var levelsToLoad = levelEntity.Level.Connections.Values
                    .Select(c => c.Connection.TargetLevelId)
                    .Where(id => Manager.FindEntity(id) == null)
                    .ToList();
                if (levelsToLoad.Count > 0)
                {
                    Repository.LoadLevels(levelsToLoad);
                }
            }

            return levelEntity;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Game otherGame))
            {
                return false;
            }

            if (ReferenceEquals(this, otherGame))
            {
                return true;
            }

            return Id == otherGame.Id;
        }

        public override int GetHashCode() => ~Id;
    }
}

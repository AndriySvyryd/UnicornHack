using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    [Component(Id = (int)EntityComponent.Position)]
    public class PositionComponent : GameComponent, IKeepAliveComponent
    {
        private GameEntity _level;
        private int _levelId;
        private byte _levelX;
        private byte _levelY;
        private Direction? _heading;
        private int _movementDelay;
        private int _turningDelay;

        public PositionComponent()
            => ComponentId = (int)EntityComponent.Position;

        public int LevelId
        {
            get => _levelId;
            set
            {
                _level = null;
                SetWithNotify(value, ref _levelId);
            }
        }

        public byte LevelX
        {
            get => _levelX;
            set => SetWithNotify(value, ref _levelX);
        }

        public byte LevelY
        {
            get => _levelY;
            set => SetWithNotify(value, ref _levelY);
        }

        public Direction? Heading
        {
            get => _heading;
            set => SetWithNotify(value, ref _heading);
        }

        [Property(MinValue = 0)]
        public int MovementDelay
        {
            get => _movementDelay;
            set => SetWithNotify(value, ref _movementDelay);
        }

        [Property(MinValue = 0)]
        public int TurningDelay
        {
            get => _turningDelay;
            set => SetWithNotify(value, ref _turningDelay);
        }

        // Unmapped properties
        public GameEntity LevelEntity
        {
            get => _level ?? (_level = Entity.Manager.FindEntity(_levelId));
            set
            {
                LevelId = value.Id;
                _level = value;
            }
        }

        public Point LevelCell
        {
            get => new Point(LevelX, LevelY);
            set
            {
                var levelXSet = NotifyChanging(value.X, ref _levelX, nameof(LevelX), out var oldLevelX);
                var levelYSet = NotifyChanging(value.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

                var changes = new List<IPropertyValueChange>();
                if (levelXSet)
                {
                    NotifyChanged(nameof(LevelX));
                    changes.Add(new PropertyValueChange<byte>(this, nameof(LevelX), oldLevelX, value.X));
                }

                if (levelYSet)
                {
                    NotifyChanged(nameof(LevelY));
                    changes.Add(new PropertyValueChange<byte>(this, nameof(LevelY), oldLevelY, value.Y));
                }

                if (changes.Count > 0)
                {
                    Entity?.HandlePropertyValuesChanged(changes);
                }
            }
        }

        /// <summary>
        ///     Sets <see cref="LevelId" /> and <see cref="LevelCell" /> atomically.
        /// </summary>
        public void SetLevelPosition(int levelId, Point levelCell)
        {
            var levelIdSet = NotifyChanging(levelId, ref _levelId, nameof(LevelId), out var oldLevelId);
            var levelXSet = NotifyChanging(levelCell.X, ref _levelX, nameof(LevelX), out var oldLevelX);
            var levelYSet = NotifyChanging(levelCell.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

            var changes = new List<IPropertyValueChange>();
            if (levelIdSet)
            {
                _level = null;
                NotifyChanged(nameof(LevelId));
                changes.Add(new PropertyValueChange<int>(this, nameof(LevelId), oldLevelId, levelId));
            }

            if (levelXSet)
            {
                NotifyChanged(nameof(LevelX));
                changes.Add(new PropertyValueChange<byte>(this, nameof(LevelX), oldLevelX, levelCell.X));
            }

            if (levelYSet)
            {
                NotifyChanged(nameof(LevelY));
                changes.Add(new PropertyValueChange<byte>(this, nameof(LevelY), oldLevelY, levelCell.Y));
            }

            if (changes.Count > 0)
            {
                Entity?.HandlePropertyValuesChanged(changes);
            }
        }

        protected override void Clean()
        {
            _level = default;
            _levelId = default;
            _levelX = default;
            _levelY = default;
            _heading = default;
            _movementDelay = default;
            _turningDelay = default;

            base.Clean();
        }
    }
}

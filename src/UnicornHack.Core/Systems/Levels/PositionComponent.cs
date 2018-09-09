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

        // TODO: Separate delay for turning
        [Property(MinValue = 0, IsCalculated = true)]
        public int MovementDelay
        {
            get => _movementDelay;
            set => SetWithNotify(value, ref _movementDelay);
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
                var levelXSet = StartSettingWithNotify(value.X, ref _levelX, nameof(LevelX), out var oldLevelX);
                var levelYSet = StartSettingWithNotify(value.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

                if (levelXSet)
                {
                    FinishSetttingWithNotify(oldLevelX, value.X, nameof(LevelX));
                }
                if (levelYSet)
                {
                    FinishSetttingWithNotify(oldLevelY, value.Y, nameof(LevelY));
                }
            }
        }

        /// <summary>
        ///     Sets <see cref="LevelId"/> and <see cref="LevelCell"/> atomically.
        /// </summary>
        public void SetLevelPosition(int levelId, Point levelCell)
        {
            var levelIdSet = StartSettingWithNotify(levelId, ref _levelId, nameof(LevelId), out var oldLevelId);
            var levelXSet = StartSettingWithNotify(levelCell.X, ref _levelX, nameof(LevelX), out var oldLevelX);
            var levelYSet = StartSettingWithNotify(levelCell.Y, ref _levelY, nameof(LevelY), out var oldLevelY);

            if (levelIdSet)
            {
                FinishSetttingWithNotify(oldLevelId, levelId, nameof(LevelId));
            }
            if (levelXSet)
            {
                FinishSetttingWithNotify(oldLevelX, levelCell.X, nameof(LevelX));
            }
            if (levelYSet)
            {
                FinishSetttingWithNotify(oldLevelY, levelCell.Y, nameof(LevelY));
            }
        }
    }
}

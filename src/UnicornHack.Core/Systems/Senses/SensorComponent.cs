using System.Buffers;
using System.Collections.Generic;
using UnicornHack.Generation;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Senses
{
    [Component(Id = (int)EntityComponent.Sensor)]
    public class SensorComponent : GameComponent
    {
        private int _primaryFOVQuadrants;
        private int _totalFOVQuadrants;
        private int _primaryVisionRange;
        private int _secondaryVisionRange;
        private bool _infravision;
        private bool _invisibilityDetection;
        private IReadOnlyCollection<GameEntity> _abilities;
        private IReadOnlyCollection<GameEntity> _appliedEffects;
        private IReadOnlyCollection<GameEntity> _items;

        public SensorComponent()
            => ComponentId = (int)EntityComponent.Sensor;

        [Property(DefaultValue = 1)]
        public int PrimaryFOVQuadrants
        {
            get => _primaryFOVQuadrants;
            set => SetWithNotify(value, ref _primaryFOVQuadrants);
        }

        [Property(DefaultValue = 2)]
        public int TotalFOVQuadrants
        {
            get => _totalFOVQuadrants;
            set => SetWithNotify(value, ref _totalFOVQuadrants);
        }

        [Property(DefaultValue = 16)]
        public int PrimaryVisionRange
        {
            get => _primaryVisionRange;
            set => SetWithNotify(value, ref _primaryVisionRange);
        }

        [Property(DefaultValue = 8)]
        public int SecondaryVisionRange
        {
            get => _secondaryVisionRange;
            set => SetWithNotify(value, ref _secondaryVisionRange);
        }

        [Property]
        public bool Infravision
        {
            get => _infravision;
            set => SetWithNotify(value, ref _infravision);
        }

        [Property]
        public bool InvisibilityDetection
        {
            get => _invisibilityDetection;
            set => SetWithNotify(value, ref _invisibilityDetection);
        }

        public byte[] VisibleTerrain { get; set; }
        public bool VisibleTerrainIsCurrent { get; set; }

        public IReadOnlyCollection<GameEntity> Abilities
        {
            get
            {
                if (_abilities == null)
                {
                    var abilities = Entity.Physical?.Abilities
                                     ?? Entity.Item?.Abilities
                                     ?? Entity.Being?.Abilities;
                    _abilities = abilities ?? new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
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
                    var appliedEffects = Entity.Physical?.AppliedEffects
                                         ?? Entity.Item?.AppliedEffects
                                         ?? Entity.Being?.AppliedEffects;
                    _appliedEffects = appliedEffects ?? new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                }

                return _appliedEffects;
            }
        }

        public IReadOnlyCollection<GameEntity> Items
        {
            get
            {
                if (_items == null)
                {
                    var items = Entity.Physical?.Items
                                ?? Entity.Item?.Items
                                ?? Entity.Being?.Items;
                    _items = items ?? new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                }

                return _items;
            }
        }

        protected override void Clean()
        {
            if (VisibleTerrain != null)
            {
                ArrayPool<byte>.Shared.Return(VisibleTerrain);
            }

            _primaryFOVQuadrants = default;
            _totalFOVQuadrants = default;
            _primaryVisionRange = default;
            _secondaryVisionRange = default;
            _infravision = default;
            _invisibilityDetection = default;
            VisibleTerrain = default;
            VisibleTerrainIsCurrent = default;
            _abilities = default;
            _appliedEffects = default;
            _items = default;

            base.Clean();
        }
    }
}

using System.Buffers;
using UnicornHack.Generation;

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

        // Untracked properties
        public byte[] VisibleTerrain { get; set; }
        public bool VisibleTerrainIsCurrent { get; set; }

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

            base.Clean();
        }
    }
}

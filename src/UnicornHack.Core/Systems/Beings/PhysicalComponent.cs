using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Beings
{
    [Component(Id = (int)EntityComponent.Physical)]
    public class PhysicalComponent : GameComponent
    {
        private Material _material;
        private int _weight;
        private int _size;
        private int? _capacity;
        private bool _infravisible;

        public PhysicalComponent()
            => ComponentId = (int)EntityComponent.Physical;

        public Material Material
        {
            get => _material;
            set => SetWithNotify(value, ref _material);
        }

        // units of 100 grams
        [Property(MinValue = 0, DefaultValue = 1, IsCalculated = true)]
        public int Weight
        {
            get => _weight;
            set => SetWithNotify(value, ref _weight);
        }

        // Largest dimension in feet
        [Property(MinValue = 0, DefaultValue = 1, IsCalculated = true)]
        public int Size
        {
            get => _size;
            set => SetWithNotify(value, ref _size);
        }

        public int? Capacity
        {
            get => _capacity;
            set => SetWithNotify(value, ref _capacity);
        }

        // TODO: Perf: Store number of items contained

        [Property(IsCalculated = true)]
        public bool Infravisible
        {
            get => _infravisible;
            set => SetWithNotify(value, ref _infravisible);
        }
    }
}

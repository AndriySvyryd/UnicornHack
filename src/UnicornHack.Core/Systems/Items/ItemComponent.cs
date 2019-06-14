using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Items
{
    [Component(Id = (int)EntityComponent.Item)]
    public class ItemComponent : GameComponent
    {
        private int? _containerId;
        private ItemType _type;
        private string _templateName;
        private string _name;
        private SizeCategory _equipableSizes;
        private EquipmentSlot _equipableSlots;
        private EquipmentSlot _equippedSlot;
        private int _maxStackSize;
        private int? _count;

        public ItemComponent()
            => ComponentId = (int)EntityComponent.Item;

        public int? ContainerId
        {
            get => _containerId;
            set => SetWithNotify(value, ref _containerId);
        }

        public ItemType Type
        {
            get => _type;
            set => SetWithNotify(value, ref _type);
        }

        public string TemplateName
        {
            get => _templateName;
            set => SetWithNotify(value, ref _templateName);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public SizeCategory EquipableSizes
        {
            get => _equipableSizes;
            set => SetWithNotify(value, ref _equipableSizes);
        }

        public EquipmentSlot EquipableSlots
        {
            get => _equipableSlots;
            set => SetWithNotify(value, ref _equipableSlots);
        }

        public EquipmentSlot EquippedSlot
        {
            get => _equippedSlot;
            set => SetWithNotify(value, ref _equippedSlot);
        }

        public int MaxStackSize
        {
            get => _maxStackSize;
            set => SetWithNotify(value, ref _maxStackSize);
        }

        public int? Count
        {
            get => _count;
            set => SetWithNotify(value, ref _count);
        }

        public int GetQuantity(GameManager manager)
            => Count ?? (Entity.Physical.Capacity == null
                   ? 1
                   : manager.EntityItemsToContainerRelationship[EntityId].Count() + 1);

        protected override void Clean()
        {
            _containerId = default;
            _type = default;
            _templateName = default;
            _name = default;
            _equipableSizes = default;
            _equipableSlots = default;
            _equippedSlot = default;
            _maxStackSize = default;
            _count = default;

            base.Clean();
        }
    }
}

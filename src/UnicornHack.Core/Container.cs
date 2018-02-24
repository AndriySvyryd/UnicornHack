using System.Collections.Generic;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Container : Item, IItemLocation
    {
        public Container()
        {
        }

        public Container(Game game) : base(game)
        {
        }

        public virtual short Capacity { get; set; }

        public ObservableSnapshotHashSet<Item> Items { get; } = new ObservableSnapshotHashSet<Item>();
        IEnumerable<Item> IItemLocation.Items => Items;
        public virtual int Quantity => Items.Count;
        protected virtual bool CanContainStacks => true;

        public override void Snapshot()
        {
            Items.CreateSnapshot();
            foreach (var containedItem in Items)
            {
                containedItem.Snapshot();
            }
        }

        public virtual bool TryAdd(Item item)
        {
            if (!CanAdd(item))
            {
                return false;
            }

            var itemOrStack = CanContainStacks ? item.StackWith(Items) : item;
            if (itemOrStack != null)
            {
                itemOrStack.ContainerId = Id;
                itemOrStack.Container = this;
                Items.Add(itemOrStack.AddReference().Referenced);
                // TODO: Update weight
            }

            return true;
        }

        public virtual bool CanAdd(Item item) =>
            Quantity < Capacity && (!(item is Container) || CanContainStacks && item is ItemStack);

        public virtual bool Remove(Item item)
        {
            item.ContainerId = null;
            item.Container = null;
            if (Items.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        protected override void Delete()
        {
            base.Delete();
            foreach (var item in Items.ToList())
            {
                Remove(item);
            }
        }
    }
}
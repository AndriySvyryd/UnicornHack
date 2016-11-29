using System.Collections.Generic;
using System.Linq;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Container : Item, IItemLocation
    {
        // For EF
        protected Container()
        {
        }

        public Container(ItemVariant variant, Game game)
            : base(variant, game)
        {
        }

        public virtual int Weight => Items.Sum(i => i.Variant.Weight);

        public ICollection<Item> Items { get; } = new HashSet<Item>();
        public virtual int Quantity => Items.Count;
        protected virtual bool CanContainStacks => true;
        public virtual short Capacity { get; set; }

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
                Items.Add(itemOrStack);
                itemOrStack.AddReference();
            }

            return true;
        }

        public virtual bool CanAdd(Item item)
            => Quantity < Capacity
               && (!(item is Container)
                   || (CanContainStacks && item is ItemStack));

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

        IEnumerable<Item> IItemLocation.Items => Items;
    }
}
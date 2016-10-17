using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Container : Item
    {
        // For EF
        protected Container()
        {
        }

        public Container(ItemType type, Game game)
            : base(type, game)
        {
        }

        public ICollection<Item> Items { get; } = new HashSet<Item>();
        public int Quantity => Items.Count;
        protected virtual bool CanContainStacks => true;

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
            => !(item is Container) || item is ItemStack;

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
    }
}
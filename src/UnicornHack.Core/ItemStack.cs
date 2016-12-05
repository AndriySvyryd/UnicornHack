using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class ItemStack : Container
    {
        public ItemStack()
        {
        }

        public ItemStack(Item baseItem, Game game)
            : base(game)
        {
            BaseName = baseItem.Name;
        }

        protected override bool CanContainStacks => false;

        public override bool CanAdd(Item item)
        {
            if (item.BaseName != BaseName)
            {
                return false;
            }

            if (Quantity >= StackSize)
            {
                return false;
            }

            return true;
        }

        public override Item StackWith(IEnumerable<Item> existingItems)
        {
            // TODO: combine stacks
            return this;
        }

        public override TransientReference<Item> Split(int quantity)
        {
            if (quantity > Quantity
                || quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            if (quantity == Quantity)
            {
                return AddReference();
            }

            TransientReference<Item> result;
            if (quantity == 1)
            {
                result = Items.First().AddReference();
                Remove(result.Referenced);
            }
            else
            {
                var newStack = new ItemStack(BaseItem, Game);
                foreach (var item in Items.Take(quantity).ToList())
                {
                    using (item.AddReference())
                    {
                        Remove(item);
                        newStack.TryAdd(item);
                    }
                }
                result = newStack.AddReference();
            }

            if (Quantity == 1)
            {
                var lastItem = Items.First();
                using (lastItem.AddReference())
                {
                    Remove(lastItem);
                    ReplaceWith(lastItem);
                }
            }

            return result;
        }
    }
}
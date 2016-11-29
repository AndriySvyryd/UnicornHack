using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class ItemStack : Container
    {
        // For EF
        protected ItemStack()
        {
        }

        public ItemStack(ItemVariant variant, Game game)
            : base(variant, game)
        {
        }

        protected override bool CanContainStacks => false;

        public override bool CanAdd(Item item)
        {
            if (item.Variant != Variant)
            {
                return false;
            }

            if (Quantity == Variant.StackSize)
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
                var newStack = new ItemStack(Variant, Game);
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
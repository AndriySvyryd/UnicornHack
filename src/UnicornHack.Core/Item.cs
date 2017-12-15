using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Item : Entity, IReferenceable
    {
        public virtual bool Nameable { get; set; }
        public virtual ItemType Type { get; set; }
        public virtual int StackSize { get; set; }
        public virtual SizeCategory EquipableSizes { get; set; }
        public virtual EquipmentSlot EquipableSlots { get; set; }
        public EquipmentSlot? EquippedSlot { get; set; }

        public int? ActorId { get; set; }
        public Actor Actor { get; set; }
        public int? ContainerId { get; set; }
        public Container Container { get; set; }

        public EquipmentSlot GetEquipableSlots(int size)
        {
            var category = GetSizeCategory(size);
            if ((EquipableSizes & category) != 0)
            {
                return EquipableSlots;
            }

            var slots = EquipmentSlot.Default;
            if ((EquipableSlots & EquipmentSlot.GraspBothExtremities) != 0 &&
                (EquipableSlots & EquipmentSlot.GraspSingleExtremity) != 0)
            {
                if (category != SizeCategory.Tiny)
                {
                    var smallerSize = (SizeCategory)((int)category >> 1);
                    if ((EquipableSizes & smallerSize) != 0)
                    {
                        slots |= EquipmentSlot.GraspSingleExtremity;
                    }
                }

                if (category != SizeCategory.Gigantic)
                {
                    var biggerSize = (SizeCategory)((int)category << 1);
                    if ((EquipableSizes & biggerSize) != 0)
                    {
                        slots |= EquipmentSlot.GraspBothExtremities;
                    }
                }
            }

            return slots;
        }

        private static SizeCategory GetSizeCategory(int size)
        {
            if (size <= 2)
            {
                return SizeCategory.Tiny;
            }
            if (size <= 4)
            {
                return SizeCategory.Small;
            }
            if (size <= 8)
            {
                return SizeCategory.Medium;
            }
            if (size <= 13)
            {
                return SizeCategory.Large;
            }
            return size <= 25
                ? SizeCategory.Huge
                : SizeCategory.Gigantic;
        }

        public Item()
        {
        }

        public Item(Game game) : base(game)
        {
        }

        private int _referenceCount;

        void IReferenceable.AddReference() => _referenceCount++;

        public TransientReference<Item> AddReference() => new TransientReference<Item>(this);

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                Delete();
            }
        }

        protected virtual void Delete()
        {
            foreach (var ability in Abilities.ToList())
            {
                Remove(ability);
            }
            Game.Repository.Delete(this);
        }

        public virtual bool MoveTo(IItemLocation location)
        {
            if (!location.CanAdd(this))
            {
                return false;
            }

            using (AddReference())
            {
                Remove();
                location.TryAdd(this);
            }

            return true;
        }

        protected virtual void ReplaceWith(Item item)
        {
            if (Container != null)
            {
                var container = Container;
                Container.Remove(this);
                var added = container.TryAdd(item);
                Debug.Assert(added);
            }

            if (Actor != null)
            {
                var actor = Actor;
                Actor.Remove(this);
                var added = actor.TryAdd(item);
                Debug.Assert(added);
            }

            if (Level != null)
            {
                var level = Level;
                var levelX = LevelX;
                var levelY = LevelY;
                Level.Remove(this);
                var added = level.TryAdd(item, levelX, levelY);
                Debug.Assert(added);
            }

            Debug.Assert(_referenceCount == 0);
        }

        public virtual Item StackWith(IEnumerable<Item> existingItems)
        {
            if (StackSize <= 1)
            {
                return this;
            }

            foreach (var existingItem in existingItems)
            {
                if (existingItem.BaseName == BaseName)
                {
                    var stack = existingItem as ItemStack;
                    if (stack == null)
                    {
                        stack = new ItemStack(this, Game);

                        existingItem.MoveTo(stack);

                        stack.TryAdd(this);
                        return stack;
                    }

                    if (!stack.CanAdd(this))
                    {
                        continue;
                    }

                    stack.TryAdd(this);
                    return null;
                }
            }

            return this;
        }

        public void Remove()
        {
            Actor?.Remove(this);

            Container?.Remove(this);

            Level?.Remove(this);
        }

        public virtual TransientReference<Item> Split(int quantity)
        {
            if (quantity != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            var result = new TransientReference<Item>(this);

            Remove();

            return result;
        }
    }
}

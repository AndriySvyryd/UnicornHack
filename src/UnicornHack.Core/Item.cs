using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Item : IReferenceable
    {
        public virtual string Name { get; set; }

        public virtual string BaseName { get; set; }
        // TODO: Remove this
        public ItemVariant BaseItem => BaseName == null ? null : ItemVariant.Loader.Get(BaseName);

        public virtual ItemType Type { get; set; }

        /// <summary> 100g units </summary>
        public virtual int Weight { get; set; }

        public virtual Material Material { get; set; }
        public virtual bool Nameable { get; set; }
        public virtual int StackSize { get; set; }
        public virtual Size EquipableSizes { get; set; }
        public virtual EquipmentSlot EquipableSlots { get; set; }
        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public virtual ISet<string> SimpleProperties { get; set; } = new HashSet<string>();
        public virtual IDictionary<string, object> ValuedProperties { get; set; } = new Dictionary<string, object>();

        public virtual int Id { get; private set; }
        public int? ActorId { get; set; }
        public Actor Actor { get; set; }
        public EquipmentSlot? EquippedSlot { get; set; }
        public int? ContainerId { get; set; }
        public Container Container { get; set; }
        public string BranchName { get; set; }
        public byte? LevelDepth { get; set; }
        public Level Level { get; set; }
        public byte? LevelX { get; set; }
        public byte? LevelY { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public EquipmentSlot GetEquipableSlots(Size size)
        {
            if (EquipableSizes.HasFlag(size))
            {
                return EquipableSlots;
            }

            var slots = EquipmentSlot.Default;
            if (EquipableSlots.HasFlag(EquipmentSlot.GraspBothExtremities)
                && EquipableSlots.HasFlag(EquipmentSlot.GraspSingleExtremity))
            {
                if (size != Size.Tiny)
                {
                    var smallerSize = (Size)((int)size >> 1);
                    if (EquipableSizes.HasFlag(smallerSize))
                    {
                        slots |= EquipmentSlot.GraspSingleExtremity;
                    }
                }

                if (size != Size.Gigantic)
                {
                    var biggerSize = (Size)((int)size << 1);
                    if (EquipableSizes.HasFlag(biggerSize))
                    {
                        slots |= EquipmentSlot.GraspBothExtremities;
                    }
                }
            }

            return slots;
        }

        public Item()
        {
        }

        public Item(Game game)
            : this()
        {
            Game = game;
            Id = game.NextItemId++;
            game.Items.Add(this);
        }

        private int _referenceCount;

        void IReferenceable.AddReference()
            => _referenceCount++;

        public TransientReference<Item> AddReference()
            => new TransientReference<Item>(this);

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                foreach (var ability in Abilities)
                {
                    ability.ItemId = null;
                    ability.RemoveReference();
                }
                // TODO: uncomment when bug fixed
                //Game.Repository.Delete(this);
            }
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
                var added = level.TryAdd(item, levelX.Value, levelY.Value);
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
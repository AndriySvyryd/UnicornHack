using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Item : IReferenceable
    {
        // For EF
        protected Item()
        {
        }

        public Item(ItemVariant variant, Game game)
        {
            VariantName = variant.Name;
            Game = game;
            Id = game.NextItemId++;
            game.Items.Add(this);
        }

        public int Id { get; private set; }
        public string Name => GivenName ?? Variant.Name;
        public string GivenName { get; set; }
        public ItemVariant Variant => ItemVariant.Get(VariantName);
        public virtual string VariantName { get; set; }

        public int? ActorId { get; set; }
        public Actor Actor { get; set; }
        public int? ContainerId { get; set; }
        public Container Container { get; set; }
        public int? LevelId { get; set; }
        public Level Level { get; set; }
        public byte? LevelX { get; set; }
        public byte? LevelY { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game { get; set; }

        public Beatitude Beatitude { get; set; }

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
            foreach (var existingItem in existingItems)
            {
                if (existingItem.Variant == Variant)
                {
                    var stack = existingItem as ItemStack;
                    if (stack == null)
                    {
                        stack = new ItemStack(Variant, Game);

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

        protected void Remove()
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

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Item> AddReference()
        {
            return new TransientReference<Item>(this);
        }

        public void RemoveReference()
        {
            if (--_referenceCount == 0)
            {
                Game.Delete(this);
            }
        }

        public static bool Create(string variantName, IItemLocation location, int quantity = 1)
        {
            var variant = ItemVariant.Get(variantName);
            if (variant.Name == "gold coin")
            {
                return Gold.Create(location.Game, quantity).MoveTo(location);
            }

            var succeeded = true;
            for (var i = 0; i < quantity; i++)
            {
                var item = new Item(variant, location.Game);
                succeeded = item.MoveTo(location) && succeeded;
            }

            return succeeded;
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameDefinitions.Effects;
using UnicornHack.Models.GameState.Events;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public abstract class Actor
    {
        protected Actor()
        {
        }

        protected Actor(ActorVariant variant, byte x, byte y, Level level)
        {
            OriginalVariant = variant.Name;
            LevelX = x;
            LevelY = y;
            Level = level;
            Game = level.Game;
            Id = level.Game.NextActorId++;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        [NotMapped]
        public abstract ActorVariant Variant { get; }

        public virtual string OriginalVariant { get; set; }
        public virtual string PolymorphedVariant { get; set; }

        public virtual string GivenName { get; set; }

        public virtual Sex Sex { get; set; }

        public virtual byte XPLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual int NextLevelXP { get; set; }
        public virtual int MaxHP { get; set; }
        public virtual int HP { get; set; }

        [NotMapped]
        public virtual bool IsAlive => HP > 0;

        public virtual int Gold { get; set; }
        public virtual int AC { get; set; }
        public virtual ICollection<Item> Inventory { get; set; } = new HashSet<Item>();

        public virtual byte LevelX { get; set; }
        public virtual byte LevelY { get; set; }
        public virtual Level Level { get; set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }

        public const int ActionPointsPerTurn = 100;
        public virtual int ActionPoints { get; set; }

        [NotMapped]
        public abstract byte MovementRate { get; }

        [NotMapped]
        public abstract IList<Ability> Abilities { get; }

        public bool Has(string property)
        {
            return Variant.SimpleProperties.Contains(property);
        }

        public T Get<T>(string property)
        {
            object value;
            if (Variant.ValuedProperties.TryGetValue(property, out value))
            {
                return (T)value;
            }

            return default(T);
        }

        public bool Move(Vector direction, bool safe = false)
        {
            return Move(direction.X, direction.Y, direction.Z, safe);
        }

        public bool Move(sbyte x, sbyte y, sbyte z, bool safe = false)
        {
            if ((x == 0) && (y == 0) && (z == 0))
            {
                return true;
            }

            if (LevelX + x < 0)
            {
                return false;
            }

            if (LevelX + x >= Level.Width)
            {
                return false;
            }

            if (LevelY + y < 0)
            {
                return false;
            }

            if (LevelY + y >= Level.Height)
            {
                return false;
            }

            if (z != 0)
            {
                if ((x != 0) || (y != 0))
                {
                    return false;
                }

                Level moveToLevel;
                byte? moveToLevelX, moveToLevelY;
                if (z < 0)
                {
                    var upStairs = Level.UpStairs.SingleOrDefault(s =>
                        (s.DownLevelX == LevelX) && (s.DownLevelY == LevelY));
                    moveToLevel = upStairs?.Up;
                    moveToLevelX = upStairs?.UpLevelX;
                    moveToLevelY = upStairs?.UpLevelY;
                }
                else
                {
                    var downStairs = Level.DownStairs.SingleOrDefault(s =>
                        (s.UpLevelX == LevelX) && (s.UpLevelY == LevelY));
                    moveToLevel = downStairs?.Down;
                    moveToLevelX = downStairs?.DownLevelX;
                    moveToLevelY = downStairs?.DownLevelY;
                }

                if (moveToLevel == null)
                {
                    return false;
                }

                if (!moveToLevel.PlayerCharacters.Any())
                {
                    // Catch up the level to current turn
                    var waitedFor = moveToLevel.Turn();
                    Debug.Assert(waitedFor == null);
                }

                // TODO: Shove off any monsters standing on stairs

                Level.Actors.Remove(this);
                Level = moveToLevel;
                LevelX = moveToLevelX.Value;
                LevelY = moveToLevelY.Value;
                moveToLevel.Actors.Add(this);

                ActorMoveEvent.New(this, movee: null);

                ActionPoints = 0;

                return true;
            }

            var newX = (byte)(LevelX + x);
            var newY = (byte)(LevelY + y);
            var conflictingActor =
                Level.Actors.SingleOrDefault(a => (a.LevelX == newX) && (a.LevelY == newY));
            if (conflictingActor != null)
            {
                if (safe)
                {
                    return false;
                }
                Attack(conflictingActor);
                return true;
            }

            if (!((MapFeature)Level.Layout[Level.PointToIndex[newX, newY]]).CanMoveTo())
            {
                return false;
            }

            LevelX = newX;
            LevelY = newY;
            var itemsOnNewCell = Level.Items.Where(i => (i.LevelX == newX) && (i.LevelY == newY));
            foreach (var itemOnNewCell in itemsOnNewCell)
            {
                PickUp(itemOnNewCell);
            }

            ActorMoveEvent.New(this, movee: null);

            ActionPoints -= Level.DefaultMovementCost/MovementRate*ActionPointsPerTurn;

            return true;
        }

        public virtual int? Attack(Actor victim)
        {
            ActionPoints = 0;
            var ability = Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.Targetted);
            var damage = ability?.Effects.OfType<PhysicalDamage>().FirstOrDefault()?.Damage
                ?? ability?.Effects.OfType<ElectricityDamage>().FirstOrDefault()?.Damage
                ?? ability?.Effects.OfType<FireDamage>().FirstOrDefault()?.Damage;
            if (ability == null
                || damage == null)
            {
                return null;
            }

            if (Game.NextRandom(maxValue: 3) == 0)
            {
                AttackEvent.New(this, victim, ability.Action, hit: false);
                return null;
            }

            AttackEvent.New(this, victim, ability.Action, hit: true, damage: damage.Value);
            victim.ChangeCurrentHP(-1*damage.Value);

            if (!victim.IsAlive)
            {
                XP += victim.XP;
            }
            return damage;
        }

        public virtual bool CanAct()
        {
            if (!IsAlive)
            {
                return false;
            }

            ActionPoints = ActionPoints >= ActionPointsPerTurn
                ? ActionPointsPerTurn
                : ActionPoints;
            ActionPoints += ActionPointsPerTurn;
            return ActionPoints > ActionPointsPerTurn;
        }

        /// <summary></summary>
        /// <returns>Returns <c>false</c> if the action wasn't completed</returns>
        // TODO: return Task
        public abstract bool Act();

        public virtual void Sense(SensoryEvent @event)
        {
        }

        public virtual SenseType CanSense(Actor target)
        {
            var sense = SenseType.None;
            if (target == this) // Or is adjecent
            {
                sense |= SenseType.Touch;
            }

            sense |= SenseType.Sight;

            return sense;
        }

        public virtual SenseType CanSense(Item target)
        {
            var sense = SenseType.Sight;

            return sense;
        }

        public virtual bool Eat(Item item)
        {
            var stackableItem = item as StackableItem;
            if ((stackableItem != null)
                && (stackableItem.Quantity > 1))
            {
                stackableItem.Quantity--;
            }
            else
            {
                Inventory.Remove(item);
                item.Actor = null;
            }

            if (item.Type == ItemType.Food)
            {
                ChangeCurrentHP(hp: 5);
            }

            ItemConsumptionEvent.New(this, item);

            return true;
        }

        public virtual bool PickUp(Item item)
        {
            var stackableItem = item as StackableItem;
            if (stackableItem != null)
            {
                ItemPickUpEvent.New(this, stackableItem);

                if (stackableItem.Type == ItemType.Gold)
                {
                    Gold += stackableItem.Quantity;
                    stackableItem.Level = null;
                    return true;
                }

                var inventoryStack = Inventory.Where(i => i.Type == item.Type).Cast<StackableItem>().SingleOrDefault();
                if (inventoryStack != null)
                {
                    inventoryStack.Quantity += stackableItem.Quantity;
                    stackableItem.Level = null;
                    return true;
                }
            }

            ItemPickUpEvent.New(this, item);

            item.Level = null;
            Inventory.Add(item);
            return true;
        }

        public virtual bool DropGold(short quantity)
        {
            if ((quantity == 0)
                || (quantity > Gold))
            {
                return false;
            }

            Gold -= quantity;
            var item = new StackableItem(ItemType.Gold, Level, LevelX, LevelY, quantity);
            Inventory.Add(item);

            ItemDropEvent.New(this, item);

            return true;
        }

        public virtual bool Drop(Item item)
        {
            var stackableItem = item as StackableItem;
            if (stackableItem != null)
            {
                var groundStack = Level.Items
                    .Where(i => (i.LevelX == LevelX) && (i.LevelY == LevelY) && (i.Type == item.Type))
                    .Cast<StackableItem>().SingleOrDefault();
                if (groundStack != null)
                {
                    // TODO: Create a transient item
                    ItemDropEvent.New(this, stackableItem);

                    groundStack.Quantity++;
                    if (stackableItem.Quantity > 1)
                    {
                        stackableItem.Quantity--;
                    }
                    else
                    {
                        stackableItem.Actor = null;
                    }
                    return true;
                }
                if (stackableItem.Quantity > 1)
                {
                    item = new StackableItem(stackableItem.Type, Level, LevelX, LevelY, quantity: 1);
                    stackableItem.Quantity--;
                }
            }

            item.Actor = null;
            item.LevelX = LevelX;
            item.LevelY = LevelY;
            item.Level = Level;
            Level.Items.Add(item);

            ItemDropEvent.New(this, item);

            return true;
        }

        public virtual bool ChangeCurrentHP(int hp)
        {
            HP += hp;
            if (!IsAlive)
            {
                DropGold((short)Gold);
                foreach (var item in Inventory)
                {
                    Drop(item);
                }
                DeathEvent.New(this, corpse: null);
                Level.Actors.Remove(this);
                return false;
            }

            if (HP > MaxHP)
            {
                HP = MaxHP;
            }

            return true;
        }
    }
}
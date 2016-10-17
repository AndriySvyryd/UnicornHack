using System.Collections.Generic;
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
            VariantName = variant.Name;
            LevelX = x;
            LevelY = y;
            Level = level;
            Game = level.Game;
            Id = level.Game.NextActorId++;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        public abstract ActorVariant Variant { get; }

        public virtual string VariantName { get; set; }

        public virtual string GivenName { get; set; }

        public virtual Sex Sex { get; set; }

        public virtual byte XPLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual int NextLevelXP { get; set; }
        public virtual int MaxHP { get; set; }
        public virtual int HP { get; set; }

        public virtual bool IsAlive => HP > 0;

        public virtual int Gold { get; set; }
        public virtual int AC { get; set; }
        public virtual ICollection<Item> Inventory { get; } = new HashSet<Item>();

        public virtual byte LevelX { get; set; }
        public virtual byte LevelY { get; set; }
        public virtual int? LevelId { get; set; }
        public virtual Level Level { get; set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }

        public const int ActionPointsPerTurn = 100;
        public virtual int ActionPoints { get; set; }

        public abstract byte MovementRate { get; }

        //Check effects
        //Check equipment
        //Check innate abilities (including skills for players)
        public virtual IEnumerable<Ability> Abilities => Variant.Abilities;

        //Check effects (attributes provide an effect for players)
        //Check equipment
        //Check sustained abilities
        //Check innate properties
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
            var itemsOnNewCell = Level.Items.Where(i => (i.LevelX == newX) && (i.LevelY == newY)).ToList();
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
            @event.Sensor = this;
            @event.Delete();
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
            using (var reference = item.Split(1))
            {
                if (reference.Referenced.Type == ItemType.Food)
                {
                    ChangeCurrentHP(hp: 5);
                }

                ItemConsumptionEvent.New(this, reference.Referenced);

                return true;
            }
        }

        public virtual bool PickUp(Item item)
        {
            item.MoveTo(this);

            ItemPickUpEvent.New(this, item);

            return true;
        }

        public virtual bool DropGold(short quantity)
        {
            if (quantity == 0
                || quantity > Gold)
            {
                return false;
            }

            Gold -= quantity;
            var item = new Gold(quantity, Game);
            item.MoveTo(Level, LevelX, LevelY);

            ItemDropEvent.New(this, item);

            return true;
        }

        public virtual bool Drop(Item item)
        {
            item.MoveTo(Level, LevelX, LevelY);

            ItemDropEvent.New(this, item);

            return true;
        }

        public virtual bool TryAdd(Item item)
        {
            if (!CanAdd(item))
            {
                return false;
            }

            var itemOrStack = item.StackWith(Inventory);
            if (itemOrStack != null)
            {
                itemOrStack.ActorId = Id;
                itemOrStack.Actor = this;
                Inventory.Add(itemOrStack);
                itemOrStack.AddReference();
            }

            return true;
        }

        public virtual bool CanAdd(Item item)
            => true;

        public virtual bool Remove(Item item)
        {
            item.ActorId = null;
            item.Actor = null;
            if (Inventory.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        public virtual bool ChangeCurrentHP(int hp)
        {
            HP += hp;
            if (!IsAlive)
            {
                DropGold((short)Gold);
                foreach (var item in Inventory.ToList())
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
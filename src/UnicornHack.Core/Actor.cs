using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class Actor : IItemLocation, IReferenceable
    {
        public const int DefaultActionDelay = 100;
        public const string InnateName = "Innate";
        public const string UnarmedAttackName = "Unarmed attack";
        public const string MeleeAttackName = "Melee attack";
        public const string SecondaryMeleeAttackName = "Secondary melee attack";

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual string BaseName { get; set; }
        public virtual Species Species { get; set; }
        public virtual SpeciesClass SpeciesClass { get; set; }
        public virtual int MovementDelay { get; set; }

        /// <summary> 100g units </summary>
        public virtual int Weight { get; set; }
        public virtual Material Material { get; set; }
        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public virtual Sex Sex { get; set; }
        public virtual int MaxHP { get; set; }
        public virtual int HP { get; set; }
        public virtual bool IsAlive => HP > 0;

        /// <summary>
        ///     Warning: this should only be updated when this actor is acting
        /// </summary>
        public virtual int NextActionTick { get; set; }
        public virtual int Gold { get; set; }
        IEnumerable<Item> IItemLocation.Items => Inventory;
        public virtual ICollection<Item> Inventory { get; } = new HashSet<Item>();
        public virtual string BranchName { get; set; }
        public virtual byte? LevelDepth { get; set; }
        public virtual Level Level { get; set; }
        public virtual byte LevelX { get; set; }
        public virtual byte LevelY { get; set; }
        public virtual Direction Heading { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }

        public virtual Game Game { get; set; }

        protected Actor()
        {
        }

        protected Actor(Level level, byte x, byte y) : this()
        {
            Game = level.Game;
            Id = level.Game.NextActorId++;
            Game.Actors.Add(this);
            LevelX = x;
            LevelY = y;
            Level = level;
            NextActionTick = level.Game.NextPlayerTick;
            level.Actors.Push(this);
            AddReference();
        }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Actor> AddReference() => new TransientReference<Actor>(this);

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                foreach (var item in Inventory)
                {
                    item.RemoveReference();
                }
                foreach (var ability in Abilities)
                {
                    ability.RemoveReference();
                }
                Game.Repository.Delete(this);
            }
        }

        public virtual void RecalculateEffectsAndAbilities()
        {
            Item twoHandedWeapon = null;
            Item mainWeapon = null;
            Item secondaryWeapon = null;
            var usedSlots = EquipmentSlot.Default;
            foreach (var item in Inventory)
            {
                if (item.EquippedSlot.HasValue)
                {
                    switch (item.EquippedSlot)
                    {
                        case EquipmentSlot.GraspBothExtremities:
                            twoHandedWeapon = item;
                            break;
                        case EquipmentSlot.GraspMainExtremity:
                            mainWeapon = item;
                            break;
                        case EquipmentSlot.GraspSecondaryExtremity:
                            secondaryWeapon = item;
                            break;
                    }

                    usedSlots |= item.EquippedSlot.Value;
                }
            }

            var mainMeleeAttack = Abilities.FirstOrDefault(a => a.Name == MeleeAttackName);
            if (mainMeleeAttack != null)
            {
                mainMeleeAttack.IsUsable = false;
            }

            var secondaryMeleeAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryMeleeAttackName);
            if (secondaryMeleeAttack != null)
            {
                secondaryMeleeAttack.IsUsable = false;
            }

            if (twoHandedWeapon != null || mainWeapon != null || secondaryWeapon != null)
            {
                if (mainMeleeAttack == null)
                {
                    mainMeleeAttack = new Ability(Game)
                    {
                        Name = MeleeAttackName,
                        Activation = AbilityActivation.OnTarget,
                        DelayTicks = 100,
                        Effects = new HashSet<Effect> {new MeleeAttack(Game), new PhysicalDamage(Game)},
                        IsUsable = false
                    };

                    Abilities.Add(mainMeleeAttack);
                }
                if (secondaryMeleeAttack == null)
                {
                    secondaryMeleeAttack = new Ability(Game)
                    {
                        Name = SecondaryMeleeAttackName,
                        Activation = AbilityActivation.OnMeleeAttack,
                        Effects = new HashSet<Effect> {new MeleeAttack(Game), new PhysicalDamage(Game)},
                        IsUsable = false
                    };

                    Abilities.Add(secondaryMeleeAttack);
                }

                mainMeleeAttack.IsUsable = true;
                var firstWeaponEffect = mainMeleeAttack.Effects.OfType<MeleeAttack>().Single();
                if (mainWeapon != null && secondaryWeapon != null)
                {
                    secondaryMeleeAttack.IsUsable = true;
                    var secondWeaponEffect = secondaryMeleeAttack.Effects.OfType<MeleeAttack>().Single();
                    firstWeaponEffect.Weapon = mainWeapon;
                    secondWeaponEffect.Weapon = secondaryWeapon;
                }
                else
                {
                    firstWeaponEffect.Weapon = mainWeapon ?? secondaryWeapon ?? twoHandedWeapon;
                }
            }

            foreach (var ability in Abilities)
            {
                if (ability.FreeSlotsRequired != EquipmentSlot.Default)
                {
                    ability.IsUsable = (ability.FreeSlotsRequired & usedSlots) == EquipmentSlot.Default;
                }
            }

            //TODO: Add active abilities first

            //Check effects (attributes provide an effect for players)
            //Check equipment
            //Check innate abilities (including skills for players)
            //Check sustained abilities
            //Check innate properties
        }

        /// <summary></summary>
        /// <returns>Returns <c>false</c> if the actor hasn't finished their turn.</returns>
        public abstract bool Act();

        public virtual void Sense(SensoryEvent @event)
        {
            @event.Game = Game;
            @event.AddReference();
            @event.RemoveReference();
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

        public virtual SenseType CanSense(Item target) => SenseType.Sight;

        public virtual bool UseStairs(bool up, bool pretend = false)
        {
            var stairs = Level.Connections.SingleOrDefault(s => s.LevelX == LevelX && s.LevelY == LevelY);

            var moveToLevel = stairs?.TargetLevel;
            if (moveToLevel == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            NextActionTick += MovementDelay;
            var moveToLevelX = stairs.TargetLevelX;
            var moveToLevelY = stairs.TargetLevelY;

            if (!moveToLevel.Players.Any())
            {
                var previousNextPlayerTick = Game.NextPlayerTick;
                Game.NextPlayerTick = NextActionTick;

                // Catch up the level to current turn
                // TODO: Instead of this put actor in 'traveling' state till its next action
                var waitedFor = moveToLevel.Turn();
                Debug.Assert(waitedFor == null);

                Game.NextPlayerTick = previousNextPlayerTick;
            }

            var conflictingActor =
                moveToLevel.Actors.SingleOrDefault(a => a.LevelX == moveToLevelX && a.LevelY == moveToLevelY);
            conflictingActor?.GetDisplaced();

            if (moveToLevel.BranchName == "surface")
            {
                ChangeCurrentHP(-1 * HP);
                return true;
            }

            using (AddReference())
            {
                Level.Actors.Remove(this);
                Level = moveToLevel;
                LevelX = moveToLevelX.Value;
                LevelY = moveToLevelY.Value;
                moveToLevel.Actors.Push(this);

                ActorMoveEvent.New(this, movee: null, eventOrder: Game.EventOrder++);
            }

            return true;
        }

        public virtual bool Move(Direction direction, bool pretend = false, bool safe = false)
        {
            if (Heading != direction)
            {
                if (pretend)
                {
                    return true;
                }

                var octants = direction.ClosestOctantsTo(Heading);

                NextActionTick += (MovementDelay * octants) / 4;

                Heading = direction;

                // TODO: Fire event

                return true;
            }

            var targetCell = ToLevelCell(Vector.Convert(direction));
            if (!targetCell.HasValue)
            {
                return false;
            }

            var conflictingActor =
                Level.Actors.SingleOrDefault(a => a.LevelX == targetCell.Value.X && a.LevelY == targetCell.Value.Y);
            if (conflictingActor != null)
            {
                if (safe)
                {
                    return false;
                }

                return Attack(conflictingActor, pretend);
            }

            if (MovementDelay == 0)
            {
                return false;
            }

            if (!Reposition(targetCell.Value, pretend, safe))
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            ActorMoveEvent.New(this, movee: null, eventOrder: Game.EventOrder++);

            // TODO: take terrain into account
            NextActionTick += MovementDelay;

            return true;
        }

        private bool Reposition(Point targetCell, bool pretend, bool safe)
        {
            if (!Level.CanMoveTo(targetCell))
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            LevelX = targetCell.X;
            LevelY = targetCell.Y;
            var itemsOnNewCell = Level.Items.Where(i => i.LevelX == targetCell.X && i.LevelY == targetCell.Y).ToList();
            foreach (var itemOnNewCell in itemsOnNewCell)
            {
                PickUp(itemOnNewCell);
            }

            return true;
        }

        public virtual bool GetDisplaced()
        {
            // TODO: displace other actors
            var possibleDirectionsToMove = Level.GetPossibleMovementDirections(new Point(LevelX, LevelY), safe: true);
            if (possibleDirectionsToMove.Count == 0)
            {
                NextActionTick += DefaultActionDelay;
                return true;
            }
            var directionIndex = Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);

            var targetCell = ToLevelCell(Vector.Convert(possibleDirectionsToMove[directionIndex]));
            if (targetCell != null)
            {
                return Reposition(targetCell.Value, pretend: false, safe: true);
            }

            // TODO: fire event

            return false;
        }

        public virtual bool Attack(Actor victim, bool pretend = false)
        {
            var ability = Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.OnTarget && a.IsUsable);
            if (ability == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            return ability.Activate(new AbilityActivationContext {Activator = this, Target = victim, IsAttack = true},
                pretend);
        }

        public virtual bool Equip(Item item, EquipmentSlot slot, bool pretend = false)
        {
            var equipped = GetEquippedItem(slot);
            if (equipped == item)
            {
                return true;
            }

            if (!item.EquipableSlots.HasFlag(slot))
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate delay
            NextActionTick += DefaultActionDelay;

            if (equipped == null)
            {
                if (slot == EquipmentSlot.GraspMainExtremity || slot == EquipmentSlot.GraspSecondaryExtremity)
                {
                    Unequip(GetEquippedItem(EquipmentSlot.GraspBothExtremities));
                }
                else if (slot == EquipmentSlot.GraspBothExtremities)
                {
                    Unequip(GetEquippedItem(EquipmentSlot.GraspMainExtremity));
                    Unequip(GetEquippedItem(EquipmentSlot.GraspSecondaryExtremity));
                }
            }
            else
            {
                Unequip(equipped);
            }

            item.EquippedSlot = slot;
            ItemEquipmentEvent.New(this, item, Game.EventOrder++);

            RecalculateEffectsAndAbilities();

            return true;
        }

        public virtual bool Unequip(Item item, bool pretend = false)
        {
            if (item?.EquippedSlot == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate delay
            NextActionTick += DefaultActionDelay;

            item.EquippedSlot = null;
            ItemUnequipmentEvent.New(this, item, Game.EventOrder++);

            // TODO: Just invalidate
            RecalculateEffectsAndAbilities();

            return true;
        }

        public virtual bool Quaff(Item item, bool pretend = false)
        {
            if (item.Type != ItemType.Potion)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate delay
            NextActionTick += DefaultActionDelay;

            using (var reference = item.Split(1))
            {
                var splitItem = reference.Referenced;
                foreach (var ability in splitItem.Abilities.Where(a => a.Activation == AbilityActivation.OnConsumption))
                {
                    ability.Activate(new AbilityActivationContext {Activator = this, Target = this}, pretend);
                }

                ItemConsumptionEvent.New(this, splitItem, Game.EventOrder++);
            }

            return true;
        }

        public virtual bool PickUp(Item item, bool pretend = false)
        {
            if (pretend)
            {
                return true;
            }

            // TODO: Calculate delay
            NextActionTick += DefaultActionDelay;

            using (item.AddReference())
            {
                item.MoveTo(this);

                ItemPickUpEvent.New(this, item, Game.EventOrder++);
            }

            return true;
        }

        public virtual bool DropGold(int quantity, bool pretend = false)
        {
            if (quantity == 0 || quantity > Gold)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            NextActionTick += DefaultActionDelay;

            Gold -= quantity;
            var item = GoldVariant.Get().Instantiate(new LevelCell(Level, LevelX, LevelY), quantity).Single();

            ItemDropEvent.New(this, item, Game.EventOrder++);

            return true;
        }

        public virtual bool Drop(Item item, bool pretend = false)
        {
            if (item.EquippedSlot != null && !Unequip(item, pretend))
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            NextActionTick += DefaultActionDelay;

            using (item.AddReference())
            {
                item.MoveTo(new LevelCell(Level, LevelX, LevelY));

                ItemDropEvent.New(this, item, Game.EventOrder++);
            }

            return true;
        }

        public virtual bool TryAdd(IEnumerable<Item> items)
        {
            var succeeded = true;
            foreach (var item in items.ToList())
            {
                succeeded = TryAdd(item) && succeeded;
            }

            return succeeded;
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

        public virtual bool CanAdd(Item item) => true;

        public virtual bool Remove(Item item)
        {
            item.ActorId = null;
            item.Actor = null;
            item.EquippedSlot = null;
            if (Inventory.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        public virtual bool ChangeCurrentHP(int hp)
        {
            if (!IsAlive)
            {
                return false;
            }
            HP += hp;
            if (!IsAlive)
            {
                Die();
                return false;
            }

            if (HP > MaxHP)
            {
                HP = MaxHP;
            }

            return true;
        }

        protected virtual void Die()
        {
            DeathEvent.New(this, corpse: null, eventOrder: Game.EventOrder++);
        }

        public virtual Item GetEquippedItem(EquipmentSlot slot) =>
            Inventory.FirstOrDefault(item => item.EquippedSlot == slot);

        public virtual Point? ToLevelCell(Vector direction)
        {
            var newX = LevelX + direction.X;
            var newY = LevelY + direction.Y;
            if (newX < 0)
            {
                return null;
            }

            if (newX >= Level.Width)
            {
                return null;
            }

            if (newY < 0)
            {
                return null;
            }

            if (newY >= Level.Height)
            {
                return null;
            }

            return new Point((byte)newX, (byte)newY);
        }

        public class TickComparer : IComparer<Actor>
        {
            public static readonly TickComparer Instance = new TickComparer();

            private TickComparer()
            {
            }

            public int Compare(Actor x, Actor y) => x.NextActionTick - y.NextActionTick;
        }
    }
}
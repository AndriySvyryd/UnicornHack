using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class Actor : Entity, IItemLocation, IReferenceable
    {
        public const int DefaultActionDelay = 100;
        public const string InnateName = "innate";
        public const string UnarmedAttackName = "unarmed attack";
        public const string MeleeAttackName = "melee attack";
        public const string SecondaryMeleeAttackName = "secondary melee attack";

        public Species Species { get; set; }
        public SpeciesClass SpeciesClass { get; set; }
        public Sex Sex { get; set; }
        public int MovementDelay { get; set; }
        public virtual Direction Heading { get; set; }
        public int MaxHP { get; set; } = 1;
        public int HP { get; set; } = 1; // So actors start out alive
        public bool IsAlive => HP > 0;
        public int MaxEP { get; set; }
        public int EP { get; set; }

        // TODO: Move to Entity when bug fixed
        public string BranchName { get; set; }

        public byte? LevelDepth { get; set; }
        public Level Level { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }

        /// <summary>
        ///     Warning: this should only be updated when this actor is acting
        /// </summary>
        public virtual int NextActionTick { get; set; }

        public virtual int Gold { get; set; }
        IEnumerable<Item> IItemLocation.Items => Inventory;
        public virtual ICollection<Item> Inventory { get; } = new HashSet<Item>();

        protected Actor()
        {
        }

        protected Actor(Level level, byte x, byte y) : base(level.Game)
        {
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
            if (_referenceCount > 0
                && --_referenceCount == 0)
            {
                foreach (var item in Inventory.ToList())
                {
                    item.RemoveReference();
                }
                foreach (var ability in Abilities.ToList())
                {
                    Remove(ability);
                }
                Game.Repository.Delete(this);
            }
        }

        public virtual void RecalculateAbilities()
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
                        Delay = 100,
                        Effects = new HashSet<Effect> {new MeleeAttack(Game), new PhysicalDamage(Game)},
                        IsUsable = false
                    };

                    Add(mainMeleeAttack);
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

                    Add(secondaryMeleeAttack);
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
        }

        /// <summary></summary>
        /// <returns>Returns <c>false</c> if the actor hasn't finished their turn.</returns>
        public abstract bool Act();

        public virtual void Sense(SensoryEvent @event)
        {
            @event.Game = Game;
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

            return ability.Activate(new AbilityActivationContext
            {
                Activator = this,
                Target = victim,
                IsAttack = true
            });
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

            foreach (var ability in item.Abilities.Where(
                a => a.Activation == AbilityActivation.WhileEquipped && !a.IsActive))
            {
                ability.Activate(new AbilityActivationContext
                {
                    Activator = this,
                    Target = this,
                    AbilityTrigger = AbilityActivation.WhileEquipped
                });
            }

            RecalculateAbilities();

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

            foreach (var ability in item.Abilities.Where(a
                => a.Activation == AbilityActivation.WhileEquipped && a.IsActive))
            {
                ability.Deactivate();
            }

            RecalculateAbilities();

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

                ItemConsumptionEvent.New(this, splitItem, Game.EventOrder++);

                foreach (var ability in splitItem.Abilities.Where(a => a.Activation == AbilityActivation.OnConsumption))
                {
                    ability.Activate(new AbilityActivationContext
                    {
                        Activator = this,
                        Target = this,
                        AbilityTrigger = AbilityActivation.OnDigestion
                    });
                }
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

            foreach (var ability in item.Abilities.Where(a
                => a.Activation == AbilityActivation.WhilePossessed && !a.IsActive))
            {
                ability.Activate(new AbilityActivationContext
                {
                    Activator = this,
                    Target = this,
                    AbilityTrigger = AbilityActivation.WhilePossessed
                });
            }

            return true;
        }

        public virtual bool CanAdd(Item item) => true;

        public virtual bool Remove(Item item)
        {
            if (item.EquippedSlot != null)
            {
                Unequip(item);
            }

            foreach (var ability in item.Abilities.Where(a
                => a.Activation == AbilityActivation.WhilePossessed && a.IsActive))
            {
                ability.Deactivate();
            }

            item.ActorId = null;
            item.Actor = null;
            if (Inventory.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        public override void Add(Ability ability)
        {
            base.Add(ability);

            if (ability.Activation == AbilityActivation.Always && !ability.IsActive)
            {
                ability.Activate(new AbilityActivationContext
                {
                    Activator = this,
                    Target = this,
                    AbilityTrigger = AbilityActivation.Always
                });
            }
        }

        public override void Remove(Ability ability)
        {
            base.Remove(ability);

            if (ability.IsActive)
            {
                ability.Deactivate();
            }
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

        public virtual void ChangeCurrentEP(int ep)
        {
            EP += ep;
            if (EP < 0)
            {
                EP = 0;
            }
            if (EP > MaxEP)
            {
                EP = MaxEP;
            }
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
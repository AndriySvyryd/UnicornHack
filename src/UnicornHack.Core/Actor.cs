using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class Actor : IItemLocation, ICSScriptSerializable, IReferenceable
    {
        #region State

        private Species? _species;
        private SpeciesClass? _speciesClass;
        private int? _movementDelay;
        private Size? _size;
        private int? _weight;
        private int? _nutrition;
        private Material? _material;
        private int? _armorClass;
        private int? _magicResistance;
        private ISet<string> _simpleProperties;
        private IDictionary<string, object> _valuedProperties;

        public const int DefaultActionDelay = 100;
        public const string UnarmedAttackName = "Unarmed attack";
        public const string MeleeAttackName = "Melee attack";
        public const string SecondaryMeleeAttackName = "Secondary melee attack";

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual string BaseName { get; set; }

        public Actor BaseActor
            => BaseName == null ? null : Get(BaseName);

        public virtual Species Species
        {
            get { return _species ?? BaseActor?.Species ?? Species.Default; }
            set { _species = value; }
        }

        public virtual SpeciesClass SpeciesClass
        {
            get { return _speciesClass ?? BaseActor?.SpeciesClass ?? SpeciesClass.None; }
            set { _speciesClass = value; }
        }

        public virtual int ArmorClass
        {
            get { return _armorClass ?? BaseActor?.ArmorClass ?? (int)CustomPropertyDescription.ArmorClass.MaxValue; }
            set { _armorClass = value; }
        }

        public int MagicResistance
        {
            get
            {
                return _magicResistance ??
                       BaseActor?.MagicResistance ?? (int)CustomPropertyDescription.MagicResistance.MinValue;
            }
            set { _magicResistance = value; }
        }

        public virtual int MovementDelay
        {
            get { return _movementDelay ?? BaseActor?.MovementDelay ?? 0; }
            set { _movementDelay = value; }
        }

        public virtual Size Size
        {
            get { return _size ?? BaseActor?.Size ?? 0; }
            set { _size = value; }
        }

        /// <summary> 100g units </summary>
        public virtual int Weight
        {
            get { return _weight ?? BaseActor?.Weight ?? 0; }
            set { _weight = value; }
        }

        public virtual int Nutrition
        {
            get { return _nutrition ?? BaseActor?.Nutrition ?? 0; }
            set { _nutrition = value; }
        }

        public virtual Material Material
        {
            get { return _material ?? BaseActor?.Material ?? Material.Flesh; }
            set { _material = value; }
        }

        public virtual ISet<string> SimpleProperties
        {
            get
            {
                if (_simpleProperties != null)
                {
                    return _simpleProperties;
                }
                if (BaseActor != null)
                {
                    return BaseActor.SimpleProperties;
                }
                return _simpleProperties = new HashSet<string>();
            }
            set { _simpleProperties = value; }
        }

        public virtual IDictionary<string, object> ValuedProperties
        {
            get
            {
                if (_valuedProperties != null)
                {
                    return _valuedProperties;
                }
                if (BaseActor != null)
                {
                    return BaseActor.ValuedProperties;
                }
                return _valuedProperties = new Dictionary<string, object>();
            }
            set { _valuedProperties = value; }
        }

        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();

        public virtual Sex Sex { get; set; }
        public virtual byte XPLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual int NextLevelXP { get; set; }
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

        public virtual int? LevelId { get; set; }
        public virtual Level Level { get; set; }
        public virtual byte LevelX { get; set; }
        public virtual byte LevelY { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }

        public virtual int GetEffectiveAC()
        {
            var effectiveAC = ArmorClass;
            var maxAC = (int)CustomPropertyDescription.ArmorClass.MaxValue;
            foreach (var item in Inventory)
            {
                if (item.EquippedSlot != null)
                {
                    effectiveAC += (int)item.ValuedProperties.GetValueOrDefault(
                        nameof(CustomPropertyDescription.ArmorClass), maxAC) - maxAC;
                }
            }

            return effectiveAC;
        }

        protected virtual void RecalculateEffectsAndAbilities()
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

            if (twoHandedWeapon != null
                || mainWeapon != null
                || secondaryWeapon != null)
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
                if (mainWeapon != null
                    && secondaryWeapon != null)
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

        #endregion

        #region Creation

        protected Actor()
        {
        }

        protected Actor(Game game)
            : this()
        {
            Game = game;
            Id = game.NextActorId++;
            Game.Actors.Add(this);
        }

        public virtual Actor Instantiate(Level level, byte x, byte y)
        {
            if (Game != null)
            {
                throw new InvalidOperationException("This actor is already part of a game.");
            }

            var actorInstance = CreateInstance(level.Game);
            actorInstance.LevelX = x;
            actorInstance.LevelY = y;
            actorInstance.Level = level;
            actorInstance.NextActionTick = level.Game.NextPlayerTick;
            level.Actors.Push(actorInstance);
            actorInstance.AddReference();

            actorInstance.BaseName = Name;
            actorInstance.Species = Species;
            actorInstance.SpeciesClass = SpeciesClass;
            actorInstance.ArmorClass = ArmorClass;
            actorInstance.MagicResistance = MagicResistance;
            actorInstance.MovementDelay = MovementDelay;
            actorInstance.Size = Size;
            actorInstance.Weight = Weight;
            actorInstance.Nutrition = Nutrition;
            actorInstance.Material = Material;

            if (actorInstance.SimpleProperties.Contains(nameof(CustomPropertyDescription.Asexuality)))
            {
                actorInstance.Sex = Sex.None;
            }
            else if (actorInstance.SimpleProperties.Contains(nameof(CustomPropertyDescription.Maleness)))
            {
                actorInstance.Sex = Sex.Male;
            }
            else if (actorInstance.SimpleProperties.Contains(nameof(CustomPropertyDescription.Femaleness)))
            {
                actorInstance.Sex = Sex.Female;
            }
            else
            {
                actorInstance.Sex = level.Game.Roll(1, 2) > 1 ? Sex.Female : Sex.Male;
            }

            foreach (var ability in Abilities)
            {
                actorInstance.Abilities.Add(ability.Instantiate(level.Game));
            }

            return actorInstance;
        }

        protected abstract Actor CreateInstance(Game game);

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Actor> AddReference()
        {
            return new TransientReference<Actor>(this);
        }

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
                Game.Delete(this);
            }
        }

        #endregion

        #region Actions

        /// <summary></summary>
        /// <returns>Returns <c>false</c> if the actor hasn't finished their turn.</returns>
        // TODO: return Task
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

        public virtual SenseType CanSense(Item target)
        {
            return SenseType.Sight;
        }

        public virtual bool UseStairs(bool up, bool pretend = false)
        {
            Level moveToLevel;
            byte? moveToLevelX, moveToLevelY;
            if (up)
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

            if (pretend)
            {
                return true;
            }

            if (!moveToLevel.Players.Any())
            {
                // Catch up the level to current turn
                var waitedFor = moveToLevel.Turn();
                Debug.Assert(waitedFor == null);
            }

            // TODO: Shove off any monsters standing on stairs

            NextActionTick += MovementDelay;

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

        public virtual bool Move(Point targetCell, bool pretend = false, bool safe = false)
        {
            if (LevelX == targetCell.X && LevelY == targetCell.Y)
            {
                return true;
            }

            if (MovementDelay == 0)
            {
                return false;
            }

            var conflictingActor = Level.Actors
                .SingleOrDefault(a => (a.LevelX == targetCell.X) && (a.LevelY == targetCell.Y));
            if (conflictingActor != null)
            {
                if (safe)
                {
                    return false;
                }

                return Attack(conflictingActor, pretend);
            }

            if (!((MapFeature)Level.Layout[Level.PointToIndex[targetCell.X, targetCell.Y]]).CanMoveTo())
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: take terrain into account
            NextActionTick += MovementDelay;

            LevelX = targetCell.X;
            LevelY = targetCell.Y;
            var itemsOnNewCell =
                Level.Items.Where(i => (i.LevelX == targetCell.X) && (i.LevelY == targetCell.Y)).ToList();
            foreach (var itemOnNewCell in itemsOnNewCell)
            {
                PickUp(itemOnNewCell);
            }

            ActorMoveEvent.New(this, movee: null, eventOrder: Game.EventOrder++);

            return true;
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

            return ability.Activate(new AbilityActivationContext {Activator = this, Target = victim}, pretend);
        }

        public virtual bool Eat(Item item, bool pretend = false)
        {
            if (item.Type != ItemType.Food)
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
                ChangeCurrentHP(splitItem.Nutrition);

                ItemConsumptionEvent.New(this, splitItem, Game.EventOrder++);

                return true;
            }
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
                if (slot == EquipmentSlot.GraspMainExtremity ||
                    slot == EquipmentSlot.GraspSecondaryExtremity)
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

            RecalculateEffectsAndAbilities();

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
            if (quantity == 0
                || quantity > Gold)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            NextActionTick += MovementDelay;

            Gold -= quantity;
            var item = UnicornHack.Gold.Get().Instantiate(new LevelCell(Level, LevelX, LevelY), quantity).Single();

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

        public virtual bool CanAdd(Item item)
            => true;

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
                DropGold(Gold);
                foreach (var item in Inventory.ToList())
                {
                    Drop(item);
                }
                DeathEvent.New(this, corpse: null, eventOrder: Game.EventOrder++);
                Level.Actors.Remove(this);
                RemoveReference();
                return false;
            }

            if (HP > MaxHP)
            {
                HP = MaxHP;
            }

            return true;
        }

        public virtual Item GetEquippedItem(EquipmentSlot slot)
            => Inventory.FirstOrDefault(item => item.EquippedSlot == slot);

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

        #endregion

        #region Serialization

        public static Actor Get(string name)
            => (Actor)Player.Get(name) ?? Creature.Get(name);

        protected static Dictionary<string, Func<TActor, object, bool>> GetPropertyConditions<TActor>()
            where TActor : Actor
        {
            return new Dictionary<string, Func<TActor, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(BaseName), (o, v) => v != null},
                {nameof(Species), (o, v) => (Species)v != (o.BaseActor?.Species ?? Species.Default)},
                {nameof(SpeciesClass), (o, v) => (SpeciesClass)v != (o.BaseActor?.SpeciesClass ?? SpeciesClass.None)},
                {
                    nameof(ArmorClass),
                    (o, v) => (int)v != (o.BaseActor?.ArmorClass ?? (int)CustomPropertyDescription.ArmorClass.MaxValue)
                },
                {nameof(MagicResistance), (o, v) => (int)v != (o.BaseActor?.MagicResistance ?? 0)},
                {nameof(MovementDelay), (o, v) => (int)v != (o.BaseActor?.MovementDelay ?? 0)},
                {nameof(Weight), (o, v) => (int)v != (o.BaseActor?.Weight ?? 0)},
                {nameof(Size), (o, v) => (Size)v != (o.BaseActor?.Size ?? Size.None)},
                {nameof(Nutrition), (o, v) => (int)v != (o.BaseActor?.Nutrition ?? 0)},
                {nameof(Material), (o, v) => (Material)v != (o.BaseActor?.Material ?? Material.Flesh)},
                {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0},
                {nameof(SimpleProperties), (o, v) => ((ICollection<string>)v).Count != 0},
                {nameof(ValuedProperties), (o, v) => ((IDictionary<string, object>)v).Keys.Count != 0}
            };
        }

        public abstract ICSScriptSerializer GetSerializer();

        #endregion

        public class TickComparer : IComparer<Actor>
        {
            public static readonly TickComparer Instance = new TickComparer();

            private TickComparer()
            {
            }

            public int Compare(Actor x, Actor y)
            {
                return x.NextActionTick - y.NextActionTick;
            }
        }
    }
}
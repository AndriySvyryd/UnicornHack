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
    public abstract class Actor : IItemLocation, ICSScriptSerializable
    {
        #region State

        private Species? _species;
        private SpeciesClass? _speciesClass;
        private byte? _movementRate;
        private Size? _size;
        private int? _weight;
        private int? _nutrition;
        private Material? _material;
        private int? _armorClass;
        private int? _magicResistance;
        private ISet<string> _simpleProperties;
        private IDictionary<string, object> _valuedProperties;

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

        public virtual byte MovementRate
        {
            get { return _movementRate ?? BaseActor?.MovementRate ?? 0; }
            set { _movementRate = value; }
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

        public virtual Ability DefaultAttack { get; set; }
        public virtual Ability MeleeAttack { get; set; }
        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();

        public virtual Sex Sex { get; set; }
        public virtual byte XPLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual int NextLevelXP { get; set; }
        public virtual int MaxHP { get; set; }
        public virtual int HP { get; set; }
        public virtual bool IsAlive => HP > 0;
        public virtual int ActionPoints { get; set; }
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

        public const int ActionPointsPerTurn = 100;

        public const int MaxCarryOverActionPoints = ActionPointsPerTurn;

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
            foreach (var item in Inventory)
            {
                if (item.EquippedSlot != null)
                {
                    if (item.EquippedSlot == EquipmentSlot.GraspBothExtremities)
                    {
                        twoHandedWeapon = item;
                    }
                    else if (item.EquippedSlot == EquipmentSlot.GraspMainExtremity)
                    {
                        mainWeapon = item;
                    }
                    else if (item.EquippedSlot == EquipmentSlot.GraspSecondaryExtremity)
                    {
                        secondaryWeapon = item;
                    }
                }
            }

            if (twoHandedWeapon != null
                || mainWeapon != null
                || secondaryWeapon != null)
            {
                if (MeleeAttack == null)
                {
                    MeleeAttack = new Ability(Game)
                    {
                        Name = "Melee attack",
                        Activation = AbilityActivation.OnTarget,
                        ActionPointCost = 100,
                        Effects = new HashSet<Effect> {new MeleeAttack(Game), new MeleeAttack(Game)}
                    };
                }

                var firstWeaponEffect = MeleeAttack.Effects.OfType<MeleeAttack>().First();
                var secondWeaponEffect = MeleeAttack.Effects.OfType<MeleeAttack>().First(e => e != firstWeaponEffect);

                firstWeaponEffect.Weapon = mainWeapon;
                secondWeaponEffect.Weapon = secondaryWeapon;

                if (DefaultAttack != null)
                {
                    Abilities.Remove(DefaultAttack);
                }
                Abilities.Add(MeleeAttack);
            }
            else
            {
                // TODO: Increase attack speed if both hands free
                if (MeleeAttack != null)
                {
                    Abilities.Remove(MeleeAttack);
                }
                if (DefaultAttack != null)
                {
                    Abilities.Add(DefaultAttack);
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
                throw new InvalidOperationException("This actor is part of a game.");
            }

            var actorInstance = CreateInstance(level.Game);
            actorInstance.LevelX = x;
            actorInstance.LevelY = y;
            actorInstance.Level = level;
            level.Actors.Add(actorInstance);

            actorInstance.BaseName = Name;
            actorInstance.Species = Species;
            actorInstance.SpeciesClass = SpeciesClass;
            actorInstance.ArmorClass = ArmorClass;
            actorInstance.MagicResistance = MagicResistance;
            actorInstance.MovementRate = MovementRate;
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

            if (DefaultAttack != null)
            {
                actorInstance.DefaultAttack = DefaultAttack.Instantiate(level.Game);
            }
            foreach (var ability in Abilities)
            {
                actorInstance.Abilities.Add(ability.Instantiate(level.Game));
            }

            return actorInstance;
        }

        protected abstract Actor CreateInstance(Game game);

        #endregion

        #region Actions

        /// <summary></summary>
        /// <returns>Returns <c>false</c> if the actor hasn't finished their turn.</returns>
        // TODO: return Task
        public abstract bool Act();

        public virtual void Sense(SensoryEvent @event)
        {
            @event.Sensor = this;
            @event.Delete();
        }

        public virtual bool CanAct()
        {
            if (!IsAlive)
            {
                return false;
            }

            ActionPoints = ActionPoints >= MaxCarryOverActionPoints
                ? MaxCarryOverActionPoints
                : ActionPoints;

            return true;
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

            if (!moveToLevel.PlayerCharacters.Any())
            {
                // Catch up the level to current turn
                var waitedFor = moveToLevel.Turn();
                Debug.Assert(waitedFor == null);
            }

            // TODO: Shove off any monsters standing on stairs

            ActionPoints = 0;

            Level.Actors.Remove(this);
            Level = moveToLevel;
            LevelX = moveToLevelX.Value;
            LevelY = moveToLevelY.Value;
            moveToLevel.Actors.Add(this);

            ActorMoveEvent.New(this, movee: null);

            return true;
        }

        public virtual bool Move(Point targetCell, bool pretend = false, bool safe = false)
        {
            if (LevelX == targetCell.X && LevelY == targetCell.Y)
            {
                return true;
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

            ActionPoints -= (Level.DefaultMovementCost * ActionPointsPerTurn) / MovementRate;

            LevelX = targetCell.X;
            LevelY = targetCell.Y;
            var itemsOnNewCell = Level.Items.Where(i => (i.LevelX == targetCell.X) && (i.LevelY == targetCell.Y)).ToList();
            foreach (var itemOnNewCell in itemsOnNewCell)
            {
                PickUp(itemOnNewCell);
            }

            ActorMoveEvent.New(this, movee: null);

            return true;
        }

        public virtual bool Attack(Actor victim, bool pretend = false)
        {
            ActionPoints = 0;
            var ability = Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.OnTarget);
            if (ability == null)
            {
                return false;
            }

            var weapon = ability.Effects.OfType<MeleeAttack>().FirstOrDefault()?.Weapon;
            if (weapon != null)
            {
                ability = weapon.Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.OnMeleeAttack);
            }

            var damage = ability.Effects.OfType<PhysicalDamage>().FirstOrDefault()?.Damage
                         ?? ability.Effects.OfType<ElectricityDamage>().FirstOrDefault()?.Damage
                         ?? ability.Effects.OfType<FireDamage>().FirstOrDefault()?.Damage;
            if (damage == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn;

            if (Game.NextRandom(maxValue: 3) == 0)
            {
                AttackEvent.New(this, victim, ability.Action, hit: false);
                return true;
            }

            AttackEvent.New(this, victim, ability.Action, hit: true, damage: damage.Value, weapon: weapon);
            victim.ChangeCurrentHP(-1 * damage.Value);

            if (!victim.IsAlive)
            {
                XP += victim.XP;
            }

            return true;
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

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn;

            using (var reference = item.Split(1))
            {
                var splitItem = reference.Referenced;
                    ChangeCurrentHP(hp: splitItem.Nutrition);

                ItemConsumptionEvent.New(this, splitItem);

                return true;
            }
        }

        public virtual bool Equip(Item item, bool pretend = false)
        {
            var slot = item.EquipableSlots.FirstOrDefault();
            if (slot == EquipmentSlot.Default)
            {
                return false;
            }

            var equipped = GetEquippedItem(slot);
            if (equipped == item)
            {
                return true;
            }

            if (!item.EquipableSlots.Contains(slot))
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn;

            if (equipped != null)
            {
                Unequip(equipped);
            }

            item.EquippedSlot = slot;
            ItemEquipmentEvent.New(this, item);

            RecalculateEffectsAndAbilities();

            return true;
        }

        public virtual bool Unequip(Item item, bool pretend = false)
        {
            if (item.EquippedSlot == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn;

            item.EquippedSlot = null;
            ItemUnequipmentEvent.New(this, item);

            RecalculateEffectsAndAbilities();

            return true;
        }

        public virtual bool PickUp(Item item, bool pretend = false)
        {
            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn / 10;

            item.MoveTo(this);

            ItemPickUpEvent.New(this, item);

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

            ActionPoints -= quantity;

            Gold -= quantity;
            var item = UnicornHack.Gold.Get().Instantiate(new LevelCell(Level, LevelX, LevelY), quantity).Single();

            ItemDropEvent.New(this, item);

            return true;
        }

        public virtual bool Drop(Item item, bool pretend = false)
        {
            if (pretend)
            {
                return true;
            }

            // TODO: Calculate AP cost
            ActionPoints -= ActionPointsPerTurn / 10;

            item.MoveTo(new LevelCell(Level, LevelX, LevelY));

            ItemDropEvent.New(this, item);

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
            HP += hp;
            if (!IsAlive)
            {
                DropGold(Gold);
                foreach (var item in Inventory.ToList())
                {
                    Drop(item);
                }
                DeathEvent.New(this, corpse: null);
                Level.Actors.Remove(this);
                LevelId = 0;
                return false;
            }

            if (HP > MaxHP)
            {
                HP = MaxHP;
            }

            return true;
        }

        public virtual Item GetEquippedItem(EquipmentSlot slot)
        {
            foreach (var item in Inventory)
            {
                if (item.EquippedSlot == slot)
                {
                    return item;
                }
            }

            return null;
        }

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
                {nameof(MovementRate), (o, v) => (byte)v != (o.BaseActor?.MovementRate ?? 0)},
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
    }
}
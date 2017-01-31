using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Player : Actor
    {
        #region State

        public virtual int Strength { get; set; }
        public virtual int Dexterity { get; set; }
        public virtual int Speed { get; set; }
        public virtual int Constitution { get; set; }
        public virtual int Intelligence { get; set; }
        public virtual int Willpower { get; set; }

        public virtual Skills Skills { get; set; }

        public virtual int MaxEP { get; set; }
        public virtual int EP { get; set; }

        public virtual int NextEventId { get; set; }
        public virtual ICollection<SensoryEvent> SensedEvents { get; set; } = new HashSet<SensoryEvent>();

        public virtual int NextLogEntryId { get; set; }
        public virtual ICollection<LogEntry> Log { get; set; } = new HashSet<LogEntry>();

        public virtual string NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }

        protected override void RecalculateEffectsAndAbilities()
        {
            base.RecalculateEffectsAndAbilities();

            foreach (var unarmedAbility in Abilities.Where(a => a.Name == UnarmedAttackName && a.IsUsable))
            {
                unarmedAbility.Effects.OfType<PhysicalDamage>().Single().Damage = 1 + Skills.Unarmed;
            }

            var mainMeleeAttack = Abilities.FirstOrDefault(a => a.Name == MeleeAttackName);
            if (mainMeleeAttack != null
                && mainMeleeAttack.IsUsable)
            {
                var mainWeapon = mainMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                var meleeSkill = mainWeapon.EquippedSlot == EquipmentSlot.GraspBothExtremities
                    ? Skills.TwoHanded
                    : Skills.OneHanded;
                var secondaryMeleeAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryMeleeAttackName);
                if (secondaryMeleeAttack.IsUsable)
                {
                    meleeSkill = Skills.DualWielding;
                    var secondaryWeapon = secondaryMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                    var secondaryWeaponSkill = 0;
                    if (secondaryWeapon.Type.HasFlag(ItemType.WeaponMeleeShort))
                    {
                        secondaryWeaponSkill = Skills.ShortWeapons;
                    }
                    else if (secondaryWeapon.Type.HasFlag(ItemType.WeaponMeleeMedium))
                    {
                        secondaryWeaponSkill = Skills.MediumWeapons;
                    }
                    else if (secondaryWeapon.Type.HasFlag(ItemType.WeaponMeleeLong))
                    {
                        secondaryWeaponSkill = Skills.LongWeapons;
                    }
                    secondaryMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                        meleeSkill + secondaryWeaponSkill;
                }

                var mainWeaponSkill = 0;
                if (mainWeapon.Type.HasFlag(ItemType.WeaponMeleeShort))
                {
                    mainWeaponSkill = Skills.ShortWeapons;
                }
                else if (mainWeapon.Type.HasFlag(ItemType.WeaponMeleeMedium))
                {
                    mainWeaponSkill = Skills.MediumWeapons;
                }
                else if (mainWeapon.Type.HasFlag(ItemType.WeaponMeleeLong))
                {
                    mainWeaponSkill = Skills.LongWeapons;
                }
                mainMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    meleeSkill + mainWeaponSkill;
            }
        }

        #endregion

        #region Creation

        public Player()
        {
        }

        public Player(Game game)
            : base(game)
        {
        }

        public override Actor Instantiate(Level level, byte x, byte y)
        {
            var player = (Player)base.Instantiate(level, x, y);
            player.XPLevel = 1;
            player.Strength = Strength;
            player.Dexterity = Dexterity;
            player.Speed = Speed;
            player.Constitution = Constitution;
            player.Intelligence = Intelligence;
            player.Willpower = Willpower;

            player.MaxHP = 10 + player.XPLevel*player.Constitution/5;
            player.HP = player.MaxHP;

            player.MaxEP = 10 + player.XPLevel*player.Willpower/5;
            player.EP = player.MaxEP;

            player.NextLevelXP = player.XPLevel*100;

            player.Abilities.Add(new Ability(player.Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnTarget,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                ActionPointCost = 100,
                Effects = new HashSet<Effect> {new MeleeAttack(player.Game), new PhysicalDamage(player.Game) {Damage = 2}}
            });

            player.Abilities.Add(new Ability(player.Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnMeleeAttack,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                Effects = new HashSet<Effect> { new MeleeAttack(player.Game), new PhysicalDamage(player.Game) {Damage = 2}}
            });

            player.Skills = Skills != null ? new Skills(Skills) : new Skills();
            player.RecalculateEffectsAndAbilities();

            Item.Get("carrot").Instantiate(player, quantity: 3);
            Item.Get("mail armor").Instantiate(player);
            Item.Get("long sword").Instantiate(player);
            Item.Get("dagger").Instantiate(player);

            return player;
        }

        protected override Actor CreateInstance(Game game) => new Player(game);

        #endregion

        #region Actions

        public override byte MovementRate => (byte)(BaseActor.MovementRate*Speed/10);

        public override bool Act()
        {
            Debug.Assert(Level != null && Level.LastTurn + 1 == Game.CurrentTurn);

            // TODO: add option to stop here and display current state
            // even if user already provided the next action / cannot perform an action

            if (ActionPoints > 0)
            {
                var action = NextAction;
                var target = NextActionTarget;
                var target2 = NextActionTarget2;
                if (action == null)
                {
                    return false;
                }

                NextAction = null;
                NextActionTarget = null;
                NextActionTarget2 = null;

                Direction? moveDirection = null;
                switch (action)
                {
                    case "N":
                        moveDirection = Direction.North;
                        break;
                    case "S":
                        moveDirection = Direction.South;
                        break;
                    case "W":
                        moveDirection = Direction.West;
                        break;
                    case "E":
                        moveDirection = Direction.East;
                        break;
                    case "NW":
                        moveDirection = Direction.Northwest;
                        break;
                    case "NE":
                        moveDirection = Direction.Northeast;
                        break;
                    case "SW":
                        moveDirection = Direction.Southwest;
                        break;
                    case "SE":
                        moveDirection = Direction.Southeast;
                        break;
                    case "U":
                        UseStairs(up: true);
                        break;
                    case "D":
                        UseStairs(up: false);
                        break;
                    case "H":
                        ActionPoints -= ActionPointsPerTurn;
                        break;
                    case "EAT":
                        Eat(GetItem(target.Value));
                        break;
                    case "DROP":
                        Drop(GetItem(target.Value));
                        break;
                    case "EQUIP":
                        Equip(GetItem(target.Value), (EquipmentSlot)target2.Value);
                        break;
                    case "UNEQUIP":
                        Unequip(GetItem(target.Value));
                        break;
                    default:
                        throw new InvalidOperationException($"Action {action} on character {Name} is invalid.");
                }

                if (moveDirection != null)
                {
                    Move(moveDirection.Value);
                }
            }

            foreach (var @event in SensedEvents.OrderBy(e => e.TurnOrder).ThenBy(e => e.Id).ToList())
            {
                var logEntry = GetLogEntry(@event);
                if (logEntry != null)
                {
                    WriteLog(logEntry);
                }
                SensedEvents.Remove(@event);
                @event.RemoveReference();
            }

            return ActionPoints <= 0;
        }

        public override void Sense(SensoryEvent @event)
        {
            @event.Sensor = this;
            @event.Game = Game;
            unchecked
            {
                @event.Id = NextEventId++;
            }
            @event.AddReference();
            SensedEvents.Add(@event);
        }

        public virtual bool UseStairs(bool up)
        {
            if (base.UseStairs(up, pretend: true))
            {
                return base.UseStairs(up, pretend: false);
            }

            WriteLog(Game.Services.Language.UnableToMove(up ? Direction.Up : Direction.Down));
            return false;
        }

        public virtual bool Move(Direction direction)
        {
            var targetCell = ToLevelCell(Vector.Convert(direction));
            if (targetCell != null
                && base.Move(targetCell.Value, pretend: true))
            {
                return base.Move(targetCell.Value);
            }

            WriteLog(Game.Services.Language.UnableToMove(direction));
            return false;
        }

        public virtual bool Eat(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget());
                return false;
            }

            return Eat(item, pretend: false);
        }

        public virtual bool Drop(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget());
                return false;
            }

            return Drop(item, pretend: false);
        }

        public virtual bool Equip(Item item, EquipmentSlot slot)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget());
                return false;
            }

            return Equip(item, slot, pretend: false);
        }

        public virtual bool Unequip(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget());
                return false;
            }

            return Unequip(item, pretend: false);
        }

        private Item GetItem(int id)
        {
            // TODO: check ground
            return Inventory.SingleOrDefault(i => i.Id == id);
        }

        public virtual void WriteLog(string format, params object[] arguments)
        {
            Log.Add(new LogEntry(this, string.Format(format, arguments)));
        }

        public virtual string GetLogEntry(SensoryEvent @event)
        {
            return GetSpecificLogEntry((dynamic)@event);
        }

        protected virtual string GetSpecificLogEntry(SensoryEvent @event)
        {
            return null;
        }

        protected virtual string GetSpecificLogEntry(AttackEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(DeathEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemEquipmentEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemUnequipmentEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemConsumptionEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemDropEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemPickUpEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        #endregion

        #region Serialization

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\players\");
        private static bool _allLoaded;
        public static readonly int StartingAttributeValue = 10;

        public static Dictionary<string, Player> NameLookup { get; } =
            new Dictionary<string, Player>(StringComparer.Ordinal);

        public static IEnumerable<Player> GetAllPlayerVariants()
        {
            if (!_allLoaded)
            {
                foreach (var file in
                    Directory.EnumerateFiles(BasePath, "*" + CSScriptDeserializer.Extension,
                        SearchOption.AllDirectories))
                {
                    if (!NameLookup.ContainsKey(
                        CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                    {
                        Load(file);
                    }
                }
                _allLoaded = true;
            }

            return NameLookup.Values;
        }

        public new static Player Get(string name)
        {
            Player definition;
            if (NameLookup.TryGetValue(name, out definition))
            {
                return definition;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            if (!File.Exists(path))
            {
                return null;
            }

            return Load(path);
        }

        private static Player Load(string path)
        {
            var player = CSScriptDeserializer.LoadFile<Player>(path);
            NameLookup[player.Name] = player;
            return player;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<Player>(
            GetPropertyConditions<Player>());

        protected new static Dictionary<string, Func<TPlayerCharacter, object, bool>> GetPropertyConditions
            <TPlayerCharacter>()
            where TPlayerCharacter : Player
        {
            var propertyConditions = Actor.GetPropertyConditions<TPlayerCharacter>();
            propertyConditions.Add(nameof(Strength), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Dexterity), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Constitution), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Intelligence), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Willpower), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Speed), (o, v) => (int)v != 0);
            propertyConditions.Add(nameof(Skills), (o, v) => v != null);
            propertyConditions.Add(nameof(SensedEvents), (o, v) => false);
            propertyConditions.Add(nameof(Log), (o, v) => false);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}
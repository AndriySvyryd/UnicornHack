using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Definitions;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Player : Actor
    {
        #region State

        public virtual byte Strength { get; set; }
        public virtual byte Agility { get; set; }
        public virtual byte Quickness { get; set; }
        public virtual byte Constitution { get; set; }
        public virtual byte Intelligence { get; set; }
        public virtual byte Willpower { get; set; }

        public virtual int NextRaceId { get; set; }
        public virtual ICollection<ActivePlayerRace> Races { get; set; } = new HashSet<ActivePlayerRace>();
        public virtual ActivePlayerRace LearningRace => Races.OrderBy(r => XPLevel).ThenBy(r => r.Id).First();

        public virtual int MaxXPLevel { get; set; }
        public virtual byte XPLevel => (byte)Races.Sum(r => r.XPLevel);
        public virtual int XP => LearningRace.XP;
        public virtual int NextLevelXP => LearningRace.NextLevelXP;

        public virtual Skills Skills { get; set; }
        public virtual int UnspentSkillPoints { get; set; }

        public virtual int MaxEP { get; set; }
        public virtual int EP { get; set; }

        public virtual int NextEventId { get; set; }
        public virtual ICollection<SensoryEvent> SensedEvents { get; set; } = new HashSet<SensoryEvent>();

        public virtual int NextLogEntryId { get; set; }
        public virtual ICollection<LogEntry> Log { get; set; } = new HashSet<LogEntry>();

        public virtual string NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }

        public override Actor BaseActor { get; }
        public override int MovementDelay => DefaultActionDelay * 10 / Quickness;

        #endregion

        #region Creation

        public Player()
        {
        }

        public Player(Level level, byte x, byte y)
            : base(level, x, y)
        {
            BaseName = "player";

            UpdateNextLevelXP(PlayerRace.Loader.Get("human").Instantiate(this));

            Abilities.Add(new Ability(Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnTarget,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                DelayTicks = 100,
                Effects = new HashSet<Effect>
                {
                    new MeleeAttack(Game),
                    new PhysicalDamage(Game) {Damage = 2}
                }
            });

            Abilities.Add(new Ability(Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnMeleeAttack,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                Effects = new HashSet<Effect>
                {
                    new MeleeAttack(Game),
                    new PhysicalDamage(Game) {Damage = 2}
                }
            });

            Skills = new Skills();
            RecalculateEffectsAndAbilities();


            MaxHP = 100 + Constitution * 10;
            HP = MaxHP;

            MaxEP = 100 + Willpower * 10;
            EP = MaxEP;

            Item.Loader.Get("mail armor").Instantiate(this);
            Item.Loader.Get("long sword").Instantiate(this);
            Item.Loader.Get("dagger").Instantiate(this);
        }

        #endregion

        #region Actions

        private void UpdateNextLevelXP(ActivePlayerRace race)
        {
            var currentLevel = MaxXPLevel > XPLevel ? race.XPLevel : XPLevel;
            race.NextLevelXP = (int)(75 + (1 + currentLevel) * 25);
        }

        public void AddXP(int xp)
        {
            ChangeCurrentHP(xp);
            var race = LearningRace;
            race.XP += xp;
            if (race.XP >= race.NextLevelXP)
            {
                race.XP -= race.NextLevelXP;
                race.XPLevel++;
                // TODO: Trigger abilities
                if (XPLevel > MaxXPLevel)
                {
                    MaxXPLevel = XPLevel;
                }
                UpdateNextLevelXP(race);
            }
        }

        protected override void RecalculateEffectsAndAbilities()
        {
            base.RecalculateEffectsAndAbilities();

            foreach (var unarmedAbility in Abilities.Where(a => a.Name == UnarmedAttackName && a.IsUsable))
            {
                unarmedAbility.Effects.OfType<PhysicalDamage>().Single().Damage = 1 + Skills.FistWeapons;
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

            // TODO: Calculate attributes, size and other properties
            Strength = StartingAttributeValue;
            Agility = StartingAttributeValue;
            Quickness = StartingAttributeValue;
            Constitution = StartingAttributeValue;
            Intelligence = StartingAttributeValue;
            Willpower = StartingAttributeValue;
        }

        public override bool Act()
        {
            Debug.Assert(Level != null);

            // TODO: add option to stop here and display current state
            // even if user already provided the next action / cannot perform an action

            var initialTick = NextActionTick;
            if (IsAlive)
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
                        NextActionTick += DefaultActionDelay;
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

            foreach (var @event in SensedEvents.OrderBy(e => e.EventOrder).ThenBy(e => e.Id).ToList())
            {
                var logEntry = GetLogEntry(@event);
                if (logEntry != null)
                {
                    WriteLog(logEntry, @event.Tick);
                }
                SensedEvents.Remove(@event);
                @event.RemoveReference();
            }

            return initialTick != NextActionTick;
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

        public override ICSScriptSerializer GetSerializer()
        {
            throw new NotImplementedException();
        }

        public virtual bool UseStairs(bool up)
        {
            if (UseStairs(up, pretend: true))
            {
                return UseStairs(up, pretend: false);
            }

            WriteLog(Game.Services.Language.UnableToMove(up ? Direction.Up : Direction.Down), Level.CurrentTick);
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

            WriteLog(Game.Services.Language.UnableToMove(direction), Level.CurrentTick);
            return false;
        }

        public virtual bool Drop(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget(), Level.CurrentTick);
                return false;
            }

            return Drop(item, pretend: false);
        }

        public virtual bool Equip(Item item, EquipmentSlot slot)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget(), Level.CurrentTick);
                return false;
            }

            return Equip(item, slot, pretend: false);
        }

        public virtual bool Unequip(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget(), Level.CurrentTick);
                return false;
            }

            return Unequip(item, pretend: false);
        }

        private Item GetItem(int id)
        {
            // TODO: check ground
            return Inventory.SingleOrDefault(i => i.Id == id);
        }

        public virtual void WriteLog(string format, int tick, params object[] arguments)
        {
            Log.Add(new LogEntry(this, string.Format(format, arguments), tick));
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

        public static readonly byte StartingAttributeValue = 10;

        #endregion
    }
}
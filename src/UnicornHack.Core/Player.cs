using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Player : Actor
    {
        public int MaxXPLevel { get; set; }
        public byte XPLevel => (byte)Races.Sum(r => r.XPLevel);
        public int XP => LearningRace?.XP ?? 0;
        public int NextLevelXP => LearningRace?.NextLevelXP ?? 0;
        public float LeftoverRegenerationXP { get; set; }
        public Skills Skills { get; set; }
        public int UnspentSkillPoints { get; set; }

        public virtual IEnumerable<ChangedRace> Races
            => ActiveEffects.OfType<ChangedRace>().OrderBy(r => r.XPLevel).ThenBy(r => r.Id);

        public virtual ChangedRace LearningRace => Races.FirstOrDefault();

        public virtual int NextEventId { get; set; }
        public virtual ICollection<SensoryEvent> SensedEvents { get; set; } = new HashSet<SensoryEvent>();

        public virtual int NextLogEntryId { get; set; }
        public virtual ICollection<LogEntry> Log { get; set; } = new HashSet<LogEntry>();

        public virtual string NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }

        public Player()
        {
        }

        public Player(Level level, byte x, byte y) : base(level, x, y)
        {
            BaseName = "player";

            new Ability(Game)
            {
                Name = "become human",
                Activation = AbilityActivation.OnActivation,
                Effects = new HashSet<Effect> {new ChangeRace(Game) {RaceName = "human"}}
            }.Activate(
                new AbilityActivationContext
                {
                    Target = this
                });

            Add(new Ability(Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnTarget,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                Delay = 100,
                Effects = new HashSet<Effect> {new MeleeAttack(Game), new PhysicalDamage(Game)}
            });

            Add(new Ability(Game)
            {
                Name = UnarmedAttackName,
                Activation = AbilityActivation.OnMeleeAttack,
                Action = AbilityAction.Punch,
                FreeSlotsRequired = EquipmentSlot.GraspBothExtremities | EquipmentSlot.GraspSingleExtremity,
                Effects = new HashSet<Effect> {new MeleeAttack(Game), new PhysicalDamage(Game)}
            });

            Skills = new Skills();
            RecalculateAbilities();

            HP = MaxHP;
            EP = MaxEP;

            ItemVariant.Loader.Get("potion of healing").Instantiate(this, quantity: 3);
            ItemVariant.Loader.Get("mail armor").Instantiate(this);
            ItemVariant.Loader.Get("long sword").Instantiate(this);
            ItemVariant.Loader.Get("dagger").Instantiate(this);
        }

        public void AddXP(int xp)
        {
            var regenerationRate = (float)NextLevelXP / (MaxHP * 5);
            var regeneratingXp = xp + LeftoverRegenerationXP;
            var hpRegenerated = (int)Math.Floor(regeneratingXp / regenerationRate);
            LeftoverRegenerationXP = regeneratingXp % regenerationRate;
            ChangeCurrentHP(hpRegenerated);
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
                race.UpdateNextLevelXP();
            }
        }

        public override void RecalculateAbilities()
        {
            base.RecalculateAbilities();

            foreach (var unarmedAbility in Abilities.Where(a => a.Name == UnarmedAttackName && a.IsUsable))
            {
                unarmedAbility.Effects.OfType<PhysicalDamage>().Single().Damage = 5 + 2 * Skills.FistWeapons;
            }

            var mainMeleeAttack = Abilities.FirstOrDefault(a => a.Name == MeleeAttackName);
            if (mainMeleeAttack != null && mainMeleeAttack.IsUsable)
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
                mainMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage = meleeSkill + mainWeaponSkill;
            }

            // TODO: Calculate attributes, size and other properties
            // StartingAttributeValue;

            //MovementDelay = DefaultActionDelay * 10 / Quickness;
            // TODO: adjust current hp/mp to maintain %
            //MaxHP = 100 + Constitution * 10;
            //MaxEP = 100 + Willpower * 10;
            MovementDelay = DefaultActionDelay;
            MaxHP = 100;
            MaxEP = 100;
        }

        public override bool Act()
        {
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
                    Level.RecomputeVisibility(new Point(LevelX, LevelY), Heading, primaryFOV: 1, secondaryFOV: 2);
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
                    case "QUAFF":
                        Quaff(GetItem(target.Value));
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
            if (base.Move(direction, pretend: true))
            {
                return base.Move(direction);
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

        public virtual bool Quaff(Item item)
        {
            if (item == null)
            {
                WriteLog(Game.Services.Language.InvalidTarget(), Level.CurrentTick);
                return false;
            }

            return Quaff(item, pretend: false);
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

        public virtual string GetLogEntry(SensoryEvent @event) => GetSpecificLogEntry((dynamic)@event);

        protected virtual string GetSpecificLogEntry(SensoryEvent @event) => null;

        protected virtual string GetSpecificLogEntry(AttackEvent @event) => Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(DeathEvent @event) => Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(ItemEquipmentEvent @event) =>
            Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(ItemUnequipmentEvent @event) =>
            Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(ItemConsumptionEvent @event) =>
            Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(ItemDropEvent @event) => Game.Services.Language.ToString(@event);

        protected virtual string GetSpecificLogEntry(ItemPickUpEvent @event) => Game.Services.Language.ToString(@event);
    }
}
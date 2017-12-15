using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Effects;
using UnicornHack.Events;
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
            => ActiveEffects.OfType<ChangedRace>().OrderByDescending(r => r.XPLevel).ThenByDescending(r => r.Id).ToList();

        public virtual ChangedRace LearningRace => Races.LastOrDefault();
        public virtual Ability DefaultAttack { get; set; }
        public virtual int NextLogEntryId { get; set; }
        public virtual ObservableSnapshotHashSet<LogEntry> Log { get; set; } = new ObservableSnapshotHashSet<LogEntry>();
        public virtual string NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }
        public virtual int NextEventId { get; set; }

        public virtual ObservableSnapshotHashSet<SensoryEvent> SensedEvents { get; set; }
            = new ObservableSnapshotHashSet<SensoryEvent>();

        public Player()
        {
        }

        public Player(Level level, byte x, byte y) : base(level, x, y)
        {
            BaseName = "player";

            var defaultRaceAbility = new Ability(Game)
            {
                Name = "become human",
                Activation = AbilityActivation.OnActivation,
                Effects = new ObservableSnapshotHashSet<Effect> {new ChangeRace(Game) {RaceName = "human"}}
            };
            var context = new AbilityActivationContext
            {
                Target = this
            };
            using (context)
            {
                defaultRaceAbility.Activate(context);
            }
            defaultRaceAbility.IsUsable = false;

            Skills = new Skills();
            RecalculateWeaponAbilities();

            ItemVariantData.PotionOfHealing.Instantiate(this);
            ItemVariantData.MailArmor.Instantiate(this);
            ItemVariantData.LongSword.Instantiate(this);
            ItemVariantData.Dagger.Instantiate(this);
            ItemVariantData.Shortbow.Instantiate(this);
            ItemVariantData.ThrowingKnives.Instantiate(this);
            ItemVariantData.FireStaff.Instantiate(this);
            ItemVariantData.FreezingFocus.Instantiate(this);
            ItemVariantData.PotionOfOgreness.Instantiate(this);
            ItemVariantData.PotionOfElfness.Instantiate(this);
            ItemVariantData.PotionOfDwarfness.Instantiate(this);
            ItemVariantData.PotionOfExperience.Instantiate(this, quantity: 5);
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
                var leftoverXP = race.XP - race.NextLevelXP;
                race.XP = 0;
                race.XPLevel++;
                // TODO: Trigger abilities and fire an event
                if (XPLevel > MaxXPLevel)
                {
                    MaxXPLevel = XPLevel;
                }
                race.UpdateNextLevelXP();
                AddXP(leftoverXP);
            }
        }

        public override bool RecalculateWeaponAbilities()
        {
            if (!base.RecalculateWeaponAbilities())
            {
                return false;
            }

            // TODO: Calculate skill effects properly

            var primaryMeleeWeaponAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryMeleeWeaponAttackName);
            var primaryMeleeWeapon = primaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon;
            var primaryMeleeWeaponSkill = GetMeleeSkill(primaryMeleeWeapon);
            primaryMeleeWeaponAttack.Effects.OfType<PhysicalDamage>().Single().Damage = primaryMeleeWeaponSkill ?? 0;

            var secondaryMeleeWeaponAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryMeleeWeaponAttackName);
            var secondaryMeleeWeapon = secondaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon;
            var secondaryMeleeWeaponSkill = GetMeleeSkill(secondaryMeleeWeapon);
            secondaryMeleeWeaponAttack.Effects.OfType<PhysicalDamage>().Single().Damage =  secondaryMeleeWeaponSkill ?? 0;

            DefaultAttack = null;
            var secondaryMeleeAttack = Abilities.First(a => a.Name == SecondaryMeleeAttackName);
            if (secondaryMeleeAttack.IsUsable)
            {
                secondaryMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage = Skills.OneHanded;
                if (secondaryMeleeWeaponSkill != null)
                {
                    DefaultAttack = secondaryMeleeAttack;
                }
            }

            var primaryMeleeAttack = Abilities.First(a => a.Name == PrimaryMeleeAttackName);
            if (primaryMeleeAttack.IsUsable)
            {
                primaryMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    primaryMeleeWeapon?.EquippedSlot == EquipmentSlot.GraspBothExtremities
                        ? Skills.TwoHanded
                        : Skills.OneHanded;
                if (primaryMeleeWeaponSkill != null
                    && (primaryMeleeWeapon != null || DefaultAttack == null))
                {
                    DefaultAttack = primaryMeleeAttack;
                }
            }

            var doubleMeleeAttack = Abilities.First(a => a.Name == DoubleMeleeAttackName);
            if (doubleMeleeAttack.IsUsable)
            {
                doubleMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage = Skills.DualWielding;
                if (primaryMeleeWeaponSkill != null && secondaryMeleeWeaponSkill != null)
                {
                    DefaultAttack = doubleMeleeAttack;
                }
            }

            var primaryRangedWeaponAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryRangedWeaponAttackName);
            var primaryRangedWeapon = primaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon;
            var primaryRangedSkill = GetRangedSkill(primaryRangedWeapon);
            primaryRangedWeaponAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                primaryRangedSkill ?? Skills.Thrown;

            var secondaryRangedWeaponAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryRangedWeaponAttackName);
            var secondaryRangedWeapon =
                secondaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon;
            var secondaryRangedSkill = GetRangedSkill(secondaryRangedWeapon);
            secondaryRangedWeaponAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                secondaryRangedSkill ?? Skills.Thrown;

            var secondaryRangedAttack = Abilities.First(a => a.Name == SecondaryRangedAttackName);
            if (secondaryRangedAttack.IsUsable)
            {
                secondaryRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage = Skills.OneHanded;
                if (secondaryRangedSkill != null)
                {
                    DefaultAttack = secondaryRangedAttack;
                }
            }

            var primaryRangedAttack = Abilities.First(a => a.Name == PrimaryRangedAttackName);
            if (primaryRangedAttack.IsUsable)
            {
                primaryRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    primaryRangedWeapon?.EquippedSlot == EquipmentSlot.GraspBothExtremities
                        ? Skills.TwoHanded
                        : Skills.OneHanded;
                if (primaryRangedSkill != null)
                {
                    DefaultAttack = primaryRangedAttack;
                }
            }

            var doubleRangedAttack = Abilities.FirstOrDefault(a => a.Name == DoubleRangedAttackName);
            if (doubleRangedAttack.IsUsable)
            {
                doubleRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage = Skills.DualWielding;
                if (primaryRangedSkill != null && secondaryRangedSkill != null)
                {
                    DefaultAttack = doubleRangedAttack;
                }
            }

            return true;
        }

        private int? GetMeleeSkill(Item weapon)
        {
            if (weapon == null)
            {
                // TODO: Calculate h2h damage
                return 20;
            }
            if ((weapon.Type & ItemType.WeaponMeleeFist) != 0)
            {
                return Skills.FistWeapons;
            }
            if ((weapon.Type & ItemType.WeaponMeleeShort) != 0)
            {
                return Skills.ShortWeapons;
            }
            if ((weapon.Type & ItemType.WeaponMeleeMedium) != 0)
            {
                return Skills.MediumWeapons;
            }
            if ((weapon.Type & ItemType.WeaponMeleeLong) != 0)
            {
                return Skills.LongWeapons;
            }
            if ((weapon.Type & ItemType.WeaponMagicFocus) != 0)
            {
                return Skills.MeleeMagicWeapons;
            }

            return null;
        }

        private int? GetRangedSkill(Item weapon)
        {
            if (weapon == null)
            {
                return 0;
            }
            if ((weapon.Type & ItemType.WeaponRangedThrown) != 0)
            {
                return Skills.Thrown;
            }
            if ((weapon.Type & ItemType.WeaponRangedBow) != 0)
            {
                return Skills.Bows;
            }
            if ((weapon.Type & ItemType.WeaponRangedCrossbow) != 0)
            {
                return Skills.Crossbows;
            }
            if ((weapon.Type & ItemType.WeaponRangedSlingshot) != 0)
            {
                return Skills.Slingshots;
            }
            if ((weapon.Type & ItemType.WeaponMagicStaff) != 0)
            {
                return Skills.RangedMagicWeapons;
            }

            return null;
        }

        public override bool Act()
        {
            // TODO: add option to stop here and display current state
            // even if user already provided the next action / cannot perform an action

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
                    case "MAKEDEFAULT":
                        DefaultAttack = Abilities.First(a => a.Id == target.Value);
                        break;
                    default:
                        throw new InvalidOperationException($"Action {action} on character {Name} is invalid.");
                }

                if (moveDirection != null)
                {
                    Move(moveDirection.Value);
                }
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

        protected override bool? HandleBlockingActor(Actor actor, bool pretend)
        {
            if (pretend)
            {
                return true;
            }

            var context = new AbilityActivationContext
            {
                Activator = this,
                Target = actor
            };
            using (context)
            {
                return DefaultAttack.Activate(context);
            }
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
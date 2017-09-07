using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Data.Properties;
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

        public virtual Ability DefaultAttack { get; set; }

        public virtual int NextEventId { get; set; }
        public virtual ICollection<SensoryEvent> SensedEvents { get; set; } = new HashSet<SensoryEvent>();

        public virtual int NextLogEntryId { get; set; }
        public virtual ICollection<LogEntry> Log { get; set; } = new HashSet<LogEntry>();

        public virtual string NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }

        private AbilityStatus _abilityStatus = AbilityStatus.Default;

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

            Skills = new Skills();
            RecalculateWeaponAbilities();

            ItemVariant.Loader.Get("potion of healing").Instantiate(this, quantity: 3);
            ItemVariant.Loader.Get("mail armor").Instantiate(this);
            ItemVariant.Loader.Get("long sword").Instantiate(this);
            ItemVariant.Loader.Get("dagger").Instantiate(this);
            ItemVariant.Loader.Get("shortbow").Instantiate(this);
            ItemVariant.Loader.Get("throwing knives").Instantiate(this);
            ItemVariant.Loader.Get("fire staff").Instantiate(this);
            ItemVariant.Loader.Get("freezing focus").Instantiate(this);
            ItemVariant.Loader.Get("potion of ogreness").Instantiate(this);
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

        public override void RecalculateWeaponAbilities()
        {
            if (_abilityStatus == AbilityStatus.BeingApplied)
            {
                _abilityStatus = AbilityStatus.Invalid;
                return;
            }

            base.RecalculateWeaponAbilities();

            // TODO: Calculate skill effects properly

            var secondaryMeleeAttack = Abilities.First(a => a.Name == SecondaryMeleeAttackName);
            if (secondaryMeleeAttack.IsUsable)
            {
                var techniqueSkill = Skills.OneHanded;
                var weapon = secondaryMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                var weaponSkill = GetMeleeSkill(weapon);
                secondaryMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage = techniqueSkill + weaponSkill ?? GetWeightDamage(weapon);
                if (weaponSkill != null)
                {
                    DefaultAttack = secondaryMeleeAttack;
                }
            }

            var primaryMeleeAttack = Abilities.First(a => a.Name == PrimaryMeleeAttackName);
            if (primaryMeleeAttack.IsUsable)
            {
                var weapon = primaryMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                var techniqueSkill = weapon?.EquippedSlot == EquipmentSlot.GraspBothExtremities
                    ? Skills.TwoHanded
                    : Skills.OneHanded;
                var weaponSkill = GetMeleeSkill(weapon);
                primaryMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage = techniqueSkill + weaponSkill ?? GetWeightDamage(weapon);
                if (weaponSkill != null)
                {
                    DefaultAttack = primaryMeleeAttack;
                }
            }

            var doubleMeleeAttack = Abilities.First(a => a.Name == DoubleMeleeAttackName);
            var additionalMeleeAttack = Abilities.First(a => a.Name == AdditionalMeleeAttackName);
            if (doubleMeleeAttack.IsUsable
                && additionalMeleeAttack.IsUsable)
            {
                var techniqueSkill = Skills.DualWielding;

                var mainWeapon = doubleMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                var mainWeaponSkill = GetMeleeSkill(mainWeapon);
                doubleMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + mainWeaponSkill ?? GetWeightDamage(mainWeapon);
                var additionalWeapon = additionalMeleeAttack.Effects.OfType<MeleeAttack>().Single().Weapon;
                var additionalWeaponSkill = GetMeleeSkill(additionalWeapon);
                additionalMeleeAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + additionalWeaponSkill ?? GetWeightDamage(additionalWeapon);
                if (mainWeaponSkill != null && additionalWeaponSkill != null)
                {
                    DefaultAttack = doubleMeleeAttack;
                }
            }

            var secondaryRangedAttack = Abilities.First(a => a.Name == SecondaryRangedAttackName);
            if (secondaryRangedAttack.IsUsable)
            {
                var techniqueSkill = Skills.OneHanded;
                var weaponSkill = GetRangedSkill(secondaryRangedAttack.Effects.OfType<RangeAttack>().Single().Weapon);
                secondaryRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + weaponSkill ?? Skills.Thrown;
                if (weaponSkill != null)
                {
                    DefaultAttack = secondaryRangedAttack;
                }
            }

            var primaryRangedAttack = Abilities.First(a => a.Name == PrimaryRangedAttackName);
            if (primaryRangedAttack.IsUsable)
            {
                var weapon = primaryRangedAttack.Effects.OfType<RangeAttack>().Single().Weapon;
                var techniqueSkill = weapon?.EquippedSlot == EquipmentSlot.GraspBothExtremities
                    ? Skills.TwoHanded
                    : Skills.OneHanded;
                var weaponSkill = GetRangedSkill(weapon);
                primaryRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + weaponSkill ?? Skills.Thrown;
                if (weaponSkill != null)
                {
                    DefaultAttack = primaryRangedAttack;
                }
            }

            var doubleRangedAttack = Abilities.FirstOrDefault(a => a.Name == DoubleRangedAttackName);
            var additionalRangedAttack = Abilities.First(a => a.Name == AdditionalRangedAttackName);
            if (doubleRangedAttack.IsUsable
                && additionalRangedAttack.IsUsable)
            {
                var techniqueSkill = Skills.DualWielding;

                var mainWeaponSkill = GetRangedSkill(doubleRangedAttack.Effects.OfType<RangeAttack>().Single().Weapon);
                doubleRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + mainWeaponSkill ?? Skills.Thrown;
                var additionalWeaponSkill =
                    GetRangedSkill(additionalRangedAttack.Effects.OfType<RangeAttack>().Single().Weapon);
                additionalRangedAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                    techniqueSkill + additionalWeaponSkill ?? Skills.Thrown;
                if (mainWeaponSkill != null
                    && additionalWeaponSkill != null)
                {
                    DefaultAttack = doubleRangedAttack;
                }
            }

            _abilityStatus = AbilityStatus.Default;
        }

        private int? GetMeleeSkill(Item weapon)
        {
            if (weapon == null)
            {
                // TODO: Calculate h2h damage
                return 20;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMeleeFist))
            {
                return Skills.FistWeapons;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMeleeShort))
            {
                return Skills.ShortWeapons;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMeleeMedium))
            {
                return Skills.MediumWeapons;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMeleeLong))
            {
                return Skills.LongWeapons;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMagicFocus))
            {
                return Skills.MeleeMagicWeapons;
            }

            return null;
        }

        private int? GetRangedSkill(Item weapon)
        {
            if (weapon.Type.HasFlag(ItemType.WeaponRangedThrown))
            {
                return Skills.Thrown;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponRangedBow))
            {
                return Skills.Bows;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponRangedCrossbow))
            {
                return Skills.Crossbows;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponRangedSlingshot))
            {
                return Skills.Slingshots;
            }
            if (weapon.Type.HasFlag(ItemType.WeaponMagicStaff))
            {
                return Skills.RangedMagicWeapons;
            }

            return null;
        }

        private int GetWeightDamage(Item weapon)
            => weapon.GetProperty<int>(PropertyData.Weight.Name);

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

        protected override bool? HandleBlockingActor(Actor actor, bool pretend)
        {
            if (pretend)
            {
                return true;
            }

            if (_abilityStatus == AbilityStatus.Default)
            {
                _abilityStatus = AbilityStatus.BeingApplied;
            }

            var result = DefaultAttack.Activate(new AbilityActivationContext
            {
                Activator = this,
                Target = actor,
                IsAttack = true
            });

            if (_abilityStatus == AbilityStatus.BeingApplied)
            {
                _abilityStatus = AbilityStatus.Default;
            }
            else if(_abilityStatus == AbilityStatus.Invalid)
            {
                RecalculateWeaponAbilities();
            }

            return result;
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

        private enum AbilityStatus
        {
            Default = 0,
            Invalid,
            BeingApplied
        }
    }
}
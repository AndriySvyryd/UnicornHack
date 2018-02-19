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
        public string PlayerName { get; set; }
        public int MaxXPLevel { get; set; }
        public byte XPLevel => (byte)Races.Sum(r => r.XPLevel);
        public int XP => LearningRace?.XP ?? 0;
        public int NextLevelXP => LearningRace?.NextLevelXP ?? 0;
        public float LeftoverRegenerationXP { get; set; }
        public Skills Skills { get; set; }
        public int UnspentSkillPoints { get; set; }

        public virtual IEnumerable<ChangedRace> Races
            => ActiveEffects.OfType<ChangedRace>().OrderByDescending(r => r.XPLevel).ThenByDescending(r => r.Id)
                .ToList();

        public virtual ChangedRace LearningRace => Races.LastOrDefault();
        public virtual int? DefaultAttackId { get; set; }
        public virtual Ability DefaultAttack { get; set; }
        public virtual int NextLogEntryId { get; set; }

        public virtual ObservableSnapshotHashSet<LogEntry> Log { get; set; } =
            new ObservableSnapshotHashSet<LogEntry>();

        public virtual PlayerAction? NextAction { get; set; }
        public virtual int? NextActionTarget { get; set; }
        public virtual int? NextActionTarget2 { get; set; }
        public virtual int NextCommandId { get; set; }

        public virtual ObservableSnapshotHashSet<PlayerCommand> CommandHistory { get; set; } =
            new ObservableSnapshotHashSet<PlayerCommand>();

        public virtual int NextQueuedCommandId { get; set; }

        public virtual ObservableSnapshotHashSet<QueuedCommand> QueuedCommands { get; set; } =
            new ObservableSnapshotHashSet<QueuedCommand>();

        public virtual int NextEventId { get; set; }

        public virtual ObservableSnapshotHashSet<SensoryEvent> SensedEvents { get; set; }
            = new ObservableSnapshotHashSet<SensoryEvent>();

        public Player()
        {
        }

        public Player(Level level, byte x, byte y) : base(level, x, y)
        {
            VariantName = "player";

            var defaultRaceAbility = new Ability(Game)
            {
                Name = "become human",
                Activation = AbilityActivation.OnActivation,
                Effects = new ObservableSnapshotHashSet<Effect> {new ChangeRace(Game) {RaceName = "human"}}
            };
            var context = new AbilityActivationContext
            {
                TargetEntity = this
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
            var regenerationRate = (float)NextLevelXP / (MaxHP * 4);
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
            secondaryMeleeWeaponAttack.Effects.OfType<PhysicalDamage>().Single().Damage =
                secondaryMeleeWeaponSkill ?? 0;

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
                return 0;
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
            var result = ActWithoutFOV();
            if (!result)
            {
                GetFOV();
            }

            return result;
        }

        private bool ActWithoutFOV()
        {
            var attacked = false;
            foreach (var @event in SensedEvents.OrderBy(e => e.EventOrder).ThenBy(e => e.Id).ToList())
            {
                var logEntry = GetLogEntry(@event);
                if (logEntry != null)
                {
                    WriteLog(logEntry, @event.Tick);
                }

                if (@event is AttackEvent attack
                    && attack.Victim == this)
                {
                    attacked = true;
                }

                SensedEvents.Remove(@event);
                @event.RemoveReference();
            }

            var initialTick = NextActionTick;
            if (!IsAlive)
            {
                return false;
            }

            QueuedCommand queuedCommand = null;
            var action = NextAction;
            var target = NextActionTarget;
            var target2 = NextActionTarget2;
            if (action == null)
            {
                if (QueuedCommands.Count == 0)
                {
                    return false;
                }

                // TODO: add an option to stop here and display current state and allow to cancel
                // if user already provided the next action

                queuedCommand = QueuedCommands.OrderBy(c => c.Id).First();
                QueuedCommands.Remove(queuedCommand);

                if ((Level.Actors.Any(a => LevelCell.DistanceTo(a.LevelCell) <= 2 && a is Creature)
                     || attacked))
                {
                    // TODO: only cancel some queued commands
                    return false;
                }

                action = queuedCommand.Action;
                target = queuedCommand.Target;
                target2 = queuedCommand.Target2;
            }
            else
            {
                CommandHistory.Add(new PlayerCommand(this, Level.CurrentTick, action.Value, target, target2));

                NextAction = null;
                NextActionTarget = null;
                NextActionTarget2 = null;
            }

            Direction? moveDirection = null;
            switch (action)
            {
                case PlayerAction.Wait:
                    NextActionTick += DefaultActionDelay;
                    break;
                case PlayerAction.MoveOneCell:
                    moveDirection = (Direction)target.Value;
                    switch (moveDirection)
                    {
                        case Direction.Down:
                            UseStairs(up: false);
                            moveDirection = null;
                            break;
                        case Direction.Up:
                            UseStairs(up: true);
                            moveDirection = null;
                            break;
                    }

                    break;
                case PlayerAction.MoveToCell:
                    var targetCell = Point.Unpack(target).Value;

                    var direction = Level.GetFirstStepFromShortestPath(LevelCell, targetCell, Heading);
                    if (direction == null)
                    {
                        WriteLog("No path to target!", Level.CurrentTick);
                        break;
                    }

                    if (Move(direction.Value)
                        && LevelCell != targetCell)
                    {
                        QueuedCommands.Add(queuedCommand ??
                                           new QueuedCommand(this, Level.CurrentTick, PlayerAction.MoveToCell, target,
                                               target2));
                    }

                    break;
                case PlayerAction.DropItem:
                    Drop(GetItem(target.Value));
                    break;
                case PlayerAction.EquipItem:
                    Equip(GetItem(target.Value), (EquipmentSlot)target2.Value);
                    break;
                case PlayerAction.UnequipItem:
                    Unequip(GetItem(target.Value));
                    break;
                case PlayerAction.ActivateItem:
                    Quaff(GetItem(target.Value));
                    break;
                case PlayerAction.ChooseDefaultAttack:
                    DefaultAttack = Abilities.First(
                        a => a.Id == target.Value && a.IsUsable && a.Activation == AbilityActivation.OnTarget);
                    break;
                case PlayerAction.PerformDefaultAttack:
                    ActivateAbility(DefaultAttack, target);
                    break;
                case PlayerAction.ActivateAbility:
                    ActivateAbility(Abilities.First(a => a.Id == target.Value), target2);
                    break;
                default:
                    throw new InvalidOperationException($"Action {action} on character {Name} is invalid.");
            }

            if (moveDirection != null)
            {
                Move(moveDirection.Value);
            }

            return initialTick != NextActionTick;
        }

        public override byte[] GetFOV()
            => Level.RecomputeVisibility(LevelCell, Heading, primaryFOV: 1, secondaryFOV: 2);

        public override void Sense(SensoryEvent @event)
        {
            @event.Sensor = this;
            @event.Game = Game;
            unchecked
            {
                @event.Id = ++NextEventId;
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

        protected override void ActivateAbility(Ability ability, int? target)
        {
            if (ability.Activation == AbilityActivation.OnTarget)
            {
                Point targetCell;
                Actor targetActor;
                if (target.Value < 0)
                {
                    var id = -target.Value;
                    targetActor = Level.Actors.FirstOrDefault(a => a.Id == id);
                    if (targetActor == null
                        || !targetActor.IsAlive)
                    {
                        return;
                    }

                    targetCell = targetActor.LevelCell;
                }
                else
                {
                    targetCell = Point.Unpack(target).Value;
                    targetActor = Level.Actors.FirstOrDefault(a => a.LevelCell == targetCell);
                }

                var shouldMoveCloser = false;
                switch (ability.TargetingType)
                {
                    case TargetingType.AdjacentSingle:
                    case TargetingType.AdjacentArc:
                        shouldMoveCloser = LevelCell.DistanceTo(targetCell) > 1;
                        break;
                    case TargetingType.Projectile:
                    case TargetingType.GuidedProjectile:
                    case TargetingType.LineOfSight:
                    case TargetingType.Beam:
                        shouldMoveCloser = GetFOV()[Level.PointToIndex[targetCell.X, targetCell.Y]] == 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (shouldMoveCloser)
                {
                    var direction = Level.GetFirstStepFromShortestPath(LevelCell, targetCell, Heading);
                    if (direction == null)
                    {
                        WriteLog("No path to target!", Level.CurrentTick);
                        return;
                    }

                    if (Move(direction.Value))
                    {
                        if (targetActor != null)
                        {
                            target = -targetActor.Id;
                        }

                        QueuedCommands.Add(new QueuedCommand(
                            this, Level.CurrentTick, PlayerAction.ActivateAbility, ability.Id, target));
                    }

                    return;
                }
            }

            base.ActivateAbility(ability, target);
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
                TargetEntity = actor
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

        // TODO: avoid dynamic dispatch
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
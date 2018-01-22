using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Abilities;
using UnicornHack.Data.Properties;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class Actor : Entity, IItemLocation, IReferenceable
    {
        public const int DefaultActionDelay = 100;
        public const string InnateAbilityName = "innate";
        public const string AttributedAbilityName = "attributed";

        public const string PrimaryMeleeWeaponAttackName = "primary melee weapon attack";
        public const string SecondaryMeleeWeaponAttackName = "secondary melee weapon attack";
        public const string PrimaryMeleeAttackName = "primary melee attack";
        public const string SecondaryMeleeAttackName = "secondary melee attack";
        public const string DoubleMeleeAttackName = "double melee attack";

        public const string PrimaryRangedWeaponAttackName = "primary ranged weapon attack";
        public const string SecondaryRangedWeaponAttackName = "secondary ranged weapon attack";
        public const string PrimaryRangedAttackName = "primary ranged attack";
        public const string SecondaryRangedAttackName = "secondary ranged attack";
        public const string DoubleRangedAttackName = "double ranged attack";

        private bool _weaponAbilitiesOutdated;

        public virtual Direction Heading { get; set; }

        // TODO: make these properties dynamic
        public Species Species { get; set; }

        public SpeciesClass SpeciesClass { get; set; }
        public Sex Sex { get; set; }
        public int MovementDelay { get; set; }
        public virtual int? NaturalWeaponId { get; set; }
        public virtual Item NaturalWeapon { get; set; }
        public int MaxHP => GetProperty<int>(PropertyData.HitPointMaximum.Name);
        public int HP => GetProperty<int>(PropertyData.HitPoints.Name);
        public bool IsAlive => HP > 0;
        public int MaxEP => GetProperty<int>(PropertyData.EnergyPointMaximum.Name);
        public int EP => GetProperty<int>(PropertyData.EnergyPoints.Name);

        /// <summary>
        ///     Warning: this should only be updated when this actor is acting
        /// </summary>
        public virtual int NextActionTick { get; set; }

        public virtual int Gold { get; set; }
        IEnumerable<Item> IItemLocation.Items => Inventory;
        public virtual ObservableSnapshotHashSet<Item> Inventory { get; } = new ObservableSnapshotHashSet<Item>();

        private static readonly Dictionary<string, List<object>> PropertyListeners =
            new Dictionary<string, List<object>>();

        static Actor()
        {
            AddPropertyListener<int>(PropertyData.HitPointMaximum.Name, (a, o, n) => a.OnMaxHPChanged(o, n));
            AddPropertyListener<int>(PropertyData.EnergyPointMaximum.Name, (a, o, n) => a.OnMaxEPChanged(o, n));
            AddPropertyListener<int>(PropertyData.Constitution.Name, (a, o, n) => a.OnConstitutionChanged(o, n));
            AddPropertyListener<int>(PropertyData.Willpower.Name, (a, o, n) => a.OnWillpowerChanged(o, n));
            AddPropertyListener<int>(PropertyData.Quickness.Name, (a, o, n) => a.OnQuicknessChanged(o, n));
        }

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

            AddAttributeAbility();
            if (CanUseWeapons())
            {
                NaturalWeapon = new Item(Game)
                {
                    VariantName = "",
                    Type = ItemType.WeaponMeleeFist,
                    Material = Material.Flesh
                }.AddReference().Referenced;

                NaturalWeapon.Add(new Ability(Game)
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    // TODO: Use correct action for claws, etc.
                    Action = AbilityAction.Punch,
                    // TODO: Calculate h2h damage
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) { Damage = 20 } }
                });
            }
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
                NaturalWeapon.RemoveReference();

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

        private void AddAttributeAbility()
        {
            var ability = new Ability(Game)
            {
                Name = AttributedAbilityName,
                Activation = AbilityActivation.Always,
                Effects = new ObservableSnapshotHashSet<Effect>
                {
                    new ChangeProperty<int>(Game)
                    {
                        PropertyName = PropertyData.HitPointMaximum.Name,
                        Function = ValueCombinationFunction.Sum
                    },
                    new ChangeProperty<int>(Game)
                    {
                        PropertyName = PropertyData.EnergyPointMaximum.Name,
                        Function = ValueCombinationFunction.Sum
                    }
                }
            };

            Add(ability);

            var constitution = GetProperty<int>(PropertyData.Constitution.Name);
            OnConstitutionChanged(constitution, constitution);

            var willpower = GetProperty<int>(PropertyData.Willpower.Name);
            OnWillpowerChanged(willpower, willpower);

            var quickness = GetProperty<int>(PropertyData.Quickness.Name);
            OnQuicknessChanged(quickness, quickness);
        }

        public override void OnAbilityActivated(Ability ability)
        {
            base.OnAbilityActivated(ability);

            if (AbilitiesBeingActivated == 0
                && _weaponAbilitiesOutdated)
            {
                RecalculateWeaponAbilities();
            }
        }

        public virtual bool RecalculateWeaponAbilities()
        {
            if (AbilitiesBeingActivated > 0)
            {
                _weaponAbilitiesOutdated = true;
                return false;
            }

            _weaponAbilitiesOutdated = false;

            var canUseWeapons = CanUseWeapons();

            var primaryMeleeWeaponAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryMeleeWeaponAttackName);
            if (primaryMeleeWeaponAttack == null && canUseWeapons)
            {
                primaryMeleeWeaponAttack = new Ability(Game)
                {
                    Name = PrimaryMeleeWeaponAttackName,
                    Triggers = new ObservableSnapshotHashSet<Trigger> { new MeleeWeaponTrigger(Game) },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.AdjacentSingle,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(primaryMeleeWeaponAttack);
            }

            var secondaryMeleeWeaponAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryMeleeWeaponAttackName);
            if (secondaryMeleeWeaponAttack == null && canUseWeapons)
            {
                secondaryMeleeWeaponAttack = new Ability(Game)
                {
                    Name = SecondaryMeleeWeaponAttackName,
                    Triggers = new ObservableSnapshotHashSet<Trigger> { new MeleeWeaponTrigger(Game) },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.AdjacentSingle,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(secondaryMeleeWeaponAttack);
            }

            var primaryMeleeAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryMeleeAttackName);
            if (primaryMeleeAttack == null && canUseWeapons)
            {
                primaryMeleeAttack = new Ability(Game)
                {
                    Name = PrimaryMeleeAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = primaryMeleeWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.AdjacentSingle,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(primaryMeleeAttack);
            }

            var secondaryMeleeAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryMeleeAttackName);
            if (secondaryMeleeAttack == null && canUseWeapons)
            {
                secondaryMeleeAttack = new Ability(Game)
                {
                    Name = SecondaryMeleeAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = secondaryMeleeWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.AdjacentSingle,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(secondaryMeleeAttack);
            }

            var doubleMeleeAttack = Abilities.FirstOrDefault(a => a.Name == DoubleMeleeAttackName);
            if (doubleMeleeAttack == null && canUseWeapons)
            {
                doubleMeleeAttack = new Ability(Game)
                {
                    Name = DoubleMeleeAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = primaryMeleeWeaponAttack.AddReference().Referenced},
                        new AbilityTrigger(Game) {AbilityToTrigger = secondaryMeleeWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new PhysicalDamage(Game)
                    },
                    TargetingType = TargetingType.AdjacentSingle,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(doubleMeleeAttack);
            }

            var primaryRangedWeaponAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryRangedWeaponAttackName);
            if (primaryRangedWeaponAttack == null && canUseWeapons)
            {
                primaryRangedWeaponAttack = new Ability(Game)
                {
                    Name = PrimaryRangedWeaponAttackName,
                    Triggers = new ObservableSnapshotHashSet<Trigger> { new RangedWeaponTrigger(Game) },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.Beam,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(primaryRangedWeaponAttack);
            }

            var secondaryRangedWeaponAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryRangedWeaponAttackName);
            if (secondaryRangedWeaponAttack == null && canUseWeapons)
            {
                secondaryRangedWeaponAttack = new Ability(Game)
                {
                    Name = SecondaryRangedWeaponAttackName,
                    Triggers = new ObservableSnapshotHashSet<Trigger> { new RangedWeaponTrigger(Game) },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.Beam,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(secondaryRangedWeaponAttack);
            }

            var primaryRangedAttack = Abilities.FirstOrDefault(a => a.Name == PrimaryRangedAttackName);
            if (primaryRangedAttack == null && canUseWeapons)
            {
                primaryRangedAttack = new Ability(Game)
                {
                    Name = PrimaryRangedAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = primaryRangedWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.Beam,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(primaryRangedAttack);
            }

            var secondaryRangedAttack = Abilities.FirstOrDefault(a => a.Name == SecondaryRangedAttackName);
            if (secondaryRangedAttack == null && canUseWeapons)
            {
                secondaryRangedAttack = new Ability(Game)
                {
                    Name = SecondaryRangedAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = secondaryRangedWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect> { new PhysicalDamage(Game) },
                    TargetingType = TargetingType.Beam,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(secondaryRangedAttack);
            }

            var doubleRangedAttack = Abilities.FirstOrDefault(a => a.Name == DoubleRangedAttackName);
            if (doubleRangedAttack == null && canUseWeapons)
            {
                doubleRangedAttack = new Ability(Game)
                {
                    Name = DoubleRangedAttackName,
                    Activation = AbilityActivation.OnTarget,
                    // TODO: Calculate proper delay
                    Delay = 100,
                    Triggers = new ObservableSnapshotHashSet<Trigger>
                    {
                        new AbilityTrigger(Game) {AbilityToTrigger = primaryRangedWeaponAttack.AddReference().Referenced},
                        new AbilityTrigger(Game) {AbilityToTrigger = secondaryRangedWeaponAttack.AddReference().Referenced}
                    },
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new PhysicalDamage(Game)
                    },
                    TargetingType = TargetingType.Beam,
                    TargetingDirection = TargetingDirection.Front2Octants
                };

                Add(doubleRangedAttack);
            }

            if (primaryMeleeWeaponAttack != null)
            {
                primaryMeleeWeaponAttack.Action = AbilityAction.Modifier;
                primaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon = null;
            }

            if (secondaryMeleeWeaponAttack != null)
            {
                secondaryMeleeWeaponAttack.Action = AbilityAction.Modifier;
                secondaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon = null;
            }

            if (primaryMeleeAttack != null)
            {
                primaryMeleeAttack.IsUsable = false;
                primaryMeleeAttack.Action = AbilityAction.Modifier;
            }

            if (secondaryMeleeAttack != null)
            {
                secondaryMeleeAttack.IsUsable = false;
                secondaryMeleeAttack.Action = AbilityAction.Modifier;
            }

            if (doubleMeleeAttack != null)
            {
                doubleMeleeAttack.IsUsable = false;
                doubleMeleeAttack.Action = AbilityAction.Modifier;
            }

            if (primaryRangedWeaponAttack != null)
            {
                primaryRangedWeaponAttack.Action = AbilityAction.Modifier;
                primaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon = null;
            }

            if (secondaryRangedWeaponAttack != null)
            {
                secondaryRangedWeaponAttack.Action = AbilityAction.Modifier;
                secondaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon = null;
            }

            if (primaryRangedAttack != null)
            {
                primaryRangedAttack.IsUsable = false;
                primaryRangedAttack.Action = AbilityAction.Modifier;
            }

            if (secondaryRangedAttack != null)
            {
                secondaryRangedAttack.IsUsable = false;
                secondaryRangedAttack.Action = AbilityAction.Modifier;
            }

            if (doubleRangedAttack != null)
            {
                doubleRangedAttack.IsUsable = false;
                doubleRangedAttack.Action = AbilityAction.Modifier;
            }

            if (!canUseWeapons)
            {
                return false;
            }

            Item twoHandedWeapon = null;
            var primaryWeapon = NaturalWeapon;
            var secondaryWeapon = NaturalWeapon;
            foreach (var item in Inventory)
            {
                if (item.EquippedSlot.HasValue)
                {
                    switch (item.EquippedSlot)
                    {
                        case EquipmentSlot.GraspBothExtremities:
                            twoHandedWeapon = item;
                            break;
                        case EquipmentSlot.GraspPrimaryExtremity:
                            primaryWeapon = item;
                            break;
                        case EquipmentSlot.GraspSecondaryExtremity:
                            secondaryWeapon = item;
                            break;
                    }
                }
            }

            var twoHandedWeaponType = twoHandedWeapon?.Type ?? ItemType.None;
            var primaryWeaponType = primaryWeapon.Type;
            var secondaryWeaponType = secondaryWeapon.Type;
            var dualWielding = primaryWeapon != null && secondaryWeapon != null
                                                     && (primaryWeaponType & ItemType.WeaponMeleeFist) == 0
                                                     && (secondaryWeaponType & ItemType.WeaponMeleeFist) == 0;
            var dualFist = (primaryWeaponType & ItemType.WeaponMeleeFist) != 0
                           && (secondaryWeaponType & ItemType.WeaponMeleeFist) != 0;

            // TODO: Choose proper targeting mode based on weapon type
            if (twoHandedWeapon != null)
            {
                if ((twoHandedWeaponType & ItemType.WeaponRanged) == 0)
                {
                    primaryMeleeAttack.IsUsable = true;
                    primaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon = twoHandedWeapon;

                    EnsureMeleeAttack(twoHandedWeapon);
                }

                primaryRangedAttack.IsUsable = true;
                primaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon = twoHandedWeapon;
                if ((twoHandedWeaponType & ItemType.WeaponRanged & ~ItemType.WeaponRangedThrown) == 0
                    && (twoHandedWeapon as Launcher)?.Projectile == null)
                {
                    primaryRangedWeaponAttack.Action = AbilityAction.Throw;
                }

                EnsureRangedAttack(twoHandedWeapon);
            }
            else
            {
                if (dualWielding
                    || dualFist)
                {
                    if ((primaryWeaponType & ItemType.WeaponRanged) == 0
                        && (secondaryWeaponType & ItemType.WeaponRanged) == 0)
                    {
                        doubleMeleeAttack.IsUsable = true;
                    }

                    if (!dualFist
                        && (((primaryWeaponType & ItemType.WeaponRanged) == 0)
                            == ((secondaryWeaponType & ItemType.WeaponRanged) == 0)))
                    {
                        doubleRangedAttack.IsUsable = true;
                    }
                }

                if ((primaryWeaponType & ItemType.WeaponRanged) == 0)
                {
                    primaryMeleeAttack.IsUsable = true;
                    primaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon = primaryWeapon;

                    EnsureMeleeAttack(primaryWeapon);
                }

                if ((secondaryWeaponType & ItemType.WeaponRanged) == 0)
                {
                    secondaryMeleeAttack.IsUsable = true;
                    secondaryMeleeWeaponAttack.Triggers.OfType<MeleeWeaponTrigger>().Single().Weapon = secondaryWeapon;

                    EnsureMeleeAttack(secondaryWeapon);
                }

                if ((primaryWeaponType & ItemType.WeaponMeleeFist) == 0)
                {
                    primaryRangedAttack.IsUsable = true;
                    primaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon = primaryWeapon;
                    if ((primaryWeaponType & ItemType.WeaponRanged & ~ItemType.WeaponRangedThrown) == 0
                        && (primaryWeapon as Launcher)?.Projectile == null)
                    {
                        primaryRangedWeaponAttack.Action = AbilityAction.Throw;
                    }

                    EnsureRangedAttack(primaryWeapon);
                }

                if ((secondaryWeaponType & ItemType.WeaponMeleeFist) == 0)
                {
                    secondaryRangedAttack.IsUsable = true;
                    secondaryRangedWeaponAttack.Triggers.OfType<RangedWeaponTrigger>().Single().Weapon =
                        secondaryWeapon;
                    if ((secondaryWeaponType & ItemType.WeaponRanged & ~ItemType.WeaponRangedThrown) == 0
                        && (secondaryWeapon as Launcher)?.Projectile == null)
                    {
                        secondaryRangedWeaponAttack.Action = AbilityAction.Throw;
                    }

                    EnsureRangedAttack(secondaryWeapon);
                }
            }

            return true;
        }

        protected bool CanUseWeapons() => !GetProperty<bool>(PropertyData.Handlessness.Name)
                                        && !GetProperty<bool>(PropertyData.Limblessness.Name);

        private void EnsureMeleeAttack(Item weapon)
        {
            if (weapon != null
                && weapon.Abilities.All(a => a.Activation != AbilityActivation.OnMeleeAttack))
            {
                weapon.Add(new Ability(Game)
                {
                    Activation = AbilityActivation.OnMeleeAttack,
                    Action = AbilityAction.Hit,
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new PhysicalDamage(Game) {Damage = GetWeightDamage(weapon)}
                    }
                });
            }
        }

        private void EnsureRangedAttack(Item weapon)
        {
            if (weapon != null
                && weapon.Abilities.All(a => a.Activation != AbilityActivation.OnRangedAttack))
            {
                weapon.Add(new Ability(Game)
                {
                    Activation = AbilityActivation.OnRangedAttack,
                    Action = AbilityAction.Hit,
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new PhysicalDamage(Game) {Damage = GetWeightDamage(weapon)}
                    }
                });
            }
        }

        private int GetWeightDamage(Item weapon)
            => 4 * weapon.GetProperty<int>(PropertyData.Weight.Name);

        /// <summary>Gives an opportunity to perform actions.</summary>
        /// <returns>Returns <c>false</c> if the actor hasn't finished their turn.</returns>
        public abstract bool Act();

        public abstract byte[] GetFOV();

        public byte GetVisibility(Point p) => GetFOV()[Level.PointToIndex[p.X, p.Y]];

        public virtual void Sense(SensoryEvent @event)
        {
            @event.Game = Game;
            @event.AddReference();
            @event.RemoveReference();
        }

        public virtual SenseType CanSense(Entity target)
        {
            var sense = SenseType.None;
            if (target == null)
            {
                return sense;
            }

            if (target == this) // Or is adjecent
            {
                sense |= SenseType.Touch;
            }

            sense |= SenseType.Sight;

            return sense;
        }

        public virtual bool UseStairs(bool up, bool pretend = false)
        {
            var stairs = Level.Connections.SingleOrDefault(s => s.LevelCell == LevelCell);

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
            var moveToLevelCell = stairs.TargetLevelCell;

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
                moveToLevel.Actors.SingleOrDefault(a => a.LevelCell == moveToLevelCell);
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
                LevelX = moveToLevelCell.X;
                LevelY = moveToLevelCell.Y;
                moveToLevel.Actors.Push(this);

                ActorMoveEvent.New(this, movee: null, eventOrder: Game.EventOrder++);
            }

            return true;
        }

        public virtual bool Move(Direction direction, bool pretend = false)
        {
            if (Heading != direction)
            {
                if (pretend)
                {
                    return true;
                }

                Turn(direction);

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
                var handled = HandleBlockingActor(conflictingActor, pretend);
                if (handled.HasValue)
                {
                    return handled.Value;
                }
            }

            if (MovementDelay == 0)
            {
                return false;
            }

            if (!Reposition(targetCell.Value, pretend))
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

        public virtual void Turn(Direction direction)
        {
            var octants = direction.ClosestOctantsTo(Heading);

            NextActionTick += (MovementDelay * octants) / 4;

            Heading = direction;
        }

        protected virtual void ActivateAbility(Ability ability, int? target)
        {
            using (var context = new AbilityActivationContext())
            {
                context.Activator = this;
                switch (ability.Activation)
                {
                    case AbilityActivation.OnActivation:
                        break;
                    case AbilityActivation.OnTarget:
                        var maxOctantDifference = 4;
                        switch (ability.TargetingDirection)
                        {
                            case TargetingDirection.Front2Octants:
                                maxOctantDifference = 0;
                                break;
                            case TargetingDirection.Front4Octants:
                                maxOctantDifference = 1;
                                break;
                            case TargetingDirection.Omnidirectional:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(
                                    "Invalid ability direction " + ability.TargetingDirection);
                        }

                        Point targetCell;
                        if (target.Value < 0)
                        {
                            var targetActor = Level.Actors.FirstOrDefault(a => a.Id == -target.Value);
                            if (targetActor == null)
                            {
                                return;
                            }

                            targetCell = targetActor.LevelCell;
                        }
                        else
                        {
                            targetCell = Point.Unpack(target).Value;
                        }
                        var targetDirection = context.Activator.LevelCell.DifferenceTo(targetCell);

                        var octantsToTurn = 0;
                        var targetAngleDifference = (int)Math.Truncate(targetDirection.OctantsTo(Heading));
                        if (targetAngleDifference > maxOctantDifference)
                        {
                            octantsToTurn = targetAngleDifference - maxOctantDifference;
                        }
                        else if (targetAngleDifference < -maxOctantDifference)
                        {
                            octantsToTurn = targetAngleDifference + maxOctantDifference;
                        }

                        if (octantsToTurn != 0)
                        {
                            var newDirection = (int)Heading - octantsToTurn;
                            if (newDirection < 0)
                            {
                                newDirection += 8;
                            }
                            else if (newDirection > 8)
                            {
                                newDirection -= 8;
                            }

                            Turn((Direction)newDirection);
                        }

                        context.TargetCell = targetCell;

                        break;
                    default:
                        throw new InvalidOperationException(
                            $"Ability {ability.Name} on character {Name} cannot be activated manually.");
                }

                ability.Activate(context);
            }
        }

        protected virtual bool? HandleBlockingActor(Actor actor, bool pretend)
            => false;

        private bool Reposition(Point targetCell, bool pretend)
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
            var itemsOnNewCell = Level.Items.Where(i => i.LevelCell == targetCell).ToList();
            foreach (var itemOnNewCell in itemsOnNewCell)
            {
                PickUp(itemOnNewCell);
            }

            return true;
        }

        public virtual bool GetDisplaced()
        {
            // TODO: displace other actors
            var possibleDirectionsToMove = Level.GetPossibleMovementDirections(LevelCell, safe: true);
            if (possibleDirectionsToMove.Count == 0)
            {
                NextActionTick += DefaultActionDelay;
                return true;
            }

            var directionIndex = Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);

            var targetCell = ToLevelCell(Vector.Convert(possibleDirectionsToMove[directionIndex]));
            if (targetCell != null)
            {
                return Reposition(targetCell.Value, pretend: false);
            }

            // TODO: fire event

            return false;
        }

        public virtual bool Equip(Item item, EquipmentSlot slot, bool pretend = false)
        {
            var equipped = GetEquippedItem(slot);
            if (equipped == item)
            {
                return true;
            }

            if ((item.EquipableSlots & slot) == 0)
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
                if (slot == EquipmentSlot.GraspPrimaryExtremity || slot == EquipmentSlot.GraspSecondaryExtremity)
                {
                    Unequip(GetEquippedItem(EquipmentSlot.GraspBothExtremities));
                }
                else if (slot == EquipmentSlot.GraspBothExtremities)
                {
                    Unequip(GetEquippedItem(EquipmentSlot.GraspPrimaryExtremity));
                    Unequip(GetEquippedItem(EquipmentSlot.GraspSecondaryExtremity));
                }
            }
            else
            {
                Unequip(equipped);
            }

            item.EquippedSlot = slot;
            ItemEquipmentEvent.New(this, item, Game.EventOrder++);

            item.ActivateAbilities(AbilityActivation.WhileEquipped, this, this);

            RecalculateWeaponAbilities();

            return true;
        }

        public virtual bool Unequip(Item item, bool pretend = false, bool fireEvent = true)
        {
            if (item?.EquippedSlot == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            item.EquippedSlot = null;

            if (fireEvent)
            {
                // TODO: Calculate delay
                NextActionTick += DefaultActionDelay;
                ItemUnequipmentEvent.New(this, item, Game.EventOrder++);
            }

            foreach (var ability in item.Abilities.Where(a
                => a.Activation == AbilityActivation.WhileEquipped && a.IsActive))
            {
                ability.Deactivate();
            }

            RecalculateWeaponAbilities();

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

                splitItem.ActivateAbilities(AbilityActivation.OnConsumption, this, this);
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
            if (item.EquippedSlot != null && !Unequip(item, pretend, fireEvent: false))
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

            item.ActivateAbilities(AbilityActivation.WhilePossessed, this, this);

            return true;
        }

        public virtual bool CanAdd(Item item) => true;

        public virtual bool Remove(Item item)
        {
            if (item.EquippedSlot != null)
            {
                Unequip(item, fireEvent: false);
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

        public override void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
        {
            if (PropertyListeners.TryGetValue(propertyName, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    ((Action<Actor, T, T>)listener)(this, oldValue, newValue);
                }
            }

            base.OnPropertyChanged(propertyName, oldValue, newValue);
        }

        public override bool HasListener(string propertyName)
            => PropertyListeners.ContainsKey(propertyName)
               || base.HasListener(propertyName);

        private static void AddPropertyListener<T>(string propertyName, Action<Actor, T, T> action)
        {
            if (!PropertyListeners.TryGetValue(propertyName, out var listeners))
            {
                listeners = new List<object>();
                PropertyListeners[propertyName] = listeners;
            }

            listeners.Add(action);
        }

        private void OnMaxHPChanged(int oldValue, int newValue)
        {
            var newHP = HP * newValue / oldValue;
            ChangeCurrentHP(newHP - HP);
        }

        private void OnMaxEPChanged(int oldValue, int newValue)
        {
            var newEP = EP * newValue / oldValue;
            ChangeCurrentEP(newEP - EP);
        }

        private void OnConstitutionChanged(int oldValue, int newValue)
        {
            var effect = Abilities.First(a => a.Name == AttributedAbilityName).ActiveEffects
                .OfType<ChangedProperty<int>>().First(e => e.PropertyName == PropertyData.HitPointMaximum.Name);
            effect.Value = newValue * 10;
            effect.UpdateProperty();
        }

        private void OnWillpowerChanged(int oldValue, int newValue)
        {
            var effect = Abilities.First(a => a.Name == AttributedAbilityName).ActiveEffects
                .OfType<ChangedProperty<int>>().First(e => e.PropertyName == PropertyData.EnergyPointMaximum.Name);
            effect.Value = newValue * 10;
            effect.UpdateProperty();
        }

        private void OnQuicknessChanged(int oldValue, int newValue)
        {
            MovementDelay = DefaultActionDelay * 10 / GetProperty<int>(PropertyData.Quickness.Name);
        }

        public virtual bool ChangeCurrentHP(int hp)
        {
            if (!IsAlive)
            {
                return false;
            }

            var hpProperty = InvalidateProperty<int>(PropertyData.HitPoints.Name);
            var newHP = hpProperty.LastValue + hp;

            if (newHP > MaxHP)
            {
                newHP = MaxHP;
            }

            hpProperty.CurrentValue = newHP;

            if (!IsAlive)
            {
                Die();
                return false;
            }

            return true;
        }

        public virtual void ChangeCurrentEP(int ep)
        {
            var epProperty = InvalidateProperty<int>(PropertyData.EnergyPoints.Name);
            var newEP = epProperty.LastValue + ep;

            if (newEP < 0)
            {
                newEP = 0;
            }

            if (EP > MaxEP)
            {
                newEP = MaxEP;
            }

            epProperty.CurrentValue = newEP;
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
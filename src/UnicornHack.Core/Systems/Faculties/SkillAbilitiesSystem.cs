using System.Linq;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Faculties
{
    public class SkillAbilitiesSystem : IGameSystem<ItemEquippedMessage>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, ExtremityType>>
    {
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

        public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
        {
            if (message.Successful)
            {
                RecalculateWeaponAbilities(message.ActorEntity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, ExtremityType> message,
            GameManager manager)
        {
            RecalculateWeaponAbilities(message.Entity, manager);

            return MessageProcessingResult.ContinueProcessing;
        }

        private bool RecalculateWeaponAbilities(GameEntity actorEntity, GameManager manager)
        {
            var canUseWeapons = CanUseWeapons(actorEntity);

            var primaryMeleeWeaponAttack =
                ResetWeaponAbility(PrimaryMeleeWeaponAttackName, actorEntity.Id, canUseWeapons, manager);

            var secondaryMeleeWeaponAttack =
                ResetWeaponAbility(SecondaryMeleeWeaponAttackName, actorEntity.Id, canUseWeapons, manager);

            var primaryMeleeAttack = ResetAttackAbility(
                PrimaryMeleeAttackName, actorEntity.Id, TargetingType.AdjacentSingle, ActivationType.OnMeleeAttack,
                canUseWeapons, primaryMeleeWeaponAttack?.EntityId, null, manager);

            var secondaryMeleeAttack = ResetAttackAbility(
                SecondaryMeleeAttackName, actorEntity.Id, TargetingType.AdjacentSingle, ActivationType.OnMeleeAttack,
                canUseWeapons, secondaryMeleeWeaponAttack?.EntityId, null, manager);

            var doubleMeleeAttack = ResetAttackAbility(
                DoubleMeleeAttackName, actorEntity.Id, TargetingType.AdjacentSingle, ActivationType.OnMeleeAttack,
                canUseWeapons, primaryMeleeWeaponAttack?.EntityId, secondaryMeleeWeaponAttack?.EntityId, manager);

            var primaryRangedWeaponAttack =
                ResetWeaponAbility(PrimaryRangedWeaponAttackName, actorEntity.Id, canUseWeapons, manager);

            var secondaryRangedWeaponAttack =
                ResetWeaponAbility(SecondaryRangedWeaponAttackName, actorEntity.Id, canUseWeapons, manager);

            var primaryRangedAttack = ResetAttackAbility(
                PrimaryRangedAttackName, actorEntity.Id, TargetingType.Projectile, ActivationType.OnRangedAttack,
                canUseWeapons, primaryRangedWeaponAttack?.EntityId, null, manager);

            var secondaryRangedAttack = ResetAttackAbility(
                SecondaryRangedAttackName, actorEntity.Id, TargetingType.Projectile, ActivationType.OnRangedAttack,
                canUseWeapons, secondaryRangedWeaponAttack?.EntityId, null, manager);

            var doubleRangedAttack = ResetAttackAbility(
                DoubleRangedAttackName, actorEntity.Id, TargetingType.Projectile, ActivationType.OnRangedAttack,
                canUseWeapons, primaryRangedWeaponAttack?.EntityId, secondaryRangedWeaponAttack?.EntityId, manager);

            if (!canUseWeapons)
            {
                return false;
            }

            var being = actorEntity.Being;
            ItemComponent twoHandedWeapon = null;
            ItemComponent primaryWeapon;
            ItemComponent secondaryWeapon;

            if (being.PrimaryNaturalWeaponId.HasValue)
            {
                primaryWeapon = manager.FindEntity(being.PrimaryNaturalWeaponId.Value).Item;
            }
            else
            {
                using (var primaryWeaponReference = CreateNaturalWeapon(manager, being.UpperExtremeties))
                {
                    primaryWeapon = primaryWeaponReference.Referenced.Item;
                    being.PrimaryNaturalWeaponId = primaryWeapon.EntityId;
                }
            }

            if (being.SecondaryNaturalWeaponId.HasValue)
            {
                secondaryWeapon = manager.FindEntity(being.SecondaryNaturalWeaponId.Value).Item;
            }
            else
            {
                using (var secondaryWeaponReference = CreateNaturalWeapon(manager, being.UpperExtremeties))
                {
                    secondaryWeapon = secondaryWeaponReference.Referenced.Item;
                    being.SecondaryNaturalWeaponId = secondaryWeapon.EntityId;
                }
            }

            foreach (var itemEntity in manager.EntityItemsToContainerRelationship[actorEntity.Id])
            {
                var item = itemEntity.Item;

                switch (item.EquippedSlot)
                {
                    case EquipmentSlot.GraspBothExtremities:
                        twoHandedWeapon = item;
                        primaryWeapon = null;
                        secondaryWeapon = null;
                        break;
                    case EquipmentSlot.GraspPrimaryExtremity:
                        primaryWeapon = item;
                        break;
                    case EquipmentSlot.GraspSecondaryExtremity:
                        secondaryWeapon = item;
                        break;
                }
            }

            var twoHandedWeaponType = twoHandedWeapon?.Type ?? ItemType.None;
            var primaryWeaponType = primaryWeapon?.Type ?? ItemType.None;
            var secondaryWeaponType = secondaryWeapon?.Type ?? ItemType.None;
            var dualWielding = primaryWeapon != null
                               && secondaryWeapon != null
                               && (primaryWeaponType & ItemType.WeaponMeleeFist) == 0
                               && (secondaryWeaponType & ItemType.WeaponMeleeFist) == 0;
            var dualFist = (primaryWeaponType & ItemType.WeaponMeleeFist) != 0
                           && (secondaryWeaponType & ItemType.WeaponMeleeFist) != 0;

            ItemComponent primaryMeleeWeapon = null;
            ItemComponent secondaryMeleeWeapon = null;
            ItemComponent primaryRangedWeapon = null;
            ItemComponent secondaryRangedWeapon = null;

            // TODO: Choose proper targeting type and angle based on weapon
            if (twoHandedWeapon != null)
            {
                if ((twoHandedWeaponType & ItemType.WeaponRanged) == 0)
                {
                    primaryMeleeAttack.IsUsable = true;
                    SetWeapon(primaryMeleeWeaponAttack, twoHandedWeapon.EntityId, manager);
                    primaryMeleeWeapon = twoHandedWeapon;

                    EnsureMeleeAttack(twoHandedWeapon, manager);
                }

                primaryRangedAttack.IsUsable = true;
                SetWeapon(primaryRangedWeaponAttack, twoHandedWeapon.EntityId, manager);
                primaryRangedWeapon = twoHandedWeapon;

                EnsureRangedAttack(twoHandedWeapon, manager);
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
                    SetWeapon(primaryMeleeWeaponAttack, primaryWeapon.EntityId, manager);
                    primaryMeleeWeapon = primaryWeapon;

                    EnsureMeleeAttack(primaryWeapon, manager);
                }

                if ((secondaryWeaponType & ItemType.WeaponRanged) == 0)
                {
                    secondaryMeleeAttack.IsUsable = true;
                    SetWeapon(secondaryMeleeWeaponAttack, secondaryWeapon.EntityId, manager);
                    secondaryMeleeWeapon = secondaryWeapon;

                    EnsureMeleeAttack(secondaryWeapon, manager);
                }

                if ((primaryWeaponType & ItemType.WeaponMeleeFist) == 0)
                {
                    primaryRangedAttack.IsUsable = true;
                    SetWeapon(primaryRangedWeaponAttack, primaryWeapon.EntityId, manager);
                    primaryRangedWeapon = primaryWeapon;

                    EnsureRangedAttack(primaryWeapon, manager);
                }

                if ((secondaryWeaponType & ItemType.WeaponMeleeFist) == 0)
                {
                    secondaryRangedAttack.IsUsable = true;
                    SetWeapon(secondaryRangedWeaponAttack, secondaryWeapon.EntityId, manager);
                    secondaryRangedWeapon = secondaryWeapon;

                    EnsureRangedAttack(secondaryWeapon, manager);
                }
            }

            var player = actorEntity.Player;
            if (player != null)
            {
                // TODO: Calculate skill effects properly
                var primaryMeleeWeaponSkill =
                    primaryMeleeWeapon == null ? 0 : GetMeleeSkill(primaryMeleeWeapon.Type, player);
                SetDamage(primaryMeleeWeaponAttack, primaryMeleeWeaponSkill ?? 0, manager);

                var secondaryMeleeWeaponSkill =
                    secondaryMeleeWeapon == null ? 0 : GetMeleeSkill(secondaryMeleeWeapon.Type, player);
                SetDamage(secondaryMeleeWeaponAttack, secondaryMeleeWeaponSkill ?? 0, manager);

                var setSlotMessage = manager.AbilitySlottingSystem.CreateSetAbilitySlotMessage(manager);
                setSlotMessage.Slot = AbilitySlottingSystem.DefaultAttackSlot;

                if (secondaryMeleeAttack.IsUsable)
                {
                    SetDamage(secondaryMeleeAttack, player.OneHanded, manager);
                    if (secondaryMeleeWeaponSkill != null)
                    {
                        setSlotMessage.AbilityEntity = secondaryMeleeAttack.Entity;
                    }
                }

                if (primaryMeleeAttack.IsUsable)
                {
                    SetDamage(primaryMeleeAttack, primaryMeleeWeapon.EquippedSlot == EquipmentSlot.GraspBothExtremities
                            ? player.TwoHanded
                            : player.OneHanded,
                        manager);
                    if (primaryMeleeWeaponSkill != null)
                    {
                        setSlotMessage.AbilityEntity = primaryMeleeAttack.Entity;
                    }
                }

                if (doubleMeleeAttack.IsUsable)
                {
                    SetDamage(doubleMeleeAttack, player.DualWielding, manager);
                    if (primaryMeleeWeaponSkill != null && secondaryMeleeWeaponSkill != null)
                    {
                        setSlotMessage.AbilityEntity = doubleMeleeAttack.Entity;
                    }
                }

                var primaryRangedSkill =
                    primaryRangedWeapon == null ? 0 : GetRangedSkill(primaryRangedWeapon.Type, player);
                SetDamage(primaryRangedWeaponAttack, primaryRangedSkill ?? player.Thrown, manager);

                var secondaryRangedSkill = secondaryRangedWeapon == null
                    ? 0
                    : GetRangedSkill(secondaryRangedWeapon.Type, player);
                SetDamage(secondaryRangedWeaponAttack, secondaryRangedSkill ?? player.Thrown, manager);

                if (secondaryRangedAttack.IsUsable)
                {
                    SetDamage(secondaryRangedAttack, player.OneHanded, manager);
                    if (secondaryRangedSkill != null)
                    {
                        setSlotMessage.AbilityEntity = secondaryRangedAttack.Entity;
                    }
                }

                if (primaryRangedAttack.IsUsable)
                {
                    SetDamage(primaryRangedAttack,
                        primaryRangedWeapon.EquippedSlot == EquipmentSlot.GraspBothExtremities
                            ? player.TwoHanded
                            : player.OneHanded,
                        manager);
                    if (primaryRangedSkill != null)
                    {
                        setSlotMessage.AbilityEntity = primaryRangedAttack.Entity;
                    }
                }

                if (doubleRangedAttack.IsUsable)
                {
                    SetDamage(doubleRangedAttack, player.DualWielding, manager);
                    if (primaryRangedSkill != null && secondaryRangedSkill != null)
                    {
                        setSlotMessage.AbilityEntity = doubleRangedAttack.Entity;
                    }
                }

                if(setSlotMessage.AbilityEntity != null)
                {
                    manager.Enqueue(setSlotMessage);
                }
                else
                {
                    manager.Queue.ReturnMessage(setSlotMessage);
                }
            }

            return true;
        }

        private ITransientReference<GameEntity> CreateNaturalWeapon(GameManager manager, ExtremityType extremityType)
        {
            var itemEntityReference = manager.CreateEntity();
            var itemEntity = itemEntityReference.Referenced;
            var item = manager.CreateComponent<ItemComponent>(EntityComponent.Item);
            item.Type = ItemType.WeaponMeleeFist;
            // TODO: Use name for claws, etc.
            item.TemplateName = "fist";

            itemEntity.Item = item;

            var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
            physical.Material = Material.Flesh;

            itemEntity.Physical = physical;

            using (var appliedEffectEntityReference = manager.CreateEntity())
            {
                var appliedEffectEntity = appliedEffectEntityReference.Referenced;
                var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                appliedEffect.EffectType = EffectType.AddAbility;
                appliedEffect.DurationTicks = (int)EffectDuration.Infinite;

                appliedEffectEntity.Effect = appliedEffect;

                var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Name = LivingSystem.InnateAbilityName;
                ability.Activation = ActivationType.OnPhysicalMeleeAttack;
                // TODO: Use correct action for claws, etc.
                ability.Action = AbilityAction.Punch;

                appliedEffectEntity.Ability = ability;

                using (var effectEntityReference = manager.CreateEntity())
                {
                    var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    effect.EffectType = EffectType.PhysicalDamage;
                    // TODO: Calculate h2h damage
                    effect.Amount = 20;
                    effect.ContainingAbilityId = appliedEffectEntity.Id;

                    effectEntityReference.Referenced.Effect = effect;
                }

                ability.OwnerId = itemEntity.Id;
                appliedEffect.AffectedEntityId = itemEntity.Id;
            }

            return itemEntityReference;
        }

        private AbilityComponent ResetWeaponAbility(
            string name,
            int ownerId,
            bool canUseWeapons,
            GameManager manager)
        {
            var weaponAttack = manager.AbilitiesToAffectableRelationship[ownerId].Select(a => a.Ability)
                .FirstOrDefault(a => a.Name == name);
            if (weaponAttack == null && canUseWeapons)
            {
                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.Name = name;
                    ability.OwnerId = ownerId;
                    // TODO: Calculate proper delay
                    ability.Delay = TimeSystem.DefaultActionDelay;

                    abilityEntity.Ability = ability;

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.PhysicalDamage;
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.Activate;
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }

                    return ability;
                }
            }

            if (weaponAttack != null)
            {
                if (canUseWeapons)
                {
                    SetWeapon(weaponAttack, null, manager);
                }
                else
                {
                    weaponAttack.Entity.RemoveComponent(EntityComponent.Ability);
                    return null;
                }
            }

            return weaponAttack;
        }

        private void SetWeapon(AbilityComponent weaponAttack, int? weaponId, GameManager manager)
        {
            foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[weaponAttack.EntityId])
            {
                var effect = effectEntity.Effect;
                if (effect.EffectType != EffectType.Activate)
                {
                    continue;
                }

                effect.ActivatableEntityId = weaponId;
                break;
            }
        }

        private void SetDamage(AbilityComponent attack, int damage, GameManager manager)
        {
            foreach (var effectEntity in manager.EffectsToContainingAbilityRelationship[attack.EntityId])
            {
                var effect = effectEntity.Effect;
                if (effect.EffectType != EffectType.PhysicalDamage)
                {
                    continue;
                }

                effect.Amount = damage;
                break;
            }
        }

        private AbilityComponent ResetAttackAbility(
            string name,
            int ownerId,
            TargetingType targetingType,
            ActivationType trigger,
            bool canUseWeapons,
            int? firstWeaponAbilityId,
            int? secondWeaponAbilityId,
            GameManager manager)
        {
            var weaponAttack = manager.AbilitiesToAffectableRelationship[ownerId].Select(a => a.Ability)
                .FirstOrDefault(a => a.Name == name);
            if (weaponAttack == null && canUseWeapons)
            {
                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.Name = name;
                    ability.OwnerId = ownerId;
                    ability.Activation = ActivationType.Targeted;
                    ability.Trigger = trigger;
                    ability.IsUsable = false;
                    ability.TargetingType = targetingType;
                    ability.TargetingAngle = TargetingAngle.Front2Octants;
                    // TODO: Calculate proper delay
                    ability.Delay = TimeSystem.DefaultActionDelay;

                    abilityEntity.Ability = ability;

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.PhysicalDamage;
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.Activate;
                        effect.ActivatableEntityId = firstWeaponAbilityId;
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }

                    if (secondWeaponAbilityId != null)
                    {
                        using (var effectEntityReference = manager.CreateEntity())
                        {
                            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                            effect.EffectType = EffectType.Activate;
                            effect.ActivatableEntityId = secondWeaponAbilityId;
                            effect.ContainingAbilityId = abilityEntity.Id;

                            effectEntityReference.Referenced.Effect = effect;
                        }
                    }

                    return ability;
                }
            }

            if (weaponAttack != null)
            {
                if (canUseWeapons)
                {
                    weaponAttack.IsUsable = false;
                }
                else
                {
                    weaponAttack.Entity.RemoveComponent(EntityComponent.Ability);
                    return null;
                }
            }

            return weaponAttack;
        }

        private bool CanUseWeapons(GameEntity actorEntity)
            => actorEntity.Being.UpperExtremeties == ExtremityType.GraspingFingers;

        private void EnsureMeleeAttack(ItemComponent weapon, GameManager manager)
        {
            if (manager.AbilitiesToAffectableRelationship[weapon.EntityId]
                .All(a => (a.Ability.Activation & ActivationType.OnMeleeAttack) == 0))
            {
                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.OwnerId = weapon.EntityId;
                    ability.Activation = ActivationType.OnPhysicalMeleeAttack;
                    ability.Action = AbilityAction.Hit;

                    abilityEntity.Ability = ability;

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.PhysicalDamage;
                        effect.Amount = GetWeightDamage(weapon);
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }
                }
            }
        }

        private void EnsureRangedAttack(ItemComponent weapon, GameManager manager)
        {
            if (manager.AbilitiesToAffectableRelationship[weapon.EntityId]
                .All(a => (a.Ability.Activation & ActivationType.OnRangedAttack) == 0))
            {
                using (var abilityEntityReference = manager.CreateEntity())
                {
                    var abilityEntity = abilityEntityReference.Referenced;

                    var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    ability.OwnerId = weapon.EntityId;
                    ability.Activation = ActivationType.OnPhysicalRangedAttack;
                    ability.Action = AbilityAction.Hit;

                    abilityEntity.Ability = ability;

                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.PhysicalDamage;
                        effect.Amount = GetWeightDamage(weapon);
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }

                    // TODO: Separate this into a different ability so the item is moved even when missed
                    using (var effectEntityReference = manager.CreateEntity())
                    {
                        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        effect.EffectType = EffectType.Move;
                        effect.ActivatableEntityId = weapon.EntityId;
                        effect.ContainingAbilityId = abilityEntity.Id;

                        effectEntityReference.Referenced.Effect = effect;
                    }
                }
            }

            // TODO: Make sure launcher has activate item for the projectile
        }

        private int GetWeightDamage(ItemComponent weapon)
            => 4 * weapon.Entity.Physical.Weight;

        private int? GetMeleeSkill(ItemType weaponType, PlayerComponent player)
        {
            if ((weaponType & ItemType.WeaponMeleeFist) != 0)
            {
                return player.FistWeapons;
            }

            if ((weaponType & ItemType.WeaponMeleeShort) != 0)
            {
                return player.ShortWeapons;
            }

            if ((weaponType & ItemType.WeaponMeleeMedium) != 0)
            {
                return player.MediumWeapons;
            }

            if ((weaponType & ItemType.WeaponMeleeLong) != 0)
            {
                return player.LongWeapons;
            }

            if ((weaponType & ItemType.WeaponMagicFocus) != 0)
            {
                return player.MeleeMagicWeapons;
            }

            return null;
        }

        private int? GetRangedSkill(ItemType weaponType, PlayerComponent player)
        {
            if ((weaponType & ItemType.WeaponRangedThrown) != 0)
            {
                return player.Thrown;
            }

            if ((weaponType & ItemType.WeaponRangedBow) != 0)
            {
                return player.Bows;
            }

            if ((weaponType & ItemType.WeaponRangedCrossbow) != 0)
            {
                return player.Crossbows;
            }

            if ((weaponType & ItemType.WeaponRangedSlingshot) != 0)
            {
                return player.Slingshots;
            }

            if ((weaponType & ItemType.WeaponMagicStaff) != 0)
            {
                return player.RangedMagicWeapons;
            }

            return null;
        }

        public bool CanBeDefaultAttack(AbilityComponent ability)
        {
            switch (ability.Name)
            {
                case DoubleMeleeAttackName:
                case PrimaryMeleeAttackName:
                case SecondaryMeleeAttackName:
                case DoubleRangedAttackName:
                case PrimaryRangedAttackName:
                case SecondaryRangedAttackName:
                    return true;
                default:
                    return false;
            }
        }
    }
}

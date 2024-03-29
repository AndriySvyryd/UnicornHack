﻿using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Faculties;

public class SkillAbilitiesSystem : IGameSystem<ItemEquippedMessage>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, ExtremityType>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
{
    public const string PrimaryMeleeWeaponAttackName = "primary melee weapon attack";
    public const string SecondaryMeleeWeaponAttackName = "secondary melee weapon attack";
    public const string TwoHandedMeleeWeaponAttackName = "two handed melee weapon attack";

    public const string PrimaryRangedWeaponAttackName = "primary ranged weapon attack";
    public const string SecondaryRangedWeaponAttackName = "secondary ranged weapon attack";
    public const string TwoHandedRangedWeaponAttackName = "two handed ranged weapon attack";

    public const string EquippedAbilityName = "equipped";

    public MessageProcessingResult Process(ItemEquippedMessage message, GameManager manager)
    {
        if (message.Successful)
        {
            if ((message.Slot & EquipmentSlot.GraspMelee) != 0
                || (message.OldSlot & EquipmentSlot.GraspMelee) != 0)
            {
                RecalculateWeaponAbilities(message.ActorEntity, melee: true, manager);
            }
            else if ((message.Slot & EquipmentSlot.GraspRanged) != 0
                     || (message.OldSlot & EquipmentSlot.GraspRanged) != 0)
            {
                RecalculateWeaponAbilities(message.ActorEntity, melee: false, manager);
            }

            RecalculateHindrance(message.ActorEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, ExtremityType> message, GameManager manager)
    {
        if (message.Entity.Being!.IsAlive)
        {
            RecalculateWeaponAbilities(message.Entity, melee: true, manager);
            RecalculateWeaponAbilities(message.Entity, melee: false, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        switch (message.ChangedPropertyName)
        {
            case nameof(PlayerComponent.HandWeapons):
            case nameof(PlayerComponent.ShortWeapons):
            case nameof(PlayerComponent.MediumWeapons):
            case nameof(PlayerComponent.LongWeapons):
                RecalculateWeaponAbilities(message.Entity, melee: true, manager);
                RecalculateHindrance(message.Entity, manager);
                break;
            case nameof(PlayerComponent.CloseRangeWeapons):
            case nameof(PlayerComponent.ShortRangeWeapons):
            case nameof(PlayerComponent.MediumRangeWeapons):
            case nameof(PlayerComponent.LongRangeWeapons):
                RecalculateWeaponAbilities(message.Entity, melee: false, manager);
                RecalculateHindrance(message.Entity, manager);
                break;
            case nameof(PlayerComponent.LightArmor):
            case nameof(PlayerComponent.HeavyArmor):
            case nameof(PlayerComponent.Artifice):
                RecalculateHindrance(message.Entity, manager);
                break;
            default:
                throw new InvalidOperationException($"Property {message.ChangedPropertyName} not supported.");
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private bool RecalculateWeaponAbilities(GameEntity actorEntity, bool melee, GameManager manager)
    {
        var canUseWeapons = CanUseWeapons(actorEntity);

        var being = actorEntity.Being!;
        ItemComponent? twoHandedWeapon = null;
        ItemComponent? primaryWeapon = null;
        ItemComponent? secondaryWeapon = null;

        if (canUseWeapons)
        {
            if (melee)
            {
                if (being.PrimaryNaturalWeaponId.HasValue)
                {
                    primaryWeapon = manager.FindEntity(being.PrimaryNaturalWeaponId.Value)!.Item;
                }
                else
                {
                    var primaryWeaponReference = TryCreateNaturalWeapon(being.UpperExtremities, melee, manager);
                    if (primaryWeaponReference != null)
                    {
                        primaryWeapon = primaryWeaponReference.Referenced.Item!;
                        primaryWeapon.EquippedSlot = EquipmentSlot.GraspPrimaryMelee;
                        being.PrimaryNaturalWeaponId = primaryWeapon.EntityId;
                        primaryWeaponReference.Dispose();
                    }
                }

                if (being.SecondaryNaturalWeaponId.HasValue)
                {
                    secondaryWeapon = manager.FindEntity(being.SecondaryNaturalWeaponId.Value)!.Item;
                }
                else
                {
                    var secondaryWeaponReference = TryCreateNaturalWeapon(being.UpperExtremities, melee, manager);
                    if (secondaryWeaponReference != null)
                    {
                        secondaryWeapon = secondaryWeaponReference.Referenced.Item!;
                        secondaryWeapon.EquippedSlot = EquipmentSlot.GraspSecondaryMelee;
                        being.SecondaryNaturalWeaponId = secondaryWeapon.EntityId;
                        secondaryWeaponReference.Dispose();
                    }
                }
            }

            foreach (var itemEntity in actorEntity.Being!.Items)
            {
                var item = itemEntity.Item!;

                if (melee)
                {
                    switch (item.EquippedSlot)
                    {
                        case EquipmentSlot.GraspBothMelee:
                            twoHandedWeapon = item;
                            primaryWeapon = null;
                            secondaryWeapon = null;
                            break;
                        case EquipmentSlot.GraspPrimaryMelee:
                            primaryWeapon = item;
                            break;
                        case EquipmentSlot.GraspSecondaryMelee:
                            secondaryWeapon = item;
                            break;
                    }
                }
                else
                {
                    switch (item.EquippedSlot)
                    {
                        case EquipmentSlot.GraspBothRanged:
                            twoHandedWeapon = item;
                            primaryWeapon = null;
                            secondaryWeapon = null;
                            break;
                        case EquipmentSlot.GraspPrimaryRanged:
                            primaryWeapon = item;
                            break;
                        case EquipmentSlot.GraspSecondaryRanged:
                            secondaryWeapon = item;
                            break;
                    }
                }
            }
        }

        var activation = melee ? ActivationType.OnMeleeAttack : ActivationType.OnRangedAttack;
        var twoHandedWeaponAttack = ResetWeaponAbility(
            melee ? TwoHandedMeleeWeaponAttackName : TwoHandedRangedWeaponAttackName,
            actorEntity, twoHandedWeapon, activation, manager);

        var primaryWeaponAttack = ResetWeaponAbility(
            melee ? PrimaryMeleeWeaponAttackName : PrimaryRangedWeaponAttackName,
            actorEntity, primaryWeapon, activation, manager);

        var secondaryWeaponAttack = ResetWeaponAbility(
            melee ? SecondaryMeleeWeaponAttackName : SecondaryRangedWeaponAttackName,
            actorEntity, secondaryWeapon, activation, manager);

        foreach (WieldingAbility defaultAttackTemplate in Ability.Loader.GetAllValues(AbilityType.DefaultAttack))
        {
            if ((melee && (defaultAttackTemplate.ItemType & ItemType.WeaponMelee) != 0)
                || (!melee && (defaultAttackTemplate.ItemType & ItemType.WeaponRanged) != 0))
            {
                EnsureAbility(defaultAttackTemplate, actorEntity, manager);
            }
        }

        AbilityComponent? defaultAttackAbility = null;
        var expectedItemType = melee ? ItemType.WeaponMelee : ItemType.WeaponRanged;
        foreach (var abilityEntity in being.Abilities)
        {
            var ability = abilityEntity.Ability!;
            if (ability.Template == null
                || !(ability.Template is WieldingAbility wieldingAbility)
                || (wieldingAbility.ItemType & expectedItemType) == 0)
            {
                continue;
            }

            switch (wieldingAbility.WieldingStyle)
            {
                case WieldingStyle.OneHanded:
                    var primaryUsable = IsCompatible(
                        primaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);
                    var secondaryUsable = IsCompatible(
                        secondaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);

                    ResetWieldingAbility(ability, wieldingAbility,
                        primaryUsable == secondaryUsable ? null :
                        primaryUsable ? primaryWeaponAttack : secondaryWeaponAttack,
                        null, activation, actorEntity);
                    break;
                case WieldingStyle.TwoHanded:
                    var twoHandedUsable = IsCompatible(
                        twoHandedWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);

                    ResetWieldingAbility(ability, wieldingAbility,
                        twoHandedUsable ? twoHandedWeaponAttack : null, null, activation, actorEntity);
                    break;
                case WieldingStyle.Dual:
                    var bothUsable = IsCompatible(
                                         primaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType)
                                     && IsCompatible(
                                         secondaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);

                    ResetWieldingAbility(ability, wieldingAbility,
                        bothUsable ? primaryWeaponAttack : null, bothUsable ? secondaryWeaponAttack : null,
                        activation, actorEntity);
                    break;
                default:
                    var primaryWeaponUsable = IsCompatible(
                        primaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);
                    var secondaryWeaponUsable = IsCompatible(
                        secondaryWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);
                    var twoHandedWeaponUsable = IsCompatible(
                        twoHandedWeapon, activation, wieldingAbility.ItemType, wieldingAbility.DamageType);

                    ResetWieldingAbility(ability, wieldingAbility,
                        primaryWeaponUsable ? primaryWeaponAttack :
                        twoHandedWeaponUsable ? twoHandedWeaponAttack : null,
                        secondaryWeaponUsable ? secondaryWeaponAttack : null, activation, actorEntity);
                    break;
            }

            if (wieldingAbility.Type == AbilityType.DefaultAttack
                && ability.IsUsable)
            {
                Debug.Assert(defaultAttackAbility == null);
                defaultAttackAbility = ability;
            }
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.Slot = melee
            ? AbilitySlottingSystem.DefaultMeleeAttackSlot
            : AbilitySlottingSystem.DefaultRangedAttackSlot;
        setSlotMessage.AbilityEntity = defaultAttackAbility?.Entity;
        setSlotMessage.OwnerEntity = actorEntity;
        manager.Enqueue(setSlotMessage);

        return canUseWeapons;
    }

    private ITransientReference<GameEntity>? TryCreateNaturalWeapon(
        ExtremityType extremityType, bool melee, GameManager manager)
    {
        if (!melee
            || extremityType != ExtremityType.GraspingFingers)
        {
            return null;
        }

        // TODO: Support claws, tentacles, etc.
        return Item.Loader.Get("fist").Instantiate(manager);
    }

    private AbilityComponent? ResetWeaponAbility(
        string name,
        GameEntity ownerEntity,
        ItemComponent? weapon,
        ActivationType activation,
        GameManager manager)
    {
        var weaponAttack = manager.AffectableAbilitiesIndex[(ownerEntity.Id, name)]?.Ability;
        if (weaponAttack == null && weapon != null)
        {
            using var abilityEntityReference = manager.CreateEntity();
            var abilityEntity = abilityEntityReference.Referenced;

            weaponAttack = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            weaponAttack.Name = name;
            weaponAttack.OwnerId = ownerEntity.Id;

            abilityEntity.Ability = weaponAttack;

            using (var effectEntityReference = manager.CreateEntity())
            {
                var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                effect.EffectType = EffectType.Activate;
                effect.ContainingAbilityId = abilityEntity.Id;

                effectEntityReference.Referenced.Effect = effect;
            }
        }

        if (weaponAttack != null)
        {
            if (weapon != null)
            {
                var abilityToActivate = weapon.Abilities.Select(a => a.Ability!)
                    .FirstOrDefault(a => (a.Activation & activation) != 0);

                if (abilityToActivate != null)
                {
                    // TODO: Calculate range
                    weaponAttack.Range = abilityToActivate.Range;
                    weaponAttack.TargetingShape = abilityToActivate.TargetingShape;
                    weaponAttack.TargetingShapeSize = abilityToActivate.TargetingShapeSize;
                    weaponAttack.Delay = manager.AbilityActivationSystem.GetDelay(abilityToActivate, ownerEntity)
                        .ToString();
                }

                foreach (var effectEntity in weaponAttack.Effects)
                {
                    var effect = effectEntity.Effect!;
                    if (effect.EffectType == EffectType.Activate)
                    {
                        effect.TargetEntityId = weapon.EntityId;
                    }
                }
            }
            else
            {
                weaponAttack.IsUsable = false;
                return null;
            }
        }

        return weaponAttack;
    }

    private AbilityComponent EnsureAbility(
        Ability abilityTemplate,
        GameEntity ownerEntity,
        GameManager manager)
        => manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityTemplate.Name)]?.Ability
           ?? abilityTemplate.AddToAffectable(ownerEntity);

    private void ResetWieldingAbility(
        AbilityComponent ability,
        WieldingAbility template,
        AbilityComponent? firstWeaponAbility,
        AbilityComponent? secondWeaponAbility,
        ActivationType trigger,
        GameEntity ownerEntity)
    {
        ability.Activation = ActivationType.Targeted;
        ability.Trigger = trigger;
        Debug.Assert(ability.CooldownTick == null && ability.CooldownXpLeft == null);
        ability.IsUsable = firstWeaponAbility != null
                           && (template.WieldingStyle != WieldingStyle.Dual || secondWeaponAbility != null);

        if (!ability.IsUsable)
        {
            return;
        }

        var manager = ownerEntity.Manager;

        // TODO: Only use weapon values when not specified by the ability
        ability.Range = firstWeaponAbility!.Range;
        ability.MinHeadingDeviation = firstWeaponAbility.MinHeadingDeviation;
        ability.MaxHeadingDeviation = firstWeaponAbility.MaxHeadingDeviation;
        ability.TargetingShapeSize = firstWeaponAbility.TargetingShapeSize;
        ability.TargetingShape = firstWeaponAbility.TargetingShape;
        var delay = manager.AbilityActivationSystem.GetDelay(firstWeaponAbility, ownerEntity);

        if (secondWeaponAbility != null)
        {
            if (secondWeaponAbility.Range < ability.Range)
            {
                ability.Range = secondWeaponAbility.Range;
            }

            if (secondWeaponAbility.MinHeadingDeviation > ability.MinHeadingDeviation)
            {
                ability.MinHeadingDeviation = secondWeaponAbility.MinHeadingDeviation;
            }

            if (secondWeaponAbility.MaxHeadingDeviation < ability.MaxHeadingDeviation)
            {
                ability.MaxHeadingDeviation = secondWeaponAbility.MaxHeadingDeviation;
            }

            if (secondWeaponAbility.TargetingShapeSize < ability.TargetingShapeSize)
            {
                ability.TargetingShapeSize = secondWeaponAbility.TargetingShapeSize;
            }

            // TODO: Calculate common shape more accurately
            if (secondWeaponAbility.TargetingShape < ability.TargetingShape)
            {
                ability.TargetingShape = secondWeaponAbility.TargetingShape;
            }

            var secondDelay = manager.AbilityActivationSystem.GetDelay(secondWeaponAbility, ownerEntity);
            if ((secondDelay > delay
                 || secondDelay < 0)
                && delay >= 0)
            {
                delay = secondDelay;
            }
        }

        // TODO: Use an expression instead of precalculating the result
        ability.Delay = delay.ToString();

        GameEntity? firstEffectEntity = null;
        foreach (var effectEntity in ability.Effects)
        {
            var effect = effectEntity.Effect!;
            if (effect.EffectType != EffectType.Activate)
            {
                continue;
            }

            if (firstEffectEntity == null)
            {
                firstEffectEntity = effectEntity;
                effect.TargetEntityId = firstWeaponAbility.EntityId;
                if (template.WieldingStyle == WieldingStyle.OneHanded
                    || template.WieldingStyle == WieldingStyle.TwoHanded)
                {
                    break;
                }
            }
            else
            {
                effect.TargetEntityId = secondWeaponAbility?.EntityId;
                break;
            }
        }
    }

    private bool IsCompatible(
        ItemComponent? weapon, ActivationType activation, ItemType? itemType, EffectType? damageType)
    {
        if (weapon != null
            && (itemType == null || (weapon.Type & itemType.Value) != 0))
        {
            if (damageType == null)
            {
                return true;
            }

            foreach (var abilityEntity in weapon.Abilities)
            {
                if ((abilityEntity.Ability!.Activation & activation) == 0)
                {
                    continue;
                }

                foreach (var effectEntity in abilityEntity.Ability.Effects)
                {
                    if (effectEntity.Effect!.EffectType == damageType)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CanUseWeapons(GameEntity actorEntity)
        => actorEntity.Being!.UpperExtremities == ExtremityType.GraspingFingers;

    private void RecalculateHindrance(GameEntity actorEntity, GameManager manager)
    {
        var being = actorEntity.Being!;
        var hindrance = 0;

        foreach (var itemEntity in actorEntity.Being!.Items)
        {
            var item = itemEntity.Item!;
            if (item.EquippedSlot == EquipmentSlot.None)
            {
                continue;
            }

            var baseMultiplier = 1;
            var handnessMultiplier = GetHandnessMultiplier(item.EquippedSlot);
            if (handnessMultiplier > 0)
            {
                baseMultiplier =
                    manager.SkillAbilitiesSystem.GetHandnessMultiplier(EquipmentSlot.GraspPrimaryMelee);
            }
            else
            {
                handnessMultiplier = 1;
            }

            var template = Item.Loader.Get(item.TemplateName);
            var skillBonus = GetItemSkillBonus(template, actorEntity.Player) * baseMultiplier;

            var requiredMight = template.RequiredMight ?? 0;
            var mightDifference = requiredMight * baseMultiplier - being.Might * handnessMultiplier -
                                  skillBonus;
            var requiredFocus = template.RequiredFocus ?? 0;
            var focusDifference = requiredFocus * baseMultiplier - being.Focus * handnessMultiplier -
                                  skillBonus;
            var requiredSpeed = template.RequiredSpeed ?? 0;
            var speedDifference = requiredSpeed * baseMultiplier - being.Speed * baseMultiplier -
                                  skillBonus;
            var requiredPerception = template.RequiredPerception ?? 0;
            var perceptionDifference = requiredPerception * baseMultiplier - being.Perception * baseMultiplier -
                                       skillBonus;
            var addedHindrance =
                Math.Max(
                    Math.Max(
                        Math.Max(
                            Math.Max(0, mightDifference),
                            focusDifference),
                        speedDifference),
                    perceptionDifference) / baseMultiplier;

            hindrance += addedHindrance;
        }

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(hindrance * 10,
            nameof(BeingComponent.Hindrance), EquippedAbilityName, actorEntity);
    }

    public int GetItemSkillBonus(Item template, PlayerComponent? player)
    {
        if (player == null)
        {
            return 0;
        }

        var itemSkill = GetItemSkill(template?.Type ?? ItemType.None, player);
        if (itemSkill != null)
        {
            var skillDifference = GetRequiredSkillLevel(template?.Complexity) - itemSkill;
            if (skillDifference > 0)
            {
                return skillDifference.Value;
            }
        }

        return 0;
    }

    public int GetHandnessMultiplier(EquipmentSlot slot)
    {
        var multiplier = 0;
        switch (slot)
        {
            case EquipmentSlot.GraspBothMelee:
            case EquipmentSlot.GraspBothRanged:
                multiplier = 5;
                break;
            case EquipmentSlot.GraspPrimaryMelee:
            case EquipmentSlot.GraspPrimaryRanged:
                multiplier = 3;
                break;
            case EquipmentSlot.GraspSecondaryMelee:
            case EquipmentSlot.GraspSecondaryRanged:
                multiplier = 2;
                break;
        }

        return multiplier;
    }

    private int GetRequiredSkillLevel(ItemComplexity? complexity)
    {
        switch (complexity)
        {
            case null:
                return 0;
            case ItemComplexity.Normal:
                return 1;
            case ItemComplexity.Intricate:
                return 2;
            case ItemComplexity.Exotic:
                return 3;
            default:
                throw new InvalidOperationException("Unhandled complexity " + complexity);
        }
    }

    public int? GetItemSkill(ItemType itemType, PlayerComponent player)
    {
        if ((itemType & ItemType.WeaponMeleeHand) != 0)
        {
            return player.HandWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeShort) != 0)
        {
            return player.ShortWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeMedium) != 0)
        {
            return player.MediumWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeLong) != 0)
        {
            return player.LongWeapons;
        }

        if ((itemType & ItemType.WeaponRangedClose) != 0)
        {
            return player.CloseRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedLong) != 0)
        {
            return player.MediumRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedMedium) != 0)
        {
            return player.LongRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedShort) != 0)
        {
            return player.ShortRangeWeapons;
        }

        if ((itemType & ItemType.LightArmor) != 0)
        {
            return player.LightArmor;
        }

        if ((itemType & ItemType.HeavyArmor) != 0)
        {
            return player.HeavyArmor;
        }

        if ((itemType & ItemType.Orb) != 0
            || (itemType & ItemType.Trinket) != 0
            || (itemType & ItemType.Figurine) != 0)
        {
            return player.Artifice;
        }

        return null;
    }

    public bool BuyAbilityLevel(Ability ability, GameEntity playerEntity)
    {
        var manager = playerEntity.Manager;
        var player = playerEntity.Player!;
        var bought = true;
        switch (ability.Type)
        {
            case AbilityType.Skill:
                if (player.SkillPoints < ability.Cost)
                {
                    bought = false;
                    break;
                }

                player.SkillPoints -= ability.Cost;
                break;
            case AbilityType.Trait:
                if (player.TraitPoints < ability.Cost)
                {
                    bought = false;
                    break;
                }

                player.TraitPoints -= ability.Cost;
                break;
            case AbilityType.Mutation:
                if (player.MutationPoints < ability.Cost)
                {
                    bought = false;
                    break;
                }

                player.MutationPoints -= ability.Cost;
                break;
            default:
                throw new InvalidOperationException(
                    $"Ability {ability.Name} of type {ability.Type} cannot be bought");
        }

        if (!bought)
        {
            throw new InvalidOperationException($"Not enough points to buy ability {ability.Name}");
        }

        var effect = manager.EffectApplicationSystem.GetOrAddAbilityEffect(
            playerEntity, ability.Name, EffectApplicationSystem.InnateAbilityName);
        effect.AppliedAmount++;

        return bought;
    }
}

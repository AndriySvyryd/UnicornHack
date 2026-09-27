using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Faculties;

public class SkillAbilitiesSystem : IGameSystem<ItemEquippedMessage>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, ExtremityType>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<EntityRemovedMessage<GameEntity>>
{
    public const string EquippedAbilityName = "equipped";

    // The weapon and armor skill abilities whose level feeds hindrance (and, for weapon skills, the
    // default-attack abilities) of the owning actor. A change to any of these ability levels triggers a
    // recalculation. Membership is keyed by the authored ability template name.
    private static readonly HashSet<string> MeleeWeaponSkillNames =
    [
        AbilityData.HandWeapons.Name,
        AbilityData.ShortWeapons.Name,
        AbilityData.MediumWeapons.Name,
        AbilityData.LongWeapons.Name
    ];

    private static readonly HashSet<string> RangedWeaponSkillNames =
    [
        AbilityData.CloseRangeWeapons.Name,
        AbilityData.ShortRangeWeapons.Name,
        AbilityData.MediumRangeWeapons.Name,
        AbilityData.LongRangeWeapons.Name
    ];

    private static readonly HashSet<string> ArmorSkillNames =
    [
        AbilityData.LightArmor.Name,
        AbilityData.HeavyArmor.Name,
        AbilityData.Artifice.Name
    ];

    // Cached default attack template groups, built lazily on first use.
    private Ability[]? _meleeDefaultAttackTemplates;
    private Ability[]? _rangedDefaultAttackTemplates;

    private Ability[] GetDefaultAttackTemplates(GameManager manager, bool melee)
    {
        var cached = melee ? _meleeDefaultAttackTemplates : _rangedDefaultAttackTemplates;
        if (cached != null)
        {
            return cached;
        }

        var array = manager.Game.Services.Data.Abilities.GetAllValues(
            melee ? AbilityType.DefaultMeleeAttack : AbilityType.DefaultRangedAttack).ToArray();
        if (melee)
        {
            _meleeDefaultAttackTemplates = array;
        }
        else
        {
            _rangedDefaultAttackTemplates = array;
        }
        return array;
    }

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
        if (message.Entity.Ability is not { } ability
            || ability.OwnerEntity is not { } ownerEntity)
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        SynchronizeDependents(ability.Name, ownerEntity, manager);

        if (ability.Name is not { } skillName
            || ownerEntity.Being is not { IsAlive: true })
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        if (MeleeWeaponSkillNames.Contains(skillName))
        {
            RecalculateWeaponAbilities(ownerEntity, melee: true, manager);
            RecalculateHindrance(ownerEntity, manager);
        }
        else if (RangedWeaponSkillNames.Contains(skillName))
        {
            RecalculateWeaponAbilities(ownerEntity, melee: false, manager);
            RecalculateHindrance(ownerEntity, manager);
        }
        else if (ArmorSkillNames.Contains(skillName))
        {
            RecalculateHindrance(ownerEntity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name != nameof(GameManager.AbilitiesToAffectableRelationship))
        {
            throw new InvalidOperationException(message.Group.Name);
        }

        if (message.Entity.Ability is { } ability
            && message.PrincipalEntity is { } owner)
        {
            SynchronizeDependents(ability.Name, owner, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        EntityRemovedMessage<GameEntity> message, GameManager manager)
    {
        if (message.Group.Name != nameof(GameManager.AbilitiesToAffectableRelationship))
        {
            throw new InvalidOperationException(message.Group.Name);
        }

        if (message.RemovedComponent is AbilityComponent ability
            && message.PrincipalEntity is { } owner)
        {
            SynchronizeDependents(ability.Name, owner, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private static void SynchronizeDependents(
        string? abilityName, GameEntity owner, GameManager manager)
    {
        if (abilityName == null
            || Ability.FindTemplate(manager, abilityName, item: false) is not { } template)
        {
            return;
        }

        foreach (var dependent in template.LevelExpressionDependents)
        {
            SynchronizeExpressionAbilityLevel(owner, dependent, manager);
        }
    }

    public static void SynchronizeExpressionAbilityLevel(
        GameEntity targetEntity, Ability template, GameManager manager)
    {
        Debug.Assert(template.LevelExpression != null);
        if (targetEntity.ActiveEffects is not { } activeEffects)
        {
            return;
        }

        ActiveComponentEffect? record = null;
        foreach (var candidate in activeEffects.ComponentEffects.GetValues(template.Name))
        {
            if (IsLevelExpressionGrant(candidate))
            {
                record = candidate;
                break;
            }
        }

        var level = template.GetExpressionLevel(targetEntity);
        if (level <= 0)
        {
            if (record != null)
            {
                EffectApplicationSystem.DetachComponentCarrier(
                    record, targetEntity, manager);
            }

            return;
        }

        if (record == null)
        {
            activeEffects.AddComponentEffect(
                EffectType.AddAbility, template.Name,
                conditionValue: ActiveComponentEffect.LevelExpressionConditionValue,
                level: level);
        }
        else
        {
            if (record.Level == level)
            {
                return;
            }

            record.Level = level;
        }

        manager.EffectApplicationSystem.RelevelAbility(
            template.Name, targetEntity, manager, template);
    }

    public static bool IsLevelExpressionGrant(ActiveComponentEffect record)
        => record is
        {
            EffectType: EffectType.AddAbility,
            ConditionValue: ActiveComponentEffect.LevelExpressionConditionValue
        };

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
                    var primaryWeaponReference = TryCreateNaturalWeapon(actorEntity, being.UpperExtremities, melee, manager);
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
                    var secondaryWeaponReference = TryCreateNaturalWeapon(actorEntity, being.UpperExtremities, melee, manager);
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
        var twoHandedAttack = GetWeaponAttack(twoHandedWeapon, activation);
        var primaryAttack = GetWeaponAttack(primaryWeapon, activation);
        var secondaryAttack = GetWeaponAttack(secondaryWeapon, activation);

        // The wielded weapons determine which single default attack applies (two-handed, dual or one-handed);
        // that template is enabled and slotted while the others are disabled. Each template's Activate effects
        // already target the weapon slots, so the weapon is resolved at activation time.
        var (selectedTemplate, firstAttack, secondAttack) =
            SelectDefaultAttack(melee, twoHandedAttack, primaryAttack, secondaryAttack);

        AbilityComponent? defaultAttackAbility = null;
        foreach (var defaultAttackTemplate in GetDefaultAttackTemplates(manager, melee))
        {
            var ability = EnsureAbility(defaultAttackTemplate, actorEntity, manager);
            if (defaultAttackTemplate == selectedTemplate)
            {
                SetWieldingStats(ability, firstAttack!, secondAttack, activation, actorEntity);
                defaultAttackAbility = ability;
            }
            else
            {
                ability.IsUsable = false;
            }
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(actorEntity, manager);
        setSlotMessage.Slot = melee
            ? AbilitySlottingSystem.DefaultMeleeAttackSlot
            : AbilitySlottingSystem.DefaultRangedAttackSlot;
        setSlotMessage.AbilityEntity = defaultAttackAbility?.Entity;
        manager.Enqueue(setSlotMessage);

        return canUseWeapons;
    }

    private ITransientReference<GameEntity>? TryCreateNaturalWeapon(
        GameEntity ownerEntity, ExtremityType extremityType, bool melee, GameManager manager)
    {
        if (!melee
            || extremityType != ExtremityType.GraspingFingers)
        {
            return null;
        }

        // TODO: Support claws, tentacles, etc.
        var weaponReference = manager.Game.Services.Data.Items.Get("fist").Instantiate(manager);

        if (ownerEntity.Position is { } ownerPosition)
        {
            ItemMovingSystem.SetItemRoom(weaponReference.Referenced, ownerPosition.RoomId);
        }

        return weaponReference;
    }

    // The attack ability the weapon exposes for this attack kind (the source of the default attack's
    // range/delay). Null when there is no weapon in the slot or it has no ability for this kind.
    private static AbilityComponent? GetWeaponAttack(ItemComponent? weapon, ActivationType activation)
        => weapon?.Abilities.Select(a => a.Ability!)
            .FirstOrDefault(a => (a.Activation & activation) != 0);

    // Chooses the default attack template that matches the wielded weapons and returns it together with the
    // weapon attack ability(ies) that supply its targeting and delay. A two-handed weapon takes precedence,
    // then dual wielding (both hands armed), then a single one-handed weapon in either hand.
    private static (Ability? Template, AbilityComponent? First, AbilityComponent? Second) SelectDefaultAttack(
        bool melee, AbilityComponent? twoHandedAttack, AbilityComponent? primaryAttack, AbilityComponent? secondaryAttack)
    {
        if (twoHandedAttack != null)
        {
            return (melee ? AbilityData.TwoHandedMeleeAttack : AbilityData.TwoHandedRangedAttack, twoHandedAttack, null);
        }

        if (primaryAttack != null && secondaryAttack != null)
        {
            return (melee ? AbilityData.DoubleMeleeAttack : AbilityData.DoubleRangedAttack, primaryAttack, secondaryAttack);
        }

        if (primaryAttack != null)
        {
            return (melee ? AbilityData.OneHandedMeleeAttack : AbilityData.OneHandedRangedAttack, primaryAttack, null);
        }

        if (secondaryAttack != null)
        {
            return (melee ? AbilityData.OneHandedMeleeAttack : AbilityData.OneHandedRangedAttack, secondaryAttack, null);
        }

        return (null, null, null);
    }

    private AbilityComponent EnsureAbility(
        Ability abilityTemplate,
        GameEntity ownerEntity,
        GameManager manager)
        => manager.AffectableAbilitiesIndex[(ownerEntity.Id, abilityTemplate.Name)]?.Ability
           ?? abilityTemplate.AddToAffectable(ownerEntity);

    // Applies the selected default attack's targeting and delay from the wielded weapon attack ability(ies)
    // and marks it usable. The Activate effects target the weapon slots statically (from the template), so
    // no per-wielder effect retargeting is needed.
    private void SetWieldingStats(
        AbilityComponent ability,
        AbilityComponent firstWeaponAttack,
        AbilityComponent? secondWeaponAttack,
        ActivationType trigger,
        GameEntity ownerEntity)
    {
        ability.Activation = ActivationType.Targeted;
        ability.Trigger = trigger;
        Debug.Assert(ability.CooldownTick == null && ability.CooldownXpLeft == null);
        ability.IsUsable = true;

        var manager = ownerEntity.Manager;

        // The default attack's targeting and delay are taken from the actual weapon wielded in that slot.
        // TODO: Only use weapon values when not specified by the ability
        ability.Range = firstWeaponAttack.Range;
        ability.MinHeadingDeviation = firstWeaponAttack.MinHeadingDeviation;
        ability.MaxHeadingDeviation = firstWeaponAttack.MaxHeadingDeviation;
        ability.TargetingShapeSize = firstWeaponAttack.TargetingShapeSize;
        ability.TargetingShape = firstWeaponAttack.TargetingShape;
        var delay = manager.AbilityActivationSystem.GetDelay(firstWeaponAttack, ownerEntity);

        if (secondWeaponAttack != null)
        {
            if (secondWeaponAttack.Range < ability.Range)
            {
                ability.Range = secondWeaponAttack.Range;
            }

            if (secondWeaponAttack.MinHeadingDeviation > ability.MinHeadingDeviation)
            {
                ability.MinHeadingDeviation = secondWeaponAttack.MinHeadingDeviation;
            }

            if (secondWeaponAttack.MaxHeadingDeviation < ability.MaxHeadingDeviation)
            {
                ability.MaxHeadingDeviation = secondWeaponAttack.MaxHeadingDeviation;
            }

            if (secondWeaponAttack.TargetingShapeSize < ability.TargetingShapeSize)
            {
                ability.TargetingShapeSize = secondWeaponAttack.TargetingShapeSize;
            }

            // TODO: Calculate common shape more accurately
            if (secondWeaponAttack.TargetingShape < ability.TargetingShape)
            {
                ability.TargetingShape = secondWeaponAttack.TargetingShape;
            }

            var secondDelay = manager.AbilityActivationSystem.GetDelay(secondWeaponAttack, ownerEntity);
            if ((secondDelay > delay
                 || secondDelay < 0)
                && delay >= 0)
            {
                delay = secondDelay;
            }
        }

        // TODO: Use an expression instead of precalculating the result
        ability.Delay = delay.ToString();
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

            var template = manager.Game.Services.Data.Items.Get(item.TemplateName);
            var skillBonus = GetItemSkillBonus(template, actorEntity) * baseMultiplier;

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

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            hindrance * 10, nameof(BeingComponent.Hindrance), actorEntity, manager);
    }

    public int GetItemSkillBonus(Item template, GameEntity actorEntity)
    {
        var itemSkill = GetItemSkill(template?.Type ?? ItemType.None, actorEntity);
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

    public int? GetItemSkill(ItemType itemType, GameEntity actorEntity)
    {
        var skillTemplate = GetItemSkillTemplate(itemType);
        if (skillTemplate == null)
        {
            return null;
        }

        return actorEntity.Manager.AffectableAbilitiesIndex[(actorEntity.Id, skillTemplate.Name)]?.Ability?.Level
                    ?? 0;
    }

    // Maps an item type to the weapon/armor skill ability template whose level determines proficiency with it,
    // or null when no skill governs the item type.
    private static Ability? GetItemSkillTemplate(ItemType itemType)
    {
        if ((itemType & ItemType.WeaponMeleeHand) != 0)
        {
            return AbilityData.HandWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeShort) != 0)
        {
            return AbilityData.ShortWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeMedium) != 0)
        {
            return AbilityData.MediumWeapons;
        }

        if ((itemType & ItemType.WeaponMeleeLong) != 0)
        {
            return AbilityData.LongWeapons;
        }

        if ((itemType & ItemType.WeaponRangedClose) != 0)
        {
            return AbilityData.CloseRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedShort) != 0)
        {
            return AbilityData.ShortRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedMedium) != 0)
        {
            return AbilityData.MediumRangeWeapons;
        }

        if ((itemType & ItemType.WeaponRangedLong) != 0)
        {
            return AbilityData.LongRangeWeapons;
        }

        if ((itemType & ItemType.LightArmor) != 0)
        {
            return AbilityData.LightArmor;
        }

        if ((itemType & ItemType.HeavyArmor) != 0)
        {
            return AbilityData.HeavyArmor;
        }

        if ((itemType & ItemType.Orb) != 0
            || (itemType & ItemType.Trinket) != 0
            || (itemType & ItemType.Figurine) != 0)
        {
            return AbilityData.Artifice;
        }

        return null;
    }

}

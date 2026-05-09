using UnicornHack.Data.Abilities;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;

namespace UnicornHack.Hubs;

public static class GameQueryProcessor
{
    public static List<object?> SerializeSlottedAbility(GameEntity abilityEntity, SerializationContext context)
    {
        var ability = abilityEntity.Ability!;
        return
        [
            null,
            context.Services.Language.GetString(ability),
            (int)ability.Activation,
            ability.Slot,
            ability.CooldownTick,
            ability.CooldownXpLeft,
            (int)ability.TargetingShape,
            ability.TargetingShapeSize
        ];
    }

    public static List<object?> SerializeInventoryItem(GameEntity itemEntity, SerializationContext context)
    {
        var item = itemEntity.Item!;
        var manager = context.Manager;
        var properties = new List<object?>(6)
        {
            null,
            (int)item.Type,
            item.TemplateName,
            context.Services.Language.GetString(item, item.GetQuantity(), true)
        };

        var slots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
            .GetNonRedundantFlags(removeComposites: true)
            .Select(s => SerializeEquipmentSlot(s, context))
            .ToDictionary(s => (int)s[0], s => s);
        properties.Add(slots.Count > 0 ? slots : null);
        properties.Add(item.EquippedSlot == EquipmentSlot.None
            ? null
            : SerializeEquipmentSlot(item.EquippedSlot, context));
        return properties;
    }

    public static List<object?> SerializeItems(GameEntity playerEntity, SerializationContext context)
        =>
        [
            playerEntity.Being!.Items
                .ToDictionary(t => t.Item!.EntityId, t => SerializeInventoryItem(t, context))
        ];

    public static List<object?> SerializeAdaptations(GameEntity playerEntity, SerializationContext context)
    {
        var traits = new List<(string, int)>();
        var mutations = new List<(string, int)>();
        foreach (var effectEntity in playerEntity.Being!.AppliedEffects)
        {
            var effect = effectEntity.Effect!;
            if (effect.EffectType != EffectType.AddAbility
                || effect.TargetName == null
                || !(context.Services.Data.Abilities.Find(effect.TargetName) is LeveledAbility template))
            {
                continue;
            }

            switch (template.Type)
            {
                case AbilityType.Trait:
                    traits.Add((template.Name, effect.AppliedAmount!.Value));
                    break;
                case AbilityType.Mutation:
                    mutations.Add((template.Name, effect.AppliedAmount!.Value));
                    break;
                default:
                    continue;
            }
        }

        return new List<object?>(4)
        {
            playerEntity.Player!.TraitPoints, playerEntity.Player.MutationPoints, traits, mutations
        };
    }

    public static List<object?> SerializeSkills(GameEntity playerEntity, SerializationContext context)
    {
        var manager = context.Manager;
        var playerId = playerEntity.Id;

        return new List<object?>(32)
        {
            null,
            GetAbilityLevel(AbilityData.HandWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.ShortWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.MediumWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.LongWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.CloseRangeWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.ShortRangeWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.MediumRangeWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.LongRangeWeapons.Name, playerId, manager),
            GetAbilityLevel(AbilityData.OneHanded.Name, playerId, manager),
            GetAbilityLevel(AbilityData.TwoHanded.Name, playerId, manager),
            GetAbilityLevel(AbilityData.DualWielding.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Acrobatics.Name, playerId, manager),
            GetAbilityLevel(AbilityData.LightArmor.Name, playerId, manager),
            GetAbilityLevel(AbilityData.HeavyArmor.Name, playerId, manager),
            GetAbilityLevel(AbilityData.AirSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.BloodSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.EarthSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.FireSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.SpiritSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.WaterSourcery.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Conjuration.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Enchantment.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Evocation.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Malediction.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Illusion.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Transmutation.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Assassination.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Stealth.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Artifice.Name, playerId, manager),
            GetAbilityLevel(AbilityData.Leadership.Name, playerId, manager),
            playerEntity.Player!.SkillPoints
        };
    }

    public static List<object?> SerializeActorAttributes(
        GameEntity? actorEntity, bool isIdentified, SerializationContext context)
    {
        if (!isIdentified)
        {
            return SerializationContext.DeletedBitArray;
        }

        var being = actorEntity!.Being!;
        var sensor = actorEntity.Sensor!;
        var physical = actorEntity.Physical!;
        var description = actorEntity.HasComponent(EntityComponent.Player)
            ? ""
            : context.Services.Language.GetDescription(
                actorEntity.Being!.Races.First().Race!.TemplateName,
                DescriptionCategory.Creature);
        var result = new List<object?>(45)
        {
            null,
            context.Services.Language.GetActorName(actorEntity, isIdentified),
            description,
            actorEntity.Manager.XPSystem.GetXPLevel(actorEntity),
            actorEntity.Position!.MovementDelay,
            physical.Size,
            physical.Weight,
            sensor.PrimaryFOVQuadrants,
            sensor.PrimaryVisionRange,
            sensor.TotalFOVQuadrants,
            sensor.SecondaryVisionRange,
            sensor.Infravision,
            sensor.InvisibilityDetection,
            physical.Infravisible,
            being.Visibility,
            being.HitPoints,
            being.HitPointMaximum,
            being.EnergyPoints,
            being.EnergyPointMaximum,
            being.Might,
            being.Speed,
            being.Focus,
            being.Perception,
            being.Regeneration,
            being.EnergyRegeneration,
            being.Armor,
            being.Deflection,
            being.Accuracy,
            being.Evasion,
            being.Hindrance,
            being.PhysicalResistance,
            being.MagicResistance,
            being.BleedingResistance,
            being.AcidResistance,
            being.ColdResistance,
            being.ElectricityResistance,
            being.FireResistance,
            being.PsychicResistance,
            being.ToxinResistance,
            being.VoidResistance,
            being.SonicResistance,
            being.StunResistance,
            being.LightResistance,
            being.WaterResistance
        };

        var isPlayer = actorEntity.HasComponent(EntityComponent.Player);
        var abilities = actorEntity.Being!.Abilities
            .Where(a => a.Ability!.IsUsable
                    && ((!isPlayer
                            && a.Ability.Activation != ActivationType.Default
                            && a.Ability.Activation != ActivationType.Always
                            && a.Ability.Activation != ActivationType.OnMeleeAttack
                            && a.Ability.Activation != ActivationType.OnRangedAttack)
                        || (isPlayer
                            && a.Ability.Type == AbilityType.DefaultAttack)))
            .Select(a => SerializeAbilityAttributes(a, actorEntity, context))
            .ToList();
        result.Add(abilities);

        return result;
    }

    public static List<object?> SerializeAbilityAttributes(
        GameEntity abilityEntity, GameEntity activator, SerializationContext context)
    {
        var manager = context.Manager;
        var ability = abilityEntity.Ability!;

        var activateMessage = ActivateAbilityMessage.Create(manager);
        activateMessage.AbilityEntity = abilityEntity;
        activateMessage.ActivatorEntity = activator;
        activateMessage.TargetEntity = activator;

        var stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
        manager.ReturnMessage(activateMessage);

        var result = new List<object?>(ability.Template == null ? 20 : 21)
        {
            null,
            ability.EntityId,
            context.Services.Language.GetString(ability),
            ability.Level,
            (int)ability.Activation,
            ability.ActivationCondition,
            (int)ability.TargetingShape,
            ability.TargetingShapeSize,
            ability.Range,
            ability.MinHeadingDeviation,
            ability.MaxHeadingDeviation,
            ability.EnergyCost,
            stats.Delay,
            ability.Cooldown,
            ability.CooldownTick == null ? 0 : ability.CooldownTick.Value - manager.Game.CurrentTick,
            ability.XPCooldown,
            ability.CooldownXpLeft ?? 0,
            stats.SelfEffects.ToDictionary(e => e.Effect!.EntityId, e => SerializeEffectAttributes(e, activator, context)),
            stats.SubAttacks.Select(s => SerializeSubAttack(s, activator, context)).ToList()
        };

        if (ability.Template != null)
        {
            // ability.Name may legitimately be null for synthesized sub-abilities (e.g. inner attack
            // abilities of a wielded weapon).
            // TODO: Generate a name for synthesized abilities
            result.Add(ability.Name is { } name
                ? context.Services.Data.Abilities.Find(name)?.EnglishDescription ?? ""
                : "");
        }

        return result;
    }

    public static List<object?> SerializeItemAttributes(
        GameEntity? itemEntity, bool isIdentified, SerializationContext context)
    {
        if (!isIdentified)
        {
            return SerializationContext.DeletedBitArray;
        }

        var manager = itemEntity!.Manager;
        var item = itemEntity.Item!;
        var template = context.Services.Data.Items.Get(item.TemplateName);
        var physical = itemEntity.Physical!;
        var equipableSlots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
            .GetNonRedundantFlags(removeComposites: true)
            .Select(s => SerializeEquipmentSlot(s, context))
            .ToDictionary(s => (int)s[0], s => s);
        return new List<object?>(13)
        {
            null,
            context.Services.Language.GetString(item, item.GetQuantity(), isIdentified),
            context.Services.Language.GetDescription(item.TemplateName, DescriptionCategory.Item),
            (int)item.Type,
            (int)physical.Material,
            physical.Weight,
            (int)(template.Complexity ?? ItemComplexity.Normal),
            template.RequiredMight ?? 0,
            template.RequiredSpeed ?? 0,
            template.RequiredFocus ?? 0,
            template.RequiredPerception ?? 0,
            equipableSlots,
            physical.Abilities
                .Where(a => a.Ability!.IsUsable
                            && a.Ability.Activation != ActivationType.Default
                            && a.Ability.Activation != ActivationType.Always
                            && (a.Ability.Name == null
                                || !a.Ability.Name.StartsWith("Add", StringComparison.Ordinal)
                                || !a.Ability.Name.EndsWith("ability", StringComparison.Ordinal)))
                .Select(a => SerializeAbilityAttributes(a, context.Observer, context)).ToList()
        };
    }

    private static int GetAbilityLevel(string abilityName, int playerId, GameManager manager)
        => manager.AffectableAbilitiesIndex[(playerId, abilityName)]?.Ability!.Level ?? 0;

    private static List<object?> SerializeEffectAttributes(
        GameEntity effectEntity, GameEntity activator, SerializationContext context)
    {
        var effect = effectEntity.Effect!;
        return new List<object?>(8)
        {
            (int)effect.EffectType,
            effect.ShouldTargetActivator,
            effect.AppliedAmount != null
                ? effect.AppliedAmount.ToString()
                : effect.Amount == null
                    ? null
                    : context.Manager.EffectApplicationSystem.GetAmount(effect, activator)
                      + " (" + effect.Amount + ")",
            (int)effect.CombinationFunction,
            effect.TargetName,
            effect.SecondaryAmount,
            (int)effect.Duration,
            effect.DurationAmount
        };
    }

    private static List<object> SerializeSubAttack(SubAttackStats stats, GameEntity activator, SerializationContext context)
        => new(3)
        {
            (int)stats.SuccessCondition,
            stats.Accuracy,
            stats.Effects.Where(e => e.Effect!.EffectType != EffectType.Activate)
                .ToDictionary(e => e.Effect!.EntityId, e => SerializeEffectAttributes(e, activator, context))
        };

    private static List<object> SerializeEquipmentSlot(EquipmentSlot slot, SerializationContext context) => new(3)
    {
        (int)slot,
        context.Services.Language.GetString(slot, context.Observer, abbreviate: true),
        context.Services.Language.GetString(slot, context.Observer, abbreviate: false)
    };

    /// <summary>
    ///     Serializes the room graph for the level the player is currently in. Per-room reveal
    ///     rules:
    ///     <list type="bullet">
    ///         <item>Features and graph topology are always revealed (the picker on the client
    ///             needs to render every cell).</item>
    ///         <item><see cref="RoomEntry.RevealedActorCount" /> /
    ///             <see cref="RoomEntry.RevealedItemCount" /> are populated for the player's
    ///             current room, every adjacent (1 hop) room and every visited room.</item>
    ///         <item><see cref="RoomEntry.StrongestActorName" /> is populated for visited
    ///             rooms only.</item>
    ///     </list>
    ///     "Adjacent" follows non-hidden intra-level connections; hidden corridors don't reveal
    ///     anything until traversed.
    /// </summary>
    public static LevelMap SerializeRoomMap(GameEntity playerEntity)
    {
        var currentRoom = playerEntity.Position?.RoomEntity.Room;
        if (currentRoom == null)
        {
            return new LevelMap();
        }

        var gameLevel = currentRoom.Level;
        var result = new LevelMap();

        // Compute available travel targets based on hostiles + player position.
        // TODO: Check hostility
        var hasHostiles = currentRoom.Actors.Values.Any(a => a.HasComponent(EntityComponent.AI));
        var playerCell = playerEntity.Position!.Cell;

        // Distance 0 = current room; distance 1 = adjacent via non-hidden intra-level
        // connection; everything else stays unrevealed unless visited.
        var adjacentRoomIds = new HashSet<int>();
        foreach (var connectionEntity in currentRoom.Connections.Values)
        {
            var connection = connectionEntity.Connection!;
            var targetRoom = connection.TargetRoomEntity.Room;
            if (targetRoom == null || targetRoom.Level != gameLevel || connection.Hidden)
            {
                continue;
            }
            adjacentRoomIds.Add(targetRoom.EntityId);
        }

        var languageService = playerEntity.Manager.Game.Services.Language;

        foreach (var room in gameLevel.Rooms)
        {
            var connectedTo = new List<int>();
            foreach (var connectionEntity in room.Connections.Values)
            {
                var connection = connectionEntity.Connection!;
                var targetRoom = connection.TargetRoomEntity.Room;
                if (targetRoom == null || targetRoom.Level != room.Level || connection.Hidden)
                {
                    continue;
                }

                connectedTo.Add(targetRoom.EntityId);

                // A target is available if this is the current room AND either there are no
                // hostiles or the player is standing on this specific connection tile.
                if (room.EntityId == currentRoom.EntityId)
                {
                    var connectionTile = connectionEntity.Position!.Cell;
                    if (!hasHostiles || playerCell == connectionTile)
                    {
                        result.AvailableTargets.Add(targetRoom.EntityId);
                    }
                }
            }

            var entry = new RoomEntry
            {
                Id = room.EntityId,
                GridX = room.GridX,
                GridY = room.GridY,
                Features = room.Features,
                IsVisited = room.IsVisited,
                ConnectedTo = connectedTo
            };

            var isCurrent = room.EntityId == currentRoom.EntityId;
            var isClose = isCurrent || adjacentRoomIds.Contains(room.EntityId);
            if (isClose || room.IsVisited)
            {
                entry.RevealedActorCount = isCurrent
                    ? room.Actors.Values.Count(a => a != playerEntity)
                    : room.Actors.Count;
                entry.RevealedItemCount = room.Items.Count;
            }

            if (room.IsVisited)
            {
                GameEntity? strongest = null;
                var bestHp = -1;
                foreach (var actor in room.Actors.Values)
                {
                    if (actor == playerEntity)
                    {
                        continue;
                    }
                    var hp = actor.Being?.HitPointMaximum ?? 0;
                    if (hp > bestHp)
                    {
                        bestHp = hp;
                        strongest = actor;
                    }
                }
                if (strongest != null)
                {
                    entry.StrongestActorName = languageService.GetActorName(strongest, isIdentified: true);
                }
            }

            entry.Tooltip = languageService.GetRoomTooltip(
                entry.Features,
                isCurrent,
                entry.IsVisited,
                entry.RevealedActorCount,
                entry.RevealedItemCount,
                entry.StrongestActorName);

            result.Rooms.Add(entry);
        }

        return result;
    }
}

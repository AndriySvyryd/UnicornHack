using UnicornHack.Data.Abilities;
using UnicornHack.Generation.Effects;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Hubs;

public static class GameQueryProcessor
{
    public static AbilityChange SerializeSlottedAbility(GameEntity abilityEntity, SerializationContext context)
    {
        var ability = abilityEntity.Ability!;
        return new AbilityChange
        {
            ChangedProperties = null,
            Name = context.Services.Language.GetString(ability),
            Activation = (int)ability.Activation,
            Slot = ability.Slot,
            CooldownTick = ability.CooldownTick,
            CooldownXpLeft = ability.CooldownXpLeft,
            TargetingShape = (int)ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize,
            Type = (int)ability.Type,
            Accumulator = AbilityChangeBuilder.GetAccumulator(abilityEntity),
            Level = ability.Level,
            Noise = ability.Noise
        };
    }

    public static Item SerializeInventoryItem(GameEntity itemEntity, SerializationContext context)
    {
        var item = itemEntity.Item!;
        var manager = context.Manager;

        var slots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
            .GetNonRedundantFlags(removeComposites: true)
            .Select(s => SerializeEquipmentSlot(s, context))
            .ToDictionary(s => s.Id);
        return new Item
        {
            ChangedProperties = null,
            Type = item.Type,
            BaseName = item.TemplateName,
            Name = context.Services.Language.GetString(item, item.GetQuantity(), true),
            EquippableSlots = slots,
            EquippedSlot = item.EquippedSlot == EquipmentSlot.None
                ? null
                : SerializeEquipmentSlot(item.EquippedSlot, context)
        };
    }

    public static PlayerInventory SerializeItems(GameEntity playerEntity, SerializationContext context)
        => new()
        {
            Items = playerEntity.Being!.Items
                .ToDictionary(t => t.Item!.EntityId, t => SerializeInventoryItem(t, context))
        };

    public static PlayerAdaptations SerializeAdaptations(GameEntity playerEntity, SerializationContext context)
    {
        var traits = new List<(string, int)>();
        var feats = new List<(string, int)>();

        // Traits and feats are granted abilities carried on the being (AddAbility now records a
        // directive that instantiates the ability rather than a lingering applied effect). Surface each
        // one at its combined level, keyed by the authored template name.
        foreach (var abilityEntity in playerEntity.ActiveEffects!.Abilities)
        {
            var ability = abilityEntity.Ability!;
            if (ability.IsLeveledSupplement
                || ability.Name is not { } name
                || context.Services.Data.Abilities.Find(name) is not LeveledAbility template)
            {
                continue;
            }

            if (template.Type.IsTraitLike())
            {
                traits.Add((template.Name, ability.Level));
            }
            else if (template.Type.IsFeatLike())
            {
                feats.Add((template.Name, ability.Level));
            }
        }

        return new PlayerAdaptations
        {
            Traits = traits.ToDictionary(entry => entry.Item1, entry => entry.Item2),
            Feats = feats.ToDictionary(entry => entry.Item1, entry => entry.Item2)
        };
    }

    public static PlayerSkills SerializeSkills(GameEntity playerEntity, SerializationContext context)
    {
        var manager = context.Manager;
        var playerId = playerEntity.Id;

        return new PlayerSkills
        {
            ChangedProperties = null,
            HandWeapons = GetAbilityLevel(AbilityData.HandWeapons.Name, playerId, manager),
            ShortWeapons = GetAbilityLevel(AbilityData.ShortWeapons.Name, playerId, manager),
            MediumWeapons = GetAbilityLevel(AbilityData.MediumWeapons.Name, playerId, manager),
            LongWeapons = GetAbilityLevel(AbilityData.LongWeapons.Name, playerId, manager),
            CloseRangeWeapons = GetAbilityLevel(AbilityData.CloseRangeWeapons.Name, playerId, manager),
            ShortRangeWeapons = GetAbilityLevel(AbilityData.ShortRangeWeapons.Name, playerId, manager),
            MediumRangeWeapons = GetAbilityLevel(AbilityData.MediumRangeWeapons.Name, playerId, manager),
            LongRangeWeapons = GetAbilityLevel(AbilityData.LongRangeWeapons.Name, playerId, manager),
            OneHanded = GetAbilityLevel(AbilityData.OneHanded.Name, playerId, manager),
            TwoHanded = GetAbilityLevel(AbilityData.TwoHanded.Name, playerId, manager),
            DualWielding = GetAbilityLevel(AbilityData.DualWielding.Name, playerId, manager),
            Acrobatics = GetAbilityLevel(AbilityData.Acrobatics.Name, playerId, manager),
            LightArmor = GetAbilityLevel(AbilityData.LightArmor.Name, playerId, manager),
            HeavyArmor = GetAbilityLevel(AbilityData.HeavyArmor.Name, playerId, manager),
            AirSourcery = GetAbilityLevel(AbilityData.AirSourcery.Name, playerId, manager),
            BloodSourcery = GetAbilityLevel(AbilityData.BloodSourcery.Name, playerId, manager),
            EarthSourcery = GetAbilityLevel(AbilityData.EarthSourcery.Name, playerId, manager),
            FireSourcery = GetAbilityLevel(AbilityData.FireSourcery.Name, playerId, manager),
            SpiritSourcery = GetAbilityLevel(AbilityData.SpiritSourcery.Name, playerId, manager),
            WaterSourcery = GetAbilityLevel(AbilityData.WaterSourcery.Name, playerId, manager),
            Conjuration = GetAbilityLevel(AbilityData.Conjuration.Name, playerId, manager),
            Enchantment = GetAbilityLevel(AbilityData.Enchantment.Name, playerId, manager),
            Evocation = GetAbilityLevel(AbilityData.Evocation.Name, playerId, manager),
            Malediction = GetAbilityLevel(AbilityData.Malediction.Name, playerId, manager),
            Illusion = GetAbilityLevel(AbilityData.Illusion.Name, playerId, manager),
            Transmutation = GetAbilityLevel(AbilityData.Transmutation.Name, playerId, manager),
            Assassination = GetAbilityLevel(AbilityData.Assassination.Name, playerId, manager),
            Stealth = GetAbilityLevel(AbilityData.Stealth.Name, playerId, manager),
            Artifice = GetAbilityLevel(AbilityData.Artifice.Name, playerId, manager),
            Leadership = GetAbilityLevel(AbilityData.Leadership.Name, playerId, manager)
        };
    }

    public static ActorAttributes SerializeActorAttributes(
        GameEntity? actorEntity, bool isIdentified, SerializationContext context)
    {
        if (!isIdentified)
        {
            return new ActorAttributes { ChangedProperties = 0 };
        }

        var being = actorEntity!.Being!;
        var physical = actorEntity.Physical!;
        var description = actorEntity.HasComponent(EntityComponent.Player)
            ? ""
            : context.Services.Language.GetDescription(
                context.Services.Language.GetTemplateName(actorEntity.Being!.Races.First().Ability!, abbreviate: false),
                DescriptionCategory.Creature);
        var result = new ActorAttributes
        {
            ChangedProperties = null,
            Name = context.Services.Language.GetActorName(actorEntity, isIdentified),
            Description = description,
            Level = being.Races.Sum(race => race.Ability!.Level),
            MovementDelay = actorEntity.Position!.MovementDelay,
            Size = physical.Size,
            Weight = physical.Weight,
            PrimaryFOVQuadrants = being.PrimaryFOVQuadrants,
            PrimaryVisionRange = being.PrimaryVisionRange,
            TotalFOVQuadrants = being.TotalFOVQuadrants,
            SecondaryVisionRange = being.SecondaryVisionRange,
            Infravision = being.Infravision,
            InvisibilityDetection = being.InvisibilityDetection,
            Infravisible = physical.Infravisible,
            Visibility = being.Visibility,
            HitPoints = being.HitPoints,
            HitPointMaximum = being.HitPointMaximum,
            EnergyPoints = being.EnergyPoints,
            EnergyPointMaximum = being.EnergyPointMaximum,
            Might = being.Might,
            Speed = being.Speed,
            Focus = being.Focus,
            Perception = being.Perception,
            Regeneration = being.Regeneration,
            EnergyRegeneration = being.EnergyRegeneration,
            Armor = being.Armor,
            Deflection = being.Deflection,
            Accuracy = being.Accuracy,
            Evasion = being.Evasion,
            Hindrance = being.Hindrance,
            PhysicalResistance = being.PhysicalResistance,
            MagicResistance = being.MagicResistance,
            BleedingResistance = being.BleedingResistance,
            AcidResistance = being.AcidResistance,
            ColdResistance = being.ColdResistance,
            ElectricityResistance = being.ElectricityResistance,
            FireResistance = being.FireResistance,
            PsychicResistance = being.PsychicResistance,
            ToxinResistance = being.ToxinResistance,
            VoidResistance = being.VoidResistance,
            SonicResistance = being.SonicResistance,
            StunResistance = being.StunResistance,
            LightResistance = being.LightResistance,
            WaterResistance = being.WaterResistance
        };

        var isPlayer = actorEntity.HasComponent(EntityComponent.Player);
        var abilities = actorEntity.ActiveEffects!.Abilities
            .Where(a => !a.Ability!.IsLeveledSupplement
                        && (a.Ability.Type.IsStatusLike()
                        || (a.Ability.IsUsable
                            && ((!isPlayer
                                    && a.Ability.Activation != ActivationType.Default
                                    && a.Ability.Activation != ActivationType.Always
                                    && a.Ability.Activation != ActivationType.OnMeleeAttack
                                    && a.Ability.Activation != ActivationType.OnRangedAttack)
                                || (isPlayer
                                    && a.Ability.Type.IsDefaultAttack())))))
            .Select(a => SerializeAbilityAttributes(a, actorEntity, context))
            .ToList();
        foreach (var accumulator in actorEntity.ActiveEffects.Accumulators)
        {
            var metadata = Effect.GetMetadata(accumulator.EffectType);
            if (metadata.StatusAbilityName is not { } abilityName
                || actorEntity.Manager.AffectableAbilitiesIndex[(actorEntity.Id, abilityName)] != null)
            {
                continue;
            }

            abilities.Add(new AbilityAttributes
            {
                ChangedProperties = null,
                Id = AbilityChangeBuilder.GetAccumulatorPlaceholderId(accumulator.EffectType),
                Name = AbilityChangeBuilder.GetAccumulatorPlaceholderName(accumulator.EffectType),
                Level = 0,
                Activation = ActivationType.Always,
                Accumulator = accumulator.Value
            });
        }

        result.Abilities = abilities;
        result.Effects = EffectChangeBuilder.SerializeSnapshot(actorEntity);

        return result;
    }

    public static AbilityAttributes SerializeAbilityAttributes(
        GameEntity abilityEntity, GameEntity activator, SerializationContext context)
    {
        var manager = context.Manager;
        var ability = abilityEntity.Ability!;

        AttackStats stats;
        if (ability.Effects.Count == 0)
        {
            stats = new AttackStats();
        }
        else
        {
            var activateMessage = ActivateAbilityMessage.Create(manager);
            activateMessage.AbilityEntity = abilityEntity;
            activateMessage.ActivatorEntity = activator;
            activateMessage.TargetEntity = activator;
            stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
            manager.ReturnMessage(activateMessage);
        }
        var expressionOwner = ability.OwnerEntity is { ActiveEffects: not null } owner
            ? owner
            : activator;

        return new AbilityAttributes
        {
            ChangedProperties = null,
            Id = ability.EntityId,
            Name = context.Services.Language.GetString(ability),
            Level = ability.Level,
            Activation = ability.Activation,
            ActivationCondition = ability.ActivationCondition,
            TargetingShape = ability.TargetingShape,
            TargetingShapeSize = ability.TargetingShapeSize,
            Range = ability.Range,
            MinHeadingDeviation = ability.MinHeadingDeviation,
            MaxHeadingDeviation = ability.MaxHeadingDeviation,
            EnergyCost = ability.EnergyCost,
            Delay = stats.Delay,
            Cooldown = ability.Cooldown,
            CooldownTicksLeft = ability.CooldownTick == null
                ? 0
                : ability.CooldownTick.Value - manager.Game.CurrentTick,
            XpCooldown = ability.XPCooldown,
            XpCooldownLeft = ability.CooldownXpLeft ?? 0,
            Noise = ActorSystemBase.GetAbilityNoise(ability),
            SelfEffects = stats.SelfEffects.ToDictionary(
                e => e.Effect!.EntityId, e => SerializeEffectAttributes(e, activator, context)),
            SubAbilities = stats.SubAttacks.Select(s => SerializeSubAttack(s, activator, context)).ToList(),
            // ability.Name may legitimately be null for synthesized sub-abilities (e.g. inner attack
            // abilities of a wielded weapon).
            // TODO: Generate a name for synthesized abilities
            Description = ability.Template != null && ability.Name is { } name
                ? context.Services.Language.GetDescription(name, DescriptionCategory.Ability)
                : "",
            Accumulator = AbilityChangeBuilder.GetAccumulator(abilityEntity),
            ConditionalGrants = ability.Template?.LevelExpressionDependents
                .OrderBy(dependent => dependent.Name, StringComparer.Ordinal)
                .Select(dependent => new ConditionalGrantAttributes
                {
                    Name = dependent.Name,
                    Level = Math.Max(0, dependent.GetExpressionLevel(expressionOwner)),
                    LevelExpression = dependent.LevelExpression!
                })
                .ToList() ?? []
        };
    }

    public static ItemAttributes SerializeItemAttributes(
        GameEntity? itemEntity, bool isIdentified, SerializationContext context)
    {
        if (!isIdentified)
        {
            return new ItemAttributes { ChangedProperties = 0 };
        }

        var manager = itemEntity!.Manager;
        var item = itemEntity.Item!;
        var template = context.Services.Data.Items.Get(item.TemplateName);
        var physical = itemEntity.Physical!;
        var equipableSlots = manager.ItemUsageSystem.GetEquipableSlots(item, context.Observer.Physical!.Size)
            .GetNonRedundantFlags(removeComposites: true)
            .Select(s => SerializeEquipmentSlot(s, context))
            .ToDictionary(s => s.Id);
        return new ItemAttributes
        {
            ChangedProperties = null,
            Name = context.Services.Language.GetString(item, item.GetQuantity(), isIdentified),
            Description = context.Services.Language.GetDescription(item.TemplateName, DescriptionCategory.Item),
            Type = item.Type,
            Material = physical.Material,
            Weight = physical.Weight,
            Complexity = template.Complexity ?? ItemComplexity.Normal,
            RequiredMight = template.RequiredMight ?? 0,
            RequiredSpeed = template.RequiredSpeed ?? 0,
            RequiredFocus = template.RequiredFocus ?? 0,
            RequiredPerception = template.RequiredPerception ?? 0,
            EquippableSlots = equipableSlots,
            Abilities = physical.Abilities
                .Where(a => !a.Ability!.IsLeveledSupplement
                            && a.Ability.IsUsable
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

    private static EffectAttributes SerializeEffectAttributes(
        GameEntity effectEntity, GameEntity activator, SerializationContext context)
    {
        var effect = effectEntity.Effect!;
        var application = new EffectApplication(effect);
        return new EffectAttributes
        {
            Type = effect.EffectType,
            ShouldTargetActivator = effect.ShouldTargetActivator,
            Amount = effect.Amount == null
                ? null
                : application.GetAmount(context.Manager.EffectApplicationSystem, activator, activator)
                  + " (" + effect.Amount + ")",
            Function = application.CombinationFunction,
            TargetName = effect.TargetName,
            SecondaryAmount = effect.SecondaryAmount,
            Duration = effect.Duration,
            DurationAmount = effect.DurationAmount
        };
    }

    private static SubAbilityAttributes SerializeSubAttack(
        SubAttackStats stats, GameEntity activator, SerializationContext context)
        => new()
        {
            SuccessCondition = stats.SuccessCondition,
            Accuracy = stats.Accuracy,
            Effects = stats.Effects.ToDictionary(
                e => e.Effect!.EntityId, e => SerializeEffectAttributes(e, activator, context))
        };

    private static EquippableSlot SerializeEquipmentSlot(EquipmentSlot slot, SerializationContext context)
        => new()
        {
            Id = (int)slot,
            ShortName = context.Services.Language.GetString(slot, context.Observer, abbreviate: true),
            Name = context.Services.Language.GetString(slot, context.Observer, abbreviate: false)
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
    public static LevelMap SerializeLevelMap(GameEntity playerEntity)
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
            var targetRoom = connection.TargetRoomEntity.Room!;
            if (targetRoom.Level != gameLevel || connection.Hidden)
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
                var targetRoom = connection.TargetRoomEntity.Room!;
                if (targetRoom.Level != room.Level || connection.Hidden)
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
                room.IsVisited,
                entry.RevealedActorCount,
                entry.RevealedItemCount,
                entry.StrongestActorName);

            result.Rooms.Add(entry);
        }

        return result;
    }
}

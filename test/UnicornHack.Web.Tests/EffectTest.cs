using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using Xunit;

namespace UnicornHack;

public class EffectTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (GameManager Manager, GameEntity PlayerEntity, RoomChangeBuilder Listener)
        SetupGameWithListener()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (manager, playerEntity, listener);
    }

    [Fact]
    public void Confused_player_hides_enemy_action_telegraphs_until_restored()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;
        var enemy = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 3));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(player, manager);
        var playerBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == player.Id);

        enemy.AI!.NextAction = ActorActionType.MoveOneCell;
        enemy.AI.NextActionTarget = (int)Direction.South;
        enemy.AI.NextActionTick = manager.Game.CurrentTick + 100;
        var visibleActors = playerBuilder.BuildChangeSet(manager, game.Services).Room!.Actors!;
        var visibleEnemy = Assert.Single(visibleActors);
        var enemyKnowledgeId = visibleEnemy.Key;
        Assert.NotNull(visibleEnemy.Value.NextAction);
        listener.Reset(manager);

        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(GameStateManager.GetState(
            player, game.Services).Room!.Actors![enemyKnowledgeId].NextAction);
        manager.EffectApplicationSystem.RemoveAbility(enemy, AbilityData.Confused.Name, manager);
        manager.Queue.ProcessQueue(manager);
        listener.Reset(manager);

        EffectTestHelper.ApplyStatus(
            player, EffectType.Confuse, EffectTestHelper.GetStatusEffectThreshold(player), manager);
        manager.Queue.ProcessQueue(manager);

        var hiddenDelta = playerBuilder.BuildChangeSet(manager, game.Services)
            .Room!.Actors![enemyKnowledgeId];
        Assert.True(hiddenDelta.ChangedProperties.IsPropertyChanged(12));
        Assert.Null(hiddenDelta.NextAction);
        Assert.Null(GameStateManager.GetState(
            player, game.Services).Room!.Actors![enemyKnowledgeId].NextAction);
        listener.Reset(manager);

        manager.EffectApplicationSystem.RemoveAbility(player, AbilityData.Confused.Name, manager);
        manager.Queue.ProcessQueue(manager);

        var restoredDelta = playerBuilder.BuildChangeSet(manager, game.Services)
            .Room!.Actors![enemyKnowledgeId];
        Assert.True(restoredDelta.ChangedProperties.IsPropertyChanged(12));
        Assert.NotNull(restoredDelta.NextAction);
    }

    [Fact]
    public void Any_capable_player_shares_enemy_action_telegraphs_with_the_room()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;
        var enemy = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 3));
        var confusedPlayer = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        var capablePlayer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(3, 2));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        var confusedPlayerBuilder = listener.PlayerBuilders.Single(
            builder => builder.PlayerEntityId == confusedPlayer.Id);
        var capablePlayerBuilder = listener.PlayerBuilders.Single(
            builder => builder.PlayerEntityId == capablePlayer.Id);

        enemy.AI!.NextAction = ActorActionType.MoveOneCell;
        enemy.AI.NextActionTarget = (int)Direction.South;
        var enemyKnowledgeId = level.KnownActors.Values.Single(
            actor => actor.Knowledge!.KnownEntityId == enemy.Id).Id;
        var confusedPlayerActorChange = confusedPlayerBuilder.BuildChangeSet(manager, game.Services)
            .Room!.Actors![enemyKnowledgeId];
        var capablePlayerActorChange = capablePlayerBuilder.BuildChangeSet(manager, game.Services)
            .Room!.Actors![enemyKnowledgeId];
        Assert.Same(confusedPlayerActorChange, capablePlayerActorChange);
        Assert.NotNull(confusedPlayerActorChange.NextAction);
        listener.Reset(manager);

        EffectTestHelper.ApplyStatus(
            confusedPlayer, EffectType.Confuse, EffectTestHelper.GetStatusEffectThreshold(confusedPlayer), manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(capablePlayer.Being!.CanPerceiveIntention);
        Assert.NotNull(GameStateManager.GetState(
            confusedPlayer, game.Services).Room!.Actors![enemyKnowledgeId].NextAction);
    }

    [Fact]
    public void Full_snapshot_surfaces_active_status_as_an_ability()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();

        // A status rides on the being as a slot-less ability, carrying its related effect accumulator.
        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Chill, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "chilled") > 0);
        var chilledRecord = Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == AbilityData.Chilled.Name);
        chilledRecord.ExpirationEvent = EffectDuration.UntilNextRoom;

        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));

        var chilled = Assert.Single(
            state.Abilities!.Values, a => a.Type == (int)AbilityType.Debuff && a.Name == "Chilled");
        Assert.Equal(0, chilled.Accumulator);
        Assert.Equal((int)EffectDuration.UntilNextRoom, chilled.ExpirationEvent);
        Assert.Empty(state.Effects!);
    }

    [Fact]
    public void Fully_suppressed_ability_remains_visible_with_its_level_reduction()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var playerBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == playerEntity.Id);
        var positiveRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.Greased);
        manager.Queue.ProcessQueue(manager);

        var added = Assert.Single(
            playerBuilder.BuildChangeSet(manager, manager.Game.Services).Abilities!.Values,
            ability => ability.Name == "Greased");
        Assert.Null(added.ChangedProperties);
        Assert.Equal(0, added.LevelReduction);
        var originalCarrierId = positiveRecord.CarrierId;
        listener.Reset(manager);

        var suppressor = new Ability
        {
            Name = "test buff suppressor",
            Activation = ActivationType.Manual,
            Effects =
            [
                new DecreaseAbilityLevel
                {
                    AbilityType = AbilityType.Buff,
                    Duration = EffectDuration.Infinite
                }
            ]
        }.AddToAffectable(playerEntity);
        manager.Queue.ProcessQueue(manager);
        listener.Reset(manager);
        TestHelper.ApplyEffect(
            Assert.Single(suppressor.Effects), playerEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(originalCarrierId, positiveRecord.CarrierId);
        var suppressedEntity = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Greased.Name)]!;
        Assert.Equal(0, suppressedEntity.Ability!.Level);
        Assert.False(suppressedEntity.Ability.IsUsable);
        var suppressed = Assert.Single(
            playerBuilder.BuildChangeSet(manager, manager.Game.Services).Abilities!,
            ability => ability.Key == originalCarrierId).Value;
        Assert.Equal(0, suppressed.Level);
        Assert.Equal(1, suppressed.LevelReduction);
        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));
        Assert.Equal(0, state.Abilities![originalCarrierId].Level);
        Assert.Equal(1, state.Abilities[originalCarrierId].LevelReduction);
        listener.Reset(manager);

        manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
            playerEntity, suppressor.EntityId, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(originalCarrierId, positiveRecord.CarrierId);
        var restored = Assert.Single(
            playerBuilder.BuildChangeSet(manager, manager.Game.Services).Abilities!,
            ability => ability.Key == originalCarrierId).Value;
        Assert.Equal(1, restored.Level);
        Assert.Equal(0, restored.LevelReduction);
    }

    [Fact]
    public void Actor_attributes_surface_status_ability_with_accumulator()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Chill, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        var context = new SerializationContext(null!, playerEntity, manager.Game.Services);
        var attributes = GameQueryProcessor.SerializeActorAttributes(playerEntity, true, context);
        var chilled = Assert.Single(attributes.Abilities, ability => ability.Name == "Chilled");

        Assert.Equal(0, chilled.Accumulator);
        Assert.Empty(attributes.Effects);
    }

    [Fact]
    public void Actor_attributes_surface_nonzero_accumulator_on_present_status_ability()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();
        const int residual = 37;

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, EffectApplicationSystem.BuildUpThreshold, playerEntity, manager);
        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, residual, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Stunned.Name) > 0);
        Assert.Equal(residual, playerEntity.ActiveEffects!.Accumulators[EffectType.Stun].Value);

        var context = new SerializationContext(null!, playerEntity, manager.Game.Services);
        var attributes = GameQueryProcessor.SerializeActorAttributes(playerEntity, true, context);
        var stunned = Assert.Single(attributes.Abilities, ability => ability.Name == "Stunned");

        Assert.Equal(residual, stunned.Accumulator);
    }

    [Fact]
    public void Supplemental_status_carriers_are_not_serialized_to_the_client()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var playerBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == playerEntity.Id);

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, EffectApplicationSystem.BuildUpThreshold * 2,
            playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotNull(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Stunned.Name + "#2")]);

        var delta = playerBuilder.BuildChangeSet(manager, manager.Game.Services);
        var deltaStatus = Assert.Single(
            delta.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Debuff);
        Assert.Equal("Stunned", deltaStatus.Name);
        Assert.Equal(2, deltaStatus.Level);

        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));
        var stateStatus = Assert.Single(
            state.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Debuff);
        Assert.Equal("Stunned", stateStatus.Name);
        Assert.Equal(2, stateStatus.Level);

        var context = new SerializationContext(null!, playerEntity, manager.Game.Services);
        var attributes = GameQueryProcessor.SerializeActorAttributes(playerEntity, true, context);
        var detailStatus = Assert.Single(
            attributes.Abilities,
            ability => ability.Name?.StartsWith("Stunned", StringComparison.Ordinal) == true);
        Assert.Equal("Stunned", detailStatus.Name);
        Assert.Equal(2, detailStatus.Level);
    }

    [Fact]
    public void Actor_attributes_surface_nonzero_accumulator_without_status_ability()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();
        var buildup = EffectApplicationSystem.BuildUpThreshold - 1;

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, buildup, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Stunned.Name));
        Assert.Equal(buildup, playerEntity.ActiveEffects!.Accumulators[EffectType.Stun].Value);

        var context = new SerializationContext(null!, playerEntity, manager.Game.Services);
        var attributes = GameQueryProcessor.SerializeActorAttributes(playerEntity, true, context);
        var stunned = Assert.Single(attributes.Abilities, ability => ability.Name == "Stunned");

        Assert.Equal(0, stunned.Level);
        Assert.Equal(buildup, stunned.Accumulator);
        Assert.Equal(
            AbilityChangeBuilder.GetAccumulatorPlaceholderId(EffectType.Stun),
            stunned.Id);

        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));
        var placeholder = Assert.Single(
            state.Abilities!.Values,
            ability => ability.Name == "Stunned");
        Assert.Equal(0, placeholder.Level);
        Assert.Equal(buildup, placeholder.Accumulator);
    }

    [Fact]
    public void Accumulator_without_status_ability_is_added_updated_and_replaced_by_the_status_carrier()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var playerBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == playerEntity.Id);
        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));
        const int residual = 37;
        var placeholderId = AbilityChangeBuilder.GetAccumulatorPlaceholderId(EffectType.Stun);

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, residual, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var added = playerBuilder.BuildChangeSet(manager, manager.Game.Services);
        var placeholder = Assert.Single(added.Abilities!, ability => ability.Key == placeholderId).Value;
        Assert.Null(placeholder.ChangedProperties);
        Assert.Equal("Stunned", placeholder.Name);
        Assert.Equal(residual, placeholder.Accumulator);
        ClientStateHelpers.ApplyChangeSets(state, [new TurnChangeSet { PlayerState = added }]);
        Assert.Equal(residual, state.Abilities![placeholderId].Accumulator);
        listener.Reset(manager);

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, 5, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var updated = playerBuilder.BuildChangeSet(manager, manager.Game.Services);
        placeholder = Assert.Single(updated.Abilities!, ability => ability.Key == placeholderId).Value;
        Assert.True(placeholder.ChangedProperties.IsPropertyChanged(9));
        Assert.Equal(residual + 5, placeholder.Accumulator);
        ClientStateHelpers.ApplyChangeSets(state, [new TurnChangeSet { PlayerState = updated }]);
        Assert.Equal(residual + 5, state.Abilities![placeholderId].Accumulator);
        listener.Reset(manager);

        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.Stun, EffectApplicationSystem.BuildUpThreshold - residual - 5,
            playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var activated = playerBuilder.BuildChangeSet(manager, manager.Game.Services);
        var activatedAbilities = activated.Abilities!;
        Assert.Equal(
            (ushort?)0,
            Assert.Single(
                activatedAbilities, ability => ability.Key == placeholderId)
                .Value.ChangedProperties);
        var status = Assert.Single(
            activatedAbilities,
            ability => ability.Key != placeholderId
                       && ability.Value.Name == "Stunned").Value;
        Assert.Equal(1, status.Level);
        Assert.Equal(0, status.Accumulator);
        ClientStateHelpers.ApplyChangeSets(state, [new TurnChangeSet { PlayerState = activated }]);
        Assert.DoesNotContain(placeholderId, state.Abilities!.Keys);
        var activeStatus = Assert.Single(
            state.Abilities,
            ability => ability.Value.Name == "Stunned" && ability.Value.Level == 1);
        listener.Reset(manager);

        manager.EffectApplicationSystem.RemoveAbility(
            playerEntity, AbilityData.Stunned.Name, manager);
        manager.Queue.ProcessQueue(manager);

        var deactivated = playerBuilder.BuildChangeSet(manager, manager.Game.Services);
        var deactivatedAbilities = deactivated.Abilities!;
        Assert.Equal(
            (ushort?)0,
            Assert.Single(deactivatedAbilities, ability => ability.Key == activeStatus.Key)
                .Value.ChangedProperties);
        placeholder = Assert.Single(
            deactivatedAbilities, ability => ability.Key == placeholderId).Value;
        Assert.Null(placeholder.ChangedProperties);
        Assert.Equal(0, placeholder.Level);
        Assert.Equal(0, placeholder.Accumulator);
        ClientStateHelpers.ApplyChangeSets(state, [new TurnChangeSet { PlayerState = deactivated }]);
        Assert.DoesNotContain(activeStatus.Key, state.Abilities.Keys);
        Assert.Equal(0, state.Abilities[placeholderId].Accumulator);
    }

    [Fact]
    public void Incapacitation_buildup_is_in_force_only_once_it_covers_an_action()
    {
        // An incapacitation accumulator banks its build-up in its Value; it is in
        // force only once that covers a full action interval - a dynamic, per-target threshold - which is
        // exactly when the "stunned" status ability is applied to mark the condition.
        var (manager, playerEntity, _) = SetupGameWithListener();

        // Resistance tuned so a single stun hit banks just under the target's own action-interval threshold
        // (a dynamic, per-target value), so it doesn't take hold; a second hit pushes it over. A hit
        // contributes 100 - StunResistance.
        var threshold = EffectTestHelper.GetStatusEffectThreshold(playerEntity);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            101 - threshold, nameof(BeingComponent.StunResistance), "test", playerEntity);
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.False(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);

        // A second hit pushes the build-up over the threshold and the stun takes hold.
        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);
    }

    [Fact]
    public void Full_snapshot_surfaces_in_force_incapacitation_as_an_ability()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);

        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));

        // The in-force incapacitation surfaces as its status ability and carries the current build-up.
        var stunned = Assert.Single(
            state.Abilities!.Values, a => a.Type == (int)AbilityType.Debuff && a.Name == "Stunned");
        Assert.Equal(0, stunned.Accumulator);
        Assert.Equal(1, stunned.Level);
        Assert.NotNull(stunned.ExpirationTarget);
        Assert.Equal((int)EffectDuration.UntilTimeout, stunned.ExpirationEvent);
    }

    [Fact]
    public void In_force_incapacitation_round_trips_through_change_sets()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var player = playerEntity.Player!;
        var services = manager.Game.Services;

        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));

        // A full-strength stun (no resistance) takes hold at once as an accumulator instance.
        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);

        // The change sets must carry the incapacitation add (and any subsequent removal as it is spent)
        // so the client state converges to a fresh snapshot without divergence or an orphaned removal.
        var changeSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        if (changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets))
        {
            ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets!);
        }

        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void Status_level_and_expiration_changes_are_delivered_as_ability_deltas()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var services = manager.Game.Services;
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        var playerBuilder = Assert.Single(
            listener.PlayerBuilders, builder => builder.PlayerEntityId == playerEntity.Id);

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        var statusAbilityEntity = manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Stunned.Name)]!;
        var statusAbilityId = statusAbilityEntity.Id;
        var record = Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.CarrierId == statusAbilityId);
        var addChangeSet = new TurnChangeSet
        {
            PlayerState = playerBuilder.BuildChangeSet(manager, services)
        };
        ClientStateHelpers.ApplyChangeSets(baseline, [addChangeSet]);
        listener.Reset(manager);

        var statusEntry = new KeyValuePair<int, AbilityChange>(
            statusAbilityId, baseline.Abilities![statusAbilityId]);
        Assert.Equal("Stunned", statusEntry.Value.Name);

        manager.Game.CurrentTick += 10;
        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Stun, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        record.ExpirationEvent = EffectDuration.UntilXPGained;
        record.ExpirationTarget = 25;

        var playerChange = playerBuilder.BuildChangeSet(manager, services);
        var abilityDelta = playerChange.Abilities![statusEntry.Key];

        Assert.NotNull(abilityDelta.ChangedProperties);
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(10));
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(12));
        Assert.True(abilityDelta.ChangedProperties.IsPropertyChanged(13));
        Assert.Equal(2, abilityDelta.Level);
        Assert.Equal(25, abilityDelta.ExpirationTarget);
        Assert.Equal((int)EffectDuration.UntilXPGained, abilityDelta.ExpirationEvent);

        ClientStateHelpers.ApplyChangeSets(
            baseline, [new TurnChangeSet { PlayerState = playerChange }]);
        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void Full_snapshot_excludes_non_status_effects()
    {
        var (manager, playerEntity, _) = SetupGameWithListener();

        // A raw property modifier is an applied effect but not a displayable condition.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            5, nameof(BeingComponent.HitPointMaximum), "test", playerEntity);
        playerEntity.ActiveEffects!.AddPropertyEffect(
            EffectType.Heal,
            magnitudePrimary: 5,
            expirationTarget: manager.Game.CurrentTick + 100,
            expirationEvent: EffectDuration.UntilTimeout);
        manager.Queue.ProcessQueue(manager);

        var state = ClientStateHelpers.Deserialize(
            GameStateManager.GetState(playerEntity, manager.Game.Services));

        Assert.DoesNotContain(state.Abilities!.Values, ability => ability.Type == (int)AbilityType.Debuff);
        Assert.Empty(state.Effects!);
    }

    [Fact]
    public void Applying_a_status_is_delivered_as_an_ability_delta()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var player = playerEntity.Player!;
        var services = manager.Game.Services;

        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        Assert.DoesNotContain(baseline.Abilities!.Values, a => a.Name == "Confused");

        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Confuse, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "confused") > 0);

        var changeSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        Assert.True(changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets));

        ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets!);

        var confused = Assert.Single(
            baseline.Abilities!.Values, a => a.Type == (int)AbilityType.Debuff && a.Name == "Confused");
        Assert.Equal(0, confused.Accumulator);

        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void Expiring_a_status_removes_its_ability_via_change_sets()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var player = playerEntity.Player!;
        var services = manager.Game.Services;

        // Start with no statuses known to the client.
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));

        // A long build-up takes hold and stays in force across the flush turn, so the add reaches the client
        // (a build-up spent below its threshold within a single turn would be added-and-lifted in one cycle,
        // which is the separate add-delta case).
        EffectTestHelper.ApplyStatus(playerEntity, EffectType.Confuse, duration: 1000, manager);
        manager.Queue.ProcessQueue(manager);

        var addSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        ClientStateHelpers.ApplyChangeSets(baseline, addSets[playerEntity.Id]);
        Assert.Contains(baseline.Abilities!.Values, a => a.Type == (int)AbilityType.Debuff && a.Name == "Confused");

        // Waiting draws the build-up down each turn; once it is spent the status ability is lifted, and that
        // removal must be carried to the client. Advance until the server lifts it (a cap guards against a
        // never-ending loop) and apply each turn's change set so the client mirror tracks the removal.
        for (var i = 0; i < 30 && (EffectApplicationSystem.GetAbilityLevel(playerEntity, "confused") > 0); i++)
        {
            var sets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
            if (sets.TryGetValue(playerEntity.Id, out var s))
            {
                ClientStateHelpers.ApplyChangeSets(baseline, s!);
            }
        }

        Assert.False(EffectApplicationSystem.GetAbilityLevel(playerEntity, "confused") > 0);
        Assert.DoesNotContain(baseline.Abilities!.Values, a => a.Type == (int)AbilityType.Debuff && a.Name == "Confused");

        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void A_damage_over_time_ailment_round_trips_as_an_effect_change()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var player = playerEntity.Player!;
        var services = manager.Game.Services;
        var being = playerEntity.Being!;

        // Keep the player alive across the bleed so the condition - not death - is what we observe.
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", playerEntity);
        manager.Queue.ProcessQueue(manager);
        being.HitPoints = 1000;

        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));

        // A slashing ability's authored bleed damage-over-time is applied directly to bank its lingering
        // instance. DoTs are the only active effects transported directly.
        GameEntity bleedAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            bleedAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = playerEntity;
            bleedAbility.Ability = ability;
        }

        GameEntity bleedEffect;
        using (var effectReference = manager.CreateEntity())
        {
            bleedEffect = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.BleedDamage;
            effect.Duration = EffectDuration.UntilTimeout;
            effect.Amount = "100";
            effect.DurationAmount = "1000";
            effect.ContainingAbilityId = bleedAbility.Id;
            bleedEffect.Effect = effect;
        }

        TestHelper.ApplyEffect(bleedEffect, playerEntity, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        var activeBleed = Assert.Single(
            playerEntity.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.BleedDamage);

        var afterApply = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        Assert.Contains(
            afterApply.Abilities!.Values,
            ability => ability.Type == (int)AbilityType.Debuff);
        var bleed = Assert.Single(afterApply.Effects!.Values);
        Assert.Equal(EffectType.BleedDamage, bleed.EffectType);
        Assert.Equal(100, bleed.Magnitude);

        var context = new SerializationContext(null!, playerEntity, services);
        var attributes = GameQueryProcessor.SerializeActorAttributes(playerEntity, true, context);
        Assert.Equal(EffectType.BleedDamage, Assert.Single(attributes.Effects.Values).EffectType);

        var changeSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        Assert.True(changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets));
        ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets!);

        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        activeBleed.MagnitudePrimary = 50;
        var updateSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        ClientStateHelpers.ApplyChangeSets(baseline, updateSets[playerEntity.Id]);
        Assert.Equal(50, Assert.Single(baseline.Effects!.Values).Magnitude);
        fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        activeBleed.CombinationFunction = ValueCombinationFunction.Sum;
        var suppressSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        ClientStateHelpers.ApplyChangeSets(baseline, suppressSets[playerEntity.Id]);
        Assert.Empty(baseline.Effects!);
        fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        activeBleed.CombinationFunction = ValueCombinationFunction.Subtract;
        var restoreSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        ClientStateHelpers.ApplyChangeSets(baseline, restoreSets[playerEntity.Id]);
        Assert.Equal(50, Assert.Single(baseline.Effects!.Values).Magnitude);
        fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        playerEntity.ActiveEffects.PropertyEffects.Remove(activeBleed);
        var removeSets = WebTestHelper.PerformAction(player, listener, services, ActorActionType.Wait);
        ClientStateHelpers.ApplyChangeSets(baseline, removeSets[playerEntity.Id]);
        Assert.Empty(baseline.Effects!);
        fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void Player_death_ends_the_turn_without_a_next_action_tick()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var player = playerEntity.Player!;
        playerEntity.Being!.HitPoints = 1;
        playerEntity.ActiveEffects!.AddPropertyEffect(
            EffectType.BleedDamage,
            combinationFunction: ValueCombinationFunction.Subtract,
            magnitudePrimary: 100,
            expirationEvent: EffectDuration.UntilTimeout,
            expirationTarget: manager.Game.CurrentTick + 1000);

        var changeSets = WebTestHelper.PerformAction(
            player, listener, manager.Game.Services, ActorActionType.Wait);

        Assert.False(playerEntity.Being.IsAlive);
        Assert.Null(player.NextActionTick);
        Assert.True(changeSets.ContainsKey(playerEntity.Id));
    }

    [Fact]
    public void Add_then_remove_damage_over_time_does_not_emit_an_effect_change()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var services = manager.Game.Services;
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));

        var transient = playerEntity.ActiveEffects!.AddPropertyEffect(
            EffectType.BleedDamage,
            combinationFunction: ValueCombinationFunction.Subtract,
            magnitudePrimary: 10,
            expirationEvent: EffectDuration.UntilTimeout,
            expirationTarget: manager.Game.CurrentTick + 100);
        playerEntity.ActiveEffects.PropertyEffects.Remove(transient);

        var changeSets = WebTestHelper.PerformAction(
            playerEntity.Player!, listener, services, ActorActionType.Wait);
        foreach (var changeSet in changeSets.GetValueOrDefault(playerEntity.Id) ?? [])
        {
            Assert.Null(changeSet.PlayerState.Effects);
        }

        if (changeSets.TryGetValue(playerEntity.Id, out var playerChangeSets))
        {
            ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets!);
        }

        Assert.Empty(baseline.Effects!);
        var fresh = ClientStateHelpers.Deserialize(GameStateManager.GetState(playerEntity, services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);
    }

    [Fact]
    public void Player_active_effects_and_resolved_stats_survive_save_and_load()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var player = persisted.PlayerEntity;
        var effects = player.ActiveEffects!;

        var propertyEffectCount = effects.PropertyEffects.Count;
        var might = player.Being!.Might;
        var maxHp = player.Being.HitPointMaximum;
        var evasion = player.Being.Evasion;
        var capacity = player.Physical!.Capacity;

        Assert.True(propertyEffectCount > 0, "Expected the seeded player to carry innate property effects.");

        persisted.Reload();

        var reloadedPlayer = persisted.PlayerEntity;
        var reloadedEffects = reloadedPlayer.ActiveEffects!;

        Assert.Equal(propertyEffectCount, reloadedEffects.PropertyEffects.Count);
        Assert.Equal(might, reloadedPlayer.Being!.Might);
        Assert.Equal(maxHp, reloadedPlayer.Being.HitPointMaximum);
        Assert.Equal(evasion, reloadedPlayer.Being.Evasion);
        Assert.Equal(capacity, reloadedPlayer.Physical!.Capacity);
    }

    [Fact]
    public void Stun_condition_reconstructs_from_the_persisted_buildup_after_reload()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;

        // Repeated stun applications accumulate past the build-up threshold, putting the condition in
        // force. The build-up accumulator persists; the in-force stun condition is reconstructed from the
        // persisted state on load.
        for (var i = 0; i < 3; i++)
        {
            ApplyStun(manager, player);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.True(
            EffectApplicationSystem.GetAbilityLevel(player, "stunned") > 0,
            "Expected the accumulated stun to be in force before saving.");
        persisted.Context.SaveChanges();

        persisted.Reload();

        var reloadedPlayer = persisted.PlayerEntity;
        Assert.True(
            EffectApplicationSystem.GetAbilityLevel(reloadedPlayer, "stunned") > 0,
            "The stun condition should reconstruct from the persisted build-up accumulator after a reload.");
    }

    [Fact]
    public void An_equip_toggle_modifier_survives_save_and_load_as_a_source_linked_instance()
    {
        Assert.SkipUnless(
            PersistedGame.IsLocalDbAvailable, "SQL Server LocalDB is not available in this environment.");

        using var persisted = PersistedGame.CreateAndSave();

        var manager = persisted.Game.Manager;
        var player = persisted.PlayerEntity;
        var baseAcid = player.Being!.AcidResistance;

        // A toggled ability (an equipped item's WhileEquipped / a toggled-on skillbook) contributing a
        // source-linked Infinite ChangeProperty, stored as an ActivePropertyEffect instance (retirement 8).
        GameEntity toggledAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            toggledAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test aegis";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = player;
            toggledAbility.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.AcidResistance);
            effect.Amount = "15";
            effect.ContainingAbilityId = toggledAbility.Id;
            effectReference.Referenced.Effect = effect;
        }

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;
        setSlotMessage.Slot = 2;
        manager.Enqueue(setSlotMessage);

        var activateMessage = ActivateAbilityMessage.Create(manager);
        activateMessage.ActivatorEntity = player;
        activateMessage.TargetEntity = player;
        activateMessage.AbilityEntity = toggledAbility;
        manager.Enqueue(activateMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(baseAcid + 15, player.Being.AcidResistance);
        Assert.Contains(player.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.ChangeProperty
                 && i.TargetName == nameof(BeingComponent.AcidResistance)
                 && i.SourceAbilityEntityId == toggledAbility.Id);

        persisted.Context.SaveChanges();
        persisted.Reload();

        // The instance persists (OwnsMany) and its definition is rebuilt on load, so the resolved
        // resistance the toggled ability granted is intact without re-running the apply path.
        var reloadedPlayer = persisted.PlayerEntity;
        Assert.Equal(baseAcid + 15, reloadedPlayer.Being!.AcidResistance);
        Assert.Contains(reloadedPlayer.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.ChangeProperty
                 && i.TargetName == nameof(BeingComponent.AcidResistance));
    }

    private static void ApplyStun(GameManager manager, GameEntity target)
    {
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stun, target, amount: 100));
    }
}

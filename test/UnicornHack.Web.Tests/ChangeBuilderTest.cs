using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class ChangeBuilderTest
{
    private const string TestMap = @"
.....
.....
.....
.....
.....";

    private static (GameManager Manager, GameEntity PlayerEntity, LevelChangeBuilder Listener)
        SetupGameWithListener()
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        return (manager, playerEntity, listener);
    }

    private static void EnqueueSetSlot(GameManager manager, GameEntity playerEntity, GameEntity abilityEntity, int? slot)
    {
        var msg = SetAbilitySlotMessage.Create(manager);
        msg.AbilityEntity = abilityEntity;
        msg.OwnerEntity = playerEntity;
        msg.Slot = slot;
        manager.Enqueue(msg);
    }

    /// <summary>
    ///     When an ability's slot is cleared and then re-assigned within the same
    ///     change-tracking cycle (Deleted → Added on the same entity), the builder
    ///     should emit a delta — not a full snapshot — because the client already
    ///     knew about this entity from a prior turn.
    /// </summary>
    [Fact]
    public void DeletedThenAdded_ProducesDelta()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        // Equip sword — creates attack ability in slot 0
        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        // Capture baseline — client now knows the ability in slot 0
        var baseline = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        var slottedAbility = baseline.Abilities!.Values.Single(a => a.Slot == 0);
        var abilityEntityId = baseline.Abilities.Single(kvp => kvp.Value == slottedAbility).Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;

        // Clear the slot then re-assign it in the same queue cycle (Deleted → Added).
        EnqueueSetSlot(manager, playerEntity, abilityEntity, null);
        EnqueueSetSlot(manager, playerEntity, abilityEntity, 0);
        manager.Queue.ProcessQueue(manager);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 0);
    }

    /// <summary>
    ///     A brand-new ability gains a slot then loses it within a single cycle.
    ///     The client never saw it, so the entry must be cancelled — no emission,
    ///     and the ability must not appear in the resulting state.
    /// </summary>
    [Fact]
    public void AddedThenRemoved_Cancels()
    {
        // Custom setup: clear the auto-slotted flask ability before the listener is
        // registered, so the client's initial state does not contain it.
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FlaskOfHealing.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        var ability = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, "flask of healing: Drink")]!;
        Assert.NotNull(ability);
        ability.Ability!.Slot = null;

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        var player = playerEntity.Player!;

        Assert.DoesNotContain(ability.Id,
            ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services)).Abilities!.Keys);

        // Queue Slot=N then Slot=null on the same ability so both transitions are
        // processed within one Turn. The listener observes Add + Remove on an entity
        // the client did not know about — the entry must be cancelled rather than
        // emitting a removal sentinel for an unknown id.
        EnqueueSetSlot(manager, playerEntity, ability, 3);
        EnqueueSetSlot(manager, playerEntity, ability, null);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        Assert.DoesNotContain(ability.Id, fresh.Abilities!.Keys);
    }

    /// <summary>
    ///     A known ability has a property change followed by slot clearing in the
    ///     same cycle. Emission should be a removal sentinel — the property
    ///     change is irrelevant once the ability leaves the client's view.
    /// </summary>
    [Fact]
    public void ModifiedThenRemoved_EmitsRemovalSentinel()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        var slottedAbility = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services))
            .Abilities!.Single(kvp => kvp.Value.Slot == 0);
        var abilityEntityId = slottedAbility.Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;

        // Mutate a property directly and queue the slot-clear so it is
        // processed in the upcoming Turn. PerformAndVerify's baseline (captured
        // before Turn runs) contains the ability with the new TargetingShapeSize;
        // the change set must then emit a removal sentinel — the property bit is
        // irrelevant once the ability leaves the client's view.
        // (CooldownTick can't be used here because SetAbilitySlotMessage gates on
        // it: slot=null with a non-null cooldown returns StopProcessing without
        // clearing the slot.)
        abilityEntity.Ability!.TargetingShapeSize += 7;
        EnqueueSetSlot(manager, playerEntity, abilityEntity, null);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, game.Services, ActorActionType.Wait);

        Assert.DoesNotContain(abilityEntityId, fresh.Abilities!.Keys);
    }

    /// <summary>
    ///     Clear slot, change a property, restore slot in one cycle. The reconcile
    ///     should keep the property change but drop the round-tripped slot bit so
    ///     the emitted delta carries only the genuine change.
    /// </summary>
    [Fact]
    public void ReAddedWithPropertyChange_PreservesChangedBitOnly()
    {
        var (manager, playerEntity, listener) = SetupGameWithListener();
        var game = manager.Game;
        var player = playerEntity.Player!;
        var swordEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        WebTestHelper.PerformAndVerify(player, listener, game.Services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        // Take baseline BEFORE mutations so we can verify the delta carries the
        // genuine TargetingShapeSize change. (PerformAndVerify's baseline is
        // captured after listener mutations and would falsely flag this as
        // redundant.)
        var baseline = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        var slottedAbility = baseline.Abilities!.Values.Single(a => a.Slot == 0);
        var abilityEntityId = baseline.Abilities.Single(kvp => kvp.Value == slottedAbility).Key;
        var abilityEntity = manager.FindEntity(abilityEntityId)!;
        var originalSize = abilityEntity.Ability!.TargetingShapeSize;

        // Clear slot, change a non-gating property, restore slot — all in one cycle.
        abilityEntity.Ability!.Slot = null;
        abilityEntity.Ability!.TargetingShapeSize = originalSize + 7;
        abilityEntity.Ability!.Slot = 0;

        var changeSets = WebTestHelper.PerformAction(player, listener, game.Services, ActorActionType.Wait);
        var playerChangeSets = changeSets[playerEntity.Id];

        // Locate the ability delta in the emitted change sets.
        AbilityChange? abilityDelta = null;
        foreach (var changeSet in playerChangeSets)
        {
            if (changeSet.PlayerState.Abilities is { } abilities
                && abilities.TryGetValue(abilityEntityId, out var d))
            {
                abilityDelta = d;
                break;
            }
        }

        Assert.NotNull(abilityDelta);
        Assert.NotNull(abilityDelta!.ChangedProperties);
        Assert.True(abilityDelta.ChangedProperties.Length > 1,
            "Expected a delta, not a removal sentinel.");
        Assert.False(abilityDelta.ChangedProperties[3],
            "Slot bit should be cleared by reconcile (round-tripped to original).");
        Assert.True(abilityDelta.ChangedProperties[7],
            "TargetingShapeSize bit should be preserved (genuinely changed).");

        // Apply and verify final state aligns with a fresh server snapshot.
        ClientStateHelpers.ApplyChangeSets(baseline, playerChangeSets);
        var fresh = ClientStateHelpers.Deserialize(WebTestHelper.GetState(playerEntity, game.Services));
        ChangeSetAssert.AssertPlayerStatesEqual(fresh, baseline);

        Assert.Equal(0, fresh.Abilities![abilityEntityId].Slot);
        Assert.Equal(originalSize + 7, fresh.Abilities![abilityEntityId].TargetingShapeSize);
    }
}

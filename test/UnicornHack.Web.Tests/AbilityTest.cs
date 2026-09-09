using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Generation.Effects;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class AbilityTest
{
    private const string TestMap = @"
.......
.......
.......
.......
.......
.......
.......";

    private static (PlayerComponent Player, RoomChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame(bool withNpc = true, bool consumables = false)
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        if (consumables)
        {
            ItemData.FlaskOfHealing.Instantiate(playerEntity);
            ItemData.PotionOfExperience.Instantiate(playerEntity);
        }
        else
        {
            ItemData.Shortbow.Instantiate(playerEntity);
        }

        manager.Queue.ProcessQueue(manager);

        if (withNpc)
        {
            CreatureData.HumanWarrior.Instantiate(level, new Point(3, 2));
            manager.Queue.ProcessQueue(manager);
        }

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
    }

    [Fact]
    public void Ability_noise_is_serialized_in_state_and_details()
    {
        var (player, _, services, _) = SetupGame(withNpc: false);
        var attack = player.Entity.Being!.SlottedAbilities[
            AbilitySlottingSystem.DefaultMeleeAttackSlot];
        attack.Ability!.Noise = 17;

        var state = AbilityChangeBuilder.SerializeAbility(attack, services);
        var context = new SerializationContext(null!, player.Entity, services);
        var slotted = GameQueryProcessor.SerializeSlottedAbility(attack, context);
        var attributes = GameQueryProcessor.SerializeAbilityAttributes(attack, player.Entity, context);

        Assert.Equal(17, state.Noise);
        Assert.Equal(17, slotted.Noise);
        Assert.Equal(17, attributes.Noise);
    }

    [Fact]
    public void Ability_noise_change_is_emitted_as_a_delta()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false);
        var attack = player.Entity.Being!.SlottedAbilities[
            AbilitySlottingSystem.DefaultMeleeAttackSlot];
        var baseline = ClientStateHelpers.Deserialize(GameStateManager.GetState(player.Entity, services));

        attack.Ability!.Noise = 23;
        var changeSets = WebTestHelper.PerformAction(
            player, listener, services, ActorActionType.Wait)[player.Entity.Id];

        var delta = changeSets
            .SelectMany(changeSet => changeSet.PlayerState.Abilities ?? [])
            .Single(pair => pair.Key == attack.Id).Value;
        Assert.True(delta.ChangedProperties.IsPropertyChanged(14));
        Assert.Equal(23, delta.Noise);

        ClientStateHelpers.ApplyChangeSets(baseline, changeSets);
        Assert.Equal(23, baseline.Abilities![attack.Id].Noise);

        manager.Queue.ProcessQueue(manager);
    }

    [Fact]
    public void Ability_preview_includes_activate_effects()
    {
        var (player, _, services, _) = SetupGame(withNpc: false);
        var attack = player.Entity.Being!.SlottedAbilities[
            AbilitySlottingSystem.DefaultMeleeAttackSlot];
        var context = new SerializationContext(null!, player.Entity, services);

        var attributes = GameQueryProcessor.SerializeAbilityAttributes(attack, player.Entity, context);
        var subAttacks = attributes.SubAbilities;

        Assert.NotEmpty(subAttacks);
        Assert.All(subAttacks, subAttack =>
        {
            Assert.Contains(subAttack.Effects.Values, effect => effect.Type == EffectType.Activate);
        });
    }

    [Fact]
    public void Ability_details_include_possible_expression_grants()
    {
        var (player, _, services, manager) = SetupGame(withNpc: false);
        var source = new LeveledAbility
        {
            Name = "detail expression source",
            Type = AbilityType.Skill,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [], [2] = [], [3] = []
            }
        };
        var dependent = new Ability
        {
            Name = "detail expression dependent",
            Type = AbilityType.RangedSpell,
            LevelExpression =
                "AbilityLevel('detail expression source') >= 2"
                + " ? AbilityLevel('detail expression source') : 0",
            Effects = []
        };
        ((TestDataProvider)services.Data).AddAbilities(source, dependent);

        manager.EffectApplicationSystem.AddAbilityEffect(player.Entity, source);
        manager.Queue.ProcessQueue(manager);
        var sourceEntity = manager.AffectableAbilitiesIndex[(player.Entity.Id, source.Name)]!;
        var context = new SerializationContext(null!, player.Entity, services);

        var unavailable = Assert.Single(
            GameQueryProcessor.SerializeAbilityAttributes(
                sourceEntity, player.Entity, context).ConditionalGrants);
        Assert.Equal(dependent.Name, unavailable.Name);
        Assert.Equal(0, unavailable.Level);
        Assert.Equal(dependent.LevelExpression, unavailable.LevelExpression);

        manager.EffectApplicationSystem.AddAbilityEffect(player.Entity, source);
        manager.Queue.ProcessQueue(manager);

        var available = Assert.Single(
            GameQueryProcessor.SerializeAbilityAttributes(
                sourceEntity, player.Entity, context).ConditionalGrants);
        Assert.Equal(2, available.Level);
    }

    [Fact]
    public void MeleeAttack()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        // Equip sword
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 0, (int)Direction.North);
    }

    [Fact]
    public void RangedAttack()
    {
        var game = TestHelper.CreateGame();
        var level = TestHelper.BuildRoom(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 5));
        manager.Queue.ProcessQueue(manager);

        ItemData.Shortbow.Instantiate(playerEntity);

        // Place NPC further away
        CreatureData.HumanWarrior.Instantiate(level, new Point(3, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        listener.RegisterPlayerBuilder(playerEntity, manager);

        var bowEntity = playerEntity.Being!.Items.Single(i => i.Item!.TemplateName == "shortbow");
        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.EquipItem, bowEntity.Id, (int)EquipmentSlot.GraspBothRanged);

        WebTestHelper.PerformAndVerify(playerEntity.Player!, listener, game.Services,
            ActorActionType.UseAbilitySlot, 1, (int)Direction.North);
    }

    [Fact]
    public void KillNPC()
    {
        var (player, listener, services, manager) = SetupGame();
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        var npc = manager.RoomActors.Single(a => a.AI != null && a.Being!.IsAlive);
        var npcId = npc.Id;
        var npcStartHp = npc.Being!.HitPoints;
        var playerStartHp = player.Entity.Being!.HitPoints;

        // Attack repeatedly until NPC dies. Seed 0 → deterministic.
        // target2 is interpreted as a packed Point (positive) or -entityId (negative);
        // pass -npcId to target the NPC directly.
        var attacks = 0;
        while (manager.FindEntity(npcId)?.Being?.IsAlive == true
               && player.Entity.Being!.IsAlive)
        {
            WebTestHelper.PerformAction(player, listener, services,
                ActorActionType.UseAbilitySlot, 0, -npcId);
            attacks++;
            Assert.True(attacks < 100,
                $"Combat should resolve within 100 attacks under seed 0 "
                + $"(attacks={attacks}, npcAlive={manager.FindEntity(npcId)?.Being?.IsAlive}, "
                + $"npcHp={manager.FindEntity(npcId)?.Being?.HitPoints}/{npcStartHp}, "
                + $"playerAlive={player.Entity.Being!.IsAlive}, playerHp={player.Entity.Being!.HitPoints}/{playerStartHp})");
        }

        // Player should win the fight given a sword vs an unarmed human.
        Assert.True(player.Entity.Being!.IsAlive,
            $"Player died first (attacks={attacks}, playerHp={player.Entity.Being.HitPoints})");
        Assert.False(manager.FindEntity(npcId)?.Being?.IsAlive == true,
            "NPC should be dead by end of combat");

        // Pin the attack count so RNG-affecting changes surface as a deliberate failure
        // rather than silent drift. Seed 0 currently kills the human in this many attacks.
        Assert.InRange(attacks, 1, 30);

        // Final change-set verification: the dead NPC must be gone from the client view.
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services, ActorActionType.Wait);
        Assert.DoesNotContain(fresh.Room!.Actors!.Values,
            a => a.BaseName != "player" && a.IsCurrentlyPerceived);
    }

    [Fact]
    public void NPCAttacksPlayer()
    {
        var (player, listener, services, manager) = SetupGame();

        var baselineHp = player.Entity.Being!.HitPoints;

        // Wait several turns; the adjacent hostile NPC attacks each turn and lands hits over time.
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services, ActorActionType.Wait);
        for (var i = 0; i < 4; i++)
        {
            fresh = WebTestHelper.PerformAndVerify(player, listener, services, ActorActionType.Wait);
        }

        // The NPC's attacks reduced the player's health, and the client HP mirrors the server.
        Assert.True(fresh.Hp < baselineHp, "NPC should have damaged the player over several turns");
        Assert.Equal(player.Entity.Being!.HitPoints, fresh.Hp);
    }

    [Fact]
    public void SetAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false, consumables: true);

        // Find a slottable ability (item drink ability)
        var slottableAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")];
        Assert.NotNull(slottableAbility);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, slottableAbility.Id, 3);

        Assert.Contains(fresh.Abilities!.Values, a => a.Slot == 3);
    }

    [Fact]
    public void Continuous_energy_reservation_is_sent_in_player_changes()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false);
        manager.EffectApplicationSystem.AddAbilityEffect(
            player.Entity, services.Data.PlayerRaces.Get("human").GetRaceAbilityTemplate(), level: 2);
        manager.Queue.ProcessQueue(manager);

        var toggledAbility = player.Entity.ActiveEffects!.Abilities
            .Single(entity => entity.Ability!.Name == "Surge of Luck");
        listener.RegisterPlayerBuilder(player.Entity, manager);

        WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.SetAbilitySlot, toggledAbility.Id, 2);
        var fresh = WebTestHelper.PerformAndVerify(
            player, listener, services, ActorActionType.UseAbilitySlot, 2);

        Assert.Equal(toggledAbility.Ability!.EnergyCost, fresh.ReservedEp);
        Assert.Equal(player.Entity.Being!.ReservedEnergyPoints, fresh.ReservedEp);
    }

    [Fact]
    public void ClearAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false, consumables: true);

        var slottableAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")];
        Assert.NotNull(slottableAbility);

        // Assign to slot 3
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, slottableAbility.Id, 3);

        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, 0, 3);

        Assert.DoesNotContain(fresh.Abilities!.Values, a => a.Slot == 3);
    }

    [Fact]
    public void SwapAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false, consumables: true);

        var flaskAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "flask of healing: Drink")];
        var potionAbility = manager.AffectableAbilitiesIndex[
            (player.Entity.Id, "potion of experience: Drink")];
        Assert.NotNull(flaskAbility);
        Assert.NotNull(potionAbility);

        // Both consumables auto-slot when picked up; find the slot the potion occupies.
        var potionSlot = potionAbility.Ability!.Slot;
        Assert.NotNull(potionSlot);

        // Slotting the flask into the potion's slot displaces the potion, whose item is dropped.
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.SetAbilitySlot, flaskAbility.Id, potionSlot!.Value);

        // The displaced potion's item is dropped on the level and removed from the player's inventory.
        Assert.Contains(fresh.Room!.Items!.Values, i => i.BaseName == "potion of experience");
        Assert.DoesNotContain(player.Entity.Being!.Items, i => i.Item!.TemplateName == "potion of experience");
    }

    [Fact]
    public void UseAbilitySlot()
    {
        var (player, listener, services, manager) = SetupGame(withNpc: false, consumables: true);

        // Place NPC south
        CreatureData.HumanWarrior.Instantiate(
            manager.FindEntity(player.Entity.Position!.RoomId)!.Room!, new Point(3, 4));
        manager.Queue.ProcessQueue(manager);

        // Equip sword to get melee attack in slot 0
        var swordEntity = player.Entity.Being!.Items.Single(i => i.Item!.TemplateName == "long sword");
        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.EquipItem, swordEntity.Id, (int)EquipmentSlot.GraspPrimaryMelee);

        WebTestHelper.PerformAndVerify(player, listener, services,
            ActorActionType.UseAbilitySlot, 0, (int)Direction.South);
    }
}

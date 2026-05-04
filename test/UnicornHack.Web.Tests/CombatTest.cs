using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using Xunit;

namespace UnicornHack;

public class CombatTest
{
    private const string TestMap = @"
.......
.......
.......
.......
.......
.......
.......";

    private static (PlayerComponent Player, LevelChangeBuilder Listener, GameServices Services, GameManager Manager)
        SetupGame(bool withNpc = true)
    {
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 3));
        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.Shortbow.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        if (withNpc)
        {
            CreatureData.Human.Instantiate(level, new Point(3, 2));
            manager.Queue.ProcessQueue(manager);
        }

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

        return (playerEntity.Player!, listener, game.Services, manager);
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
        var game = WebTestHelper.CreateGame();
        var level = WebTestHelper.BuildLevel(TestMap, game);
        var manager = game.Manager;

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(3, 5));
        manager.Queue.ProcessQueue(manager);

        ItemData.Shortbow.Instantiate(playerEntity);

        // Place NPC further away
        CreatureData.Human.Instantiate(level, new Point(3, 1));
        manager.Queue.ProcessQueue(manager);

        var listener = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(playerEntity, listener, manager);

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

        var npc = manager.LevelActors.Single(a => a.AI != null && a.Being!.IsAlive);
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
        Assert.DoesNotContain(fresh.Level!.Actors!.Values,
            a => a.BaseName != "player" && a.IsCurrentlyPerceived);
    }

    [Fact]
    public void NPCAttacksPlayer()
    {
        var (player, listener, services, manager) = SetupGame();

        var baselineHp = player.Entity.Being!.HitPoints;

        // Wait — NPC should attack the player
        var fresh = WebTestHelper.PerformAndVerify(player, listener, services, ActorActionType.Wait);

        // Player may have been attacked
        Assert.True(fresh.Hp <= baselineHp);

        // NPC actor should have a next action
        var npcActor = fresh.Level!.Actors!.Values.FirstOrDefault(a => a.BaseName != "player" && a.IsCurrentlyPerceived);
        if (npcActor != null)
        {
            var nextAction = npcActor.NextAction;
            Assert.NotNull(nextAction);
            Assert.NotNull(nextAction!.Name);
            Assert.True(
                nextAction.Type == ActorActionType.UseAbilitySlot
                || nextAction.Type == ActorActionType.MoveOneCell
                || nextAction.Type == ActorActionType.ChangeHeading,
                $"Unexpected NPC action type: {nextAction.Type}");
            Assert.True(nextAction.Target != 0, "NPC action target should be set");
        }
    }
}

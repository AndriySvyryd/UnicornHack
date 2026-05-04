using BenchmarkDotNet.Attributes;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;

namespace UnicornHack.Web.PerformanceTests;

/// <summary>
///     Benchmarks the change-tracking pipeline that produces what the SignalR hub sends
///     to the client. For each scenario two paths run the same simulation and produce
///     equivalent client-side state, but differ in how that state is serialized:
///     <list type="bullet">
///         <item><description><c>Delta</c> — <see cref="GameStateManager.Turn" /> with
///             change tracking enabled. The registered builders accumulate per-property
///             changes during simulation and emit them as one or more
///             <see cref="TurnChangeSet" /> at the end of the turn.</description></item>
///         <item><description><c>FullSnapshot</c> — <see cref="GameStateManager.Turn" />
///             with <c>collectChanges: false</c> followed by
///             <see cref="GameStateManager.GetState" />, which re-serializes every
///             visible entity from scratch (the same path the hub uses on initial load).
///             This is the equivalent cost if the hub didn't maintain incremental
///             tracking.</description></item>
///     </list>
///     Each scenario is designed to produce many changes per single player action and,
///     where possible, multiple changed properties per entity (NPC moves → both
///     coordinates change; AI takes a turn → <c>NextActionTick</c>/<c>NextAction</c>
///     change; HP regenerates → <c>Hp</c> changes; etc.).
/// </summary>
[MemoryDiagnoser]
public class ChangeDetectionPerfTest
{
    /// <summary>Number of NPCs placed around the player (one per available cell).</summary>
    [Params(8, 24)]
    public int NpcCount;

    private const string Map = @"
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................
....................";

    private Game _game = null!;
    private LevelComponent _level = null!;
    private GameEntity _playerEntity = null!;
    private PlayerComponent _player = null!;
    private LevelChangeBuilder _builder = null!;
    private GameServices _services = null!;

    /// <summary>
    ///     Rebuilds the world before every benchmark invocation. Keeping the cost in
    ///     <see cref="IterationSetup" /> so it does not pollute timing, while ensuring
    ///     <c>Delta</c> benchmarks see a freshly seeded change-tracker (the <c>Delta</c>
    ///     path mutates state, so a single instance can't be reused across iterations).
    /// </summary>
    [IterationSetup]
    public void Setup()
    {
        _game = WebTestHelper.CreateGame();
        _level = WebTestHelper.BuildLevel(Map, _game);
        var manager = _game.Manager;

        var center = new Point(10, 7);
        _playerEntity = PlayerRace.InstantiatePlayer("Bench", Sex.Male, _level, center);
        manager.Queue.ProcessQueue(manager);

        // Equip the player so attack-summary recomputation is exercised every turn.
        ItemData.LongSword.Instantiate(_playerEntity);
        ItemData.Shortbow.Instantiate(_playerEntity);
        manager.Queue.ProcessQueue(manager);

        // Surround the player with NPCs in concentric rings (skipping the center cell).
        // Rings cannot overlap so no per-cell collision check is needed.
        var placed = 0;
        for (var radius = 1; radius < 7 && placed < NpcCount; radius++)
        {
            for (var dx = -radius; dx <= radius && placed < NpcCount; dx++)
            {
                for (var dy = -radius; dy <= radius && placed < NpcCount; dy++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                    {
                        continue; // ring boundary only
                    }

                    var x = center.X + dx;
                    var y = center.Y + dy;
                    if (x <= 0 || x >= _level.Width - 1 || y <= 0 || y >= _level.Height - 1)
                    {
                        continue;
                    }

                    CreatureData.Human.Instantiate(_level, new Point((byte)x, (byte)y));
                    placed++;
                }
            }
        }

        manager.Queue.ProcessQueue(manager);

        _builder = WebTestHelper.SetupListeners(manager);
        WebTestHelper.RegisterPlayer(_playerEntity, _builder, manager);
        _player = _playerEntity.Player!;
        _services = _game.Services;
    }

    /// <summary>
    ///     Number of player actions per benchmark invocation. Each action triggers a
    ///     full simulation pass; running multiple per measurement amortizes per-action
    ///     variance (RNG-driven AI choices, GC, JIT tier-up) so the timing is more
    ///     stable.
    /// </summary>
    private const int ActionsPerInvocation = 10;

    /// <summary>
    ///     Delta path for a wait turn: <see cref="GameStateManager.Turn" /> runs the
    ///     simulation with change tracking enabled and produces one or more
    ///     <see cref="TurnChangeSet" /> entries containing only the modified properties.
    /// </summary>
    [Benchmark]
    public void WaitTurn_Delta()
    {
        for (var i = 0; i < ActionsPerInvocation; i++)
        {
            WebTestHelper.PerformAction(_player, _builder, _services, ActorActionType.Wait);
        }
    }

    /// <summary>
    ///     Full-snapshot equivalent of <see cref="WaitTurn_Delta" />: runs the same
    ///     simulation with change tracking <i>disabled</i>, then serializes the entire
    ///     player/level state from scratch. This is what the hub would have to send if it
    ///     did not maintain incremental change tracking.
    /// </summary>
    [Benchmark]
    public void WaitTurn_FullSnapshot()
    {
        for (var i = 0; i < ActionsPerInvocation; i++)
        {
            _player.NextAction = ActorActionType.Wait;
            _player.NextActionTarget = null;
            _player.NextActionTarget2 = null;
            GameStateManager.Turn(_player, _builder, _services, collectChanges: false);
            GameStateManager.GetState(_playerEntity, _services);
        }
    }

    /// <summary>
    ///     Movement delta: alternates East/West so 10 actions stay in bounds. Each move
    ///     shifts the player's visible area by one cell, which forces terrain visibility
    ///     recomputation in addition to the per-NPC updates.
    /// </summary>
    [Benchmark]
    public void Move_Delta()
    {
        for (var i = 0; i < ActionsPerInvocation; i++)
        {
            var direction = (i & 1) == 0 ? Direction.East : Direction.West;
            WebTestHelper.PerformAction(
                _player, _builder, _services, ActorActionType.MoveOneCell, (int)direction);
        }
    }

    /// <summary>
    ///     Full-snapshot equivalent of <see cref="Move_Delta" />.
    /// </summary>
    [Benchmark]
    public void Move_FullSnapshot()
    {
        for (var i = 0; i < ActionsPerInvocation; i++)
        {
            _player.NextAction = ActorActionType.MoveOneCell;
            _player.NextActionTarget = (int)((i & 1) == 0 ? Direction.East : Direction.West);
            _player.NextActionTarget2 = null;
            GameStateManager.Turn(_player, _builder, _services, collectChanges: false);
            GameStateManager.GetState(_playerEntity, _services);
        }
    }
}

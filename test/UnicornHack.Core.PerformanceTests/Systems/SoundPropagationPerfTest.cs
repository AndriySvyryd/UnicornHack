using BenchmarkDotNet.Attributes;
using UnicornHack.Data.Creatures;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;

namespace UnicornHack.PerformanceTests.Systems;

[MemoryDiagnoser]
public class SoundPropagationPerfTest
{
    private const int EightListenerOperations = 128;
    private const int OneListenerOperations = 1024;

    private readonly GameEntity[] _listeners = new GameEntity[8];
    private readonly SenseType[] _soundSenses = new SenseType[8];
    private GameEntity _source = null!;
    private GameManager _manager = null!;

    [Params(15)]
    public int RoomSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var row = new string('.', RoomSize);
        var map = string.Join(Environment.NewLine, Enumerable.Repeat(row, RoomSize));
        var room = TestHelper.BuildRoom(map);
        for (byte i = 0; i < _listeners.Length; i++)
        {
            _listeners[i] = CreatureData.HumanWarrior.Instantiate(room, new Point(i, 0));
        }

        _source = CreatureData.HumanWarrior.Instantiate(room, new Point((byte)(RoomSize - 1), (byte)(RoomSize - 1)));
        _manager = _source.Manager;
        _manager.Queue.ProcessQueue(_manager);
        foreach (var listener in _listeners)
        {
            _manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
                20, nameof(BeingComponent.HearingLevel), "benchmark hearing", listener);
        }

        _manager.Queue.ProcessQueue(_manager);
    }

    [Benchmark]
    public bool AStar_one_listener()
    {
        var heard = false;
        for (var i = 0; i < OneListenerOperations; i++)
        {
            heard ^= _manager.SensorySystem.CanHear(
                _listeners[0], _source, actionNoise: 200);
        }

        return heard;
    }

    [Benchmark]
    public int AStar_eight_listeners()
    {
        var heard = 0;
        for (var operation = 0; operation < EightListenerOperations; operation++)
        {
            _manager.SensorySystem.CalculateSensedSound(
                _source, actionNoise: 200, _listeners, _soundSenses);
            for (var i = 0; i < _soundSenses.Length; i++)
            {
                if (_soundSenses[i] != SenseType.None)
                {
                    heard++;
                }
            }
        }

        return heard;
    }
}

using BenchmarkDotNet.Attributes;
using UnicornHack.Data.Creatures;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;

namespace UnicornHack.PerformanceTests.Systems;

[MemoryDiagnoser]
public class AbilitySystemsPerfTest
{
    private const int DeathOperations = 3;

    private GameEntity[] _actors = null!;
    private GameManager _manager = null!;
    private RoomComponent _room = null!;

    [Params(64)]
    public int Count { get; set; }

    [IterationSetup(Target = nameof(Creature_death))]
    public void SetupDeath()
    {
        _room = TestHelper.BuildRoom("...");
        _actors = new GameEntity[DeathOperations];
        for (var actorIndex = 0; actorIndex < _actors.Length; actorIndex++)
        {
            var actor = CreatureData.HumanWarrior.Instantiate(
                _room, new Point((byte)actorIndex, 0));
            _manager = actor.Manager;
            _manager.Queue.ProcessQueue(_manager);

            for (var i = 0; i < Count; i++)
            {
                var abilityEntity = _manager.CreateEntity().Referenced;
                var ability = _manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                ability.Name = $"benchmark ability {actorIndex}-{i}";
                ability.Activation = ActivationType.WhileToggled;
                ability.IsActive = true;
                ability.OwnerId = actor.Id;
                abilityEntity.Ability = ability;
            }

            _actors[actorIndex] = actor;
        }

        _manager.Queue.ProcessQueue(_manager);
    }

    [IterationSetup(Target = nameof(Instantiate_creatures))]
    public void SetupInstantiation()
    {
        var row = new string('.', 16);
        _room = TestHelper.BuildRoom(string.Join(Environment.NewLine, Enumerable.Repeat(row, 16)));
        _manager = _room.Entity.Manager;
        _manager.Queue.ProcessQueue(_manager);
    }

    [Benchmark]
    public void Creature_death()
    {
        foreach (var actor in _actors)
        {
            DiedMessage.Enqueue(actor, _manager);
        }

        _manager.Queue.ProcessQueue(_manager);
    }

    [Benchmark]
    public void Instantiate_creatures()
    {
        for (var i = 0; i < Count; i++)
        {
            CreatureData.HumanWarrior.Instantiate(
                _room, new Point((byte)(i % 16), (byte)(i / 16)));
        }

        _manager.Queue.ProcessQueue(_manager);
    }
}
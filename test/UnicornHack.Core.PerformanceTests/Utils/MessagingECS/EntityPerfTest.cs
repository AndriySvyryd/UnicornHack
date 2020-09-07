using BenchmarkDotNet.Attributes;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests.Utils.MessagingECS
{
    public class EntityPerfTest
    {
        public int EntityCount = 1000000;

        private Entity[] _entities;

        [GlobalSetup(Target = nameof(AddComponent) + "," + nameof(FindComponent) + "," + nameof(HasComponent))]
        public void ComponentSetup()
        {
            var manager = TestHelper.CreateGameManager();
            if (_entities == null
                || _entities.Length != EntityCount)
            {
                _entities = new Entity[EntityCount];
            }

            for (var i = 0; i < EntityCount; i++)
            {
                var entity = manager.CreateEntity().Referenced;
                entity.AddComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
                entity.AddComponent<LevelComponent>((int)EntityComponent.Level);
                _entities[i] = entity;
            }
        }

        [GlobalCleanup(Target = nameof(AddComponent) + "," + nameof(FindComponent) + "," + nameof(HasComponent))]
        public void ComponentCleanup()
        {
            var context = _entities[0].Manager;
            for (var i = 0; i < EntityCount; i++)
            {
                context.RemoveEntity(_entities[i]);
            }
        }

        //[Benchmark]
        public void AddComponent()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                var entity = _entities[i];
                entity.AddComponent<AIComponent>((int)EntityComponent.AI);
            }
        }

        //[Benchmark]
        public void FindComponent()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                var _ = _entities[i].FindComponent((int)EntityComponent.Position);
            }
        }

        //[Benchmark]
        public void HasComponent()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                var _ = _entities[i].HasComponent((int)EntityComponent.Position);
            }
        }
    }
}

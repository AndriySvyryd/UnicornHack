using BenchmarkDotNet.Attributes;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.PerformanceTests.Utils.MessagingECS
{
    public class EntityRelationshipPerfTest
    {
        public int EntityCount = 1000000;

        private Entity[] _abilityEntities;
        private EffectComponent[] _effects;

        [GlobalSetup(Target = nameof(UpdateRelationship))]
        public void RelationshipSetup()
        {
            var manager = TestHelper.CreateGameManager();

            if (_abilityEntities == null
                || _abilityEntities.Length != EntityCount)
            {
                _abilityEntities = new Entity[EntityCount];
            }

            if (_effects == null
                || _effects.Length != EntityCount)
            {
                _effects = new EffectComponent[EntityCount];
            }

            for (var i = 0; i < EntityCount; i++)
            {
                var abilityEntity = manager.CreateEntity().Referenced;
                abilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);
                _abilityEntities[i] = abilityEntity;

                var effectEntity = manager.CreateEntity().Referenced;
                _effects[i] = effectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);
            }
        }

        [GlobalCleanup(Target = nameof(UpdateRelationship))]
        public void RelationshipCleanup()
        {
            var context = _abilityEntities[0].Manager;
            for (var i = 0; i < EntityCount; i++)
            {
                _effects[i].Entity.RemoveReference(context);
                _abilityEntities[i].RemoveReference(context);
            }
        }

        [Benchmark]
        public void UpdateRelationship()
        {
            for (var i = 0; i < EntityCount; i++)
            {
                _effects[i].ContainingAbilityId = _abilityEntities[i].Id;
            }
        }
    }
}

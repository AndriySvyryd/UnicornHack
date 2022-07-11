using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class UniqueEntityIndexTest
{
    [Fact]
    public void Index_is_updated()
    {
        var level = TestHelper.BuildLevel(".");
        var manager = level.Entity.Manager;

        manager.Queue.ProcessQueue(manager);

        using var beingEntityReference = manager.CreateEntity();
        var beingEntity = beingEntityReference.Referenced;
        beingEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
        beingEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
        var position = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
        position.LevelId = level.EntityId;
        position.LevelCell = new Point(2, 3);
        beingEntity.Position = position;

        using var innateAbilityReference = manager.CreateEntity();
        var innateAbilityEntity = innateAbilityReference.Referenced;

        var innateEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        innateEffect.EffectType = EffectType.AddAbility;
        innateEffect.Duration = EffectDuration.Infinite;

        innateAbilityEntity.Effect = innateEffect;

        var innateAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        innateAbility.Name = EffectApplicationSystem.InnateAbilityName;
        innateAbility.OwnerId = beingEntity.Id;
        innateAbility.Activation = ActivationType.Always;
        innateAbility.SuccessCondition = AbilitySuccessCondition.Always;

        innateAbilityEntity.Ability = innateAbility;

        manager.EffectApplicationSystem.AddPropertyEffect(nameof(PhysicalComponent.Capacity),
            (AbilitySlottingSystem.DefaultSlotCapacity) + 2, innateAbilityEntity.Id, manager);

        manager.Queue.ProcessQueue(manager);

        Assert.Single(manager.AffectableAbilitiesIndex);
        Assert.Same(innateAbilityEntity,
            manager.AffectableAbilitiesIndex[(beingEntity.Id, innateAbility.Name)]);

        using var manualAbilityEntityReference = manager.CreateEntity();
        var manualAbilityEntity = manualAbilityEntityReference.Referenced;

        var manualAbility = manager.CreateComponent<AbilityComponent>((int)EntityComponent.Ability);
        manualAbility.Name = "Test Ability";
        manualAbility.OwnerId = beingEntity.Id;
        manualAbility.Activation = ActivationType.Manual;
        manualAbility.Slot = 2;
        manualAbilityEntity.Ability = manualAbility;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, manager.AffectableAbilitiesIndex.Count());
        Assert.Same(manualAbilityEntity,
            manager.AffectableAbilitiesIndex[(beingEntity.Id, manualAbility.Name)]);

        manualAbility.Name = "Test Ability 2";

        Assert.Equal(2, manager.AffectableAbilitiesIndex.Count());
        Assert.Same(manualAbilityEntity,
            manager.AffectableAbilitiesIndex[(beingEntity.Id, manualAbility.Name)]);

        manualAbility.OwnerId = null;

        manager.Queue.ProcessQueue(manager);
        Assert.Same(innateAbilityEntity, manager.AffectableAbilitiesIndex.Single());
        Assert.Null(manager.AffectableAbilitiesIndex[(beingEntity.Id, manualAbility.Name)]);

        manualAbility.OwnerId = beingEntity.Id;

        Assert.Throws<InvalidOperationException>(() => manualAbility.Name = EffectApplicationSystem.InnateAbilityName);
    }
}

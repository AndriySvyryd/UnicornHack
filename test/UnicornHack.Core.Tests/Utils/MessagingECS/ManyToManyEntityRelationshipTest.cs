using UnicornHack.Data.Creatures;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Utils.MessagingECS;

public class ManyToManyEntityRelationshipTest
{
    [Fact]
    public void Foreign_key_zero_is_treated_as_no_reference()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        // An item whose foreign key is null/0 is not a reference, so it is neither indexed nor subject to the
        // referential-integrity check (which would otherwise throw for an unresolved key).
        AddItem(playerEntity.ActiveEffects!.PropertyEffects, sourceAbilityId: null);

        Assert.Empty(manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(0));
    }

    [Fact]
    public void An_entity_is_tracked_once_until_its_last_item_for_a_principal_is_removed()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var relationship = manager.AppliedEffectsToSourceAbilityRelationship;
        var ability = CreateAbility(manager, playerEntity, "buff");
        var effects = playerEntity.ActiveEffects!.PropertyEffects;

        // Two items from the same dependent reference the same principal: the dependent is tracked once, with
        // a reference count of two.
        var first = AddItem(effects, ability.Id);
        var second = AddItem(effects, ability.Id);
        Assert.Single(relationship.GetDependents(ability.Id));
        Assert.Contains(playerEntity, relationship.GetDependents(ability.Id));

        // Removing one of the two items keeps the dependent (its reference count is still positive).
        effects.Remove(first);
        Assert.Single(relationship.GetDependents(ability.Id));

        // Removing the last item referencing the principal drops the dependent from the reverse index.
        effects.Remove(second);
        Assert.Empty(relationship.GetDependents(ability.Id));
    }

    [Fact]
    public void Property_effect_foreign_key_changes_reindex_the_relationship()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var firstAbility = CreateAbility(manager, playerEntity, "first");
        var secondAbility = CreateAbility(manager, playerEntity, "second");
        var effect = AddItem(playerEntity.ActiveEffects!.PropertyEffects, firstAbility.Id);
        var relationship = manager.AppliedEffectsToSourceAbilityRelationship;

        effect.SourceAbilityEntityId = secondAbility.Id;

        Assert.Empty(relationship.GetDependents(firstAbility.Id));
        Assert.Contains(playerEntity, relationship.GetDependents(secondAbility.Id));
    }

    [Fact]
    public void Component_effect_foreign_key_changes_reindex_the_relationship()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var carrier = CreateAbility(manager, playerEntity, "race carrier");

        var effect = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, "test");

        effect.CarrierId = carrier.Id;

        Assert.Contains(playerEntity, manager.ComponentEffectsToCarrierRelationship.GetDependents(carrier.Id));
    }

    [Fact]
    public void All_component_effect_types_reference_their_carrier()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var carrier = CreateAbility(manager, playerEntity, "race carrier");

        var effect = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, "test", carrier.Id);

        Assert.Contains(playerEntity, manager.ComponentEffectsToCarrierRelationship.GetDependents(carrier.Id));
    }

    [Fact]
    public void Clearing_the_last_carrier_id_removes_the_carrier_component()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var carrier = CreateAbility(manager, playerEntity, "test carrier");
        var effect = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, "test", carrier.Id);

        effect.CarrierId = 0;

        Assert.Null(carrier.Ability);
        Assert.Contains(effect, playerEntity.ActiveEffects.ComponentEffects);
        Assert.Empty(manager.ComponentEffectsToCarrierRelationship.GetDependents(carrier.Id));
    }

    [Fact]
    public void Removing_a_carrier_component_removes_all_referencing_component_effects()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var carrier = CreateAbility(manager, playerEntity, "test carrier");
        var abilityEffect = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, "test ability", carrier.Id);
        var raceEffect = playerEntity.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, "test race", carrier.Id);

        carrier.Ability = null;

        Assert.DoesNotContain(abilityEffect, playerEntity.ActiveEffects.ComponentEffects);
        Assert.DoesNotContain(raceEffect, playerEntity.ActiveEffects.ComponentEffects);
        Assert.Empty(manager.ComponentEffectsToCarrierRelationship.GetDependents(carrier.Id));
    }

    [Fact]
    public void Removing_a_dependent_from_the_group_clears_its_reverse_index_entries()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var creature = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var relationship = manager.AppliedEffectsToSourceAbilityRelationship;
        var ability = CreateAbility(manager, playerEntity, "aura");
        AddItem(creature.ActiveEffects!.PropertyEffects, ability.Id);
        Assert.Contains(creature, relationship.GetDependents(ability.Id));

        // Deleting the dependent takes it out of the dependent group, so the relationship stops tracking its
        // collection and it leaves the reverse index (the principal ability is untouched).
        DeleteEntityMessage.Enqueue(creature, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.FindEntity(creature.Id));
        Assert.Empty(relationship.GetDependents(ability.Id));
    }

    [Fact]
    public void An_unresolved_foreign_key_throws_unless_the_manager_is_loading()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var creature = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        const int missingPrincipal = 987654321;

        // Adding an item whose foreign key does not resolve to a live principal throws.
        Assert.Throws<InvalidOperationException>(() =>
            AddItem(playerEntity.ActiveEffects!.PropertyEffects, missingPrincipal));

        // ...but the check is suspended while the manager is loading, since the principal may be materialized
        // later in the load; the item is still indexed.
        manager.IsLoading = true;
        try
        {
            AddItem(creature.ActiveEffects!.PropertyEffects, missingPrincipal);
        }
        finally
        {
            manager.IsLoading = false;
        }

        Assert.Contains(creature,
            manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(missingPrincipal));
    }

    private static GameEntity CreateAbility(GameManager manager, GameEntity owner, string name)
    {
        GameEntity abilityEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = name;
            ability.OwnerEntity = owner;
            abilityEntity.Ability = ability;
        }

        manager.Queue.ProcessQueue(manager);
        return abilityEntity;
    }

    private static ActivePropertyEffect AddItem(
        ICollection<ActivePropertyEffect> effects, int? sourceAbilityId)
    {
        var instance = new ActivePropertyEffect(0, 0, effects.Count, EffectType.Confuse, null)
        {
            SourceAbilityEntityId = sourceAbilityId
        };
        effects.Add(instance);
        return instance;
    }
}

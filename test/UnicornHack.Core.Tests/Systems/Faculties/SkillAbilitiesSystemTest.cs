using UnicornHack.Data.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Systems.Faculties;

public class SkillAbilitiesSystemTest
{
    [Fact]
    public void AddToEffect_defaults_to_a_single_first_level()
    {
        var manager = TestHelper.BuildRoom(".").Entity.Manager;
        var nonLeveled = new Ability { Name = "non-leveled max test" };
        var leveled = new LeveledAbility
        {
            Name = "leveled max test",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [new ChangeProperty<int> { PropertyName = "Armor", Amount = "1" }],
                [3] = [new ChangeProperty<int> { PropertyName = "Armor", Amount = "3" }]
            }
        };

        Assert.Equal(1, nonLeveled.MaxLevel);
        Assert.Equal(3, leveled.MaxLevel);
        Assert.Equal(["1", "3"], leveled.Effects.Select(effect => effect.Amount));
        Assert.All(leveled.Effects, effect => Assert.Same(leveled, effect.ContainingAbility));

        using var nonLeveledReference = manager.CreateEntity();
        var nonLeveledAbility = nonLeveled.AddToEffect(nonLeveledReference.Referenced)!;
        Assert.Equal(1, nonLeveledAbility.Level);

        using var leveledReference = manager.CreateEntity();
        var leveledAbility = leveled.AddToEffect(leveledReference.Referenced)!;
        Assert.Equal(1, leveledAbility.Level);
        Assert.Single(leveledAbility.Effects);

        using var explicitLastReference = manager.CreateEntity();
        Assert.Equal(2, leveled.AddToEffect(explicitLastReference.Referenced, lastLevel: 2)!.Level);

        using var explicitFirstReference = manager.CreateEntity();
        var upperRange = leveled.AddToEffect(explicitFirstReference.Referenced, firstLevel: 2)!;
        Assert.Equal(2, upperRange.Level);
        Assert.Empty(upperRange.Effects);
    }

    [Fact]
    public void Plain_trait_levels_can_increase_without_a_cap()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var trait = new Ability
        {
            Name = "test plain trait",
            Type = AbilityType.Trait,
            Effects = []
        };

        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, trait);
        manager.Queue.ProcessQueue(manager);

        var granted = manager.AffectableAbilitiesIndex[(playerEntity.Id, trait.Name)]!.Ability!;
        Assert.Equal(1, granted.Level);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, trait.Name));
        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, trait);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, granted.Level);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, trait.Name));
    }

    [Fact]
    public void Leveled_abilities_are_cumulative_by_default()
    {
        var ability = new LeveledAbility();

        Assert.True(ability.Cumulative);

        ability.Cumulative = false;
        Assert.False(ability.Cumulative);
    }

    [Fact]
    public void Repeated_grants_through_the_same_carrier_increase_the_existing_record()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var first = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        var second = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);

        Assert.Same(first, second);
        Assert.Equal(2, first.Level);
        Assert.NotEqual(0, first.CarrierId);
        var authoritative = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!;
        Assert.Equal(2, authoritative.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name + "#2")]);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
        Assert.Single(playerEntity.ActiveEffects!.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name));

        EffectApplicationSystem.DetachComponentCarrier(second, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Empty(playerEntity.ActiveEffects.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name));
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name + "#2")]);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
    }

    [Fact]
    public void Repeated_status_named_grants_overwrite_the_existing_carrier_record()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var first = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.Stunned);
        var second = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.Stunned);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(0, first.CarrierId);
        Assert.Same(first, second);
        Assert.Equal(2, first.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Stunned.Name + "#2")]);
    }

    [Fact]
    public void Expression_levels_combine_with_unconditional_grants_and_decreases()
    {
        var sourceA = new LeveledAbility
        {
            Name = "expression source a",
            Type = AbilityType.Skill,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [], [2] = [], [3] = []
            }
        };
        var sourceB = new LeveledAbility
        {
            Name = "expression source b",
            Type = AbilityType.Skill,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [], [2] = [], [3] = []
            }
        };
        var dependent = new Ability
        {
            Name = "expression dependent",
            Type = AbilityType.RangedSpell,
            LevelExpression =
                "AbilityLevel('expression source a') + AbilityLevel('expression source b') >= 2"
                + " ? AbilityLevel('expression source a') + AbilityLevel('expression source b') : 0",
            Effects = []
        };
        var game = TestHelper.CreateGame();
        var data = (TestDataProvider)game.Services.Data;
        data.AddAbilities(sourceA, sourceB, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, sourceA);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, dependent.Name)]);

        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, sourceB);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, GetAbilityLevel());
        Assert.Single(playerEntity.ActiveEffects!.ComponentEffects.GetValues(dependent.Name),
            TestHelper.IsLevelExpressionGrant);

        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, dependent, sourceAbilityEntityId: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, GetAbilityLevel());
        Assert.Equal(2, playerEntity.ActiveEffects.ComponentEffects.GetValues(dependent.Name).Count);

        var decrease = TestHelper.ApplyAbilityLevelDecrease(
            playerEntity, abilityName: dependent.Name,
            duration: EffectDuration.UntilTimeout);
        Assert.Equal(2, GetAbilityLevel());

        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, sourceA);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, GetAbilityLevel());

        manager.EffectApplicationSystem.RemoveAbility(
            playerEntity, sourceB.Name, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, GetAbilityLevel());

        manager.EffectApplicationSystem.RemoveAbility(
            playerEntity, dependent.Name, manager, sourceAbilityEntityId: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, GetAbilityLevel());

        manager.Game.CurrentTick = decrease.ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, GetAbilityLevel());

        var typeDecrease = TestHelper.ApplyAbilityLevelDecrease(
            playerEntity, abilityType: AbilityType.RangedSpell,
            duration: EffectDuration.UntilTimeout);
        Assert.Equal(1, GetAbilityLevel());

        manager.Game.CurrentTick = typeDecrease.ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, GetAbilityLevel());

        manager.EffectApplicationSystem.RemoveAbility(
            playerEntity, sourceA.Name, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, dependent.Name)]);

        int GetAbilityLevel()
            => manager.AffectableAbilitiesIndex[(playerEntity.Id, dependent.Name)]!.Ability!.Level;
    }

    [Fact]
    public void Non_positive_expression_result_removes_the_grant()
    {
        var source = new Ability
        {
            Name = "negative expression source",
            Effects = []
        };
        var dependent = new Ability
        {
            Name = "negative expression dependent",
            LevelExpression = "AbilityLevel('negative expression source') == 1 ? 1 : -1",
            Effects = []
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(source, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, dependent.Name)]);

        manager.EffectApplicationSystem.AddAbilityEffect(player, source);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(player, dependent.Name));

        manager.EffectApplicationSystem.AddAbilityEffect(player, source);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, dependent.Name)]);
        Assert.DoesNotContain(
            player.ActiveEffects!.ComponentEffects.GetValues(dependent.Name),
            TestHelper.IsLevelExpressionGrant);
    }

    [Fact]
    public void Source_level_change_synchronizes_all_expression_dependents()
    {
        var source = new Ability
        {
            Name = "fan-out expression source",
            Effects = []
        };
        var firstDependent = new Ability
        {
            Name = "first fan-out expression dependent",
            LevelExpression = "AbilityLevel('fan-out expression source')",
            Effects = []
        };
        var secondDependent = new Ability
        {
            Name = "second fan-out expression dependent",
            LevelExpression =
                "AbilityLevel('fan-out expression source') > 0"
                + " ? AbilityLevel('fan-out expression source') + 1 : 0",
            Effects = []
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(
            source, firstDependent, secondDependent);
        var level = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, firstDependent.Name)]);
        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, secondDependent.Name)]);

        manager.EffectApplicationSystem.AddAbilityEffect(player, source, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(player, firstDependent.Name));
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(player, secondDependent.Name));
        Assert.Equal([firstDependent, secondDependent], source.LevelExpressionDependents);
    }

    [Fact]
    public void Repeated_source_references_register_one_dependency_and_all_contributions()
    {
        var source = new Ability
        {
            Name = "repeated expression source",
            Effects = []
        };
        var dependent = new Ability
        {
            Name = "repeated expression dependent",
            LevelExpression =
                "AbilityLevel('repeated expression source') + AbilityLevel('repeated expression source')",
            Effects = []
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(source, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.AddAbilityEffect(player, source, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevel(player, dependent.Name));
        Assert.Equal([dependent], source.LevelExpressionDependents);
    }

    [Fact]
    public void Level_expression_drives_a_leveled_ability()
    {
        var source = new LeveledAbility
        {
            Name = "leveled expression source",
            Type = AbilityType.Skill,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [], [2] = [], [3] = []
            }
        };
        var dependent = new LeveledAbility
        {
            Name = "leveled expression dependent",
            Type = AbilityType.Feat,
            LevelExpression = "AbilityLevel('leveled expression source')",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [new Heal { Amount = "1" }],
                [2] = [new Heal { Amount = "2" }]
            }
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(source, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, source, level: 2);
        manager.Queue.ProcessQueue(manager);

        var ability = manager.AffectableAbilitiesIndex[(playerEntity.Id, dependent.Name)]!.Ability!;
        Assert.Equal(2, ability.Level);
        Assert.Equal(["1", "2"], ability.Effects.Select(effect => effect.Effect!.Amount));
    }

    [Fact]
    public void Ability_loader_rejects_cyclic_level_expressions()
    {
        var first = new Ability
        {
            Name = "expression cycle first",
            LevelExpression = "AbilityLevel('expression cycle second')"
        };
        var second = new Ability
        {
            Name = "expression cycle second",
            LevelExpression = "AbilityLevel('expression cycle first')"
        };
        var cycleException = Assert.Throws<InvalidOperationException>(
            () => AbilityLoader.Prepare(new Dictionary<string, Ability>(StringComparer.OrdinalIgnoreCase)
            {
                [first.Name] = first,
                [second.Name] = second
            }));
        Assert.Contains("cycle", cycleException.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Component_effects_are_ordered_by_instance_id()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var firstRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, AbilityData.Stunned.Name);
        var secondRecord = playerEntity.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, AbilityData.Stunned.Name);

        manager.EffectApplicationSystem.RelevelAbility(
            AbilityData.Stunned.Name, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(0, firstRecord.CarrierId);
        Assert.NotEqual(0, secondRecord.CarrierId);
        Assert.NotEqual(firstRecord.CarrierId, secondRecord.CarrierId);
        Assert.Equal(
            firstRecord.CarrierId,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Stunned.Name)]!.Id);
        Assert.Equal(
            secondRecord.CarrierId,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Stunned.Name + "#2")]!.Id);
    }

    [Fact]
    public void One_add_ability_effect_can_contribute_multiple_levels()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var effect = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, effect.Level);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
        var record = Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects.GetValues(
                AbilityData.WaterSourcery.Name));
        Assert.Equal(2, record.Level);

        EffectApplicationSystem.DetachComponentCarrier(effect, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public void Component_carriers_instantiate_their_contributed_level_range(
        bool cumulative, int expectedEffectCount)
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var template = new LeveledAbility
        {
            Name = "range test",
            Type = AbilityType.Trait,
            Cumulative = cumulative,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [new Heal { Amount = "1" }],
                [2] = [new Heal { Amount = "2" }],
                [3] = [new Heal { Amount = "3" }]
            }
        };
        var firstRecord = manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, template);
        var secondRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, template.Name, sourceAbilityEntityId: 1, level: 2);

        manager.EffectApplicationSystem.RelevelAbility(template.Name, playerEntity, manager);

        Assert.Equal(cumulative, firstRecord.CarrierId != 0);
        var carrier = manager.FindEntity(secondRecord.CarrierId)!.Ability!;
        Assert.Equal(3, carrier.Level);
        Assert.Equal(expectedEffectCount, carrier.Effects.Count);
        Assert.DoesNotContain(carrier.Effects, effect => effect.Effect!.Amount == "1");
        Assert.Contains(carrier.Effects, effect => effect.Effect!.Amount == "3");
        Assert.Equal(
            cumulative,
            carrier.Effects.Any(effect => effect.Effect!.Amount == "2"));
    }

    [Fact]
    public void Non_cumulative_ability_recreates_the_effective_carrier_when_its_level_decreases()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        var template = new LeveledAbility
        {
            Name = "non-cumulative decrease test",
            Type = AbilityType.Trait,
            Cumulative = false,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [new Heal { Amount = "1" }],
                [3] = [new Heal { Amount = "3" }]
            }
        };
        var firstRecord = manager.EffectApplicationSystem.AddAbilityEffect(playerEntity, template);
        var lastRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, template.Name, level: 2);
        manager.EffectApplicationSystem.RelevelAbility(template.Name, playerEntity, manager);

        Assert.Equal(0, firstRecord.CarrierId);
        Assert.NotEqual(0, lastRecord.CarrierId);
        Assert.Equal(
            "3",
            Assert.Single(manager.FindEntity(lastRecord.CarrierId)!.Ability!.Effects).Effect!.Amount);

        playerEntity.ActiveEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, template.Name, level: 2);
        manager.EffectApplicationSystem.RelevelAbility(template.Name, playerEntity, manager);

        Assert.NotEqual(0, firstRecord.CarrierId);
        Assert.Equal(0, lastRecord.CarrierId);
        var restored = manager.FindEntity(firstRecord.CarrierId)!.Ability!;
        Assert.Equal(template.Name, restored.Name);
        Assert.Equal(1, restored.Level);
        Assert.Equal("1", Assert.Single(restored.Effects).Effect!.Amount);
    }

    [Fact]
    public void Cumulative_level_range_instantiates_same_property_effects_independently()
    {
        var manager = TestHelper.BuildRoom(".").Entity.Manager;
        var template = new LeveledAbility
        {
            Name = "single level range test",
            Type = AbilityType.Trait,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [2] =
                [
                    new ChangeProperty<int> { PropertyName = "Armor", Amount = "1" },
                    new PhysicalDamage { ArmorPenetration = Effect.PropagatedAmountPlaceholder }
                ],
                [3] = [new ChangeProperty<int> { PropertyName = "Armor", Amount = "2" }]
            }
        };

        using var carrierReference = manager.CreateEntity();
        var ability = template.AddToEffect(
            carrierReference.Referenced, firstLevel: 2, lastLevel: 3,
            propagatedSecondaryAmount: "7")!;

        Assert.Equal(3, ability.Effects.Count);
        Assert.Contains(ability.Effects, effect => effect.Effect!.Amount == "1");
        Assert.Contains(ability.Effects, effect => effect.Effect!.Amount == "2");
        Assert.Equal(
            "7",
            ability.Effects.Single(effect => effect.Effect!.EffectType == EffectType.PhysicalDamage)
                .Effect!.SecondaryAmount);
    }

    [Fact]
    public void Removing_the_authoritative_level_promotes_the_next_carrier()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var authoritativeRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, AbilityData.WaterSourcery.Name, sourceAbilityEntityId: 1);
        var remainingRecord = playerEntity.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, AbilityData.WaterSourcery.Name, sourceAbilityEntityId: 2);
        manager.EffectApplicationSystem.RelevelAbility(
            AbilityData.WaterSourcery.Name, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        EffectApplicationSystem.DetachComponentCarrier(
            authoritativeRecord, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var authoritative = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!;
        Assert.Equal(remainingRecord.CarrierId, authoritative.EntityId);
        Assert.Equal(1, authoritative.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name + "#2")]);
    }

    [Fact]
    public void Skills_add_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]);

        var firstWaterBookAbility =
            manager.AffectableAbilitiesIndex[
                (playerEntity.Id, ItemData.SkillbookOfWaterSourcery.Name + ": Consult")]!;
        var firstWaterBook = playerEntity.Being!.Items.Single();
        ItemData.SkillbookOfWaterSourcery.Instantiate(playerEntity);
        ItemData.SkillbookOfConjuration.Instantiate(playerEntity);
        TestHelper.ActivateAbility(firstWaterBookAbility, playerEntity, manager, 2);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        var conjurationBookAbility = playerEntity.ActiveEffects!.Abilities
            .First(a => a.Ability!.Name == ItemData.SkillbookOfConjuration.Name + ": Consult");
        TestHelper.ActivateAbility(conjurationBookAbility, playerEntity, manager, 3);

        manager.Queue.ProcessQueue(manager);

        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);

        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(4,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, EffectApplicationSystem.InnateAbilityName)]);
        Assert.Equal(1, playerEntity.ActiveEffects!.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name).Count(
                effect => effect.SourceAbilityEntityId == 0));
        Assert.Equal(1,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]!.Ability!.Level);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        manager.Enqueue(DeactivateAbilityMessage.Create(
            conjurationBookAbility.Ability!, playerEntity, manager));

        var moveItemMessage = MoveItemMessage.Create(manager);
        moveItemMessage.ItemEntity = firstWaterBook;
        moveItemMessage.TargetCell = new Point(0, 0);
        moveItemMessage.TargetRoomEntity = level.Entity;

        manager.Enqueue(moveItemMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Same(
            firstWaterBookAbility,
            manager.AffectableAbilitiesIndex[
                (playerEntity.Id, ItemData.SkillbookOfWaterSourcery.Name + ": Consult")]);
        Assert.Equal(3,
            manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
        Assert.DoesNotContain(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == AbilityData.Conjuration.Name);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]);
        Assert.True(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]!.Ability!
            .IsUsable);

        manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability = null;

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.WaterSourcery.Name)]);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Conjuration.Name)]);
        Assert.False(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.IceShard.Name)]?.Ability
            ?.IsUsable == true);
    }

    [Fact]
    public void Default_attacks_are_slotted_when_weapons_are_equipped()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);
        ItemData.FireStaff.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Same(AbilityData.DoubleMeleeAttack.Name,
            playerEntity.Being!.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!.Name);
        Assert.False(
            playerEntity.Being.SlottedAbilities.ContainsKey(AbilitySlottingSystem.DefaultRangedAttackSlot));
        Assert.Equal(3, playerEntity.ActiveEffects!.Abilities.Select(a => a.Ability!)
            .Count(a => a.IsUsable && (a.Activation & ActivationType.Slottable) != 0));

        var swordEntity = playerEntity.Being.Items
            .Single(e => e.Item!.TemplateName == ItemData.LongSword.Name);
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);

        var staffEntity = playerEntity.Being.Items
            .Single(e => e.Item!.TemplateName == ItemData.FireStaff.Name);
        equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = staffEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothRanged;

        manager.Enqueue(equipMessage);

        manager.Queue.ProcessQueue(manager);

        Assert.Same(AbilityData.TwoHandedMeleeAttack.Name,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot]!.Ability!.Name);
        Assert.Same(AbilityData.TwoHandedRangedAttack.Name,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultRangedAttackSlot]!.Ability!.Name);
        Assert.Equal(2, playerEntity.ActiveEffects!.Abilities.Select(a => a.Ability!)
            .Count(a => a.IsUsable && (a.Activation & ActivationType.Slottable) != 0));
    }

    [Fact]
    public void Weapon_attack_targets_the_weapon_slot_and_resolves_natural_weapons()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        // A weaponless wielder fights with natural weapons; the dual default melee attack is slotted and its
        // two Activate effects target the primary- and secondary-hand SLOTS (negative ids) rather than fixed
        // weapon entities, so the same static template works for any wielder.
        var defaultAttack =
            playerEntity.Being!.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot].Ability!;
        Assert.Same(AbilityData.DoubleMeleeAttack, defaultAttack.Template);

        var activates = defaultAttack.Effects.Select(e => e.Effect!)
            .Where(e => e.EffectType == EffectType.Activate)
            .ToList();
        Assert.Equal(2, activates.Count);

        // Each Activate targets a hand slot (a negative encoded id) and resolves to that hand's fist.
        var primaryActivate = activates.Single(e =>
            e.TargetEntityId == -(int)EquipmentSlot.GraspPrimaryMelee);
        var secondaryActivate = activates.Single(e =>
            e.TargetEntityId == -(int)EquipmentSlot.GraspSecondaryMelee);

        var primaryResolved = AbilityActivationSystem.ResolveActivationTarget(
            primaryActivate.TargetEntityId!.Value, playerEntity, manager);
        Assert.Equal(playerEntity.Being.PrimaryNaturalWeaponId, primaryResolved!.Id);

        var secondaryResolved = AbilityActivationSystem.ResolveActivationTarget(
            secondaryActivate.TargetEntityId!.Value, playerEntity, manager);
        Assert.Equal(playerEntity.Being.SecondaryNaturalWeaponId, secondaryResolved!.Id);
    }
}

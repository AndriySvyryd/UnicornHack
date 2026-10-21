using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Systems.Abilities;

public class AbilityLevelSystemTest
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Non_decreasing_ordinary_levels_follow_grant_removal_and_expiration(bool leveled, bool cumulative, bool expire)
    {
        Ability template = leveled
            ? new LeveledAbility
            {
                Cumulative = cumulative,
                LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
                {
                    [1] = [], [2] = [],
                    [3] = [new ChangeProperty<int> { TargetActivator = true, PropertyName = "Armor", Amount = "3" }]
                }
            }
            : new Ability
            {
                Effects = [new ChangeProperty<int> { TargetActivator = true, PropertyName = "Armor", Amount = "3" }]
            };
        template.Name = "test non-decreasing ordinary grants";
        template.Type = AbilityType.Buff;
        template.NonDecreasingLevel = true;
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(template);
        var actor = CreatureData.Boggart.Instantiate(TestHelper.BuildRoom(".", game), new Point(0, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);
        var initialArmor = actor.Being!.Armor;
        var first = manager.AbilityLevelSystem.AddAbilityEffect(actor, template);
        var second = manager.AbilityLevelSystem.AddAbilityEffect(actor, template, level: 2, sourceAbilityEntityId: 1000);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));
        Assert.Equal(initialArmor + 3, actor.Being.Armor);
        if (leveled && cumulative)
        {
            Assert.NotEqual(0, first.CarrierId);
            Assert.NotEqual(0, second.CarrierId);
            Assert.NotEqual(first.CarrierId, second.CarrierId);
        }

        if (expire)
        {
            second.ExpirationEvent = EffectDuration.UntilTimeout;
            second.ExpirationTarget = manager.Game.CurrentTick + 10;
            manager.Game.CurrentTick += 10;
            manager.EffectApplicationSystem.ExpireTimedPropertyEffects(actor, manager);
        }
        else
        {
            manager.AbilityLevelSystem.RemoveAbility(actor, template.Name, manager, sourceAbilityEntityId: 1000);
        }

        manager.Queue.ProcessQueue(manager);
        Assert.Same(first, Assert.Single(actor.ActiveEffects!.ComponentEffects.GetValues(template.Name)));
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));
        Assert.Equal(1, manager.AffectableAbilitiesIndex[(actor.Id, template.Name)]!.Ability!.Level);
        Assert.Equal(initialArmor + (leveled ? 0 : 3), actor.Being.Armor);

        manager.AbilityLevelSystem.RemoveAbility(actor, template.Name, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));
        Assert.Empty(actor.ActiveEffects.ComponentEffects.GetValues(template.Name));
        Assert.Null(manager.AffectableAbilitiesIndex[(actor.Id, template.Name)]);
        Assert.Equal(initialArmor, actor.Being.Armor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Local_expression_grants_keep_their_source_until_explicitly_removed(bool nonDecreasing)
    {
        var template = new LeveledAbility
        {
            Name = "local expression grant test",
            Type = AbilityType.Buff,
            LevelExpression = "$energyPoints>0?2:0",
            NonDecreasingLevel = nonDecreasing,
            Cumulative = false,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [] }
        };
        var creature = new Creature
        {
            Name = "local expression owner",
            BaseName = CreatureData.Boggart.Name,
            InitialLevel = 1,
            Abilities = [template]
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).Creatures.Add(creature);
        var room = TestHelper.BuildRoom(".", game);
        var actor = creature.Instantiate(room, new Point(0, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);
        var record = Assert.Single(actor.ActiveEffects!.ComponentEffects.GetValues(template.Name));
        Assert.Equal(Assert.Single(actor.Being!.Races).Id, record.SourceAbilityEntityId);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));

        actor.Being.EnergyPoints = 0;
        manager.Queue.ProcessQueue(manager);
        Assert.Same(record, Assert.Single(actor.ActiveEffects.ComponentEffects.GetValues(template.Name)));
        Assert.Equal(nonDecreasing ? 2 : 0, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));
        actor.Being.EnergyPoints = 1;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));

        AbilityLevelSystem.DetachComponentCarrier(record, actor, manager);
        manager.Queue.ProcessQueue(manager);
        actor.Being.EnergyPoints = 0;
        actor.Being.EnergyPoints = 1;
        AbilityLevelSystem.RebuildDynamicAbilities(actor, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Empty(actor.ActiveEffects.ComponentEffects.GetValues(template.Name));
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(actor, template.Name));
        Assert.DoesNotContain(template,
            actor.ActiveEffects.GetDynamicAbilityDependents(nameof(BeingComponent.EnergyPoints)));
    }

    [Theory]
    [InlineData("Balor ua Neit", "Balor's eye cycle")]
    [InlineData("Ellen Trechend", "Ellen Trechend's shriek cycle")]
    [InlineData("lindwurm", "Lindwurm's tail cycle")]
    [InlineData("Rubezahl", "Rubezahl's stone cycle")]
    public void Non_decreasing_boss_phases_start_at_one_and_do_not_regress_after_healing(string creatureName, string abilityName)
    {
        var room = TestHelper.BuildRoom(".");
        var manager = room.Entity.Manager;
        var actor = manager.Game.Services.Data.Creatures.Get(creatureName).Instantiate(room, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(actor, abilityName));

        actor.Being!.HitPoints = actor.Being.HitPointMaximum / 2;
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(actor, abilityName));
        actor.Being.HitPoints = actor.Being.HitPointMaximum;
        AbilityLevelSystem.RebuildDynamicAbilities(actor, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(actor, abilityName));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    public void Creature_dies_when_its_expression_controlled_race_is_non_positive(int rawLevel, bool initiallyInactive)
    {
        var game = TestHelper.CreateGame();
        var creature = new Creature
        {
            Name = CreatureData.Boggart.Name,
            BaseName = CreatureData.Boggart.Name,
            InitialLevel = 1
        };
        var race = creature.GetRaceAbilityTemplate();
        race.LevelExpression = initiallyInactive ? rawLevel.ToString() : "$energyPoints-6";
        var data = (TestDataProvider)game.Services.Data;
        data.Creatures.Add(creature);
        data.AddAbilities(race);
        var room = TestHelper.BuildRoom(".", game);
        var actor = creature.Instantiate(room, new Point(0, 0));
        var manager = game.Manager;
        var actorId = actor.Id;
        var initialDeaths = room.Level.Stats.DeathCount;
        manager.Queue.ProcessQueue(manager);
        if (!initiallyInactive)
        {
            Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(actor, race.Name));
            var record = Assert.Single(actor.ActiveEffects!.ComponentEffects.GetValues(race.Name));
            Assert.Null(record.ConditionValue);
            AbilityLevelSystem.RebuildDynamicAbilities(actor, manager);
            manager.Queue.ProcessQueue(manager);
            Assert.Same(record, Assert.Single(actor.ActiveEffects.ComponentEffects.GetValues(race.Name)));
            Assert.All(actor.Being!.Races, entity => Assert.True(entity.Ability!.Level > 0));

            actor.Being.EnergyPoints = rawLevel + 6;
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Null(manager.FindEntity(actorId));
        Assert.Equal(initialDeaths + 1, room.Level.Stats.DeathCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expression_levels_are_bounded_and_only_positive_levels_apply_effects(bool cumulative)
    {
        var properties = new[] { "Armor", "Evasion", "Deflection" };
        var template = new LeveledAbility
        {
            Name = "bounded expression ability",
            Type = AbilityType.Buff,
            LevelExpression = "$energyPoints-6",
            Cumulative = cumulative,
            LeveledEffects = Enumerable.Range(1, 3).ToDictionary(level => level,
                level => (IReadOnlyList<Effect>)[new ChangeProperty<int>
                {
                    TargetActivator = true, PropertyName = properties[level - 1], Amount = (2 * level).ToString()
                }])
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(template);
        var room = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Being!.EnergyPoints = 6;
        manager.Queue.ProcessQueue(manager);
        var initialValues = properties.Select(property => player.ActiveEffects!.ResolveInt(property)).ToArray();

        foreach (var rawLevel in new[] { -3, 0, 1, 2, 3, 9, 0, -2, 2 })
        {
            player.Being.EnergyPoints = rawLevel + 6;
            manager.Queue.ProcessQueue(manager);

            var expectedLevel = Math.Clamp(rawLevel, 0, 3);
            Assert.Equal(expectedLevel, AbilityLevelSystem.GetAbilityLevel(player, template.Name));
            Assert.Equal(expectedLevel, template.GetExpressionLevel(player));
            for (var index = 0; index < properties.Length; index++)
            {
                var stage = index + 1;
                var active = cumulative ? stage <= expectedLevel : stage == expectedLevel;
                Assert.Equal(initialValues[index] + (active ? 2 * stage : 0), player.ActiveEffects!.ResolveInt(properties[index]));
            }
            if (expectedLevel > 0)
            {
                var record = Assert.Single(player.ActiveEffects!.ComponentEffects.GetValues(template.Name));
                Assert.Null(record.ConditionValue);
                Assert.Equal(expectedLevel, record.Level);
                Assert.Equal(expectedLevel, manager.AffectableAbilitiesIndex[(player.Id, template.Name)]!.Ability!.Level);
            }
        }
    }

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

        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, trait);
        manager.Queue.ProcessQueue(manager);

        var granted = manager.AffectableAbilitiesIndex[(playerEntity.Id, trait.Name)]!.Ability!;
        Assert.Equal(1, granted.Level);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(playerEntity, trait.Name));
        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, trait);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, granted.Level);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(playerEntity, trait.Name));
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

        var first = manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery);
        var second = manager.AbilityLevelSystem.AddAbilityEffect(
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
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
        Assert.Single(playerEntity.ActiveEffects!.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name));

        AbilityLevelSystem.DetachComponentCarrier(second, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Empty(playerEntity.ActiveEffects.ComponentEffects.GetValues(
            AbilityData.WaterSourcery.Name));
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name + "#2")]);
        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(
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

        var first = manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.Stunned);
        var second = manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.Stunned);
        manager.Queue.ProcessQueue(manager);

        Assert.NotEqual(0, first.CarrierId);
        Assert.Same(first, second);
        Assert.Equal(2, first.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, AbilityData.Stunned.Name + "#2")]);
    }

    [Fact]
    public void Expression_levels_follow_sources_and_reject_unconditional_grants()
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
        var dependent = new LeveledAbility
        {
            Name = "expression dependent",
            Type = AbilityType.RangedSpell,
            LevelExpression =
                "AbilityLevel('expression source a') + AbilityLevel('expression source b') >= 2"
                + " ? AbilityLevel('expression source a') + AbilityLevel('expression source b') : 0",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [], [3] = [] }
        };
        var game = TestHelper.CreateGame();
        var data = (TestDataProvider)game.Services.Data;
        data.AddAbilities(sourceA, sourceB, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, sourceA);
        manager.Queue.ProcessQueue(manager);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, dependent.Name)]);

        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, sourceB);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, GetAbilityLevel());
        Assert.Single(playerEntity.ActiveEffects!.ComponentEffects.GetValues(dependent.Name));

        Assert.Throws<InvalidOperationException>(() => manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, dependent, sourceAbilityEntityId: 1000));
        Assert.Equal(2, GetAbilityLevel());
        Assert.Single(playerEntity.ActiveEffects.ComponentEffects.GetValues(dependent.Name));

        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, sourceA);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, GetAbilityLevel());

        manager.AbilityLevelSystem.RemoveAbility(
            playerEntity, sourceB.Name, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, GetAbilityLevel());

        manager.AbilityLevelSystem.RemoveAbility(
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

        manager.AbilityLevelSystem.AddAbilityEffect(player, source);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(1, AbilityLevelSystem.GetAbilityLevel(player, dependent.Name));

        manager.AbilityLevelSystem.AddAbilityEffect(player, source);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(player.Id, dependent.Name)]);
        Assert.Empty(player.ActiveEffects!.ComponentEffects.GetValues(dependent.Name));
    }

    [Fact]
    public void Source_level_change_synchronizes_all_expression_dependents()
    {
        var source = new Ability
        {
            Name = "fan-out expression source",
            Effects = []
        };
        var firstDependent = new LeveledAbility
        {
            Name = "first fan-out expression dependent",
            LevelExpression = "AbilityLevel('fan-out expression source')",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [] }
        };
        var secondDependent = new LeveledAbility
        {
            Name = "second fan-out expression dependent",
            LevelExpression =
                "AbilityLevel('fan-out expression source') > 0"
                + " ? AbilityLevel('fan-out expression source') + 1 : 0",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [], [2] = [], [3] = [] }
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

        manager.AbilityLevelSystem.AddAbilityEffect(player, source, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(player, firstDependent.Name));
        Assert.Equal(3, AbilityLevelSystem.GetAbilityLevel(player, secondDependent.Name));
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
        var dependent = new LeveledAbility
        {
            Name = "repeated expression dependent",
            LevelExpression =
                "AbilityLevel('repeated expression source') + AbilityLevel('repeated expression source')",
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [4] = [] }
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(source, dependent);
        var level = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.AbilityLevelSystem.AddAbilityEffect(player, source, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(4, AbilityLevelSystem.GetAbilityLevel(player, dependent.Name));
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

        manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, source, level: 2);
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

        manager.AbilityLevelSystem.RelevelAbility(
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

        var effect = manager.AbilityLevelSystem.AddAbilityEffect(
            playerEntity, AbilityData.WaterSourcery, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, effect.Level);
        Assert.Equal(2, AbilityLevelSystem.GetAbilityLevel(
            playerEntity, AbilityData.WaterSourcery.Name));
        var record = Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects.GetValues(
                AbilityData.WaterSourcery.Name));
        Assert.Equal(2, record.Level);

        AbilityLevelSystem.DetachComponentCarrier(effect, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, AbilityLevelSystem.GetAbilityLevel(
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
        var firstRecord = manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, template);
        var secondRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, template.Name, sourceAbilityEntityId: 1, level: 2);

        manager.AbilityLevelSystem.RelevelAbility(template.Name, playerEntity, manager);

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
        var firstRecord = manager.AbilityLevelSystem.AddAbilityEffect(playerEntity, template);
        var lastRecord = playerEntity.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, template.Name, level: 2);
        manager.AbilityLevelSystem.RelevelAbility(template.Name, playerEntity, manager);

        Assert.Equal(0, firstRecord.CarrierId);
        Assert.NotEqual(0, lastRecord.CarrierId);
        Assert.Equal(
            "3",
            Assert.Single(manager.FindEntity(lastRecord.CarrierId)!.Ability!.Effects).Effect!.Amount);

        playerEntity.ActiveEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, template.Name, level: 2);
        manager.AbilityLevelSystem.RelevelAbility(template.Name, playerEntity, manager);

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
        manager.AbilityLevelSystem.RelevelAbility(
            AbilityData.WaterSourcery.Name, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        AbilityLevelSystem.DetachComponentCarrier(
            authoritativeRecord, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var authoritative = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name)]!.Ability!;
        Assert.Equal(remainingRecord.CarrierId, authoritative.EntityId);
        Assert.Equal(1, authoritative.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.WaterSourcery.Name + "#2")]);
    }
}

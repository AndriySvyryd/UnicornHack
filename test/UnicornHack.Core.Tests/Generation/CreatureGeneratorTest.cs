using UnicornHack.Data.Branches;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation.Effects;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public class CreatureGeneratorTest
{
    [Fact]
    public void Creature_identifiers_and_authored_spawn_references_are_ascii()
    {
        foreach (var creature in Creature.Loader.GetAll())
        {
            Assert.All(creature.Name, character => Assert.InRange(character, '\0', '\x7f'));
        }

        foreach (var fragment in DefiningMapFragment.Loader.GetAll())
        {
            foreach (var name in fragment.Creatures?.Values ?? Enumerable.Empty<string>())
            {
                Assert.All(name, character => Assert.InRange(character, '\0', '\x7f'));
                Assert.NotNull(Creature.Loader.Find(name));
            }
        }
    }

    [Fact]
    public void Authored_creatures_explicitly_define_their_branch_generation_weights()
    {
        foreach (var creature in Creature.Loader.GetAll())
        {
            Assert.False(string.IsNullOrWhiteSpace(creature.GenerationWeight), creature.Name);
            var weight = Creature.CreateWeightFunction(creature.GenerationWeight!);
            Assert.Equal(0, weight("unspecified_branch", 1, 0));
        }
    }

    [Theory]
    [InlineData("outer wards")]
    [InlineData("ruined stronghold")]
    [InlineData("other")]
    public void Default_creature_weight_is_one_independently_of_branch(string branchName)
    {
        var room = TestHelper.BuildRoom(".");
        room.Level.Branch.Name = branchName;
        var creature = new Creature { Name = "test unweighted creature" };

        Assert.Equal(1, creature.GetWeight(room));
    }

    [Theory]
    [InlineData("outer wards", 1, false)]
    [InlineData("ruined stronghold", 1, false)]
    [InlineData("ruined stronghold", 2, true)]
    [InlineData("ruined stronghold", 3, true)]
    [InlineData("other", 2, false)]
    public void Elf_scout_generation_is_limited_to_the_later_stronghold_floors(string branchName, byte depth, bool eligible)
    {
        var room = TestHelper.BuildRoom(".");
        room.Level.Branch.Name = branchName;
        room.Level.Depth = depth;

        Assert.Equal(eligible, CreatureData.ElfScout.GetWeight(room) > 0);
    }

    [Theory]
    [InlineData("cu sidhe", "three bays", 4)]
    [InlineData("Aillen mac Midgna", "sleeping air and candle", 2)]
    public void Leveled_creature_stages_use_existing_level_increases_and_calculated_resources(
        string name, string controllerName, int stages)
    {
        var room = TestHelper.BuildRoom(".");
        var creature = Creature.Loader.Get(name).Instantiate(room, new Point(0, 0));
        var manager = creature.Manager;
        manager.Queue.ProcessQueue(manager);
        Assert.InRange(creature.Being!.HitPointMaximum, 10 * creature.Being.Might - 1, 10 * creature.Being.Might);
        Assert.InRange(creature.Being.EnergyPointMaximum, 10 * creature.Being.Focus - 1, 10 * creature.Being.Focus);

        for (var stage = 1; stage <= stages; stage++)
        {
            var carrier = manager.AffectableAbilitiesIndex[(creature.Id, controllerName)]!;
            Assert.Equal(stage, carrier.Ability!.Level);
            var template = Assert.IsType<LeveledAbility>(carrier.Ability.Template);
            var attackTemplate = Assert.IsType<AddAbility>(Assert.Single(template.LeveledEffects[stage])).Ability!;
            var attack = manager.AffectableAbilitiesIndex[(creature.Id, attackTemplate.Name)]!.Ability!;
            Assert.Equal(attackTemplate.Action, attack.Action);
            Assert.Equal(attackTemplate.Windup, attack.Windup);
            if (stage < stages)
            {
                manager.AbilityLevelSystem.AddAbilityEffect(creature, template);
                manager.Queue.ProcessQueue(manager);

                Assert.Equal(stage + 1, manager.AffectableAbilitiesIndex[(creature.Id, controllerName)]!.Ability!.Level);
                Assert.Null(manager.AffectableAbilitiesIndex[(creature.Id, attackTemplate.Name)]);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Removing_or_killing_a_stage_controller_removes_its_granted_attack(bool dies)
    {
        var room = TestHelper.BuildRoom(".");
        var actor = CreatureData.CuSidhe.Instantiate(room, new Point(0, 0));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);
        var carrier = manager.AffectableAbilitiesIndex[(actor.Id, "three bays")]!;
        var actorId = actor.Id;
        var controllerName = carrier.Ability!.Name!;
        var template = Assert.IsType<LeveledAbility>(carrier.Ability.Template);
        var attackName = Assert.IsType<AddAbility>(Assert.Single(template.LeveledEffects[1])).Ability!.Name;
        if (dies)
        {
            actor.Being!.HitPoints = 0;
        }
        else
        {
            manager.AbilityLevelSystem.RemoveAbility(actor, controllerName, manager, sourceAbilityEntityId: null);
        }

        manager.Queue.ProcessQueue(manager);

        Assert.Null(manager.AffectableAbilitiesIndex[(actorId, controllerName)]);
        Assert.Null(manager.AffectableAbilitiesIndex[(actorId, attackName)]);
    }

    [Theory]
    [InlineData("Balor ua Neit")]
    [InlineData("Rubezahl")]
    [InlineData("Ellen Trechend")]
    [InlineData("Lindwurm")]
    public void Boss_stages_follow_health_thresholds_and_never_cycle_or_regress(string name)
    {
        var room = TestHelper.BuildRoom(".");
        var actor = Creature.Loader.Get(name).Instantiate(room, new Point(0, 0));
        var manager = actor.Manager;
        manager.Queue.ProcessQueue(manager);
        var raceName = Creature.Loader.Get(name).GetRaceAbilityName();
        var template = Creature.Loader.Get(name).Abilities.OfType<LeveledAbility>().SingleOrDefault()
            ?? actor.Game.Services.Data.Abilities.GetAll().OfType<LeveledAbility>()
                .Single(ability => ability.LevelExpressionInputs.Contains(raceName));
        var first = manager.AffectableAbilitiesIndex[(actor.Id, template.Name)]!;
        Assert.IsType<LeveledAbility>(first.Ability!.Template);
        Assert.Contains("HitPoints", template.LevelExpressionInputs);
        Assert.Equal(1, first.Ability.Level);
        var firstAttack = Assert.IsType<AddAbility>(Assert.Single(template.LeveledEffects[1])).Ability!;
        Assert.NotNull(manager.AffectableAbilitiesIndex[(actor.Id, firstAttack.Name)]);

        actor.Being!.HitPoints = actor.Being.HitPointMaximum / 2;
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, manager.AffectableAbilitiesIndex[(actor.Id, template.Name)]!.Ability!.Level);
        actor.Being.HitPoints = actor.Being.HitPointMaximum;
        manager.Queue.ProcessQueue(manager);

        var second = manager.AffectableAbilitiesIndex[(actor.Id, template.Name)]!.Ability!;
        Assert.Equal(2, second.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[(actor.Id, firstAttack.Name)]);
        var secondAttack = Assert.IsType<AddAbility>(Assert.Single(template.LeveledEffects[2])).Ability!;
        var grantedAttack = manager.AffectableAbilitiesIndex[(actor.Id, secondAttack.Name)]!.Ability!;
        Assert.Equal(secondAttack.Action, grantedAttack.Action);
        Assert.Equal(secondAttack.Windup, grantedAttack.Windup);
    }

    [Fact]
    public void Authored_weights_select_across_initial_level_buckets()
    {
        var map = string.Join('\n', Enumerable.Repeat(new string('.', 25), 16));
        var (room, chunks) = TestHelper.BuildRoomWithChunks(map, seed: 31);
        room.Level.Branch.Name = "outer wards";
        room.Level.Depth = 1;
        new CreatureGenerator { ExpectedInitialCount = 60, MaximumInitialCount = 60 }.Fill(room, chunks);
        room.Game.Manager.Queue.ProcessQueue(room.Game.Manager);

        var selected = room.Actors.Values.Select(actor => Creature.GetTemplate(actor)!.Name).Distinct().Order().ToArray();
        Assert.Equal(new[] { "bat", "boggart", "firefly", "rat" }, selected);
    }

    [Fact]
    public void Henwen_family_stays_non_generating_until_shared_advanced_ai_is_available()
    {
        var room = TestHelper.BuildRoom(".");
        room.Level.Branch.Name = "ruined stronghold";
        room.Level.Depth = 2;
        foreach (var creature in new[]
                 {
                     CreatureData.Henwen, CreatureData.MenwaedsWolf, CreatureData.BrynachsEagle, CreatureData.CathPalug
                 })
        {
            Assert.Equal("0", creature.GenerationWeight);
            Assert.Equal(0, creature.GetWeight(room));
        }
    }

    [Fact]
    public void Generation_checkpoint_restores_committed_counts_without_rewinding_diagnostics()
    {
        var stats = new LevelGenerationStats();
        stats.RecordFragmentInstance("committed room", ["household"], RoomFeatures.RacePedestal);
        stats.RecordCreatureInstance("committed creature", CreatureGenerationFlags.None, false);
        stats.RecordItemInstance("committed item", null);
        stats.RecordFragmentVariant("committed variant");
        var checkpoint = stats.CaptureInstanceCounts();
        stats.RecordFragmentInstance("failed room", ["boss"], RoomFeatures.Boss);
        stats.RecordCreatureInstance("failed creature", CreatureGenerationFlags.LevelBoss, false);
        stats.RecordItemInstance("failed item", null);
        stats.RecordFragmentVariant("failed variant");
        stats.ValidationFailures++;

        stats.RestoreInstanceCounts(checkpoint);

        Assert.Equal("committed room", Assert.Single(stats.FragmentInstancesByName).Key);
        Assert.Equal("household", Assert.Single(stats.FragmentInstancesByTag).Key);
        Assert.Equal("committed creature", Assert.Single(stats.CreatureInstancesByName).Key);
        Assert.Equal("committed item", Assert.Single(stats.ItemInstancesByName).Key);
        Assert.Equal("committed variant", Assert.Single(stats.FragmentInstancesByVariant).Key);
        Assert.Empty(stats.CreatureInstancesByFlag);
        Assert.Equal(1, stats.ValidationFailures);
    }

    [Fact]
    public void Outer_wards_population_targets_six_and_never_exceeds_eight_actors_per_room()
    {
        Assert.Equal(6, BranchData.OuterWards.CreatureGenerator!.ExpectedInitialCount);
        Assert.Equal(8, BranchData.OuterWards.CreatureGenerator.MaximumInitialCount);

        for (uint seed = 1; seed <= 10; seed++)
        {
            var game = TestHelper.CreateGame(seed);
            var branch = BranchData.OuterWards.Instantiate(game);
            var level = LevelGenerator.CreateEmpty(branch, 1, seed, game.Manager);
            LevelGenerator.EnsureGenerated(level);

            Assert.InRange(level.Rooms.Count, 10, 14);
            Assert.All(level.Rooms, room => Assert.InRange(room.Actors.Count, 0, 8));
            Assert.All(level.Rooms, room => Assert.DoesNotContain(room.Terrain.Take(room.TileCount),
                feature => feature is MapFeature.TraitPedestal or MapFeature.FeatPedestal));
        }
    }

    [Fact]
    public void Exact_spawns_enforce_the_population_cap_without_recording_rejected_instances()
    {
        var room = TestHelper.BuildRoom("..");
        var creature = CreatureData.Rat;

        Assert.True(CreatureGenerator.TrySpawnExact(room, new Point(0, 0), creature.Name, maximumInitialCount: 1));
        Assert.False(CreatureGenerator.TrySpawnExact(room, new Point(1, 0), creature.Name, maximumInitialCount: 1));
        room.Entity.Manager.Queue.ProcessQueue(room.Entity.Manager);

        Assert.Single(room.Actors);
        Assert.Equal(1, room.Level.GenerationStats.CreatureInstancesByName[creature.Name]);
        Assert.Equal(1, room.Game.GenerationStats.CreatureInstancesByName[creature.Name]);
    }

    [Fact]
    public void Exact_spawns_reject_protected_arrival_cells_without_relocating_or_recording_them()
    {
        var room = TestHelper.BuildRoom(new string('.', 14));
        var source = TestHelper.BuildRoom(".", room.Game);
        var manager = room.Entity.Manager;
        using var connectionReference = manager.CreateEntity();
        var entity = connectionReference.Referenced;
        var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
        position.RoomEntity = source.Entity;
        position.Cell = new Point(0, 0);
        entity.Position = position;
        var connection = manager.CreateComponent<ConnectionComponent>(EntityComponent.Connection);
        connection.TargetRoomEntity = room.Entity;
        connection.TargetCell = new Point(0, 0);
        connection.Directionality = Directionality.Source;
        entity.Connection = connection;
        manager.Queue.ProcessQueue(manager);

        Assert.False(CreatureGenerator.TrySpawnExact(room, new Point(0, 0), CreatureData.Rat.Name));
        Assert.False(CreatureGenerator.TrySpawnExact(room, new Point(CreatureGenerator.ConnectionExclusionRadius, 0), CreatureData.Rat.Name));
        Assert.Empty(room.Actors);
        Assert.Empty(room.Level.GenerationStats.CreatureInstancesByName);
        Assert.Empty(room.Game.GenerationStats.CreatureInstancesByName);
        Assert.True(CreatureGenerator.TrySpawnExact(
            room, new Point(CreatureGenerator.ConnectionExclusionRadius + 1, 0), CreatureData.Rat.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Authored_exact_spawns_relocate_to_the_nearest_free_walkable_cell_outside_the_arrival_zone(int obstruction)
    {
        var (room, chunks) = TestHelper.BuildRoomWithChunks(new string('.', 14));
        var source = TestHelper.BuildRoom(".", room.Game);
        var manager = room.Entity.Manager;
        var connection = LevelConnection.CreateSourceConnection(source, new Point(0, 0), new LevelConnection
        {
            BranchName = room.BranchName,
            Depth = room.Depth
        });
        connection.TargetCell = new Point(0, 0);
        var nearest = new Point(CreatureGenerator.ConnectionExclusionRadius + 1, 0);
        if (obstruction == 1)
        {
            CreatureData.Boggart.Instantiate(room, nearest);
        }
        else if (obstruction == 2)
        {
            room.Terrain[room.PointToIndex![nearest.X, nearest.Y]] = MapFeature.StoneWall;
        }

        manager.Queue.ProcessQueue(manager);
        using var parameters = TestHelper.CreateRoomParameters(room);
        var chunk = new Chunk(room, room.BoundingRectangle, chunks[0].Fragment,
            doorwayPoints: null, insidePoints: chunks[0].InsidePoints, perimeterPoints: null,
            roomParameters: parameters,
            creaturePoints: [(new Point(0, 0), CreatureData.EllenTrechend.Name)]);

        new CreatureGenerator { ExpectedInitialCount = 0, MaximumInitialCount = 2 }.Fill(room, [chunk]);
        manager.Queue.ProcessQueue(manager);

        var boss = Assert.Single(room.Actors.Values, actor => Creature.GetTemplate(actor)!.IsBoss);
        Assert.Equal(new Point((byte)(nearest.X + (obstruction == 0 ? 0 : 1)), 0), boss.Position!.Cell);
        Assert.True(boss.Position.Cell!.Value.DistanceTo(new Point(0, 0)) > CreatureGenerator.ConnectionExclusionRadius);
        Assert.Equal(1, room.Level.GenerationStats.CreatureInstancesByName[CreatureData.EllenTrechend.Name]);
        Assert.Equal(1, room.Game.GenerationStats.CreatureInstancesByName[CreatureData.EllenTrechend.Name]);
        Assert.Equal(CreatureData.EllenTrechend.Name, Assert.Single(chunk.CreaturePoints).CreatureName);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Exact_unique_spawns_record_counts_and_roll_back_with_the_level(bool boss)
    {
        var room = TestHelper.BuildRoom("..");
        var creature = boss ? CreatureData.EllenTrechend : CreatureData.CathPalug;
        var game = room.Game.GenerationStats;
        var level = room.Level.GenerationStats;

        Assert.True(CreatureGenerator.TrySpawnExact(room, new Point(0, 0), creature.Name.ToLowerInvariant()));
        room.Entity.Manager.Queue.ProcessQueue(room.Entity.Manager);
        Assert.False(CreatureGenerator.TrySpawnExact(room, new Point(1, 0), creature.Name));
        Assert.Equal(1, game.CreatureInstancesByName[creature.Name]);
        Assert.Equal(1, level.CreatureInstancesByName[creature.Name]);
        Assert.Equal(boss ? 0 : 1, game.NamedNonBossCreatureCount);

        game.RemoveCreatureInstances(level);
        level.ClearCreatureInstances();

        Assert.Empty(game.CreatureInstancesByName);
        Assert.Empty(level.CreatureInstancesByName);
        Assert.Empty(game.CreatureInstancesByFlag);
        Assert.Empty(level.CreatureInstancesByFlag);
        Assert.Equal(0, game.NamedNonBossCreatureCount);
        Assert.Equal(0, level.NamedNonBossCreatureCount);
    }

    [Theory]
    [InlineData("outer wards")]
    public void Outer_wards_population_is_selected_only_by_authored_weights(string branchName)
    {
        var room = TestHelper.BuildRoom(".");
        room.Level.Branch.Name = branchName;
        var eligible = Creature.Loader.GetAll()
            .Where(creature => creature.GetWeight(room) > 0)
            .Select(creature => creature.Name)
            .Order()
            .ToArray();

        Assert.Equal(new[] { "bat", "boggart", "firefly", "rat" }, eligible);
    }

    [Fact]
    public void Proper_name_templates_use_capitalized_display_names()
    {
        Assert.Equal("Keteb Meriri", CreatureData.KetebMeriri.Name);
        Assert.Equal("Damned Thing", CreatureData.DamnedThing.Name);
        Assert.Equal("Threshold-Breaker", CreatureData.ThresholdBreaker.Name);
        Assert.Equal("Azhdaha", CreatureData.Azhdaha.Name);
        Assert.Equal("Rise", CreatureData.Rise.Name);
        Assert.Equal("Robed Devil", CreatureData.RobedDevil.Name);
        Assert.Equal("lemure", CreatureData.Lemure.Name);
        Assert.True(CreatureData.KetebMeriri.IsNamedNonBoss);
        Assert.Same(CreatureData.KetebMeriri, Creature.Loader.Find(CreatureData.KetebMeriri.Name));
        Assert.Null(Creature.Loader.Find("Hundred-Eyed Demon Lord"));
        Assert.Same(CreatureData.Lemure, Creature.Loader.Find(CreatureData.Lemure.Name));
        Assert.Null(Creature.Loader.Find("lemur"));
    }

    [Fact]
    public void Shared_properties_fall_back_to_base_creature_and_allow_local_overrides()
    {
        Creature baseCreature = CreatureData.Manusya;
        var derivedCreature = new Creature { BaseName = baseCreature.Name };
        var properties = typeof(Being).GetProperties()
            .Where(property => property.DeclaringType == typeof(Being)
                               && property.CanRead
                               && property.CanWrite
                               && property.Name is not nameof(Being.BaseName)
                                   and not nameof(Being.Name)
                                   and not nameof(Being.AbilityReferences))
            .ToArray();

        foreach (var property in properties)
        {
            var baseValue = property.GetValue(baseCreature);
            Assert.Equal(baseValue, property.GetValue(derivedCreature));

            var overrideValue = GetOverrideValue(property.PropertyType, baseValue);
            property.SetValue(derivedCreature, overrideValue);
            Assert.Equal(overrideValue, property.GetValue(derivedCreature));
        }
    }

    /// <summary>
    ///     Regression: no creature may be generated within
    ///     <see cref="CreatureGenerator.ConnectionExclusionRadius" /> cells (Chebyshev) of an
    ///     incoming inter-level connection's landing cell — the player arrives there, so they must
    ///     never materialize surrounded by monsters. The connecting-fragment weight-zeroing in
    ///     <see cref="CreatureGenerator" /> does NOT cover this case: the room that <em>receives</em>
    ///     an incoming connection hosts the landing cell on its regular (defining) fragment, whose
    ///     chunk has normal creature weight, so a dedicated exclusion zone is required.
    /// </summary>
    [Fact]
    public void No_creature_within_exclusion_radius_of_entrance_incoming_connection()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        var surfaceBranch = BranchData.Surface.Instantiate(game);
        BranchData.OuterWards.Instantiate(game);
        BranchData.RuinedStronghold.Instantiate(game);

        var surfaceLevel = LevelGenerator.CreateEmpty(surfaceBranch, depth: 1, seed: 1u, manager);
        LevelGenerator.EnsureGenerated(surfaceLevel);

        var entranceRoom = surfaceLevel.Rooms.First().Connections.Single().Value.Connection!.TargetRoomEntity.Room!;
        LevelGenerator.EnsureGenerated(entranceRoom);

        var landingCell = entranceRoom.IncomingConnections
            .Select(c => c.Connection!.TargetCell)
            .Single(c => c != null)!.Value;

        foreach (var (cell, actorEntity) in entranceRoom.Actors)
        {
            if (!actorEntity.HasComponent(EntityComponent.AI))
            {
                continue;
            }

            var distance = cell.DistanceTo(landingCell);
            Assert.True(distance > CreatureGenerator.ConnectionExclusionRadius,
                $"Creature at {cell} is {distance} cells from the incoming connection at {landingCell}; "
                + $"expected more than {CreatureGenerator.ConnectionExclusionRadius}.");
        }
    }

    private static object GetOverrideValue(Type propertyType, object? baseValue)
    {
        var valueType = Nullable.GetUnderlyingType(propertyType)!;
        if (valueType == typeof(int))
        {
            return !Equals(baseValue, 25) ? 25 : 50;
        }

        if (valueType == typeof(bool))
        {
            return !Equals(baseValue, true);
        }

        return Enum.GetValues(valueType)
            .Cast<object>()
            .First(value => !Equals(value, baseValue));
    }
}

using UnicornHack.Hubs.ChangeTracking;
using Xunit;

namespace UnicornHack.TestUtilities;

public static class ChangeSetAssert
{
    public static void AssertPlayerStatesEqual(PlayerChange expected, PlayerChange actual)
    {
        Assert.True(expected.Name == actual.Name, $"Player Name expected '{expected.Name}', actual '{actual.Name}'");
        Assert.True(expected.CurrentTick == actual.CurrentTick, $"Player CurrentTick expected {expected.CurrentTick}, actual {actual.CurrentTick}");
        Assert.True(expected.NextActionTick == actual.NextActionTick, $"Player NextActionTick expected {expected.NextActionTick}, actual {actual.NextActionTick}");
        Assert.True(expected.XP == actual.XP, $"Player XP expected {expected.XP}, actual {actual.XP}");
        Assert.True(expected.Hp == actual.Hp, $"Player Hp expected {expected.Hp}, actual {actual.Hp}");
        Assert.True(expected.MaxHp == actual.MaxHp, $"Player MaxHp expected {expected.MaxHp}, actual {actual.MaxHp}");
        Assert.True(expected.Ep == actual.Ep, $"Player Ep expected {expected.Ep}, actual {actual.Ep}");
        Assert.True(expected.MaxEp == actual.MaxEp, $"Player MaxEp expected {expected.MaxEp}, actual {actual.MaxEp}");
        Assert.True(expected.ReservedEp == actual.ReservedEp, $"Player ReservedEp expected {expected.ReservedEp}, actual {actual.ReservedEp}");
        Assert.True(expected.Fortune == actual.Fortune, $"Player Fortune expected {expected.Fortune}, actual {actual.Fortune}");

        AssertAbilitiesEqual(expected.Abilities!, actual.Abilities!);
        AssertRacesEqual(expected.Races!, actual.Races!);
        AssertLogContainsAll(expected.Log!, actual.Log!);
        AssertLevelEqual(expected.Level!, actual.Level!);
    }

    private static void AssertAbilitiesEqual(Dictionary<int, AbilityChange> expected, Dictionary<int, AbilityChange> actual)
    {
        var expectedKeys = expected.Keys.OrderBy(k => k).ToList();
        var actualKeys = actual.Keys.OrderBy(k => k).ToList();
        Assert.True(expectedKeys.SequenceEqual(actualKeys),
            $"Ability keys differ. Expected: [{string.Join(", ", expectedKeys)}], Actual: [{string.Join(", ", actualKeys)}]");

        foreach (var kvp in expected)
        {
            var exp = kvp.Value;
            var act = actual[kvp.Key];
            Assert.True(exp.Name == act.Name, $"Ability {kvp.Key}: Name expected '{exp.Name}', actual '{act.Name}'");
            Assert.True(exp.Activation == act.Activation, $"Ability {kvp.Key} '{exp.Name}': Activation expected {exp.Activation}, actual {act.Activation}");
            Assert.True(exp.Slot == act.Slot, $"Ability {kvp.Key} '{exp.Name}': Slot expected {exp.Slot}, actual {act.Slot}");
            Assert.True(exp.CooldownTick == act.CooldownTick, $"Ability {kvp.Key} '{exp.Name}': CooldownTick expected {exp.CooldownTick}, actual {act.CooldownTick}");
            Assert.True(exp.CooldownXpLeft == act.CooldownXpLeft, $"Ability {kvp.Key} '{exp.Name}': CooldownXpLeft expected {exp.CooldownXpLeft}, actual {act.CooldownXpLeft}");
            Assert.True(exp.TargetingShape == act.TargetingShape, $"Ability {kvp.Key} '{exp.Name}': TargetingShape expected {exp.TargetingShape}, actual {act.TargetingShape}");
            Assert.True(exp.TargetingShapeSize == act.TargetingShapeSize, $"Ability {kvp.Key} '{exp.Name}': TargetingShapeSize expected {exp.TargetingShapeSize}, actual {act.TargetingShapeSize}");
        }
    }

    private static void AssertRacesEqual(Dictionary<int, RaceChange> expected, Dictionary<int, RaceChange> actual)
    {
        var expectedKeys = expected.Keys.OrderBy(k => k).ToList();
        var actualKeys = actual.Keys.OrderBy(k => k).ToList();
        Assert.True(expectedKeys.SequenceEqual(actualKeys),
            $"Race keys differ. Expected: [{string.Join(", ", expectedKeys)}], Actual: [{string.Join(", ", actualKeys)}]");

        foreach (var kvp in expected)
        {
            var exp = kvp.Value;
            var act = actual[kvp.Key];
            Assert.True(exp.Name == act.Name, $"Race {kvp.Key}: Name expected '{exp.Name}', actual '{act.Name}'");
            Assert.True(exp.ShortName == act.ShortName, $"Race {kvp.Key} '{exp.Name}': ShortName expected '{exp.ShortName}', actual '{act.ShortName}'");
        }
    }

    private static void AssertLogContainsAll(Dictionary<int, LogEntryChange> expected, Dictionary<int, LogEntryChange> actual)
    {
        foreach (var kvp in expected)
        {
            Assert.True(actual.ContainsKey(kvp.Key),
                $"Log entry {kvp.Key} ('{kvp.Value.Message}') missing from delta-applied state");
            Assert.Equal(kvp.Value.Message, actual[kvp.Key].Message);
            Assert.Equal(kvp.Value.Ticks, actual[kvp.Key].Ticks);
        }
    }

    private static void AssertLevelEqual(LevelChange expected, LevelChange actual)
    {
        Assert.True(expected.BranchName == actual.BranchName, $"Level BranchName expected '{expected.BranchName}', actual '{actual.BranchName}'");
        Assert.True(expected.Depth == actual.Depth, $"Level Depth expected {expected.Depth}, actual {actual.Depth}");
        Assert.True(expected.Width == actual.Width, $"Level Width expected {expected.Width}, actual {actual.Width}");
        Assert.True(expected.Height == actual.Height, $"Level Height expected {expected.Height}, actual {actual.Height}");

        AssertActorsEqual(expected.Actors!, actual.Actors!);
        AssertItemsEqual(expected.Items!, actual.Items!);
        AssertConnectionsEqual(expected.Connections!, actual.Connections!);
        AssertTerrainEqual(expected, actual);
    }

    private static void AssertActorsEqual(Dictionary<int, ActorChange> expected, Dictionary<int, ActorChange> actual)
    {
        var expectedKeys = expected.Keys.OrderBy(k => k).ToList();
        var actualKeys = actual.Keys.OrderBy(k => k).ToList();
        Assert.True(expectedKeys.SequenceEqual(actualKeys),
            $"Actor keys differ. Expected: [{string.Join(", ", expectedKeys)}], Actual: [{string.Join(", ", actualKeys)}]");

        foreach (var kvp in expected)
        {
            var exp = kvp.Value;
            var act = actual[kvp.Key];
            Assert.True(exp.LevelX == act.LevelX, $"Actor {kvp.Key}: LevelX expected {exp.LevelX}, actual {act.LevelX}");
            Assert.True(exp.LevelY == act.LevelY, $"Actor {kvp.Key}: LevelY expected {exp.LevelY}, actual {act.LevelY}");
            Assert.True(exp.Heading == act.Heading, $"Actor {kvp.Key}: Heading expected {exp.Heading}, actual {act.Heading}");
            Assert.True(exp.BaseName == act.BaseName, $"Actor {kvp.Key}: BaseName expected '{exp.BaseName}', actual '{act.BaseName}'");
            Assert.True(exp.Name == act.Name, $"Actor {kvp.Key}: Name expected '{exp.Name}', actual '{act.Name}'");
            Assert.True(exp.IsCurrentlyPerceived == act.IsCurrentlyPerceived, $"Actor {kvp.Key}: IsCurrentlyPerceived expected {exp.IsCurrentlyPerceived}, actual {act.IsCurrentlyPerceived}");
            Assert.True(exp.Hp == act.Hp, $"Actor {kvp.Key}: Hp expected {exp.Hp}, actual {act.Hp}");
            Assert.True(exp.MaxHp == act.MaxHp, $"Actor {kvp.Key}: MaxHp expected {exp.MaxHp}, actual {act.MaxHp}");
            Assert.True(exp.Ep == act.Ep, $"Actor {kvp.Key}: Ep expected {exp.Ep}, actual {act.Ep}");
            Assert.True(exp.MaxEp == act.MaxEp, $"Actor {kvp.Key}: MaxEp expected {exp.MaxEp}, actual {act.MaxEp}");
            Assert.True(exp.NextActionTick == act.NextActionTick, $"Actor {kvp.Key}: NextActionTick expected {exp.NextActionTick}, actual {act.NextActionTick}");
        }
    }

    private static void AssertItemsEqual(Dictionary<int, ItemChange> expected, Dictionary<int, ItemChange> actual)
    {
        var expectedKeys = expected.Keys.OrderBy(k => k).ToList();
        var actualKeys = actual.Keys.OrderBy(k => k).ToList();
        Assert.True(expectedKeys.SequenceEqual(actualKeys),
            $"Item keys differ. Expected: [{string.Join(", ", expectedKeys)}], Actual: [{string.Join(", ", actualKeys)}]");

        foreach (var kvp in expected)
        {
            var exp = kvp.Value;
            var act = actual[kvp.Key];
            Assert.True(exp.LevelX == act.LevelX, $"Item {kvp.Key} '{exp.BaseName}': LevelX expected {exp.LevelX}, actual {act.LevelX}");
            Assert.True(exp.LevelY == act.LevelY, $"Item {kvp.Key} '{exp.BaseName}': LevelY expected {exp.LevelY}, actual {act.LevelY}");
            Assert.True(exp.Type == act.Type, $"Item {kvp.Key} '{exp.BaseName}': Type expected {exp.Type}, actual {act.Type}");
            Assert.True(exp.BaseName == act.BaseName, $"Item {kvp.Key}: BaseName expected '{exp.BaseName}', actual '{act.BaseName}'");
            Assert.True(exp.Name == act.Name, $"Item {kvp.Key} '{exp.BaseName}': Name expected '{exp.Name}', actual '{act.Name}'");
            Assert.True(exp.IsCurrentlyPerceived == act.IsCurrentlyPerceived, $"Item {kvp.Key} '{exp.BaseName}': IsCurrentlyPerceived expected {exp.IsCurrentlyPerceived}, actual {act.IsCurrentlyPerceived}");
        }
    }

    private static void AssertConnectionsEqual(Dictionary<int, ConnectionChange> expected, Dictionary<int, ConnectionChange> actual)
    {
        var expectedKeys = expected.Keys.OrderBy(k => k).ToList();
        var actualKeys = actual.Keys.OrderBy(k => k).ToList();
        Assert.True(expectedKeys.SequenceEqual(actualKeys),
            $"Connection keys differ. Expected: [{string.Join(", ", expectedKeys)}], Actual: [{string.Join(", ", actualKeys)}]");

        foreach (var kvp in expected)
        {
            var exp = kvp.Value;
            var act = actual[kvp.Key];
            Assert.True(exp.LevelX == act.LevelX, $"Connection {kvp.Key}: LevelX expected {exp.LevelX}, actual {act.LevelX}");
            Assert.True(exp.LevelY == act.LevelY, $"Connection {kvp.Key}: LevelY expected {exp.LevelY}, actual {act.LevelY}");
            Assert.True(exp.IsDown == act.IsDown, $"Connection {kvp.Key}: IsDown expected {exp.IsDown}, actual {act.IsDown}");
        }
    }

    private static void AssertTerrainEqual(LevelChange expectedLevel, LevelChange actualLevel)
    {
        var expectedMap = expectedLevel.LevelMap;
        var actualMap = actualLevel.LevelMap;
        Assert.NotNull(expectedMap);
        Assert.NotNull(actualMap);

        var tileCount = expectedLevel.Width * expectedLevel.Height;
        for (var i = 0; i < tileCount; i++)
        {
            if (expectedMap.Terrain[i] != actualMap.Terrain[i])
            {
                Assert.Fail($"Terrain mismatch at tile {i}: expected {expectedMap.Terrain[i]}, actual {actualMap.Terrain[i]}");
            }

            if (expectedMap.WallNeighbors[i] != actualMap.WallNeighbors[i])
            {
                Assert.Fail($"WallNeighbors mismatch at tile {i}: expected {expectedMap.WallNeighbors[i]}, actual {actualMap.WallNeighbors[i]}");
            }

            if (expectedMap.VisibleTerrain[i] != actualMap.VisibleTerrain[i])
            {
                Assert.Fail($"VisibleTerrain mismatch at tile {i}: expected {expectedMap.VisibleTerrain[i]}, actual {actualMap.VisibleTerrain[i]}");
            }
        }
    }

    /// <summary>
    ///     Validates a sequence of change sets and applies them to <paramref name="runningState" />.
    ///     For each change set the running state represents what the client has just before
    ///     receiving it; any property bit set in the change set must differ from the running
    ///     state value (otherwise the server is wasting bandwidth on a value the client
    ///     already has). After validation, the change set is applied to the running state so
    ///     the next iteration compares against the just-updated state.
    /// </summary>
    public static void ApplyAndAssertNoRedundantChanges(PlayerChange runningState, List<TurnChangeSet> changeSets)
    {
        // Bits 13–16 (attack summaries) are excluded from this check: they are derived
        // from many inputs (positions, HP, equipment) that can oscillate legitimately
        // across a multi-turn sequence, and the server can't know which value the
        // client received via the initial GetState snapshot. Their non-redundancy is
        // enforced by the production-code OnAfterEmit cache in ActorChangeBuilder
        // (covered by ChangeBuilderTest's targeted assertions).
        foreach (var changeSet in changeSets)
        {
            var player = changeSet.PlayerState;
            var level = player.Level;

            if (level != null && level.ChangedProperties != null)
            {
                if (level.Actors != null)
                {
                    AssertNoRedundantEntityChanges(level.Actors, runningState.Level!.Actors!, AssertActorPropertiesChanged);
                }

                if (level.Items != null)
                {
                    AssertNoRedundantEntityChanges(level.Items, runningState.Level!.Items!, AssertItemPropertiesChanged);
                }

                if (level.Connections != null)
                {
                    AssertNoRedundantEntityChanges(level.Connections, runningState.Level!.Connections!, AssertConnectionPropertiesChanged);
                }
            }

            if (player.ChangedProperties![6] && player.Races != null)
            {
                AssertNoRedundantEntityChanges(player.Races, runningState.Races!, AssertRacePropertiesChanged);
            }

            if (player.ChangedProperties[7] && player.Abilities != null)
            {
                AssertNoRedundantEntityChanges(player.Abilities, runningState.Abilities!, AssertAbilityPropertiesChanged);
            }

            ClientStateHelpers.ApplyDelta(runningState, player);
        }
    }

    private static void AssertNoRedundantEntityChanges<TChange>(
        Dictionary<int, TChange> changes,
        Dictionary<int, TChange> baselineEntities,
        Action<int, TChange, TChange> assertPropertyChanged)
        where TChange : IChangeWithState
    {
        foreach (var (id, change) in changes)
        {
            var bits = change.ChangedProperties;

            if (bits == null)
            {
                continue;
            }

            if (bits.Length == 1)
            {
                continue;
            }

            if (baselineEntities.TryGetValue(id, out var baselineEntity))
            {
                assertPropertyChanged(id, change, baselineEntity);
            }
        }
    }

    private static void AssertActorPropertiesChanged(int id, ActorChange change, ActorChange baseline)
    {
        var bits = change.ChangedProperties!;
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var same = i switch
            {
                1 => change.LevelX == baseline.LevelX,
                2 => change.LevelY == baseline.LevelY,
                3 => change.Heading == baseline.Heading,
                4 => change.BaseName == baseline.BaseName,
                5 => change.Name == baseline.Name,
                6 => change.IsCurrentlyPerceived == baseline.IsCurrentlyPerceived,
                7 => change.Hp == baseline.Hp,
                8 => change.MaxHp == baseline.MaxHp,
                9 => change.Ep == baseline.Ep,
                10 => change.MaxEp == baseline.MaxEp,
                11 => change.NextActionTick == baseline.NextActionTick,
                12 => NextActionEquals(change.NextAction, baseline.NextAction),
                // Bits 13-16 (attack summaries) are derived from inputs that may oscillate
                // legitimately within a multi-turn sequence and detecting their redundancy
                // would add more perf impact than it's worth.
                _ => false
            };

            Assert.False(same, $"Actor {id}: property index {i} flagged but value unchanged from baseline");
        }
    }

    private static void AssertItemPropertiesChanged(int id, ItemChange change, ItemChange baseline)
    {
        var bits = change.ChangedProperties!;
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var same = i switch
            {
                1 => change.LevelX == baseline.LevelX,
                2 => change.LevelY == baseline.LevelY,
                3 => change.Type == baseline.Type,
                4 => change.BaseName == baseline.BaseName,
                5 => change.Name == baseline.Name,
                6 => change.IsCurrentlyPerceived == baseline.IsCurrentlyPerceived,
                _ => false
            };

            Assert.False(same, $"Item {id}: property index {i} flagged but value unchanged from baseline");
        }
    }

    private static void AssertConnectionPropertiesChanged(int id, ConnectionChange change, ConnectionChange baseline)
    {
        var bits = change.ChangedProperties!;
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var same = i switch
            {
                1 => change.LevelX == baseline.LevelX,
                2 => change.LevelY == baseline.LevelY,
                3 => change.IsDown == baseline.IsDown,
                _ => false
            };

            Assert.False(same, $"Connection {id}: property index {i} flagged but value unchanged from baseline");
        }
    }

    private static void AssertRacePropertiesChanged(int id, RaceChange change, RaceChange baseline)
    {
        var bits = change.ChangedProperties!;
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var same = i switch
            {
                1 => change.Name == baseline.Name,
                2 => change.ShortName == baseline.ShortName,
                _ => false
            };

            Assert.False(same, $"Race {id}: property index {i} flagged but value unchanged from baseline");
        }
    }

    private static void AssertAbilityPropertiesChanged(int id, AbilityChange change, AbilityChange baseline)
    {
        var bits = change.ChangedProperties!;
        for (var i = 1; i < bits.Length; i++)
        {
            if (!bits[i])
            {
                continue;
            }

            var same = i switch
            {
                1 => change.Name == baseline.Name,
                2 => change.Activation == baseline.Activation,
                3 => change.Slot == baseline.Slot,
                4 => change.CooldownTick == baseline.CooldownTick,
                5 => change.CooldownXpLeft == baseline.CooldownXpLeft,
                6 => change.TargetingShape == baseline.TargetingShape,
                7 => change.TargetingShapeSize == baseline.TargetingShapeSize,
                _ => false
            };

            Assert.False(same, $"Ability {id}: property index {i} flagged but value unchanged from baseline");
        }
    }

    private static bool NextActionEquals(ActorActionChange? a, ActorActionChange? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Type == b.Type
            && a.Name == b.Name
            && a.Target == b.Target
            && a.TargetingShape == b.TargetingShape
            && a.TargetingShapeSize == b.TargetingShapeSize;
    }

    private static string FormatNextAction(ActorActionChange? a)
        => a == null ? "null" : $"(Type={a.Type}, Name={a.Name}, Target={a.Target})";
}

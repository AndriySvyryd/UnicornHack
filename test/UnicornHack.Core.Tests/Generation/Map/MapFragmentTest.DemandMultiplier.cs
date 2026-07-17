namespace UnicornHack.Generation.Map;

/// <summary>
///     Exercises <see cref="MapFragment.GetDemandMultiplier" />: the per-fragment weight
///     boost that biases selection toward fragments whose tags advertise still-outstanding
///     room feature or intra-level doorway demand.
/// </summary>
public partial class MapFragmentTest
{
    private const float Bonus = MapFragment.DemandTagBonus;
    private const float Penalty = 1f / MapFragment.DemandTagBonus;

    [Fact]
    public void Returns_one_when_room_has_no_outstanding_feature_demand()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.Normal;
        // A fragment with no static feature glyphs and no advertised feature tag must
        // pass through with weight 1 when there's no demand to satisfy.
        var fragment = NewFragment(tags: ["custom"]);

        Assert.Equal(1f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Excludes_fragment_whose_static_map_places_an_undemanded_feature()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.Normal;
        // The `{` glyph commits a SmallFountain MapFeature into the room. Placing this
        // fragment would saddle a Normal room with a fountain it never asked for, so
        // GetDemandMultiplier hard-excludes it via the `small-fountain` feature tag.
        var fragment = NewFragment("""
            ###
            #{#
            ###
            """);

        Assert.Equal(0f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Excludes_fragment_when_static_feature_set_extends_beyond_demand()
    {
        var (room, _) = BuildRoom();
        // Room demands SmallFountain only; the fragment statically places both a fountain
        // (`{`) and a weapon stand (`)`), so it overcommits the room and must be excluded
        // regardless of the partial overlap.
        room.Features = RoomFeatures.SmallFountain;
        var fragment = NewFragment("""
            ####
            #{)#
            ####
            """);

        Assert.Equal(0f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Allows_fragment_when_static_features_match_demand()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain;
        var fragment = NewFragment("""
            ###
            #{#
            ###
            """);

        Assert.Equal(Bonus, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Tag_advertisement_triggers_exclusion_for_undemanded_feature()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.Normal;
        // After the StaticPlaceableFeatures removal, the canonical feature tag is the
        // sole source of truth for "this fragment commits the feature into the room".
        // Any fragment advertising a placeable feature the room didn't demand is now
        // hard-excluded — there's no separate "I can host it dynamically" channel.
        var fragment = NewFragment(tags: ["small-fountain"]);

        Assert.Equal(0f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Wildcard_tag_does_not_trigger_exclusion()
    {
        var (room, _) = BuildRoom();
        // A wildcard-tagged fragment is intentionally generic (it does not pin a specific
        // feature) so a room with no demand must still accept it at neutral weight.
        room.Features = RoomFeatures.Normal;
        var fragment = NewFragment(tags: [MapFragment.WildcardFeatureTag]);

        Assert.Equal(1f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Boosts_fragment_with_matching_feature_tag()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain;
        var fragment = NewFragment(tags: ["small-fountain"]);

        Assert.Equal(Bonus, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Wildcard_tag_matches_any_outstanding_placeable_feature()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain | RoomFeatures.WeaponStand;
        var fragment = NewFragment(tags: [MapFragment.WildcardFeatureTag]);

        // Two outstanding bits: the bonus accumulates linearly, not exponentially.
        Assert.Equal(1f + 2 * (Bonus - 1f), fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Penalises_fragment_with_anti_tag_matching_demanded_feature()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain;
        var fragment = NewFragment(tags: ["custom"], antiTags: ["small-fountain"]);

        Assert.Equal(Penalty, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Multiple_anti_tags_stack_linearly()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain | RoomFeatures.WeaponStand;
        var fragment = NewFragment(tags: ["custom"], antiTags: ["small-fountain", "weapon-stand"]);

        // Two anti-tag matches: the penalty deepens linearly (reciprocal), not exponentially.
        Assert.Equal(1f / (1f + 2 * (1f / Penalty - 1f)), fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Already_bound_feature_does_not_boost()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain;
        room.BoundPlaceableFeatures = RoomFeatures.SmallFountain;
        var fragment = NewFragment(tags: ["small-fountain"]);

        Assert.Equal(1f, fragment.GetDemandMultiplier(room, Ctx()));
    }

    [Fact]
    public void Door_direction_tag_boosts_for_demanded_doorway()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.Normal;
        var fragment = NewFragment(tags: ["door-n"]);

        Assert.Equal(Bonus, fragment.GetDemandMultiplier(room, Ctx(DirectionFlags.North)));
    }

    [Fact]
    public void Door_direction_tag_irrelevant_when_no_demand_on_that_edge()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.Normal;
        var fragment = NewFragment(tags: ["door-n"]);

        Assert.Equal(1f, fragment.GetDemandMultiplier(room, Ctx(DirectionFlags.South)));
    }

    [Fact]
    public void Multiple_demands_stack_linearly()
    {
        var (room, _) = BuildRoom();
        room.Features = RoomFeatures.SmallFountain;
        var fragment = NewFragment(tags: ["small-fountain", "door-e"]);

        // A feature match and a doorway match: two demands accumulate linearly.
        Assert.Equal(1f + 2 * (Bonus - 1f), fragment.GetDemandMultiplier(room, Ctx(DirectionFlags.East)));
    }

    private static LotPlacementContext Ctx(DirectionFlags demandedDoorways = DirectionFlags.None)
        => new(
            new Rectangle(new Point(0, 0), 5, 5),
            TestRoomParameters(),
            TargetEdge: null,
            EmptySpace: null,
            MinLotSize: new Dimensions(1, 1),
            PlacedChunks: null,
            PreservedConditions: null,
            DemandedDoorways: demandedDoorways);
}

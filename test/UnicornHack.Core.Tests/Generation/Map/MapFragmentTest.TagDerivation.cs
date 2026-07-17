namespace UnicornHack.Generation.Map;

/// <summary>
///     <see cref="MapFragment.EnsureInitialized" /> derives auto-tags, splits
///     author-supplied <c>!</c> opt-out entries into <see cref="MapFragment.AntiTags" />,
///     and seeds <see cref="MapFragment.SourceName" />.
/// </summary>
public partial class MapFragmentTest
{
    [Fact]
    public void SourceName_defaults_to_Name_on_source_fragment()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """);
        Assert.Equal("test-fragment", f.SourceName);
    }

    [Fact]
    public void SourceName_reads_through_to_Name_when_never_assigned()
    {
        // Synthetic fragment never goes through EnsureInitialized (no Map).
        var f = new NormalMapFragment { Name = "lazy" };
        Assert.Equal("lazy", f.SourceName);
    }

    [Fact]
    public void Auto_tags_size_small_for_tiny_payload()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """);
        Assert.Contains("size-small", f.Tags!);
        Assert.DoesNotContain("size-medium", f.Tags!);
        Assert.DoesNotContain("size-large", f.Tags!);
    }

    [Fact]
    public void Auto_tags_size_medium_at_threshold_boundary()
    {
        // 4x4 = 16 tiles -> medium (>15).
        var f = NewFragment(@"
....
....
....
....");
        Assert.Contains("size-medium", f.Tags!);
        Assert.DoesNotContain("size-small", f.Tags!);
    }

    [Fact]
    public void Auto_tags_size_large_above_medium_threshold()
    {
        // 8x7 = 56 tiles -> large (>49).
        var f = NewFragment(@"
........
........
........
........
........
........
........");
        Assert.Contains("size-large", f.Tags!);
        Assert.DoesNotContain("size-medium", f.Tags!);
    }

    [Fact]
    public void Authored_negative_tags_move_to_AntiTags()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """, ["custom", "!cursed", "!boss"]);
        Assert.Contains("custom", f.Tags!);
        Assert.DoesNotContain("!cursed", f.Tags!);
        Assert.DoesNotContain("!boss", f.Tags!);
        Assert.NotNull(f.AntiTags);
        Assert.Contains("cursed", f.AntiTags!);
        Assert.Contains("boss", f.AntiTags!);
    }

    [Fact]
    public void AntiTags_stays_null_when_no_negative_entries()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """, ["custom"]);
        Assert.Null(f.AntiTags);
    }

    [Fact]
    public void Empty_negative_tag_throws()
    {
        var f = new NormalMapFragment
        {
            Name = "bad",
            Map = """
                ###
                #.#
                ###
                """,
            Tags = ["!"]
        };

        Assert.Throws<InvalidOperationException>(() => f.EnsureInitialized());
    }

    [Fact]
    public void Auto_tags_altar_from_deity_altar_glyph()
    {
        var f = NewFragment("""
            ###
            #_#
            ###
            """);
        Assert.Contains("altar", f.Tags!);
    }

    [Fact]
    public void Auto_tags_pedestal_from_any_pedestal_glyph()
    {
        foreach (var glyph in new[] { '?', '!', '"', '=' })
        {
            var f = NewFragment($"""
                ###
                #{glyph}#
                ###
                """);
            Assert.Contains("pedestal", f.Tags!);
        }
    }

    [Fact]
    public void Auto_tags_stand_from_weapon_or_armor_stand_glyphs()
    {
        var weapon = NewFragment("""
            ###
            #)#
            ###
            """);
        Assert.Contains("stand", weapon.Tags!);
        var armor = NewFragment("""
            ###
            #[#
            ###
            """);
        Assert.Contains("stand", armor.Tags!);
    }

    [Fact]
    public void Auto_tags_stairs_up_and_down_from_connection_glyphs()
    {
        var up = NewFragment("""
            ###
            #<#
            ###
            """);
        Assert.Contains("entrance", up.Tags!);
        Assert.DoesNotContain("exit", up.Tags!);

        var down = NewFragment("""
            ###
            #>#
            ###
            """);
        Assert.Contains("exit", down.Tags!);
        Assert.DoesNotContain("entrance", down.Tags!);
    }

    [Fact]
    public void Exit_auto_tag_suppressed_when_fragment_authored_as_shortcut()
    {
        // A shortcut fragment may draw down-stairs ('>') for flavour without being a general
        // exit fragment: the auto 'exit' tag is suppressed so the role-tag bias keeps it for
        // shortcut rooms instead of regular exit rooms.
        var shortcutDown = NewFragment("""
            ###
            #>#
            ###
            """, authoredTags: ["shortcut"]);
        Assert.Contains("shortcut", shortcutDown.Tags!);
        Assert.DoesNotContain("exit", shortcutDown.Tags!);

        // Without the shortcut tag, '>' still auto-emits 'exit'.
        var plainDown = NewFragment("""
            ###
            #>#
            ###
            """);
        Assert.Contains("exit", plainDown.Tags!);
    }

    [Fact]
    public void Plain_room_has_no_content_tags()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """);
        Assert.DoesNotContain("altar", f.Tags!);
        Assert.DoesNotContain("pedestal", f.Tags!);
        Assert.DoesNotContain("stand", f.Tags!);
        Assert.DoesNotContain("entrance", f.Tags!);
        Assert.DoesNotContain("exit", f.Tags!);
    }

    [Fact]
    public void Auto_tags_door_n_when_authored_doorway_sits_on_north_edge()
    {
        var f = NewFragment("""
            #+#
            #.#
            ###
            """);
        Assert.Contains("door-n", f.Tags!);
        Assert.DoesNotContain("door-s", f.Tags!);
        Assert.DoesNotContain("door-e", f.Tags!);
        Assert.DoesNotContain("door-w", f.Tags!);
    }

    [Fact]
    public void Auto_tags_door_directions_for_all_four_edges()
    {
        var f = NewFragment("""
            #+#
            +.+
            #+#
            """);
        Assert.Contains("door-n", f.Tags!);
        Assert.Contains("door-s", f.Tags!);
        Assert.Contains("door-e", f.Tags!);
        Assert.Contains("door-w", f.Tags!);
    }

    [Fact]
    public void Auto_tags_no_door_when_no_doorway_glyphs_authored()
    {
        var f = NewFragment("""
            ###
            #.#
            ###
            """);
        Assert.DoesNotContain("door-n", f.Tags!);
        Assert.DoesNotContain("door-s", f.Tags!);
        Assert.DoesNotContain("door-e", f.Tags!);
        Assert.DoesNotContain("door-w", f.Tags!);
    }

    [Fact]
    public void Auto_tags_door_direction_rotates_with_variant()
    {
        var source = NewFragment("""
            #+#
            #.#
            ###
            """);
        Assert.Contains("door-n", source.Tags!);

        var rotated = source.CreateVariant(MapFragmentTransform.Rotate90);
        Assert.Contains("door-e", rotated.Tags!);
        Assert.DoesNotContain("door-n", rotated.Tags!);
        // Source must remain unchanged when its variant is materialized.
        Assert.Contains("door-n", source.Tags!);
        Assert.DoesNotContain("door-e", source.Tags!);
    }

    [Theory]
    [InlineData('_', "altar", "deity-altar")]
    [InlineData('\\', "shrine", "major-shrine")]
    [InlineData('/', "shrine", "minor-shrine")]
    [InlineData('&', "shrine", "ritual-shrine")]
    [InlineData('{', "fountain", "small-fountain")]
    [InlineData('}', "fountain", "big-fountain")]
    [InlineData('?', "pedestal", "skill-pedestal")]
    [InlineData('!', "pedestal", "race-pedestal")]
    [InlineData('"', "pedestal", "trait-pedestal")]
    [InlineData('=', "pedestal", "feat-pedestal")]
    [InlineData('[', "stand", "armor-stand")]
    [InlineData('(', "stand", "item-case")]
    [InlineData(')', "stand", "weapon-stand")]
    public void Auto_tags_emit_both_generic_and_specific_feature_tags(char glyph, string generic, string specific)
    {
        var f = NewFragment($"""
            ###
            #{glyph}#
            ###
            """);
        Assert.Contains(generic, f.Tags!);
        Assert.Contains(specific, f.Tags!);
    }

    [Fact]
    public void Auto_tags_wildcard_feature_from_dollar_glyph()
    {
        var f = NewFragment("""
            ###
            #$#
            ###
            """);
        Assert.Contains(MapFragment.WildcardFeatureTag, f.Tags!);
    }

    [Fact]
    public void FeatureTagNames_maps_stairs_directions_to_entrance_and_exit()
    {
        // Entrance and Exit map to the `entrance` / `exit` tags per design.
        Assert.Equal("entrance", MapFragment.FeatureTagNames[RoomFeatures.Entrance]);
        Assert.Equal("exit", MapFragment.FeatureTagNames[RoomFeatures.Exit]);
    }
}

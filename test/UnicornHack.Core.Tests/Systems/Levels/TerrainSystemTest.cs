namespace UnicornHack.Systems.Levels;

public class TerrainSystemTest
{
    [Fact]
    public void Special_terrain_values_and_inactive_pairs_are_stable()
    {
        Assert.Equal(6, (byte)MapFeature.Pool);
        Assert.Equal((byte)MapFeature.DeityAltar + 1, (byte)MapFeature.InactiveDeityAltar);
        Assert.Equal((byte)MapFeature.MajorShrine + 1, (byte)MapFeature.InactiveMajorShrine);
        Assert.Equal((byte)MapFeature.MinorShrine + 1, (byte)MapFeature.InactiveMinorShrine);
        Assert.Equal((byte)MapFeature.RitualShrine + 1, (byte)MapFeature.InactiveRitualShrine);
        Assert.Equal((byte)MapFeature.SmallFountain + 1, (byte)MapFeature.InactiveSmallFountain);
        Assert.Equal((byte)MapFeature.BigFountain + 1, (byte)MapFeature.InactiveBigFountain);
        Assert.Equal((byte)MapFeature.SkillPedestal + 1, (byte)MapFeature.InactiveSkillPedestal);
        Assert.Equal((byte)MapFeature.RacePedestal + 1, (byte)MapFeature.InactiveRacePedestal);
        Assert.Equal((byte)MapFeature.TraitPedestal + 1, (byte)MapFeature.InactiveTraitPedestal);
        Assert.Equal((byte)MapFeature.FeatPedestal + 1, (byte)MapFeature.InactiveFeatPedestal);
        Assert.Equal((byte)MapFeature.ArmorStand + 1, (byte)MapFeature.InactiveArmorStand);
        Assert.Equal((byte)MapFeature.ItemCase + 1, (byte)MapFeature.InactiveItemCase);
        Assert.Equal((byte)MapFeature.WeaponStand + 1, (byte)MapFeature.InactiveWeaponStand);
    }

    [Theory]
    [InlineData(MapFeature.DeityAltar, MapFeature.InactiveDeityAltar)]
    [InlineData(MapFeature.MajorShrine, MapFeature.InactiveMajorShrine)]
    [InlineData(MapFeature.MinorShrine, MapFeature.InactiveMinorShrine)]
    [InlineData(MapFeature.RitualShrine, MapFeature.InactiveRitualShrine)]
    [InlineData(MapFeature.SmallFountain, MapFeature.InactiveSmallFountain)]
    [InlineData(MapFeature.BigFountain, MapFeature.InactiveBigFountain)]
    [InlineData(MapFeature.SkillPedestal, MapFeature.InactiveSkillPedestal)]
    [InlineData(MapFeature.RacePedestal, MapFeature.InactiveRacePedestal)]
    [InlineData(MapFeature.TraitPedestal, MapFeature.InactiveTraitPedestal)]
    [InlineData(MapFeature.FeatPedestal, MapFeature.InactiveFeatPedestal)]
    [InlineData(MapFeature.ArmorStand, MapFeature.InactiveArmorStand)]
    [InlineData(MapFeature.ItemCase, MapFeature.InactiveItemCase)]
    [InlineData(MapFeature.WeaponStand, MapFeature.InactiveWeaponStand)]
    public void DeactivateFeature_uses_the_matching_inactive_terrain(
        MapFeature active, MapFeature inactive)
    {
        var room = TestHelper.BuildRoom(".");
        var point = new Point(0, 0);
        TerrainSystem.SetTerrain(active, point, room);

        TerrainSystem.DeactivateFeature(active, point, room);

        Assert.Equal(inactive, room.Terrain[room.PointToIndex![0, 0]]);
    }
}

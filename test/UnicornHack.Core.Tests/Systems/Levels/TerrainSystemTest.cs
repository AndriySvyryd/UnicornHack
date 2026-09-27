namespace UnicornHack.Systems.Levels;

public class TerrainSystemTest
{
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
    [InlineData(MapFeature.GildedArmorStand, MapFeature.InactiveGildedArmorStand)]
    [InlineData(MapFeature.ItemCase, MapFeature.InactiveItemCase)]
    [InlineData(MapFeature.GildedItemCase, MapFeature.InactiveGildedItemCase)]
    [InlineData(MapFeature.WeaponStand, MapFeature.InactiveWeaponStand)]
    [InlineData(MapFeature.GildedWeaponStand, MapFeature.InactiveGildedWeaponStand)]
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

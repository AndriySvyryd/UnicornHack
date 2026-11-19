namespace UnicornHack.Systems.Levels;

public class TerrainSystemTest
{
    [Theory]
    [InlineData(RoomFeatures.DeityAltar)]
    [InlineData(RoomFeatures.MajorShrine)]
    [InlineData(RoomFeatures.MinorShrine)]
    [InlineData(RoomFeatures.RitualShrine)]
    [InlineData(RoomFeatures.SmallFountain)]
    [InlineData(RoomFeatures.BigFountain)]
    [InlineData(RoomFeatures.SkillPedestal)]
    [InlineData(RoomFeatures.RacePedestal)]
    [InlineData(RoomFeatures.TraitPedestal)]
    [InlineData(RoomFeatures.FeatPedestal)]
    [InlineData(RoomFeatures.ArmorStand)]
    [InlineData(RoomFeatures.GildedArmorStand)]
    [InlineData(RoomFeatures.ItemCase)]
    [InlineData(RoomFeatures.GildedItemCase)]
    [InlineData(RoomFeatures.WeaponStand)]
    [InlineData(RoomFeatures.GildedWeaponStand)]
    public void Interactive_entities_remain_solid_over_floor_after_destruction(RoomFeatures kind)
    {
        var room = TestHelper.BuildRoom(".");
        var point = new Point(0, 0);
        var feature = FeatureGenerator.Place(room, point, kind);
        Assert.False(room.Entity.Manager.TravelSystem.CanMoveTo(point, room));

        feature.Feature!.Destroyed = true;

        Assert.False(room.Entity.Manager.TravelSystem.CanMoveTo(point, room));
        Assert.Equal(MapFeature.StoneFloor, room.Terrain[room.PointToIndex![0, 0]]);
    }
}

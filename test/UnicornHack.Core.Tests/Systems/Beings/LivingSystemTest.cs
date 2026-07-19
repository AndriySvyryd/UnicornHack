using UnicornHack.Data.Items;

namespace UnicornHack.Systems.Beings;

public class LivingSystemTest
{
    [Fact]
    public void Reserved_hp_clamps_current_health_to_the_usable_ceiling()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        being.HitPoints = being.HitPointMaximum;

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            25, nameof(BeingComponent.ReservedHitPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(25, being.ReservedHitPoints);
        Assert.Equal(being.HitPointMaximum - 25, being.HitPoints);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            0, nameof(BeingComponent.ReservedHitPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, being.ReservedHitPoints);
        Assert.Equal(being.HitPointMaximum - 25, being.HitPoints);
    }

    [Fact]
    public void Hp_and_ep_depend_on_might_and_focus_and_regenerate_with_xp()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        ItemData.PotionOfOgreness.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        Assert.Equal(1, being.EnergyRegeneration);
        Assert.Equal(100, being.EnergyPointMaximum);
        Assert.Equal(100, being.EnergyPoints);
        Assert.Equal(100, being.HitPointMaximum);
        Assert.Equal(100, being.HitPoints);

        being.EnergyPoints -= 50;
        being.HitPoints -= 50;
        manager.XPSystem.AddPlayerXP(25, manager);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(59, being.EnergyPoints);
        Assert.Equal(50, being.HitPoints);

        TestHelper.ActivateAbility(ItemData.PotionOfOgreness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(90, being.EnergyPointMaximum);
        Assert.Equal(53, being.EnergyPoints);
        Assert.Equal(120, being.HitPointMaximum);
        Assert.Equal(60, being.HitPoints);
    }

    [Fact]
    public void Energy_regeneration_increases_ep_restored_from_xp()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = player.Being!;
        being.EnergyPoints = 1;
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            2, nameof(BeingComponent.EnergyRegeneration), player, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, being.EnergyRegeneration);

        manager.XPSystem.AddPlayerXP(6, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(7, being.EnergyPoints);
    }

    [Fact]
    public void Reserved_ep_does_not_replenish()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        Assert.Equal(100, being.EnergyPointMaximum);
        Assert.Equal(100, being.EnergyPoints);
        Assert.Equal(0, being.ReservedEnergyPoints);

        being.EnergyPoints += 50;

        Assert.Equal(100, being.EnergyPoints);

        being.EnergyPoints -= 120;

        Assert.Equal(0, being.EnergyPoints);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            -50, nameof(BeingComponent.ReservedEnergyPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, being.ReservedEnergyPoints);

        being.EnergyPoints = 100;
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            60, nameof(BeingComponent.ReservedEnergyPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(40, being.EnergyPoints);

        being.EnergyPoints = 100;

        Assert.Equal(40, being.EnergyPoints);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            150, nameof(BeingComponent.ReservedEnergyPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            100, nameof(BeingComponent.EnergyPointMaximum), playerEntity, manager);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(150, being.ReservedEnergyPoints);
        Assert.Equal(0, being.EnergyPoints);
        Assert.Equal(200, being.EnergyPointMaximum);

        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            0, nameof(BeingComponent.ReservedEnergyPoints), playerEntity, manager);
        manager.Queue.ProcessQueue(manager);
        being.EnergyPoints = 50;

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(0, being.ReservedEnergyPoints);
        Assert.Equal(50, being.EnergyPoints);
    }
}

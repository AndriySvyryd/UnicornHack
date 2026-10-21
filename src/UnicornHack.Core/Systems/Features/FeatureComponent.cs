using UnicornHack.Systems.Levels;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Features;

[Component(Id = (int)EntityComponent.Feature)]
public class FeatureComponent : GameComponent
{
    public FeatureComponent()
    {
        ComponentId = (int)EntityComponent.Feature;
    }

    public ICollection<Reward> Rewards { get; set; } = new ObservableHashSet<Reward>();

    protected override void Clean()
    {
        Rewards.Clear();
        base.Clean();
    }
}
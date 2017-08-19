using System.Linq;
using UnicornHack.Events;
using UnicornHack.Generation;

namespace UnicornHack.Effects
{
    public class ChangeRace : Effect
    {
        public ChangeRace()
        {
        }

        public ChangeRace(Game game) : base(game)
        {
        }

        public string RaceName { get; set; }
        public bool Remove { get; set; }

        public override Effect Instantiate(Game game) => new ChangeRace(game) {RaceName = RaceName, Remove = Remove};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            var player = abilityContext.Target as Player;
            if (player != null)
            {
                if (Remove)
                {
                    // TODO: Show a dialog
                    player.Game.Random.Pick(player.Races.ToList()).Remove();
                }
                else
                {
                    var race = PlayerRaceDefinition.Loader.Get(RaceName);
                    var existingRace = player.Races.SingleOrDefault(r => r.Species == race.Species);
                    if (existingRace == null)
                    {
                        race.Instantiate(player);
                    }
                    // TODO: Change subrace
                }
            }
        }
    }
}
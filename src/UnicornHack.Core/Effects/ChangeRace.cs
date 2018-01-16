using System.Linq;
using UnicornHack.Abilities;
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

        public override Effect Copy(Game game) => new ChangeRace(game) {RaceName = RaceName, Remove = Remove};

        public override void Apply(AbilityActivationContext abilityContext)
        {
            if (abilityContext.TargetEntity is Player player)
            {
                if (Remove)
                {
                    var race = player.Races.SingleOrDefault(r => r.Name == RaceName);
                    if (race == null)
                    {
                        return;
                    }

                    race.Remove();

                    if (!player.Races.Any())
                    {
                        player.ChangeCurrentHP(-1 * player.HP);
                    }
                }
                else
                {
                    var race = PlayerRaceDefinition.Loader.Get(RaceName);
                    var existingRace = player.Races.SingleOrDefault(r => r.Species == race.Species);
                    if (existingRace == null)
                    {
                        var changedRace = race.Instantiate(abilityContext);
                        changedRace.Add();
                        abilityContext.TargetEntity.Add(changedRace.Ability);

                        changedRace.XPLevel = 1;
                        changedRace.UpdateNextLevelXP();
                    }
                    // TODO: Change subrace
                }
            }
        }
    }
}
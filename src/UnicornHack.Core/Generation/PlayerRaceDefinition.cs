using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Players;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class PlayerRaceDefinition : ILoadable
    {
        public virtual string Name { get; set; }

        public virtual Species Species { get; set; }
        public virtual SpeciesClass SpeciesClass { get; set; }
        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();

        public PlayerRace Instantiate(Player player)
        {
            var race = new PlayerRace
            {
                Game = player.Game,
                Player = player,
                Id = player.NextRaceId++,
                Name = Name,
                Species = Species,
                SpeciesClass = SpeciesClass
            };

            foreach (var ability in Abilities)
            {
                race.Abilities.Add(ability.Instantiate(player.Game).AddReference().Referenced);
            }

            player.Add(race);

            return race;
        }

        public static readonly CSScriptLoader<PlayerRaceDefinition> Loader =
            new CSScriptLoader<PlayerRaceDefinition>(@"Data\Players\", typeof(PlayerRaceData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<PlayerRaceDefinition>(GetPropertyConditions<PlayerRaceDefinition>());

        protected static Dictionary<string, Func<TPlayerRace, object, bool>> GetPropertyConditions<TPlayerRace>()
            where TPlayerRace : PlayerRaceDefinition => new Dictionary<string, Func<TPlayerRace, object, bool>>
        {
            {nameof(Name), (o, v) => v != null},
            {nameof(Species), (o, v) => (Species)v != Species.Default},
            {nameof(SpeciesClass), (o, v) => (SpeciesClass)v != SpeciesClass.None},
            {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0}
        };

        public virtual ICSScriptSerializer GetSerializer() => Serializer;
    }
}
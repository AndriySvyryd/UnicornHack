using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Abilities;
using UnicornHack.Data.Players;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public class PlayerRaceDefinition : ILoadable
    {
        public virtual string Name { get; set; }

        public virtual Species Species { get; set; }
        public virtual SpeciesClass SpeciesClass { get; set; }
        public virtual ISet<AbilityDefinition> Abilities { get; set; } = new HashSet<AbilityDefinition>();

        public ChangedRace Instantiate(AbilityActivationContext abilityContext)
        {
            var race = new ChangedRace(abilityContext)
            {
                Name = Name,
                Species = Species,
                SpeciesClass = SpeciesClass,
                Ability = new Ability(abilityContext.TargetEntity.Game)
                {
                    Name = Name,
                    Activation = AbilityActivation.Always
                }
            };

            foreach (var abilityDefinition in Abilities)
            {
                race.Ability.Effects.Add(
                    new AddAbility(abilityContext.TargetEntity.Game)
                    {
                        Ability = abilityDefinition.Copy(abilityContext.TargetEntity.Game)
                    });
            }

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
            {nameof(Abilities), (o, v) => ((ICollection<AbilityDefinition>)v).Count != 0}
        };

        public virtual ICSScriptSerializer GetSerializer() => Serializer;
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicornHack.Models.GameDefinitions
{
    public abstract class ActorVariant
    {
        // TODO: pregenerate the indices
        private static readonly Dictionary<string, ActorVariant> NameLookup =
            new Dictionary<string, ActorVariant>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<Species, List<ActorVariant>> SpeciesLookup =
            new Dictionary<Species, List<ActorVariant>>();

        private static readonly Dictionary<SpeciesClass, List<ActorVariant>> SpeciesCategoryLookup
            = new Dictionary<SpeciesClass, List<ActorVariant>>();

        protected ActorVariant(
            string name,
            Species species,
            IReadOnlyList<ActorProperty> innateProperties,
            byte movementRate,
            Size size,
            short weight,
            short nutrition,
            IReadOnlyList<Tuple<ActorProperty, Frequency>> consumptionProperties = null,
            SpeciesClass speciesClass = SpeciesClass.None)
        {
            Name = name;
            Species = species;
            SpeciesClass = speciesClass;
            InnateProperties = innateProperties;
            MovementRate = movementRate;
            Size = size;
            Weight = weight;
            Nutrition = nutrition;
            ConsumptionProperties = consumptionProperties ?? new Tuple<ActorProperty, Frequency>[0];

            Debug.Assert(!NameLookup.ContainsKey(name));
            NameLookup[name] = this;

            List<ActorVariant> actorTypes;
            if (!SpeciesLookup.TryGetValue(species, out actorTypes))
            {
                actorTypes = new List<ActorVariant>();
                SpeciesLookup[species] = actorTypes;
            }
            actorTypes.Add(this);

            if (!SpeciesCategoryLookup.TryGetValue(speciesClass, out actorTypes))
            {
                actorTypes = new List<ActorVariant>();
                SpeciesCategoryLookup[speciesClass] = actorTypes;
            }
            actorTypes.Add(this);
        }

        public Species Species { get; }
        public SpeciesClass SpeciesClass { get; }
        public string Name { get; }

        public byte MovementRate { get; }
        public Size Size { get; }

        /// <summary> 100g units </summary>
        public short Weight { get; }

        public short Nutrition { get; }
        public IReadOnlyList<Tuple<ActorProperty, Frequency>> ConsumptionProperties { get; }

        public IReadOnlyList<ActorProperty> InnateProperties { get; }

        protected static List<ActorProperty> Has(params SimpleActorPropertyType[] propertyTypes)
        {
            Debug.Assert(propertyTypes.Length > 0);

            var actorProperties = new List<ActorProperty>();
            foreach (var simpleActorPropertyType in propertyTypes)
            {
                actorProperties.Add(ActorProperty.Add(simpleActorPropertyType));
            }

            return actorProperties;
        }

        public static ActorVariant Get(string name)
        {
            return NameLookup[name];
        }

        public static IEnumerable<ActorVariant> Get(Species species)
        {
            return SpeciesLookup[species];
        }

        public static IEnumerable<ActorVariant> Get(SpeciesClass @class)
        {
            return SpeciesCategoryLookup[@class];
        }
    }
}
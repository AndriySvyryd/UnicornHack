using System.Collections.Generic;

namespace UnicornHack.Models.GameDefinitions
{
    public abstract class ActorVariant
    {
        public virtual string Name { get; set; }
        public virtual Species Species { get; set; }
        public virtual SpeciesClass SpeciesClass { get; set; }

        public virtual byte MovementRate { get; set; }
        public virtual Size Size { get; set; }

        /// <summary> 100g units </summary>
        public virtual short Weight { get; set; }

        public virtual short Nutrition { get; set; }

        // Material

        public virtual ISet<string> SimpleProperties { get; set; }
        public virtual IDictionary<string, object> ValuedProperties { get; set; }

        public virtual IList<Ability> Abilities { get; set; }

        protected virtual void Initialize()
        {
            Abilities = Abilities ?? new List<Ability>();
            SimpleProperties = SimpleProperties ?? new HashSet<string>();
            ValuedProperties = ValuedProperties ?? new Dictionary<string, object>();
        }
    }
}
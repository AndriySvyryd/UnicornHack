using System.Collections.Generic;

namespace UnicornHack
{
    public class ActivePlayerRace
    {
        public virtual int Id { get; set; }
        public virtual int GameId { get; private set; }
        public virtual int PlayerId { get; private set; }
        public virtual string Name { get; set; }
        public virtual Species Species { get; set; }
        public virtual SpeciesClass SpeciesClass { get; set; }

        public virtual byte XPLevel { get; set; }
        public virtual int XP { get; set; }
        public virtual int NextLevelXP { get; set; }

        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public virtual Game Game { get; set; }
        public virtual Player Player { get; set; }
    }
}

using System.Collections.Generic;

namespace UnicornHack.Systems.Abilities
{
    public class AttackStats
    {
        public int Delay { get; set; }
        public List<int> HitProbabilities { get; set; } = new List<int>();
        public List<int> Damages { get; set; } = new List<int>();
    }
}

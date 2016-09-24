using UnicornHack.Models.GameDefinitions.Effects;

namespace UnicornHack.Models.GameDefinitions
{
    public class Ability
    {
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        public virtual int ActionPointCost { get; set; }
        public virtual int EnergyPointCost { get; set; }
        // Targetting mode
        public virtual AbilityEffect[] Effects { get; set; }
    }
}
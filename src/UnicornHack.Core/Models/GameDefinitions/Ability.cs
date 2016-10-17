using UnicornHack.Models.GameDefinitions.Effects;

namespace UnicornHack.Models.GameDefinitions
{
    public class Ability
    {
        public virtual AbilityActivation Activation { get; set; }
        public virtual AbilityAction Action { get; set; }
        public virtual int Timeout { get; set; }
        // If more than one turn can be interrupted
        public virtual int ActionPointCost { get; set; }
        public virtual int EnergyPointCost { get; set; }
        // Targetting mode
        // Success condition
        public virtual AbilityEffect[] Effects { get; set; }
    }
}
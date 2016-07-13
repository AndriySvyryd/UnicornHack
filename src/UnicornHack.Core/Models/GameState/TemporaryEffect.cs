using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    // From eating, drinking, spell, etc.
    public abstract class TemporaryEffect
    {
        public ActorProperty PropertyAffected { get; protected set; }
        public int TurnsLeft { get; set; }
    }
}
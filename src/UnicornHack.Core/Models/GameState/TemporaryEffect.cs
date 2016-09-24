namespace UnicornHack.Models.GameState
{
    // From eating, drinking, spell, etc.
    public abstract class TemporaryEffect
    {
        public string PropertyAffected { get; protected set; }
        public int TurnsLeft { get; set; }
    }
}
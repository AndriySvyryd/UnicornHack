namespace UnicornHack.Effects
{
    public class ScriptedEffect : Effect
    {
        public ScriptedEffect()
        {
        }

        public ScriptedEffect(Game game)
            : base(game)
        {
        }

        public string Script { get; set; }

        public override Effect Instantiate(Game game)
            => new ScriptedEffect(game) {Script = Script};
    }
}
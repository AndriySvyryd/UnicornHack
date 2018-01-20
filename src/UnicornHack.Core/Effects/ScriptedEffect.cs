using UnicornHack.Abilities;

namespace UnicornHack.Effects
{
    public class ScriptedEffect : Effect
    {
        public ScriptedEffect()
        {
        }

        public ScriptedEffect(Game game) : base(game)
        {
        }

        public ScriptedEffect(ScriptedEffect effect, Game game)
            : base(effect, game)
            => Script = effect.Script;

        public string Script { get; set; }

        public override Effect Copy(Game game) => new ScriptedEffect(this, game);

        public override void Apply(AbilityActivationContext abilityContext)
        {
        }
    }
}
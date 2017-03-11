using System;
using UnicornHack.Events;

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

        public override void Apply(AbilityActivationContext abilityContext)
        {
            throw new NotImplementedException();
        }
    }
}
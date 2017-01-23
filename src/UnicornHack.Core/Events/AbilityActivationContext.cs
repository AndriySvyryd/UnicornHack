namespace UnicornHack.Events
{
    public class AbilityActivationContext
    {
        public virtual Actor Activator { get; set; }
        public virtual Actor Target { get; set; }
        public virtual Ability Ability { get; set; }
        public virtual AbilityActivation AbilityTrigger { get; set; }
        public virtual bool Succeeded { get; set; }
    }
}
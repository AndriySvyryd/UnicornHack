namespace UnicornHack
{
    public abstract class Property<T> : Property
    {
        protected Property()
        {
        }

        protected Property(Entity entity, string name) : base(entity, name)
            => LastValue = ((PropertyDescription<T>)PropertyDescription.Loader.Get(Name)).DefaultValue;

        public override object Value => CurrentValue;

        public T LastValue { get; set; }

        public abstract T CurrentValue { get; set; }
    }
}

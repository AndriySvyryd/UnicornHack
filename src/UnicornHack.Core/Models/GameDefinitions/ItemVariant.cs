namespace UnicornHack.Models.GameDefinitions
{
    public class ItemVariant
    {
        public virtual string Name { get; set; }

        /// <summary> 100g units </summary>
        public virtual short Weight { get; set; }

        // Material

        public virtual bool Nameable { get; set; }
        public virtual int StackSize { get; set; } = 1;
    }
}
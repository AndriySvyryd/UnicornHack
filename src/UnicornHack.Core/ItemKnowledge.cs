namespace UnicornHack
{
    public class ItemKnowledge : EntityKnowledge
    {
        public ItemKnowledge()
        {
        }

        public ItemKnowledge(Item item) : base(item.Game) => Item = item.AddReference().Referenced;

        public virtual Item Item { get; set; }

        public override void Delete()
        {
            base.Delete();
            if (Item != null)
            {
                Item.PlayerKnowledge = null;
                Item.RemoveReference();
            }
            Item = null;
        }
    }
}

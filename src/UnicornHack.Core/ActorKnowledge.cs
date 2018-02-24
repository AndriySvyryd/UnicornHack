namespace UnicornHack
{
    public class ActorKnowledge : EntityKnowledge
    {
        public ActorKnowledge()
        {
        }

        public ActorKnowledge(Actor actor) : base(actor.Game) => Actor = actor.AddReference().Referenced;

        public virtual Direction Heading { get; set; }

        public virtual Actor Actor { get; set; }

        public override void Delete()
        {
            base.Delete();
            if (Actor != null)
            {
                Actor.PlayerKnowledge = null;
                Actor.RemoveReference();
            }
            Actor = null;
        }
    }
}
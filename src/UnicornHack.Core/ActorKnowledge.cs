namespace UnicornHack
{
    public class ActorKnowledge : EntityKnowledge
    {
        public ActorKnowledge()
        {
        }

        public ActorKnowledge(Game game) : base(game) => Id = ++game.NextEntityId;

        public ActorKnowledge(Actor actor) : base(actor.Game)
        {
            Id = actor.Id;
            Actor = actor.AddReference().Referenced;
        }

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
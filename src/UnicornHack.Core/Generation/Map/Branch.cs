namespace UnicornHack.Generation.Map
{
    public class Branch
    {
        public virtual string Name { get; set; }
        public virtual int Length { get; set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        // TODO: Fragment, item and creature generation weight and distribution modifiers
        // TODO: default terrain type for floor/wall/empty space
    }
}
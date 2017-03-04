namespace UnicornHack.Generation.Map
{
    public class ConnectingMapFragment : MapFragment
    {
        public virtual string ConnectedBranchName { get; private set; }
        public virtual ConnectionDirection Direction { get; set; }
    }
}

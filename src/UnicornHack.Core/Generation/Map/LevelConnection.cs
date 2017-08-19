using System;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation.Map
{
    public class LevelConnection : ICSScriptSerializable
    {
        public virtual string BranchName { get; set; }
        public virtual byte? Depth { get; set; }
        public virtual char? Glyph { get; set; }
        public virtual ConnectionDirection Direction { get; set; } = ConnectionDirection.Both;

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<LevelConnection>(
                new Dictionary<string, Func<LevelConnection, object, bool>>
                {
                    {nameof(Direction), (o, v) => (ConnectionDirection)v != ConnectionDirection.Both}
                });

        public ICSScriptSerializer GetSerializer() => Serializer;
    }
}
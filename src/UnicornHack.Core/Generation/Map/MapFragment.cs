using System;
using System.Collections.Generic;
using System.IO;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack.Generation.Map
{
    public class MapFragment : ICSScriptSerializable
    {
        public virtual string Name { get; set; }
        public virtual int Id { get; set; }
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual MapFragmentType Type { get; set; }
        public virtual string Map { get; set; }
        public virtual byte[] ByteMap { get; set; }
        public virtual byte Width { get; set; }
        public virtual byte Height { get; set; }
        public virtual Rectangle PayloadArea { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        public virtual ICollection<string> Tags { get; set; }
        // TODO: properties: NoMirror (for reflection symmetry, only 3 turns),
        // NoTurnOver(for rotational symmetry, only mirror and half-turn),
        // NoTransform(implies previous two), NoOverwrite

        public virtual bool ConditionalFirstRow { get; set; }
        public virtual bool ConditionalLastRow { get; set; }
        public virtual bool ConditionalFirstColumn { get; set; }
        public virtual bool ConditionalLastColumn { get; set; }

        #region Creation

        // Characters that can be used as conditions for neighbors:
        // ~ - marks the edge row and/or column as conditional
        // X - should be outside the level
        // # - wall or other unpassable terrain
        // . - floor or other passable terrain
        private void Initialize()
        {
            byte x = 0, y = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Map.Length; i++)
            {
                switch (Map[i])
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                        if (x > Width)
                        {
                            Width = x;
                        }
                        x = 0;
                        y++;
                        continue;
                    default:
                        x++;
                        break;
                }
            }

            if (x > 0)
            {
                if (x > Width)
                {
                    Width = x;
                }
                y++;
            }

            Height = y;

            if (Height == 0
                || Width == 0)
            {
                throw new InvalidOperationException($"{Name}: Empty map");
            }

            x = 0;
            y = 0;
            ByteMap = new byte[Width*Height];
            for (var i = 0; i < Map.Length; i++)
            {
                var character = Map[i];
                switch (character)
                {
                    case '\r':
                        continue;
                    case '\n':
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }
                        while (x != Width)
                        {
                            ByteMap[x + Width*y] = (byte)' ';
                            x++;
                        }
                        x = 0;
                        y++;
                        continue;
                    case '~':
                        if (x == 0)
                        {
                            ConditionalFirstColumn = true;
                        }
                        else if (x == Width - 1)
                        {
                            ConditionalLastColumn = true;
                        }
                        else if (y != 0 && y != Height - 1)
                        {
                            throw new InvalidOperationException(
                                $"{Name}: '~' can only be either the first or the last character in a row if not in the first or last row.");
                        }

                        if (y == 0)
                        {
                            ConditionalFirstRow = true;
                        }
                        else if (y == Height - 1)
                        {
                            ConditionalLastRow = true;
                        }

                        ByteMap[x + Width*y] = (byte)character;
                        x++;
                        break;
                    default:
                        ByteMap[x + Width*y] = (byte)character;
                        x++;
                        break;
                }
            }

            if (x > 0)
            {
                while (x != Width)
                {
                    ByteMap[x + Width*y] = (byte)' ';
                    x++;
                }
            }

            var payloadOrigin = new Point((byte)(ConditionalFirstColumn ? 1 : 0), (byte)(ConditionalFirstRow ? 1 : 0));
            PayloadArea = new Rectangle(
                payloadOrigin,
                (byte)(Width - payloadOrigin.X - (ConditionalLastColumn ? 1 : 0)),
                (byte)(Height - payloadOrigin.Y - (ConditionalLastRow ? 1 : 0)));

            if (PayloadArea.Height == 0
                || PayloadArea.Width == 0)
            {
                throw new InvalidOperationException($"{Name}: The payload area is empty");
            }
        }

        #endregion

        #region Serialization

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\fragments\");
        private static bool _allLoaded;

        public static Dictionary<string, MapFragment> NameLookup { get; } =
            new Dictionary<string, MapFragment>(StringComparer.Ordinal);

        public static IEnumerable<MapFragment> GetAllMapFragmentVariants()
        {
            if (!_allLoaded)
            {
                foreach (var file in
                    Directory.EnumerateFiles(BasePath, "*" + CSScriptDeserializer.Extension,
                        SearchOption.AllDirectories))
                {
                    if (!NameLookup.ContainsKey(
                        CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                    {
                        Load(file);
                    }
                }
                _allLoaded = true;
            }

            return NameLookup.Values;
        }

        public static MapFragment Get(string name)
        {
            MapFragment variant;
            if (NameLookup.TryGetValue(name, out variant))
            {
                return variant;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            if (!File.Exists(path))
            {
                return null;
            }

            return Load(path);
        }

        private static MapFragment Load(string path)
        {
            var variant = CSScriptDeserializer.LoadFile<MapFragment>(path);
            variant.Initialize();
            NameLookup[variant.Name] = variant;
            return variant;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<MapFragment>(GetPropertyConditions<MapFragment>());

        protected static Dictionary<string, Func<TMapFragment, object, bool>> GetPropertyConditions<TMapFragment>()
            where TMapFragment : MapFragment
        {
            return new Dictionary<string, Func<TMapFragment, object, bool>>
            {
                {nameof(Name), (o, v) => v != null},
                {nameof(Type), (o, v) => (MapFragmentType)v != MapFragmentType.Normal},
                {nameof(GenerationWeight), (o, v) => (Weight)v != null},
                {nameof(ByteMap), (o, v) => false},
                {nameof(PayloadArea), (o, v) => false},
                {nameof(Width), (o, v) => false},
                {nameof(Height), (o, v) => false},
                {nameof(ConditionalFirstColumn), (o, v) => false},
                {nameof(ConditionalFirstRow), (o, v) => false},
                {nameof(ConditionalLastColumn), (o, v) => false},
                {nameof(ConditionalLastRow), (o, v) => false}
            };
        }

        public virtual ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}
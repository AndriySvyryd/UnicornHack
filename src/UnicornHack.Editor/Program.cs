using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpScriptSerialization;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack.Editor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerializeCreatures(verify: true);
            SerializePlayers(verify: true);
            SerializeItems(verify: true);
            SerializeBranches(verify: true);
            SerializeNormalFragments(verify: true);
            SerializeConnectingFragments(verify: true);
            SerializeDefiningFragments(verify: true);
        }

        private static void SerializePlayers(bool verify = false)
        {
            Console.WriteLine("Serializing players...");

            Directory.CreateDirectory(PlayerDirectory);

            foreach (var playerVariant in Player.Loader.GetAll())
            {
                var script = CSScriptSerializer.Serialize(playerVariant);

                File.WriteAllText(
                    Path.Combine(PlayerDirectory, CSScriptDeserializer.GetFilename(playerVariant.Name)),
                    script);

                if (verify)
                {
                    Verify(script, playerVariant);
                }
            }
        }

        private static void SerializeCreatures(bool verify = false)
        {
            Console.WriteLine("Serializing creatures...");

            Directory.CreateDirectory(CreatureDirectory);

            foreach (var creatureVariant in Creature.Loader.GetAll())
            {
                var script = CSScriptSerializer.Serialize(creatureVariant);

                File.WriteAllText(
                    Path.Combine(CreatureDirectory, CSScriptDeserializer.GetFilename(creatureVariant.Name)),
                    script);

                if (verify)
                {
                    Verify(script, creatureVariant);
                }
            }
        }

        private static void SerializeItems(bool verify = false)
        {
            Console.WriteLine("Serializing items...");

            Directory.CreateDirectory(ItemDirectory);

            foreach (var item in Item.GetAllItemVariants())
            {
                var script = CSScriptSerializer.Serialize(item);

                File.WriteAllText(
                    Path.Combine(ItemDirectory, CSScriptDeserializer.GetFilename(item.Name)),
                    script);

                if (verify)
                {
                    Verify(script, item);
                }
            }
        }

        private static void SerializeBranches(bool verify = false)
        {
            Console.WriteLine("Serializing branches...");

            Directory.CreateDirectory(BranchDirectory);

            foreach (var branch in Branch.GetAllBranches())
            {
                var script = CSScriptSerializer.Serialize(branch);

                File.WriteAllText(
                    Path.Combine(BranchDirectory, CSScriptDeserializer.GetFilename(branch.Name)),
                    script);

                if (verify)
                {
                    Verify(script, branch);
                }
            }
        }

        private static void SerializeNormalFragments(bool verify = false)
        {
            Console.WriteLine("Serializing normal fragments...");

            Directory.CreateDirectory(MapFragmentDirectory);

            foreach (var fragment in MapFragment.GetAllNormalMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(MapFragmentDirectory, CSScriptDeserializer.GetFilename(fragment.Name)),
                    script);

                if (verify)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static void SerializeConnectingFragments(bool verify = false)
        {
            Console.WriteLine("Serializing connecting fragments...");

            Directory.CreateDirectory(ConnectingMapFragmentDirectory);

            foreach (var fragment in ConnectingMapFragment.GetAllConnectingMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(ConnectingMapFragmentDirectory, CSScriptDeserializer.GetFilename(fragment.Name)),
                    script);

                if (verify)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static void SerializeDefiningFragments(bool verify = false)
        {
            Console.WriteLine("Serializing defining fragments...");

            Directory.CreateDirectory(DefiningMapFragmentDirectory);

            foreach (var fragment in DefiningMapFragment.GetAllDefiningMapFragments())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(
                    Path.Combine(DefiningMapFragmentDirectory, CSScriptDeserializer.GetFilename(fragment.Name)),
                    script);

                if (verify)
                {
                    Verify(script, fragment);
                }
            }
        }

        private static void Verify(string script, Creature creature)
            => Verify<Creature>(script, c => c.Name == creature.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, Player player)
            => Verify<Player>(script, c => c.Name == player.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, Item item)
            => Verify<Item>(script, c => c.Name == item.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, Branch branch)
            => Verify<Branch>(script, f => f.Name == branch.Name,
                null, null);

        private static void Verify(string script, MapFragment fragment)
            => Verify<MapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment),
                null, null);

        private static void Verify(string script, ConnectingMapFragment fragment)
            => Verify<ConnectingMapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment),
                null, null);

        private static void Verify(string script, DefiningMapFragment fragment)
            => Verify<DefiningMapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment),
                null, null);

        private static bool VerifyNoUnicode(MapFragment fragment)
        {
            int x = 0, y = 0;
            for (var i = 0; i < fragment.Map.Length; i++)
            {
                var character = fragment.Map[i];
                switch (character)
                {
                    case '\r':
                        continue;
                    case '\n':
                        x = 0;
                        y++;
                        continue;
                }

                if (character != (byte)character)
                {
                    throw new InvalidOperationException($"Invalid character '{character}' at {x},{y}");
                }
                x++;
            }

            return true;
        }

        private static void Verify<T>(string script, Func<T, bool> isValid, Func<T, ISet<string>> getSimpleProperties,
            Func<T, IDictionary<string, object>> getValuedProperties)
        {
            try
            {
                var serializedVariant = CSScriptDeserializer.Load<T>(script);
                if (!isValid(serializedVariant))
                {
                    Console.WriteLine(script);
                }

                var simpleProperties = getSimpleProperties?.Invoke(serializedVariant);
                if (simpleProperties != null)
                {
                    foreach (var simpleProperty in simpleProperties)
                    {
                        if (!CustomProperties.TryGetValue(simpleProperty, out CustomPropertyDescription description))
                        {
                            throw new InvalidOperationException(
                                "Invalid simple property: " + simpleProperty);
                        }
                        if (description.PropertyType != typeof(bool))
                        {
                            throw new InvalidOperationException(
                                $"Simple property {simpleProperty} should be of type {description.PropertyType}");
                        }
                    }
                }

                var valuedProperties = getValuedProperties?.Invoke(serializedVariant);
                if (valuedProperties != null)
                {
                    foreach (var valuedProperty in valuedProperties)
                    {
                        if (!CustomProperties.TryGetValue(valuedProperty.Key,
                            out CustomPropertyDescription description))
                        {
                            throw new InvalidOperationException("Invalid valued property: " + valuedProperty);
                        }
                        if (description.PropertyType != valuedProperty.Value.GetType())
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be of type {description.PropertyType}");
                        }

                        if (((IComparable)description.MinValue)?.CompareTo(valuedProperty.Value) > 0)
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be lesser or equal to " +
                                description.MinValue);
                        }
                        if (((IComparable)description.MaxValue)?.CompareTo(valuedProperty.Value) < 0)
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be greater or equal to " +
                                description.MaxValue);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

        private static readonly Dictionary<string, CustomPropertyDescription> CustomProperties = GetCustomProperties();

        private static Dictionary<string, CustomPropertyDescription> GetCustomProperties()
            => typeof(CustomPropertyDescription).GetProperties(
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static)
                .Where(p => !p.CanWrite)
                .ToDictionary(
                    p => p.Name,
                    p => (CustomPropertyDescription)p.GetGetMethod().Invoke(null, null));

        public static readonly string BaseDirectory =
            GetCommonPrefix(new[]
            {
                Player.Loader.BasePath,
                Creature.Loader.BasePath,
                Item.BasePath,
                Branch.Loader.BasePath,
                MapFragment.NormalLoader.BasePath,
                DefiningMapFragment.DefiningLoader.BasePath
            });

        public static readonly string PlayerDirectory =
            Path.Combine(BaseDirectory, "new",
                Player.Loader.BasePath.Substring(BaseDirectory.Length,
                    Player.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string CreatureDirectory =
            Path.Combine(BaseDirectory, "new",
                Creature.Loader.BasePath.Substring(BaseDirectory.Length,
                    Creature.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string ItemDirectory =
            Path.Combine(BaseDirectory, "new",
                Item.BasePath.Substring(BaseDirectory.Length, Item.BasePath.Length - BaseDirectory.Length));

        public static readonly string BranchDirectory =
            Path.Combine(BaseDirectory, "new",
                Branch.Loader.BasePath.Substring(BaseDirectory.Length,
                    Branch.Loader.BasePath.Length - BaseDirectory.Length));

        public static readonly string MapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                MapFragment.NormalLoader.BasePath.Substring(BaseDirectory.Length,
                    MapFragment.NormalLoader.BasePath.Length - BaseDirectory.Length));

        public static readonly string ConnectingMapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                ConnectingMapFragment.ConnectingLoader.BasePath.Substring(BaseDirectory.Length,
                    ConnectingMapFragment.ConnectingLoader.BasePath.Length - BaseDirectory.Length));

        public static readonly string DefiningMapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                DefiningMapFragment.DefiningLoader.BasePath.Substring(BaseDirectory.Length,
                    DefiningMapFragment.DefiningLoader.BasePath.Length - BaseDirectory.Length));

        private static string GetCommonPrefix(IReadOnlyList<string> strings)
        {
            if (strings.Count == 0)
            {
                return null;
            }

            var firstString = strings[0];
            var prefixLength = firstString.Length;

            for (var y = 1; y < strings.Count; y++)
            {
                var s = strings[y];
                for (var i = 0; i < firstString.Length && i < prefixLength; i++)
                {
                    var c = firstString[i];
                    if (i == s.Length
                        || s[i] != c)
                    {
                        prefixLength = i;
                    }
                }
            }

            return firstString.Substring(0, prefixLength);
        }
    }
}
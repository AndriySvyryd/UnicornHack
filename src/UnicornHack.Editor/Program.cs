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
            SerializeFragments(verify: true);
        }

        private static void SerializePlayers(bool verify = false)
        {
            Directory.CreateDirectory(PlayerDirectory);

            foreach (var playerVariant in Player.GetAllPlayerVariants())
            {
                var script = CSScriptSerializer.Serialize(playerVariant);

                File.WriteAllText(GetFilePath(playerVariant), script);

                if (verify)
                {
                    Verify(script, playerVariant);
                }
            }
        }

        private static void SerializeCreatures(bool verify = false)
        {
            Directory.CreateDirectory(CreatureDirectory);

            foreach (var creatureVariant in Creature.GetAllCreatureVariants())
            {
                var script = CSScriptSerializer.Serialize(creatureVariant);

                File.WriteAllText(GetFilePath(creatureVariant), script);

                if (verify)
                {
                    Verify(script, creatureVariant);
                }
            }
        }

        private static void SerializeItems(bool verify = false)
        {
            Directory.CreateDirectory(ItemDirectory);

            foreach (var item in Item.GetAllItemVariants())
            {
                var script = CSScriptSerializer.Serialize(item);

                File.WriteAllText(GetFilePath(item), script);

                if (verify)
                {
                    Verify(script, item);
                }
            }
        }

        private static void SerializeFragments(bool verify = false)
        {
            Directory.CreateDirectory(MapFragmentDirectory);

            foreach (var fragment in MapFragment.GetAllMapFragmentVariants())
            {
                var script = CSScriptSerializer.Serialize(fragment);

                File.WriteAllText(GetFilePath(fragment), script);

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

        private static void Verify(string script, MapFragment fragment)
            => Verify<MapFragment>(script, f => f.Name == fragment.Name && VerifyNoUnicode(fragment),
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
                        CustomPropertyDescription description;
                        if (!CustomProperties.TryGetValue(simpleProperty, out description))
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
                        CustomPropertyDescription description;
                        if (!CustomProperties.TryGetValue(valuedProperty.Key, out description))
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

        private static readonly Dictionary<string, CustomPropertyDescription> CustomProperties =
            GetCustomProperties();

        private static Dictionary<string, CustomPropertyDescription> GetCustomProperties()
            => typeof(CustomPropertyDescription).GetProperties(
                BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static)
                .Where(p => !p.CanWrite)
                .ToDictionary(
                    p => p.Name,
                    p => (CustomPropertyDescription)p.GetGetMethod().Invoke(null, null));

        public static readonly string BaseDirectory =
            GetCommonPrefix(new[] {Player.BasePath, Creature.BasePath, Item.BasePath, MapFragment.BasePath});

        public static readonly string PlayerDirectory =
            Path.Combine(BaseDirectory, "new",
                Player.BasePath.Substring(BaseDirectory.Length, Player.BasePath.Length - BaseDirectory.Length));

        public static readonly string CreatureDirectory =
            Path.Combine(BaseDirectory, "new",
                Creature.BasePath.Substring(BaseDirectory.Length, Creature.BasePath.Length - BaseDirectory.Length));

        public static readonly string ItemDirectory =
            Path.Combine(BaseDirectory, "new",
                Item.BasePath.Substring(BaseDirectory.Length, Item.BasePath.Length - BaseDirectory.Length));

        public static readonly string MapFragmentDirectory =
            Path.Combine(BaseDirectory, "new",
                MapFragment.BasePath.Substring(BaseDirectory.Length, MapFragment.BasePath.Length - BaseDirectory.Length));

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

        private static string GetFilePath(Creature creature)
            => Path.Combine(CreatureDirectory, CSScriptDeserializer.GetFilename(creature.Name));

        private static string GetFilePath(Player player)
            => Path.Combine(PlayerDirectory, CSScriptDeserializer.GetFilename(player.Name));

        private static string GetFilePath(Item item)
            => Path.Combine(ItemDirectory, CSScriptDeserializer.GetFilename(item.Name));

        private static string GetFilePath(MapFragment fragment)
            => Path.Combine(MapFragmentDirectory, CSScriptDeserializer.GetFilename(fragment.Name));
    }
}
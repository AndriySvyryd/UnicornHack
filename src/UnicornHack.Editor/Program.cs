using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpScriptSerialization;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Editor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerializeCreatureVariants(verify: true);
            SerializePlayersVariants(verify: true);
            SerializeItemVariants(verify: true);
        }

        private static void SerializePlayersVariants(bool verify = false)
        {
            foreach (var playerVariant in PlayerVariant.GetAllPlayerVariants())
            {
                playerVariant.Abilities = playerVariant.Abilities.Any() ? playerVariant.Abilities : null;
                playerVariant.SimpleProperties = playerVariant.SimpleProperties.Any()
                    ? playerVariant.SimpleProperties
                    : null;
                playerVariant.ValuedProperties = playerVariant.ValuedProperties.Any()
                    ? playerVariant.ValuedProperties
                    : null;
                playerVariant.SkillAptitudes = playerVariant.SkillAptitudes.Any()
                    ? playerVariant.SkillAptitudes
                    : null;

                var script = CSScriptSerializer.Serialize(playerVariant);

                File.WriteAllText(GetFilePath(playerVariant), script);

                if (verify)
                {
                    Verify(script, playerVariant);
                }
            }
        }

        private static void SerializeCreatureVariants(bool verify = false)
        {
            foreach (var creatureVariant in CreatureVariant.GetAllCreatureVariants())
            {
                creatureVariant.Abilities = creatureVariant.Abilities.Any() ? creatureVariant.Abilities : null;
                creatureVariant.SimpleProperties = creatureVariant.SimpleProperties.Any()
                    ? creatureVariant.SimpleProperties
                    : null;
                creatureVariant.ValuedProperties = creatureVariant.ValuedProperties.Any()
                    ? creatureVariant.ValuedProperties
                    : null;

                var script = CSScriptSerializer.Serialize(creatureVariant);

                File.WriteAllText(GetFilePath(creatureVariant), script);

                if (verify)
                {
                    Verify(script, creatureVariant);
                }
            }
        }

        private static void SerializeItemVariants(bool verify = false)
        {
            foreach (var itemVariant in ItemVariant.GetAllItemVariants())
            {
                itemVariant.Abilities = itemVariant.Abilities.Any() ? itemVariant.Abilities : null;
                itemVariant.SimpleProperties = itemVariant.SimpleProperties.Any()
                    ? itemVariant.SimpleProperties
                    : null;
                itemVariant.ValuedProperties = itemVariant.ValuedProperties.Any()
                    ? itemVariant.ValuedProperties
                    : null;
                itemVariant.EquipableSlots = itemVariant.EquipableSlots.Any()
                    ? itemVariant.EquipableSlots
                    : null;

                var script = CSScriptSerializer.Serialize(itemVariant);

                File.WriteAllText(GetFilePath(itemVariant), script);

                if (verify)
                {
                    Verify(script, itemVariant);
                }
            }
        }

        private static void Verify(string script, CreatureVariant creatureVariant)
            => Verify<CreatureVariant>(script, c => c.Name == creatureVariant.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, PlayerVariant playerVariant)
            => Verify<PlayerVariant>(script, c => c.Name == playerVariant.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static void Verify(string script, ItemVariant itemVariant)
            => Verify<ItemVariant>(script, c => c.Name == itemVariant.Name,
                c => c.SimpleProperties, c => c.ValuedProperties);

        private static readonly Dictionary<string, CustomPropertyDescription> CustomProperties =
            GetCustomProperties();

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

                var simpleProperties = getSimpleProperties(serializedVariant);
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

                var valuedProperties = getValuedProperties(serializedVariant);
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

                        if (((IComparable) description.MinValue)?.CompareTo(valuedProperty.Value) > 0)
                        {
                            throw new InvalidOperationException(
                                $"Valued property {valuedProperty} should be lesser or equal to " + description.MinValue);
                        }
                        if (((IComparable) description.MaxValue)?.CompareTo(valuedProperty.Value) < 0)
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

        private static Dictionary<string, CustomPropertyDescription> GetCustomProperties()
            => typeof(CustomPropertyDescription).GetProperties(
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static)
                .Where(p => !p.CanWrite)
                .ToDictionary(
                    p => p.Name,
                    p => (CustomPropertyDescription) p.GetGetMethod().Invoke(null, null));

        private static string GetFilePath(CreatureVariant creatureVariant)
        {
            var directory = Path.Combine(CreatureVariant.BasePath, "new");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, CSScriptDeserializer.GetFilename(creatureVariant.Name));
        }

        private static string GetFilePath(PlayerVariant playerVariant)
        {
            var directory = Path.Combine(PlayerVariant.BasePath, "new");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, CSScriptDeserializer.GetFilename(playerVariant.Name));
        }

        private static string GetFilePath(ItemVariant itemVariant)
        {
            var directory = Path.Combine(ItemVariant.BasePath, "new");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, CSScriptDeserializer.GetFilename(itemVariant.Name));
        }
    }
}
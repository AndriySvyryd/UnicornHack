using System;
using System.IO;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Editor
{
    public class Tuples
    {
        public Tuple<int?, bool?> First { get; set; }
        public Tuple<int, Tuple<bool?>> Second { get; set; }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            SerializeCreatureVariants();
            SerializePlayersVariants();
            SerializeItemVariants();
        }

        private static void SerializePlayersVariants()
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

                // Verify(script, playerVariant);
            }
        }

        private static void SerializeCreatureVariants()
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

                // Verify(script, creatureVariant);
            }
        }

        private static void SerializeItemVariants()
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

                // Verify(script, itemVariant);
            }
        }

        private static void Verify(string script, CreatureVariant creatureVariant)
        {
            try
            {
                var serializedVariant = CSScriptDeserializer.Load<CreatureVariant>(script);
                if (creatureVariant.Name != serializedVariant.Name)
                {
                    Console.WriteLine(script);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

        private static void Verify(string script, PlayerVariant playerVariant)
        {
            try
            {
                var serializedVariant = CSScriptDeserializer.Load<PlayerVariant>(script);
                if (playerVariant.Name != serializedVariant.Name)
                {
                    Console.WriteLine(script);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

        private static void Verify(string script, ItemVariant itemVariant)
        {
            try
            {
                var serializedVariant = CSScriptDeserializer.Load<ItemVariant>(script);
                if (itemVariant.Name != serializedVariant.Name)
                {
                    Console.WriteLine(script);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

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
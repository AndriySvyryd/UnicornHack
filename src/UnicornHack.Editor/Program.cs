using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            SerializeCreatureVariants();
            //SerializePlayersVariants();
            await Task.FromResult(0);
        }

        private static void SerializePlayersVariants()
        {
            foreach (var playerVariant in PlayerVariant.GetAllPlayerVariants())
            {
                var baseCreature = CreatureVariant.Get(playerVariant.BaseCreatureVariantName);
                if (baseCreature != null)
                {
                    if (playerVariant.MovementRate == baseCreature.MovementRate)
                    {
                        playerVariant.MovementRate = 0;
                    }
                    if (playerVariant.Nutrition == baseCreature.Nutrition)
                    {
                        playerVariant.Nutrition = 0;
                    }
                    if (playerVariant.Weight == baseCreature.Weight)
                    {
                        playerVariant.Weight = 0;
                    }
                    if (playerVariant.Size == baseCreature.Size)
                    {
                        playerVariant.Size = 0;
                    }
                    if (playerVariant.Species == baseCreature.Species)
                    {
                        playerVariant.Species = Species.Default;
                    }
                    if (playerVariant.SpeciesClass == baseCreature.SpeciesClass)
                    {
                        playerVariant.SpeciesClass = SpeciesClass.None;
                    }
                }

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

                //Verify(script, playerVariant);
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
    }
}
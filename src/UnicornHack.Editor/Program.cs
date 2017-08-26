using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpScriptSerialization;
using UnicornHack.Effects;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Utils;

namespace UnicornHack.Editor
{
    public class Program
    {
        private static readonly bool SerializeToScript = false;

        public static void Main(string[] args)
        {
            Serialize(PropertyDescription.Loader);
            Serialize(CreatureVariant.Loader);
            Serialize(PlayerRaceDefinition.Loader);
            Serialize(ItemVariant.Loader);
            Serialize(ItemGroup.Loader);
            Serialize(BranchDefinition.Loader);
            Serialize(NormalMapFragment.Loader);
            Serialize(ConnectingMapFragment.Loader);
            Serialize(DefiningMapFragment.Loader);
        }

        private static void Serialize<T>(CSScriptLoaderBase<T> loader, Func<T, T> transform = null)
            where T : ILoadable
        {
            Console.WriteLine("Serializing " + typeof(T).Name + " instances...");

            var directory = Path.Combine(AppContext.BaseDirectory, "New", loader.RelativePath);
            Directory.CreateDirectory(directory);
            foreach (var item in loader.GetAll())
            {
                try
                {
                    var itemToSerialize = transform != null ? transform(item) : item;
                    string script = null;
                    if (SerializeToScript)
                    {
                        script = CSScriptSerializer.Serialize(itemToSerialize);
                        File.WriteAllText(
                            Path.Combine(
                                loader.RelativePath,
                                CSScriptLoaderBase.GetScriptFilename(itemToSerialize.Name)),
                            script);
                    }
                    else
                    {
                        var code = CSClassSerializer.Serialize(
                            itemToSerialize, itemToSerialize.Name, loader.DataType.Namespace, loader.DataType.Name);
                        File.WriteAllText(
                            Path.Combine(directory, CSScriptLoaderBase.GetClassFilename(itemToSerialize.Name)), code);
                    }
                    Verify(script, (dynamic)item);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to serialize " + item.Name, e);
                }
            }
        }

        private static void Verify(string script, PropertyDescription property)
            => Verify(script, property, i => i.Name == property.Name, null, null, null);

        private static void Verify(string script, CreatureVariant creature)
            => Verify(script, creature, c => c.Name == creature.Name, c => c.SimpleProperties, c => c.ValuedProperties, c => c.Abilities);

        private static void Verify(string script, PlayerRaceDefinition player)
            => Verify(script, player, p => p.Name == player.Name, null, null, c => c.Abilities);

        private static void Verify(string script, ItemVariant item)
            => Verify(script, item, i => i.Name == item.Name, c => c.SimpleProperties, c => c.ValuedProperties, c => c.Abilities);

        private static void Verify(string script, ItemGroup item)
            => Verify(script, item, i => i.Name == item.Name, null, null, null);

        private static void Verify(string script, BranchDefinition branch)
            => Verify(script, branch, b => b.Name == branch.Name, null, null, null);

        private static void Verify(string script, MapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null, null);

        private static void Verify(string script, ConnectingMapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null, null);

        private static void Verify(string script, DefiningMapFragment fragment)
            => Verify(script, fragment, f => f.Name == fragment.Name && VerifyNoUnicode(fragment), null, null, null);

        private static bool VerifyNoUnicode(MapFragment fragment)
        {
            int x = 0, y = 0;
            foreach (var character in fragment.Map)
            {
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

        private static void Verify<T>(string script, T variant, Func<T, bool> isValid,
            Func<T, ISet<string>> getSimpleProperties,
            Func<T, IDictionary<string, object>> getValuedProperties,
            Func<T, ISet<AbilityDefinition>> getAbilities)
        {
            // TODO: Verify abilities
            try
            {
                var serializedVariant = script == null ? variant : CSScriptLoaderBase.Load<T>(script);
                if (!isValid(serializedVariant))
                {
                    Console.WriteLine(script);
                }

                var simpleProperties = getSimpleProperties?.Invoke(serializedVariant);
                if (simpleProperties != null)
                {
                    foreach (var simpleProperty in simpleProperties)
                    {
                        var description = PropertyDescription.Loader.Get(simpleProperty);
                        if (description == null)
                        {
                            throw new InvalidOperationException("Invalid simple property: " + simpleProperty);
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
                        var description = PropertyDescription.Loader.Get(valuedProperty.Key);
                        if (description == null)
                        {
                            throw new InvalidOperationException("Invalid valued property: " + valuedProperty);
                        }
                        if (description.PropertyType == typeof(bool))
                        {
                            throw new InvalidOperationException("Simple property used as valued: " + valuedProperty);
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

                var abilities = getAbilities?.Invoke(serializedVariant);
                if (abilities != null)
                {
                    foreach (var ability in abilities)
                    {
                        Validate(ability);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(script);
                throw;
            }
        }

        private static void Validate(AbilityDefinition ability)
        {
            foreach (var effect in ability.Effects)
            {
                switch (effect)
                {
                    case AddAbility addAbility:
                        Validate(addAbility.Ability);
                        break;
                    case ChangeProperty<int> property:
                        Validate(property);
                        break;
                    case ChangeProperty<bool> property:
                        Validate(property);
                        break;
                    case ChangeProperty<string> property:
                        Validate(property);
                        break;
                    case ChangeProperty<byte> property:
                        Validate(property);
                        break;
                }
            }
        }

        private static void Validate<T>(ChangeProperty<T> property)
        {
            var description = PropertyDescription.Loader.Get(property.PropertyName);
            if (description == null)
            {
                throw new InvalidOperationException("Invalid valued property: " + property.PropertyName);
            }
            if (description.PropertyType != property.Value.GetType()
                || description.PropertyType != typeof(T))
            {
                throw new InvalidOperationException(
                    $"Valued property {property.PropertyName} should be of type {description.PropertyType}");
            }
            if (property.Function != ValueCombinationFunction.Sum
                && property.Function != ValueCombinationFunction.Percent)
            {
                if (((IComparable)description.MinValue)?.CompareTo(property.Value) > 0)
                {
                    throw new InvalidOperationException(
                        $"Valued property {property.PropertyName} should be lesser or equal to " +
                        description.MinValue);
                }
                if (((IComparable)description.MaxValue)?.CompareTo(property.Value) < 0)
                {
                    throw new InvalidOperationException(
                        $"Valued property {property.PropertyName} should be greater or equal to " +
                        description.MaxValue);
                }
            }
        }
    }
}
using System.IO;
using System.Threading.Tasks;
using CSharpScriptSerialization;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Editor;

public static class Program
{
    private static readonly bool SerializeToScript = false;

    private static string _directory = "";
    private static CSClassSerializer? _serializer;

    public static async Task Main()
    {
        _directory = Path.Combine(AppContext.BaseDirectory, "New");
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        Directory.CreateDirectory(_directory);

        if (!SerializeToScript)
        {
            _serializer = new CSClassSerializer();
            await _serializer.InitializeAsync(typeof(Game).Assembly, _directory);
        }

        // Add code in the lambdas below to make bulk changes
        await SerializeAsync(Creature.Loader, c =>
        {
            foreach (var ability in c.Abilities)
            {
                Transform(ability);
            }

            return c;
        });
        await SerializeAsync(PlayerRace.Loader, r =>
        {
            foreach (var ability in r.Abilities)
            {
                Transform(ability);
            }

            return r;
        });
        await SerializeAsync(Item.Loader, i =>
        {
            var activatedAbilityFound = false;
            foreach (var ability in i.Abilities)
            {
                if ((ability.Activation & ActivationType.Slottable) != 0)
                {
                    if (activatedAbilityFound)
                    {
                        throw new InvalidOperationException($"Item {i.Name} has more than 1 activated ability.");
                    }

                    activatedAbilityFound = true;
                }

                Transform(ability);
            }

            return i;
        });
        await SerializeAsync(ItemGroup.Loader);
        await SerializeAsync(Ability.Loader, Transform);
        await SerializeAsync(Branch.Loader);
        await SerializeAsync(NormalMapFragment.Loader);
        await SerializeAsync(ConnectingMapFragment.Loader);
        await SerializeAsync(DefiningMapFragment.Loader);

        Console.WriteLine();
        Console.WriteLine("Serialization completed successfully!");
        Console.WriteLine("Results placed in " + _directory);
    }

    private static Ability Transform(Ability ability)
    {
        if (ability.Effects == null)
        {
            return ability;
        }

        foreach (var effect in ability.Effects)
        {
        }

        if (ability is LeveledAbility leveledAbility)
        {
            foreach (var effects in leveledAbility.LeveledEffects.Values)
            {
                foreach (var effect in effects)
                {
                }
            }
        }

        return ability;
    }

    private static async Task SerializeAsync<T>(CSScriptLoaderBase<T> loader, Func<T, T>? transform = null)
        where T : class, ILoadable
    {
        Console.WriteLine("Serializing " + typeof(T).Name + " instances...");

        var directory = Path.Combine(_directory, loader.RelativePath);
        Directory.CreateDirectory(directory);

        foreach (var item in loader.GetAll())
        {
            try
            {
                var itemToSerialize = transform != null ? transform(item) : item;
                string? script = null;
                if (SerializeToScript)
                {
                    // TODO: use .editorconfig
                    script = CSScriptSerializer.Serialize(itemToSerialize);
                    await File.WriteAllTextAsync(
                        Path.Combine(
                            loader.RelativePath,
                            CSScriptLoaderHelpers.GetScriptFilename(itemToSerialize.Name)),
                        script);
                }
                else
                {
                    var code = await _serializer!.SerializeAsync(
                        itemToSerialize, itemToSerialize.Name, loader.DataType.Namespace!, loader.DataType.Name);
                    await File.WriteAllTextAsync(
                        Path.Combine(directory, CSScriptLoaderHelpers.GetClassFilename(itemToSerialize.Name)),
                        code);
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
        => Verify(script, property, i => i.Name);

    private static void Verify(string script, Creature creature)
        => Verify(script, creature, c => c.Name, getAbilities: c => c.Abilities);

    private static void Verify(string script, PlayerRace player)
        => Verify(script, player, p => p.Name, getAbilities: c => c.Abilities);

    private static void Verify(string script, Item item)
        => Verify(script, item, i => i.Name, i =>
        {
            if (i.Weight == null
                && i.BaseItem == null)
            {
                return "no weight";
            }

            if (i.Material == null
                && i.BaseItem == null)
            {
                return "no material";
            }

            return null;
        }, getAbilities: c => c.Abilities);

    private static void Verify(string script, ItemGroup item)
        => Verify(script, item, i => i.Name);

    // TODO: verify Name not null
    private static void Verify(string script, Ability ability)
        => Verify(script, ability, i => i.Name, a =>
        {
            Validate(a, "itself", ability);
            return null;
        });

    private static void Verify(string script, Branch branch)
        => Verify(script, branch, b => b.Name);

    private static void Verify(string script, MapFragment fragment)
        => Verify(script, fragment, f => f.Name, VerifyNoUnicode);

    private static void Verify(string script, ConnectingMapFragment fragment)
        => Verify(script, fragment, f => f.Name, VerifyNoUnicode);

    private static void Verify(string script, DefiningMapFragment fragment)
        => Verify(script, fragment, f => f.Name, VerifyNoUnicode);

    private static string? VerifyNoUnicode(MapFragment fragment)
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
                return $"Invalid character '{character}' at {x},{y}";
            }

            x++;
        }

        return null;
    }

    private static void Verify<T>(string? script, T variant,
        Func<T, string> getName,
        Func<T, string?>? validate = null,
        Func<T, ISet<Ability>>? getAbilities = null)
    {
        try
        {
            var serializedVariant = script == null ? variant : CSScriptLoaderHelpers.Load<T>(script);
            var name = getName(variant);
            if (name != getName(serializedVariant))
            {
                throw new InvalidOperationException(name + " has invalid name.");
            }

            var validationError = validate?.Invoke(serializedVariant);
            if (name != getName(serializedVariant)
                || validationError != null)
            {
                throw new InvalidOperationException(name + " is not valid: " + validationError);
            }

            var abilities = getAbilities?.Invoke(serializedVariant);
            if (abilities != null)
            {
                foreach (var ability in abilities)
                {
                    Validate(ability, name, variant);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine(script);
            throw;
        }
    }

    private static void Validate<T>(Ability ability, string ownerName, T owner)
    {
        if ((ability.Activation & ActivationType.Slottable) != 0
            && ability.Delay == null && ability.Cooldown == 0 && ability.XPCooldown == 0)
        {
            throw new InvalidOperationException(
                $"Ability {ability.Name} on {ownerName} has no delay or cooldown");
        }

        var onItem = owner is Item;
        var isAttack = ability.SuccessCondition == AbilitySuccessCondition.NormalAttack
                       || ability.SuccessCondition == AbilitySuccessCondition.UnavoidableAttack
                       || ability.SuccessCondition == AbilitySuccessCondition.UnblockableAttack;
        if (isAttack
            && ability.Accuracy == null)
        {
            throw new InvalidOperationException(
                $"Ability {ability.Name} on {ownerName} has no accuracy");
        }

        if (isAttack
            && ability.Action == AbilityAction.Default)
        {
            throw new InvalidOperationException(
                $"Ability {ability.Name} on {ownerName} has no action");
        }

        if (isAttack
            && ability.Trigger == ActivationType.Default
            && (ability.Activation & ActivationType.OnHit) == 0
            && !onItem)
        {
            throw new InvalidOperationException(
                $"Ability {ability.Name} on {ownerName} has no trigger");
        }

        if ((ability.Effects == null
             || ability.Effects.Count == 0)
            && !onItem
            && !(ability is LeveledAbility))
        {
            throw new InvalidOperationException(
                $"Ability {ability.Name} on {ownerName} has no effects");
        }

        if (ability.Delay != null)
        {
            AbilityActivationSystem.CreateDelayFunction(ability.Delay, ability.Name);
        }

        if (ability.Accuracy != null)
        {
            AbilityActivationSystem.CreateAccuracyFunction(ability.Accuracy, ability.Name);
        }

        if (ability.Effects == null)
        {
            return;
        }

        foreach (var effect in ability.Effects)
        {
            switch (effect)
            {
                case AddAbility addAbility:
                    if (addAbility.Ability != null)
                    {
                        Validate(addAbility.Ability, ownerName, owner);
                    }
                    else
                    {
                        Ability.Loader.Get(addAbility.Name);
                    }

                    break;
                case ChangeProperty<int> property:
                    Validate(property);
                    break;
                case ChangeProperty<bool> property:
                    Validate(property);
                    break;
                case ChangeProperty<byte> property:
                    Validate(property);
                    break;
                case PhysicalDamage physicalDamage:
                    if (physicalDamage.ArmorPenetration != null)
                    {
                        EffectApplicationSystem.CreateAmountFunction(physicalDamage.ArmorPenetration, ability.Name);
                    }

                    break;
                case DamageEffect damageEffect:
                    EffectApplicationSystem.CreateAmountFunction(damageEffect.Damage, ability.Name);

                    break;
                case DurationEffect durationEffect:
                    if (durationEffect.DurationAmount != null)
                    {
                        EffectApplicationSystem.CreateAmountFunction(durationEffect.DurationAmount, ability.Name);
                    }

                    break;
                case AmountEffect amountEffect:
                    EffectApplicationSystem.CreateAmountFunction(amountEffect.Amount, ability.Name);

                    break;
            }
        }
    }

    private static void Validate<T>(ChangeProperty<T> property)
        where T : struct, IComparable<T>, IConvertible
    {
        var description = (PropertyDescription<T>?)PropertyDescription.Loader.Find(property.PropertyName);
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
            if (description.MinValue?.CompareTo(property.Value) > 0)
            {
                throw new InvalidOperationException(
                    $"Valued property {property.PropertyName} should be lesser or equal to " +
                    description.MinValue);
            }

            if (description.MaxValue?.CompareTo(property.Value) < 0)
            {
                throw new InvalidOperationException(
                    $"Valued property {property.PropertyName} should be greater or equal to " +
                    description.MaxValue);
            }
        }
    }
}

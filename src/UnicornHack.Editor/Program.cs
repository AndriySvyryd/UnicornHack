using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpScriptSerialization;
using UnicornHack.Services;
using UnicornHack.Services.English;
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
        await SerializeAsync(Creature.Loader);
        await SerializeAsync(PlayerRace.Loader);
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
            }

            return i;
        });
        await SerializeAsync(ItemGroup.Loader);
        await SerializeAsync(Ability.Loader, Transform);
        await SerializeAsync(Branch.Loader);
        await SerializeAsync(NormalMapFragment.Loader, Transform);
        await SerializeAsync(ConnectingMapFragment.Loader, Transform);
        await SerializeAsync(DefiningMapFragment.Loader, Transform);

        Console.WriteLine();
        Console.WriteLine("Serialization completed successfully!");
        Console.WriteLine("Results placed in " + _directory);
    }

    private static Ability? Transform(Ability ability)
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

    private static NormalMapFragment? Transform(NormalMapFragment fragment)
    {
        // Transform variants are materialized from their source at load time
        // (see MapFragmentLoader) and must never be written back as data files;
        // serialize only the source fragments.
        if (fragment.Transform != MapFragmentTransform.Identity)
        {
            return null;
        }

        // Emit only the author-declared tags - the size / content / door markers are
        // re-derived from the payload on every load, so they don't belong in the data.
        fragment.ReduceTagsToAuthored();

        return fragment;
    }

    private static DefiningMapFragment? Transform(DefiningMapFragment fragment)
    {
        // Transform variants are materialized from their source at load time
        // (see MapFragmentLoader) and must never be written back as data files;
        // serialize only the source fragments.
        if (fragment.Transform != MapFragmentTransform.Identity)
        {
            return null;
        }

        // Emit only the author-declared tags - the size / content / door markers are
        // re-derived from the payload on every load, so they don't belong in the data.
        fragment.ReduceTagsToAuthored();

        return fragment;
    }

    private static ConnectingMapFragment? Transform(ConnectingMapFragment fragment)
    {
        // Transform variants are materialized from their source at load time
        // (see MapFragmentLoader) and must never be written back as data files;
        // serialize only the source fragments.
        if (fragment.Transform != MapFragmentTransform.Identity)
        {
            return null;
        }

        // Emit only the author-declared tags - the size / content / door markers are
        // re-derived from the payload on every load, so they don't belong in the data.
        fragment.ReduceTagsToAuthored();

        return fragment;
    }

    private static async Task SerializeAsync<T>(CSScriptLoaderBase<T> loader, Func<T, T?>? transform = null)
        where T : class, ILoadable
    {
        Console.WriteLine("Serializing " + typeof(T).Name + " instances...");

        foreach (var item in loader.GetAll())
        {
            try
            {
                var itemToSerialize = transform != null ? transform(item) : item;
                if (itemToSerialize == null)
                {
                    continue;
                }

                var directory = Path.Combine(
                    _directory, loader.GetOutputRelativePath(itemToSerialize));
                Directory.CreateDirectory(directory);

                string? script = null;
                if (SerializeToScript)
                {
                    // TODO: use .editorconfig
                    script = CSScriptSerializer.Serialize(itemToSerialize);
                    await File.WriteAllTextAsync(
                        Path.Combine(directory,
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

    private static void Verify(string script, Creature creature)
        => Verify(
            script, creature, c => c.Name,
            getAbilities: c => c.Abilities,
            getAbilityReferences: c => c.AbilityReferences);

    private static void Verify(string script, PlayerRace player)
        => Verify(
            script, player, p => p.Name,
            validate: race =>
            {
                foreach (var (level, effects) in race.Effects)
                {
                    if (effects.Count == 0)
                    {
                        continue;
                    }

                    Validate(new Ability
                    {
                        Name = $"{race.Name} race level {level}",
                        Type = AbilityType.Race,
                        Activation = ActivationType.Always,
                        Effects = effects
                    }, race.Name, race);
                }

                if (race.Name == "human")
                {
                    var humanRaceAbility = race.GetRaceAbilityTemplate();
                    if (humanRaceAbility is not LeveledAbility leveledHumanRaceAbility
                        || leveledHumanRaceAbility.LeveledEffects.GetValueOrDefault(3) is not { } levelThreeEffects
                        || levelThreeEffects.OfType<AddAbility>().SingleOrDefault()?.Ability?.Name
                            != "Surge of Luck")
                    {
                        throw new InvalidOperationException(
                            "Human race level 3 must grant Surge of Luck.");
                    }
                }

                return null;
            },
            getAbilityReferences: race => race.AbilityReferences);

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

    private static void Verify(string script, Ability ability)
        => Verify(script, ability, i => i.Name, a =>
        {
            Validate(a, "itself", ability);
            if (a.Type == AbilityType.Skill
                && (a is not LeveledAbility skill
                    || !skill.LeveledEffects.Keys.Order().SequenceEqual([1, 2, 3])))
            {
                throw new InvalidOperationException(
                    $"Skill '{a.Name}' must define exactly levels 1, 2, and 3.");
            }

            if (a.Type.IsStatusLike())
            {
                var language = new EnglishLanguageService();
                var description = language.GetDescription(a.Name, DescriptionCategory.Ability);
                if (string.IsNullOrWhiteSpace(description))
                {
                    throw new InvalidOperationException(
                        $"Status ability '{a.Name}' has no tooltip description.");
                }

                if (a is LeveledAbility leveledAbility)
                {
                    foreach (var level in leveledAbility.LeveledEffects.Keys)
                    {
                        if (description.Contains($"Level {level}: No additional effects"))
                        {
                            throw new InvalidOperationException(
                                $"Status ability '{a.Name}' level {level} has no effect or behavior description.");
                        }
                    }
                }
            }

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
        Func<T, IReadOnlyList<Ability>>? getAbilities = null,
        Func<T, ISet<AbilityReference>>? getAbilityReferences = null)
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
                var abilityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var ability in abilities)
                {
                    Validate(ability, name, variant);
                    if (!abilityNames.Add(ability.Name))
                    {
                        throw new InvalidOperationException(
                            $"{name} defines more than one ability named '{ability.Name}'");
                    }
                }
            }

            var abilityReferences = getAbilityReferences?.Invoke(serializedVariant);
            if (abilityReferences != null)
            {
                foreach (var reference in abilityReferences)
                {
                    if (string.IsNullOrWhiteSpace(reference.Name)
                        || reference.Level <= 0
                        || Ability.Loader.Find(reference.Name) == null)
                    {
                        throw new InvalidOperationException(
                            $"Ability reference {reference.Name} level {reference.Level} on {name} is invalid");
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

    private static void Validate<T>(Ability ability, string ownerName, T owner)
    {
        if (string.IsNullOrWhiteSpace(ability.Name))
        {
            throw new InvalidOperationException($"Ability on {ownerName} has no name");
        }

        if (ability.LevelExpression is { } levelExpression)
        {
            var ownerParameter = Expression.Parameter(typeof(GameEntity), "owner");
            var visitor = new UnicornExpressionVisitor(
                [ownerParameter], ValidateLevelExpressionInvocation);
            visitor.TranslateExpression<Func<GameEntity, int>, int>(
                levelExpression, convertResult: false);
        }

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
            && !(ability is LeveledAbility)
            && ability.Type != AbilityType.Debuff)
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
                        if (Ability.Loader.Find(addAbility.Name) == null
                            && PlayerRace.Loader.FindByIndex(addAbility.Name) == null
                            && Creature.Loader.FindByIndex(addAbility.Name) == null)
                        {
                            throw new InvalidOperationException($"Ability '{addAbility.Name}' not found");
                        }
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
                        EffectApplicationSystem.CreateAmountFunction(
                            physicalDamage.ArmorPenetration, ability.Name, nameof(PhysicalDamage));
                    }

                    break;
            }

            if (effect.Amount != null)
            {
                EffectApplicationSystem.CreateAmountFunction(effect.Amount, ability.Name, effect.GetType().Name);
            }

            if (effect.DurationAmount != null)
            {
                EffectApplicationSystem.CreateAmountFunction(
                    effect.DurationAmount, ability.Name, effect.GetType().Name);
            }
        }
    }

    private static void ValidateLevelExpressionInvocation(
        string function, IReadOnlyList<Expression> arguments)
    {
        if (function != "AbilityLevel")
        {
            return;
        }

        if (arguments is not [ConstantExpression { Value: string abilityName }])
        {
            throw new InvalidOperationException(
                "Function AbilityLevel expects one string-literal argument.");
        }

        if (Ability.Loader.Find(abilityName) == null)
        {
            throw new InvalidOperationException(
                $"AbilityLevel references unknown ability '{abilityName}'.");
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

        if (description.PropertyType != typeof(T))
        {
            throw new InvalidOperationException(
                $"Valued property {property.PropertyName} should be of type {description.PropertyType}");
        }

        var combinationFunction = property.CombinationFunction ?? ValueCombinationFunction.Default;
        if (combinationFunction == ValueCombinationFunction.Default)
        {
            combinationFunction = description.DefaultCombinationFunction;
        }

        if (combinationFunction != ValueCombinationFunction.Sum
            && combinationFunction != ValueCombinationFunction.Percent
            && int.TryParse(property.Amount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
        {
            var value = (T)Convert.ChangeType(amount, typeof(T), CultureInfo.InvariantCulture);
            if (description.MinValue?.CompareTo(value) > 0)
            {
                throw new InvalidOperationException(
                    $"Valued property {property.PropertyName} should be lesser or equal to " +
                    description.MinValue);
            }

            if (description.MaxValue?.CompareTo(value) < 0)
            {
                throw new InvalidOperationException(
                    $"Valued property {property.PropertyName} should be greater or equal to " +
                    description.MaxValue);
            }
        }
    }
}

using UnicornHack.Systems.Effects;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Generation;

public class AbilityLoader
    : GroupedCSScriptLoader<AbilityType, Ability>
{
    public AbilityLoader(string relativePath)
        : base(relativePath, ability => ability.Type, typeof(AbilityData))
    {
    }

    public override string GetOutputRelativePath(Ability ability)
        => GetSubfolder(ability.Type) is { } category
            ? Path.Combine(RelativePath, category)
            : RelativePath;

    protected override void ValidateSourcePath(Ability ability, string sourcePath)
    {
        var actualDirectory = Path.GetRelativePath(
            BasePath, Path.GetDirectoryName(sourcePath)!);
        var expectedDirectory = GetSubfolder(ability.Type) ?? ".";
        if (!string.Equals(
                actualDirectory, expectedDirectory,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Ability '{ability.Name}' of type {ability.Type} must be in "
                + $"'{expectedDirectory}', not '{actualDirectory}'.");
        }
    }

    internal static string? GetSubfolder(AbilityType type)
        => type switch
        {
            AbilityType.Skill => "Skills",
            AbilityType.Buff or AbilityType.Debuff => "Status",
            AbilityType.Boon or AbilityType.Curse => "Boons",
            AbilityType.DefaultMeleeAttack or AbilityType.DefaultRangedAttack
                or AbilityType.MeleeAttack or AbilityType.RangedAttack => "Attacks",
            AbilityType.MeleeSpell or AbilityType.RangedSpell => "Spells",
            AbilityType.Trait or AbilityType.Flaw => "Traits",
            AbilityType.Feat or AbilityType.Fault => "Feats",
            AbilityType.Miracle => "Miracles",
            AbilityType.Faith => "Deities",
            AbilityType.Default or AbilityType.Innate
                or AbilityType.Item or AbilityType.Race => null,
            _ => throw new InvalidOperationException(
                $"No data folder is defined for {nameof(AbilityType)}.{type}.")
        };

    protected override Dictionary<string, Ability> Process(Dictionary<string, Ability> nameLookup)
        => Prepare(nameLookup);

    public static Dictionary<string, Ability> Prepare(Dictionary<string, Ability> abilities)
    {
        Prepare(abilities.Values);
        return abilities;
    }

    public static void Prepare(IReadOnlyCollection<Ability> abilities)
    {
        Dictionary<string, List<Ability>>? dependentsByName = null;
        foreach (var ability in abilities)
        {
            ability.ClearLevelExpressionDependents();
            if (ability.LevelExpression is not { } expression)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new InvalidOperationException(
                    $"Ability '{ability.Name}' has an empty level expression.");
            }

            var prepared = EffectApplicationSystem.CreateLevelExpression(expression, ability.Name);
            ability.SetLevelFunction(prepared.Function, prepared.Dependencies);
            foreach (var dependencyName in prepared.Dependencies)
            {
                dependentsByName ??= new(StringComparer.OrdinalIgnoreCase);
                if (!dependentsByName.TryGetValue(dependencyName, out var dependents))
                {
                    dependents = [];
                    dependentsByName.Add(dependencyName, dependents);
                }

                dependents.Add(ability);
            }
        }

        if (dependentsByName == null)
        {
            return;
        }

        foreach (var ability in abilities)
        {
            if (!dependentsByName.TryGetValue(ability.Name, out var dependents))
            {
                continue;
            }

            foreach (var dependent in dependents)
            {
                if (ReferenceEquals(dependent, ability))
                {
                    throw new InvalidOperationException(
                        $"Ability '{ability.Name}' cannot depend on its own level.");
                }

                ability.AddLevelExpressionDependent(dependent);
            }
        }

        var states = new Dictionary<Ability, byte>();
        foreach (var ability in abilities)
        {
            Visit(ability, states);
        }
    }

    private static void Visit(Ability ability, Dictionary<Ability, byte> states)
    {
        if (states.TryGetValue(ability, out var state))
        {
            if (state == 1)
            {
                throw new InvalidOperationException(
                    $"Ability level expressions contain a cycle through '{ability.Name}'.");
            }

            return;
        }

        states.Add(ability, 1);
        foreach (var dependent in ability.LevelExpressionDependents)
        {
            Visit(dependent, states);
        }

        states[ability] = 2;
    }
}
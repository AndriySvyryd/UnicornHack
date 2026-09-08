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
        var dependenciesByAbility = new Dictionary<Ability, IReadOnlyList<Ability>>();
        foreach (var ability in abilities.Values)
        {
            ability.ClearLevelExpressionDependents();
        }

        foreach (var ability in abilities.Values)
        {
            if (ability.LevelExpression is not { } expression)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new InvalidOperationException(
                    $"Ability '{ability.Name}' has an empty level expression.");
            }

            var prepared = EffectApplicationSystem.CreateLevelExpression(
                expression, ability.Name);
            ability.SetLevelFunction(prepared.Function);
            var dependencies = new List<Ability>();
            foreach (var dependencyName in prepared.Dependencies)
            {
                if (!abilities.TryGetValue(dependencyName, out var dependency))
                {
                    continue;
                }

                if (ReferenceEquals(dependency, ability))
                {
                    throw new InvalidOperationException(
                        $"Ability '{ability.Name}' cannot depend on its own level.");
                }

                dependency.AddLevelExpressionDependent(ability);
                dependencies.Add(dependency);
            }

            dependenciesByAbility.Add(ability, dependencies);
        }

        ValidateAcyclic(dependenciesByAbility);
        return abilities;
    }

    private static void ValidateAcyclic(
        IReadOnlyDictionary<Ability, IReadOnlyList<Ability>> dependenciesByAbility)
    {
        var states = new Dictionary<Ability, byte>();
        foreach (var ability in dependenciesByAbility.Keys)
        {
            Visit(ability, states, dependenciesByAbility);
        }
    }

    private static void Visit(
        Ability ability, Dictionary<Ability, byte> states,
        IReadOnlyDictionary<Ability, IReadOnlyList<Ability>> dependenciesByAbility)
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
        if (dependenciesByAbility.TryGetValue(ability, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                if (dependenciesByAbility.ContainsKey(dependency))
                {
                    Visit(dependency, states, dependenciesByAbility);
                }
            }
        }

        states[ability] = 2;
    }
}
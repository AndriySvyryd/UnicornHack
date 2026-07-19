namespace UnicornHack.Generation;

public readonly record struct AbilityReference
{
	public AbilityReference(string name, int level = 1)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);
		Name = name;
		Level = level;
	}

	public string Name { get; init; }
	public int Level { get; init; }
}

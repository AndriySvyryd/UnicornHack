namespace UnicornHack.Services;

public class TaggingService
{
    public virtual string GetDamageTag(
        IReadOnlyDictionary<EffectType, int> resourceChanges, out bool damageTaken,
        string? resourceProperty = null)
    {
        damageTaken = false;
        var totalDamage = 0;
        var builder = new StringBuilder();
        builder.Append("<damage");
        foreach (var (effectType, amount) in resourceChanges)
        {
            var metadata = Effect.GetMetadata(effectType);
            if (amount >= 0
                || metadata.ResourceProperty == null
                || resourceProperty != null && metadata.ResourceProperty != resourceProperty)
            {
                continue;
            }

            var damage = Math.Abs(amount);
            builder.Append(' ');
            builder.Append((int)effectType);
            builder.Append("='");
            builder.Append(damage);
            builder.Append("'");
            totalDamage += damage;
        }

        damageTaken = totalDamage != 0;
        builder.Append(">");
        builder.Append(totalDamage);
        builder.Append("</damage>");
        return builder.ToString();
    }
}

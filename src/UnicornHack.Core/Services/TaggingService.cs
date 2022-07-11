namespace UnicornHack.Services;

public class TaggingService
{
    public virtual string GetDamageTag(IReadOnlyList<GameEntity> effects, out bool damageTaken)
    {
        damageTaken = false;
        var totalDamage = 0;
        var builder = new StringBuilder();
        builder.Append("<damage");
        foreach (var effectEntity in effects)
        {
            var effect = effectEntity.Effect!;
            switch (effect.EffectType)
            {
                case EffectType.PhysicalDamage:
                    builder.Append(" physical='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Burn:
                    builder.Append(" fire='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Bleed:
                    builder.Append(" bleeding='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Blight:
                    builder.Append(" toxin='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Corrode:
                    builder.Append(" acid='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Freeze:
                    builder.Append(" cold='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.LightDamage:
                    builder.Append(" light='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.PsychicDamage:
                    builder.Append(" psychic='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Shock:
                    builder.Append(" electricity='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Soak:
                    builder.Append(" water='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.SonicDamage:
                    builder.Append(" sonic='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.Wither:
                    builder.Append(" void='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                case EffectType.DrainLife:
                    builder.Append(" drain='");
                    builder.Append(effect.AppliedAmount);
                    builder.Append("'");
                    break;
                default:
                    continue;
            }

            totalDamage += effect.AppliedAmount!.Value;
            if (totalDamage != 0)
            {
                damageTaken = true;
            }
        }

        builder.Append(">");
        builder.Append(totalDamage);
        builder.Append("</damage>");

        return builder.ToString();
    }
}

using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation;

public abstract class Being : Affectable
{
    public string Name
    {
        get;
        set;
    } = null!;

    public virtual Species? Species
    {
        get;
        set;
    }

    public virtual SpeciesClass? SpeciesClass
    {
        get;
        set;
    }

    public ISet<Ability> Abilities
    {
        get;
        set;
    } = new HashSet<Ability>();

    public RaceComponent AddToAppliedEffect(GameEntity appliedEffectEntity, GameEntity beingEntity)
    {
        var manager = appliedEffectEntity.Manager;
        var race = manager.CreateComponent<RaceComponent>(EntityComponent.Race);
        race.TemplateName = Name;
        race.Species = Species ?? Primitives.Species.Default;
        race.SpeciesClass = SpeciesClass ?? Primitives.SpeciesClass.None;
        race.Level = 1;

        appliedEffectEntity.Race = race;

        var raceAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        raceAbility.Name = Name;
        raceAbility.Activation = ActivationType.Always;

        appliedEffectEntity.Ability = raceAbility;

        foreach (var abilityDefinition in Abilities)
        {
            using var effectEntityReference = manager.CreateEntity();
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.AddAbility;
            effect.Duration = EffectDuration.Infinite;
            effect.ContainingAbilityId = appliedEffectEntity.Id;

            effectEntityReference.Referenced.Effect = effect;

            abilityDefinition.AddToEffect(effectEntityReference.Referenced);
        }

        appliedEffectEntity.Ability.OwnerEntity = beingEntity;

        return race;
    }
}

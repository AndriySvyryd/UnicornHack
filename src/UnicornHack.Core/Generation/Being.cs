using System.Globalization;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Generation;

public abstract class Being
{
    private Ability? _raceAbilityTemplate;

    public string? BaseName
    {
        get;
        set;
    }

    public abstract Being? BaseBeing
    {
        get;
    }

    public string Name
    {
        get;
        set;
    } = null!;

    public virtual SpeciesClass? SpeciesClass
    {
        get => field ?? BaseBeing?.SpeciesClass;
        set;
    }

    public ISet<AbilityReference> AbilityReferences
    {
        get;
        set;
    } = new HashSet<AbilityReference>();

    public virtual Material? Material
    {
        get => field ?? BaseBeing?.Material ?? Primitives.Material.Flesh;
        set;
    }

    public virtual int? MovementDelay
    {
        get => field ?? BaseBeing?.MovementDelay;
        set;
    }

    public virtual int? TurningDelay
    {
        get => field ?? BaseBeing?.TurningDelay;
        set;
    }

    public virtual int? Size
    {
        get => field ?? BaseBeing?.Size;
        set;
    }

    public virtual int? Weight
    {
        get => field ?? BaseBeing?.Weight;
        set;
    }

    public virtual int? PerceptionBonus
    {
        get => field ?? BaseBeing?.PerceptionBonus;
        set;
    }

    public virtual int? MightBonus
    {
        get => field ?? BaseBeing?.MightBonus;
        set;
    }

    public virtual int? SpeedBonus
    {
        get => field ?? BaseBeing?.SpeedBonus;
        set;
    }

    public virtual int? FocusBonus
    {
        get => field ?? BaseBeing?.FocusBonus;
        set;
    }

    public virtual int? Regeneration
    {
        get => field ?? BaseBeing?.Regeneration;
        set;
    }

    public virtual int? HealingResistance
    {
        get => field ?? BaseBeing?.HealingResistance;
        set;
    }

    public virtual int? EnergyRestorationResistance
    {
        get => field ?? BaseBeing?.EnergyRestorationResistance;
        set;
    }

    public virtual int? EnergyRegeneration
    {
        get => field ?? BaseBeing?.EnergyRegeneration;
        set;
    }

    public virtual int? BonusHitPoints
    {
        get => field ?? BaseBeing?.BonusHitPoints;
        set;
    }

    public virtual int? BonusEnergyPoints
    {
        get => field ?? BaseBeing?.BonusEnergyPoints;
        set;
    }

    public virtual int? Accuracy
    {
        get => field ?? BaseBeing?.Accuracy;
        set;
    }

    public virtual int? Evasion
    {
        get => field ?? BaseBeing?.Evasion;
        set;
    }

    public virtual int? Deflection
    {
        get => field ?? BaseBeing?.Deflection;
        set;
    }

    public virtual int? Armor
    {
        get => field ?? BaseBeing?.Armor;
        set;
    }

    public virtual int? PhysicalResistance
    {
        get => field ?? BaseBeing?.PhysicalResistance;
        set;
    }

    public virtual int? MagicResistance
    {
        get => field ?? BaseBeing?.MagicResistance;
        set;
    }

    public virtual int? AcidResistance
    {
        get => field ?? BaseBeing?.AcidResistance;
        set;
    }

    public virtual int? BleedingResistance
    {
        get => field ?? BaseBeing?.BleedingResistance;
        set;
    }

    public virtual int? ColdResistance
    {
        get => field ?? BaseBeing?.ColdResistance;
        set;
    }

    public virtual int? ElectricityResistance
    {
        get => field ?? BaseBeing?.ElectricityResistance;
        set;
    }

    public virtual int? FireResistance
    {
        get => field ?? BaseBeing?.FireResistance;
        set;
    }

    public virtual int? LightResistance
    {
        get => field ?? BaseBeing?.LightResistance;
        set;
    }

    public virtual int? PsychicResistance
    {
        get => field ?? BaseBeing?.PsychicResistance;
        set;
    }

    public virtual int? SonicResistance
    {
        get => field ?? BaseBeing?.SonicResistance;
        set;
    }

    public virtual int? StunResistance
    {
        get => field ?? BaseBeing?.StunResistance;
        set;
    }

    public virtual int? ToxinResistance
    {
        get => field ?? BaseBeing?.ToxinResistance;
        set;
    }

    public virtual int? VoidResistance
    {
        get => field ?? BaseBeing?.VoidResistance;
        set;
    }

    public virtual int? WaterResistance
    {
        get => field ?? BaseBeing?.WaterResistance;
        set;
    }

    public virtual int? DoomResistance
    {
        get => field ?? BaseBeing?.DoomResistance;
        set;
    }

    public virtual int? BindResistance
    {
        get => field ?? BaseBeing?.BindResistance;
        set;
    }

    public virtual int? BlindResistance
    {
        get => field ?? BaseBeing?.BlindResistance;
        set;
    }

    public virtual int? CorrodeResistance
    {
        get => field ?? BaseBeing?.CorrodeResistance;
        set;
    }

    public virtual int? CrippleResistance
    {
        get => field ?? BaseBeing?.CrippleResistance;
        set;
    }

    public virtual int? DazeResistance
    {
        get => field ?? BaseBeing?.DazeResistance;
        set;
    }

    public virtual int? DeafenResistance
    {
        get => field ?? BaseBeing?.DeafenResistance;
        set;
    }

    public virtual int? DecayResistance
    {
        get => field ?? BaseBeing?.DecayResistance;
        set;
    }

    public virtual int? DisorientResistance
    {
        get => field ?? BaseBeing?.DisorientResistance;
        set;
    }

    public virtual int? EnervateResistance
    {
        get => field ?? BaseBeing?.EnervateResistance;
        set;
    }

    public virtual int? EngulfResistance
    {
        get => field ?? BaseBeing?.EngulfResistance;
        set;
    }

    public virtual int? ExposeResistance
    {
        get => field ?? BaseBeing?.ExposeResistance;
        set;
    }

    public virtual int? FascinateResistance
    {
        get => field ?? BaseBeing?.FascinateResistance;
        set;
    }

    public virtual int? FrightenResistance
    {
        get => field ?? BaseBeing?.FrightenResistance;
        set;
    }

    public virtual int? MuteResistance
    {
        get => field ?? BaseBeing?.MuteResistance;
        set;
    }

    public virtual int? ParalyzeResistance
    {
        get => field ?? BaseBeing?.ParalyzeResistance;
        set;
    }

    public virtual int? SedateResistance
    {
        get => field ?? BaseBeing?.SedateResistance;
        set;
    }

    public virtual int? SlowResistance
    {
        get => field ?? BaseBeing?.SlowResistance;
        set;
    }

    public virtual int? StaggerResistance
    {
        get => field ?? BaseBeing?.StaggerResistance;
        set;
    }

    public virtual int? StickResistance
    {
        get => field ?? BaseBeing?.StickResistance;
        set;
    }

    public virtual int? SuffocateResistance
    {
        get => field ?? BaseBeing?.SuffocateResistance;
        set;
    }

    public virtual int? TauntResistance
    {
        get => field ?? BaseBeing?.TauntResistance;
        set;
    }

    public virtual int? ToppleResistance
    {
        get => field ?? BaseBeing?.ToppleResistance;
        set;
    }

    public virtual int? WetResistance
    {
        get => field ?? BaseBeing?.WetResistance;
        set;
    }

    public virtual HeadType? HeadType { get => field ?? BaseBeing?.HeadType; set; }
    public virtual TorsoType? TorsoType { get => field ?? BaseBeing?.TorsoType; set; }
    public virtual ExtremityType? UpperExtremities { get => field ?? BaseBeing?.UpperExtremities; set; }
    public virtual ExtremityType? LowerExtremities { get => field ?? BaseBeing?.LowerExtremities; set; }
    public virtual ExtremityType? BackExtremities { get => field ?? BaseBeing?.BackExtremities; set; }
    public virtual RespirationType? RespirationType { get => field ?? BaseBeing?.RespirationType; set; }
    public virtual LocomotionType? LocomotionType { get => field ?? BaseBeing?.LocomotionType; set; }
    public virtual int? SlotCapacity { get => field ?? BaseBeing?.SlotCapacity; set; }
    public virtual int? EyeCount { get => field ?? BaseBeing?.EyeCount; set; }
    public virtual int? Quietness { get => field ?? BaseBeing?.Quietness; set; }
    public virtual int? VisibilityLevel { get => field ?? BaseBeing?.VisibilityLevel; set; }
    public virtual int? AcuityLevel { get => field ?? BaseBeing?.AcuityLevel; set; }
    public virtual int? HearingLevel { get => field ?? BaseBeing?.HearingLevel; set; }
    public virtual int? Telepathic { get => field ?? BaseBeing?.Telepathic; set; }
    public virtual bool? Infravisible { get => field ?? BaseBeing?.Infravisible; set; }
    public virtual bool? Infravision { get => field ?? BaseBeing?.Infravision; set; }
    public virtual bool? InvisibilityDetection { get => field ?? BaseBeing?.InvisibilityDetection; set; }
    public virtual bool? Clairvoyant { get => field ?? BaseBeing?.Clairvoyant; set; }
    public virtual bool? Clingy { get => field ?? BaseBeing?.Clingy; set; }
    public virtual bool? Displaced { get => field ?? BaseBeing?.Displaced; set; }
    public virtual bool? Reflective { get => field ?? BaseBeing?.Reflective; set; }
    public virtual bool? Mindless { get => field ?? BaseBeing?.Mindless; set; }
    public virtual bool? NonAnimal { get => field ?? BaseBeing?.NonAnimal; set; }
    public virtual bool? Reanimation { get => field ?? BaseBeing?.Reanimation; set; }

    protected static Dictionary<string, Func<TBeing, object?, bool>> GetPropertyConditions<TBeing>()
        where TBeing : Being => new()
    {
        { nameof(Name), (_, v) => v != null },
        { nameof(BaseName), (_, v) => v != null },
        { nameof(SpeciesClass), (o, v) => (SpeciesClass?)v != o.BaseBeing?.SpeciesClass },
        { nameof(AbilityReferences), (_, v) => ((ICollection<AbilityReference>)v!).Count != 0 },
        { nameof(Material), (o, v) => (Material?)v != (o.BaseBeing?.Material ?? Primitives.Material.Flesh) },
        { nameof(MovementDelay), (o, v) => (int?)v != o.BaseBeing?.MovementDelay },
        { nameof(TurningDelay), (o, v) => (int?)v != o.BaseBeing?.TurningDelay },
        { nameof(Size), (o, v) => (int?)v != o.BaseBeing?.Size },
        { nameof(Weight), (o, v) => (int?)v != o.BaseBeing?.Weight },
        { nameof(PerceptionBonus), (o, v) => (int?)v != o.BaseBeing?.PerceptionBonus },
        { nameof(MightBonus), (o, v) => (int?)v != o.BaseBeing?.MightBonus },
        { nameof(SpeedBonus), (o, v) => (int?)v != o.BaseBeing?.SpeedBonus },
        { nameof(FocusBonus), (o, v) => (int?)v != o.BaseBeing?.FocusBonus },
        { nameof(Regeneration), (o, v) => (int?)v != o.BaseBeing?.Regeneration },
        { nameof(HealingResistance), (o, v) => (int?)v != o.BaseBeing?.HealingResistance },
        { nameof(EnergyRestorationResistance), (o, v) => (int?)v != o.BaseBeing?.EnergyRestorationResistance },
        { nameof(EnergyRegeneration), (o, v) => (int?)v != o.BaseBeing?.EnergyRegeneration },
        { nameof(BonusHitPoints), (o, v) => (int?)v != o.BaseBeing?.BonusHitPoints },
        { nameof(BonusEnergyPoints), (o, v) => (int?)v != o.BaseBeing?.BonusEnergyPoints },
        { nameof(Accuracy), (o, v) => (int?)v != o.BaseBeing?.Accuracy },
        { nameof(Evasion), (o, v) => (int?)v != o.BaseBeing?.Evasion },
        { nameof(Deflection), (o, v) => (int?)v != o.BaseBeing?.Deflection },
        { nameof(Armor), (o, v) => (int?)v != o.BaseBeing?.Armor },
        { nameof(PhysicalResistance), (o, v) => (int?)v != o.BaseBeing?.PhysicalResistance },
        { nameof(MagicResistance), (o, v) => (int?)v != o.BaseBeing?.MagicResistance },
        { nameof(AcidResistance), (o, v) => (int?)v != o.BaseBeing?.AcidResistance },
        { nameof(BleedingResistance), (o, v) => (int?)v != o.BaseBeing?.BleedingResistance },
        { nameof(ColdResistance), (o, v) => (int?)v != o.BaseBeing?.ColdResistance },
        { nameof(ElectricityResistance), (o, v) => (int?)v != o.BaseBeing?.ElectricityResistance },
        { nameof(FireResistance), (o, v) => (int?)v != o.BaseBeing?.FireResistance },
        { nameof(LightResistance), (o, v) => (int?)v != o.BaseBeing?.LightResistance },
        { nameof(PsychicResistance), (o, v) => (int?)v != o.BaseBeing?.PsychicResistance },
        { nameof(SonicResistance), (o, v) => (int?)v != o.BaseBeing?.SonicResistance },
        { nameof(StunResistance), (o, v) => (int?)v != o.BaseBeing?.StunResistance },
        { nameof(ToxinResistance), (o, v) => (int?)v != o.BaseBeing?.ToxinResistance },
        { nameof(VoidResistance), (o, v) => (int?)v != o.BaseBeing?.VoidResistance },
        { nameof(WaterResistance), (o, v) => (int?)v != o.BaseBeing?.WaterResistance },
        { nameof(DoomResistance), (o, v) => (int?)v != o.BaseBeing?.DoomResistance },
        { nameof(BindResistance), (o, v) => (int?)v != o.BaseBeing?.BindResistance },
        { nameof(BlindResistance), (o, v) => (int?)v != o.BaseBeing?.BlindResistance },
        { nameof(CorrodeResistance), (o, v) => (int?)v != o.BaseBeing?.CorrodeResistance },
        { nameof(CrippleResistance), (o, v) => (int?)v != o.BaseBeing?.CrippleResistance },
        { nameof(DazeResistance), (o, v) => (int?)v != o.BaseBeing?.DazeResistance },
        { nameof(DeafenResistance), (o, v) => (int?)v != o.BaseBeing?.DeafenResistance },
        { nameof(DecayResistance), (o, v) => (int?)v != o.BaseBeing?.DecayResistance },
        { nameof(DisorientResistance), (o, v) => (int?)v != o.BaseBeing?.DisorientResistance },
        { nameof(EnervateResistance), (o, v) => (int?)v != o.BaseBeing?.EnervateResistance },
        { nameof(EngulfResistance), (o, v) => (int?)v != o.BaseBeing?.EngulfResistance },
        { nameof(ExposeResistance), (o, v) => (int?)v != o.BaseBeing?.ExposeResistance },
        { nameof(FascinateResistance), (o, v) => (int?)v != o.BaseBeing?.FascinateResistance },
        { nameof(FrightenResistance), (o, v) => (int?)v != o.BaseBeing?.FrightenResistance },
        { nameof(MuteResistance), (o, v) => (int?)v != o.BaseBeing?.MuteResistance },
        { nameof(ParalyzeResistance), (o, v) => (int?)v != o.BaseBeing?.ParalyzeResistance },
        { nameof(SedateResistance), (o, v) => (int?)v != o.BaseBeing?.SedateResistance },
        { nameof(SlowResistance), (o, v) => (int?)v != o.BaseBeing?.SlowResistance },
        { nameof(StaggerResistance), (o, v) => (int?)v != o.BaseBeing?.StaggerResistance },
        { nameof(StickResistance), (o, v) => (int?)v != o.BaseBeing?.StickResistance },
        { nameof(SuffocateResistance), (o, v) => (int?)v != o.BaseBeing?.SuffocateResistance },
        { nameof(TauntResistance), (o, v) => (int?)v != o.BaseBeing?.TauntResistance },
        { nameof(ToppleResistance), (o, v) => (int?)v != o.BaseBeing?.ToppleResistance },
        { nameof(WetResistance), (o, v) => (int?)v != o.BaseBeing?.WetResistance },
        { nameof(HeadType), (o, v) => (HeadType?)v != o.BaseBeing?.HeadType },
        { nameof(TorsoType), (o, v) => (TorsoType?)v != o.BaseBeing?.TorsoType },
        { nameof(UpperExtremities), (o, v) => (ExtremityType?)v != o.BaseBeing?.UpperExtremities },
        { nameof(LowerExtremities), (o, v) => (ExtremityType?)v != o.BaseBeing?.LowerExtremities },
        { nameof(BackExtremities), (o, v) => (ExtremityType?)v != o.BaseBeing?.BackExtremities },
        { nameof(RespirationType), (o, v) => (RespirationType?)v != o.BaseBeing?.RespirationType },
        { nameof(LocomotionType), (o, v) => (LocomotionType?)v != o.BaseBeing?.LocomotionType },
        { nameof(SlotCapacity), (o, v) => (int?)v != o.BaseBeing?.SlotCapacity },
        { nameof(EyeCount), (o, v) => (int?)v != o.BaseBeing?.EyeCount },
        { nameof(Quietness), (o, v) => (int?)v != o.BaseBeing?.Quietness },
        { nameof(VisibilityLevel), (o, v) => (int?)v != o.BaseBeing?.VisibilityLevel },
        { nameof(AcuityLevel), (o, v) => (int?)v != o.BaseBeing?.AcuityLevel },
        { nameof(HearingLevel), (o, v) => (int?)v != o.BaseBeing?.HearingLevel },
        { nameof(Telepathic), (o, v) => (int?)v != o.BaseBeing?.Telepathic },
        { nameof(Infravisible), (o, v) => (bool?)v != o.BaseBeing?.Infravisible },
        { nameof(Infravision), (o, v) => (bool?)v != o.BaseBeing?.Infravision },
        { nameof(InvisibilityDetection), (o, v) => (bool?)v != o.BaseBeing?.InvisibilityDetection },
        { nameof(Clairvoyant), (o, v) => (bool?)v != o.BaseBeing?.Clairvoyant },
        { nameof(Clingy), (o, v) => (bool?)v != o.BaseBeing?.Clingy },
        { nameof(Displaced), (o, v) => (bool?)v != o.BaseBeing?.Displaced },
        { nameof(Reflective), (o, v) => (bool?)v != o.BaseBeing?.Reflective },
        { nameof(Mindless), (o, v) => (bool?)v != o.BaseBeing?.Mindless },
        { nameof(NonAnimal), (o, v) => (bool?)v != o.BaseBeing?.NonAnimal },
        { nameof(Reanimation), (o, v) => (bool?)v != o.BaseBeing?.Reanimation }
    };

    protected void ForEachBeingStat(Action<string, int?> apply)
    {
        apply(nameof(PositionComponent.MovementDelay), MovementDelay);
        apply(nameof(PositionComponent.TurningDelay), TurningDelay);
        apply(nameof(PhysicalComponent.Size), Size);
        apply(nameof(PhysicalComponent.Weight), Weight);
        apply(nameof(PhysicalComponent.Material), (int?)Material);
        apply(nameof(BeingComponent.Perception), PerceptionBonus);
        apply(nameof(BeingComponent.Might), MightBonus);
        apply(nameof(BeingComponent.Speed), SpeedBonus);
        apply(nameof(BeingComponent.Focus), FocusBonus);
        apply(nameof(BeingComponent.Regeneration), Regeneration);
        apply(nameof(BeingComponent.HealingResistance), HealingResistance);
        apply(nameof(BeingComponent.EnergyRestorationResistance), EnergyRestorationResistance);
        apply(nameof(BeingComponent.EnergyRegeneration), EnergyRegeneration);
        apply(nameof(BeingComponent.HitPointMaximum), BonusHitPoints);
        apply(nameof(BeingComponent.EnergyPointMaximum), BonusEnergyPoints);
        apply(nameof(BeingComponent.Accuracy), Accuracy);
        apply(nameof(BeingComponent.Evasion), Evasion);
        apply(nameof(BeingComponent.Deflection), Deflection);
        apply(nameof(BeingComponent.Armor), Armor);
        apply(nameof(BeingComponent.PhysicalResistance), PhysicalResistance);
        apply(nameof(BeingComponent.MagicResistance), MagicResistance);
        apply(nameof(BeingComponent.AcidResistance), AcidResistance);
        apply(nameof(BeingComponent.BleedingResistance), BleedingResistance);
        apply(nameof(BeingComponent.ColdResistance), ColdResistance);
        apply(nameof(BeingComponent.ElectricityResistance), ElectricityResistance);
        apply(nameof(BeingComponent.FireResistance), FireResistance);
        apply(nameof(BeingComponent.LightResistance), LightResistance);
        apply(nameof(BeingComponent.PsychicResistance), PsychicResistance);
        apply(nameof(BeingComponent.SonicResistance), SonicResistance);
        apply(nameof(BeingComponent.StunResistance), StunResistance);
        apply(nameof(BeingComponent.ToxinResistance), ToxinResistance);
        apply(nameof(BeingComponent.VoidResistance), VoidResistance);
        apply(nameof(BeingComponent.WaterResistance), WaterResistance);
        apply(nameof(BeingComponent.DoomResistance), DoomResistance);
        apply(nameof(BeingComponent.BindResistance), BindResistance);
        apply(nameof(BeingComponent.BlindResistance), BlindResistance);
        apply(nameof(BeingComponent.CorrodeResistance), CorrodeResistance);
        apply(nameof(BeingComponent.CrippleResistance), CrippleResistance);
        apply(nameof(BeingComponent.DazeResistance), DazeResistance);
        apply(nameof(BeingComponent.DeafenResistance), DeafenResistance);
        apply(nameof(BeingComponent.DecayResistance), DecayResistance);
        apply(nameof(BeingComponent.DisorientResistance), DisorientResistance);
        apply(nameof(BeingComponent.EnervateResistance), EnervateResistance);
        apply(nameof(BeingComponent.EngulfResistance), EngulfResistance);
        apply(nameof(BeingComponent.ExposeResistance), ExposeResistance);
        apply(nameof(BeingComponent.FascinateResistance), FascinateResistance);
        apply(nameof(BeingComponent.FrightenResistance), FrightenResistance);
        apply(nameof(BeingComponent.MuteResistance), MuteResistance);
        apply(nameof(BeingComponent.ParalyzeResistance), ParalyzeResistance);
        apply(nameof(BeingComponent.SedateResistance), SedateResistance);
        apply(nameof(BeingComponent.SlowResistance), SlowResistance);
        apply(nameof(BeingComponent.StaggerResistance), StaggerResistance);
        apply(nameof(BeingComponent.StickResistance), StickResistance);
        apply(nameof(BeingComponent.SuffocateResistance), SuffocateResistance);
        apply(nameof(BeingComponent.TauntResistance), TauntResistance);
        apply(nameof(BeingComponent.ToppleResistance), ToppleResistance);
        apply(nameof(BeingComponent.WetResistance), WetResistance);
        apply(nameof(BeingComponent.HeadType), (int?)HeadType);
        apply(nameof(BeingComponent.TorsoType), (int?)TorsoType);
        apply(nameof(BeingComponent.UpperExtremities), (int?)UpperExtremities);
        apply(nameof(BeingComponent.LowerExtremities), (int?)LowerExtremities);
        apply(nameof(BeingComponent.BackExtremities), (int?)BackExtremities);
        apply(nameof(BeingComponent.HearingLevel), HearingLevel);
        apply(nameof(BeingComponent.Quietness), Quietness);
        apply(nameof(PhysicalComponent.Capacity), SlotCapacity - AbilitySlottingSystem.DefaultSlotCapacity);
        apply(nameof(PhysicalComponent.Infravisible), Infravisible == true ? 1 : null);
        apply(nameof(BeingComponent.Infravision), Infravision == true ? 1 : null);
        apply(nameof(BeingComponent.InvisibilityDetection), InvisibilityDetection == true ? 1 : null);
    }

    /// <summary>The dense loader id of this template, used by creature species resolution.</summary>
    public abstract int GetTemplateId();

    public string GetRaceAbilityName()
        => Name + " race";

    public Ability GetRaceAbilityTemplate()
    {
        if (_raceAbilityTemplate != null)
        {
            return _raceAbilityTemplate;
        }

        var effects = new List<Effect>();
        ForEachBeingStat((property, value) =>
        {
            if (value == null)
            {
                return;
            }

            effects.Add(new ChangeProperty<int>
            {
                TargetActivator = true,
                Amount = value.Value.ToString(CultureInfo.InvariantCulture),
                PropertyName = property
            });
        });

        foreach (var reference in AbilityReferences)
        {
            if (string.IsNullOrWhiteSpace(reference.Name)
                || reference.Level <= 0)
            {
                throw new InvalidOperationException(
                    $"Being '{Name}' has an invalid ability reference.");
            }

            effects.Add(new AddAbility
            {
                Name = reference.Name,
                Amount = reference.Level.ToString(CultureInfo.InvariantCulture)
            });
        }

        var leveledEffects = GetRaceLeveledEffects();
        leveledEffects[1] = leveledEffects.TryGetValue(1, out var firstLevelEffects)
            ? [.. effects, .. firstLevelEffects]
            : effects;
        var raceAbility = new LeveledAbility
        {
            Name = GetRaceAbilityName(),
            Type = AbilityType.Race,
            Activation = ActivationType.Always,
            LeveledEffects = leveledEffects
        };
        return _raceAbilityTemplate = raceAbility;
    }

    protected virtual Dictionary<int, IReadOnlyList<Effect>> GetRaceLeveledEffects() => [];

}

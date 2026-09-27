using System.Buffers;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings;

[Component(Id = (int)EntityComponent.Being)]
public class BeingComponent : GameComponent
{
    private Sex _sex;
    private int _energyPoints;
    private int _hitPoints;
    private int _experiencePoints;
    private int? _primaryNaturalWeaponId;
    private int? _secondaryNaturalWeaponId;
    private int _entropyState;
    private IReadOnlyCollection<GameEntity>? _items;

    public BeingComponent()
    {
        ComponentId = (int)EntityComponent.Being;
    }

    public Sex Sex
    {
        get => _sex;
        set => SetWithNotify(value, ref _sex);
    }

    [Property<int>]
    public int EnergyPointMaximum =>
        Entity is { ActiveEffects: { } activeEffects }
            ? activeEffects.ResolveInt(nameof(EnergyPointMaximum))
            : int.MaxValue;

    public int EnergyPoints
    {
        get => _energyPoints;
        set
        {
            var newEP = value;
            if (newEP < 0)
            {
                newEP = 0;
            }

            var maxEP = EnergyPointMaximum - ReservedEnergyPoints;
            if (newEP > maxEP)
            {
                newEP = maxEP;
            }

            SetWithNotify(newEP, ref _energyPoints);
        }
    }

    [Property<int>(MinValue = 0)]
    public int ReservedEnergyPoints =>
        Entity is { ActiveEffects: { } activeEffects }
            ? Math.Min(EnergyPointMaximum, activeEffects.ResolveInt(nameof(ReservedEnergyPoints)))
            : 0;

    [Property<int>]
    public int HitPointMaximum =>
        Entity is { ActiveEffects: { } activeEffects }
            ? activeEffects.ResolveInt(nameof(HitPointMaximum))
            : int.MaxValue;

    public int HitPoints
    {
        get => _hitPoints;
        set
        {
            var newHP = value;
            var maxHP = HitPointMaximum - ReservedHitPoints;
            if (newHP > maxHP)
            {
                newHP = maxHP;
            }

            SetWithNotify(newHP, ref _hitPoints);
        }
    }

    [Property<int>(MinValue = 0)]
    public int ReservedHitPoints =>
        Entity is { ActiveEffects: { } activeEffects }
            ? Math.Min(HitPointMaximum, activeEffects.ResolveInt(nameof(ReservedHitPoints)))
            : 0;

    public bool IsAlive => HitPoints > 0;

    [Property<int>(DefaultValue = 10, MinValue = 0)]
    public int Perception => Entity.ActiveEffects!.ResolveInt(nameof(Perception));

    [Property<int>(DefaultValue = 10, MinValue = 0)]
    public int Might => Entity.ActiveEffects!.ResolveInt(nameof(Might));

    [Property<int>(DefaultValue = 10, MinValue = 0)]
    public int Speed => Entity.ActiveEffects!.ResolveInt(nameof(Speed));

    [Property<int>(DefaultValue = 10, MinValue = 0)]
    public int Focus => Entity.ActiveEffects!.ResolveInt(nameof(Focus));

    [Property<int>(DefaultValue = 100, MinValue = 1)]
    public int CooldownFactor => Entity.ActiveEffects!.ResolveInt(nameof(CooldownFactor));

    [Property<int>(DefaultValue = 1, MinValue = 0)]
    public int EnergyRegeneration => Entity.ActiveEffects!.ResolveInt(nameof(EnergyRegeneration));

    [Property<int>]
    public int Regeneration => Entity.ActiveEffects!.ResolveInt(nameof(Regeneration));

    [Property<int>]
    public int HealingResistance => Entity.ActiveEffects!.ResolveInt(nameof(HealingResistance));

    [Property<int>]
    public int EnergyRestorationResistance => Entity.ActiveEffects!.ResolveInt(nameof(EnergyRestorationResistance));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanMove => Entity.ActiveEffects!.ResolveBool(nameof(CanMove));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanMoveItems => Entity.ActiveEffects!.ResolveBool(nameof(CanMoveItems));

    [Property<int>(MinValue = 0, MaxValue = 100)]
    public int UnableToActPercent => Entity.ActiveEffects!.ResolveInt(nameof(UnableToActPercent));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanCastSpells => Entity.ActiveEffects!.ResolveBool(nameof(CanCastSpells));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseVerbalAbilities => Entity.ActiveEffects!.ResolveBool(nameof(CanUseVerbalAbilities));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanPerceiveIntention => Entity.ActiveEffects!.ResolveBool(nameof(CanPerceiveIntention));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseDefaultAttacks => Entity.ActiveEffects!.ResolveBool(nameof(CanUseDefaultAttacks));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseComplexAbilities => Entity.ActiveEffects!.ResolveBool(nameof(CanUseComplexAbilities));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseNonDefaultAbilities => Entity.ActiveEffects!.ResolveBool(nameof(CanUseNonDefaultAbilities));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseNonAttackAbilities => Entity.ActiveEffects!.ResolveBool(nameof(CanUseNonAttackAbilities));

    [Property<int>(MinValue = 0, MaxValue = 100)]
    public int RandomMovementPercent => Entity.ActiveEffects!.ResolveInt(nameof(RandomMovementPercent));

    [Property<int>(MinValue = 0, MaxValue = 100)]
    public int RandomActionPercent => Entity.ActiveEffects!.ResolveInt(nameof(RandomActionPercent));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseDivineAbilities => Entity.ActiveEffects!.ResolveBool(nameof(CanUseDivineAbilities));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseComplexFollowerCommands =>
        Entity.ActiveEffects!.ResolveBool(nameof(CanUseComplexFollowerCommands));

    [Property<bool>(DefaultValue = true, DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool CanUseFollowerCommands => Entity.ActiveEffects!.ResolveBool(nameof(CanUseFollowerCommands));

    [Property<int>(MinValue = 0, MaxValue = 100)]
    public int RandomMoveAfterActionPercent =>
        Entity.ActiveEffects!.ResolveInt(nameof(RandomMoveAfterActionPercent));

    [Property<bool>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool EmitsSmoke => Entity.ActiveEffects!.ResolveBool(nameof(EmitsSmoke));

    [Property<int>(MinValue = 0, MaxValue = 100)]
    public int FollowerCommandFailurePercent =>
        Entity.ActiveEffects!.ResolveInt(nameof(FollowerCommandFailurePercent));

    [Property<int>(MinValue = 0)]
    public int Hindrance => Entity.ActiveEffects!.ResolveInt(nameof(Hindrance));

    [Property<int>]
    public int Accuracy => Entity.ActiveEffects!.ResolveInt(nameof(Accuracy));

    [Property<int>]
    public int Evasion => Entity.ActiveEffects!.ResolveInt(nameof(Evasion));

    [Property<int>]
    public int Deflection => Entity.ActiveEffects!.ResolveInt(nameof(Deflection));

    [Property<int>(MinValue = 0)]
    public int Armor => Entity.ActiveEffects!.ResolveInt(nameof(Armor));

    [Property<int>]
    public int PhysicalResistance => Entity.ActiveEffects!.ResolveInt(nameof(PhysicalResistance));

    [Property<int>]
    public int PhysicalStatusResistance => Entity.ActiveEffects!.ResolveInt(nameof(PhysicalStatusResistance));

    [Property<int>]
    public int MagicResistance => Entity.ActiveEffects!.ResolveInt(nameof(MagicResistance));

    [Property<int>]
    public int MentalStatusResistance => Entity.ActiveEffects!.ResolveInt(nameof(MentalStatusResistance));

    [Property<int>]
    public int BindResistance => Entity.ActiveEffects!.ResolveInt(nameof(BindResistance));

    [Property<int>]
    public int BlindResistance => Entity.ActiveEffects!.ResolveInt(nameof(BlindResistance));

    [Property<int>]
    public int CorrodeResistance => Entity.ActiveEffects!.ResolveInt(nameof(CorrodeResistance));

    [Property<int>]
    public int CrippleResistance => Entity.ActiveEffects!.ResolveInt(nameof(CrippleResistance));

    [Property<int>]
    public int DazeResistance => Entity.ActiveEffects!.ResolveInt(nameof(DazeResistance));

    [Property<int>]
    public int DeafenResistance => Entity.ActiveEffects!.ResolveInt(nameof(DeafenResistance));

    [Property<int>]
    public int DecayResistance => Entity.ActiveEffects!.ResolveInt(nameof(DecayResistance));

    [Property<int>]
    public int DisorientResistance => Entity.ActiveEffects!.ResolveInt(nameof(DisorientResistance));

    [Property<int>]
    public int EnervateResistance => Entity.ActiveEffects!.ResolveInt(nameof(EnervateResistance));

    [Property<int>]
    public int EngulfResistance => Entity.ActiveEffects!.ResolveInt(nameof(EngulfResistance));

    [Property<int>]
    public int ExposeResistance => Entity.ActiveEffects!.ResolveInt(nameof(ExposeResistance));

    [Property<int>]
    public int FascinateResistance => Entity.ActiveEffects!.ResolveInt(nameof(FascinateResistance));

    [Property<int>]
    public int FrightenResistance => Entity.ActiveEffects!.ResolveInt(nameof(FrightenResistance));

    [Property<int>]
    public int MuteResistance => Entity.ActiveEffects!.ResolveInt(nameof(MuteResistance));

    [Property<int>]
    public int ParalyzeResistance => Entity.ActiveEffects!.ResolveInt(nameof(ParalyzeResistance));

    [Property<int>]
    public int SedateResistance => Entity.ActiveEffects!.ResolveInt(nameof(SedateResistance));

    [Property<int>]
    public int SlowResistance => Entity.ActiveEffects!.ResolveInt(nameof(SlowResistance));

    [Property<int>]
    public int StaggerResistance => Entity.ActiveEffects!.ResolveInt(nameof(StaggerResistance));

    [Property<int>]
    public int StickResistance => Entity.ActiveEffects!.ResolveInt(nameof(StickResistance));

    [Property<int>]
    public int SuffocateResistance => Entity.ActiveEffects!.ResolveInt(nameof(SuffocateResistance));

    [Property<int>]
    public int TauntResistance => Entity.ActiveEffects!.ResolveInt(nameof(TauntResistance));

    [Property<int>]
    public int ToppleResistance => Entity.ActiveEffects!.ResolveInt(nameof(ToppleResistance));

    [Property<int>]
    public int WetResistance => Entity.ActiveEffects!.ResolveInt(nameof(WetResistance));

    [Property<int>]
    public int AcidResistance => Entity.ActiveEffects!.ResolveInt(nameof(AcidResistance));

    [Property<int>]
    public int BleedingResistance => Entity.ActiveEffects!.ResolveInt(nameof(BleedingResistance));

    [Property<int>]
    public int ColdResistance => Entity.ActiveEffects!.ResolveInt(nameof(ColdResistance));

    [Property<int>]
    public int ElectricityResistance => Entity.ActiveEffects!.ResolveInt(nameof(ElectricityResistance));

    [Property<int>]
    public int FireResistance => Entity.ActiveEffects!.ResolveInt(nameof(FireResistance));

    [Property<int>(DefaultValue = 100)]
    public int LightResistance => Entity.ActiveEffects!.ResolveInt(nameof(LightResistance));

    [Property<int>]
    public int PsychicResistance => Entity.ActiveEffects!.ResolveInt(nameof(PsychicResistance));

    [Property<int>]
    public int SonicResistance => Entity.ActiveEffects!.ResolveInt(nameof(SonicResistance));

    [Property<int>]
    public int StunResistance => Entity.ActiveEffects!.ResolveInt(nameof(StunResistance));

    [Property<int>]
    public int DoomResistance => Entity.ActiveEffects!.ResolveInt(nameof(DoomResistance));

    [Property<int>]
    public int ToxinResistance => Entity.ActiveEffects!.ResolveInt(nameof(ToxinResistance));

    [Property<int>]
    public int VoidResistance => Entity.ActiveEffects!.ResolveInt(nameof(VoidResistance));

    [Property<int>(DefaultValue = 100)]
    public int WaterResistance => Entity.ActiveEffects!.ResolveInt(nameof(WaterResistance));

    [Property<HeadType>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public HeadType HeadType => (HeadType)Entity.ActiveEffects!.ResolveInt(nameof(HeadType));

    [Property<TorsoType>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public TorsoType TorsoType => (TorsoType)Entity.ActiveEffects!.ResolveInt(nameof(TorsoType));

    [Property<ExtremityType>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public ExtremityType UpperExtremities => (ExtremityType)Entity.ActiveEffects!.ResolveInt(nameof(UpperExtremities));

    [Property<ExtremityType>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public ExtremityType LowerExtremities => (ExtremityType)Entity.ActiveEffects!.ResolveInt(nameof(LowerExtremities));

    [Property<ExtremityType>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public ExtremityType BackExtremities => (ExtremityType)Entity.ActiveEffects!.ResolveInt(nameof(BackExtremities));

    [Property<int>(DefaultValue = 100, MinValue = 0)]
    public int Visibility => Entity.ActiveEffects!.ResolveInt(nameof(Visibility));

    [Property<int>(DefaultValue = 1)]
    public int PrimaryFOVQuadrants => Entity.ActiveEffects!.ResolveInt(nameof(PrimaryFOVQuadrants));

    [Property<int>(DefaultValue = 2)]
    public int TotalFOVQuadrants => Entity.ActiveEffects!.ResolveInt(nameof(TotalFOVQuadrants));

    [Property<int>(DefaultValue = 16)]
    public int PrimaryVisionRange => Entity.ActiveEffects!.ResolveInt(nameof(PrimaryVisionRange));

    [Property<int>(DefaultValue = 8)]
    public int SecondaryVisionRange => Entity.ActiveEffects!.ResolveInt(nameof(SecondaryVisionRange));

    [Property<int>(MinValue = 0, MaxValue = 254)]
    public int HearingLevel => Entity.ActiveEffects!.ResolveInt(nameof(HearingLevel));

    [Property<int>(MinValue = 0, MaxValue = 254)]
    public int Quietness => Entity.ActiveEffects!.ResolveInt(nameof(Quietness));

    [Property<bool>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool Infravision => Entity.ActiveEffects!.ResolveBool(nameof(Infravision));

    [Property<bool>(DefaultCombinationFunction = ValueCombinationFunction.Override)]
    public bool InvisibilityDetection => Entity.ActiveEffects!.ResolveBool(nameof(InvisibilityDetection));

    /// <summary>The per-tile visibility the being currently perceives; transient (pooled array), not persisted.</summary>
    public byte[]? VisibleTerrain
    {
        get;
        set;
    }

    public bool VisibleTerrainIsCurrent
    {
        get;
        set;
    }

    public int ExperiencePoints
    {
        get => _experiencePoints;
        set => SetWithNotify(value, ref _experiencePoints);
    }

    public int? PrimaryNaturalWeaponId
    {
        get => _primaryNaturalWeaponId;
        set => SetWithNotify(value, ref _primaryNaturalWeaponId);
    }

    public int? SecondaryNaturalWeaponId
    {
        get => _secondaryNaturalWeaponId;
        set => SetWithNotify(value, ref _secondaryNaturalWeaponId);
    }

    public int EntropyState
    {
        get => _entropyState;
        set => SetWithNotify(value, ref _entropyState);
    }

    /// <summary>
    ///     The being's current form as a creature/player-race template id: while a polymorph transformation
    ///     is in force it resolves to the assumed form, otherwise to the being's own single innate race (null
    ///     for a being with no single race - e.g. a player - or for a race carrier). Reverts automatically
    ///     once the transformation lapses and its identity marker expires.
    /// </summary>
    public int? Species
    {
        get
        {
            if (Entity.ActiveEffects is { } activeEffects)
            {
                foreach (var instance in activeEffects.PropertyEffects)
                {
                    if (instance.EffectType == EffectType.Polymorph
                        && instance.TargetName == null)
                    {
                        return instance.MagnitudeSecondary;
                    }
                }
            }

            // A player is not a single fixed species; only a single-race creature reports its innate form.
            if (Entity.Player != null
                || Races.Count != 1)
            {
                return null;
            }

            foreach (var raceEntity in Races)
            {
                var raceName = raceEntity.Ability?.Template?.Name ?? raceEntity.Ability?.Name;
                return raceName != null
                       && Entity.Manager.Game.Services.Data.Creatures.FindByIndex(raceName) is { } creature
                    ? creature.GetTemplateId()
                    : null;
            }

            return null;
        }
    }

    public IReadOnlyDictionary<int, GameEntity> SlottedAbilities
    {
        get;
    } = new SortedDictionary<int, GameEntity>();

    public IReadOnlyCollection<GameEntity> Abilities => Entity.ActiveEffects!.Abilities;

    /// <summary>
    ///     The race carriers this being currently gained, derived from its AddAbility structural directives
    ///     (<see cref="ActiveComponentEffect" /> entries in its <see cref="ActiveEffectsComponent" />). Only
    ///     each race's authoritative unsuffixed carrier is exposed; supplemental level carriers remain internal.
    ///     Empty on a race carrier itself.
    /// </summary>
    public IReadOnlyCollection<GameEntity> Races
    {
        get
        {
            if (Entity?.ActiveEffects is not { } activeEffects)
            {
                return Array.Empty<GameEntity>();
            }

            HashSet<GameEntity>? races = null;
            foreach (var record in activeEffects.ComponentEffects)
            {
                if (record.EffectType == EffectType.AddAbility
                    && record.CarrierId != 0
                    && Entity.Manager.FindEntity(record.CarrierId) is
                    {
                        Ability:
                        {
                            Type: AbilityType.Race,
                            IsLeveledSupplement: false,
                            Level: > 0
                        }
                    } carrier)
                {
                    (races ??= new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance)).Add(carrier);
                }
            }

            return (IReadOnlyCollection<GameEntity>?)races ?? Array.Empty<GameEntity>();
        }
    }

    public IReadOnlyCollection<GameEntity> Items
    {
        get
        {
            if (_items == null)
            {
                _items = new HashSet<GameEntity>(EntityEqualityComparer<GameEntity>.Instance);
                var items = Entity.Item?.Items
                            ?? Entity.Physical?.Items;
                if (items != null)
                {
                    _items = items;
                }
            }

            return _items;
        }
    }

    protected override void Clean()
    {
        _sex = default;
        _energyPoints = default;
        _hitPoints = default;
        _experiencePoints = default;
        _primaryNaturalWeaponId = default;
        _secondaryNaturalWeaponId = default;
        _entropyState = default;
        ((SortedDictionary<int, GameEntity>)SlottedAbilities).Clear();
        _items = default!;

        if (VisibleTerrain != null)
        {
            ArrayPool<byte>.Shared.Return(VisibleTerrain);
        }

        VisibleTerrain = default;
        VisibleTerrainIsCurrent = default;

        base.Clean();
    }
}

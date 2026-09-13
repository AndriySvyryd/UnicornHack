using System.Globalization;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Data.Players;
using UnicornHack.Generation.Effects;
using UnicornHack.Services;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Time;
using UnicornHack.Utils.MessagingECS;
using static UnicornHack.TestHelper;

namespace UnicornHack.Systems.Effects;

public class EffectApplicationSystemTest
{
    [Fact]
    public void Propagated_amount_placeholder_throws_when_evaluated()
    {
        var decayedEffects = Assert.IsAssignableFrom<IReadOnlyList<Effect>>(AbilityData.Decayed.Effects);
        var enervatedEffects = Assert.IsAssignableFrom<IReadOnlyList<Effect>>(AbilityData.Enervated.Effects);
        Assert.Equal(
            Effect.PropagatedAmountPlaceholder,
            Assert.Single(decayedEffects).Amount);
        Assert.Equal(
            Effect.PropagatedAmountPlaceholder,
            Assert.Single(enervatedEffects).Amount);
        var function = EffectApplicationSystem.CreateAmountFunction(
            Effect.PropagatedAmountPlaceholder, "test ability", "test effect");

        var exception = Assert.Throws<InvalidOperationException>(
            () => function(null!, null!, null));

        Assert.Contains("propagated amount placeholder", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Propagated_amounts_only_replace_matching_placeholders()
    {
        var manager = TestHelper.BuildRoom(".").Entity.Manager;
        var template = new Ability
        {
            Name = "placeholder test",
            Effects =
            [
                new PhysicalDamage
                {
                    Amount = Effect.PropagatedAmountPlaceholder,
                    ArmorPenetration = "11"
                },
                new PhysicalDamage
                {
                    Amount = "5",
                    ArmorPenetration = Effect.PropagatedAmountPlaceholder
                }
            ]
        };

        using var carrierReference = manager.CreateEntity();
        var ability = template.AddToEffect(
            carrierReference.Referenced, firstLevel: 1, lastLevel: 1,
            propagatedAmount: "7",
            propagatedSecondaryAmount: "13")!;

        var primaryPropagated = ability.Effects.Single(effect => effect.Effect!.Amount == "7").Effect!;
        Assert.Equal("11", primaryPropagated.SecondaryAmount);
        var secondaryPropagated = ability.Effects.Single(effect => effect.Effect!.SecondaryAmount == "13").Effect!;
        Assert.Equal("5", secondaryPropagated.Amount);
    }

    [Fact]
    public void Unresolved_polymorph_creature_placeholder_throws_when_applied()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var target = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var template = new Ability
        {
            Name = "unresolved polymorph test",
            Effects = [new Polymorph { CreatureName = Effect.PropagatedAmountPlaceholder }]
        };

        using var abilityReference = manager.CreateEntity();
        var ability = template.AddToEffect(abilityReference.Referenced, 1, 1)!;
        ability.OwnerEntity = target;
        var effectEntity = Assert.Single(ability.Effects);
        TestHelper.ApplyEffect(effectEntity, target, target, manager);

        var exception = Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));
        Assert.Contains("SecondaryAmount", exception.Message);
        Assert.Contains(
            "propagated amount placeholder",
            exception.InnerException!.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Named_secondary_properties_populate_the_runtime_effect_component()
    {
        var manager = TestHelper.BuildRoom(".").Entity.Manager;
        var template = new Ability
        {
            Name = "named secondary test",
            Effects =
            [
                new Bind { ResistancePenetration = "25" },
                new PhysicalDamage { ArmorPenetration = "11" },
                new Polymorph { CreatureName = "gnome" },
                new ConferLycanthropy { CreatureName = "wolfwere" }
            ]
        };

        using var carrierReference = manager.CreateEntity();
        var ability = template.AddToEffect(carrierReference.Referenced, 1, 1)!;

        Assert.Equal(
            "25",
            ability.Effects.Single(effect => effect.Effect!.EffectType == EffectType.Bind)
                .Effect!.SecondaryAmount);
        Assert.Equal(
            "11",
            ability.Effects.Single(effect => effect.Effect!.EffectType == EffectType.PhysicalDamage)
                .Effect!.SecondaryAmount);
        var polymorph = ability.Effects.Single(
            effect => effect.Effect!.EffectType == EffectType.Polymorph).Effect!;
        Assert.Equal("gnome", polymorph.TargetName);
        Assert.Null(polymorph.SecondaryAmount);
        var lycanthropy = ability.Effects.Single(
            effect => effect.Effect!.EffectType == EffectType.ConferLycanthropy).Effect!;
        Assert.Equal("wolfwere", lycanthropy.TargetName);
        Assert.Null(lycanthropy.SecondaryAmount);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 30)]
    [InlineData(7, 13)]
    [InlineData(30, 1)]
    [InlineData(30, 30)]
    public void Independent_decay_instances_sum_their_live_magnitudes(int first, int second)
    {
        var (target, manager) = CreateEffectTarget();

        EffectTestHelper.ApplyStatus(
            target, EffectType.Decay, duration: 100, manager, amount: first);
        EffectTestHelper.ApplyStatus(
            target, EffectType.Decay, duration: 200, manager, amount: second);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(first + second, target.Being!.ReservedHitPoints);
        Assert.Equal(2, target.ActiveEffects!.PropertyEffects.Count(
            effect => effect.EffectType == EffectType.Decay));
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(1, 200)]
    [InlineData(17, 83)]
    [InlineData(200, 1)]
    [InlineData(83, 17)]
    public void Expiration_removes_only_instances_due_at_the_current_tick(
        int firstDuration, int secondDuration)
    {
        var earlierDuration = Math.Min(firstDuration, secondDuration);
        var laterDuration = Math.Max(firstDuration, secondDuration);
        var (target, manager) = CreateEffectTarget();

        EffectTestHelper.ApplyStatus(
            target, EffectType.Decay, firstDuration, manager, amount: 3);
        EffectTestHelper.ApplyStatus(
            target, EffectType.Decay, secondDuration, manager, amount: 5);
        manager.Queue.ProcessQueue(manager);

        manager.Game.CurrentTick = earlierDuration;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(target, manager);
        manager.Queue.ProcessQueue(manager);

        var remaining = target.ActiveEffects!.PropertyEffects
            .Single(effect => effect.EffectType == EffectType.Decay);
        Assert.Equal(EffectDuration.UntilTimeout, remaining.ExpirationEvent);
        Assert.Equal(laterDuration, remaining.ExpirationTarget);
        Assert.Equal(firstDuration > secondDuration ? 3 : 5, target.Being!.ReservedHitPoints);
    }

    [Fact]
    public void Transition_expiration_distinguishes_room_and_level_bound_records()
    {
        var (target, manager) = CreateEffectTarget();
        var activeEffects = target.ActiveEffects!;
        activeEffects.AddPropertyEffect(CreatePropertyEffect(activeEffects,
            0, 0, 0, EffectType.ChangeProperty, null,
            expirationEvent: EffectDuration.UntilNextRoom));
        activeEffects.AddPropertyEffect(CreatePropertyEffect(activeEffects,
            0, 0, 0, EffectType.ChangeProperty, null,
            expirationEvent: EffectDuration.UntilNextLevel));
        var roomAbility = manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics);
        roomAbility.ExpirationEvent = EffectDuration.UntilNextRoom;
        var levelAbility = manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Stealth);
        levelAbility.ExpirationEvent = EffectDuration.UntilNextLevel;

        manager.EffectApplicationSystem.ExpireTransitionEffects(
            target, levelChanged: false, manager);

        Assert.DoesNotContain(
            activeEffects.PropertyEffects,
            effect => effect.ExpirationEvent == EffectDuration.UntilNextRoom);
        Assert.Contains(
            activeEffects.PropertyEffects,
            effect => effect.ExpirationEvent == EffectDuration.UntilNextLevel);
        Assert.DoesNotContain(roomAbility, activeEffects.ComponentEffects);
        Assert.Contains(levelAbility, activeEffects.ComponentEffects);

        manager.EffectApplicationSystem.ExpireTransitionEffects(
            target, levelChanged: true, manager);

        Assert.DoesNotContain(
            activeEffects.PropertyEffects,
            effect => effect.ExpirationEvent != null);
        Assert.DoesNotContain(levelAbility, activeEffects.ComponentEffects);
    }

    [Fact]
    public void Same_room_travel_does_not_expire_transition_bound_effects()
    {
        var (target, manager) = CreateEffectTarget();
        var marker = target.ActiveEffects!.AddPropertyEffect(
            EffectType.ChangeProperty,
            expirationEvent: EffectDuration.UntilNextRoom);
        var traveled = TraveledMessage.Create(manager);
        traveled.Entity = target;
        traveled.InitialLevel = target.Position!.RoomEntity;
        traveled.Successful = true;

        manager.EffectApplicationSystem.Process(traveled, manager);

        Assert.Contains(marker, target.ActiveEffects!.PropertyEffects);
        manager.ReturnMessage(traveled);
    }

    [Fact]
    public void Named_ability_decrease_suppresses_and_restores_without_consuming_positive_levels()
    {
        var (target, manager) = CreateEffectTarget();
        var positiveRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 3);
        manager.Queue.ProcessQueue(manager);

        var suppressor = AddLevelSuppressor(
            target, "acrobatics suppressor", AbilityType.Buff,
            abilityName: AbilityData.Acrobatics.Name, abilityType: null,
            amount: 4, manager);

        Assert.Equal(3, positiveRecord.Level);
        Assert.Equal(-1, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.Acrobatics.Name));
        Assert.NotEqual(0, positiveRecord.CarrierId);
        var suppressed = manager.AffectableAbilitiesIndex[
            (target.Id, AbilityData.Acrobatics.Name)]!.Ability!;
        Assert.Equal(0, suppressed.Level);
        Assert.False(suppressed.IsUsable);
        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevelReduction(
            target, AbilityData.Acrobatics.Name));

        manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
            target, suppressor.EntityId, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(3, positiveRecord.Level);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.Acrobatics.Name));
        var acrobatics = manager.AffectableAbilitiesIndex[
            (target.Id, AbilityData.Acrobatics.Name)]!.Ability!;
        Assert.Equal(3, acrobatics.Level);
        Assert.True(acrobatics.IsUsable);
    }

    [Fact]
    public void Partial_suppression_keeps_only_records_contributing_active_levels()
    {
        var (target, manager) = CreateEffectTarget();
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);
        var firstRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 2, firstSource.Id);
        var secondRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 2, secondSource.Id);
        manager.Queue.ProcessQueue(manager);

        var suppressor = AddLevelSuppressor(
            target, "partial acrobatics suppressor", AbilityType.Buff,
            abilityName: AbilityData.Acrobatics.Name, abilityType: null,
            amount: 3, manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Acrobatics.Name));
        Assert.NotEqual(0, firstRecord.CarrierId);
        Assert.Equal(0, secondRecord.CarrierId);
        Assert.Equal(
            1,
            manager.AffectableAbilitiesIndex[(target.Id, AbilityData.Acrobatics.Name)]!.Ability!.Level);

        manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
            target, suppressor.EntityId, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Acrobatics.Name));
        Assert.NotEqual(0, firstRecord.CarrierId);
        Assert.NotEqual(0, secondRecord.CarrierId);
    }

    [Fact]
    public void Non_leveled_ability_does_not_create_a_supplemental_carrier()
    {
        var manager = TestHelper.BuildRoom(".").Entity.Manager;
        var template = new Ability { Name = "non-leveled test", Type = AbilityType.Trait };
        using var carrierReference = manager.CreateEntity();

        Assert.Null(template.AddToEffect(
            carrierReference.Referenced, firstLevel: 2, lastLevel: 2));
        Assert.Null(carrierReference.Referenced.Ability);
    }

    [Theory]
    [InlineData(AbilityType.MeleeSpell, "spell")]
    [InlineData(AbilityType.RangedSpell, "spell")]
    [InlineData(AbilityType.Miracle, "miracle")]
    [InlineData(AbilityType.Buff, "buff")]
    [InlineData(AbilityType.Boon, "boon")]
    [InlineData(AbilityType.Curse, "curse")]
    [InlineData(AbilityType.Trait, "trait")]
    [InlineData(AbilityType.Flaw, "flaw")]
    [InlineData(AbilityType.Feat, "feat")]
    [InlineData(AbilityType.Fault, "fault")]
    [InlineData(AbilityType.Skill, "skill")]
    [InlineData(AbilityType.Faith, "faith")]
    public void Player_ability_level_changes_are_logged(
        AbilityType abilityType, string kind)
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();
        var abilityName = $"test {kind}";
        var template = new Ability
        {
            Name = abilityName,
            Type = abilityType
        };
        var abilityRef =
            $"<tooltip id='{abilityName}' category='{(int)DescriptionCategory.Ability}'>{abilityName}</tooltip>";

        manager.EffectApplicationSystem.AddAbilityEffect(player, template);
        manager.EffectApplicationSystem.AddAbilityEffect(player, template);
        manager.EffectApplicationSystem.RemoveAbility(
            player, abilityName, manager);

        var removalMessage = abilityType.IsStatusLike()
            ? $"Your {abilityRef} {kind} ends."
            : $"You lose the {abilityRef} {kind}.";
        Assert.Equal(
            [
                $"You gain the {abilityRef} {kind}.",
                $"Your {abilityRef} {kind} increases to level 2.",
                removalMessage
            ],
            player.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Direct_debuff_level_changes_do_not_use_generic_ability_messages()
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();
        var template = new Ability
        {
            Name = "test debuff",
            Type = AbilityType.Debuff
        };

        manager.EffectApplicationSystem.AddAbilityEffect(player, template);
        manager.EffectApplicationSystem.RemoveAbility(
            player, template.Name, manager);

        Assert.Empty(player.Player.LogEntries);
    }

    [Fact]
    public void Player_race_changes_are_logged_outside_player_choices()
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();
        var race = PlayerRaceData.Dwarf;
        var raceRef =
            $"<tooltip id='{race.Name}' category='{(int)DescriptionCategory.Race}'>{race.Name}</tooltip>";

        manager.EffectApplicationSystem.AddAbilityEffect(
            player, race.GetRaceAbilityTemplate());
        manager.EffectApplicationSystem.RemoveAbility(
            player, race.GetRaceAbilityName(), manager);

        Assert.Equal(
            [
                $"You gain the {raceRef} race.",
                $"You lose the {raceRef} race."
            ],
            player.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Missing_effect_level_creates_an_empty_carrier_below_the_max_level()
    {
        var (target, manager) = CreateEffectTarget();
        var template = new LeveledAbility
        {
            Name = "effectless level test",
            Type = AbilityType.Trait,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                [1] = [],
                [3] = []
            }
        };
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);

        manager.EffectApplicationSystem.AddAbilityEffect(target, template, level: 1, firstSource.Id);
        var secondRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            target, template, level: 1, secondSource.Id);

        Assert.NotEqual(0, secondRecord.CarrierId);
        Assert.Empty(manager.FindEntity(secondRecord.CarrierId)!.Ability!.Effects);
    }

    [Fact]
    public void Level_above_the_max_defined_level_does_not_create_a_carrier()
    {
        var (target, manager) = CreateEffectTarget();
        var template = new LeveledAbility
        {
            Name = "max level test",
            Type = AbilityType.Trait,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>> { [1] = [] }
        };
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);

        manager.EffectApplicationSystem.AddAbilityEffect(target, template, level: 1, firstSource.Id);
        var secondRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            target, template, level: 1, secondSource.Id);

        Assert.Equal(0, secondRecord.CarrierId);
    }

    [Fact]
    public void DecreaseAbilityLevel_requires_exactly_one_selector_and_uses_default_amount_and_duration()
    {
        var (target, manager) = CreateEffectTarget();

        Assert.Throws<InvalidOperationException>(() => new Ability
        {
            Name = "missing selector",
            Effects = [new DecreaseAbilityLevel()]
        }.AddToAffectable(target));
        Assert.Throws<InvalidOperationException>(() => new Ability
        {
            Name = "duplicate selector",
            Effects =
            [
                new DecreaseAbilityLevel
                {
                    AbilityName = AbilityData.Acrobatics.Name,
                    AbilityType = AbilityType.Skill
                }
            ]
        }.AddToAffectable(target));

        var ability = new Ability
        {
            Name = "valid selector",
            Effects =
            [
                new DecreaseAbilityLevel
                {
                    AbilityName = AbilityData.Acrobatics.Name
                }
            ]
        }.AddToAffectable(target);
        manager.Queue.ProcessQueue(manager);
        var effect = Assert.Single(ability.Effects).Effect!;

        Assert.Equal(EffectType.DecreaseAbilityLevel, effect.EffectType);
        Assert.Equal(EffectDuration.Infinite, effect.Duration);
        Assert.Equal("1", effect.Amount);
        Assert.Equal(AbilityData.Acrobatics.Name, effect.TargetName);
        Assert.Null(effect.TargetEntityId);

        var typeSelector = new Ability
        {
            Name = "type selector",
            Effects = [new DecreaseAbilityLevel { AbilityType = AbilityType.Skill }]
        }.AddToAffectable(target);
        var typeEffect = Assert.Single(typeSelector.Effects).Effect!;
        Assert.True(typeEffect.TargetEntityId < 0);
        Assert.Equal(-1 - (int)AbilityType.Skill, typeEffect.TargetEntityId);
    }

    [Fact]
    public void Type_decrease_applies_to_each_matching_ability_and_excludes_its_source()
    {
        var (target, manager) = CreateEffectTarget();
        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 2);
        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Stealth, level: 2);
        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Greased, level: 2);
        manager.Queue.ProcessQueue(manager);

        var suppressor = AddLevelSuppressor(
            target, "skill suppressor", AbilityType.Skill,
            abilityName: null, abilityType: AbilityType.Skill,
            amount: 1, manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.Acrobatics.Name));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.Stealth.Name));
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.Greased.Name));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, suppressor.Name!));
        Assert.True(suppressor.IsUsable);
        var decreaseRecord = Assert.Single(
            target.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel
                      && effect.SourceAbilityEntityId == suppressor.EntityId);
        Assert.Null(decreaseRecord.TargetName);
        Assert.Equal((int)AbilityType.Skill, decreaseRecord.ConditionValue);

        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.WaterSourcery, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(
            target, AbilityData.WaterSourcery.Name));
        Assert.Equal(
            1,
            manager.AffectableAbilitiesIndex[
                (target.Id, AbilityData.WaterSourcery.Name)]!.Ability!.Level);
    }

    [Fact]
    public void Instant_decrease_removes_matching_positive_records_without_persisting_a_decrease()
    {
        var (target, manager) = CreateEffectTarget();
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);
        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 2, firstSource.Id);
        manager.EffectApplicationSystem.AddAbilityEffect(
            target, AbilityData.Acrobatics, level: 1, secondSource.Id);
        manager.Queue.ProcessQueue(manager);

        AddLevelSuppressor(
            target, "instant skill remover", AbilityType.Buff,
            abilityName: AbilityData.Acrobatics.Name, abilityType: null,
            amount: 1, manager, EffectDuration.Instant);

        Assert.DoesNotContain(
            target.ActiveEffects!.ComponentEffects,
            effect => effect.TargetName == AbilityData.Acrobatics.Name);
        Assert.DoesNotContain(
            target.ActiveEffects.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel);
        Assert.Null(manager.AffectableAbilitiesIndex[(target.Id, AbilityData.Acrobatics.Name)]);
        Assert.True(target.Being!.IsAlive);
    }

    [Fact]
    public void Non_instant_decrease_cannot_suppress_every_race()
    {
        var (target, manager) = CreateEffectTarget();
        var raceAbilityName = target.Being!.Races.Single().Ability!.Name!;

        Assert.Throws<InvalidOperationException>(() => AddLevelSuppressor(
            target, "invalid race suppressor", AbilityType.Buff,
            abilityName: null, abilityType: AbilityType.Race,
            amount: 3, manager));

        Assert.True(target.Being.IsAlive);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, raceAbilityName));
        Assert.DoesNotContain(
            target.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel
                      && effect.ConditionValue == (int)AbilityType.Race);
    }

    [Fact]
    public void Instant_decrease_of_the_last_race_kills_the_target()
    {
        var (target, manager) = CreateEffectTarget();
        var being = target.Being!;
        var targetId = target.Id;
        var raceAbilityName = being.Races.Single().Ability!.Name!;

        AddLevelSuppressor(
            target, "instant race remover", AbilityType.Buff,
            abilityName: raceAbilityName, abilityType: null,
            amount: 1, manager, EffectDuration.Instant);

        Assert.False(being.IsAlive);
        Assert.Null(manager.FindEntity(targetId));
    }

    private static AbilityComponent AddLevelSuppressor(
        GameEntity target, string name, AbilityType sourceType,
        string? abilityName, AbilityType? abilityType, int amount, GameManager manager,
        EffectDuration duration = EffectDuration.Infinite)
    {
        var template = new LeveledAbility
        {
            Name = name,
            Type = sourceType,
            LeveledEffects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                {
                    1,
                    new Effect[]
                    {
                        new DecreaseAbilityLevel
                        {
                            TargetActivator = true,
                            AbilityName = abilityName,
                            AbilityType = abilityType,
                            Amount = amount.ToString(CultureInfo.InvariantCulture),
                            Duration = duration
                        }
                    }
                }
            }
        };
        var record = manager.EffectApplicationSystem.AddAbilityEffect(target, template);
        var ability = manager.FindEntity(record.CarrierId)!.Ability!;
        manager.Queue.ProcessQueue(manager);
        return ability;
    }

    public static IEnumerable<object[]> CleansingSubsets()
    {
        foreach (var wet in new[] { false, true })
        {
            foreach (var chilled in new[] { false, true })
            {
                foreach (var greased in new[] { false, true })
                {
                    yield return [wet, chilled, greased];
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(CleansingSubsets))]
    public void Burning_suppression_is_idempotent_for_every_subset_of_opposing_conditions(
        bool wet, bool chilled, bool greased)
    {
        var (target, manager) = CreateEffectTarget();
        if (wet)
        {
            manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
                EffectType.Wet, target, amount: 100));
        }

        if (chilled)
        {
            manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
                EffectType.Chill, target, amount: 100));
        }

        if (greased)
        {
            manager.EffectApplicationSystem.AddAbilityEffect(target, AbilityData.Greased);
        }

        manager.Queue.ProcessQueue(manager);
        var wettedLevel = EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Wetted.Name);
        var chilledLevel = EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Chilled.Name);
        var greasedLevel = EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Greased.Name);
        manager.EffectApplicationSystem.AddAbilityEffect(target, AbilityData.Burning, level: 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            wettedLevel - 2,
            EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Wetted.Name));
        Assert.Equal(
            chilledLevel - 2,
            EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Chilled.Name));
        Assert.Equal(
            greasedLevel - 2,
            EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Greased.Name));
    }

    [Fact]
    public void Per_source_build_up_resets_when_an_instant_application_changes_source()
    {
        var (target, manager) = CreateEffectTarget();
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, amount: 80, firstSource.Id));
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, amount: 120, firstSource.Id));

        var accumulator = target.ActiveEffects!.Accumulators[EffectType.Frighten];
        Assert.Equal(60, accumulator.Value);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, amount: 40, secondSource.Id));

        Assert.Equal(20, accumulator.Value);
        var marker = Assert.Single(target.ActiveEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.Frighten);
        Assert.Equal(secondSource.Id, marker.SourceAbilityEntityId);
    }

    [Fact]
    public void Only_the_strongest_lingering_per_source_build_up_is_applied()
    {
        var (target, manager) = CreateEffectTarget();
        var firstSource = CreateSourceAbility(target);
        var secondSource = CreateSourceAbility(target);
        var weaker = CreatePropertyEffect(target.ActiveEffects!,
            0, 0, 0, EffectType.Frighten, null,
            combinationFunction: ValueCombinationFunction.BuildUpPerSource,
            sourceAbilityEntityId: firstSource.Id,
            magnitudePrimary: 20);
        var stronger = CreatePropertyEffect(target.ActiveEffects!,
            0, 0, 0, EffectType.Frighten, null,
            combinationFunction: ValueCombinationFunction.BuildUpPerSource,
            sourceAbilityEntityId: secondSource.Id,
            magnitudePrimary: 30);
        var activeEffects = target.ActiveEffects!;
        weaker = activeEffects.AddPropertyEffect(weaker);
        stronger = activeEffects.AddPropertyEffect(stronger);

        manager.EffectApplicationSystem.ApplyLingeringBuildUpEffect(
            weaker, target, TimeSystem.DefaultActionDelay, manager);
        manager.EffectApplicationSystem.ApplyLingeringBuildUpEffect(
            stronger, target, TimeSystem.DefaultActionDelay, manager);

        Assert.Equal(30, activeEffects.Accumulators[EffectType.Frighten].Value);
        Assert.Same(
            stronger,
            EffectApplicationSystem.FindPerSourceBuildUpSource(target, EffectType.Frighten));

        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            target.ActiveEffects!, target,
            instance => instance.SourceAbilityEntityId == secondSource.Id, manager);

        Assert.Equal(0, target.ActiveEffects!.Accumulators[EffectType.Frighten].Value);
        Assert.Same(
            weaker,
            EffectApplicationSystem.FindPerSourceBuildUpSource(target, EffectType.Frighten));
    }

    [Fact]
    public void Per_source_status_is_removed_with_its_marker_and_can_reactivate()
    {
        var (target, manager) = CreateEffectTarget();
        var source = CreateSourceAbility(target);

        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, 6 * EffectApplicationSystem.BuildUpThreshold, source.Id));
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, 2 * EffectApplicationSystem.BuildUpThreshold, source.Id));
        Assert.Equal(4, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Frightened.Name));
        Assert.True(EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Frightened.Name) > 0);

        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            target.ActiveEffects!, target,
            instance => instance.SourceAbilityEntityId == source.Id, manager);

        var accumulator = target.ActiveEffects!.Accumulators[EffectType.Frighten];
        Assert.Equal(0, accumulator.Value);
        Assert.False(EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Frightened.Name) > 0);

        var replacementSource = CreateSourceAbility(target);
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Frighten, target, amount: 240, replacementSource.Id));

        Assert.Equal(20, accumulator.Value);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Frightened.Name));
        Assert.True(EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Frightened.Name) > 0);
    }

    [Fact]
    public void Instant_per_source_application_preserves_a_matching_lingering_marker()
    {
        var (target, manager) = CreateEffectTarget();
        var source = CreateSourceAbility(target);

        ApplyAuthoredStatus(
            target, source, EffectType.Frighten, amount: 30,
            EffectDuration.UntilTimeout, durationAmount: "50", manager);

        var marker = Assert.Single(target.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.Frighten);
        var magnitude = marker.MagnitudePrimary;
        var expirationTick = marker.ExpirationTarget;

        ApplyAuthoredStatus(
            target, source, EffectType.Frighten, amount: 10,
            EffectDuration.Instant, durationAmount: null, manager);

        Assert.Same(marker, Assert.Single(target.ActiveEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.Frighten));
        Assert.Equal(magnitude, marker.MagnitudePrimary);
        Assert.Equal(expirationTick, marker.ExpirationTarget);
    }

    private static void ApplyAuthoredStatus(
        GameEntity target, GameEntity sourceAbility, EffectType effectType, int amount,
        EffectDuration duration, string? durationAmount, GameManager manager)
    {
        TestHelper.ApplyEffect(effectType, target, target, manager, effect =>
        {
            effect.ContainingAbilityId = sourceAbility.Id;
            effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
            effect.Duration = duration;
            effect.DurationAmount = durationAmount;
        });
        manager.Queue.ProcessQueue(manager);
    }

    private static void ApplyFire(GameEntity target, GameManager manager)
    {
        TestHelper.ApplyEffect(EffectType.FireDamage, target, target, manager, effect =>
        {
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "10";
        });
        manager.Queue.ProcessQueue(manager);
    }

    [Fact]
    public void Unsupported_effect_type_throws_at_runtime()
    {
        var room = TestHelper.BuildRoom(".");
        var target = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = target.Manager;
        manager.Queue.ProcessQueue(manager);

        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.DisableItem;
        effect.Duration = EffectDuration.Instant;
        effectReference.Referenced.Effect = effect;

        var exception = Assert.Throws<NotSupportedException>(
            () => TestHelper.ApplyEffect(effectReference.Referenced, target, target, manager));
        Assert.Contains(nameof(EffectType.DisableItem), exception.Message);
    }

    [Theory]
    [InlineData(EffectType.Decay)]
    [InlineData(EffectType.Enervate)]
    public void Source_bound_non_buildup_status_cannot_be_instant(EffectType effectType)
    {
        var (target, manager) = CreateEffectTarget();
        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = effectType;
        effect.Duration = EffectDuration.Instant;
        effect.Amount = "5";
        effectReference.Referenced.Effect = effect;

        var exception = Assert.Throws<NotSupportedException>(
            () => TestHelper.ApplyEffect(effectReference.Referenced, target, target, manager));
        Assert.Contains($"Instant effect type {effectType}", exception.Message);
    }

    [Fact]
    public void Instant_buildup_contribution_decays_while_its_timed_status_remains_active()
    {
        var (target, manager) = CreateEffectTarget();

        ApplyInstantBuildup(amount: 100);

        var accumulator = target.ActiveEffects!.Accumulators[EffectType.Paralyze];
        Assert.Equal(50, accumulator.Value);

        manager.EffectApplicationSystem.RealizeBuildUps(
            target, TimeSystem.DefaultActionDelay, manager);
        Assert.Equal(40, accumulator.Value);

        ApplyInstantBuildup(amount: 220);
        Assert.Equal(50, accumulator.Value);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Paralyzed.Name));
        Assert.True(EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Paralyzed.Name) > 0);

        manager.EffectApplicationSystem.RealizeBuildUps(
            target, TimeSystem.DefaultActionDelay, manager);
        Assert.Equal(30, accumulator.Value);
        Assert.True(EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Paralyzed.Name) > 0);

        void ApplyInstantBuildup(int amount)
        {
            using var effectReference = manager.CreateEntity();
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Paralyze;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = amount.ToString(CultureInfo.InvariantCulture);
            effectReference.Referenced.Effect = effect;

            TestHelper.ApplyEffect(effectReference.Referenced, target, target, manager);
            manager.Queue.ProcessQueue(manager);
        }
    }

    [Fact]
    public void Active_effect_add_methods_construct_return_and_identify_owned_instances()
    {
        var level = TestHelper.BuildRoom(".");
        var owner = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var activeEffects = owner.ActiveEffects!;
        var firstPropertyId = activeEffects.NextPropertyEffectId;
        var firstComponentId = activeEffects.NextComponentEffectId;

        var firstProperty = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum, magnitudePrimary: 2);
        var secondProperty = activeEffects.AddPropertyEffect(
            EffectType.Polymorph, expirationTarget: 10,
            expirationEvent: EffectDuration.UntilTimeout);
        var firstComponent = activeEffects.AddComponentEffect(
            EffectType.AddAbility, "test ability", level: 2);
        var secondComponent = activeEffects.AddComponentEffect(
            EffectType.DecreaseAbilityLevel, targetName: null,
            conditionValue: (int)AbilityType.Debuff);

        Assert.Contains(activeEffects.PropertyEffects, effect => ReferenceEquals(effect, firstProperty));
        Assert.Contains(activeEffects.PropertyEffects, effect => ReferenceEquals(effect, secondProperty));
        Assert.Contains(activeEffects.ComponentEffects, effect => ReferenceEquals(effect, firstComponent));
        Assert.Contains(activeEffects.ComponentEffects, effect => ReferenceEquals(effect, secondComponent));
        Assert.All(
            new ActiveEffect[] { firstProperty, secondProperty, firstComponent, secondComponent },
            effect =>
            {
                Assert.Equal(activeEffects.GameId, effect.GameId);
                Assert.Equal(activeEffects.EntityId, effect.EntityId);
            });
        Assert.Equal(firstPropertyId, firstProperty.InstanceId);
        Assert.Equal(firstPropertyId + 1, secondProperty.InstanceId);
        Assert.Equal(firstComponentId, firstComponent.InstanceId);
        Assert.Equal(firstComponentId + 1, secondComponent.InstanceId);
        Assert.Equal(nameof(BeingComponent.Might), firstProperty.TargetName);
        Assert.Equal(2, firstComponent.Level);
        Assert.Equal((int)AbilityType.Debuff, secondComponent.ConditionValue);
    }

    [Fact]
    public void Active_effect_instance_ids_remain_strictly_increasing_after_removing_the_highest_entry()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var activeEffects = entity.ActiveEffects;
        var firstProperty = activeEffects.AddPropertyEffect(EffectType.Default);
        var middleProperty = activeEffects.AddPropertyEffect(EffectType.Default);
        var lastProperty = activeEffects.AddPropertyEffect(EffectType.Default);
        activeEffects.PropertyEffects.Remove(lastProperty);

        var addedProperty = activeEffects.AddPropertyEffect(EffectType.Default);

        Assert.Equal(3, addedProperty.InstanceId);
        Assert.Equal(
            activeEffects.PropertyEffects.Count,
            activeEffects.PropertyEffects.Select(effect => effect.InstanceId).Distinct().Count());

        var firstComponent = activeEffects.AddComponentEffect(EffectType.AddAbility, "first");
        var middleComponent = activeEffects.AddComponentEffect(EffectType.AddAbility, "middle");
        var lastComponent = activeEffects.AddComponentEffect(EffectType.AddAbility, "last");
        activeEffects.ComponentEffects.Remove(lastComponent);

        var addedComponent = activeEffects.AddComponentEffect(EffectType.AddAbility, "added");

        Assert.Equal(3, addedComponent.InstanceId);
        Assert.Equal(
            activeEffects.ComponentEffects.Count,
            activeEffects.ComponentEffects.Select(effect => effect.InstanceId).Distinct().Count());
    }

    [Fact]
    public void Active_effect_collection_changes_can_be_subscribed_to_before_entity_creation()
    {
        var manager = TestHelper.CreateGameManager();
        var listener = new ActiveEffectCollectionListener();
        manager.AffectableEntities.AddListener(listener);

        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

        var propertyEffect = entity.ActiveEffects.AddPropertyEffect(EffectType.Stun);
        Assert.Same(Effect.GetMetadata(EffectType.Stun), propertyEffect.Metadata);
        Assert.Same(entity.ActiveEffects, propertyEffect.ContainingComponent);
        propertyEffect.MagnitudePrimary = 10;
        entity.ActiveEffects.PropertyEffects.Remove(propertyEffect);
        Assert.Null(propertyEffect.ContainingComponent);
        propertyEffect.MagnitudePrimary = 20;

        var componentEffect = entity.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, "test");
        Assert.Same(entity.ActiveEffects, componentEffect.ContainingComponent);
        componentEffect.Level = 2;
        entity.ActiveEffects.ComponentEffects.Remove(componentEffect);
        Assert.Null(componentEffect.ContainingComponent);
        componentEffect.Level = 3;

        var accumulator = CreateAccumulator(entity.ActiveEffects, EffectType.Paralyze);
        entity.ActiveEffects.Accumulators.Add(accumulator);
        Assert.Same(entity.ActiveEffects, accumulator.ContainingComponent);
        entity.ActiveEffects.Accumulators.Remove(accumulator);
        Assert.Null(accumulator.ContainingComponent);

        Assert.Equal(
            [
                (nameof(ActiveEffectsComponent.PropertyEffects), EntityCollectionChangeAction.Add, null),
                (nameof(ActiveEffectsComponent.PropertyEffects), EntityCollectionChangeAction.None,
                    nameof(ActivePropertyEffect.MagnitudePrimary)),
                (nameof(ActiveEffectsComponent.PropertyEffects), EntityCollectionChangeAction.Remove, null),
                (nameof(ActiveEffectsComponent.ComponentEffects), EntityCollectionChangeAction.Add, null),
                (nameof(ActiveEffectsComponent.ComponentEffects), EntityCollectionChangeAction.None,
                    nameof(ActiveComponentEffect.Level)),
                (nameof(ActiveEffectsComponent.ComponentEffects), EntityCollectionChangeAction.Remove, null)
            ],
            listener.Changes);
        Assert.Contains(
            (nameof(ActiveEffectsComponent.PropertyEffects), nameof(ActivePropertyEffect.MagnitudePrimary), 0, 10),
            listener.ValueChanges);
        Assert.Contains(
            (nameof(ActiveEffectsComponent.ComponentEffects), nameof(ActiveComponentEffect.Level), 1, 2),
            listener.ValueChanges);
    }

    [Fact]
    public void Effect_iteration_does_not_skip_entries_removed_during_processing()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var activeEffects = entity.ActiveEffects;
        activeEffects.AddPropertyEffect(CreatePropertyEffect(activeEffects, 0, 0, 0, EffectType.Heal, null));
        activeEffects.AddPropertyEffect(CreatePropertyEffect(activeEffects, 0, 0, 0, EffectType.BleedDamage, null));
        activeEffects.AddPropertyEffect(CreatePropertyEffect(activeEffects, 0, 0, 0, EffectType.Stun, null));
        var processed = new List<EffectType>();

        ActiveEffectsComponent.ForEachEffect(
            activeEffects.PropertyEffects,
            (ActiveEffects: activeEffects, Processed: processed),
            static (effect, state) =>
            {
                state.Processed.Add(effect.EffectType);
                state.ActiveEffects.PropertyEffects.Remove(effect);
            });

        Assert.Equal([EffectType.Heal, EffectType.BleedDamage, EffectType.Stun], processed);
        Assert.Empty(activeEffects.PropertyEffects);
    }

    [Fact]
    public void Effect_collections_manage_item_ownership_on_replace_and_clear()
    {
        var manager = TestHelper.CreateGameManager();
        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        entity.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        var activeEffects = entity.ActiveEffects;
        var first = CreatePropertyEffect(activeEffects, 0, 0, 0, EffectType.Default, null);
        var replacement = CreatePropertyEffect(activeEffects, 0, 0, 0, EffectType.Default, null);

        activeEffects.PropertyEffects.Add(first);
        activeEffects.PropertyEffects[0] = replacement;

        Assert.Null(first.ContainingComponent);
        Assert.Null(first.ContainingPropertyName);
        Assert.Same(activeEffects, replacement.ContainingComponent);
        Assert.Equal(nameof(ActiveEffectsComponent.PropertyEffects), replacement.ContainingPropertyName);

        activeEffects.PropertyEffects.Clear();

        Assert.Null(replacement.ContainingComponent);
        Assert.Null(replacement.ContainingPropertyName);

        var firstAccumulator = CreateAccumulator(activeEffects, EffectType.Paralyze);
        var replacementAccumulator = CreateAccumulator(activeEffects, EffectType.Paralyze);

        activeEffects.Accumulators.Add(firstAccumulator);
        ((IList<EffectAccumulator>)activeEffects.Accumulators)[0] = replacementAccumulator;

        Assert.Null(firstAccumulator.ContainingComponent);
        Assert.Null(firstAccumulator.ContainingPropertyName);
        Assert.Same(activeEffects, replacementAccumulator.ContainingComponent);
        Assert.Equal(nameof(ActiveEffectsComponent.Accumulators), replacementAccumulator.ContainingPropertyName);

        activeEffects.Accumulators.Clear();

        Assert.Null(replacementAccumulator.ContainingComponent);
        Assert.Null(replacementAccumulator.ContainingPropertyName);
    }

    private sealed class ActiveEffectCollectionListener : IEntityChangeListener<GameEntity>
    {
        public List<(string Collection, EntityCollectionChangeAction Action, string? Property)> Changes { get; } = [];
        public List<(string Collection, string Property, int OldValue, int NewValue)> ValueChanges { get; } = [];

        public void OnEntityAdded(in EntityChange<GameEntity> entityChange)
        {
        }

        public void OnEntityRemoved(in EntityChange<GameEntity> entityChange)
        {
        }

        public void OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        {
            var changes = entityChange.PropertyChanges;
            for (var i = 0; i < changes.Count; i++)
            {
                var propertyName = changes.GetChangedPropertyName(i);
                if (propertyName is not (
                        nameof(ActiveEffectsComponent.PropertyEffects)
                        or nameof(ActiveEffectsComponent.ComponentEffects)))
                {
                    continue;
                }

                if (changes.GetNestedPropertyName(i) is { } nestedPropertyName)
                {
                    Changes.Add((propertyName, EntityCollectionChangeAction.None, nestedPropertyName));
                    ValueChanges.Add((
                        propertyName,
                        nestedPropertyName,
                        changes.GetValue<int>(i, Utils.MessagingECS.ValueType.Old),
                        changes.GetValue<int>(i, Utils.MessagingECS.ValueType.Current)));
                    continue;
                }

                var oldChange = changes.GetValue<EntityCollectionChange>(i, Utils.MessagingECS.ValueType.Old);
                var newChange = changes.GetValue<EntityCollectionChange>(i, Utils.MessagingECS.ValueType.Current);
                var change = oldChange.Action != EntityCollectionChangeAction.None ? oldChange : newChange;
                Changes.Add((propertyName, change.Action, null));
            }
        }
    }

    [Fact]
    public void Instant_sum_resource_effects_are_accumulated_before_application()
    {
        var room = TestHelper.BuildRoom(".");
        var target = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = target.Manager;
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
            target, EffectType.ToxinDamage, ratePerAction: 200, duration: 100, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(target, AbilityData.Blighted.Name));

        target.Being!.HitPoints -= 10;
        var hitPointsBefore = target.Being.HitPoints;

        using var abilityReference = manager.CreateEntity();
        var abilityEntity = abilityReference.Referenced;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "combined heal";
        ability.Activation = ActivationType.Manual;
        ability.OwnerEntity = target;
        abilityEntity.Ability = ability;

        var healingEffects = new List<GameEntity>();
        for (var i = 0; i < 2; i++)
        {
            using var effectReference = manager.CreateEntity();
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.CombinationFunction = ValueCombinationFunction.Sum;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "1";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectEntity.Effect = effect;
            healingEffects.Add(effectEntity);
        }

        var message = AbilityActivatedMessage.Create(manager);
        message.AbilityEntity = abilityEntity;
        message.ActivatorEntity = target;
        message.TargetEntity = target;
        message.EffectsToApply = [.. healingEffects];
        message.Outcome = ApplicationOutcome.Success;
        manager.Enqueue(message);
        manager.Queue.ProcessQueue(manager);

        // Blighted halves healing with integer arithmetic. Applying each Heal(1) separately yields zero;
        // accumulating them first yields Heal(2), which restores one hit point.
        Assert.Equal(hitPointsBefore + 1, target.Being.HitPoints);
    }

    [Fact]
    public void Same_type_damage_effects_are_accumulated_before_resistance()
    {
        var room = TestHelper.BuildRoom(".");
        var target = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = target.Manager;
        manager.Queue.ProcessQueue(manager);

        var resistance = target.Being!.FireResistance + target.Being.MagicResistance;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            50 - resistance, nameof(BeingComponent.FireResistance), "test fire resistance", target);
        manager.Queue.ProcessQueue(manager);

        using var abilityReference = manager.CreateEntity();
        var abilityEntity = abilityReference.Referenced;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "combined fire damage";
        ability.Action = AbilityAction.Hit;
        ability.Activation = ActivationType.Manual;
        ability.OwnerEntity = target;
        abilityEntity.Ability = ability;

        var damageEffects = new List<GameEntity>();
        for (var i = 0; i < 2; i++)
        {
            using var effectReference = manager.CreateEntity();
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.FireDamage;
            effect.CombinationFunction = ValueCombinationFunction.Subtract;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "1";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectEntity.Effect = effect;
            damageEffects.Add(effectEntity);
        }

        var hitPointsBefore = target.Being.HitPoints;
        var message = AbilityActivatedMessage.Create(manager);
        message.AbilityEntity = abilityEntity;
        message.ActivatorEntity = target;
        message.TargetEntity = target;
        message.EffectsToApply = [.. damageEffects];
        message.Outcome = ApplicationOutcome.Success;
        manager.Enqueue(message);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(hitPointsBefore - 1, target.Being.HitPoints);
    }

    [Fact]
    public void Subtract_removes_the_contribution_from_the_running_value()
    {
        var value = 100;
        var runningSum = 100;
        var summandCount = 1;

        PropertyValueCombiner.Apply(
            20, ValueCombinationFunction.Subtract, ref value, ref runningSum, ref summandCount);

        Assert.Equal(80, value);
    }

    [Fact]
    public void Effects_attached_before_targeting_apply_on_activation_and_deactivation_removes_their_contributions()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        using var abilityEntityReference = manager.CreateEntity();
        var toggledAbility = abilityEntityReference.Referenced;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Activation = ActivationType.WhileToggled;
        toggledAbility.Ability = ability;

        GameEntity acidEffectEntity;
        using (var effectEntityReference = manager.CreateEntity())
        {
            acidEffectEntity = effectEntityReference.Referenced;

            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.AcidResistance);
            effect.Amount = "10";
            effect.SecondaryAmount = "11";
            effect.ContainingAbilityId = toggledAbility.Id;

            acidEffectEntity.Effect = effect;
        }

        using (var effectEntityReference = manager.CreateEntity())
        {
            var coldEffectEntity = effectEntityReference.Referenced;

            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.ColdResistance);
            effect.Amount = "10";
            effect.ContainingAbilityId = toggledAbility.Id;

            coldEffectEntity.Effect = effect;
        }

        ability.OwnerEntity = playerEntity;

        Assert.Null(toggledAbility.Ability.Slot);
        Assert.False(toggledAbility.Ability.IsActive);
        Assert.False(playerEntity.Being!.SlottedAbilities.ContainsKey(2));
        Assert.Equal(0, playerEntity.Being.AcidResistance);
        Assert.Equal(0, playerEntity.Being.ColdResistance);

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, toggledAbility.Ability.Slot);
        Assert.True(toggledAbility.Ability.IsActive);
        Assert.Same(toggledAbility, playerEntity.Being.SlottedAbilities[2]);
        Assert.Equal(10, playerEntity.Being.AcidResistance);
        Assert.Equal(10, playerEntity.Being.ColdResistance);
        Assert.Equal(
            11,
            Assert.Single(
                playerEntity.ActiveEffects!.PropertyEffects,
                effect => effect.TargetName == nameof(BeingComponent.AcidResistance))
                .MagnitudeSecondary);

        var setSlotMessage = SetAbilitySlotMessage.Create(manager);
        setSlotMessage.AbilityEntity = toggledAbility;

        manager.Enqueue(setSlotMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(toggledAbility.Ability.Slot);
        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(0, playerEntity.Being.AcidResistance);
        Assert.Equal(0, playerEntity.Being.ColdResistance);
    }

    [Fact]
    public void Continuous_heal_restores_health_over_time_and_stops_when_deactivated()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        being.HitPoints = 1;

        GameEntity healingAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            healingAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test regeneration";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = playerEntity;
            healingAbility.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.Duration = EffectDuration.Infinite;
            effect.Amount = "20";
            effect.ContainingAbilityId = healingAbility.Id;
            effectReference.Referenced.Effect = effect;
        }

        TestHelper.ActivateAbility(healingAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, being.HitPoints);
        Assert.Contains(
            playerEntity.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.Heal
                 && Effect.GetMetadata(i.EffectType).ResourceProperty != null
                 && i.SourceAbilityEntityId == healingAbility.Id);

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            playerEntity, TimeSystem.DefaultActionDelay);

        Assert.Equal(21, being.HitPoints);

        var clearSlot = SetAbilitySlotMessage.Create(manager);
        clearSlot.AbilityEntity = healingAbility;
        manager.Enqueue(clearSlot);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            playerEntity.ActiveEffects.PropertyEffects,
            i => i.EffectType == EffectType.Heal
                  && Effect.GetMetadata(i.EffectType).ResourceProperty != null);
    }

    [Theory]
    [InlineData(
        EffectType.DrainLife, nameof(BeingComponent.HitPoints), nameof(BeingComponent.HitPointMaximum),
        nameof(BeingComponent.BleedingResistance))]
    [InlineData(
        EffectType.DrainEnergy, nameof(BeingComponent.EnergyPoints), nameof(BeingComponent.EnergyPointMaximum),
        nameof(BeingComponent.VoidResistance))]
    public void Timed_drain_restores_the_activator_on_turn_and_expiry_pulses(
        EffectType effectType, string resourceProperty, string maximumProperty, string resistanceProperty)
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var activatorEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var targetEntity = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, maximumProperty, "test activator pool", activatorEntity);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, maximumProperty, "test target pool", targetEntity);
        var resistance = targetEntity.ActiveEffects!.ResolveInt(resistanceProperty)
                         + targetEntity.Being!.MagicResistance;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -resistance, resistanceProperty, "test drain resistance", targetEntity);
        manager.Queue.ProcessQueue(manager);

        SetPool(activatorEntity, 1);
        SetPool(targetEntity, 100);

        GameEntity effectEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            var abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = activatorEntity;
            abilityEntity.Ability = ability;

            using var effectReference = manager.CreateEntity();
            effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = effectType;
            effect.Duration = EffectDuration.UntilTimeout;
            effect.DurationAmount = "150";
            effect.Amount = "20";
            effect.SecondaryAmount = "13";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectEntity.Effect = effect;
        }

        var startTick = manager.Game.CurrentTick;
        TestHelper.ApplyEffect(effectEntity, activatorEntity, targetEntity, manager);
        manager.Queue.ProcessQueue(manager);

        var instance = Assert.Single(targetEntity.ActiveEffects!.PropertyEffects,
            i => i.EffectType == effectType
                 && Effect.GetMetadata(i.EffectType).ResourceProperty != null);
        Assert.Equal(effectEntity.Effect!.ContainingAbilityId, instance.SourceAbilityEntityId);
        Assert.Equal(13, instance.MagnitudeSecondary);

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            targetEntity, TimeSystem.DefaultActionDelay);

        Assert.Equal(80, GetPool(targetEntity));
        Assert.Equal(21, GetPool(activatorEntity));

        targetEntity.AI!.PreviousActionTick = startTick + TimeSystem.DefaultActionDelay;
        manager.Game.CurrentTick = startTick + 150;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(targetEntity, manager);

        Assert.Equal(70, GetPool(targetEntity));
        Assert.Equal(31, GetPool(activatorEntity));

        int GetPool(GameEntity entity)
            => resourceProperty == nameof(BeingComponent.HitPoints)
                ? entity.Being!.HitPoints
                : entity.Being!.EnergyPoints;

        void SetPool(GameEntity entity, int value)
        {
            if (resourceProperty == nameof(BeingComponent.HitPoints))
            {
                entity.Being!.HitPoints = value;
            }
            else
            {
                entity.Being!.EnergyPoints = value;
            }
        }
    }

    [Fact]
    public void Independent_periodic_resources_stack_as_separate_instances()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
            target, EffectType.FireDamage, ratePerAction: 5, duration: 100, manager);
        EffectTestHelper.ApplyDamageOverTimeEffect(
            target, EffectType.FireDamage, ratePerAction: 7, duration: 200, manager);

        var instances = target.ActiveEffects!.PropertyEffects
            .Where(i => i.EffectType == EffectType.FireDamage)
            .ToList();
        Assert.Equal(DurationPolicy.Independent, Effect.GetMetadata(EffectType.FireDamage).DurationPolicy);
        Assert.Equal(2, instances.Count);
        Assert.Equal(new[] { 5, 7 }, instances.Select(i => i.MagnitudePrimary).Order().ToArray());
    }

    [Fact]
    public void Periodic_resource_change_is_not_reported_as_immediate_damage()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.FireDamage;
        effect.CombinationFunction = ValueCombinationFunction.Subtract;
        effect.Duration = EffectDuration.Infinite;
        effectReference.Referenced.Effect = effect;

        var result = AttackResultMessage.Create(manager);
        var hitPointsBefore = target.Being!.HitPoints;
        manager.EffectApplicationSystem.ApplyResourceEffect(
            effect, target, target, rawAmount: 20, result, manager, pretend: false);

        Assert.Empty(result.ResourceChanges);
        Assert.Equal(hitPointsBefore, target.Being.HitPoints);

        var activeEffect = Assert.Single(
            target.ActiveEffects!.PropertyEffects,
            activeEffect => activeEffect.EffectType == EffectType.FireDamage);
        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            target, TimeSystem.DefaultActionDelay);

        Assert.Equal(hitPointsBefore - activeEffect.MagnitudePrimary, target.Being.HitPoints);
        result.ReturnMessage(manager);
    }

    [Fact]
    public void Periodic_resource_copies_the_effect_combination_function()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
            target, EffectType.FireDamage, ratePerAction: 5, duration: 100, manager,
            combinationFunction: ValueCombinationFunction.Sum);

        var instance = Assert.Single(target.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.FireDamage);
        Assert.Equal(ValueCombinationFunction.Sum, instance.CombinationFunction);

        target.Being!.HitPoints = target.Being.HitPointMaximum - 10;
        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            target, TimeSystem.DefaultActionDelay);

        Assert.Equal(target.Being.HitPointMaximum - 5, target.Being.HitPoints);
    }

    [Fact]
    public void Independent_periodic_heals_keep_each_rate_and_expiry()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        GameEntity abilityEntity;
        GameEntity effectEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.Manual;
            ability.OwnerEntity = target;
            abilityEntity.Ability = ability;

            using var effectReference = manager.CreateEntity();
            effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.Duration = EffectDuration.UntilTimeout;
            effect.DurationAmount = "100";
            effect.Amount = "10";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectEntity.Effect = effect;
        }

        TestHelper.ApplyEffect(effectEntity, target, target, manager);
        manager.Queue.ProcessQueue(manager);

        var instance = Assert.Single(target.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.Heal);
        instance.ExpirationEvent = EffectDuration.UntilTimeout;
        instance.ExpirationTarget = manager.Game.CurrentTick + 200;
        effectEntity.Effect = null;
        using (var effectReference = manager.CreateEntity())
        {
            effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.Duration = EffectDuration.UntilTimeout;
            effect.DurationAmount = "100";
            effect.Amount = "20";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectEntity.Effect = effect;
        }

        TestHelper.ApplyEffect(effectEntity, target, target, manager);
        manager.Queue.ProcessQueue(manager);

        var instances = target.ActiveEffects.PropertyEffects
            .Where(i => i.EffectType == EffectType.Heal)
            .OrderBy(i => i.MagnitudePrimary)
            .ToList();
        Assert.Equal(DurationPolicy.Independent, Effect.GetMetadata(EffectType.Heal).DurationPolicy);
        Assert.Equal(2, instances.Count);
        Assert.Same(instance, instances[0]);
        Assert.Equal(10, instances[0].MagnitudePrimary);
        Assert.Equal(manager.Game.CurrentTick + 200, instances[0].ExpirationTarget);
        Assert.Equal(20, instances[1].MagnitudePrimary);
        Assert.Equal(manager.Game.CurrentTick + 100, instances[1].ExpirationTarget);
        Assert.All(instances, effect => Assert.Null(effect.SourceAbilityEntityId));
    }

    [Fact]
    public void Fully_resisted_damage_neither_inflicts_nor_clears_statuses()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var targetEntity = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
            targetEntity, EffectType.FireDamage, ratePerAction: 5, duration: 1000, manager);
        Assert.Contains(targetEntity.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.FireDamage
                 && Effect.GetMetadata(i.EffectType).ResourceProperty != null);

        var resistance = targetEntity.Being!.ColdResistance + targetEntity.Being.MagicResistance;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            100 - resistance, nameof(BeingComponent.ColdResistance), "test cold immunity", targetEntity);
        manager.Queue.ProcessQueue(manager);

        var hitPoints = targetEntity.Being.HitPoints;
        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ColdDamage;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "20";
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, targetEntity, targetEntity, manager);
        }
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(hitPoints, targetEntity.Being.HitPoints);
        Assert.False(targetEntity.Being.HasEffectOrAccumulator(EffectType.Chill));
        Assert.Contains(targetEntity.ActiveEffects.PropertyEffects,
            i => i.EffectType == EffectType.FireDamage
                 && Effect.GetMetadata(i.EffectType).ResourceProperty != null);
    }

    [Fact]
    public void Status_resistance_combines_specific_category_and_nonnegative_penetration()
    {
        var level = TestHelper.BuildRoom("....");
        var manager = level.Entity.Manager;
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var resistantTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        var penetratedTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        var vulnerableTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(3, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(50, resistantTarget.Being!.PhysicalStatusResistance);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.BindResistance), "test bind resistance", resistantTarget);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.BindResistance), "test bind resistance", penetratedTarget);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -100, nameof(BeingComponent.BindResistance), "test bind vulnerability", vulnerableTarget);
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = CreateAbility(manager, source, "test bind");
        var plainBind = CreateBindEffect(secondaryAmount: null);
        var penetratingBind = CreateBindEffect(secondaryAmount: "25");

        TestHelper.ApplyEffect(plainBind, source, resistantTarget, manager);
        TestHelper.ApplyEffect(penetratingBind, source, penetratedTarget, manager);
        TestHelper.ApplyEffect(penetratingBind, source, vulnerableTarget, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(50, resistantTarget.ActiveEffects!.Accumulators[EffectType.Bind].Value);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(penetratedTarget, AbilityData.Bound.Name));
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(vulnerableTarget, AbilityData.Bound.Name));

        GameEntity CreateBindEffect(string? secondaryAmount)
        {
            using var effectReference = manager.CreateEntity();
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Bind;
            effect.Amount = "200";
            effect.SecondaryAmount = secondaryAmount;
            effect.ContainingAbilityId = sourceAbility.Id;
            effectEntity.Effect = effect;
            manager.Queue.ProcessQueue(manager);
            return effectEntity;
        }
    }

    [Fact]
    public void Topple_resistance_and_penetration_control_unstable_levels()
    {
        var level = TestHelper.BuildRoom("....");
        var manager = level.Entity.Manager;
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var resistantTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        var penetratedTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(2, 0));
        var vulnerableTarget = CreatureData.HumanWarrior.Instantiate(level, new Point(3, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.ToppleResistance), "test topple resistance", resistantTarget);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            25, nameof(BeingComponent.ToppleResistance), "test topple resistance", penetratedTarget);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -100, nameof(BeingComponent.ToppleResistance), "test topple vulnerability", vulnerableTarget);
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = CreateAbility(manager, source, "test topple");
        var plainTopple = CreateToppleEffect(secondaryAmount: null);
        var penetratingTopple = CreateToppleEffect(secondaryAmount: "25");

        TestHelper.ApplyEffect(plainTopple, source, resistantTarget, manager);
        TestHelper.ApplyEffect(penetratingTopple, source, penetratedTarget, manager);
        TestHelper.ApplyEffect(penetratingTopple, source, vulnerableTarget, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(50, resistantTarget.ActiveEffects!.Accumulators[EffectType.Topple].Value);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(resistantTarget, AbilityData.Unstable.Name));
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(penetratedTarget, AbilityData.Unstable.Name));
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(vulnerableTarget, AbilityData.Unstable.Name));
        Assert.Equal(0, vulnerableTarget.ActiveEffects!.Accumulators[EffectType.Topple].Value);

        GameEntity CreateToppleEffect(string? secondaryAmount)
        {
            using var effectReference = manager.CreateEntity();
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Topple;
            effect.Amount = "200";
            effect.SecondaryAmount = secondaryAmount;
            effect.ContainingAbilityId = sourceAbility.Id;
            effectEntity.Effect = effect;
            manager.Queue.ProcessQueue(manager);
            return effectEntity;
        }
    }

    [Fact]
    public void Physical_damage_secondary_amount_does_not_penetrate_physical_resistance()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var source = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            50 - target.Being!.PhysicalResistance,
            nameof(BeingComponent.PhysicalResistance), "test physical resistance", target);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -target.Being.Armor, nameof(BeingComponent.Armor), "remove test armor", target);
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = CreateAbility(manager, source, "test physical damage");
        using var effectReference = manager.CreateEntity();
        var effectEntity = effectReference.Referenced;
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.PhysicalDamage;
        effect.Amount = "20";
        effect.SecondaryAmount = "100";
        effect.ContainingAbilityId = sourceAbility.Id;
        effectEntity.Effect = effect;
        manager.Queue.ProcessQueue(manager);

        var hitPoints = target.Being.HitPoints;
        TestHelper.ApplyEffect(effectEntity, source, target, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(hitPoints - 10, target.Being.HitPoints);
    }

    [Fact]
    public void Fully_resisted_periodic_damage_does_not_apply_its_status_ability()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var targetEntity = CreatureData.HumanWarrior.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var resistance = targetEntity.Being!.FireResistance + targetEntity.Being.MagicResistance;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            100 - resistance, nameof(BeingComponent.FireResistance), "test fire immunity", targetEntity);
        manager.Queue.ProcessQueue(manager);

        EffectTestHelper.ApplyDamageOverTimeEffect(
            targetEntity, EffectType.FireDamage, ratePerAction: 20, duration: 1000, manager);

        Assert.DoesNotContain(targetEntity.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.FireDamage
                 && Effect.GetMetadata(i.EffectType).ResourceProperty != null);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(
            targetEntity, Effect.GetMetadata(EffectType.FireDamage).StatusAbilityName!));
    }

    [Fact]
    public void Deactivating_a_continuous_ability_removes_every_instance_it_sourced()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var baseAcid = playerEntity.Being!.AcidResistance;

        GameEntity toggledAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            toggledAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test aegis";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = playerEntity;
            toggledAbility.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.AcidResistance);
            effect.Amount = "10";
            effect.ContainingAbilityId = toggledAbility.Id;
            effectReference.Referenced.Effect = effect;
        }

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        // The equip/toggle modifier lives as a source-linked ActivePropertyEffect instance (retirement 2a),
        // not a cloned applied effect entity.
        Assert.Equal(baseAcid + 10, playerEntity.Being.AcidResistance);
        Assert.Contains(playerEntity.ActiveEffects!.PropertyEffects,
            i => i.EffectType == EffectType.ChangeProperty
                 && i.TargetName == nameof(BeingComponent.AcidResistance)
                 && i.SourceAbilityEntityId == toggledAbility.Id);

        // A second, non-ChangeProperty instance the same ability sourced: retirement 8's deactivation
        // teardown drops every instance kind by source ability, not only ChangeProperty (the narrow
        // predecessor would have left this behind). It has no TargetName, so the resolver ignores it
        // while it lingers.
        playerEntity.ActiveEffects.PropertyEffects.Add(CreatePropertyEffect(playerEntity.ActiveEffects,
            0, 0, playerEntity.ActiveEffects.PropertyEffects.Count, EffectType.Confuse, null,
            sourceAbilityEntityId: toggledAbility.Id));

        var deactivateMessage = DeactivateAbilityMessage.Create(
            toggledAbility.Ability, playerEntity, manager);
        manager.Enqueue(deactivateMessage);
        manager.Queue.ProcessQueue(manager);

        // Turning the ability off reverts its resolved contribution and removes every instance it sourced.
        Assert.False(toggledAbility.Ability.IsActive);
        Assert.Equal(baseAcid, playerEntity.Being.AcidResistance);
        Assert.DoesNotContain(playerEntity.ActiveEffects.PropertyEffects,
            i => i.SourceAbilityEntityId == toggledAbility.Id);
    }

    [Fact]
    public void Instant_damage_applies_directly_without_a_clone()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var hitPointsBefore = playerEntity.Being!.HitPoints;

        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.PhysicalDamage;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "50";
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, playerEntity, playerEntity, manager);
            Assert.True(
                playerEntity.Being.HitPoints < hitPointsBefore,
                $"Expected damage to lower HP from {hitPointsBefore}, but it is {playerEntity.Being.HitPoints}.");
        }
        manager.Queue.ProcessQueue(manager);

        // Instant damage mutates the pool directly (retirement 2c): it leaves no applied effect entity
        // behind, only a lightweight reporting descriptor.
    }

    [Fact]
    public void Properties_are_updated_when_races_change()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));

        manager.Queue.ProcessQueue(manager);

        Assert.Contains(
            playerEntity.ActiveEffects!.PropertyEffects,
            effect => effect.TargetName == nameof(PhysicalComponent.Size)
                      && effect.CombinationFunction == ValueCombinationFunction.MeanRoundDown);
        Assert.Contains(
            playerEntity.ActiveEffects.PropertyEffects,
            effect => effect.TargetName == nameof(BeingComponent.UpperExtremities)
                      && effect.CombinationFunction == ValueCombinationFunction.Sum);

        ItemData.PotionOfDwarfness.Instantiate(playerEntity);
        ItemData.PotionOfElfness.Instantiate(playerEntity);
        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        ItemData.PotionOfInhumanity.Instantiate(playerEntity);

        manager.Queue.ProcessQueue(manager);

        Assert.Equal(6, playerEntity.Physical!.Size);
        //Assert.Equal(1000, playerEntity.Physical.Weight);
        Assert.Equal(10, playerEntity.Being!.Speed);
        Assert.Equal(10, playerEntity.Being.Might);
        Assert.Equal(10, playerEntity.Being.Perception);
        Assert.Equal(10, playerEntity.Being.Focus);

        TestHelper.ActivateAbility(ItemData.PotionOfElfness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ActivateAbility(ItemData.PotionOfDwarfness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        TestHelper.ActivateAbility(ItemData.PotionOfOgreness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        ItemData.PotionOfOgreness.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);
        TestHelper.ActivateAbility(ItemData.PotionOfOgreness.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        const string ogreRaceName = "ogre race";
        var ogreRecords = playerEntity.ActiveEffects!.ComponentEffects.Where(
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: ogreRaceName }).ToList();
        Assert.Equal(2, ogreRecords.Count);
        Assert.NotEqual(0, ogreRecords[0].CarrierId);
        Assert.Equal(0, ogreRecords[1].CarrierId);
        var ogreRace = playerEntity.Being.Races.Single(race => race.Ability!.Name == ogreRaceName);
        Assert.Equal(ogreRecords[0].CarrierId, ogreRace.Id);
        Assert.Equal(2, ogreRace.Ability!.Level);
        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, ogreRaceName));
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, "ogre race#2")]);

        EffectApplicationSystem.DetachComponentCarrier(ogreRecords[0], playerEntity, manager);

        var promotedRace = playerEntity.Being.Races.Single(race => race.Ability!.Name == ogreRaceName);
        Assert.NotEqual(0, ogreRecords[1].CarrierId);
        Assert.Equal(ogreRecords[1].CarrierId, promotedRace.Id);
        Assert.Equal(1, promotedRace.Ability!.Level);
        Assert.Null(manager.AffectableAbilitiesIndex[(playerEntity.Id, "ogre race#2")]);
        Assert.Single(
            playerEntity.ActiveEffects.ComponentEffects,
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: ogreRaceName });

        TestHelper.ActivateAbility(ItemData.PotionOfInhumanity.Name + ": Drink", playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(7, playerEntity.Physical.Size);
        //Assert.Equal(1000, playerEntity.Physical.Weight);
        Assert.Equal(8, playerEntity.Being.Speed);
        Assert.Equal(12, playerEntity.Being.Might);
        Assert.Equal(9, playerEntity.Being.Perception);
        Assert.Equal(9, playerEntity.Being.Focus);
    }

    [Fact]
    public void Race_template_merges_top_level_properties_into_authored_level_effects()
    {
        var authoredEffect = new ChangeProperty<int>
        {
            Amount = "3",
            PropertyName = nameof(BeingComponent.Armor)
        };
        var race = new PlayerRace
        {
            Name = "test",
            MightBonus = 2,
            Infravision = true,
            Effects = new Dictionary<int, IReadOnlyList<Effect>>
            {
                {
                    1,
                    [
                        authoredEffect,
                        new ChangeProperty<int> { Amount = "4", PropertyName = nameof(BeingComponent.Speed) }
                    ]
                }
            }
        };

        var template = Assert.IsType<LeveledAbility>(race.GetRaceAbilityTemplate());
        var mergedEffects = Assert.IsAssignableFrom<IReadOnlyList<Effect>>(template.Effects);

        Assert.Equal("test race", template.Name);
        Assert.Equal(AbilityType.Race, template.Type);
        Assert.Equal(ActivationType.Always, template.Activation);
        Assert.True(template.Cumulative);
        Assert.Contains(mergedEffects, effect => effect is ChangeProperty<int>
        {
            PropertyName: nameof(BeingComponent.Might),
            Amount: "2",
            CombinationFunction: ValueCombinationFunction.Default
        });
        Assert.Contains(mergedEffects, effect => ReferenceEquals(effect, authoredEffect));
        Assert.Single(mergedEffects, effect => ReferenceEquals(effect, authoredEffect));

        var effectCount = mergedEffects.Count;
        Assert.Same(template, race.GetRaceAbilityTemplate());
        Assert.Equal(effectCount, mergedEffects.Count);
    }

    [Fact]
    public void Creature_race_template_grants_explicit_ability_references_at_their_level()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var creature = new Creature
        {
            Name = "referencing creature",
            InitialLevel = 1,
            AbilityReferences = new HashSet<AbilityReference>
            {
                new(AbilityData.Acrobatics.Name, 2)
            }
        };

        var entity = creature.Instantiate(level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(entity, AbilityData.Acrobatics.Name));
    }

    [Fact]
    public void Same_source_race_reapplication_merges_an_effectless_new_level()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var sourceAbility = CreateSourceAbility(playerEntity);
        var race = manager.Game.Services.Data.PlayerRaces.Get("ogre");

        var raceTemplate = race.GetRaceAbilityTemplate();
        var raceAbilityName = raceTemplate.Name;
        var firstRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, raceTemplate, level: 1, sourceAbility.Id);
        var firstCarrier = manager.FindEntity(firstRecord.CarrierId)!;
        Assert.Same(firstRecord, Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: var name }
                      && name == raceAbilityName));
        var firstCarrierId = firstRecord.CarrierId;
        Assert.Equal(firstCarrierId, firstCarrier.Id);
        Assert.Contains(
            playerEntity,
            manager.ComponentEffectsToCarrierRelationship.GetDependents(firstCarrierId));
        var secondRecord = manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, raceTemplate, level: 1, sourceAbility.Id);
        var secondCarrier = manager.FindEntity(secondRecord.CarrierId)!;
        Assert.Same(secondRecord, Assert.Single(
            playerEntity.ActiveEffects.ComponentEffects,
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: var name }
                      && name == raceAbilityName));
        manager.Queue.ProcessQueue(manager);

        Assert.Same(firstRecord, secondRecord);
        Assert.Same(firstCarrier, secondCarrier);
        Assert.Equal(2, firstRecord.Level);
        Assert.NotEqual(0, firstRecord.CarrierId);
        Assert.Equal(firstCarrierId, firstRecord.CarrierId);
        Assert.Contains(
            playerEntity,
            manager.ComponentEffectsToCarrierRelationship.GetDependents(firstRecord.CarrierId));
        Assert.Single(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect is { EffectType: EffectType.AddAbility, TargetName: var name }
                      && name == raceAbilityName);
        Assert.Equal(2, manager.FindEntity(firstRecord.CarrierId)!.Ability!.Level);
    }

    [Fact]
    public void Resolves_base_with_layered_modifiers_in_activation_then_function_order()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        AddBaseEffect(effects, nameof(BeingComponent.Might), 10);
        Assert.Equal(10, effects.ResolveInt(nameof(BeingComponent.Might)));

        // A -3 Sum status modifier.
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: -3));
        effects.TryRefresh(nameof(BeingComponent.Might), isBool: false, out _, out _);
        Assert.Equal(7, effects.ResolveInt(nameof(BeingComponent.Might)));

        // An Override(5) from an innate (Always) source sorts before the status Sum,
        // so the fold is Override(5) then Sum(-3) = 2.
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 2, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Override,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: 5));
        effects.TryRefresh(nameof(BeingComponent.Might), isBool: false, out _, out _);
        Assert.Equal(2, effects.ResolveInt(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Clamps_to_property_min()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        AddBaseEffect(effects, nameof(BeingComponent.Might), 2);
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: -10));

        // Might has MinValue = 0, so 2 + (-10) clamps to 0.
        Assert.Equal(0, effects.ResolveInt(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Caches_until_refreshed()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        AddBaseEffect(effects, nameof(BeingComponent.Might), 10);
        Assert.Equal(10, effects.ResolveInt(nameof(BeingComponent.Might)));
        // Mutating the store without refreshing keeps the cached value.
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: 4));
        Assert.Equal(10, effects.ResolveInt(nameof(BeingComponent.Might)));

        // Refresh captures the old cached value, folds in the new contribution and caches the result.
        Assert.True(effects.TryRefresh(
            nameof(BeingComponent.Might), isBool: false, out var oldValue, out var newValue));
        Assert.Equal(10, oldValue);
        Assert.Equal(14, newValue);
        Assert.Equal(newValue, effects.ResolveInt(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Refresh_uses_the_cached_resolved_value_as_the_notification_baseline()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        using var reference = game.Manager.CreateEntity();
        var effects = game.Manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        reference.Referenced.ActiveEffects = effects;

        var baseEffect = AddBaseEffect(effects, nameof(BeingComponent.Might), 10);
        Assert.Equal(10, effects.ResolveInt(nameof(BeingComponent.Might)));

        baseEffect.MagnitudePrimary = 15;
        Assert.True(effects.TryRefresh(
            nameof(BeingComponent.Might), isBool: false, out var oldValue, out var newValue));
        Assert.Equal(10, oldValue);
        Assert.Equal(15, newValue);

        Assert.False(effects.TryRefresh(
            nameof(BeingComponent.Might), isBool: false, out oldValue, out newValue));
        Assert.Equal(15, oldValue);
        Assert.Equal(15, newValue);
    }

    [Fact]
    public void Same_tier_and_function_use_instance_id_as_tie_break()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var ownerReference = manager.CreateEntity();
        var owner = ownerReference.Referenced;
        owner.ActiveEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);

        owner.ActiveEffects.AddPropertyEffect(CreatePropertyEffect(owner.ActiveEffects,
            0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Override,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: 3));
        owner.ActiveEffects.AddPropertyEffect(CreatePropertyEffect(owner.ActiveEffects,
            0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Override,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: 5));

        Assert.Equal(5, owner.ActiveEffects.ResolveInt(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Resolves_boolean_properties()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        Assert.False(effects.ResolveBool(nameof(BeingComponent.Infravision)));

        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Infravision),
            combinationFunction: ValueCombinationFunction.Override,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: 1));
        effects.TryRefresh(nameof(BeingComponent.Infravision), isBool: true, out _, out _);

        Assert.True(effects.ResolveBool(nameof(BeingComponent.Infravision)));
    }

    [Fact]
    public void Same_tier_absolute_functions_apply_before_relative()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        AddBaseEffect(effects, nameof(BeingComponent.Might), 10);
        // Both modifiers are the same activation tier, so function precedence decides order:
        // Percent (Absolute-ish, higher precedence) applies before Sum: 10 * 150% = 15, then +2 = 17.
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: 2));
        effects.PropertyEffects.Add(CreatePropertyEffect(effects,
            0, 0, 2, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Percent,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: 150));

        Assert.Equal(17, effects.ResolveInt(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Resolver_reproduces_property_values_for_a_real_being()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var effects = playerEntity.ActiveEffects!;

        var being = playerEntity.Being!;

        // The resolver, reading the being's applied ChangeProperty effects, must reproduce the value
        // the legacy engine materialized into the field - for base attributes, derived stats, and
        // resistances alike.
        Assert.Equal(being.Might, effects.ResolveInt(nameof(BeingComponent.Might)));
        Assert.Equal(being.Speed, effects.ResolveInt(nameof(BeingComponent.Speed)));
        Assert.Equal(being.Focus, effects.ResolveInt(nameof(BeingComponent.Focus)));
        Assert.Equal(being.Perception, effects.ResolveInt(nameof(BeingComponent.Perception)));
        Assert.Equal(being.HitPointMaximum, effects.ResolveInt(nameof(BeingComponent.HitPointMaximum)));
        Assert.Equal(being.EnergyPointMaximum, effects.ResolveInt(nameof(BeingComponent.EnergyPointMaximum)));
        Assert.Equal(being.Accuracy, effects.ResolveInt(nameof(BeingComponent.Accuracy)));
        Assert.Equal(being.Evasion, effects.ResolveInt(nameof(BeingComponent.Evasion)));
        Assert.Equal(being.Armor, effects.ResolveInt(nameof(BeingComponent.Armor)));
        Assert.Equal(being.MagicResistance, effects.ResolveInt(nameof(BeingComponent.MagicResistance)));
        Assert.Equal(being.FireResistance, effects.ResolveInt(nameof(BeingComponent.FireResistance)));
        Assert.Equal(being.WaterResistance, effects.ResolveInt(nameof(BeingComponent.WaterResistance)));
    }

    [Fact]
    public void UpdateOrAddPropertyModifier_removes_a_self_sourced_modifier_set_to_zero()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        // Setting a self-sourced Sum modifier stores a single instance.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(3, nameof(BeingComponent.Evasion), entity, manager);
        Assert.Single(effects.PropertyEffects);
        effects.TryRefresh(nameof(BeingComponent.Evasion), isBool: false, out _, out _);
        Assert.Equal(3, effects.ResolveInt(nameof(BeingComponent.Evasion)));

        // Updating it modifies the same instance in place rather than adding another.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(5, nameof(BeingComponent.Evasion), entity, manager);
        Assert.Single(effects.PropertyEffects);
        effects.TryRefresh(nameof(BeingComponent.Evasion), isBool: false, out _, out _);
        Assert.Equal(5, effects.ResolveInt(nameof(BeingComponent.Evasion)));

        // Setting it to a zero Sum removes the instance rather than leaving it inert at magnitude 0.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(0, nameof(BeingComponent.Evasion), entity, manager);
        Assert.Empty(effects.PropertyEffects);
        effects.TryRefresh(nameof(BeingComponent.Evasion), isBool: false, out _, out _);
        Assert.Equal(0, effects.ResolveInt(nameof(BeingComponent.Evasion)));

        // Setting an absent self-sourced modifier to a zero Sum adds nothing.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(0, nameof(BeingComponent.Evasion), entity, manager);
        Assert.Empty(effects.PropertyEffects);
    }

    [Fact]
    public void Recomputes_a_dynamic_magnitude_when_its_input_property_changes()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        // Might (the input) starts at 10; a dynamic instance contributes the current Might to Evasion.
        var mightBase = AddBaseEffect(effects, nameof(BeingComponent.Might), 10);
        var dynamic = CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Evasion),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityActivation: ActivationType.Targeted,
            magnitudePrimary: effects.ResolveInt(nameof(BeingComponent.Might)));
        effects.PropertyEffects.Add(dynamic);
        effects.RegisterDynamicMagnitude(
            dynamic,
            new[] { nameof(BeingComponent.Might) },
            () => effects.ResolveInt(nameof(BeingComponent.Might)));

        effects.TryRefresh(
            nameof(BeingComponent.Evasion), isBool: false, out _, out var evasionAtTen);
        Assert.True(effects.HasDynamicMagnitudeDependents(nameof(BeingComponent.Might)));

        // The input changes: recomputing reports the output as changed and refolds the new magnitude.
        mightBase.MagnitudePrimary = 15;
        effects.TryRefresh(nameof(BeingComponent.Might), isBool: false, out _, out _);
        var changedOutputs = new List<string>();
        effects.RecomputeDynamicMagnitudes(nameof(BeingComponent.Might), changedOutputs);

        Assert.Contains(nameof(BeingComponent.Evasion), changedOutputs);
        Assert.Equal(15, dynamic.MagnitudePrimary);
        effects.TryRefresh(
            nameof(BeingComponent.Evasion), isBool: false, out _, out var evasionAtFifteen);
        Assert.Equal(evasionAtTen + 5, evasionAtFifteen);

        // An unchanged input recompute reports nothing.
        changedOutputs.Clear();
        effects.RecomputeDynamicMagnitudes(nameof(BeingComponent.Might), changedOutputs);
        Assert.Empty(changedOutputs);
    }

    [Fact]
    public void Rejects_a_self_referential_dynamic_magnitude()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        var instance = CreatePropertyEffect(effects,
            0, 0, 1, EffectType.ChangeProperty, nameof(BeingComponent.Might),
            combinationFunction: ValueCombinationFunction.Sum);
        effects.PropertyEffects.Add(instance);

        // A magnitude that feeds Might may not also read Might - that would be an unresolvable cycle.
        Assert.Throws<InvalidOperationException>(() =>
            effects.RegisterDynamicMagnitude(
                instance, new[] { nameof(BeingComponent.Might) }, () => 0));

        // Unregistering an instance that was never registered is a no-op.
        effects.UnregisterDynamicMagnitude(instance);
        Assert.False(effects.HasDynamicMagnitudeDependents(nameof(BeingComponent.Might)));
    }

    [Fact]
    public void Rejects_an_indirect_dynamic_magnitude_cycle()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        var manager = game.Manager;

        using var reference = manager.CreateEntity();
        var entity = reference.Referenced;
        var effects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = effects;

        var evasionFromMight = CreatePropertyEffect(effects,
            0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.Evasion));
        var mightFromEvasion = CreatePropertyEffect(effects,
            0, 0, 0, EffectType.ChangeProperty, nameof(BeingComponent.Might));
        effects.AddPropertyEffect(evasionFromMight);
        effects.AddPropertyEffect(mightFromEvasion);

        effects.RegisterDynamicMagnitude(
            evasionFromMight, [nameof(BeingComponent.Might)], () => 0);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            effects.RegisterDynamicMagnitude(
                mightFromEvasion, [nameof(BeingComponent.Evasion)], () => 0));

        Assert.Contains("creates a cycle", exception.Message);
    }

    [Fact]
    public void Removing_status_ability_batches_matching_directives()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        GameEntity carrier;
        using (var carrierReference = manager.CreateEntity())
        {
            carrier = carrierReference.Referenced;
            carrier.Ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            carrier.Ability.Name = "batch status";
            carrier.Ability.Type = AbilityType.Debuff;
            carrier.Ability.OwnerId = player.Id;
        }

        var first = player.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, "batch status", carrier.Id);
        var second = player.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, "batch status", carrier.Id);

        manager.EffectApplicationSystem.RemoveAbility(player, "batch status", manager);

        Assert.DoesNotContain(first, player.ActiveEffects!.ComponentEffects);
        Assert.DoesNotContain(second, player.ActiveEffects!.ComponentEffects);
        Assert.Null(carrier.Ability);
    }

    [Fact]
    public void Removing_source_ability_batches_its_directives_and_reconciles_surviving_level()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var surviving = manager.EffectApplicationSystem.AddAbilityEffect(
            player, AbilityData.WaterSourcery);
        const int sourceAbilityId = 12345;
        var firstRemoved = player.ActiveEffects!.AddComponentEffect(
            EffectType.AddAbility, AbilityData.WaterSourcery.Name,
            sourceAbilityEntityId: sourceAbilityId);
        var secondRemoved = player.ActiveEffects.AddComponentEffect(
            EffectType.AddAbility, AbilityData.WaterSourcery.Name,
            sourceAbilityEntityId: sourceAbilityId);
        manager.EffectApplicationSystem.RelevelAbility(
            AbilityData.WaterSourcery.Name, player, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(3, EffectApplicationSystem.GetAbilityLevel(
            player, AbilityData.WaterSourcery.Name));

        manager.EffectApplicationSystem.RemoveAbilityComponentEffects(
            player, sourceAbilityId, manager);
        manager.Queue.ProcessQueue(manager);

        var remaining = Assert.Single(
            player.ActiveEffects!.ComponentEffects.GetValues(AbilityData.WaterSourcery.Name));
        Assert.Same(surviving, remaining);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(
            player, AbilityData.WaterSourcery.Name));
        Assert.Equal(
            surviving.CarrierId,
            manager.AffectableAbilitiesIndex[(player.Id, AbilityData.WaterSourcery.Name)]!.Id);
    }

    [Fact]
    public void Amount_expressions_read_activator_and_target_being_properties()
    {
        var room = TestHelper.BuildRoom("..");
        var activator = CreatureData.HumanWarrior.Instantiate(room, new Point(0, 0));
        var target = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);

        using var effectReference = manager.CreateEntity();
        var function = EffectApplicationSystem.CreateAmountFunction(
            "$MiGhT + $target.FoCuS", "property variable test", "test effect");

        Assert.Equal(
            activator.Being!.Might + target.Being!.Focus,
            function(effectReference.Referenced, activator, target));
        Assert.Equal(
            [nameof(BeingComponent.Might)],
            EffectApplicationSystem.GetAmountInputProperties("$MiGhT + $target.FoCuS"));

        const string modifierExpression = "MightModifier() + FocusModifier() + $target.Speed";
        var modifierInputs = EffectApplicationSystem.GetAmountInputProperties(modifierExpression);
        Assert.Equal(
            [nameof(BeingComponent.Might), nameof(BeingComponent.Focus)],
            modifierInputs);
        Assert.Same(
            modifierInputs,
            EffectApplicationSystem.GetAmountInputProperties(modifierExpression));
        Assert.Same(
            EffectApplicationSystem.CreateAmountFunction(modifierExpression, "cache test", "test effect"),
            EffectApplicationSystem.CreateAmountFunction(modifierExpression, "cache test", "test effect"));
    }

    [Theory]
    [InlineData("AbilityLevel('level expression source') + 2", 5)]
    [InlineData("AbilityLevel('level expression source') / 2", 1)]
    [InlineData("Min(AbilityLevel('level expression source'), 2)", 2)]
    [InlineData("Max(AbilityLevel('level expression source'), 4)", 4)]
    [InlineData("Abs(AbilityLevel('level expression source') - 5)", 2)]
    [InlineData("AbilityLevel('level expression source') >= 3 ? 7 : 0", 7)]
    public void Level_expressions_support_shared_arithmetic_and_functions(
        string expression, int expectedLevel)
    {
        var source = new Ability
        {
            Name = "level expression source",
            Effects = []
        };
        var dependent = new Ability
        {
            Name = "level expression dependent",
            LevelExpression = expression,
            Effects = []
        };
        var game = TestHelper.CreateGame();
        ((TestDataProvider)game.Services.Data).AddAbilities(source, dependent);
        var room = TestHelper.BuildRoom(".", game);
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        manager.EffectApplicationSystem.AddAbilityEffect(player, source, level: 3);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(expectedLevel, dependent.GetExpressionLevel(player));
        Assert.Equal([dependent], source.LevelExpressionDependents);
    }

    [Fact]
    public void Self_referential_amount_expression_throws_when_registering_dynamic_magnitude()
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = room.Entity.Manager;
        manager.Queue.ProcessQueue(manager);

        GameEntity abilityEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "self-referential test ability";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = player;
            abilityEntity.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.Evasion);
            effect.Amount = "$evasion";
            effect.ContainingAbilityId = abilityEntity.Id;
            effectReference.Referenced.Effect = effect;
        }

        TestHelper.ActivateAbility(abilityEntity, player, manager, 2);

        var exception = Assert.Throws<InvalidOperationException>(() => manager.Queue.ProcessQueue(manager));
        Assert.Contains("Evasion", exception.Message);
        Assert.Contains("cannot depend on itself", exception.Message);
    }

    [Fact]
    public void A_dynamic_magnitude_cascades_through_the_effect_system_when_its_input_changes()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var effects = playerEntity.ActiveEffects!;

        // A dynamic instance whose contribution to Evasion tracks the being's Might. Evasion is otherwise
        // derived from Perception/Speed, so Might only reaches it through this dynamic magnitude.
        var dynamic = CreatePropertyEffect(effects,
            0, 0, effects.PropertyEffects.Count, EffectType.ChangeProperty, nameof(BeingComponent.Evasion),
            combinationFunction: ValueCombinationFunction.Sum);
        effects.PropertyEffects.Add(dynamic);
        effects.RegisterDynamicMagnitude(
            dynamic, new[] { nameof(BeingComponent.Might) }, () => being.Might);

        // Raise Might through the normal effect path; the system re-resolves Might and the cascade
        // recomputes the dynamic Evasion contribution from the new Might.
        GameEntity toggledAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            toggledAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = playerEntity;
            toggledAbility.Ability = ability;
        }

        GameEntity mightEffectEntity;
        using (var effectReference = manager.CreateEntity())
        {
            mightEffectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.Might);
            effect.Amount = "4";
            effect.ContainingAbilityId = toggledAbility.Id;
            mightEffectEntity.Effect = effect;
        }

        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        var mightAfterFirst = being.Might;
        var evasionAfterFirst = being.Evasion;
        Assert.Equal(mightAfterFirst, dynamic.MagnitudePrimary);

        // Replace the modifier while the ability is inactive, then reactivate it. Might changes again and
        // the dynamic Evasion contribution follows the cascade.
        manager.Enqueue(DeactivateAbilityMessage.Create(
            toggledAbility.Ability!, playerEntity, manager));
        manager.Queue.ProcessQueue(manager);
        mightEffectEntity.Effect = null;
        using (var effectReference = manager.CreateEntity())
        {
            mightEffectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.Might);
            effect.Amount = "9";
            effect.ContainingAbilityId = toggledAbility.Id;
            mightEffectEntity.Effect = effect;
        }
        TestHelper.ActivateAbility(toggledAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(mightAfterFirst + 5, being.Might);
        Assert.Equal(being.Might, dynamic.MagnitudePrimary);
        Assert.Equal(evasionAfterFirst + 5, being.Evasion);
    }

    [Fact]
    public void A_self_targeted_stat_scaled_modifier_tracks_the_caster_stat_and_unbinds_when_removed()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var effects = playerEntity.ActiveEffects!;

        // A self toggled ability whose modifier adds twice Might to Evasion - a stat-derived magnitude
        // applied to the caster itself. Evasion is otherwise independent of Might, so Might only reaches it
        // through this modifier's dynamic magnitude.
        GameEntity buffAbility;
        using (var abilityReference = manager.CreateEntity())
        {
            buffAbility = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = "test buff";
            ability.Activation = ActivationType.WhileToggled;
            ability.OwnerEntity = playerEntity;
            buffAbility.Ability = ability;
        }

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ChangeProperty;
            effect.Duration = EffectDuration.Infinite;
            effect.TargetName = nameof(BeingComponent.Evasion);
            effect.CombinationFunction = ValueCombinationFunction.Sum;
            effect.Amount = "$might*2";
            effect.ContainingAbilityId = buffAbility.Id;
            effectReference.Referenced.Effect = effect;
        }

        var evasionBefore = being.Evasion;
        var mightBefore = being.Might;

        TestHelper.ActivateAbility(buffAbility, playerEntity, manager, 2);
        manager.Queue.ProcessQueue(manager);

        // The modifier landed at twice Might and registered as a dynamic magnitude keyed on Might.
        var dynamic = effects.PropertyEffects.Single(
            e => e.EffectType == EffectType.ChangeProperty
                 && e.TargetName == nameof(BeingComponent.Evasion)
                 && e.SourceAbilityEntityId == buffAbility.Id);
        Assert.Equal(2 * mightBefore, dynamic.MagnitudePrimary);
        Assert.Equal(evasionBefore + dynamic.MagnitudePrimary, being.Evasion);
        Assert.True(effects.HasDynamicMagnitudeDependents(nameof(BeingComponent.Might)));

        // Raising the caster's Might re-derives the modifier's magnitude in step, with no re-application.
        manager.EffectApplicationSystem.UpdateOrAddPropertyModifier(
            10, nameof(BeingComponent.Might), playerEntity, manager);

        Assert.Equal(mightBefore + 10, being.Might);
        Assert.Equal(2 * being.Might, dynamic.MagnitudePrimary);
        Assert.Equal(evasionBefore + dynamic.MagnitudePrimary, being.Evasion);

        // Turning the ability off drops the modifier and its dynamic-magnitude binding.
        manager.Enqueue(DeactivateAbilityMessage.Create(
            buffAbility.Ability!, playerEntity, manager));
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(effects.PropertyEffects, e => e.SourceAbilityEntityId == buffAbility.Id);
        Assert.True(effects.HasDynamicMagnitudeDependents(nameof(BeingComponent.Might)));
        Assert.Equal(evasionBefore, being.Evasion);
    }

    [Fact]
    public void Innate_derived_stats_use_being_property_expressions()
    {
        var effects = AbilityData.Innate.Effects!
            .Cast<ChangeProperty<int>>()
            .ToDictionary(effect => effect.PropertyName, effect => effect.Amount);

        Assert.Equal("10*$perception", effects[nameof(BeingComponent.Accuracy)]);
        Assert.Equal("50+5*($speed-$hindrance/10)", effects[nameof(BeingComponent.Evasion)]);
        Assert.Equal(
            "($speed-$hindrance/10)==0?0:1000/($speed-$hindrance/10)",
            effects["MovementDelay"]);
        Assert.Equal(
            "($speed-$hindrance/10)==0?0:1000/($speed-$hindrance/10)",
            effects["TurningDelay"]);
    }

    [Fact]
    public void Polymorph_assumes_the_form_stats_and_identity_then_reverts_on_expiry()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var physical = playerEntity.Physical!;
        var form = Creature.Loader.Find(CreatureData.Tiger.Name)!;
        var formId = Creature.Loader.GetId(form.Name!);

        var originalWeight = physical.Weight;
        var originalRaceCount = being.Races.Count;
        var originalAbilityCount = being.Abilities.Count;

        // A player has no innate species, the form's weight differs, and the form grants abilities, so the
        // transformation is observable.
        Assert.Null(being.Species);
        Assert.NotEqual(originalWeight, form.Weight);
        Assert.NotEmpty(form.Abilities);

        const int duration = 50;
        manager.EffectApplicationSystem.ApplyPolymorph(form, playerEntity, duration, manager);
        manager.Queue.ProcessQueue(manager);

        // While polymorphed the being resolves the form's identity, the form's stats override its own, and
        // the being carries the form as a race, gaining the form's abilities.
        Assert.Equal(formId, being.Species);
        Assert.Equal(form.Weight, physical.Weight);
        Assert.Contains(being.Races, r => r.Ability?.Name == form.GetRaceAbilityName());
        Assert.True(being.Abilities.Count > originalAbilityCount);
        var formOverrides = playerEntity.ActiveEffects!.PropertyEffects
            .Where(effect => effect.TargetName != null
                             && effect.ExpirationTarget == manager.Game.CurrentTick + duration)
            .ToList();
        Assert.NotEmpty(formOverrides);
        Assert.All(formOverrides, effect =>
        {
            Assert.Equal(EffectType.ChangeProperty, effect.EffectType);
            Assert.Equal(AbilityType.Debuff, effect.SourceAbilityType);
            Assert.Equal(ValueCombinationFunction.Override, effect.CombinationFunction);
        });
        var polymorphedAbilityCount = being.Abilities.Count;

        // Once the duration lapses the transformation expires: the being's own form and stats resurface, the
        // form race is dropped and the abilities it granted are removed.
        manager.Game.CurrentTick += duration + 1;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(being.Species);
        Assert.Equal(originalWeight, physical.Weight);
        Assert.DoesNotContain(being.Races, r => r.Ability?.Name == form.GetRaceAbilityName());
        Assert.Equal(originalRaceCount, being.Races.Count);
        Assert.True(being.Abilities.Count < polymorphedAbilityCount,
            $"the form's granted abilities should be dropped: {polymorphedAbilityCount} -> {being.Abilities.Count}");
    }

    [Fact]
    public void Polymorph_suppresses_the_beings_own_race_abilities_and_restores_them()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var form = Creature.Loader.Find(CreatureData.Tiger.Name)!;

        // The being's own active race passives are the ones a transformation suppresses.
        var suppressibleNames = OwnRaceActivePassives(playerEntity, manager)
            .Select(abilityEntity => abilityEntity.Ability!.Name!)
            .ToList();
        Assert.NotEmpty(suppressibleNames);

        const int duration = 50;
        manager.EffectApplicationSystem.ApplyPolymorph(form, playerEntity, duration, manager);
        manager.Queue.ProcessQueue(manager);

        // While polymorphed, the being's own race keeps an effectless presentation carrier while abilities
        // granted by that race are inactive.
        foreach (var abilityName in suppressibleNames)
        {
            if (manager.AffectableAbilitiesIndex[(playerEntity.Id, abilityName)] is { Ability: { } suppressed })
            {
                Assert.Equal(AbilityType.Race, suppressed.Type);
                Assert.Equal(0, suppressed.Level);
                Assert.False(suppressed.IsUsable);
            }
        }

        // Once the transformation lapses they are reactivated.
        manager.Game.CurrentTick += duration + 1;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.DoesNotContain(
            playerEntity.ActiveEffects!.ComponentEffects,
            effect => effect.EffectType == EffectType.DecreaseAbilityLevel
                      && effect.ConditionValue == (int)AbilityType.Race);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, "human race"));
        Assert.NotEqual(
            0,
            Assert.Single(
                playerEntity.ActiveEffects.ComponentEffects,
                effect => effect is { EffectType: EffectType.AddAbility, TargetName: "human race" })
                .CarrierId);
        Assert.All(
            suppressibleNames,
            abilityName => Assert.True(
                manager.AffectableAbilitiesIndex[(playerEntity.Id, abilityName)]!.Ability!.IsActive));
    }

    [Fact]
    public void Polymorph_replaces_an_existing_transformation_rather_than_stacking()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var physical = playerEntity.Physical!;
        var activeEffects = playerEntity.ActiveEffects!;

        var firstForm = Creature.Loader.Find(CreatureData.Tiger.Name)!;
        var secondForm = Creature.Loader.Find("gnome")!;
        var secondId = Creature.Loader.GetId(secondForm.Name!);
        Assert.NotEqual(firstForm.Weight, secondForm.Weight);

        var originalRaceCount = being.Races.Count;

        manager.EffectApplicationSystem.ApplyPolymorph(firstForm, playerEntity, duration: 50, manager);
        manager.Queue.ProcessQueue(manager);
        Assert.Equal(Creature.Loader.GetId(firstForm.Name!), being.Species);
        Assert.Equal(firstForm.Weight, physical.Weight);

        // A second polymorph replaces the first rather than stacking on top of it.
        manager.EffectApplicationSystem.ApplyPolymorph(secondForm, playerEntity, duration: 50, manager);
        manager.Queue.ProcessQueue(manager);

        // The being now resolves as the second form with its stats; the first form's identity, race carrier
        // and stat overrides are gone, and exactly one transformation is in force (not two stacked).
        Assert.Equal(secondId, being.Species);
        Assert.Equal(secondForm.Weight, physical.Weight);
        Assert.Contains(being.Races, r => r.Ability?.Name == secondForm.GetRaceAbilityName());
        Assert.DoesNotContain(being.Races, r => r.Ability?.Name == firstForm.GetRaceAbilityName());
        Assert.Single(activeEffects.PropertyEffects,
            e => e.EffectType == EffectType.Polymorph && e.TargetName == null);

        // When the single remaining transformation lapses, the being reverts fully to its own form.
        manager.Game.CurrentTick += 51;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Null(being.Species);
        Assert.Equal(originalRaceCount, being.Races.Count);
    }

    [Fact]
    public void Drinking_a_potion_of_polymorph_transforms_the_drinker()
    {
        var room = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        Assert.Null(being.Species);

        ItemData.PotionOfPolymorph.Instantiate(playerEntity);
        manager.Queue.ProcessQueue(manager);

        // Drinking the potion (a full action, exercising the normal turn flow) transforms the drinker into
        // the potion's creature form.
        var drinkAbility = ItemData.PotionOfPolymorph.Name + ": Drink";
        TestHelper.ActivateAbility(drinkAbility, playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(Creature.Loader.GetId("gnome"), being.Species);
    }

    [Fact]
    public void Removes_only_the_instances_of_the_given_source_ability_and_reverts_their_properties()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var activeEffects = playerEntity.ActiveEffects!;
        var baseArmor = playerEntity.Being!.Armor;

        // Real ability entities: the source-ability relationship enforces referential integrity, so an
        // instance can only reference an ability that exists.
        var abilityA = CreateAbility(manager, playerEntity, "buff a");
        var abilityB = CreateAbility(manager, playerEntity, "buff b");

        // Two instances from ability A (one contributing a resolved property, one bare) and one from B.
        activeEffects.PropertyEffects.Add(CreatePropertyEffect(activeEffects,
            0, 0, activeEffects.PropertyEffects.Count, EffectType.ChangeProperty, nameof(BeingComponent.Armor),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityEntityId: abilityA.Id,
            magnitudePrimary: 5));
        activeEffects.PropertyEffects.Add(CreatePropertyEffect(activeEffects,
            0, 0, activeEffects.PropertyEffects.Count, EffectType.Confuse, null,
            sourceAbilityEntityId: abilityA.Id));
        activeEffects.PropertyEffects.Add(CreatePropertyEffect(activeEffects,
            0, 0, activeEffects.PropertyEffects.Count, EffectType.Confuse, null,
            sourceAbilityEntityId: abilityB.Id));

        manager.EffectApplicationSystem.RemoveMatchingPropertyEffects(
            activeEffects, playerEntity,
            instance => instance.SourceAbilityEntityId == abilityA.Id, manager);

        Assert.DoesNotContain(activeEffects.PropertyEffects, e => e.SourceAbilityEntityId == abilityA.Id);
        Assert.Single(activeEffects.PropertyEffects, e => e.SourceAbilityEntityId == abilityB.Id);
        // The removed Armor contribution is reverted, returning the resolved value to its baseline.
        Assert.Equal(baseArmor, playerEntity.Being.Armor);
    }

    [Fact]
    public void Removing_a_continuous_source_ability_reverts_and_drops_its_instances()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var activeEffects = playerEntity.ActiveEffects!;
        var baseArmor = playerEntity.Being!.Armor;

        var abilityA = CreateAbility(manager, playerEntity, "buff a");
        var abilityB = CreateAbility(manager, playerEntity, "buff b");

        // Continuously sustained modifiers (kept only while their ability is active).
        AddModifier(activeEffects, abilityA.Id, ActivationType.WhileEquipped, 5, expirationTick: null, manager);
        AddModifier(activeEffects, abilityB.Id, ActivationType.WhileEquipped, 7, expirationTick: null, manager);
        Assert.Equal(baseArmor + 12, playerEntity.Being.Armor);
        Assert.Contains(playerEntity,
            manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(abilityA.Id));

        DeleteEntityMessage.Enqueue(abilityA, manager);
        manager.Queue.ProcessQueue(manager);

        // Ability A's sustained modifier is reverted and dropped; ability B's is untouched.
        Assert.DoesNotContain(activeEffects.PropertyEffects, e => e.SourceAbilityEntityId == abilityA.Id);
        Assert.Single(activeEffects.PropertyEffects, e => e.SourceAbilityEntityId == abilityB.Id);
        Assert.Equal(baseArmor + 7, playerEntity.Being.Armor);
        Assert.Empty(manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(abilityA.Id));
    }

    [Fact]
    public void Gets_abilities_affecting_a_property_with_an_optional_type_filter()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var statusAbilityEntity = CreateAbility(manager, playerEntity, "test status");
        statusAbilityEntity.Ability!.Type = AbilityType.Debuff;
        var traitAbilityEntity = CreateAbility(manager, playerEntity, "test trait");
        traitAbilityEntity.Ability!.Type = AbilityType.Trait;
        var activeEffects = playerEntity.ActiveEffects!;

        AddModifier(
            activeEffects, statusAbilityEntity.Id, ActivationType.Always,
            1, expirationTick: null, manager);
        AddModifier(
            activeEffects, traitAbilityEntity.Id, ActivationType.Always,
            3, expirationTick: null, manager);

        var all = EffectApplicationSystem.GetAbilitiesAffectingProperty(
            playerEntity, nameof(BeingComponent.Armor)).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(statusAbilityEntity.Ability, all);
        Assert.Contains(traitAbilityEntity.Ability, all);

        var status = Assert.Single(
            EffectApplicationSystem.GetAbilitiesAffectingProperty(
                playerEntity, nameof(BeingComponent.Armor), AbilityType.Debuff));
        Assert.Same(statusAbilityEntity.Ability, status);
    }

    [Fact]
    public void Removing_a_non_continuous_source_ability_unlinks_its_timed_instances_and_keeps_their_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var activeEffects = playerEntity.ActiveEffects!;
        var baseArmor = playerEntity.Being!.Armor;

        var ability = CreateAbility(manager, playerEntity, "thrown hex");

        // A fire-and-forget timed status: it carries its own expiry, so it must outlive its source.
        AddModifier(activeEffects, ability.Id, ActivationType.Default, 5,
            expirationTick: manager.Game.CurrentTick + 1000, manager);
        Assert.Equal(baseArmor + 5, playerEntity.Being.Armor);

        DeleteEntityMessage.Enqueue(ability, manager);
        manager.Queue.ProcessQueue(manager);

        var surviving = Assert.Single(activeEffects.PropertyEffects,
            e => e.TargetName == nameof(BeingComponent.Armor));
        Assert.Null(surviving.SourceAbilityEntityId);
        Assert.Equal(baseArmor + 5, playerEntity.Being.Armor);
    }

    [Fact]
    public void Removing_a_source_ability_reaches_the_instances_it_sourced_on_every_entity()
    {
        var level = TestHelper.BuildRoom("..");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var creature = CreatureData.HumanWarrior.Instantiate(level, new Point(1, 0));
        manager.Queue.ProcessQueue(manager);

        var ability = CreateAbility(manager, playerEntity, "aura");

        // The same ability contributes a sustained modifier to two different affectable entities.
        AddModifier(playerEntity.ActiveEffects!, ability.Id, ActivationType.WhileEquipped, 5,
            expirationTick: null, manager);
        var creatureBaseArmor = creature.Being!.Armor;
        AddModifier(creature.ActiveEffects!, ability.Id, ActivationType.WhileEquipped, 5,
            expirationTick: null, manager);
        Assert.Equal(creatureBaseArmor + 5, creature.Being.Armor);
        Assert.Equal(2, manager.AppliedEffectsToSourceAbilityRelationship.GetDependents(ability.Id).Count);

        DeleteEntityMessage.Enqueue(ability, manager);
        manager.Queue.ProcessQueue(manager);

        // The relationship reached the instance on both entities, reverting them.
        Assert.DoesNotContain(playerEntity.ActiveEffects!.PropertyEffects, e => e.SourceAbilityEntityId == ability.Id);
        Assert.DoesNotContain(creature.ActiveEffects!.PropertyEffects, e => e.SourceAbilityEntityId == ability.Id);
        Assert.Equal(creatureBaseArmor, creature.Being.Armor);
    }

    [Fact]
    public void Adding_an_instance_referencing_a_missing_ability_throws()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var activeEffects = playerEntity.ActiveEffects!;

        Assert.Throws<InvalidOperationException>(() =>
            activeEffects.PropertyEffects.Add(CreatePropertyEffect(activeEffects,
                0, 0, activeEffects.PropertyEffects.Count, EffectType.ChangeProperty, nameof(BeingComponent.Armor),
                combinationFunction: ValueCombinationFunction.Sum,
                sourceAbilityEntityId: 987654321,
                magnitudePrimary: 5)));
    }

    [Fact]
    public void Deleting_a_being_unlinks_the_timed_instances_it_sourced_on_another_being()
    {
        var room = TestHelper.BuildRoom(@"
...
...
...");
        var manager = room.Game.Manager;

        var attacker = CreatureData.HumanWarrior.Instantiate(room, new Point(1, 1));
        var victim = CreatureData.HumanWarrior.Instantiate(room, new Point(2, 2));
        manager.Queue.ProcessQueue(manager);

        var sourceAbility = CreateAbility(manager, attacker, "venom");

        var victimBaseArmor = victim.Being!.Armor;
        AddModifier(victim.ActiveEffects!, sourceAbility.Id, ActivationType.Default, 5,
            expirationTick: manager.Game.CurrentTick + 1000, manager);
        Assert.Equal(victimBaseArmor + 5, victim.Being.Armor);

        DeleteEntityMessage.Enqueue(attacker, manager);
        manager.Queue.ProcessQueue(manager);

        // The attacker (and its ability) is gone, but the timed instance it applied to the still-living
        // victim persists - it merely loses its source link, so its contribution lingers until it expires.
        Assert.Null(manager.FindEntity(attacker.Id));
        var surviving = Assert.Single(victim.ActiveEffects!.PropertyEffects,
            e => e.TargetName == nameof(BeingComponent.Armor));
        Assert.Null(surviving.SourceAbilityEntityId);
        Assert.Equal(victimBaseArmor + 5, victim.Being.Armor);
    }

    [Fact]
    public void Confers_visible_lycanthropy_that_builds_to_a_timed_transformation()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        var initialRaceCount = being.Races.Count;

        ConferLycanthropy(playerEntity, "wolfwere", manager);

        // The bite applies the permanent, client-visible base status without transforming the victim.
        Assert.Equal(initialRaceCount, being.Races.Count);
        Assert.DoesNotContain(being.Races, r => r.Ability?.Name == "wolfwere race");
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Lycanthropic.Name));

        var counter = playerEntity.ActiveEffects!.Accumulators[EffectType.ConferLycanthropy];
        Assert.Equal(0, counter.Value);
        var marker = Assert.Single(playerEntity.ActiveEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.CombinationFunction == ValueCombinationFunction.Sum
                      && effect.MagnitudeSecondary != 0);
        Assert.Equal(Creature.Loader.GetId("wolfwere"), marker.MagnitudeSecondary);
        var source = Assert.Single(playerEntity.ActiveEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.MagnitudePrimary != 0);
        Assert.Equal(10, source.MagnitudePrimary);
        Assert.Equal(ValueCombinationFunction.BuildUpPerSource, source.CombinationFunction);
        Assert.Equal(Creature.Loader.GetId("wolfwere"), source.MagnitudeSecondary);
        var statusAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Lycanthropic.Name)]!.Ability!;
        var statusEffect = Assert.Single(statusAbility.Effects).Effect!;
        Assert.Equal(
            Creature.Loader.GetId("wolfwere").ToString(CultureInfo.InvariantCulture),
            statusEffect.SecondaryAmount);

        // The resisted L1 contribution does not outpace regular status decay, so its spent bank is removed.
        manager.EffectApplicationSystem.ApplyLingeringBuildUpEffect(
            source, playerEntity, TimeSystem.DefaultActionDelay, manager);
        manager.EffectApplicationSystem.RealizeBuildUps(
            playerEntity, TimeSystem.DefaultActionDelay, manager);
        Assert.False(playerEntity.ActiveEffects.Accumulators.ContainsKey(EffectType.ConferLycanthropy));

        // Reaching the next threshold adds the cumulative L2 Polymorph while retaining the L1 source.
        manager.EffectApplicationSystem.BankBuildUp(
            EffectType.ConferLycanthropy, EffectApplicationSystem.BuildUpThreshold,
            playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(2, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Lycanthropic.Name));
        statusAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Lycanthropic.Name)]!.Ability!;
        statusEffect = Assert.Single(
            statusAbility.Effects,
            effect => effect.Effect!.EffectType == EffectType.Polymorph).Effect!;
        Assert.Equal(EffectType.Polymorph, statusEffect.EffectType);
        Assert.Equal(
            Creature.Loader.GetId("wolfwere").ToString(CultureInfo.InvariantCulture),
            statusEffect.SecondaryAmount);
        Assert.Equal(Creature.Loader.GetId("wolfwere"), being.Species);
        Assert.Contains(being.Races, r => r.Ability?.Name == "wolfwere race");
        var statusRecords = playerEntity.ActiveEffects.ComponentEffects
            .GetValues(AbilityData.Lycanthropic.Name)
            .OrderBy(record => record.InstanceId)
            .ToList();
        Assert.Equal(2, statusRecords.Count);
        Assert.All(statusRecords, record => Assert.Equal(1, record.Level));
        Assert.Equal(0, statusRecords[0].CarrierId);
        Assert.NotEqual(0, statusRecords[1].CarrierId);
        Assert.Null(manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Lycanthropic.Name + "#2")]);
        Assert.Null(statusRecords[0].ExpirationEvent);
        Assert.Null(statusRecords[0].ExpirationTarget);
        var timedStatusRecord = statusRecords[1];
        Assert.Equal(EffectDuration.UntilTimeout, timedStatusRecord.ExpirationEvent);
        Assert.Equal(manager.Game.CurrentTick + 2000, timedStatusRecord.ExpirationTarget);

        // When L2 expires, Polymorph ends and the permanent status returns to L1 without an expiration.
        manager.Game.CurrentTick = timedStatusRecord.ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Lycanthropic.Name));
        var persistentStatusRecord = Assert.Single(
            playerEntity.ActiveEffects.ComponentEffects.GetValues(AbilityData.Lycanthropic.Name));
        Assert.Same(statusRecords[0], persistentStatusRecord);
        Assert.NotEqual(0, persistentStatusRecord.CarrierId);
        Assert.Null(persistentStatusRecord.ExpirationEvent);
        Assert.Null(persistentStatusRecord.ExpirationTarget);
        Assert.NotEqual(Creature.Loader.GetId("wolfwere"), being.Species);
        Assert.DoesNotContain(being.Races, r => r.Ability?.Name == "wolfwere race");
    }

    [Fact]
    public void Lycanthropy_status_rebuild_recovers_the_form_from_its_lingering_source()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ConferLycanthropy(playerEntity, "wolfwere", manager);

        var activeEffects = playerEntity.ActiveEffects!;
        var marker = Assert.Single(
            activeEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.CombinationFunction == ValueCombinationFunction.Sum);
        activeEffects.PropertyEffects.Remove(marker);

        var formId = Creature.Loader.GetId("wolfwere");
        Assert.Contains(
            activeEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.MagnitudeSecondary == formId);

        manager.EffectApplicationSystem.RelevelAbility(
            AbilityData.Lycanthropic.Name, playerEntity, manager);

        var statusAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Lycanthropic.Name)]!.Ability!;
        Assert.Equal(
            formId.ToString(CultureInfo.InvariantCulture),
            Assert.Single(statusAbility.Effects).Effect!.SecondaryAmount);
    }

    [Fact]
    public void Stronger_lycanthropy_application_switches_the_target_form()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ConferLycanthropy(playerEntity, "wolfwere", manager);
        var activeEffects = playerEntity.ActiveEffects!;
        var accumulator = activeEffects.Accumulators[EffectType.ConferLycanthropy];
        accumulator.Value = 10;

        ConferLycanthropy(playerEntity, "hyenawere", manager, amount: "10");
        Assert.Equal(
            Creature.Loader.GetId("wolfwere"),
            activeEffects.PropertyEffects.Single(
                effect => effect.EffectType == EffectType.ConferLycanthropy
                          && effect.CombinationFunction == ValueCombinationFunction.Sum)
                .MagnitudeSecondary);

        ConferLycanthropy(playerEntity, "hyenawere", manager, amount: "40");

        var hyenawereId = Creature.Loader.GetId("hyenawere");
        Assert.Equal(
            hyenawereId,
            activeEffects.PropertyEffects.Single(
                effect => effect.EffectType == EffectType.ConferLycanthropy
                          && effect.CombinationFunction == ValueCombinationFunction.Sum)
                .MagnitudeSecondary);
        var statusAbility = manager.AffectableAbilitiesIndex[
            (playerEntity.Id, AbilityData.Lycanthropic.Name)]!.Ability!;
        Assert.Equal(
            hyenawereId.ToString(CultureInfo.InvariantCulture),
            Assert.Single(
                statusAbility.Effects,
                effect => effect.Effect!.EffectType == EffectType.ConferLycanthropy)
                .Effect!.SecondaryAmount);
        Assert.Equal(1, statusAbility.Level);
    }

    [Fact]
    public void Lycanthropy_uses_regular_decay_without_its_lingering_source()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ConferLycanthropy(playerEntity, "wolfwere", manager);
        var activeEffects = playerEntity.ActiveEffects!;
        var source = Assert.Single(activeEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.MagnitudePrimary != 0);
        activeEffects.PropertyEffects.Remove(source);

        var accumulator = activeEffects.Accumulators[EffectType.ConferLycanthropy];
        accumulator.Value = EffectApplicationSystem.BuildUpThreshold - 1;
        var species = playerEntity.Being!.Species;

        manager.EffectApplicationSystem.RealizeBuildUps(
            playerEntity, TimeSystem.DefaultActionDelay, manager);

        Assert.Equal(species, playerEntity.Being.Species);
        Assert.Equal(EffectApplicationSystem.BuildUpThreshold - 21, accumulator.Value);
        Assert.Equal(1, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Lycanthropic.Name));
    }

    [Fact]
    public void Non_instant_lycanthropy_creates_an_expiring_active_source()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);
        var appliedTick = manager.Game.CurrentTick;

        ConferLycanthropy(
            playerEntity, "wolfwere", manager,
            EffectDuration.UntilTimeout, durationAmount: "50");

        var activeEffects = playerEntity.ActiveEffects!;
        var source = Assert.Single(activeEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy
                      && effect.MagnitudePrimary != 0);
        Assert.Equal(ValueCombinationFunction.BuildUpFixed, source.CombinationFunction);
        Assert.Null(source.SourceAbilityEntityId);
        Assert.Equal(ActivationType.Manual, source.SourceAbilityActivation);
        Assert.Equal(EffectDuration.UntilTimeout, source.ExpirationEvent);
        Assert.Equal(appliedTick + 50, source.ExpirationTarget);
        Assert.Equal(0, source.MagnitudeSecondary);
        Assert.Equal(source.ExpirationTarget, activeEffects.NextEffectTick);

        Assert.False(activeEffects.Accumulators.ContainsKey(EffectType.ConferLycanthropy));
        manager.Game.CurrentTick = source.ExpirationTarget!.Value;
        manager.EffectApplicationSystem.ExpireTimedPropertyEffects(playerEntity, manager);

        Assert.DoesNotContain(activeEffects.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy);
        Assert.Equal(50, activeEffects.Accumulators[EffectType.ConferLycanthropy].Value);
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(playerEntity, AbilityData.Lycanthropic.Name));
    }

    [Fact]
    public void Non_instant_lycanthropy_reapplication_does_not_store_the_form_on_its_source()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        ConferLycanthropy(
            playerEntity, "wolfwere", manager,
            EffectDuration.UntilTimeout, durationAmount: "50");
        var source = Assert.Single(
            playerEntity.ActiveEffects!.PropertyEffects,
            effect => effect.EffectType == EffectType.ConferLycanthropy);

        ConferLycanthropy(
            playerEntity, "hyenawere", manager,
            EffectDuration.UntilTimeout, durationAmount: "50");

        Assert.Same(
            source,
            Assert.Single(
                playerEntity.ActiveEffects.PropertyEffects,
                effect => effect.EffectType == EffectType.ConferLycanthropy));
            Assert.Equal(0, source.MagnitudeSecondary);
    }

    [Fact]
    public void Removing_a_conferred_race_detaches_it_and_drops_its_directive()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var initialRaceCount = playerEntity.Being!.Races.Count;
        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, Creature.Loader.Find("wolfwere")!.GetRaceAbilityTemplate());
        manager.Queue.ProcessQueue(manager);
        Assert.Contains(playerEntity.Being.Races, r => r.Ability?.Name == "wolfwere race");

        RemoveRace(playerEntity, "wolfwere race", manager);

        // The were-race leaves the being's effective Races collection while its positive source remains
        // available for restoration, without disturbing the being's original races.
        Assert.Equal(initialRaceCount, playerEntity.Being.Races.Count);
        Assert.DoesNotContain(playerEntity.Being.Races, r => r.Ability?.Name == "wolfwere race");
        Assert.Equal(0, EffectApplicationSystem.GetAbilityLevel(playerEntity, "wolfwere race"));
        Assert.Contains(playerEntity.ActiveEffects!.ComponentEffects,
            r => r.EffectType == EffectType.AddAbility && r.TargetName == "wolfwere race");
        Assert.Contains(playerEntity.ActiveEffects.ComponentEffects,
            r => r.EffectType == EffectType.DecreaseAbilityLevel && r.TargetName == "wolfwere race");
        Assert.True(playerEntity.Being.IsAlive);
    }

    [Fact]
    public void Xp_does_not_level_a_conferred_non_player_race()
    {
        var level = TestHelper.BuildRoom(".");
        var manager = level.Entity.Manager;
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.AddAbilityEffect(
            playerEntity, Creature.Loader.Find("wolfwere")!.GetRaceAbilityTemplate());
        manager.Queue.ProcessQueue(manager);
        var wereRace = playerEntity.Being!.Races.Single(r => r.Ability?.Name == "wolfwere race");
        var initialLevel = wereRace.Ability!.Level;
        var initialXp = playerEntity.Being.ExperiencePoints;

        manager.XPSystem.AddPlayerXP(5000, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(initialLevel, wereRace.Ability.Level);
        Assert.Equal(initialXp + 5000, playerEntity.Being.ExperiencePoints);
    }

    [Fact]
    public void Removed_property_effect_is_cleaned_and_returned_to_pool()
    {
        var target = CreatePoolTarget();
        var activeEffects = target.ActiveEffects!;
        var original = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty,
            nameof(BeingComponent.Might),
            ValueCombinationFunction.Sum,
            sourceAbilityType: AbilityType.Buff,
            sourceAbilityActivation: ActivationType.WhileToggled,
            magnitudePrimary: 7,
            magnitudeSecondary: 11,
            expirationTarget: 17,
            expirationEvent: EffectDuration.UntilTimeout);
        var notifications = 0;
        original.PropertyChanged += (_, _) => notifications++;

        activeEffects.PropertyEffects.Remove(original);
        target.Manager.Queue.ProcessQueue(target.Manager);
        var reused = RentPropertyEffect(activeEffects, original);

        Assert.Equal(EffectType.ChangeProperty, reused.EffectType);
        Assert.Equal(nameof(BeingComponent.Focus), reused.TargetName);
        Assert.Equal(0, reused.MagnitudePrimary);
        Assert.Null(reused.SourceAbilityEntityId);
        Assert.Null(reused.ExpirationTarget);
        Assert.Null(reused.ExpirationEvent);

        reused.MagnitudePrimary = 1;
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Removed_component_effect_is_cleaned_and_returned_to_pool()
    {
        var target = CreatePoolTarget();
        var activeEffects = target.ActiveEffects!;
        var original = activeEffects.AddComponentEffect(
            EffectType.AddAbility,
            "test ability",
            conditionValue: 789,
            level: 3,
            expirationTarget: 17,
            expirationEvent: EffectDuration.UntilTimeout);

        activeEffects.ComponentEffects.Remove(original);
        target.Manager.Queue.ProcessQueue(target.Manager);
        var reused = RentComponentEffect(activeEffects, original);

        Assert.Equal(EffectType.AddAbility, reused.EffectType);
        Assert.Equal("replacement ability", reused.TargetName);
        Assert.Equal(0, reused.CarrierId);
        Assert.Equal(0, reused.SourceAbilityEntityId);
        Assert.Null(reused.ConditionValue);
        Assert.Equal(1, reused.Level);
        Assert.Null(reused.ExpirationTarget);
        Assert.Null(reused.ExpirationEvent);
    }

    [Fact]
    public void Removed_effect_accumulator_is_cleaned_and_returned_to_pool()
    {
        var target = CreatePoolTarget();
        var activeEffects = target.ActiveEffects!;
        var original = activeEffects.AddAccumulator(EffectType.Paralyze);
        original.Value = 37;
        var notifications = 0;
        original.PropertyChanged += (_, _) => notifications++;

        activeEffects.Accumulators.Remove(original);
        target.Manager.Queue.ProcessQueue(target.Manager);
        var reused = RentAccumulator(activeEffects, original);

        Assert.Equal(EffectType.Stun, reused.EffectType);
        Assert.Equal(0, reused.Value);
        Assert.Equal(target.GameId, reused.GameId);
        Assert.Equal(target.Id, reused.EntityId);

        reused.Value = 1;
        Assert.Equal(0, notifications);
    }

    [Fact]
    public void Active_effects_are_retired_when_entity_manager_is_reset_before_component_cleanup()
    {
        var manager = TestHelper.CreateGameManager();
        var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var activeEffects = manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        entity.ActiveEffects = activeEffects;
        var propertyEffect = activeEffects.AddPropertyEffect(
            EffectType.ChangeProperty, nameof(BeingComponent.Might));
        var componentEffect = activeEffects.AddComponentEffect(EffectType.AddAbility, "test ability");
        var accumulator = activeEffects.AddAccumulator(EffectType.Paralyze);
        var tracker = new object();
        ((ITrackable)activeEffects).StartTracking(tracker);

        entityReference.Dispose();
        ((ITrackable)entity).StopTracking(tracker);
        ((ITrackable)activeEffects).StopTracking(tracker);
        manager.Queue.ProcessQueue(manager);

        using var replacementReference = manager.CreateEntity();
        var replacementEntity = replacementReference.Referenced;
        replacementEntity.ActiveEffects =
            manager.CreateComponent<ActiveEffectsComponent>(EntityComponent.ActiveEffects);
        Assert.Same(propertyEffect, RentPropertyEffect(replacementEntity.ActiveEffects, propertyEffect));
        Assert.Same(componentEffect, RentComponentEffect(replacementEntity.ActiveEffects, componentEffect));
        Assert.Same(accumulator, RentAccumulator(replacementEntity.ActiveEffects, accumulator));
    }

    [Fact]
    public void Tracked_effect_is_not_returned_to_pool_during_collection_notification()
    {
        var target = CreatePoolTarget();
        var activeEffects = target.ActiveEffects!;
        var original = activeEffects.AddComponentEffect(EffectType.AddAbility, "test ability");
        var tracker = new object();
        ((ITrackable)original).StartTracking(tracker);
        activeEffects.ComponentEffects.CollectionChanged += (_, args) =>
        {
            if (args.OldItems?.Contains(original) == true)
            {
                ((ITrackable)original).StopTracking(tracker);
            }
        };

        activeEffects.ComponentEffects.Remove(original);
        var replacement = activeEffects.AddComponentEffect(EffectType.AddAbility, "replacement ability");

        Assert.NotSame(original, replacement);
        target.Manager.Queue.ProcessQueue(target.Manager);
        Assert.Same(original, RentComponentEffect(activeEffects, original));
    }

    private static GameEntity CreatePoolTarget()
    {
        var room = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, room, new Point(0, 0));
        player.Manager.Queue.ProcessQueue(player.Manager);
        return player;
    }

    private static ActivePropertyEffect RentPropertyEffect(
        ActiveEffectsComponent activeEffects, ActivePropertyEffect expected)
    {
        for (var i = 0; i < 16384; i++)
        {
            var candidate = activeEffects.AddPropertyEffect(
                EffectType.ChangeProperty, nameof(BeingComponent.Focus));
            if (ReferenceEquals(candidate, expected))
            {
                return candidate;
            }

            activeEffects.PropertyEffects.Remove(candidate);
        }

        throw new Xunit.Sdk.XunitException("Property effect was not returned to its pool.");
    }

    private static ActiveComponentEffect RentComponentEffect(
        ActiveEffectsComponent activeEffects, ActiveComponentEffect expected)
    {
        for (var i = 0; i < 4096; i++)
        {
            var candidate = activeEffects.AddComponentEffect(
                EffectType.AddAbility, "replacement ability");
            if (ReferenceEquals(candidate, expected))
            {
                return candidate;
            }

            activeEffects.ComponentEffects.Remove(candidate);
        }

        throw new Xunit.Sdk.XunitException("Component effect was not returned to its pool.");
    }

    private static EffectAccumulator RentAccumulator(
        ActiveEffectsComponent activeEffects, EffectAccumulator expected)
    {
        for (var i = 0; i < 4096; i++)
        {
            var candidate = activeEffects.AddAccumulator(EffectType.Stun);
            if (ReferenceEquals(candidate, expected))
            {
                return candidate;
            }

            activeEffects.Accumulators.Remove(candidate);
        }

        throw new Xunit.Sdk.XunitException("Effect accumulator was not returned to its pool.");
    }

    private static GameEntity CreateAbility(GameManager manager, GameEntity owner, string name)
    {
        GameEntity abilityEntity;
        using (var abilityReference = manager.CreateEntity())
        {
            abilityEntity = abilityReference.Referenced;
            var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
            ability.Name = name;
            ability.OwnerEntity = owner;
            abilityEntity.Ability = ability;
        }

        manager.Queue.ProcessQueue(manager);
        return abilityEntity;
    }

    private static ActivePropertyEffect AddModifier(
        ActiveEffectsComponent activeEffects, int sourceAbilityId, ActivationType activation, int amount,
        int? expirationTick, GameManager manager)
    {
        var instance = CreatePropertyEffect(activeEffects,
            0, 0, activeEffects.PropertyEffects.Count, EffectType.ChangeProperty, nameof(BeingComponent.Armor),
            combinationFunction: ValueCombinationFunction.Sum,
            sourceAbilityEntityId: sourceAbilityId,
            sourceAbilityActivation: activation,
            magnitudePrimary: amount,
            expirationTarget: expirationTick,
            expirationEvent: expirationTick == null ? null : EffectDuration.UntilTimeout);
        activeEffects.PropertyEffects.Add(instance);
        // Adding an instance directly bypasses the apply path, so refresh as the real pipeline would.
        activeEffects.TryRefresh(nameof(BeingComponent.Armor), isBool: false, out _, out _);
        return instance;
    }

    private static ActivePropertyEffect AddBaseEffect(
        ActiveEffectsComponent activeEffects, string property, int amount)
    {
        var instance = CreatePropertyEffect(activeEffects,
            0, 0, 0, EffectType.ChangeProperty, property,
            combinationFunction: ValueCombinationFunction.Override,
            sourceAbilityActivation: ActivationType.Always,
            magnitudePrimary: amount);
        return activeEffects.AddPropertyEffect(instance);
    }

    // The being's own active Race carriers and their Always-activation grants - the set a polymorph suppresses.
    private static List<GameEntity> OwnRaceActivePassives(GameEntity beingEntity, GameManager manager)
    {
        var races = beingEntity.Being!.Races.ToList();
        var raceCarrierIds = races.Select(r => r.Id).ToHashSet();

        return races
            .Concat(beingEntity.ActiveEffects!.ComponentEffects
            .Where(r => r.EffectType == EffectType.AddAbility
                        && r.CarrierId != 0
                        && raceCarrierIds.Contains(r.SourceAbilityEntityId))
            .Select(r => manager.FindEntity(r.CarrierId)!))
            .Where(a => a.Ability is { IsActive: true, Activation: var activation }
                        && (activation & ActivationType.Always) != 0)
            .ToList();
    }

    private static void ConferLycanthropy(
        GameEntity targetEntity, string variantName, GameManager manager,
        EffectDuration duration = EffectDuration.Instant, string? durationAmount = null,
        string amount = "100*MightModifier()", string? secondaryAmount = null)
    {
        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.ConferLycanthropy;
            effect.Amount = amount;
            effect.Duration = duration;
            effect.DurationAmount = durationAmount;
            effect.SecondaryAmount = secondaryAmount;
            effect.TargetName = variantName;
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, targetEntity, targetEntity, manager);
        }
        manager.Queue.ProcessQueue(manager);
    }

    private static void RemoveRace(GameEntity targetEntity, string raceName, GameManager manager)
    {
        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.DecreaseAbilityLevel;
            effect.Duration = EffectDuration.Infinite;
            effect.Amount = "1";
            effect.TargetName = raceName;
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, targetEntity, targetEntity, manager);
        }
        manager.Queue.ProcessQueue(manager);
    }
}

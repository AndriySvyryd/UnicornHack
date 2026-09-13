using System.ComponentModel;
using System.Globalization;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataLoading;

namespace UnicornHack.Services.English;

public class EnglishLanguageService : ILanguageService
{
    protected virtual CultureInfo Culture
    {
        get;
    } = new(name: "en-US");

    private EnglishMorphologicalProcessor EnglishMorphologicalProcessor
    {
        get;
    } = new();

    private TaggingService TaggingService
    {
        get;
    } = new();

    #region Game concepts

    [return: NotNullIfNotNull("actorEntity")]
    public string? GetActorName(GameEntity? actorEntity, bool isIdentified)
        => GetString(actorEntity, EnglishPerson.Third, isIdentified, definiteDeterminer: null);

    public string GetDescription(string id, DescriptionCategory category)
    {
        switch (category)
        {
            case DescriptionCategory.Creature:
                return Creature.Loader.Get(id).EnglishDescription ?? "";
            case DescriptionCategory.Ability:
                return GetAbilityDescription(id);
            case DescriptionCategory.Effect:
                return Enum.TryParse<EffectType>(id, out var effectType)
                    ? GetEffectDescription(effectType)
                    : "Unknown effect";
            case DescriptionCategory.Property:
                return GetPropertyDescription(id);
        }

        return id;
    }

    public string GetPlayerChoiceText(
        string choice,
        PlayerChoiceText text,
        string? action = null,
        string? option = null)
    {
        if (choice != PlayerChoiceSystem.RaceChoice)
        {
            return "Unknown choice.";
        }

        return text switch
        {
            PlayerChoiceText.NoPendingChoice => "There is no race choice to resolve.",
            PlayerChoiceText.SourceUnavailable => "The race pedestal is no longer available.",
            PlayerChoiceText.StaleSelection => "Those race options are no longer available.",
            PlayerChoiceText.UnknownAction => "Unknown race pedestal action.",
            PlayerChoiceText.NoAvailableOptions => "There are no new races available.",
            PlayerChoiceText.NoOptionToReplace => "You have no race to replace.",
            PlayerChoiceText.OptionUnavailable => "That race is no longer available.",
            PlayerChoiceText.Title => action switch
            {
                null => "Race pedestal",
                PlayerChoiceSystem.AddAction => "Choose a race to add",
                PlayerChoiceSystem.RemoveAction => "Choose a race to replace",
                _ => "Choose"
            },
            PlayerChoiceText.OptionTitle => option == null
                ? action == PlayerChoiceSystem.AddAction
                    ? "Obtain a race"
                    : action == PlayerChoiceSystem.RemoveAction
                        ? "Replace a race"
                        : "Destroy the pedestal"
                : Capitalize(new StringBuilder(option)).ToString(),
            PlayerChoiceText.OptionDescription => option == null
                ? action == PlayerChoiceSystem.AddAction
                    ? "Choose a new race to add permanently."
                    : action == PlayerChoiceSystem.RemoveAction
                        ? "Choose one of your races to replace permanently."
                        : "Destroy the pedestal to gain a random race not shown among the other options."
                : $"{(action == PlayerChoiceSystem.AddAction ? "Gain" : "Replace")} "
                  + $"the <tooltip id='{option}' category='{(int)DescriptionCategory.Race}'>{option}</tooltip> race.",
            PlayerChoiceText.Notice => "Your selection is permanent.",
            _ => throw new ArgumentOutOfRangeException(nameof(text), text, message: null)
        };
    }

    public string PlayerChoiceRequired()
        => "Choose one of the presented options before taking another action.";

    public string InvalidPlayerChoiceSelection()
        => "The selected option was invalid.";

    public string PlayerChoiceApplicationFailed()
        => "The selected option could not be applied.";

    public string PlayerAbilityLevelChanged(
        string abilityName,
        AbilityType abilityType,
        int previousLevel,
        int currentLevel)
    {
        var ability = abilityType == AbilityType.Race
            ? RaceRef(abilityName)
            : AbilityRef(abilityName);
        var kind = abilityType switch
        {
            AbilityType.MeleeSpell or AbilityType.RangedSpell => "spell",
            AbilityType.Miracle => "miracle",
            AbilityType.Race => "race",
            AbilityType.Debuff => "debuff",
            AbilityType.Buff => "buff",
            AbilityType.Boon => "boon",
            AbilityType.Curse => "curse",
            AbilityType.Trait => "trait",
            AbilityType.Flaw => "flaw",
            AbilityType.Feat => "feat",
            AbilityType.Fault => "fault",
            AbilityType.Skill => "skill",
            AbilityType.Faith => "faith",
            _ => throw new ArgumentOutOfRangeException(nameof(abilityType), abilityType, message: null)
        };

        if (previousLevel == 0 && currentLevel > 0)
        {
            return $"You gain the {ability} {kind}.";
        }

        if (previousLevel > 0 && currentLevel <= 0)
        {
            return abilityType.IsStatusLike()
                ? $"Your {ability} {kind} ends."
                : $"You lose the {ability} {kind}.";
        }

        if (previousLevel != 0)
        {
            var verb = currentLevel > previousLevel
                ? "increases"
                : "decreases";
            return $"Your {ability} {kind} {verb} to level {currentLevel}.";
        }

        return $"Your {ability} {kind} is now level {currentLevel}.";
    }

    // Embeds a reference to a status ability so the client renders it as a nested tooltip (category 4 =
    // DescriptionCategory.Ability); hovering it shows that ability's own description.
    private static string AbilityRef(string abilityName, string? displayText = null)
        => $"<tooltip id='{abilityName}' category='{(int)DescriptionCategory.Ability}'>{displayText ?? abilityName}</tooltip>";

    private static string EffectRef(EffectType effectType, string? displayText = null)
        => $"<tooltip id='{effectType}' category='{(int)DescriptionCategory.Effect}'>{displayText ?? effectType.ToString()}</tooltip>";

    private static string RaceRef(string raceName)
        => $"<tooltip id='{raceName}' category='{(int)DescriptionCategory.Race}'>{raceName}</tooltip>";

    private string PropertyRef(string propertyName)
        => $"<tooltip id='{propertyName}' category='{(int)DescriptionCategory.Property}'>{GetPropertyName(propertyName, abbreviate: false)}</tooltip>";

    /// <summary>
    ///     A tooltip description for each template ability, keyed by its authored name (which doubles as its
    ///     tooltip topic id). Status abilities describe the condition they impose; the effects that apply
    ///     them link back here through <see cref="AbilityRef" />.
    /// </summary>
    protected virtual string GetAbilityDescription(string abilityName)
    {
        int? currentLevel = null;
        if (Ability.TryGetLeveledTemplateName(
                abilityName, out var templateName, out var parsedLevel))
        {
            currentLevel = parsedLevel;
            abilityName = templateName;
        }

        var description = GetBaseAbilityDescription(abilityName);
        if (Ability.Loader.Find(abilityName) is not LeveledAbility leveledAbility)
        {
            return description;
        }

        var result = new StringBuilder(description);
        foreach (var (level, effects) in leveledAbility.LeveledEffects.OrderBy(entry => entry.Key))
        {
            result.Append("\n\nLevel ").Append(level);
            if (!leveledAbility.Cumulative && level > 1)
            {
                result.Append(" (replaces earlier levels)");
            }

            if (currentLevel != null)
            {
                result.Append(level switch
                {
                    _ when level < currentLevel => " [active]",
                    _ when level == currentLevel => " [current]",
                    _ when level == currentLevel + 1 => " [next]",
                    _ => " [later]"
                });
            }

            result.Append(": ");
            var descriptions = new List<string>();
            if (GetStatusLevelBehaviorDescription(abilityName, level) is { } behaviorDescription)
            {
                descriptions.Add(behaviorDescription);
            }

            descriptions.AddRange(effects.Select(effect => effect switch
            {
                IChangeProperty property => DescribePropertyChange(effect, property.PropertyName),
                DecreaseAbilityLevel decrease => DescribeAbilityLevelDecrease(decrease),
                _ => effect.GetType().Name
            }));

            result.AppendJoin("; ", descriptions.Count == 0 ? ["No additional effects"] : descriptions);
            result.Append('.');
        }

        return result.ToString();
    }

    private static string? GetStatusLevelBehaviorDescription(string abilityName, int level)
        => (abilityName, level) switch
        {
            ("frightened", 1) => "Accuracy against the source is halved",
            ("frightened", 2) => "Cannot move closer to a perceived source",
            ("frightened", 3) => "Must make a random legal escape move away from the source",
            ("taunted", 1) => "Cannot move farther from a perceived source",
            ("taunted", 2) => "Attacks are redirected to the source",
            ("taunted", 3) => "Movement must approach the source and non-offensive actions are spent",
            ("muted", 1) => "Spell energy costs are doubled; follower command costs will double when follower commands are implemented",
            ("muted", 2) => "Verbal and divine ability energy costs are doubled",
            _ => null
        };

    private static string DescribeAbilityLevelDecrease(DecreaseAbilityLevel decrease)
    {
        var amount = decrease.Amount ?? "1";
        return decrease.AbilityType is { } abilityType
            ? $"each {abilityType} ability level -{amount}"
            : $"{decrease.AbilityName} level -{amount}";
    }

    private string DescribePropertyChange(Effect effect, string propertyName)
    {
        var property = PropertyRef(propertyName);
        var amount = effect.Amount ?? "0";
        var function = effect.CombinationFunction;
        if (function == ValueCombinationFunction.Default)
        {
            function = PropertyDescription.Loader.Find(propertyName)?.DefaultCombinationFunction;
        }

        return function switch
        {
            ValueCombinationFunction.Percent => $"{property} becomes {amount}% of its current value",
            ValueCombinationFunction.Override => $"{property} becomes {amount}",
            ValueCombinationFunction.Sum => $"{property} {(amount.StartsWith('-') ? amount : "+" + amount)}",
            _ => $"{property}: {function} {amount}"
        };
    }

    private static string GetBaseAbilityDescription(string abilityName)
        => abilityName switch
        {
            // Status conditions - what carrying the condition does to the afflicted being.
            "stunned" => "Skipping your next action and unable to evade.",
            "paralyzed" => "Physical status resistance falls; at level 2 you cannot act or evade.",
            "sleepy" => "Perception is reduced; at level 2 you cannot act or evade. Damage wakes you.",
            "fascinated" => "Mental status resistance falls; at level 2 you take no action until attacked.",
            "slowed" => "Your speed is reduced further at each level.",
            "crippled" => "Movement is hindered; severe crippling also reduces evasion.",
            "corroded" => "Armor is reduced; severe corrosion also lowers deflection and physical resistance.",
            "chilled" => "Speed and cold resistance fall while fire resistance rises; severe chilling also hinders movement.",
            "wetted" => "Fire resistance rises while shock and cold resistance fall; severe soaking also hinders movement.",
            "dazed" => "Perception, speed and evasion fall; higher levels progressively restrict available actions.",
            "deafened" => "Hearing is reduced, then removed; follower commands become unreliable when that system is available.",
            "blinded" => "Vision range is halved, then lost; level 3 also reduces accuracy and evasion.",
            "muted" => "Spell energy costs double; level 2 blocks spells and doubles verbal costs; level 3 blocks verbal abilities.",
            "exposed" => "Physical status resistance falls; evasion is halved at level 2 and removed at level 3.",
            "confused" => "Mental resistance and action telegraphs are lost; movement and legal action targets become increasingly unreliable.",
            "stuck" => "Item interactions are blocked; level 2 reduces evasion and level 3 prevents movement.",
            "bound" => "Movement is blocked; level 2 reduces evasion and level 3 prevents default attacks.",
            "drained" => "Health and energy regeneration stop; severe draining also slows ability recovery.",
            "staggered" => "Encumbered and lurching a step in a random direction after each action.",
            "unstable" => "Off-balance in melee; level 2 doubles the attack and evasion penalties.",
            "frightened" => "Attacks against the source are impaired; level 2 prevents approaching it.",
            "taunted" => "You cannot move away; level 2 redirects attacks to the source and level 3 requires approaching.",
            "greased" => "Quietness rises and physical immobilization is suppressed further at each level.",
            "warded" => "Each level suppresses one level of every other status ability.",
            "canceled" => "Each level suppresses one level of every buff ability.",
            "engulfed" => "Movement is blocked; level 2 redirects attacks and removes sight; level 3 reduces evasion.",
            "blighted" => "Health regeneration stops; higher levels add healing resistance.",
            "withered" => "Energy regeneration stops; higher levels halve and then block direct restoration.",
            "burning" => "Visibility rises at each level; severe burning also lowers fire resistance.",
            "decayed" => "Maximum recoverable health is reduced until enough experience is gained.",
            "enervated" => "Maximum recoverable energy is reduced until enough experience is gained.",
            "lycanthropic" => "A permanent curse that slowly builds toward temporary beastly transformation.",
            // Default attacks - the fallback strikes an actor makes with (or without) a weapon.
            "double melee attack" => "Your default melee attack, striking twice in the time a single blow would take.",
            "double ranged attack" => "Your default ranged attack, loosing twice in the time a single shot would take.",
            "one-handed melee attack" => "A melee attack made with a single one-handed weapon.",
            "one-handed ranged attack" => "A ranged attack made with a single one-handed weapon.",
            "two-handed melee attack" => "A melee attack made with a two-handed weapon.",
            "two-handed ranged attack" => "A ranged attack made with a two-handed weapon.",
            // Spells.
            "ice shard" => "Hurls a shard of ice at a distant foe, dealing cold damage that can " + AbilityRef(AbilityData.Chilled.Name, "chill") + ".",
            // Melee weapon skills - reach determines how far and wide you can strike.
            "hand weapons" => "Proficiency with hand-to-hand weapons, which strike the single cell in front of you.",
            "short weapons" => "Proficiency with short weapons, which can strike any of the three cells in front of you.",
            "medium weapons" => "Proficiency with medium weapons, which can strike any of the five cells in front of you.",
            "long weapons" => "Proficiency with long weapons, which reach up to two cells away. Longer weapons hit harder over a wider area but swing slower and louder.",
            // Ranged weapon skills - range band determines speed, noise and damage.
            "close range weapons" => "Proficiency with close-range weapons (darts, whips, grappling hooks): fast, silent, medium damage.",
            "short range weapons" => "Proficiency with short-range weapons (slingshots, wands): medium speed, low noise, low damage.",
            "medium range weapons" => "Proficiency with medium-range weapons (crossbows, rods): slow and loud, but high damage.",
            "long range weapons" => "Proficiency with long-range weapons (bows, staves): medium speed, noise and damage.",
            // Combat styles.
            "one handed" => "Fighting with a single one-handed weapon, leaving the other hand for a shield or item - a defensive style.",
            "two handed" => "Fighting with a two-handed weapon - an offensive style.",
            "dual wielding" => "Wielding a weapon in each hand and striking with both at once, at the slowest speed.",
            // Defense.
            "acrobatics" => "Fighting unarmored: no armor, but no hindrance to slow you either.",
            "light armor" => "Proficiency with light armor: low hindrance for modest protection.",
            "heavy armor" => "Proficiency with heavy armor: high protection at the cost of high hindrance.",
            // Magic sources - each unlocks and empowers the spells of its element.
            "fire sourcery" => "Command of the fire and light source, unlocking and empowering its spells.",
            "air sourcery" => "Command of the air and electricity source, unlocking and empowering its spells.",
            "blood sourcery" => "Command of the blood and life source, unlocking and empowering its spells.",
            "earth sourcery" => "Command of the earth and acid source, unlocking and empowering its spells.",
            "water sourcery" => "Command of the water and ice source, unlocking and empowering its spells.",
            "spirit sourcery" => "Command of the spirit and aether source, unlocking and empowering its spells.",
            // Magic schools - each unlocks and empowers the spells of its discipline.
            "evocation" => "The magic school of evocation: energy manipulation.",
            "conjuration" => "The magic school of conjuration: summoning creatures and items.",
            "transmutation" => "The magic school of transmutation: manipulating matter.",
            "enchantment" => "The magic school of enchantment: physical improvement.",
            "malediction" => "The magic school of malediction: physical deterioration.",
            "illusion" => "The magic school of illusion: manipulating knowledge and perception.",
            // Shadow arts and other skills.
            "stealth" => "The art of stealth, improving how close you can sense others and how hard you are to detect.",
            "assassination" => "The art of assassination, improving your read on how aware a target is of you.",
            "artifice" => "Skill with gadgets: no penalties for intricate or exotic gizmos, traps and orbs, and the means to set triggers on trinkets.",
            "leadership" => "The art of leadership: sensing others' disposition and giving commands to your followers.",
            _ => ""
        };

    protected virtual string GetEffectDescription(EffectType effectType)
        => effectType switch
        {
            // Damage over time
            EffectType.BleedDamage => "Losing health each time you act. Moving prolongs the bleeding.",
            EffectType.FireDamage => "Taking fire damage over time and easier to see. Getting wet puts it out.",
            EffectType.ToxinDamage => "Taking toxin damage over time; healing is halved and health regeneration is stopped.",
            EffectType.Wither => "Losing energy over time; energy regeneration is stopped.",
            // Instant damage - several also inflict a lingering status.
            EffectType.PhysicalDamage => "Physical harm.",
            EffectType.ColdDamage => $"Struck by cold, which can leave the victim {AbilityRef(AbilityData.Chilled.Name)}.",
            EffectType.ShockDamage => $"An electric shock that can {AbilityRef(AbilityData.Paralyzed.Name, "paralyze")}.",
            EffectType.WaterDamage => $"A drenching that leaves the victim {AbilityRef(AbilityData.Wetted.Name)} and puts out flames.",
            EffectType.LightDamage => $"Seared by light, which can {AbilityRef(AbilityData.Blinded.Name, "blind")}.",
            EffectType.SonicDamage => $"A deafening blast that can {AbilityRef(AbilityData.Dazed.Name, "daze")}.",
            EffectType.PsychicDamage => $"A psychic assault that can {AbilityRef(AbilityData.Confused.Name, "confuse")} the mind.",
            EffectType.AcidDamage => $"Corrosive acid that eats away at armor, leaving it {AbilityRef(AbilityData.Corroded.Name)}.",
            EffectType.DrainLife => "Health is stolen and given to the attacker.",
            EffectType.DrainEnergy => "Energy is stolen and given to the attacker.",
            // Status conditions - each inflicts its matching status ability, whose own tooltip describes it.
            EffectType.Stun => $"Leaves the target {AbilityRef(AbilityData.Stunned.Name)}.",
            EffectType.Paralyze => $"Leaves the target {AbilityRef(AbilityData.Paralyzed.Name)}.",
            EffectType.Sedate => $"Makes the target {AbilityRef(AbilityData.Sleepy.Name)}.",
            EffectType.Bind => $"Leaves the target {AbilityRef(AbilityData.Bound.Name)}.",
            EffectType.Stick => $"Leaves the target {AbilityRef(AbilityData.Stuck.Name)}.",
            EffectType.Engulf => $"Swallows the target, leaving it {AbilityRef(AbilityData.Engulfed.Name)}.",
            EffectType.Blind => $"Leaves the target {AbilityRef(AbilityData.Blinded.Name)}.",
            EffectType.Deafen => $"Leaves the target {AbilityRef(AbilityData.Deafened.Name)}.",
            EffectType.Confuse => $"Leaves the target {AbilityRef(AbilityData.Confused.Name)}.",
            EffectType.Slow => $"Leaves the target {AbilityRef(AbilityData.Slowed.Name)}.",
            EffectType.Cripple => $"Leaves the target {AbilityRef(AbilityData.Crippled.Name)}.",
            EffectType.Corrode => $"Leaves the target {AbilityRef(AbilityData.Corroded.Name)}.",
            EffectType.Daze => $"Leaves the target {AbilityRef(AbilityData.Dazed.Name)}.",
            EffectType.Chill => $"Leaves the target {AbilityRef(AbilityData.Chilled.Name)}.",
            EffectType.Wet => $"Leaves the target {AbilityRef(AbilityData.Wetted.Name)}.",
            EffectType.Expose => $"Leaves the target {AbilityRef(AbilityData.Exposed.Name)}.",
            EffectType.Topple => $"Builds {AbilityRef(AbilityData.Unstable.Name)} on the target.",
            EffectType.Fascinate => $"Leaves the target {AbilityRef(AbilityData.Fascinated.Name)}.",
            EffectType.Stagger => $"Leaves the target {AbilityRef(AbilityData.Staggered.Name)}.",
            EffectType.Frighten => $"Leaves the target {AbilityRef(AbilityData.Frightened.Name)}.",
            EffectType.Taunt => $"Leaves the target {AbilityRef(AbilityData.Taunted.Name)}.",
            EffectType.Mute => $"Leaves the target {AbilityRef(AbilityData.Muted.Name)}.",
            EffectType.Cancel => $"Leaves the target {AbilityRef(AbilityData.Canceled.Name)}.",
            // Counters
            EffectType.Doom => "A death sentence builds; if it ever exceeds your current health, you die at once. It slowly fades.",
            EffectType.Overcharge => "Arcane charge builds; at its peak it burns away all your energy, dealing damage equal to it.",
            EffectType.ChangeProperty => "A characteristic is temporarily altered.",
            EffectType.AddAbility => "Grants or improves an ability while its source remains active.",
            EffectType.DecreaseAbilityLevel => "Temporarily reduces an ability's effective level.",
            EffectType.Activate => "Activates another ability, weapon or projectile effect.",
            EffectType.EquipItem => "Equips the specified item in a compatible slot.",
            EffectType.RemoveItem => "Consumes and removes one item from its stack.",
            EffectType.Move => "Moves the specified item to the target creature or location.",
            EffectType.Teleport => "Teleports the target to another location.",
            EffectType.LevelTeleport => "Teleports the target to another level.",
            // Curses / transformation / misc afflictions
            EffectType.ConferLycanthropy => "Afflicted with a curse that forces a beastly transformation from time to time.",
            EffectType.Polymorph => "Transformed into another creature: you take on its form, stats and abilities until it wears off.",
            EffectType.Disarm => "Disarmed: your weapon is knocked from your grip.",
            EffectType.DisableItem => "One of your items has been rendered useless for a time.",
            EffectType.Suffocate => "Unable to breathe.",
            EffectType.Enervate => "Your energy can't be restored past a lowered maximum until you gain experience.",
            EffectType.Decay => "Your flesh decays: health can't be restored past a lowered maximum until you gain experience.",
            // Boons / beneficial
            EffectType.Heal => "Health is restored.",
            EffectType.Recharge => "Energy is restored.",
            EffectType.GainXP => "Experience is gained.",
            _ => ""
        };

    /// <summary>
    ///     A short tooltip description for each dynamic property that effects can alter, keyed by the
    ///     property name (which doubles as its tooltip topic id).
    /// </summary>
    protected virtual string GetPropertyDescription(string property)
        => property switch
        {
            // Vitals & resources
            nameof(BeingComponent.HitPointMaximum) => "The most health you can have.",
            nameof(BeingComponent.EnergyPointMaximum) => "The most energy you can have for casting and abilities.",
            nameof(BeingComponent.ReservedHitPoints) => "Health that temporarily can't be healed.",
            nameof(BeingComponent.ReservedEnergyPoints) => "Energy reserved by active abilities and effects.",
            nameof(BeingComponent.Regeneration) => "How quickly lost health is restored over time.",
            nameof(BeingComponent.EnergyRegeneration) => "Energy points restored per XP gained.",
            nameof(BeingComponent.HealingResistance) => "Percentage of healing prevented.",
            nameof(BeingComponent.EnergyRestorationResistance) => "Percentage of energy restoration prevented.",
            // Attributes
            nameof(BeingComponent.Might) => "Physical power - drives melee damage and carrying capacity.",
            nameof(BeingComponent.Speed) => "How quickly you act and move.",
            nameof(BeingComponent.Focus) => "Mental discipline - drives energy and spellcasting.",
            nameof(BeingComponent.Perception) => "Awareness - drives accuracy and noticing hidden things.",
            // Offense & defense
            nameof(BeingComponent.Accuracy) => "How likely your attacks are to land.",
            nameof(BeingComponent.Evasion) => "How likely you are to avoid incoming attacks.",
            nameof(BeingComponent.Deflection) => "Your chance to deflect an attack outright.",
            nameof(BeingComponent.Armor) => "Reduces the damage taken from physical hits.",
            nameof(BeingComponent.Hindrance) => "Encumbrance from your equipment that slows and impedes you.",
            // Capabilities
            nameof(BeingComponent.CanMove) => "Whether you can move to another cell.",
            nameof(BeingComponent.CanMoveItems) => "Whether you can pick up, drop, equip or unequip items.",
            nameof(BeingComponent.UnableToActPercent) =>
                "The percentage chance that an attempted action is replaced with waiting.",
            nameof(BeingComponent.CanCastSpells) => "Whether you can cast spells.",
            nameof(BeingComponent.CanUseVerbalAbilities) => "Whether you can use verbal abilities.",
            nameof(BeingComponent.CanUseDefaultAttacks) => "Whether you can use default attacks.",
            nameof(BeingComponent.CanUseComplexAbilities) => "Whether you can use complex abilities.",
            nameof(BeingComponent.CanUseNonDefaultAbilities) => "Whether you can use abilities other than default attacks.",
            nameof(BeingComponent.CanUseNonAttackAbilities) => "Whether you can use abilities that are not attacks.",
            nameof(BeingComponent.CanUseComplexFollowerCommands) =>
                "Whether complex follower commands can be issued.",
            nameof(BeingComponent.CanUseDivineAbilities) => "Whether divine abilities can be used.",
            nameof(BeingComponent.CanUseFollowerCommands) => "Whether follower commands can be issued.",
            nameof(BeingComponent.CanPerceiveIntention) => "Whether you can perceive other actors' planned actions.",
            // Status behavior
            nameof(BeingComponent.RandomMovementPercent) =>
                "The percentage of movement attempts that use a random direction.",
            nameof(BeingComponent.RandomMoveAfterActionPercent) =>
                "The percentage of actions followed by a delay-free random move.",
            nameof(BeingComponent.RandomActionPercent) =>
                "The percentage of attempted actions and targets that are randomized.",
            nameof(BeingComponent.EmitsSmoke) => "Emits smoke.",
            nameof(BeingComponent.FollowerCommandFailurePercent) =>
                "The percentage chance that follower commands fail.",
            // Resistances
            nameof(BeingComponent.PhysicalResistance) =>
                $"Reduces {EffectRef(EffectType.PhysicalDamage, "physical damage")} taken.",
            nameof(BeingComponent.PhysicalStatusResistance) => "Reduces buildup from physical status effects.",
            nameof(BeingComponent.MagicResistance) => "Reduces magical damage taken and resists hostile magic.",
            nameof(BeingComponent.MentalStatusResistance) => "Reduces buildup from mental status effects.",
            nameof(BeingComponent.AcidResistance) =>
                $"Reduces {EffectRef(EffectType.AcidDamage, "acid damage")} taken and protects your gear from {EffectRef(EffectType.Corrode, "corrosion")}.",
            nameof(BeingComponent.BleedingResistance) =>
                $"Reduces {EffectRef(EffectType.BleedDamage, "bleeding damage")} and the chance to bleed.",
            nameof(BeingComponent.ColdResistance) =>
                $"Reduces {EffectRef(EffectType.ColdDamage, "cold damage")} taken.",
            nameof(BeingComponent.ElectricityResistance) =>
                $"Reduces {EffectRef(EffectType.ShockDamage, "electrical damage")} taken.",
            nameof(BeingComponent.FireResistance) =>
                $"Reduces {EffectRef(EffectType.FireDamage, "fire damage")} taken.",
            nameof(BeingComponent.LightResistance) =>
                $"Reduces {EffectRef(EffectType.LightDamage, "light damage")} taken and the chance to be {EffectRef(EffectType.Blind, "blinded")}.",
            nameof(BeingComponent.PsychicResistance) =>
                $"Reduces {EffectRef(EffectType.PsychicDamage, "psychic damage")} taken and protects your mind.",
            nameof(BeingComponent.SonicResistance) =>
                $"Reduces {EffectRef(EffectType.SonicDamage, "sonic damage")} taken and the chance to be {EffectRef(EffectType.Deafen, "deafened")}.",
            nameof(BeingComponent.StunResistance) =>
                $"Reduces the chance and duration of being {EffectRef(EffectType.Stun, "stunned")}.",
            nameof(BeingComponent.ToxinResistance) =>
                $"Reduces {EffectRef(EffectType.ToxinDamage, "toxin damage")} taken and resists poison and blight.",
            nameof(BeingComponent.VoidResistance) =>
                $"Reduces {EffectRef(EffectType.Wither, "void damage")} taken.",
            nameof(BeingComponent.WaterResistance) =>
                $"Reduces {EffectRef(EffectType.WaterDamage, "water damage")} taken and the effects of {EffectRef(EffectType.Wet, "soaking")}.",
            // Body composition
            nameof(BeingComponent.HeadType) => "The kind of head you have, affecting what you can wear and do.",
            nameof(BeingComponent.TorsoType) => "The kind of torso you have, affecting what you can wear and do.",
            nameof(BeingComponent.UpperExtremities) => "Your upper limbs, affecting wielding and wearing.",
            nameof(BeingComponent.LowerExtremities) => "Your lower limbs, affecting movement and footwear.",
            nameof(BeingComponent.BackExtremities) => "Your back appendages, such as wings.",
            nameof(BeingComponent.DoomResistance) =>
                $"Resistance to {EffectRef(EffectType.Doom, "being doomed")}.",
            nameof(BeingComponent.BindResistance) =>
                $"Resistance to {EffectRef(EffectType.Bind, "being bound")}.",
            nameof(BeingComponent.BlindResistance) =>
                $"Resistance to {EffectRef(EffectType.Blind, "being blinded")}.",
            nameof(BeingComponent.CorrodeResistance) =>
                $"Resistance to {EffectRef(EffectType.Corrode, "being corroded")}.",
            nameof(BeingComponent.CrippleResistance) =>
                $"Resistance to {EffectRef(EffectType.Cripple, "being crippled")}.",
            nameof(BeingComponent.DazeResistance) =>
                $"Resistance to {EffectRef(EffectType.Daze, "being dazed")}.",
            nameof(BeingComponent.DeafenResistance) =>
                $"Resistance to {EffectRef(EffectType.Deafen, "being deafened")}.",
            nameof(BeingComponent.DecayResistance) =>
                $"Resistance to {EffectRef(EffectType.Decay, "decay")}.",
            nameof(BeingComponent.DisorientResistance) =>
                $"Resistance to {EffectRef(EffectType.Confuse, "being confused")}.",
            nameof(BeingComponent.EnervateResistance) =>
                $"Resistance to {EffectRef(EffectType.Enervate, "being enervated")}.",
            nameof(BeingComponent.EngulfResistance) =>
                $"Resistance to {EffectRef(EffectType.Engulf, "being engulfed")}.",
            nameof(BeingComponent.ExposeResistance) =>
                $"Resistance to {EffectRef(EffectType.Expose, "being exposed")}.",
            nameof(BeingComponent.FascinateResistance) =>
                $"Resistance to {EffectRef(EffectType.Fascinate, "being fascinated")}.",
            nameof(BeingComponent.FrightenResistance) =>
                $"Resistance to {EffectRef(EffectType.Frighten, "being frightened")}.",
            nameof(BeingComponent.MuteResistance) =>
                $"Resistance to {EffectRef(EffectType.Mute, "being muted")}.",
            nameof(BeingComponent.ParalyzeResistance) =>
                $"Resistance to {EffectRef(EffectType.Paralyze, "being paralyzed")}.",
            nameof(BeingComponent.SedateResistance) =>
                $"Resistance to {EffectRef(EffectType.Sedate, "becoming sleepy")}.",
            nameof(BeingComponent.SlowResistance) =>
                $"Resistance to {EffectRef(EffectType.Slow, "being slowed")}.",
            nameof(BeingComponent.StaggerResistance) =>
                $"Resistance to {EffectRef(EffectType.Stagger, "being staggered")}.",
            nameof(BeingComponent.StickResistance) =>
                $"Resistance to {EffectRef(EffectType.Stick, "being stuck")}.",
            nameof(BeingComponent.SuffocateResistance) =>
                $"Resistance to {EffectRef(EffectType.Suffocate, "suffocation")}.",
            nameof(BeingComponent.TauntResistance) =>
                $"Resistance to {EffectRef(EffectType.Taunt, "being taunted")}.",
            nameof(BeingComponent.ToppleResistance) =>
                $"Resistance to {EffectRef(EffectType.Topple, "becoming unstable")}.",
            nameof(BeingComponent.WetResistance) =>
                $"Resistance to {EffectRef(EffectType.Wet, "becoming wet")}.",
            nameof(BeingComponent.Visibility) => "How easily other creatures can see you.",
            nameof(BeingComponent.CooldownFactor) => "How long abilities take to recharge - lower is faster.",
            // Physical form
            nameof(PhysicalComponent.Size) => "Your physical size, in feet across.",
            nameof(PhysicalComponent.Weight) => "How much you weigh.",
            nameof(PhysicalComponent.Material) => "What your body is made of.",
            nameof(PhysicalComponent.Capacity) => "How many abilities you can keep readied at once.",
            nameof(PhysicalComponent.Infravisible) => "Whether you give off heat that infravision can see.",
            // Movement
            nameof(PositionComponent.MovementDelay) => "How long each step takes - lower is faster.",
            nameof(PositionComponent.TurningDelay) => "How long turning to face a new direction takes - lower is faster.",
            // Senses
            nameof(BeingComponent.PrimaryVisionRange) => "How far you can see clearly.",
            nameof(BeingComponent.SecondaryVisionRange) => "How far you can make out shapes at the edge of sight.",
            nameof(BeingComponent.HearingLevel) => "Hearing sensitivity.",
            nameof(BeingComponent.Quietness) => "How difficult your actions are to hear; higher is quieter.",
            nameof(BeingComponent.PrimaryFOVQuadrants) => "The width of your sharp central vision.",
            nameof(BeingComponent.TotalFOVQuadrants) => "The width of your whole field of view.",
            nameof(BeingComponent.Infravision) => "Lets you see the body heat of nearby creatures in the dark.",
            nameof(BeingComponent.InvisibilityDetection) => "Lets you sense creatures that are invisible.",
            nameof(AbilityComponent.Noise) => "How much using the ability reduces your effective quietness.",
            _ => ""
        };

    [return: NotNullIfNotNull("actorEntity")]
    protected virtual string? GetString(GameEntity? actorEntity, EnglishPerson person, SenseType sense)
        => GetString(
            actorEntity,
            person,
            sense.CanIdentify(),
            definiteDeterminer: true);

    [return: NotNullIfNotNull("actorEntity")]
    protected virtual string? GetString(
        GameEntity? actorEntity, EnglishPerson person, bool isIdentified, bool? definiteDeterminer)
    {
        if (actorEntity == null)
        {
            return null;
        }

        if (person == EnglishPerson.Second)
        {
            return EnglishMorphologicalProcessor.GetPronoun(
                EnglishPronounForm.Normal, EnglishNumber.Singular, person, gender: null);
        }

        if (!isIdentified)
        {
            return "something";
        }

        var properName = actorEntity.AI?.ProperName ?? actorEntity.Player?.ProperName;
        if (properName != null)
        {
            return properName;
        }

        var name = "";
        foreach (var raceEntity in actorEntity.Being!.Races)
        {
            name += " " + GetTemplateName(raceEntity.Ability!, abbreviate: false);
        }

        if (definiteDeterminer.HasValue)
        {
            name = (definiteDeterminer.Value ? "the" : "a") + name;
        }

        return name.Trim();
    }

    public virtual string GetString(ItemComponent item, int quantity, bool isIdentified)
        => GetString(item, quantity, isIdentified, definiteDeterminer: null);

    public string GetTemplateName(AbilityComponent race, bool abbreviate)
    {
        var name = GetRaceTemplateName(race);
        return abbreviate ? name[..Math.Min(2, name.Length)] : name;
    }

    private static string GetRaceTemplateName(AbilityComponent race)
    {
        var abilityName = race.Template?.Name ?? race.Name ?? "";
        var data = race.Entity.Manager.Game.Services.Data;
        return data.PlayerRaces.FindByIndex(abilityName)?.Name
               ?? data.Creatures.FindByIndex(abilityName)?.Name
               ?? abilityName;
    }

    public virtual string GetString(ItemComponent item, int quantity, SenseType sense, bool? definiteDeterminer)
        => GetString(item, quantity, sense.CanIdentify(), definiteDeterminer);

    public virtual string GetString(ItemComponent item, int quantity, bool isIdentified, bool? definiteDeterminer)
    {
        if (!isIdentified)
        {
            return "something";
        }

        var itemName = item.TemplateName + (item.Name == null ? "" : " named \"" + item.Name + "\"");
        if (quantity > 1)
        {
            return quantity + " " +
                   EnglishMorphologicalProcessor.ProcessNoun(itemName, EnglishNounForm.Plural);
        }

        return (EnglishMorphologicalProcessor.IsPlural(itemName) || definiteDeterminer == null
            ? ""
            : definiteDeterminer.Value
                ? "the "
                : EnglishMorphologicalProcessor.IsVocal(itemName[0])
                    ? "an "
                    : "a ") + itemName;
    }

    public virtual string GetPropertyName(string propertyName, bool abbreviate)
    {
        if (!abbreviate)
        {
            return propertyName;
        }

        switch (propertyName)
        {
            case nameof(BeingComponent.HitPoints):
                return "HP";
            case nameof(BeingComponent.EnergyPoints):
                return "MP";
            default:
                throw new NotImplementedException("Property " + propertyName + " not supported.");
        }
    }

    public virtual string GetString(AbilityComponent ability)
    {
        var name = ability.Name;
        if (name == null
            && ability.Action != AbilityAction.Default
            && ability.Action != AbilityAction.Modifier)
        {
            name = GetString(ability.Action);
        }

        if (name == null)
        {
            name = "attack";
        }

        return Capitalize(new StringBuilder(name)).ToString();
    }

    public virtual string GetString(EquipmentSlot slot, GameEntity actorEntity, bool abbreviate)
    {
        // TODO: take body shape into account
        switch (slot)
        {
            case EquipmentSlot.GraspPrimaryMelee:
                return abbreviate ? "MM" : "main hand for melee";
            case EquipmentSlot.GraspPrimaryRanged:
                return abbreviate ? "MR" : "main hand for ranged";
            case EquipmentSlot.GraspSecondaryMelee:
                return abbreviate ? "OM" : "off hand for melee";
            case EquipmentSlot.GraspSecondaryRanged:
                return abbreviate ? "OR" : "off hand for ranged";
            case EquipmentSlot.GraspSingleMelee:
                return abbreviate ? "SM" : "single hand for melee";
            case EquipmentSlot.GraspSingleRanged:
                return abbreviate ? "SR" : "single hand for ranged";
            case EquipmentSlot.GraspBothMelee:
                return abbreviate ? "BM" : "both hands for melee";
            case EquipmentSlot.GraspBothRanged:
                return abbreviate ? "BR" : "both hands for ranged";
            case EquipmentSlot.GraspMelee:
                return abbreviate ? "AM" : "single or both hands for melee";
            case EquipmentSlot.GraspRanged:
                return abbreviate ? "AR" : "single or both hands for ranged";
            case EquipmentSlot.GraspMouth:
                return abbreviate ? "M" : "mouth";
            case EquipmentSlot.Feet:
                return abbreviate ? "F" : "feet";
            case EquipmentSlot.Torso:
                return abbreviate ? "T" : "torso";
            case EquipmentSlot.Head:
                return abbreviate ? "H" : "head";
            case EquipmentSlot.Hands:
                return abbreviate ? "G" : "hands";
            case EquipmentSlot.Neck:
                return abbreviate ? "N" : "neck";
            case EquipmentSlot.Back:
                return abbreviate ? "B" : "back";
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }
    }

    private string PrepositionFor(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.GraspPrimaryMelee:
            case EquipmentSlot.GraspPrimaryRanged:
            case EquipmentSlot.GraspSecondaryMelee:
            case EquipmentSlot.GraspSecondaryRanged:
            case EquipmentSlot.GraspMouth:
                return "in the";
            case EquipmentSlot.GraspBothMelee:
            case EquipmentSlot.GraspBothRanged:
                return "in";
            case EquipmentSlot.Feet:
            case EquipmentSlot.Torso:
            case EquipmentSlot.Head:
            case EquipmentSlot.Hands:
            case EquipmentSlot.Neck:
            case EquipmentSlot.Back:
                return "on the";
            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }
    }

    protected virtual string GetString(AbilityAction abilityAction)
    {
        string verb;
        switch (abilityAction)
        {
            case AbilityAction.Punch:
                verb = "punch";
                break;
            case AbilityAction.Kick:
                verb = "kick";
                break;
            case AbilityAction.Touch:
                verb = "touch";
                break;
            case AbilityAction.Headbutt:
                verb = "headbutt";
                break;
            case AbilityAction.Claw:
                verb = "claw";
                break;
            case AbilityAction.Bite:
                verb = "bite";
                break;
            case AbilityAction.Suck:
                verb = "suck";
                break;
            case AbilityAction.Sting:
                verb = "sting";
                break;
            case AbilityAction.Hug:
                verb = "squeeze";
                break;
            case AbilityAction.Trample:
                verb = "trample";
                break;
            case AbilityAction.Spit:
                verb = "spit at";
                break;
            case AbilityAction.Throw:
                verb = "throw";
                break;
            case AbilityAction.Shoot:
                verb = "shoot at";
                break;
            case AbilityAction.Digestion:
                verb = "digest";
                break;
            case AbilityAction.Spell:
                verb = "cast a spell at";
                break;
            case AbilityAction.Breath:
                verb = "breath at";
                break;
            case AbilityAction.Gaze:
                verb = "gaze at";
                break;
            case AbilityAction.Scream:
                verb = "scream at";
                break;
            case AbilityAction.Screech:
                verb = "screech at";
                break;
            case AbilityAction.Explosion:
                verb = "explode";
                break;
            case AbilityAction.Chop:
                verb = "chop";
                break;
            case AbilityAction.Bludgeon:
                verb = "bludgeon";
                break;
            case AbilityAction.Slash:
                verb = "slash";
                break;
            case AbilityAction.Poke:
                verb = "poke";
                break;
            case AbilityAction.Hit:
                verb = "hit";
                break;
            case AbilityAction.Impale:
                verb = "impale";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(abilityAction), abilityAction, message: null);
        }

        return verb;
    }

    public virtual string GetString(Direction direction, bool abbreviate)
    {
        if (abbreviate)
        {
            switch (direction)
            {
                case Direction.North:
                    return "N";
                case Direction.South:
                    return "S";
                case Direction.West:
                    return "W";
                case Direction.East:
                    return "E";
                case Direction.Northwest:
                    return "NW";
                case Direction.Northeast:
                    return "NE";
                case Direction.Southwest:
                    return "SW";
                case Direction.Southeast:
                    return "SE";
                case Direction.Up:
                    return "U";
                case Direction.Down:
                    return "D";
            }
        }

        switch (direction)
        {
            case Direction.North:
                return "north";
            case Direction.South:
                return "south";
            case Direction.West:
                return "west";
            case Direction.East:
                return "east";
            case Direction.Northwest:
                return "northwest";
            case Direction.Northeast:
                return "northeast";
            case Direction.Southwest:
                return "southwest";
            case Direction.Southeast:
                return "southeast";
            case Direction.Up:
                return "up";
            case Direction.Down:
                return "down";
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, message: null);
        }
    }

    #endregion

    #region Events

    public virtual string? GetString(in AttackEvent @event)
    {
        if (@event.SensorEntity != @event.VictimEntity
            && @event.SensorEntity != @event.AttackerEntity
            && !@event.AttackerSensed.CanIdentify())
        {
            if ((!@event.Ranged
                 || (@event.VictimSensed &
                     (SenseType.Sight | SenseType.Touch | SenseType.Sound | SenseType.SoundDistant)) == 0)
                && (@event.AttackerSensed & SenseType.Sound) == 0
                && (@event.AttackerSensed & SenseType.SoundDistant) == 0)
            {
                return null;
            }

            if (!@event.Ranged
                || !@event.VictimSensed.CanIdentify())
            {
                return ToSentence(
                    "You hear a",
                    (@event.AttackerSensed & SenseType.Sound) == 0
                    && (@event.VictimSensed & SenseType.Sound) == 0
                        ? "distant"
                        : null,
                    @event.AbilityAction == AbilityAction.Scream
                    || @event.AbilityAction == AbilityAction.Screech
                        ? "scream"
                        : "noise");
            }
        }

        var attackerPerson = @event.SensorEntity == @event.AttackerEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var attacker = GetString(@event.AttackerEntity, attackerPerson, @event.AttackerSensed);

        var victimGender = (EnglishGender?)@event.VictimEntity?.Being?.Sex;
        var victimPerson = @event.SensorEntity == @event.VictimEntity ? EnglishPerson.Second : EnglishPerson.Third;
        var victim = GetString(@event.VictimEntity, victimPerson, @event.VictimSensed);

        var attackVerb = GetString(@event.AbilityAction);
        var mainVerbForm = attackerPerson == EnglishPerson.Third
            ? EnglishVerbForm.ThirdPersonSingularPresent
            : EnglishVerbForm.BareInfinitive;

        var weapon = @event.WeaponEntity?.Item;
        if (weapon == null
            || !@event.Ranged)
        {
            if (victim == null)
            {
                return ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                    "the air",
                    @event.SensorEntity == @event.VictimEntity ? "!" : ".");
            }

            if (@event.Hit)
            {
                var attackSentence = ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                    @event.AttackerEntity != @event.VictimEntity
                        ? victim
                        : EnglishMorphologicalProcessor.GetPronoun(EnglishPronounForm.Reflexive,
                            EnglishNumber.Singular,
                            attackerPerson, victimGender),
                    @event.SensorEntity == @event.VictimEntity ? "!" : ".");

                if (!@event.VictimSensed.CanIdentify())
                {
                    return attackSentence;
                }

                return attackSentence + " " + FormatDamage(
                    @event.ResourceChanges,
                    victimPerson == EnglishPerson.Second,
                    EnglishMorphologicalProcessor.GetPronoun(
                        EnglishPronounForm.Normal, EnglishNumber.Singular, victimPerson, victimGender));
            }

            return ToSentence(attacker,
                EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "try", mainVerbForm),
                EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.Infinitive),
                victim,
                ", but",
                EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "miss", mainVerbForm));
        }

        var isProjectile = (weapon.Type & ItemType.WeaponProjectile) != 0;
        if (@event.AbilityAction == AbilityAction.Throw
            || @event.AbilityAction == AbilityAction.Shoot)
        {
            return null;
        }

        if (@event.Hit)
        {
            var attackSentence = ToSentence(
                GetString(weapon, 1, @event.AttackerSensed,
                    definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second),
                EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.ThirdPersonSingularPresent),
                @event.AttackerEntity != @event.VictimEntity
                    ? victim
                    : EnglishMorphologicalProcessor.GetPronoun(
                        EnglishPronounForm.Reflexive, EnglishNumber.Singular, attackerPerson, victimGender),
                @event.SensorEntity == @event.VictimEntity ? "!" : ".");

            if (!@event.VictimSensed.CanIdentify())
            {
                return attackSentence;
            }

            return attackSentence + " " + FormatDamage(
                @event.ResourceChanges,
                victimPerson == EnglishPerson.Second,
                EnglishMorphologicalProcessor.GetPronoun(
                    EnglishPronounForm.Normal, EnglishNumber.Singular, victimPerson, victimGender));
        }

        return victim == null
            ? null
            : ToSentence(
                GetString(weapon, 1, @event.AttackerSensed,
                    definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second),
                EnglishMorphologicalProcessor.ProcessVerb("miss", EnglishVerbForm.ThirdPersonSingularPresent),
                victim);
    }

    protected virtual string FormatDamage(
        IReadOnlyDictionary<EffectType, int> resourceChanges, bool reflexive, string victimPronoun)
    {
        var damageTag = TaggingService.GetDamageTag(resourceChanges, out var damageTaken);

        return damageTaken
            ? string.Format(Culture, reflexive ? "[{0} pts.]" : "({0} pts.)", damageTag)
            : ToSentence(victimPronoun, reflexive ? "are" : "is", "unaffected.");
    }

    public virtual string GetString(in DeathEvent @event)
    {
        var deceasedPerson = @event.SensorEntity == @event.DeceasedEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        return ToSentence(
            GetString(@event.DeceasedEntity, deceasedPerson, @event.DeceasedSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "die", deceasedPerson),
            deceasedPerson == EnglishPerson.Second ? "!" : ".");
    }

    public virtual string GetString(in StatusActivatedEvent @event)
    {
        var targetPerson = @event.SensorEntity == @event.TargetEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var sentence = ToSentence(
            GetString(@event.TargetEntity, targetPerson, @event.TargetSensed),
            targetPerson == EnglishPerson.Second ? "are" : "is",
            @event.StatusName,
            targetPerson == EnglishPerson.Second ? "!" : ".");

        // Only the afflicted player is told the mechanical detail of how the status affects them; an
        // onlooker just learns that the condition has taken hold.
        return targetPerson == EnglishPerson.Second
            ? ToSentence(sentence, GetAbilityDescription(@event.StatusName))
            : sentence;
    }

    public virtual string GetString(in LingeringAttackEvent @event)
    {
        var sourcePerson = @event.SensorEntity == @event.SourceEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var targetPerson = @event.SensorEntity == @event.TargetEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var source = GetString(@event.SourceEntity, sourcePerson, @event.SourceSensed);
        var target = GetString(@event.TargetEntity, targetPerson, @event.TargetSensed);
        var clauses = @event.Effects.Select(effect => GetClause(effect.Key));

        return ToSentence(source, string.Join(" and ", clauses));

        string GetClause(EffectType effectType)
        {
            var verbForm = sourcePerson == EnglishPerson.Third
                ? EnglishVerbForm.ThirdPersonSingularPresent
                : EnglishVerbForm.BareInfinitive;

            string Conjugate(string verb)
                => EnglishMorphologicalProcessor.ProcessVerb(verb, verbForm);

            return effectType switch
            {
                EffectType.AcidDamage => $"{Conjugate("coat")} {target} in acid",
                EffectType.BleedDamage => $"{Conjugate("lacerate")} {target}",
                EffectType.ToxinDamage => $"{Conjugate("poison")} {target}",
                EffectType.FireDamage => $"{Conjugate("set")} {target} on fire",
                EffectType.DrainEnergy => $"{Conjugate("start")} draining energy from {target}",
                EffectType.DrainLife => $"{Conjugate("start")} draining life from {target}",
                EffectType.ColdDamage => $"{Conjugate("wrap")} {target} in frost",
                EffectType.LightDamage => $"{Conjugate("surround")} {target} with radiance",
                EffectType.PhysicalDamage => $"{Conjugate("crush")} {target}",
                EffectType.PsychicDamage => $"{Conjugate("fill")} {target} with anguish",
                EffectType.ShockDamage => $"{Conjugate("shock")} {target}",
                EffectType.WaterDamage => $"{Conjugate("soak")} {target}",
                EffectType.SonicDamage => $"{Conjugate("rattle")} {target}",
                EffectType.Wither => $"{Conjugate("cause")} {target} to wither",
                EffectType.GainXP => $"{Conjugate("awaken")} new insight in {target}",
                EffectType.Heal => $"{Conjugate("restore")} {target}",
                EffectType.Recharge => $"{Conjugate("invigorate")} {target}",
                _ => $"{Conjugate("apply")} continuous "
                     + $"{CSScriptLoaderHelpers.GetNameFromIdentifier(effectType.ToString())} to {target}"
            };
        }
    }

    public virtual string GetString(in ResourceEffectEvent @event)
    {
        var targetPerson = @event.SensorEntity == @event.TargetEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var target = GetString(@event.TargetEntity, targetPerson, @event.TargetSensed);
        var resourceChanges = @event.ResourceChanges;
        var clauses = new List<string>();
        AppendDamageClause(nameof(BeingComponent.HitPoints), "take", "damage");
        AppendDamageClause(nameof(BeingComponent.EnergyPoints), "lose", "energy");
        AppendPositiveClause(nameof(BeingComponent.HitPoints), "restore", "health");
        AppendPositiveClause(nameof(BeingComponent.EnergyPoints), "restore", "energy");
        AppendPositiveClause(nameof(BeingComponent.ExperiencePoints), "gain", "experience");

        return ToSentence(target, string.Join(" and ", clauses));

        void AppendDamageClause(string resourceProperty, string verb, string resourceName)
        {
            var damageTag = TaggingService.GetDamageTag(
                resourceChanges, out var damageTaken, resourceProperty);
            if (damageTaken)
            {
                clauses.Add(string.Join(' ',
                    EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verb, targetPerson),
                    damageTag, resourceName));
            }
        }

        void AppendPositiveClause(string resourceProperty, string verb, string resourceName)
        {
            var amount = 0;
            foreach (var (effectType, resourceAmount) in resourceChanges)
            {
                if (resourceAmount > 0
                    && Effect.GetMetadata(effectType).ResourceProperty == resourceProperty)
                {
                    amount += resourceAmount;
                }
            }

            if (amount != 0)
            {
                clauses.Add(string.Join(' ',
                    EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verb, targetPerson),
                    amount.ToString(Culture), resourceName));
            }
        }
    }

    public virtual string GetString(in ItemPickUpEvent @event)
    {
        var pickerPerson = @event.SensorEntity == @event.PickerEntity ? EnglishPerson.Second : EnglishPerson.Third;
        return ToSentence(
            GetString(@event.PickerEntity, pickerPerson, @event.PickerSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "pick up", pickerPerson),
            GetString(@event.ItemEntity.Item!, @event.Quantity, @event.ItemSensed,
                definiteDeterminer: pickerPerson == EnglishPerson.Second));
    }

    public virtual string GetString(in ItemDropEvent @event)
    {
        var dropperPerson = @event.SensorEntity == @event.DropperEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        return ToSentence(
            GetString(@event.DropperEntity, dropperPerson, @event.DropperSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "drop", dropperPerson),
            GetString(@event.ItemEntity.Item!, @event.Quantity, @event.ItemSensed,
                definiteDeterminer: dropperPerson == EnglishPerson.Second));
    }

    public virtual string GetString(in ItemActivationEvent @event)
    {
        var consumerPerson = @event.SensorEntity == @event.ActivatorEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var verb = (@event.ItemEntity.Item!.Type & ItemType.Potion) != 0
            ? @event.Consumed ? "drink" : "drink from"
            : (@event.ItemEntity.Item.Type & ItemType.SkillBook) != 0
                ? "read"
                : "activate";

        return ToSentence(
            GetString(@event.ActivatorEntity, consumerPerson, @event.ActivatorSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verb, consumerPerson),
            GetString(@event.ItemEntity.Item, 1, @event.ItemSensed,
                definiteDeterminer: consumerPerson == EnglishPerson.Second));
    }

    public virtual string GetString(in ItemEquipmentEvent @event)
    {
        var equipperPerson = @event.SensorEntity == @event.EquipperEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;

        if (@event.Slot == EquipmentSlot.None)
        {
            return ToSentence(
                GetString(@event.EquipperEntity, equipperPerson, @event.EquipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "unequip", equipperPerson),
                GetString(@event.ItemEntity.Item!, 1, @event.ItemSensed,
                    definiteDeterminer: equipperPerson == EnglishPerson.Second));
        }

        var slotKnown = (@event.EquipperSensed & SenseType.Sight) != 0
                        || (@event.EquipperSensed & SenseType.Touch) != 0;
        return ToSentence(
            GetString(@event.EquipperEntity, equipperPerson, @event.EquipperSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "equip", equipperPerson),
            GetString(@event.ItemEntity.Item!, 1, @event.ItemSensed,
                definiteDeterminer: equipperPerson == EnglishPerson.Second),
            slotKnown ? PrepositionFor(@event.Slot) : null,
            slotKnown ? GetString(@event.Slot, @event.EquipperEntity, abbreviate: false) : null);
    }

    public string NoDefaultAttack(bool melee)
    {
        var weaponType = melee ? "melee" : "ranged";
        return $"Can't attack. You don't have a {weaponType} weapon equipped.";
    }

    public string NoAbilityInSlot(int slot)
        => $"No ability in slot {slot}.";

    public string AbilityCannotBeActivated(AbilityComponent ability)
        => $"Can't use {GetString(ability)}.";

    public string AbilityNotSlotted(AbilityComponent ability)
        => $"{GetString(ability)} isn't assigned to a slot.";

    public string AbilityAlreadyActive(AbilityComponent ability)
        => $"{GetString(ability)} is already active.";

    public string AbilityOnCooldown(AbilityComponent ability)
        => $"{GetString(ability)} is on cooldown.";

    public string AbilityNotUsable(AbilityComponent ability)
        => $"Can't use {GetString(ability)}.";

    public string CannotCastAbility(AbilityComponent ability)
        => $"Can't cast {GetString(ability)}.";

    public string CannotUseVerbalAbility(AbilityComponent ability)
        => $"Can't use verbal ability {GetString(ability)}.";

    public string CannotUseDivineAbility(AbilityComponent ability)
        => $"Can't use divine ability {GetString(ability)}.";

    public string CannotUseDefaultAttack(AbilityComponent ability)
        => $"Can't use default attack {GetString(ability)}.";

    public string CannotUseNonDefaultAbility(AbilityComponent ability)
        => $"Can't use non-default ability {GetString(ability)}.";

    public string CannotUseComplexAbility(AbilityComponent ability)
        => $"Can't use complex ability {GetString(ability)}.";

    public string CannotUseNonAttackAbility(AbilityComponent ability)
        => $"Can't use non-attack ability {GetString(ability)}.";

    public string InvalidDigestionTarget(AbilityComponent ability)
        => $"{GetString(ability)} can only target a creature engulfed by you.";

    public string AbilityHasNoEffects(AbilityComponent ability)
        => $"{GetString(ability)} has no effects.";

    public string NotEnoughEnergy(AbilityComponent ability)
        => $"Not enough energy to use {GetString(ability)}.";

    public string TooSlowToUseAbility(AbilityComponent ability)
        => $"You are too slow to use {GetString(ability)}.";

    public string AbilityRequiresTarget(AbilityComponent ability)
        => $"{GetString(ability)} requires a target.";

    public string CannotTurnToUseAbility(AbilityComponent ability)
        => $"You can't turn to use {GetString(ability)}.";

    public string StatusPreventsAction(AbilityComponent statusAbility)
        => $"You are {GetStatusName(statusAbility)} and unable to act.";

    public string StatusPreventsAttemptedAction(AbilityComponent statusAbility)
        => $"You are {GetStatusName(statusAbility)} and can't perform that action.";

    public string StatusPreventsMovement(AbilityComponent statusAbility)
        => $"You are {GetStatusName(statusAbility)} and can't move.";

    public string StatusPreventsItemManipulation(AbilityComponent statusAbility)
        => $"You are {GetStatusName(statusAbility)} and can't manipulate items.";

    public string StatusPreventsAbility(AbilityComponent statusAbility, AbilityComponent ability)
        => $"You are {GetStatusName(statusAbility)} and can't use {GetString(ability)}.";

    private static string GetStatusName(AbilityComponent statusAbility)
        => statusAbility.Template?.Name ?? statusAbility.Name ?? "affected";

    public string ItemNotInInventory()
        => "You don't have that item.";

    public string InventoryFull()
        => "You can't carry any more items.";

    public string NothingToPickUp()
        => "There's nothing to pick up here.";

    public string NoPath() => "No path to target!";

    public string TooAfraidToApproach(GameEntity source)
        => $"You are too afraid to approach {GetActorName(source, isIdentified: true)}.";

    public string CannotFleeSource(GameEntity source)
        => $"You can't bring yourself to move away from {GetActorName(source, isIdentified: true)}.";

    #endregion

    #region Interface messages

    public virtual string Welcome(GameEntity playerEntity)
        => string.Format(Culture,
            "Welcome to the {0}, {1}!",
            playerEntity.Position!.RoomEntity.Room!.Level.Branch.Name,
            GetString(playerEntity, EnglishPerson.Third, SenseType.Sight));

    public virtual string UnableToMove(Direction direction)
        => string.Format(Culture, "Can't move {0}.", GetString(direction, abbreviate: false));

    public virtual string SoundFrom(Direction direction)
        => string.Format(Culture, "You hear a sound from the {0}.", GetString(direction, abbreviate: false));

    #endregion

    #region Formatting

    protected virtual string ToSentence(params string?[] components)
    {
        var builder = new StringBuilder();

        foreach (var component in components)
        {
            if (!string.IsNullOrEmpty(component))
            {
                builder.Append(component).Append(value: " ");
            }
        }

        builder.Remove(builder.Length - 1, length: 1);

        Capitalize(builder);

        if (!char.IsPunctuation(builder[^1]))
        {
            builder.Append(value: '.');
        }

        builder.Replace(oldValue: " ,", newValue: ",").Replace(oldValue: " .", newValue: ".")
            .Replace(oldValue: " .", newValue: ".").Replace(oldValue: " !", newValue: "!");

        return builder.ToString();
    }

    private static StringBuilder Capitalize(StringBuilder builder)
    {
        if (builder.Length == 0)
        {
            return builder;
        }

        var first = builder[index: 0];
        builder.Remove(startIndex: 0, length: 1).Insert(index: 0, value: char.ToUpper(first));

        return builder;
    }

    #endregion

    #region Map screen

    public string GetRoomTooltip(
        RoomFeatures features,
        bool isCurrent,
        bool isVisited,
        int revealedActorCount,
        int revealedItemCount,
        string? strongestActorName)
    {
        var sb = new StringBuilder();

        var header = isCurrent ? "Current room"
            : isVisited ? "Visited room"
            : "Unexplored room";
        sb.Append("<b>").Append(header).Append("</b>");

        var featureNames = GetRoomFeatureNames(features);
        if (featureNames.Length > 0)
        {
            sb.AppendLine().Append("Features: ").Append(featureNames);
        }

        if (revealedActorCount >= 0)
        {
            sb.AppendLine().Append("Creatures: ").Append(revealedActorCount.ToString(Culture));
        }

        if (revealedItemCount >= 0)
        {
            sb.AppendLine().Append("Items: ").Append(revealedItemCount.ToString(Culture));
        }

        if (!string.IsNullOrEmpty(strongestActorName))
        {
            sb.AppendLine().Append("Strongest: ").Append(strongestActorName);
        }

        return sb.ToString();
    }

    private static string GetRoomFeatureNames(RoomFeatures features)
    {
        if (features == RoomFeatures.Normal)
        {
            return "";
        }

        var parts = new List<string>();
        AppendIf(features, RoomFeatures.Entrance, "Entrance", parts);
        AppendIf(features, RoomFeatures.Exit, "Exit", parts);
        AppendIf(features, RoomFeatures.Boss, "Boss", parts);
        AppendIf(features, RoomFeatures.Cursed, "Cursed", parts);
        AppendIf(features, RoomFeatures.Challenge, "Challenge", parts);
        AppendIf(features, RoomFeatures.DeityAltar, "Deity altar", parts);
        AppendIf(features, RoomFeatures.SkillPedestal, "Skill pedestal", parts);
        AppendIf(features, RoomFeatures.TraitPedestal, "Trait pedestal", parts);
        AppendIf(features, RoomFeatures.FeatPedestal, "Feat pedestal", parts);
        AppendIf(features, RoomFeatures.RacePedestal, "Race pedestal", parts);
        AppendIf(features, RoomFeatures.WeaponStand, "Weapon stand", parts);
        AppendIf(features, RoomFeatures.ArmorStand, "Armor stand", parts);
        AppendIf(features, RoomFeatures.ItemCase, "Item case", parts);
        AppendIf(features, RoomFeatures.SmallFountain, "Fountain", parts);
        AppendIf(features, RoomFeatures.BigFountain, "Big fountain", parts);
        AppendIf(features, RoomFeatures.MinorShrine, "Minor shrine", parts);
        AppendIf(features, RoomFeatures.MajorShrine, "Major shrine", parts);
        AppendIf(features, RoomFeatures.RitualShrine, "Ritual shrine", parts);
        AppendIf(features, RoomFeatures.Shortcut, "Shortcut", parts);
        AppendIf(features, RoomFeatures.Secret, "Secret", parts);
        return string.Join(", ", parts);

        static void AppendIf(RoomFeatures features, RoomFeatures flag, string name, List<string> target)
        {
            if ((features & flag) != 0)
            {
                target.Add(name);
            }
        }
    }

    #endregion

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString() => base.ToString();
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;

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

    public string GetActorName(GameEntity actorEntity, SenseType sense)
        => GetString(actorEntity, EnglishPerson.Third, sense, definiteDeterminer: null);

    public string GetDescription(string id, DescriptionCategory category)
    {
        switch (category)
        {
            case DescriptionCategory.Creature:
                return Creature.Loader.Get(id).EnglishDescription;
        }

        return "";
    }

    protected virtual string GetString(GameEntity actorEntity, EnglishPerson person, SenseType sense)
        => GetString(
            actorEntity,
            person,
            sense,
            definiteDeterminer: true);

    protected virtual string GetString(
        GameEntity actorEntity, EnglishPerson person, SenseType sense, bool? definiteDeterminer)
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

        if (!sense.CanIdentify())
        {
            return "something";
        }

        var properName = actorEntity.AI?.ProperName ?? actorEntity.Player?.ProperName;
        if (properName != null)
        {
            return properName;
        }

        var name = "";
        foreach (var raceEntity in actorEntity.Being.Races)
        {
            name += " " + raceEntity.Race.TemplateName;
        }

        if (definiteDeterminer.HasValue)
        {
            name = (definiteDeterminer.Value ? "the" : "a") + name;
        }

        return name.Trim();
    }

    public virtual string GetString(ItemComponent item, int quantity, SenseType sense)
        => GetString(item, quantity, sense, definiteDeterminer: null);

    public string GetString(RaceComponent race, bool abbreviate)
        => abbreviate
            ? race.TemplateName.Substring(0, 2)
            : race.TemplateName;

    public virtual string GetString(ItemComponent item, int quantity, SenseType sense, bool? definiteDeterminer)
    {
        if (!sense.CanIdentify())
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

    public virtual string GetString(in AttackEvent @event)
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
                    @event.AppliedEffects,
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
                @event.AppliedEffects,
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

    protected virtual string FormatDamage(IReadOnlyList<GameEntity> effects, bool reflexive, string victimPronoun)
    {
        // TODO: EffectType.DrainEnergy
        var damageTag = TaggingService.GetDamageTag(effects, out var damageTaken);

        return damageTaken
            ? Format(reflexive ? "[{0} pts.]" : "({0} pts.)", damageTag)
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

    public virtual string GetString(in ItemPickUpEvent @event)
    {
        var pickerPerson = @event.SensorEntity == @event.PickerEntity ? EnglishPerson.Second : EnglishPerson.Third;
        return ToSentence(
            GetString(@event.PickerEntity, pickerPerson, @event.PickerSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "pick up", pickerPerson),
            GetString(@event.ItemEntity.Item, @event.Quantity, @event.ItemSensed,
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
            GetString(@event.ItemEntity.Item, @event.Quantity, @event.ItemSensed,
                definiteDeterminer: dropperPerson == EnglishPerson.Second));
    }

    public virtual string GetString(in ItemActivationEvent @event)
    {
        var consumerPerson = @event.SensorEntity == @event.ActivatorEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var verb = (@event.ItemEntity.Item.Type & ItemType.Potion) != 0
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
                GetString(@event.ItemEntity.Item, 1, @event.ItemSensed,
                    definiteDeterminer: equipperPerson == EnglishPerson.Second));
        }

        var slotKnown = (@event.EquipperSensed & SenseType.Sight) != 0
                        || (@event.EquipperSensed & SenseType.Touch) != 0;
        return ToSentence(
            GetString(@event.EquipperEntity, equipperPerson, @event.EquipperSensed),
            EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "equip", equipperPerson),
            GetString(@event.ItemEntity.Item, 1, @event.ItemSensed,
                definiteDeterminer: equipperPerson == EnglishPerson.Second),
            slotKnown ? PrepositionFor(@event.Slot) : null,
            slotKnown ? GetString(@event.Slot, @event.EquipperEntity, abbreviate: false) : null);
    }

    public string GetString(in LeveledUpEvent @event)
    {
        var leveledPerson = @event.SensorEntity == @event.LeveledEntity
            ? EnglishPerson.Second
            : EnglishPerson.Third;
        var leveledGender = (EnglishGender?)@event.LeveledEntity.Being.Sex;

        return ToSentence(
                   GetString(@event.LeveledEntity, leveledPerson, SenseType.Sight),
                   EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "level up", leveledPerson),
                   "!") + " " +
               ToSentence(
                   EnglishMorphologicalProcessor.GetPronoun(EnglishPronounForm.Normal, EnglishNumber.Singular,
                       leveledPerson, leveledGender),
                   EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "gain", leveledPerson),
                   $"{@event.SkillPointsGained} SP {@event.TraitPointsGained} TP {@event.MutationPointsGained} MP");
    }

    public string NoDefaultAttack(bool melee)
    {
        var weaponType = melee ? "melee" : "ranged";
        return $"Can't attack. You don't have a {weaponType} weapon equipped.";
    }

    public string NoPath() => "No path to target!";

    #endregion

    #region Interface messages

    public virtual string Welcome(GameEntity playerEntity)
    {
        var manager = playerEntity.Manager;
        return Format(
            "Welcome to the {0}, {1}!",
            manager.FindEntity(playerEntity.Position.LevelId).Level.Branch.Name,
            GetString(playerEntity, EnglishPerson.Third, SenseType.Sight));
    }

    public virtual string UnableToMove(Direction direction)
        => Format("Can't move {0}.", GetString(direction, abbreviate: false));

    #endregion

    #region Formatting

    protected string Format(string format, params object[] arguments) => string.Format(Culture, format, arguments);

    protected virtual string ToSentence(params string[] components)
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

        if (!char.IsPunctuation(builder[builder.Length - 1]))
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => base.ToString();
}

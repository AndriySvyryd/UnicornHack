using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageService : ILanguageService
    {
        protected virtual CultureInfo Culture { get; } = new CultureInfo(name: "en-US");

        private EnglishMorphologicalProcessor EnglishMorphologicalProcessor { get; } =
            new EnglishMorphologicalProcessor();

        #region Game concepts

        protected virtual string GetString(GameEntity beingEntity, EnglishPerson person, SenseType sense)
            => GetString(
                beingEntity,
                person,
                canIdentify: sense.CanIdentify(),
                definiteDeterminer: true);

        protected virtual string GetString(
            GameEntity beingEntity, EnglishPerson person, bool canIdentify, bool definiteDeterminer = false)
        {
            if (beingEntity == null)
            {
                return null;
            }

            if (person == EnglishPerson.Second)
            {
                return EnglishMorphologicalProcessor.GetPronoun(
                    EnglishPronounForm.Normal, EnglishNumber.Singular, person, gender: null);
            }

            if (!canIdentify)
            {
                return "something";
            }

            var properName = beingEntity.AI?.ProperName ?? beingEntity.Player?.ProperName;
            if (properName != null)
            {
                return properName;
            }

            var name = definiteDeterminer ? "the" : "a";
            foreach (var raceEntity in beingEntity.Manager.RacesToBeingRelationship[beingEntity.Id].Values)
            {
                name += " " + raceEntity.Race.TemplateName;
            }

            return name;
        }

        public virtual string GetString(ItemComponent item, int quantity, SenseType sense)
            => (sense & SenseType.Sight) == 0 ? "something" : GetString(item, quantity);

        public virtual string GetString(ItemComponent item, int quantity, bool definiteDeterminer = false)
        {
            var itemName = item.TemplateName + (item.Name == null ? "" : " named \"" + item.Name + "\"");
            if (quantity > 1)
            {
                return quantity + " " +
                       EnglishMorphologicalProcessor.ProcessNoun(itemName, EnglishNounForm.Plural);
            }

            return (EnglishMorphologicalProcessor.IsPlural(itemName)
                       ? ""
                       : definiteDeterminer
                           ? "the "
                           : EnglishMorphologicalProcessor.IsVocal(itemName[0])
                               ? "an "
                               : "a ") + itemName;
        }

        public virtual string GetPropertyString(string propertyName, bool abbreviate)
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
            => Capitalize(new StringBuilder(ability.Name)).ToString();

        public virtual string GetString(EquipmentSlot slot, GameEntity actorEntity, bool abbreviate)
        {
            // TODO: take body shape into account
            switch (slot)
            {
                case EquipmentSlot.GraspPrimaryExtremity:
                    return abbreviate ? "MH" : "main hand";
                case EquipmentSlot.GraspSecondaryExtremity:
                    return abbreviate ? "OH" : "off hand";
                case EquipmentSlot.GraspSingleExtremity:
                    return abbreviate ? "SH" : "single hand";
                case EquipmentSlot.GraspBothExtremities:
                    return abbreviate ? "BH" : "both hands";
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
                case EquipmentSlot.GraspPrimaryExtremity:
                case EquipmentSlot.GraspSecondaryExtremity:
                case EquipmentSlot.GraspMouth:
                    return "in the";
                case EquipmentSlot.GraspBothExtremities:
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

        protected virtual string ToVerb(AbilityAction abilityAction)
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
                    verb = "shoot with";
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

        protected virtual string GetString(Direction direction)
        {
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
            // TODO: don't log attacks, only hit/misses

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
                        @event.AbilityAction == AbilityAction.Scream ? "scream" : "noise");
                }
            }

            var attackerPerson = @event.SensorEntity == @event.AttackerEntity
                ? EnglishPerson.Second
                : EnglishPerson.Third;
            var attacker = GetString(@event.AttackerEntity, attackerPerson, @event.AttackerSensed);

            var victimGender = (EnglishGender?)@event.VictimEntity?.Being?.Sex;
            var victimPerson = @event.SensorEntity == @event.VictimEntity ? EnglishPerson.Second : EnglishPerson.Third;
            var victim = GetString(@event.VictimEntity, victimPerson, @event.VictimSensed);

            var attackVerb = ToVerb(@event.AbilityAction);
            var mainVerbForm = attackerPerson == EnglishPerson.Third
                ? EnglishVerbForm.ThirdPersonSingularPresent
                : EnglishVerbForm.BareInfinitive;

            var weapon = @event.WeaponEntity?.Item;
            if (weapon == null
                || !@event.Ranged)
            {
                // TODO: Don't add the weapon name
                var meleeWeaponPhrase = weapon == null || weapon.TemplateName == ""
                    ? ""
                    : "with " +
                      (@event.AttackerSensed.CanIdentify()
                          ? GetString(weapon, 1, definiteDeterminer: attackerPerson == EnglishPerson.Second)
                          : "something");

                if (victim == null)
                {
                    return ToSentence(attacker,
                        EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                        "the air",
                        meleeWeaponPhrase,
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
                        meleeWeaponPhrase,
                        @event.SensorEntity == @event.VictimEntity ? "!" : ".");

                    if (!@event.VictimSensed.CanIdentify())
                    {
                        return attackSentence;
                    }

                    return attackSentence + " " + FormatDamage(
                               @event.AppliedEffects, victimPerson == EnglishPerson.Second ? null : victim);
                }

                return ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "try", mainVerbForm),
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.Infinitive),
                    victim,
                    meleeWeaponPhrase,
                    ", but",
                    EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "miss", mainVerbForm));
            }

            var isProjectile = (weapon.Type & ItemType.WeaponProjectile) != 0;
            if (@event.AbilityAction == AbilityAction.Throw
                || @event.AbilityAction == AbilityAction.Shoot)
            {
                return ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                    @event.AttackerSensed.CanIdentify()
                        ? GetString(weapon, 1,
                            definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second)
                        : "something");
            }

            if (@event.Hit)
            {
                var attackSentence = ToSentence(
                    GetString(weapon, 1,
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
                           @event.AppliedEffects, victimPerson == EnglishPerson.Second ? null : victim);
            }

            return victim == null
                ? null
                : ToSentence(
                    GetString(weapon, 1,
                        definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second),
                    EnglishMorphologicalProcessor.ProcessVerb("miss", EnglishVerbForm.ThirdPersonSingularPresent),
                    victim);
        }

        protected virtual string FormatDamage(IReadOnlyList<GameEntity> effects, string victim)
        {
            // TODO: Differentiate damage types
            var damage = effects.Select(e => e.Effect).Where(e => e.EffectType.IsDamage())
                .Aggregate(0, (d, e) => d + e.Amount.Value);

            if (damage == 0)
            {
                return victim == null ? "You are unaffected." : ToSentence(victim, "seems unaffected.");
            }

            return Format(victim == null ? "[{0} pts.]" : "({0} pts.)", damage);
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
                GetString(@event.ItemEntity.Item, @event.Quantity));
        }

        public virtual string GetString(in ItemDropEvent @event)
        {
            var dropperPerson = @event.SensorEntity == @event.DropperEntity
                ? EnglishPerson.Second
                : EnglishPerson.Third;
            return ToSentence(
                GetString(@event.DropperEntity, dropperPerson, @event.DropperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "drop", dropperPerson),
                GetString(@event.ItemEntity.Item, @event.Quantity));
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
                GetString(@event.ItemEntity.Item, 1, @event.ItemSensed));
        }

        public virtual string GetString(in ItemEquipmentEvent @event)
        {
            var equipperPerson = @event.SensorEntity == @event.EquipperEntity
                ? EnglishPerson.Second
                : EnglishPerson.Third;
            // TODO: Use the definite determiner for items when the (un)equipper is the sensor
            if (@event.Slot == EquipmentSlot.None)
            {
                return ToSentence(
                    GetString(@event.EquipperEntity, equipperPerson, @event.EquipperSensed),
                    EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "unequip", equipperPerson),
                    GetString(@event.ItemEntity.Item, 1, @event.ItemSensed));
            }

            var slotKnown = (@event.EquipperSensed & SenseType.Sight) != 0
                            || (@event.EquipperSensed & SenseType.Touch) != 0;
            return ToSentence(
                GetString(@event.EquipperEntity, equipperPerson, @event.EquipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "equip", equipperPerson),
                GetString(@event.ItemEntity.Item, 1, @event.ItemSensed),
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

        public string NoPath() => "No path to target!";

        #endregion

        #region Interface messages

        public virtual string Welcome(GameEntity playerEntity)
        {
            var manager = playerEntity.Manager;
            return Format(
                "Welcome to the {0}, {1}!",
                manager.FindEntity(playerEntity.Position.LevelId).Level.Branch.Name,
                GetString(playerEntity, EnglishPerson.Third, true));
        }

        public virtual string UnableToMove(Direction direction) => Format("Can't move {0}.", GetString(direction));

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
            var first = builder[index: 0];
            builder.Remove(startIndex: 0, length: 1).Insert(index: 0, value: char.ToUpper(first));

            return builder;
        }

        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}

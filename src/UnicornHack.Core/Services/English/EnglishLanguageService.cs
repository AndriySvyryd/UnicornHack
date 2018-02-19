using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Events;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageService : ILanguageService
    {
        protected virtual CultureInfo Culture { get; } = new CultureInfo(name: "en-US");

        private EnglishMorphologicalProcessor EnglishMorphologicalProcessor { get; } =
            new EnglishMorphologicalProcessor();

        #region Game concepts

        private string ToString(Actor sensee, EnglishPerson person, SenseType sense)
            => ToString(
                sensee,
                person,
                canSense: (sense & (SenseType.Sight | SenseType.Telepathy | SenseType.Touch)) != 0,
                definiteDeterminer: true);

        private string ToString(Actor actor, EnglishPerson person, bool canSense, bool definiteDeterminer = false)
        {
            if (actor == null)
            {
                return null;
            }

            if (person == EnglishPerson.Second)
            {
                return EnglishMorphologicalProcessor.GetPronoun(EnglishPronounForm.Normal, EnglishNumber.Singular,
                    person, gender: null);
            }

            if (!canSense)
            {
                return "something";
            }

            if (actor is Creature creature)
            {
                var name = creature.VariantName + (creature.Name == null ? "" : " named \"" + creature.Name + "\"");

                var proper = char.IsUpper(name[index: 0]);
                return (proper ? "" : definiteDeterminer ? "the " : "a ") + name;
            }

            return (actor as Player)?.Name;
        }

        private string ToString(Item item, SenseType sense)
            => (sense & SenseType.Sight) == 0 ? "something" : ToString(item);

        public virtual string ToString(Item item)
            => ToString(item, definiteDeterminer: false);

        public virtual string ToString(Item item, bool definiteDeterminer)
        {
            var itemName = item.VariantName + (item.Name == null ? "" : " named \"" + item.Name + "\"");
            var quantity = (item as ItemStack)?.Quantity ?? (item as Gold)?.Quantity;
            if (quantity != null && quantity.Value > 1)
            {
                return quantity.Value + " " +
                       EnglishMorphologicalProcessor.ProcessNoun(itemName, EnglishNounForm.Plural);
            }

            return (EnglishMorphologicalProcessor.IsPlural(itemName)
                       ? ""
                       : definiteDeterminer
                           ? "the "
                           : IsVocal(itemName[0])
                               ? "an "
                               : "a ") + itemName;
        }

        public string ToString(Property property)
        {
            switch (property.Value)
            {
                case bool boolValue:
                    return boolValue ? property.Name : "";
                case int intValue:
                    return property.Name + ": " + intValue;
                default:
                    throw new NotImplementedException("Property value " + property.Value + " not supported.");
            }
        }

        public string ToString(Ability ability)
            => ability.Name;

        public string ToString(EquipmentSlot slot, Actor actor, bool abbreviate)
        {
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

        private string ToVerb(AbilityAction abilityAction)
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

        private static string ToString(Direction direction)
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

        private bool IsVocal(char c)
        {
            switch (char.ToLowerInvariant(c))
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Events

        public virtual string ToString(AttackEvent @event)
        {
            var meleeAttack = @event.AppliedEffects.OfType<MeleeAttacked>().SingleOrDefault();
            var meleeWeapon = meleeAttack?.Weapon;
            var rangedAttack = @event.AppliedEffects.OfType<RangeAttacked>().SingleOrDefault();
            var rangedWeapon = rangedAttack?.Weapon;

            if (@event.Sensor != @event.Victim && @event.Sensor != @event.Attacker
                && (@event.AttackerSensed & (SenseType.Sight | SenseType.Touch)) == 0
                && (rangedAttack == null || @event.VictimSensed == SenseType.None))
            {
                if ((@event.AttackerSensed & SenseType.Sound) == 0 &&
                    (@event.AttackerSensed & SenseType.SoundDistant) == 0)
                {
                    return null;
                }

                return ToSentence(
                    "You hear a",
                    (@event.AttackerSensed & SenseType.SoundDistant) != 0 ? "distant" : null,
                    @event.AbilityAction == AbilityAction.Scream ? "scream" : "noise");
            }

            var attackerPerson = @event.Sensor == @event.Attacker ? EnglishPerson.Second : EnglishPerson.Third;
            var attacker = ToString(@event.Attacker, attackerPerson, @event.AttackerSensed);

            var victimGender = (EnglishGender?)@event.Victim?.Sex;
            var victimPerson = @event.Sensor == @event.Victim ? EnglishPerson.Second : EnglishPerson.Third;
            var victim = ToString(@event.Victim, victimPerson, @event.VictimSensed);

            var attackVerb = ToVerb(@event.AbilityAction);
            var mainVerbForm = attackerPerson == EnglishPerson.Third
                ? EnglishVerbForm.ThirdPersonSingularPresent
                : EnglishVerbForm.BareInfinitive;

            if (rangedAttack == null)
            {
                var meleeWeaponPhrase = meleeWeapon == null || meleeWeapon.VariantName == ""
                    ? ""
                    : "with " +
                      ((@event.AttackerSensed & (SenseType.Sight | SenseType.Touch)) != 0
                          ? ToString(meleeWeapon, definiteDeterminer: attackerPerson == EnglishPerson.Second)
                          : "something");

                if (victim == null)
                {
                    return ToSentence(attacker,
                        EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                        "the air",
                        meleeWeaponPhrase,
                        @event.Sensor == @event.Victim ? "!" : ".");
                }

                if (@event.Hit)
                {
                    var attackSentence = ToSentence(attacker,
                        EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                        @event.Attacker != @event.Victim
                            ? victim
                            : EnglishMorphologicalProcessor.GetPronoun(EnglishPronounForm.Reflexive,
                                EnglishNumber.Singular,
                                attackerPerson, victimGender),
                        meleeWeaponPhrase,
                        @event.Sensor == @event.Victim ? "!" : ".");

                    if ((@event.VictimSensed & (SenseType.Sight | SenseType.Touch)) == 0)
                    {
                        return attackSentence;
                    }

                    return attackSentence + " " + FormatDamage(
                               @event.AppliedEffects, victimPerson == EnglishPerson.Second ? null : victim);
                }

                return ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "try", form: mainVerbForm),
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.Infinitive),
                    victim,
                    meleeWeaponPhrase,
                    ", but",
                    EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "miss", form: mainVerbForm));
            }

            if (@event.AbilityAction == AbilityAction.Throw
                && rangedWeapon is Launcher launcher)
            {
                rangedWeapon = launcher.Projectile;
            }

            var isProjectile = (rangedWeapon.Type & ItemType.WeaponProjectile) != 0;
            if (@event.AbilityAction == AbilityAction.Throw
                || @event.AbilityAction == AbilityAction.Shoot)
            {

                return ToSentence(attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                    (@event.AttackerSensed & (SenseType.Sight | SenseType.Touch)) != 0
                        ? ToString(rangedWeapon, definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second)
                        : "something");
            }

            if (@event.Hit)
            {
                var attackSentence = ToSentence(
                    ToString(rangedWeapon, definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second),
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.ThirdPersonSingularPresent),
                    @event.Attacker != @event.Victim
                        ? victim
                        : EnglishMorphologicalProcessor.GetPronoun(
                            EnglishPronounForm.Reflexive, EnglishNumber.Singular, attackerPerson, victimGender),
                    @event.Sensor == @event.Victim ? "!" : ".");

                if ((@event.VictimSensed & (SenseType.Sight | SenseType.Touch)) == 0)
                {
                    return attackSentence;
                }

                return attackSentence + " " + FormatDamage(
                           @event.AppliedEffects, victimPerson == EnglishPerson.Second ? null : victim);
            }

            return victim == null
                ? null
                : ToSentence(
                    ToString(rangedWeapon, definiteDeterminer: !isProjectile && attackerPerson == EnglishPerson.Second),
                    EnglishMorphologicalProcessor.ProcessVerb("miss", EnglishVerbForm.ThirdPersonSingularPresent),
                    victim);
        }

        private string FormatDamage(ISet<AppliedEffect> effects, string victim)
        {
            // TODO: Differentiate damage types
            var damage = effects.OfType<Damaged>().Aggregate(0, (d, e) => d + e.Damage);

            if (damage == 0)
            {
                return victim == null ? "You are unaffected." : ToSentence(victim, "seems unaffected.");
            }
            return Format(victim == null ? "[{0} pts.]" : "({0} pts.)", damage);
        }

        public virtual string ToString(ItemConsumptionEvent @event)
        {
            var consumerPerson = @event.Sensor == @event.Consumer ? EnglishPerson.Second : EnglishPerson.Third;
            var verb = (@event.Item.Type & ItemType.Potion) != 0 ? "drink" : "eat";
            return ToSentence(ToString(@event.Consumer, consumerPerson, @event.ConsumerSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verb, consumerPerson),
                ToString(@event.Item, @event.ItemSensed));
        }

        public virtual string ToString(ItemPickUpEvent @event)
        {
            var pickerPerson = @event.Sensor == @event.Picker ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(ToString(@event.Picker, pickerPerson, @event.PickerSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "pick up", person: pickerPerson),
                ToString(@event.Item));
        }

        public virtual string ToString(ItemDropEvent @event)
        {
            var dropperPerson = @event.Sensor == @event.Dropper ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(ToString(@event.Dropper, dropperPerson, @event.DropperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "drop", person: dropperPerson),
                ToString(@event.Item));
        }

        public string ToString(ItemEquipmentEvent @event)
        {
            var equipperPerson = @event.Sensor == @event.Equipper ? EnglishPerson.Second : EnglishPerson.Third;
            var slotKnown = (@event.EquipperSensed & SenseType.Sight) != 0 ||
                            (@event.EquipperSensed & SenseType.Touch) != 0;
            return ToSentence(ToString(@event.Equipper, equipperPerson, @event.EquipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "equip", person: equipperPerson),
                ToString(@event.Item, @event.ItemSensed),
                slotKnown ? PrepositionFor(@event.Item.EquippedSlot.Value) : null,
                slotKnown ? ToString(@event.Item.EquippedSlot.Value, @event.Equipper, abbreviate: false) : null);
        }

        public string ToString(ItemUnequipmentEvent @event)
        {
            var equipperPerson = @event.Sensor == @event.Unequipper ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(ToString(@event.Unequipper, equipperPerson, @event.UnequipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "unequip", person: equipperPerson),
                ToString(@event.Item, @event.ItemSensed));
        }

        public virtual string ToString(DeathEvent @event)
        {
            var deceasedPerson = @event.Sensor == @event.Deceased ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(ToString(@event.Deceased, deceasedPerson, @event.DeceasedSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "die", person: deceasedPerson),
                deceasedPerson == EnglishPerson.Second ? "!" : ".");
        }

        #endregion

        #region Interface messages

        public virtual string Welcome(Player character) => Format("Welcome to the {0}, {1}!",
            character.Level.Branch.Name, ToString(character, EnglishPerson.Third, canSense: true));

        public string InvalidTarget() => "The specified target is invalid.";

        public virtual string UnableToMove(Direction direction) => Format("Can't move {0}.", ToString(direction));

        #endregion

        #region Formatting

        private string Format(string format, params object[] arguments) => string.Format(Culture, format, arguments);

        private string ToSentence(params string[] components)
        {
            var builder = new StringBuilder();

            foreach (var component in components)
            {
                if (!string.IsNullOrEmpty(component))
                {
                    builder.Append(component).Append(value: " ");
                }
            }

            var first = builder[index: 0];
            builder.Remove(startIndex: 0, length: 1).Insert(index: 0, value: char.ToUpper(first))
                .Remove(builder.Length - 1, length: 1);

            if (!char.IsPunctuation(builder[builder.Length - 1]))
            {
                builder.Append(value: '.');
            }
            builder.Replace(oldValue: " ,", newValue: ",").Replace(oldValue: " .", newValue: ".")
                .Replace(oldValue: " .", newValue: ".").Replace(oldValue: " !", newValue: "!");

            return builder.ToString();
        }

        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
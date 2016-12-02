using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;

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
                sense.HasFlag(SenseType.Sight) || sense.HasFlag(SenseType.Telepathy),
                definiteDeterminer: true);

        private string ToString(Actor actor, EnglishPerson person, bool variantKnown, bool? definiteDeterminer = null)
        {
            if (person == EnglishPerson.Second)
            {
                return EnglishMorphologicalProcessor.GetPronoun(
                    EnglishPronounForm.Normal, EnglishNumber.Singular, person, gender: null);
            }

            if (!variantKnown)
            {
                return "something";
            }

            var monster = actor as Creature;
            if (monster != null)
            {
                var name = monster.Variant.Name +
                           (monster.GivenName == null ? "" : " named \"" + monster.GivenName + "\"");

                var proper = char.IsUpper(name[index: 0]);
                return (definiteDeterminer == null || proper
                    ? ""
                    : definiteDeterminer.Value
                        ? "the "
                        : "a ")
                       + name;
            }

            var character = actor as PlayerCharacter;
            return character.GivenName;
        }

        private string ToString(Item item, SenseType sense)
        {
            if (!sense.HasFlag(SenseType.Sight))
            {
                return "something";
            }

            return ToString(item);
        }

        private string ToString(Item item)
        {
            var itemName = item.Name;
            var quantity = (item as ItemStack)?.Quantity ?? (item as Gold)?.Quantity;
            if (quantity != null
                && quantity.Value > 1)
            {
                return quantity.Value + " " +
                       EnglishMorphologicalProcessor.ProcessNoun(itemName, EnglishNounForm.Plural);
            }

            return "a " + itemName;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(abilityAction), abilityAction, message: null);
            }

            return verb;
        }

        private static string ToString(Direction direction)
        {
            switch (direction)
            {
                case Direction.None:
                    return "nowhere";
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

        public virtual string ToString(AttackEvent @event)
        {
            if (@event.Sensor != @event.Victim
                && @event.Sensor != @event.Attacker
                && !@event.AttackerSensed.HasFlag(SenseType.Sight))
            {
                if (!@event.AttackerSensed.HasFlag(SenseType.Sound)
                    && !@event.AttackerSensed.HasFlag(SenseType.SoundDistant))
                {
                    return null;
                }

                var distanceModifier = @event.AttackerSensed.HasFlag(SenseType.SoundDistant) ? "distant" : null;

                if (@event.AbilityAction == AbilityAction.Scream)
                {
                    return ToSentence("You hear a", distanceModifier, "scream.");
                }
                return ToSentence("You hear some", distanceModifier, "noises.");
            }

            var attackerPerson = @event.Sensor == @event.Attacker ? EnglishPerson.Second : EnglishPerson.Third;
            var attacker = ToString(@event.Attacker, attackerPerson, @event.AttackerSensed);

            var victimGender = (EnglishGender)@event.Victim.Sex;
            var victimPerson = @event.Sensor == @event.Victim ? EnglishPerson.Second : EnglishPerson.Third;
            var victim = ToString(@event.Victim, victimPerson, @event.VictimSensed);

            var attackVerb = ToVerb(@event.AbilityAction);
            var mainVerbForm = attackerPerson == EnglishPerson.Third
                ? EnglishVerbForm.ThirdPersonSingularPresent
                : EnglishVerbForm.BareInfinitive;

            if (@event.Hit)
            {
                var attackSentence = ToSentence(
                    attacker,
                    EnglishMorphologicalProcessor.ProcessVerb(attackVerb, mainVerbForm),
                    @event.Attacker != @event.Victim
                        ? victim
                        : EnglishMorphologicalProcessor.GetPronoun(
                            EnglishPronounForm.Reflexive, EnglishNumber.Singular, attackerPerson, victimGender),
                    @event.Sensor == @event.Victim ? "!" : ".");

                if (!@event.VictimSensed.HasFlag(SenseType.Sight))
                {
                    return attackSentence;
                }

                var damageSentence = "";
                if (victimPerson == EnglishPerson.Second)
                {
                    if (@event.Damage == 0)
                    {
                        damageSentence = "You are unaffected.";
                    }
                    else
                    {
                        damageSentence = Format("[{0} pts.]", @event.Damage);
                    }
                }
                else
                {
                    if (@event.Damage == 0)
                    {
                        damageSentence = ToSentence(victim, "seems unaffected.");
                    }
                    else
                    {
                        damageSentence = Format("({0} pts.)", @event.Damage);
                    }
                }

                return attackSentence + " " + damageSentence;
            }
            return ToSentence(
                attacker,
                EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "try", form: mainVerbForm),
                EnglishMorphologicalProcessor.ProcessVerb(attackVerb, EnglishVerbForm.Infinitive),
                victim,
                ", but",
                EnglishMorphologicalProcessor.ProcessVerb(verbPhrase: "miss", form: mainVerbForm));
        }

        public virtual string ToString(ItemConsumptionEvent @event)
        {
            var consumerPerson = @event.Sensor == @event.Consumer ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Consumer, consumerPerson, @event.ConsumerSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "eat", person: consumerPerson),
                ToString(@event.Item, @event.ItemSensed));
        }

        public virtual string ToString(ItemPickUpEvent @event)
        {
            var pickerPerson = @event.Sensor == @event.Picker ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Picker, pickerPerson, @event.PickerSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "pick up", person: pickerPerson),
                ToString(@event.Item));
        }

        public virtual string ToString(ItemDropEvent @event)
        {
            var dropperPerson = @event.Sensor == @event.Dropper ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Dropper, dropperPerson, @event.DropperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "drop", person: dropperPerson),
                ToString(@event.Item));
        }

        public string ToString(ItemEquipmentEvent @event)
        {
            var equipperPerson = @event.Sensor == @event.Equipper ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Equipper, equipperPerson, @event.EquipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "equip", person: equipperPerson),
                ToString(@event.Item, @event.ItemSensed));
        }

        public string ToString(ItemUnequipmentEvent @event)
        {
            var equipperPerson = @event.Sensor == @event.Unequipper ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Unequipper, equipperPerson, @event.UnequipperSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "unequip", person: equipperPerson),
                ToString(@event.Item, @event.ItemSensed));
        }

        public virtual string ToString(DeathEvent @event)
        {
            var deceasedPerson = @event.Sensor == @event.Deceased ? EnglishPerson.Second : EnglishPerson.Third;
            return ToSentence(
                ToString(@event.Deceased, deceasedPerson, @event.DeceasedSensed),
                EnglishMorphologicalProcessor.ProcessVerbSimplePresent(verbPhrase: "die", person: deceasedPerson),
                deceasedPerson == EnglishPerson.Second ? "!" : ".");
        }

        #endregion

        #region Interface messages

        public virtual string Welcome(PlayerCharacter character)
        {
            return Format("Welcome to the {0}, {1}!", character.Level.Name,
                ToString(character, EnglishPerson.Third, variantKnown: true));
        }

        public virtual string UnableToMove(Direction direction)
        {
            return Format("Can't move {0}.", ToString(direction));
        }

        #endregion

        #region Formatting

        private string Format(string format, params object[] arguments)
        {
            return string.Format(Culture, format, arguments);
        }

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
            builder.Remove(startIndex: 0, length: 1)
                .Insert(index: 0, value: char.ToUpper(first))
                .Remove(builder.Length - 1, length: 1);

            if (!char.IsPunctuation(builder[builder.Length - 1]))
            {
                builder.Append(value: '.');
            }
            builder.Replace(oldValue: " ,", newValue: ",")
                .Replace(oldValue: " .", newValue: ".")
                .Replace(oldValue: " .", newValue: ".")
                .Replace(oldValue: " !", newValue: "!");

            return builder.ToString();
        }

        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
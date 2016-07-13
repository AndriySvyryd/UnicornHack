using System;
using System.ComponentModel;
using System.Globalization;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;

namespace UnicornHack.Services
{
    public class LanguageService
    {
        // TODO: Use SharpSimpleNLG

        protected virtual CultureInfo Culture { get; } = new CultureInfo(name: "en-US");

        public virtual string Format(string format, params object[] arguments)
        {
            return string.Format(Culture, format, arguments);
        }

        #region Natural language helpers

        static string ToUppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[index: 0]) + s.Substring(startIndex: 1);
        }

        private enum Tense
        {
            Infinitive,
            Present
        }

        private string ToReflexivePronoun(Sex sex, bool thirdPerson)
        {
            if (!thirdPerson)
            {
                return "yourself";
            }

            switch (sex)
            {
                case Sex.Male:
                    return "himself";
                case Sex.Female:
                    return "herself";
                default:
                    return "itself";
            }
        }

        private bool IsVowel(char character)
        {
            switch (character)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'A':
                case 'E':
                case 'I':
                case 'O':
                case 'U':
                    return true;
                default:
                    return false;
            }
        }

        private bool IsConsonant(char character)
        {
            return !IsVowel(character);
        }

        private string ToSForm(string word, bool verb)
        {
            var lastCharacter = word[word.Length - 1];
            var beforeLastCharacter = word[word.Length - 2];

            return word + "s";
        }

        #endregion

        #region Game concepts

        private string ToString(Actor actor)
        {
            return actor.Variant.Name + (actor.GivenName == null ? "" : " named \"" + actor.GivenName + "\"");
        }

        private string ToString(Item item)
        {
            var dropedItemString = item.Name;
            var stackableItem = item as StackableItem;
            if (stackableItem != null)
            {
                dropedItemString = stackableItem.Quantity + " " +
                                   (stackableItem.Quantity > 1
                                       ? ToSForm(dropedItemString, verb: false)
                                       : dropedItemString);
            }

            return dropedItemString;
        }

        private string ToVerb(AttackType attackType, Tense tense, bool singularThirdPerson)
        {
            string verb;
            switch (attackType)
            {
                case AttackType.Weapon:
                    // TODO: depends on the weapon type
                    verb = "hit";
                    break;
                case AttackType.Punch:
                    verb = "punch";
                    break;
                case AttackType.Kick:
                    verb = "kick";
                    break;
                case AttackType.Touch:
                    verb = "touch";
                    break;
                case AttackType.Headbutt:
                    verb = "headbutt";
                    break;
                case AttackType.Claw:
                    verb = "claw";
                    break;
                case AttackType.Bite:
                    verb = "bite";
                    break;
                case AttackType.Suck:
                    verb = "suck";
                    break;
                case AttackType.Sting:
                    verb = "sting";
                    break;
                case AttackType.Hug:
                    verb = "hug";
                    break;
                case AttackType.Trample:
                    verb = "trample";
                    break;
                case AttackType.Spit:
                    verb = "spit";
                    break;
                case AttackType.Digestion:
                    verb = "digest";
                    break;
                case AttackType.Spell:
                    verb = "cast";
                    break;
                case AttackType.Breath:
                    verb = "breath";
                    break;
                case AttackType.Gaze:
                    verb = "gaze";
                    break;
                case AttackType.Scream:
                    verb = "scream";
                    break;
                case AttackType.Explosion:
                    verb = "explode";
                    break;
                case AttackType.OnMeleeHit:
                case AttackType.OnRangedHit:
                case AttackType.OnDeath:
                case AttackType.OnConsumption:
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackType), attackType, message: null);
            }

            switch (tense)
            {
                case Tense.Infinitive:
                    return "to " + verb;
                case Tense.Present:
                    return singularThirdPerson ? ToSForm(verb, verb: true) : verb;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tense), tense, message: null);
            }
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
            var attacker = @event.Sensor == @event.Attacker
                ? "you"
                : @event.AttackerSensed.HasFlag(SenseType.Sight) ? ToString(@event.Attacker) : "something";

            var victim = @event.Attacker == @event.Victim
                ? ToReflexivePronoun(@event.Victim.Sex, @event.Sensor != @event.Victim)
                : @event.Sensor == @event.Victim
                    ? "you"
                    : @event.VictimSensed.HasFlag(SenseType.Sight) ? ToString(@event.Victim) : "something";

            if (@event.Hit)
            {
                return Format("{0} {1} {2} for {3} damage.", ToUppercaseFirst(attacker),
                    ToVerb(@event.AttackType, Tense.Present, @event.Sensor != @event.Attacker), victim, @event.Damage);
            }
            return Format("{0} tries {1} {2}, but misses.", ToUppercaseFirst(attacker),
                ToVerb(@event.AttackType, Tense.Infinitive, @event.Sensor != @event.Attacker), victim);
        }

        public virtual string ToString(ItemDropEvent @event)
        {
            var dropper = @event.Sensor == @event.Dropper
                ? "you"
                : @event.DropperSensed.HasFlag(SenseType.Sight)
                    ? ToString(@event.Dropper)
                    : "something";

            return Format("{0} drops {1}.", ToUppercaseFirst(dropper), ToString(@event.Item));
        }

        public virtual string ToString(ItemPickUpEvent @event)
        {
            var dropper = @event.Sensor == @event.Picker
                ? "you"
                : @event.PickerSensed.HasFlag(SenseType.Sight)
                    ? ToString(@event.Picker)
                    : "something";

            return Format("{0} picks up {1}.", ToUppercaseFirst(dropper), ToString(@event.Item));
        }

        public virtual string ToString(DeathEvent @event)
        {
            string message;

            if (@event.Sensor == @event.Deceased)
            {
                message = "You die";
            }
            else
            {
                var deceased = @event.DeceasedSensed.HasFlag(SenseType.Sight) ? ToString(@event.Deceased) : "something";
                message = Format("{0} dies", ToUppercaseFirst(deceased));
            }

            if (@event.Corpse != null)
            {
                message += " leaving behind a corpse";
            }

            return message + ".";
        }

        public virtual string ToString(ItemConsumptionEvent @event)
        {
            var @object = @event.ObjectSensed.HasFlag(SenseType.Sight) ? @event.Object.Name : "something";
            if (@event.Sensor == @event.Consumer)
            {
                return Format("You eat a {0}.", @object);
            }
            var consumer = @event.ConsumerSensed.HasFlag(SenseType.Sight) ? ToString(@event.Consumer) : "something";
            return Format("{0} eats a {1}.", consumer, @object);
        }

        #endregion

        #region Interface messages

        public virtual string Welcome(PlayerCharacter character)
        {
            return Format("Welcome to {0}, {1}!", character.Level.Name, ToString(character));
        }

        public virtual string UnableToMove(Direction direction)
        {
            return Format("Can't move {0}.", ToString(direction));
        }

        #endregion

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameState.Events;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class PlayerCharacter : Actor
    {
        protected PlayerCharacter()
        {
        }

        public PlayerCharacter(ActorVariant variant)
            : base(variant)
        {
        }

        public PlayerCharacter(ActorVariant variant, byte x, byte y, Level level)
            : base(variant, x, y, level)
        {
        }

        public virtual byte Strength { get; set; }
        public virtual byte Dexterity { get; set; }
        public virtual byte Constitution { get; set; }
        public virtual byte Intelligence { get; set; }
        public virtual byte Wisdom { get; set; }
        public virtual byte Charisma { get; set; }

        public virtual int NextEventId { get; set; }
        public virtual ICollection<SensoryEvent> SensedEvents { get; set; } = new HashSet<SensoryEvent>();

        public virtual int NextLogEntryId { get; set; }
        public virtual ICollection<LogEntry> Log { get; set; } = new HashSet<LogEntry>();

        public virtual string NextAction { get; set; }
        public virtual int NextActionTarget { get; set; }

        protected virtual void WriteLog(string format, params object[] arguments)
        {
            Log.Add(new LogEntry(this, string.Format(format, arguments)));
        }

        public override bool Act()
        {
            Debug.Assert(Level.LastTurn + 1 == Game.CurrentTurn);

            // TODO: add option to stop here and display current state
            // even if user already provided the next action / cannot perform an action

            // TODO: Move event processing after user action processing
            foreach (var @event in SensedEvents.ToList())
            {
                var logEntry = GetLogEntry(@event);
                if (logEntry != null)
                {
                    WriteLog(logEntry);
                }
                SensedEvents.Remove(@event);
            }

            var action = NextAction;
            var target = NextActionTarget;
            if (action == null)
            {
                return false;
            }

            MovementPoints = MovementPoints > Level.MovementCost ? Level.MovementCost : MovementPoints;
            MovementPoints += MovementRate;

            NextAction = null;
            NextActionTarget = 0;

            Direction? moveDirection = null;
            switch (action)
            {
                case "N":
                    moveDirection = Direction.North;
                    break;
                case "S":
                    moveDirection = Direction.South;
                    break;
                case "W":
                    moveDirection = Direction.West;
                    break;
                case "E":
                    moveDirection = Direction.East;
                    break;
                case "NW":
                    moveDirection = Direction.Northwest;
                    break;
                case "NE":
                    moveDirection = Direction.Northeast;
                    break;
                case "SW":
                    moveDirection = Direction.Southwest;
                    break;
                case "SE":
                    moveDirection = Direction.Southeast;
                    break;
                case "U":
                    moveDirection = Direction.Up;
                    break;
                case "D":
                    moveDirection = Direction.Down;
                    break;
                case "H":
                    break;
                case "EAT":
                    var itemToEat = Inventory.Single(i => i.Id == target);
                    Eat(itemToEat);
                    break;
                case "DROP":
                    var itemToDrop = Inventory.Single(i => i.Id == target);
                    Drop(itemToDrop);
                    break;
                default:
                    throw new InvalidOperationException($"Action {action} on character {GivenName} is invalid.");
            }

            if ((moveDirection != null)
                && !Move(Vector.Convert(moveDirection.Value)))
            {
                WriteLog(Game.Services.Language.UnableToMove(moveDirection.Value));
            }

            return true;
        }

        public override void Sense(SensoryEvent @event)
        {
            @event.Sensor = this;
            @event.Id = NextEventId++;
            SensedEvents.Add(@event);
        }

        public virtual string GetLogEntry(SensoryEvent @event)
        {
            return GetSpecificLogEntry((dynamic)@event);
        }

        protected virtual string GetSpecificLogEntry(SensoryEvent @event)
        {
            return null;
        }

        protected virtual string GetSpecificLogEntry(AttackEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(DeathEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemConsumptionEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemDropEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        protected virtual string GetSpecificLogEntry(ItemPickUpEvent @event)
        {
            return Game.Services.Language.ToString(@event);
        }

        public static PlayerCharacter CreateCharacter(Game game, string name)
        {
            var initialLevel = Level.CreateLevel(game, Level.MainBranchName, depth: 1);
            var upStairs = initialLevel.UpStairs.First();
            var character = new PlayerCharacter(
                ActorVariant.Human, upStairs.DownLevelX, upStairs.DownLevelY, initialLevel)
            {
                GivenName = name,
                HP = 100,
                MaxHP = 100,
                XPLevel = 1,
                Game = game,
                NextLevelXP = 100,
                AC = 8,
                Charisma = 16,
                Constitution = 14,
                Dexterity = 15,
                Intelligence = 16,
                Strength = 12,
                Wisdom = 17
            };

            character.MaxHP = 10;
            character.HP = character.MaxHP;

            character.Inventory.Add(new StackableItem(ItemType.Food, quantity: 3, actor: character));
            game.Actors.Add(character);

            character.WriteLog(game.Services.Language.Welcome(character));
            game.CurrentTurn++;
            return character;
        }
    }
}
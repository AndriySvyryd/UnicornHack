using UnicornHack.Models.GameDefinitions;
using UnicornHack.Models.GameState;
using UnicornHack.Models.GameState.Events;
using Xunit;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageServiceTest
    {
        [Fact]
        public void AttackEventTest()
        {
            var newt = new Creature {OriginalVariant = "newt"};
            var nymph = new Creature {OriginalVariant = "water nymph", Sex = Sex.Female};
            var rodney = new Creature {OriginalVariant = "Wizard of Yendor", Sex = Sex.None};
            var player = new PlayerCharacter {OriginalVariant = "human", GivenName = "Dudley", Sex = Sex.Male};
            var player2 = new PlayerCharacter {OriginalVariant = "human", GivenName = "Cudley", Sex = Sex.Male};

            Verify(newt, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Bite, 11,
                expectedMessage: "The newt bites the water nymph. (11 pts.)");

            Verify(player, rodney, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "Dudley stings Wizard of Yendor. (11 pts.)");

            Verify(rodney, player, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Spell, null,
                expectedMessage: "Wizard of Yendor tries to cast a spell at Dudley, but misses.");

            Verify(newt, nymph, player, SenseType.Sound, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "You hear some noises.");

            Verify(newt, nymph, player, SenseType.Sight, SenseType.SoundDistant | SenseType.Danger | SenseType.Touch,
                AbilityAction.Headbutt, 11,
                expectedMessage: "The newt headbutts something.");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Punch, 11,
                expectedMessage: "The water nymph punches you! [11 pts.]");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Spit, null,
                expectedMessage: "The water nymph tries to spit at you, but misses.");

            Verify(player, newt, player, SenseType.Sight | SenseType.Touch, SenseType.Sight, AbilityAction.Hug, 11,
                expectedMessage: "You squeeze the newt. (11 pts.)");

            Verify(player, newt, player, SenseType.Sight | SenseType.Touch, SenseType.Telepathy | SenseType.Touch,
                AbilityAction.Trample, null,
                expectedMessage: "You try to trample the newt, but miss.");

            Verify(newt, newt, player, SenseType.Sight, SenseType.Sight, AbilityAction.Claw, 11,
                expectedMessage: "The newt claws itself. (11 pts.)");

            Verify(player, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Kick, 12,
                expectedMessage: "You kick yourself! [12 pts.]");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Touch,
                AbilityAction.Scream, 0,
                expectedMessage: "The water nymph screams at you! You are unaffected.");

            Verify(nymph, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Scream, 0,
                expectedMessage: "The water nymph screams at herself. The water nymph seems unaffected.");

            Verify(nymph, nymph, player, SenseType.Sound, SenseType.None, AbilityAction.Scream, null,
                expectedMessage: "You hear a scream.");

            //Verify(nymph, player, player, SenseType.Sight, SenseType.Sight, AbilityAction.Weapon, 11, weapon: Bow, projectile: Arrow,
            //    expectedMessage: "The water nymph shoots an arrow. You are hit by an arrow! [11 pts.]");

            //Verify(player, newt, player, SenseType.Sight, SenseType.Sight, AbilityAction.Weapon, 11, weapon: Bow, projectile: Arrow,
            //    expectedMessage: "The arrow hits the newt. (11 pts.)");
        }

        private void Verify(
            Actor attacker,
            Actor victim,
            PlayerCharacter sensor,
            SenseType attackerSensed,
            SenseType victimSensed,
            AbilityAction abilityAction,
            int? damage,
            Item weapon = null,
            Item projectile = null,
            string expectedMessage = "")
        {
            var languageService = CreateLanguageService();
            var attackEvent = new AttackEvent
            {
                Attacker = attacker,
                Victim = victim,
                Sensor = sensor,
                AttackerSensed = attackerSensed,
                VictimSensed = victimSensed,
                AbilityAction = abilityAction,
                Hit = damage.HasValue,
                Weapon = weapon,
                Projectile = projectile
            };

            if (damage.HasValue)
            {
                attackEvent.Damage = damage.Value;
            }

            Assert.Equal(expectedMessage, languageService.ToString(attackEvent));
        }

        [Fact]
        public void Welcome()
        {
            var languageService = CreateLanguageService();

            var message = languageService.Welcome(
                new PlayerCharacter
                {
                    OriginalVariant = "human",
                    GivenName = "Conan the Barbarian",
                    Level = new Level {Name = "Dungeon of Fun"}
                });

            Assert.Equal("Welcome to the Dungeon of Fun, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}
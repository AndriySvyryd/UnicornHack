using System.Collections.Generic;
using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Services.English;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Knowledge
{
    public class LoggingSystemTest
    {
        // TODO: Test through LoggingSystem instead of calling the language system directly

        // TODO: Test explosion

        [Fact]
        public void AttackEvent()
        {
            var level = TestHelper.BuildLevel();
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var player2 = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level.Entity, new Point(0, 3));
            var manager = level.Entity.Manager;

            Verify(blob, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Bite, manager, 11,
                expectedMessage: "The acid blob bites the water nymph. (11 pts.)");

            Verify(player, blob, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Sting, manager, 11,
                expectedMessage: "Dudley stings the acid blob. (11 pts.)");

            Verify(blob, player, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Spell, manager, null,
                expectedMessage: "The acid blob tries to cast a spell at Dudley, but misses.");

            Verify(blob, nymph, player, SenseType.Sound, SenseType.Sight, AbilityAction.Sting, manager, 11,
                expectedMessage: "You hear a noise.");

            Verify(blob, nymph, player, SenseType.Sight, SenseType.SoundDistant | SenseType.Danger | SenseType.Telepathy,
                AbilityAction.Headbutt, manager, 11,
                expectedMessage: "The acid blob headbutts the water nymph. (11 pts.)");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Punch, manager, 11,
                expectedMessage: "The water nymph punches you! [11 pts.]");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Spit, manager, null,
                expectedMessage: "The water nymph tries to spit at you, but misses.");

            Verify(player, blob, player, SenseType.Sight | SenseType.Touch, SenseType.Sight, AbilityAction.Hug, manager, 11,
                expectedMessage: "You squeeze the acid blob. (11 pts.)");

            Verify(player, blob, player, SenseType.Sight | SenseType.Touch, SenseType.Telepathy | SenseType.Touch,
                AbilityAction.Trample, manager, null,
                expectedMessage: "You try to trample the acid blob, but miss.");

            Verify(blob, blob, player, SenseType.Sight, SenseType.Sight, AbilityAction.Claw, manager, 11,
                expectedMessage: "The acid blob claws itself. (11 pts.)");

            Verify(player, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Kick, manager, 12,
                expectedMessage: "You kick yourself! [12 pts.]");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Touch,
                AbilityAction.Scream, manager, 0,
                expectedMessage: "The water nymph screams at you! You are unaffected.");

            Verify(nymph, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Scream, manager, 0,
                expectedMessage: "The water nymph screams at herself. The water nymph seems unaffected.");

            Verify(nymph, nymph, player, SenseType.Sound, SenseType.None, AbilityAction.Scream, manager, null,
                expectedMessage: "You hear a scream.");

            var dagger = ItemData.Dagger.Instantiate(player.Manager).Referenced;

            Verify(player, blob, player, SenseType.Touch | SenseType.Telepathy, SenseType.Sight, AbilityAction.Slash, manager, 11,
                weapon: dagger,
                expectedMessage: "You slash the acid blob. (11 pts.)");

            Verify(player, null, player, SenseType.Touch | SenseType.Telepathy, SenseType.Sight, AbilityAction.Slash, manager, null,
                weapon: dagger,
                expectedMessage: "You slash the air.");

            var bow = ItemData.Shortbow.Instantiate(player.Manager).Referenced;
            var arrow = ItemData.Arrow.Instantiate(player.Manager).Referenced;

            Verify(nymph, player, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, manager, null, weapon: bow,
                expectedMessage: null);

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, 11, weapon: arrow,
                expectedMessage: "An arrow hits you! [11 pts.]");

            Verify(nymph, player, player, SenseType.SoundDistant, SenseType.None, AbilityAction.Shoot, manager, null,
                weapon: bow,
                expectedMessage: null);

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, null, weapon: arrow,
                expectedMessage: "An arrow misses you.");

            Verify(player, blob, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, manager, null, weapon: bow,
                expectedMessage: null);

            Verify(player, blob, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, 11, weapon: arrow,
                expectedMessage: "An arrow hits the acid blob. (11 pts.)");

            Verify(nymph, blob, player, SenseType.Sound, SenseType.None, AbilityAction.Shoot, manager, null, weapon: bow,
                expectedMessage: "You hear a noise.");

            Verify(nymph, blob, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, 2, weapon: arrow,
                expectedMessage: "An arrow hits the acid blob. (2 pts.)");

            var throwingKnife = ItemData.ThrowingKnife.Instantiate(player.Manager).Referenced;

            Verify(nymph, player, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, manager, null,
                weapon: throwingKnife,
                expectedMessage: null);

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, 11, weapon: throwingKnife,
                expectedMessage: "A throwing knife hits you! [11 pts.]");

            Verify(player, null, player, SenseType.Sight, SenseType.None, AbilityAction.Throw, manager, null,
                weapon: throwingKnife,
                expectedMessage: null);

            Verify(player, blob, player, SenseType.None, SenseType.Sound, AbilityAction.Hit, manager, null,
                weapon: throwingKnife,
                expectedMessage: "A throwing knife misses something.");

            Verify(player, null, player, SenseType.None, SenseType.None, AbilityAction.Hit, manager, null, weapon: throwingKnife,
                expectedMessage: null);

            Verify(nymph, blob, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, manager, null,
                weapon: throwingKnife,
                expectedMessage: "You hear a noise.");

            Verify(nymph, blob, player, SenseType.None, SenseType.SoundDistant, AbilityAction.Hit, manager, 2,
                weapon: throwingKnife,
                expectedMessage: "You hear a distant noise.");
        }

        private void Verify(
            GameEntity attacker,
            GameEntity victim,
            GameEntity sensor,
            SenseType attackerSensed,
            SenseType victimSensed,
            AbilityAction abilityAction,
            GameManager manager,
            int? damage,
            GameEntity weapon = null,
            string expectedMessage = "")
        {
            var languageService = manager.Game.Services.Language;

            var appliedEffects = new List<GameEntity>();
            if (damage.HasValue)
            {
                using (var damageEffectEntity = manager.CreateEntity())
                {
                    var entity = damageEffectEntity.Referenced;

                    var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    appliedEffect.Amount = damage.Value;
                    appliedEffect.EffectType = EffectType.PhysicalDamage;
                    appliedEffect.AffectedEntityId = victim.Id;

                    entity.Effect = appliedEffect;

                    appliedEffects.Add(entity);
                }

                if (weapon != null)
                {
                    using (var weaponEffectEntity = manager.CreateEntity())
                    {
                        var entity = weaponEffectEntity.Referenced;

                        var appliedEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                        appliedEffect.Amount = damage.Value;
                        appliedEffect.EffectType = EffectType.Activate;
                        appliedEffect.TargetEntityId = weapon.Id;
                        appliedEffect.AffectedEntityId = victim.Id;

                        entity.Effect = appliedEffect;

                        appliedEffects.Add(entity);
                    }
                }
            }

            var attackEvent = new AttackEvent(sensor, attacker, victim, attackerSensed, victimSensed,
                appliedEffects, abilityAction, weapon,
                ranged: weapon != null && (weapon.Item.Type & ItemType.WeaponRanged) != 0, hit: damage.HasValue);

            Assert.Equal(expectedMessage, languageService.GetString(attackEvent));
        }

        [Fact]
        public void DeathEvent()
        {
            var level = TestHelper.BuildLevel();
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The acid blob dies.",
                languageService.GetString(new DeathEvent(player, blob, SenseType.Sight)));

            Assert.Equal("You die!",
                languageService.GetString(new DeathEvent(player, player, SenseType.Sight | SenseType.Touch)));
        }

        [Fact]
        public void ItemEquipmentEvent()
        {
            var level = TestHelper.BuildLevel();
            var armor = ItemData.MailArmor.Instantiate(level.Entity.Manager).Referenced;
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The water nymph equips something on the torso.",
                languageService.GetString(new ItemEquipmentEvent(
                    player, nymph, armor, SenseType.Sight, SenseType.Sound, EquipmentSlot.Torso)));

            Assert.Equal("Something equips a mail armor.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sound, SenseType.Sight, EquipmentSlot.Torso)));

            Assert.Equal("You equip something on the torso.", languageService.GetString(new ItemEquipmentEvent(
                player, player, armor, SenseType.Sight | SenseType.Touch, SenseType.Touch, EquipmentSlot.Torso)));

            var sword = ItemData.LongSword.Instantiate(level.Entity.Manager).Referenced;

            Assert.Equal("You equip a long sword in the main hand for melee.", languageService.GetString(new ItemEquipmentEvent(
                player, player, sword, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch, EquipmentSlot.GraspPrimaryMelee)));
        }

        [Fact]
        public void ItemUnequipmentEvent()
        {
            var level = TestHelper.BuildLevel();
            var armor = ItemData.MailArmor.Instantiate(level.Entity.Manager).Referenced;
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The water nymph unequips something.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sight, SenseType.Sound, EquipmentSlot.None)));

            Assert.Equal("Something unequips a mail armor.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sound, SenseType.Sight, EquipmentSlot.None)));

            Assert.Equal("You unequip something.", languageService.GetString(new ItemEquipmentEvent(
                player, player, armor, SenseType.Sight | SenseType.Touch, SenseType.Touch, EquipmentSlot.None)));

            var sword = ItemData.LongSword.Instantiate(level.Entity.Manager).Referenced;

            Assert.Equal("You unequip a long sword.", languageService.GetString(new ItemEquipmentEvent(
                player, player, sword, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch, EquipmentSlot.None)));
        }

        [Fact]
        public void ItemConsumptionEvent()
        {
            var level = TestHelper.BuildLevel();
            var flask = ItemData.FlaskOfHealing.Instantiate(level.Entity.Manager).Referenced;
            var potion = ItemData.PotionOfExperience.Instantiate(level.Entity.Manager).Referenced;
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The acid blob drinks from a flask of healing.", languageService.GetString(
                new ItemActivationEvent(player, flask, blob, blob,
                    SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound,
                    SenseType.Sight | SenseType.Sound, consumed: false, successful: true)));

            Assert.Equal("You drink from a flask of healing.", languageService.GetString(
                new ItemActivationEvent(player, flask, player, player,
                    SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Touch,
                    SenseType.Sight | SenseType.Touch, consumed: false, successful: true)));

            Assert.Equal("You drink a potion of experience.", languageService.GetString(
                new ItemActivationEvent(player, potion, player, player,
                    SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Touch,
                    SenseType.Sight | SenseType.Touch, consumed: true, successful: true)));
        }

        [Fact]
        public void ItemPickUpEvent()
        {
            var level = TestHelper.BuildLevel();
            var coins = ItemData.GoldCoin.Instantiate(level.Entity.Manager).Referenced;
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The acid blob picks up 11 gold coins.", languageService.GetString(new ItemPickUpEvent(
                player, blob, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));

            Assert.Equal("You pick up 11 gold coins.", languageService.GetString(new ItemPickUpEvent(
                player, player, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));
        }

        [Fact]
        public void ItemDropEvent()
        {
            var level = TestHelper.BuildLevel();
            var coins = ItemData.GoldCoin.Instantiate(level.Entity.Manager).Referenced;
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The acid blob drops 11 gold coins.", languageService.GetString(new ItemDropEvent(
                player, blob, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));

            Assert.Equal("You drop 11 gold coins.", languageService.GetString(new ItemDropEvent(
                player, player, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));
        }

        [Fact]
        public void LeveledUpEvent()
        {
            var level = TestHelper.BuildLevel();
            var player1 = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 0));
            var player2 = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level.Entity, new Point(0, 1));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("You level up! You gain 2 SP 1 TP 0 MP.",
                languageService.GetString(new LeveledUpEvent(
                    player1, player1, manager.RacesToBeingRelationship[player1.Id].Single().Value.Race, 2, 1, 0)));

            Assert.Equal("Cudley levels up! She gains 3 SP 2 TP 1 MP.",
                languageService.GetString(new LeveledUpEvent(
                    player1, player2, manager.RacesToBeingRelationship[player2.Id].Single().Value.Race, 3, 2, 1)));
        }

        [Fact]
        public void Welcome()
        {
            var level = TestHelper.BuildLevel();
            var player = PlayerRace.InstantiatePlayer("Conan the Barbarian", Sex.Male, level.Entity, new Point(0, 0));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            var message = languageService.Welcome(player);

            Assert.Equal("Welcome to the test branch, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}

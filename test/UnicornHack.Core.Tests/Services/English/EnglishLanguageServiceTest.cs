using System.Collections.Generic;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Effects;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageServiceTest
    {
        [Fact]
        public void AttackEvent()
        {
            var level = TestHelper.BuildLevel();
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));
            var player2 = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level.Entity, new Point(0, 3));

            Verify(blob, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Bite, 11,
                expectedMessage: "The acid blob bites the water nymph. (11 pts.)");

            Verify(player, blob, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "Dudley stings the acid blob. (11 pts.)");

            Verify(blob, player, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Spell, null,
                expectedMessage: "The acid blob tries to cast a spell at Dudley, but misses.");

            Verify(blob, nymph, player, SenseType.Sound, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "You hear a noise.");

            Verify(blob, nymph, player, SenseType.Sight, SenseType.SoundDistant | SenseType.Danger | SenseType.Telepathy,
                AbilityAction.Headbutt, 11,
                expectedMessage: "The acid blob headbutts the water nymph. (11 pts.)");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Punch, 11,
                expectedMessage: "The water nymph punches you! [11 pts.]");

            Verify(nymph, player, player, SenseType.Sight | SenseType.Touch, SenseType.Sight | SenseType.Touch,
                AbilityAction.Spit, null,
                expectedMessage: "The water nymph tries to spit at you, but misses.");

            Verify(player, blob, player, SenseType.Sight | SenseType.Touch, SenseType.Sight, AbilityAction.Hug, 11,
                expectedMessage: "You squeeze the acid blob. (11 pts.)");

            Verify(player, blob, player, SenseType.Sight | SenseType.Touch, SenseType.Telepathy | SenseType.Touch,
                AbilityAction.Trample, null,
                expectedMessage: "You try to trample the acid blob, but miss.");

            Verify(blob, blob, player, SenseType.Sight, SenseType.Sight, AbilityAction.Claw, 11,
                expectedMessage: "The acid blob claws itself. (11 pts.)");

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

            var dagger = ItemData.Dagger.Instantiate(player.Manager).Referenced;

            Verify(player, blob, player, SenseType.Touch | SenseType.Telepathy, SenseType.Sight, AbilityAction.Slash, 11, weapon: dagger,
                expectedMessage: "You slash the acid blob with the dagger. (11 pts.)");

            Verify(player, null, player, SenseType.Touch | SenseType.Telepathy, SenseType.Sight, AbilityAction.Slash, null, weapon: dagger,
                expectedMessage: "You slash the air with the dagger.");

            var bow = ItemData.Shortbow.Instantiate(player.Manager).Referenced;
            var arrow = ItemData.Arrow.Instantiate(player.Manager).Referenced;

            Verify(nymph, player, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "The water nymph shoots with a shortbow.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: arrow,
                expectedMessage: "An arrow hits you! [11 pts.]");

            Verify(nymph, player, player, SenseType.SoundDistant, SenseType.None, AbilityAction.Shoot, null,
                weapon: bow,
                expectedMessage: "Something shoots with something.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, null, weapon: arrow,
                expectedMessage: "An arrow misses you.");

            Verify(player, blob, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "You shoot with the shortbow.");

            Verify(player, blob, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: arrow,
                expectedMessage: "An arrow hits the acid blob. (11 pts.)");

            Verify(nymph, blob, player, SenseType.Sound, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "You hear a noise.");

            Verify(nymph, blob, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 2, weapon: arrow,
                expectedMessage: "An arrow hits the acid blob. (2 pts.)");

            var throwingKnife = ItemData.ThrowingKnife.Instantiate(player.Manager).Referenced;

            Verify(nymph, player, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, null,
                weapon: throwingKnife,
                expectedMessage: "Something throws something.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: throwingKnife,
                expectedMessage: "A throwing knife hits you! [11 pts.]");

            Verify(player, null, player, SenseType.Sight, SenseType.None, AbilityAction.Throw, null,
                weapon: throwingKnife,
                expectedMessage: "You throw a throwing knife.");

            Verify(player, blob, player, SenseType.None, SenseType.Sound, AbilityAction.Hit, null,
                weapon: throwingKnife,
                expectedMessage: "A throwing knife misses something.");

            Verify(player, null, player, SenseType.None, SenseType.None, AbilityAction.Hit, null, weapon: throwingKnife,
                expectedMessage: null);

            Verify(nymph, blob, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, null,
                weapon: throwingKnife,
                expectedMessage: "You hear a noise.");

            Verify(nymph, blob, player, SenseType.None, SenseType.SoundDistant, AbilityAction.Hit, 2,
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
            int? damage,
            GameEntity weapon = null,
            string expectedMessage = "")
        {
            var languageService = CreateLanguageService();

            var appliedEffects = new List<GameEntity>();
            if (damage.HasValue)
            {
                var manager = sensor.Manager;
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
                        appliedEffect.ActivatableEntityId = weapon.Id;
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

            var languageService = CreateLanguageService();

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

            var languageService = CreateLanguageService();

            Assert.Equal("The water nymph equips something on the torso.",
                languageService.GetString(new ItemEquipmentEvent(
                    player, nymph, armor, SenseType.Sight, SenseType.Sound, EquipmentSlot.Torso)));

            Assert.Equal("Something equips a mail armor.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sound, SenseType.Sight, EquipmentSlot.Torso)));

            Assert.Equal("You equip something on the torso.", languageService.GetString(new ItemEquipmentEvent(
                player, player, armor, SenseType.Sight | SenseType.Touch, SenseType.Touch, EquipmentSlot.Torso)));

            var sword = ItemData.LongSword.Instantiate(level.Entity.Manager).Referenced;

            Assert.Equal("You equip a long sword in the main hand.", languageService.GetString(new ItemEquipmentEvent(
                player, player, sword, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch, EquipmentSlot.GraspPrimaryExtremity)));
        }

        [Fact]
        public void ItemUnequipmentEvent()
        {
            var level = TestHelper.BuildLevel();
            var armor = ItemData.MailArmor.Instantiate(level.Entity.Manager).Referenced;
            var nymph = CreatureData.WaterNymph.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));

            var languageService = CreateLanguageService();

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
            var potion = ItemData.PotionOfHealing.Instantiate(level.Entity.Manager).Referenced;
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));

            var languageService = CreateLanguageService();

            Assert.Equal("The acid blob drinks a potion of healing.", languageService.GetString(new ItemActivationEvent(
                player, potion, blob, blob, null, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound,
                SenseType.Sight | SenseType.Sound, ActivationType.ManualActivation, successful: true)));

            Assert.Equal("You drink a potion of healing.", languageService.GetString(new ItemActivationEvent(
                player, potion, player, player, null, SenseType.Sight | SenseType.Sound,
                SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch, ActivationType.ManualActivation, successful: true)));
        }

        [Fact]
        public void ItemPickUpEvent()
        {
            var level = TestHelper.BuildLevel();
            var coins = ItemData.GoldCoin.Instantiate(level.Entity.Manager).Referenced;
            var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 0));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level.Entity, new Point(0, 2));

            var languageService = CreateLanguageService();

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

            var languageService = CreateLanguageService();

            Assert.Equal("The acid blob drops 11 gold coins.", languageService.GetString(new ItemDropEvent(
                player, blob, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));

            Assert.Equal("You drop 11 gold coins.", languageService.GetString(new ItemDropEvent(
                player, player, coins, 11, SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound)));
        }

        [Fact]
        public void Welcome()
        {
            var level = TestHelper.BuildLevel();
            var player = PlayerRace.InstantiatePlayer("Conan the Barbarian", Sex.Male, level.Entity, new Point(0, 0));
            var languageService = CreateLanguageService();

            var message = languageService.Welcome(player);

            Assert.Equal("Welcome to the test branch, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}

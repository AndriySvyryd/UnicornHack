using System.Collections.Generic;
using System.Linq;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Services.English;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Systems.Knowledge
{
    public class LoggingSystemTest
    {
        // TODO: Test explosion

        [Fact]
        public void AttackEvent()
        {
            var level = TestHelper.BuildLevel(@"
...
...
...");
            var wizard = CreatureData.WizardOfYendor.Instantiate(level, new Point(0, 0));

            var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));

            var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
            playerEntity.Position.Heading = Direction.West;
            var manager = playerEntity.Manager;

            manager.Queue.ProcessQueue(manager);

            ItemData.LongSword.Instantiate(playerEntity);

            var player2Entity = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(2, 0));
            player2Entity.Position.Heading = Direction.North;

            manager.Queue.ProcessQueue(manager);

            var swordEntity = playerEntity.Being.Items
                .Single(e => e.Item.TemplateName == ItemData.LongSword.Name);
            var equipMessage = EquipItemMessage.Create(manager);
            equipMessage.ActorEntity = playerEntity;
            equipMessage.ItemEntity = swordEntity;
            equipMessage.Slot = EquipmentSlot.GraspBothMelee;

            manager.Enqueue(equipMessage);
            manager.Queue.ProcessQueue(manager);
            playerEntity.Player.LogEntries.Clear();
            player2Entity.Player.LogEntries.Clear();

            var wizardPunch = wizard.Being.Abilities
                .Single(a => a.Ability.Action == AbilityAction.Punch);

            var activateMessage = ActivateAbilityMessage.Create(manager);
            activateMessage.AbilityEntity = wizardPunch;
            activateMessage.ActivatorEntity = wizard;
            activateMessage.TargetEntity = undine;

            var stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
            manager.ReturnMessage(activateMessage);

            Assert.Equal(97, stats.SubAttacks
                .Aggregate(0, (sum, a) => sum + manager.EffectApplicationSystem.GetExpectedDamage(a.Effects, wizard, undine)));

            Verify(wizard, undine, playerEntity, player2Entity,
                wizardPunch, success: true,
                "The Wizard of Yendor punches the undine. (<damage physical='97'>97</damage> pts.)",
                "The Wizard of Yendor punches something.",
                manager);

            Verify(wizard, undine, playerEntity, player2Entity,
                wizardPunch, success: false,
                "The Wizard of Yendor tries to punch the undine, but misses.",
                "The Wizard of Yendor tries to punch something, but misses.",
                manager);

            Verify(playerEntity, wizard, playerEntity, player2Entity,
                playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot], success: true,
                "You slash the Wizard of Yendor. (<damage physical='90'>90</damage> pts.)",
                "Dudley slashes the Wizard of Yendor. (<damage physical='90'>90</damage> pts.)",
                manager);

            Verify(playerEntity, wizard, playerEntity, player2Entity,
                playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot], success: false,
                "You try to slash the Wizard of Yendor, but miss.",
                "Dudley tries to slash the Wizard of Yendor, but misses.",
                manager);

            var wizardSpell = wizard.Being.Abilities
                .Single(a => a.Ability.Action == AbilityAction.Spell);
            Verify(wizard, playerEntity, playerEntity, player2Entity,
                wizardSpell, success: true,
                "The Wizard of Yendor casts a spell at you! You are unaffected.",
                "The Wizard of Yendor casts a spell at Dudley. He is unaffected.",
                manager);

            Verify(wizard, playerEntity, playerEntity, player2Entity,
                wizardSpell, success: false,
                "The Wizard of Yendor tries to cast a spell at you, but misses.",
                "The Wizard of Yendor tries to cast a spell at Dudley, but misses.",
                manager);

            Verify(wizard, playerEntity, player2Entity, SenseType.Sight, SenseType.Sight, AbilityAction.Spell,
                manager, null,
                expectedMessage: "The Wizard of Yendor tries to cast a spell at Dudley, but misses.");

            Verify(wizard, undine, playerEntity, SenseType.Sound, SenseType.Sight, AbilityAction.Sting, manager, 11,
                expectedMessage: "You hear a noise.");

            Verify(undine, playerEntity, playerEntity, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch,
                AbilityAction.Punch, manager, 11,
                expectedMessage: "The undine punches you! [<damage physical='11'>11</damage> pts.]");

            Verify(undine, playerEntity, playerEntity, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch,
                AbilityAction.Spit, manager, null,
                expectedMessage: "The undine tries to spit at you, but misses.");

            Verify(playerEntity, wizard, playerEntity, SenseType.Sight | SenseType.Touch, SenseType.Sight,
                AbilityAction.Hug, manager, 11,
                expectedMessage: "You squeeze the Wizard of Yendor. (<damage physical='11'>11</damage> pts.)");

            Verify(playerEntity, wizard, playerEntity, SenseType.Sight | SenseType.Touch,
                SenseType.Telepathy | SenseType.Touch,
                AbilityAction.Trample, manager, null,
                expectedMessage: "You try to trample the Wizard of Yendor, but miss.");

            Verify(wizard, wizard, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Claw, manager,
                11,
                expectedMessage: "The Wizard of Yendor claws himself. (<damage physical='11'>11</damage> pts.)");

            Verify(playerEntity, playerEntity, playerEntity, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch,
                AbilityAction.Kick, manager, 12,
                expectedMessage: "You kick yourself! [<damage physical='12'>12</damage> pts.]");

            Verify(undine, playerEntity, playerEntity, SenseType.Sight | SenseType.Sound,
                SenseType.Sight | SenseType.Touch,
                AbilityAction.Scream, manager, 0,
                expectedMessage: "The undine screams at you! You are unaffected.");

            Verify(undine, undine, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Scream, manager, 0,
                expectedMessage: "The undine screams at herself. She is unaffected.");

            Verify(undine, undine, playerEntity, SenseType.Sound, SenseType.None, AbilityAction.Scream, manager, null,
                expectedMessage: "You hear a scream.");

            var dagger = ItemData.Dagger.Instantiate(playerEntity.Manager).Referenced;

            Verify(playerEntity, null, playerEntity, SenseType.Touch | SenseType.Telepathy, SenseType.Sight,
                AbilityAction.Slash, manager, null,
                weapon: dagger,
                expectedMessage: "You slash the air.");

            var bow = ItemData.Shortbow.Instantiate(playerEntity.Manager).Referenced;
            var arrow = ItemData.Arrow.Instantiate(playerEntity.Manager).Referenced;

            Verify(undine, playerEntity, playerEntity, SenseType.Sight, SenseType.None, AbilityAction.Shoot, manager,
                null, weapon: bow,
                expectedMessage: null);

            Verify(undine, playerEntity, playerEntity, SenseType.None, SenseType.Sight, AbilityAction.Hit, manager, 11,
                weapon: arrow,
                expectedMessage: "Something hits you! [<damage physical='11'>11</damage> pts.]");

            Verify(undine, playerEntity, playerEntity, SenseType.SoundDistant, SenseType.None, AbilityAction.Shoot,
                manager, null,
                weapon: bow,
                expectedMessage: null);

            Verify(undine, playerEntity, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Hit, manager,
                null, weapon: arrow,
                expectedMessage: "An arrow misses you.");

            Verify(playerEntity, wizard, playerEntity, SenseType.Sight, SenseType.None, AbilityAction.Shoot,
                manager, null, weapon: bow,
                expectedMessage: null);

            Verify(playerEntity, wizard, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Hit, manager,
                11, weapon: arrow,
                expectedMessage: "An arrow hits the Wizard of Yendor. (<damage physical='11'>11</damage> pts.)");

            Verify(undine, wizard, playerEntity, SenseType.Sound, SenseType.None, AbilityAction.Shoot, manager, null,
                weapon: bow,
                expectedMessage: "You hear a noise.");

            Verify(undine, wizard, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Hit, manager, 2,
                weapon: arrow,
                expectedMessage: "An arrow hits the Wizard of Yendor. (<damage physical='2'>2</damage> pts.)");

            var throwingKnife = ItemData.ThrowingKnife.Instantiate(playerEntity.Manager).Referenced;

            Verify(undine, playerEntity, playerEntity, SenseType.Sound, SenseType.None, AbilityAction.Throw,
                manager,
                null,
                weapon: throwingKnife,
                expectedMessage: null);

            Verify(undine, playerEntity, playerEntity, SenseType.Sight, SenseType.Sight, AbilityAction.Hit, manager,
                11,
                weapon: throwingKnife,
                expectedMessage: "A throwing knife hits you! [<damage physical='11'>11</damage> pts.]");

            Verify(playerEntity, null, playerEntity, SenseType.None, SenseType.None, AbilityAction.Hit, manager, null,
                weapon: throwingKnife,
                expectedMessage: null);

            Verify(undine, wizard, playerEntity, SenseType.Sound, SenseType.None, AbilityAction.Throw, manager, null,
                weapon: throwingKnife,
                expectedMessage: "You hear a noise.");

            Verify(undine, wizard, playerEntity, SenseType.None, SenseType.SoundDistant, AbilityAction.Hit, manager,
                2,
                weapon: throwingKnife,
                expectedMessage: "You hear a distant noise.");
        }

        private static void Verify(
            GameEntity attacker,
            GameEntity victim,
            GameEntity player1,
            GameEntity player2,
            GameEntity ability,
            bool success,
            string expectedMessage1,
            string expectedMessage2,
            GameManager manager)
        {
            var activationMessage = ActivateAbilityMessage.Create(manager);
            activationMessage.AbilityEntity = ability;
            activationMessage.ActivatorEntity = attacker;
            activationMessage.TargetEntity = victim;
            ability.Ability.CooldownTick = null;

            manager.Enqueue(activationMessage);

            ((TestRandom)manager.Game.Random).EnqueueNextBool(!success);
            manager.Queue.ProcessQueue(manager);

            Assert.Equal(expectedMessage1,
                player1.Player.LogEntries.Single().Message);
            player1.Player.LogEntries.Clear();
            Assert.Equal(expectedMessage2,
                player2.Player.LogEntries.Single().Message);
            player2.Player.LogEntries.Clear();
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
                    appliedEffect.AppliedAmount = damage.Value;
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
                        appliedEffect.AppliedAmount = damage.Value;
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
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
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
            var nymph = CreatureData.Undine.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The undine equips something on the torso.",
                languageService.GetString(new ItemEquipmentEvent(
                    player, nymph, armor, SenseType.Sight, SenseType.Sound, EquipmentSlot.Torso)));

            Assert.Equal("Something equips a mail armor.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sound, SenseType.Sight, EquipmentSlot.Torso)));

            Assert.Equal("You equip something on the torso.", languageService.GetString(new ItemEquipmentEvent(
                player, player, armor, SenseType.Sight | SenseType.Touch, SenseType.Touch, EquipmentSlot.Torso)));

            var sword = ItemData.LongSword.Instantiate(level.Entity.Manager).Referenced;

            Assert.Equal("You equip the long sword in the main hand for melee.", languageService.GetString(new ItemEquipmentEvent(
                player, player, sword, SenseType.Sight | SenseType.Touch,
                SenseType.Sight | SenseType.Touch, EquipmentSlot.GraspPrimaryMelee)));
        }

        [Fact]
        public void ItemUnequipmentEvent()
        {
            var level = TestHelper.BuildLevel();
            var armor = ItemData.MailArmor.Instantiate(level.Entity.Manager).Referenced;
            var nymph = CreatureData.Undine.Instantiate(level, new Point(0, 1));
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The undine unequips something.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sight, SenseType.Sound, EquipmentSlot.None)));

            Assert.Equal("Something unequips a mail armor.", languageService.GetString(new ItemEquipmentEvent(
                player, nymph, armor, SenseType.Sound, SenseType.Sight, EquipmentSlot.None)));

            Assert.Equal("You unequip something.", languageService.GetString(new ItemEquipmentEvent(
                player, player, armor, SenseType.Sight | SenseType.Touch, SenseType.Touch, EquipmentSlot.None)));

            var sword = ItemData.LongSword.Instantiate(level.Entity.Manager).Referenced;

            Assert.Equal("You unequip the long sword.", languageService.GetString(new ItemEquipmentEvent(
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
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("The acid blob drinks from a flask of healing.", languageService.GetString(
                new ItemActivationEvent(player, flask, blob, blob,
                    SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Sound,
                    SenseType.Sight | SenseType.Sound, consumed: false, successful: true)));

            Assert.Equal("You drink from the flask of healing.", languageService.GetString(
                new ItemActivationEvent(player, flask, player, player,
                    SenseType.Sight | SenseType.Sound, SenseType.Sight | SenseType.Touch,
                    SenseType.Sight | SenseType.Touch, consumed: false, successful: true)));

            Assert.Equal("You drink the potion of experience.", languageService.GetString(
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
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
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
            var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
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
            var player1 = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
            var player2 = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(0, 1));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            Assert.Equal("You level up! You gain 2 SP 1 TP 0 MP.",
                languageService.GetString(new LeveledUpEvent(
                    player1, player1, player1.Being.Races.Single().Race, 2, 1, 0)));

            Assert.Equal("Cudley levels up! She gains 3 SP 2 TP 1 MP.",
                languageService.GetString(new LeveledUpEvent(
                    player1, player2, player2.Being.Races.Single().Race, 3, 2, 1)));
        }

        [Fact]
        public void Welcome()
        {
            var level = TestHelper.BuildLevel();
            var player = PlayerRace.InstantiatePlayer("Conan the Barbarian", Sex.Male, level, new Point(0, 0));
            var manager = level.Entity.Manager;
            var languageService = manager.Game.Services.Language;

            var message = languageService.Welcome(player);

            Assert.Equal("Welcome to the test branch, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}

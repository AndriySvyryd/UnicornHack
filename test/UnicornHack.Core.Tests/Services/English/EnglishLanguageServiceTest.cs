using System.Collections.Generic;
using UnicornHack.Abilities;
using UnicornHack.Data.Items;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using Xunit;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageServiceTest
    {
        [Fact]
        public void AttackEvent()
        {
            var newt = new Creature {VariantName = "newt"};
            var nymph = new Creature {VariantName = "water nymph", Sex = Sex.Female};
            var rodney = new Creature {VariantName = "Wizard of Yendor", Sex = Sex.None};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};
            var player2 = new Player {VariantName = "human", Name = "Cudley", Sex = Sex.Male};

            Verify(newt, nymph, player, SenseType.Sight, SenseType.Sight, AbilityAction.Bite, 11,
                expectedMessage: "The newt bites the water nymph. (11 pts.)");

            Verify(player, rodney, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "Dudley stings Wizard of Yendor. (11 pts.)");

            Verify(rodney, player, player2, SenseType.Sight, SenseType.Sight, AbilityAction.Spell, null,
                expectedMessage: "Wizard of Yendor tries to cast a spell at Dudley, but misses.");

            Verify(newt, nymph, player, SenseType.Sound, SenseType.Sight, AbilityAction.Sting, 11,
                expectedMessage: "You hear a noise.");

            Verify(newt, nymph, player, SenseType.Sight, SenseType.SoundDistant | SenseType.Danger | SenseType.Touch,
                AbilityAction.Headbutt, 11,
                expectedMessage: "The newt headbutts the water nymph. (11 pts.)");

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

            var game = TestHelper.CreateGame();
            var dagger = ItemVariantData.Dagger.Instantiate(game);

            Verify(player, newt, player, SenseType.Touch, SenseType.Sight, AbilityAction.Slash, 11, weapon: dagger,
                expectedMessage: "You slash the newt with the dagger. (11 pts.)");

            Verify(player, null, player, SenseType.Touch, SenseType.Sight, AbilityAction.Slash, null, weapon: dagger,
                expectedMessage: "You slash the air with the dagger.");

            var bow = (Launcher)ItemVariantData.Shortbow.Instantiate(game);

            Verify(nymph, player, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "The water nymph shoots with a shortbow.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: bow.Projectile,
                expectedMessage: "An arrow hits you! [11 pts.]");

            Verify(nymph, player, player, SenseType.SoundDistant, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "Something shoots with something.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, null, weapon: bow.Projectile,
                expectedMessage: "An arrow misses you.");

            Verify(player, newt, player, SenseType.Sight, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "You shoot with the shortbow.");

            Verify(player, newt, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: bow.Projectile,
                expectedMessage: "An arrow hits the newt. (11 pts.)");

            Verify(nymph, newt, player, SenseType.Sound, SenseType.None, AbilityAction.Shoot, null, weapon: bow,
                expectedMessage: "You hear a noise.");

            Verify(nymph, newt, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 2, weapon: bow.Projectile,
                expectedMessage: "An arrow hits the newt. (2 pts.)");

            var throwingKnives = (Launcher)ItemVariantData.ThrowingKnives.Instantiate(game);

            Verify(nymph, player, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, null, weapon: throwingKnives,
                expectedMessage: "Something throws something.");

            Verify(nymph, player, player, SenseType.None, SenseType.Sight, AbilityAction.Hit, 11, weapon: throwingKnives.Projectile,
                expectedMessage: "A throwing knife hits you! [11 pts.]");

            Verify(player, null, player, SenseType.Sight, SenseType.None, AbilityAction.Throw, null, weapon: throwingKnives,
                expectedMessage: "You throw a throwing knife.");

            Verify(player, newt, player, SenseType.None, SenseType.Sound, AbilityAction.Hit, null, weapon: throwingKnives.Projectile,
                expectedMessage: "A throwing knife misses something.");

            Verify(player, null, player, SenseType.None, SenseType.None, AbilityAction.Hit, null, weapon: throwingKnives.Projectile,
                expectedMessage: null);

            Verify(nymph, newt, player, SenseType.Sound, SenseType.None, AbilityAction.Throw, null, weapon: throwingKnives,
                expectedMessage: "You hear a noise.");

            Verify(nymph, newt, player, SenseType.None, SenseType.Sound, AbilityAction.Hit, 2, weapon: throwingKnives.Projectile,
                expectedMessage: "A throwing knife hits something.");
        }

        private void Verify(
            Actor attacker,
            Actor victim,
            Player sensor,
            SenseType attackerSensed,
            SenseType victimSensed,
            AbilityAction abilityAction,
            int? damage,
            Item weapon = null,
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
                Hit = damage.HasValue
            };

            if (damage.HasValue)
            {
                attackEvent.AppliedEffects.Add(new PhysicallyDamaged {Damage = damage.Value});
            }

            if (weapon != null)
            {
                if ((weapon.Type & ItemType.WeaponRanged) == 0)
                {
                    attackEvent.AppliedEffects.Add(new MeleeAttacked {Weapon = weapon});
                }
                else
                {
                    attackEvent.AppliedEffects.Add(new RangeAttacked {Weapon = weapon});
                }
            }

            Assert.Equal(expectedMessage, languageService.ToString(attackEvent));
        }

        [Fact]
        public void DeathEvent()
        {
            var newt = new Creature {VariantName = "newt"};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The newt dies.", languageService.ToString(new DeathEvent
            {
                Deceased = newt,
                DeceasedSensed = SenseType.Sight,
                Sensor = player
            }));

            Assert.Equal("You die!", languageService.ToString(new DeathEvent
            {
                Deceased = player,
                DeceasedSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemEquipmentEvent()
        {
            var armor = new Item {VariantName = "mail armor", EquippedSlot = EquipmentSlot.Torso};
            var nymph = new Creature {VariantName = "water nymph", Sex = Sex.Female};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The water nymph equips something on the body.",
                languageService.ToString(new ItemEquipmentEvent
                {
                    Item = armor,
                    ItemSensed = SenseType.Sound,
                    Equipper = nymph,
                    EquipperSensed = SenseType.Sight,
                    Sensor = player
                }));

            Assert.Equal("Something equips a mail armor.", languageService.ToString(new ItemEquipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Sight,
                Equipper = nymph,
                EquipperSensed = SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You equip something on the body.", languageService.ToString(new ItemEquipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Touch,
                Equipper = player,
                EquipperSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));

            var sword = new Item {VariantName = "long sword", EquippedSlot = EquipmentSlot.GraspPrimaryExtremity};
            Assert.Equal("You equip a long sword in the main hand.", languageService.ToString(new ItemEquipmentEvent
            {
                Item = sword,
                ItemSensed = SenseType.Sight | SenseType.Touch,
                Equipper = player,
                EquipperSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemUnequipmentEvent()
        {
            var armor = new Item {VariantName = "mail armor", EquippedSlot = EquipmentSlot.Torso};
            var nymph = new Creature {VariantName = "water nymph", Sex = Sex.Female};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The water nymph unequips something.", languageService.ToString(new ItemUnequipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Sound,
                Unequipper = nymph,
                UnequipperSensed = SenseType.Sight,
                Sensor = player
            }));

            Assert.Equal("Something unequips a mail armor.", languageService.ToString(new ItemUnequipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Sight,
                Unequipper = nymph,
                UnequipperSensed = SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You unequip something.", languageService.ToString(new ItemUnequipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Touch,
                Unequipper = player,
                UnequipperSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));

            Assert.Equal("You unequip a mail armor.", languageService.ToString(new ItemUnequipmentEvent
            {
                Item = armor,
                ItemSensed = SenseType.Sight | SenseType.Touch,
                Unequipper = player,
                UnequipperSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemConsumptionEvent()
        {
            var potion = new Item {VariantName = "potion of healing", Type = ItemType.Potion};
            var newt = new Creature {VariantName = "newt"};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The newt drinks a potion of healing.", languageService.ToString(new ItemConsumptionEvent
            {
                Item = potion,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Consumer = newt,
                ConsumerSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You drink a potion of healing.", languageService.ToString(new ItemConsumptionEvent
            {
                Item = potion,
                ItemSensed = SenseType.Sight | SenseType.Touch,
                Consumer = player,
                ConsumerSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemPickUpEvent()
        {
            var coins = (Gold)GoldVariant.Get().Instantiate(new Game());
            coins.Quantity = 11;
            var newt = new Creature {VariantName = "newt"};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The newt picks up 11 gold coins.", languageService.ToString(new ItemPickUpEvent
            {
                Item = coins,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Picker = newt,
                PickerSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You pick up 11 gold coins.", languageService.ToString(new ItemPickUpEvent
            {
                Item = coins,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Picker = player,
                PickerSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemDropEvent()
        {
            var coins = (Gold)GoldVariant.Get().Instantiate(new Game());
            coins.Quantity = 11;
            var newt = new Creature {VariantName = "newt"};
            var player = new Player {VariantName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The newt drops 11 gold coins.", languageService.ToString(new ItemDropEvent
            {
                Item = coins,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Dropper = newt,
                DropperSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You drop 11 gold coins.", languageService.ToString(new ItemDropEvent
            {
                Item = coins,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Dropper = player,
                DropperSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));
        }

        [Fact]
        public void Welcome()
        {
            var languageService = CreateLanguageService();

            var message = languageService.Welcome(
                new Player
                {
                    VariantName = "human",
                    Name = "Conan the Barbarian",
                    Level = new Level {Branch = new Branch {Name = "Dungeon of Fun"}}
                });

            Assert.Equal("Welcome to the Dungeon of Fun, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}
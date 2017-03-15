using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation.Map;
using Xunit;

namespace UnicornHack.Services.English
{
    public class EnglishLanguageServiceTest
    {
        [Fact]
        public void AttackEvent()
        {
            var newt = new Creature {BaseName = "newt"};
            var nymph = new Creature {BaseName = "water nymph", Sex = Sex.Female};
            var rodney = new Creature {BaseName = "Wizard of Yendor", Sex = Sex.None};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};
            var player2 = new Player {BaseName = "human", Name = "Cudley", Sex = Sex.Male};

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
            Player sensor,
            SenseType attackerSensed,
            SenseType victimSensed,
            AbilityAction abilityAction,
            int? damage,
            Item weapon = null,
            Item projectile = null,
            string expectedMessage = "")
        {
            var languageService = CreateLanguageService();
            var ability = new Ability
            {
                Action = abilityAction,
                Effects = new HashSet<Effect>()
            };
            var attackEvent = new AttackEvent
            {
                Attacker = attacker,
                Victim = victim,
                Sensor = sensor,
                AttackerSensed = attackerSensed,
                VictimSensed = victimSensed,
                Ability = ability,
                Hit = damage.HasValue
            };

            if (damage.HasValue)
            {
                ability.Effects.Add(new PhysicalDamage {Damage = damage.Value});
            }

            if (weapon != null)
            {
                ability.Effects.Add(new MeleeAttack {Weapon = weapon});
            }

            Assert.Equal(expectedMessage, languageService.ToString(attackEvent));
        }

        [Fact]
        public void DeathEvent()
        {
            var newt = new Creature {BaseName = "newt"};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

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
            var armor = new Item {BaseName = "mail armor", EquippedSlot = EquipmentSlot.Body};
            var nymph = new Creature {BaseName = "water nymph", Sex = Sex.Female};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

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

            var sword = new Item {BaseName = "long sword", EquippedSlot = EquipmentSlot.GraspMainExtremity};
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
            var armor = new Item {BaseName = "mail armor", EquippedSlot = EquipmentSlot.Body};
            var nymph = new Creature {BaseName = "water nymph", Sex = Sex.Female};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

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
            var carrot = new Item {BaseName = "carrot"};
            var newt = new Creature {BaseName = "newt"};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

            var languageService = CreateLanguageService();

            Assert.Equal("The newt eats a carrot.", languageService.ToString(new ItemConsumptionEvent
            {
                Item = carrot,
                ItemSensed = SenseType.Sight | SenseType.Sound,
                Consumer = newt,
                ConsumerSensed = SenseType.Sight | SenseType.Sound,
                Sensor = player
            }));

            Assert.Equal("You eat a carrot.", languageService.ToString(new ItemConsumptionEvent
            {
                Item = carrot,
                ItemSensed = SenseType.Sight | SenseType.Touch,
                Consumer = player,
                ConsumerSensed = SenseType.Sight | SenseType.Touch,
                Sensor = player
            }));
        }

        [Fact]
        public void ItemPickUpEvent()
        {
            var coins = (Gold)Gold.Get().Instantiate(new Game());
            coins.Quantity = 11;
            var newt = new Creature {BaseName = "newt"};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

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
            var coins = (Gold)Gold.Get().Instantiate(new Game());
            coins.Quantity = 11;
            var newt = new Creature {BaseName = "newt"};
            var player = new Player {BaseName = "human", Name = "Dudley", Sex = Sex.Male};

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
                    BaseName = "human",
                    Name = "Conan the Barbarian",
                    Level = new Level {Branch = new Branch{Name = "Dungeon of Fun" } }
                });

            Assert.Equal("Welcome to the Dungeon of Fun, Conan the Barbarian!", message);
        }

        protected EnglishLanguageService CreateLanguageService() => new EnglishLanguageService();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Generation;
using UnicornHack.Utils;
using Xunit;

namespace UnicornHack
{
    public class ActorTest
    {
        [Fact]
        public void Effects_applied_in_order()
        {
            var race = new PlayerRaceDefinition
            {
                Name = "troll",
                Species = Species.Troll,
                Abilities = new HashSet<AbilityDefinition>
                {
                    new AbilityDefinition
                    {
                        Name = "innate",
                        Activation = AbilityActivation.Always,
                        Effects = new HashSet<Effect>
                        {
                            new ChangeProperty<bool> {PropertyName = "humanoidness", Value = false, Function = ValueCombinationFunction.MeanRoundUp},
                            new ChangeProperty<int> {PropertyName = "strength", Value = 20, Function = ValueCombinationFunction.Override},
                            new ChangeProperty<int>
                            {
                                PropertyName = "size",
                                Value = 7,
                                Function = ValueCombinationFunction.MeanRoundUp
                            }
                        }
                    }
                }
            };

            var player = new Player(TestHelper.BuildLevel("..", Environment.TickCount), x: 0, y: 0);

            Assert.True(player.GetProperty<bool>("infravisibility"));
            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(10, player.GetProperty<int>("strength"));
            Assert.Equal(6, player.GetProperty<int>("size"));

            var activationContext = new AbilityActivationContext
            {
                Ability = new Ability(player.Game)
                {
                    Name = "become troll",
                    Activation = AbilityActivation.OnActivation
                },
                Target = player
            };
            using (activationContext)
            {
                var changedRace = race.Instantiate(activationContext);
                player.ActiveEffects.Add(changedRace);
                player.Add(changedRace.Ability);
            }

            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(20, player.GetProperty<int>("strength"));
            Assert.Equal(7, player.GetProperty<int>("size"));

            var activatableAbility =
                new Ability(player.Game)
                {
                    Name = "hunker down",
                    Activation = AbilityActivation.WhileToggled,
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new ChangeProperty<bool>(player.Game)
                        {
                            PropertyName = "humanoidness",
                            Value = true,
                            Function = ValueCombinationFunction.Max
                        },
                        new ChangeProperty<int>(player.Game)
                        {
                            PropertyName = "strength",
                            Value = 150,
                            Function = ValueCombinationFunction.Percent
                        },
                        new ChangeProperty<int>(player.Game)
                        {
                            PropertyName = "size",
                            Value = -2,
                            Function = ValueCombinationFunction.Sum
                        }
                    }
                };

            player.Add(activatableAbility);

            var permanentAbility =
                new Ability(player.Game)
                {
                    Name = "burly man",
                    Activation = AbilityActivation.Always,
                    Effects = new ObservableSnapshotHashSet<Effect>
                    {
                        new ChangeProperty<bool>(player.Game)
                        {
                            PropertyName = "humanoidness",
                            Value = false,
                            Function = ValueCombinationFunction.MeanRoundDown
                        },
                        new ChangeProperty<int>(player.Game)
                        {
                            PropertyName = "strength",
                            Value = 2,
                            Function = ValueCombinationFunction.Sum
                        },
                        new ChangeProperty<int>(player.Game)
                        {
                            PropertyName = "size",
                            Value = 10,
                            Function = ValueCombinationFunction.MeanRoundUp
                        }
                    }
                };

            player.Add(permanentAbility);

            Assert.False(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(22, player.GetProperty<int>("strength"));
            Assert.Equal(9, player.GetProperty<int>("size"));

            var context = new AbilityActivationContext
            {
                Activator = player,
                Target = player
            };
            activatableAbility.Activate(context);

            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(33, player.GetProperty<int>("strength"));
            Assert.Equal(7, player.GetProperty<int>("size"));

            var item = new Item(player.Game)
            {
                Name = "potion of strangeness",
                Type = ItemType.Potion,
                EquipableSizes = SizeCategory.Medium | SizeCategory.Large,
                EquipableSlots = EquipmentSlot.GraspSingleExtremity,
                Abilities = new ObservableSnapshotHashSet<Ability>
                {
                    new Ability(player.Game)
                    {
                        Activation = AbilityActivation.OnConsumption,
                        Effects = new ObservableSnapshotHashSet<Effect>
                        {
                            new ChangeProperty<bool>(player.Game)
                            {
                                PropertyName = "humanoidness",
                                Value = false,
                                Function = ValueCombinationFunction.MeanRoundUp,
                                Duration = 20
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "strength",
                                Value = 33,
                                Function = ValueCombinationFunction.Percent,
                                Duration = 20
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "size",
                                Value = 12,
                                Function = ValueCombinationFunction.MeanRoundDown,
                                Duration = 20
                            }
                        }
                    },
                    new Ability(player.Game)
                    {
                        Activation = AbilityActivation.WhileEquipped,
                        Effects = new ObservableSnapshotHashSet<Effect>
                        {
                            new ChangeProperty<bool>(player.Game)
                            {
                                PropertyName = "humanoidness",
                                Value = false,
                                Function = ValueCombinationFunction.MeanRoundUp
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "strength",
                                Value = 2,
                                Function = ValueCombinationFunction.Sum
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "size",
                                Value = 8,
                                Function = ValueCombinationFunction.MeanRoundDown
                            }
                        }
                    },
                    new Ability(player.Game)
                    {
                        Activation = AbilityActivation.WhilePossessed,
                        Effects = new ObservableSnapshotHashSet<Effect>
                        {
                            new ChangeProperty<bool>(player.Game)
                            {
                                PropertyName = "humanoidness",
                                Value = false,
                                Function = ValueCombinationFunction.MeanRoundUp
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "strength",
                                Value = 2,
                                Function = ValueCombinationFunction.Min
                            },
                            new ChangeProperty<int>(player.Game)
                            {
                                PropertyName = "size",
                                Value = 4,
                                Function = ValueCombinationFunction.Override
                            }
                        }
                    }
                }
            };

            player.TryAdd(item);

            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(2, player.GetProperty<int>("strength"));
            Assert.Equal(4, player.GetProperty<int>("size"));

            player.Equip(item, EquipmentSlot.GraspPrimaryExtremity);

            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(4, player.GetProperty<int>("strength"));
            Assert.Equal(6, player.GetProperty<int>("size"));

            player.Quaff(item);

            Assert.True(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(10, player.GetProperty<int>("strength"));
            Assert.Equal(9, player.GetProperty<int>("size"));

            activatableAbility.Deactivate();

            Assert.False(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(7, player.GetProperty<int>("strength"));
            Assert.Equal(10, player.GetProperty<int>("size"));

            player.Races.Single(r => r.Name == "human").Remove();

            Assert.False(player.GetProperty<bool>("infravisibility"));
            Assert.False(player.GetProperty<bool>("humanoidness"));
            Assert.Equal(7, player.GetProperty<int>("strength"));
            Assert.Equal(10, player.GetProperty<int>("size"));
        }
    }
}
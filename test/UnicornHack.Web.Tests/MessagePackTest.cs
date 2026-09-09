using MessagePack;
using MessagePack.Resolvers;
using System.Reflection;
using UnicornHack.Generation;
using UnicornHack.Hubs.ChangeTracking;
using Xunit;

namespace UnicornHack;

public class MessagePackTest
{
    [Fact]
    public void Integer_changed_property_masks_cover_contiguous_message_pack_keys()
    {
        Type[] dtoTypes =
        [
            typeof(AbilityChange),
            typeof(ActorChange),
            typeof(ConnectionChange),
            typeof(EffectChange),
            typeof(ItemChange),
            typeof(LogEntryChange),
            typeof(PlayerChange),
            typeof(RoomChange),
            typeof(RoomMapChanges),
            typeof(Hubs.AbilityAttributes),
            typeof(Hubs.ActorAttributes),
            typeof(Hubs.DialogData),
            typeof(Hubs.Item),
            typeof(Hubs.ItemAttributes),
            typeof(Hubs.PlayerSkills)
        ];

        foreach (var dtoType in dtoTypes)
        {
            var keyedMembers = dtoType
                .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                .Select(member => (Member: member, Key: member.GetCustomAttribute<KeyAttribute>()))
                .Where(entry => entry.Key != null)
                .OrderBy(entry => entry.Key!.IntKey)
                .ToArray();
            Assert.NotEmpty(keyedMembers);

            for (var expectedKey = 0; expectedKey < keyedMembers.Length; expectedKey++)
            {
                Assert.Equal(expectedKey, keyedMembers[expectedKey].Key!.IntKey);
            }

            var changedProperties = Assert.Single(
                keyedMembers, entry => entry.Key!.IntKey == 0);
            Assert.Equal(nameof(PlayerChange.ChangedProperties), changedProperties.Member.Name);

            var memberType = changedProperties.Member switch
            {
                PropertyInfo property => property.PropertyType,
                FieldInfo field => field.FieldType,
                _ => throw new InvalidOperationException(changedProperties.Member.MemberType.ToString())
            };
            var maskType = Nullable.GetUnderlyingType(memberType);
            var maskWidth = maskType == typeof(byte) ? 8
                : maskType == typeof(ushort) ? 16
                : maskType == typeof(uint) ? 32
                : maskType == typeof(ulong) ? 64
                : throw new InvalidOperationException(
                    $"{dtoType.Name}.ChangedProperties uses unsupported type {memberType}.");
            Assert.True(
                keyedMembers.Length <= maskWidth,
                $"{dtoType.Name} has {keyedMembers.Length} keys but its {maskType!.Name} mask has {maskWidth} bits.");

            if (dtoType.GetField(nameof(PlayerChange.PropertyCount)) is { } propertyCount)
            {
                Assert.Equal(keyedMembers.Length, propertyCount.GetRawConstantValue());
            }
        }
    }

    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithSecurity(MessagePackSecurity.UntrustedData)
        .WithResolver(CompositeResolver.Create(
            GeneratedResolver.Instance,
            BuiltinResolver.Instance));

    [Fact]
    public void Full_player_state_with_ability_expiration_and_noise_round_trips()
    {
        var state = new PlayerChange
        {
            ChangedProperties = null,
            Name = "Dudley",
            Abilities = new Dictionary<int, AbilityChange>
            {
                [1] = new AbilityChange
                {
                    ChangedProperties = null,
                    Name = "test ability",
                    ExpirationEvent = (int)EffectDuration.UntilXPGained,
                    Noise = 7
                }
            }
        };

        var cancellationToken = TestContext.Current.CancellationToken;
        var bytes = MessagePackSerializer.Serialize(state, Options, cancellationToken);
        var result = MessagePackSerializer.Deserialize<PlayerChange>(
            bytes, Options, cancellationToken);
        Assert.Equal((int)EffectDuration.UntilXPGained, result.Abilities![1].ExpirationEvent);
        Assert.Equal(7, result.Abilities[1].Noise);
    }

    [Fact]
    public void All_clear_ability_mask_serializes_as_a_single_element()
    {
        var change = new AbilityChange
        {
            ChangedProperties = 0
        };

        var bytes = MessagePackSerializer.Serialize(
            change, Options, TestContext.Current.CancellationToken);
        var reader = new MessagePackReader(bytes);

        Assert.Equal(1, reader.ReadArrayHeader());
        Assert.Equal((ushort?)0, Options.Resolver.GetFormatterWithVerify<ushort?>()
            .Deserialize(ref reader, Options));
        Assert.True(reader.End);
    }

    [Fact]
    public void Typed_dialog_data_round_trips()
    {
        var effect = new EffectAttributes
        {
            Type = EffectType.FireDamage,
            ShouldTargetActivator = true,
            Amount = "12",
            Function = ValueCombinationFunction.Subtract,
            TargetName = "HitPoints",
            SecondaryAmount = "3",
            Duration = EffectDuration.UntilTimeout,
            DurationAmount = "100"
        };
        var ability = new AbilityAttributes
        {
            ChangedProperties = null,
            Id = 5,
            Name = "Chilled",
            Activation = ActivationType.Always,
            ActivationCondition = 7,
            TargetingShape = TargetingShape.Line,
            Accumulator = 42,
            Noise = 18,
            ConditionalGrants =
            [
                new ConditionalGrantAttributes
                {
                    Name = "ice shard",
                    Level = 2,
                    LevelExpression = "AbilityLevel('conjuration') + AbilityLevel('water sourcery')"
                }
            ],
            SelfEffects = { [6] = effect },
            SubAbilities =
            [
                new SubAbilityAttributes
                {
                    SuccessCondition = AbilitySuccessCondition.NormalAttack,
                    Accuracy = 8,
                    Effects = { [9] = effect }
                }
            ]
        };
        var actor = new ActorAttributes
        {
            ChangedProperties = null,
            Name = "Dudley",
            Description = "A test actor",
            Level = 2,
            MovementDelay = 3,
            HitPoints = 17,
            WaterResistance = 19,
            EnergyRegeneration = 21,
            Abilities = [ability],
            Effects = { [10] = new EffectChange { ChangedProperties = null, Magnitude = 11 } }
        };
        var slot = new EquippableSlot { Id = 12, ShortName = "hand", Name = "primary hand" };
        var dialog = new DialogData
        {
            ChangedProperties = null,
            AbilitySlot = 3,
            SlottableAbilities =
            {
                [4] = new AbilityChange
                {
                    ChangedProperties = null,
                    Name = "Attack",
                    Accumulator = 42,
                    Level = 2,
                    LevelReduction = 3,
                    ExpirationTarget = 123,
                    Noise = 19,
                    ExpirationEvent = (int)EffectDuration.UntilXPGained
                }
            },
            PlayerAttributes = actor,
            PlayerInventory = new PlayerInventory
            {
                Items =
                {
                    [13] = new Hubs.Item
                    {
                        ChangedProperties = null,
                        Type = ItemType.Weapon,
                        BaseName = "sword",
                        Name = "a sword",
                        EquippableSlots = { [12] = slot },
                        EquippedSlot = slot
                    }
                }
            },
            PlayerAdaptations = new PlayerAdaptations
            {
                Traits = { ["strong"] = 2 },
                Feats = { ["wings"] = 3 }
            },
            PlayerSkills = new PlayerSkills
            {
                ChangedProperties = null,
                HandWeapons = 16,
                Leadership = 17
            },
            AbilityAttributes = ability,
            ActorAttributes = actor,
            ItemAttributes = new ItemAttributes
            {
                ChangedProperties = null,
                Name = "Sword",
                Description = "Sharp",
                Type = ItemType.Weapon,
                Material = Material.Steel,
                Complexity = ItemComplexity.Normal,
                RequiredMight = 20,
                EquippableSlots = { [12] = slot },
                Abilities = [ability]
            },
            StaticDescription = "Description",
            PostGameStatistics = new PostGameStatistics(),
            LevelMap = new LevelMap
            {
                Rooms = [new RoomEntry { Id = 21, ConnectedTo = [22] }],
                AvailableTargets = [21]
            }
        };

        var bytes = MessagePackSerializer.Serialize(
            dialog, Options, TestContext.Current.CancellationToken);
        var result = MessagePackSerializer.Deserialize<DialogData>(
            bytes, Options, TestContext.Current.CancellationToken);

        Assert.Equal(3, result.AbilitySlot);
        Assert.Equal(42, result.SlottableAbilities[4].Accumulator);
        Assert.Equal(2, result.SlottableAbilities[4].Level);
        Assert.Equal(3, result.SlottableAbilities[4].LevelReduction);
        Assert.Equal(123, result.SlottableAbilities[4].ExpirationTarget);
        Assert.Equal(19, result.SlottableAbilities[4].Noise);
        Assert.Equal((int)EffectDuration.UntilXPGained, result.SlottableAbilities[4].ExpirationEvent);
        Assert.Equal("Dudley", result.PlayerAttributes!.Name);
        Assert.Equal(17, result.PlayerAttributes.HitPoints);
        Assert.Equal(42, Assert.Single(result.PlayerAttributes.Abilities).Accumulator);
        Assert.Equal(11, Assert.Single(result.PlayerAttributes.Effects).Value.Magnitude);
        Assert.Equal("a sword", result.PlayerInventory!.Items[13].Name);
        Assert.Equal("primary hand", result.PlayerInventory.Items[13].EquippedSlot!.Name);
        Assert.Equal(2, result.PlayerAdaptations!.Traits["strong"]);
        Assert.Equal(17, result.PlayerSkills!.Leadership);
        Assert.Equal("3", result.AbilityAttributes!.SelfEffects[6].SecondaryAmount);
        Assert.Equal(8, Assert.Single(result.AbilityAttributes.SubAbilities).Accuracy);
        Assert.Equal(18, result.AbilityAttributes.Noise);
        var conditionalGrant = Assert.Single(result.AbilityAttributes.ConditionalGrants);
        Assert.Equal("ice shard", conditionalGrant.Name);
        Assert.Equal(2, conditionalGrant.Level);
        Assert.Equal(
            "AbilityLevel('conjuration') + AbilityLevel('water sourcery')",
            conditionalGrant.LevelExpression);
        Assert.Equal(19, result.ActorAttributes!.WaterResistance);
        Assert.Equal(21, result.ActorAttributes.EnergyRegeneration);
        Assert.Equal(20, result.ItemAttributes!.RequiredMight);
        Assert.Equal("Description", result.StaticDescription);
        Assert.NotNull(result.PostGameStatistics);
        Assert.Equal(21, Assert.Single(result.LevelMap!.Rooms).Id);
    }

    [Fact]
    public void Sparse_level_map_dialog_contains_only_its_mask_and_payload()
    {
        var dialog = new DialogData
        {
            ChangedProperties = 1 << 12,
            LevelMap = new LevelMap
            {
                Rooms = [new RoomEntry { Id = 21 }],
                AvailableTargets = [21]
            }
        };

        var bytes = MessagePackSerializer.Serialize(
            dialog, Options, TestContext.Current.CancellationToken);
        var reader = new MessagePackReader(bytes);
        Assert.Equal(2, reader.ReadArrayHeader());
        Assert.Equal((ushort?)(1 << 12), Options.Resolver.GetFormatterWithVerify<ushort?>()
            .Deserialize(ref reader, Options));
        Assert.NotNull(Options.Resolver.GetFormatterWithVerify<LevelMap?>()
            .Deserialize(ref reader, Options));
        Assert.True(reader.End);
    }

    [Fact]
    public void Deleted_dialog_contains_only_its_mask()
    {
        var bytes = MessagePackSerializer.Serialize(
            DialogData.Deleted(), Options, TestContext.Current.CancellationToken);
        var reader = new MessagePackReader(bytes);
        Assert.Equal(1, reader.ReadArrayHeader());
        var deleted = Options.Resolver.GetFormatterWithVerify<ushort?>()
            .Deserialize(ref reader, Options);
        Assert.Equal((ushort?)0, deleted);
        Assert.True(reader.End);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    public void Sparse_deleted_attribute_dialog_contains_only_its_masks(int field)
    {
        var dialog = new DialogData
        {
            ChangedProperties = (ushort)(1 << field),
            ActorAttributes = field == 8
                ? new ActorAttributes { ChangedProperties = 0 }
                : null,
            ItemAttributes = field == 9
                ? new ItemAttributes { ChangedProperties = 0 }
                : null
        };

        var bytes = MessagePackSerializer.Serialize(
            dialog, Options, TestContext.Current.CancellationToken);
        var reader = new MessagePackReader(bytes);
        Assert.Equal(2, reader.ReadArrayHeader());
        Assert.Equal((ushort?)(1 << field), Options.Resolver.GetFormatterWithVerify<ushort?>()
            .Deserialize(ref reader, Options));
        Assert.Equal(1, reader.ReadArrayHeader());
        if (field == 8)
        {
            Assert.Equal((ulong?)0, Options.Resolver.GetFormatterWithVerify<ulong?>()
                .Deserialize(ref reader, Options));
        }
        else
        {
            Assert.Equal((ushort?)0, Options.Resolver.GetFormatterWithVerify<ushort?>()
                .Deserialize(ref reader, Options));
        }
        Assert.True(reader.End);
    }

    [Fact]
    public void Typed_turn_events_round_trip()
    {
        var changeSet = new TurnChangeSet
        {
            PlayerState = new PlayerChange { ChangedProperties = null, Name = "Dudley" },
            Events =
            [
                new TurnEvent
                {
                    Type = TurnEventType.SteppedOnConnection,
                    Value1 = (int)ConnectionScope.Room,
                    Value2 = 123
                }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(
            changeSet, Options, TestContext.Current.CancellationToken);
        var result = MessagePackSerializer.Deserialize<TurnChangeSet>(
            bytes, Options, TestContext.Current.CancellationToken);
        var turnEvent = Assert.Single(result.Events!);

        Assert.Equal(TurnEventType.SteppedOnConnection, turnEvent.Type);
        Assert.Equal((int)ConnectionScope.Room, turnEvent.Value1);
        Assert.Equal(123, turnEvent.Value2);
    }

    [Fact]
    public void Change_dto_full_sparse_and_deleted_bytes_are_stable()
    {
        var lines = new List<string>();
        Add("Player", new PlayerChange { ChangedProperties = null }, new PlayerChange { Fortune = 7 });
        Add("Room", new RoomChange { ChangedProperties = null }, new RoomChange { GridY = 7 });
        Add("Actor", new ActorChange { ChangedProperties = null },
            new ActorChange
            {
                NextAction = new ActorActionChange { Type = Systems.Actors.ActorActionType.Wait }
            },
            new ActorChange { ChangedProperties = 0 });
        Add("Item", new ItemChange { ChangedProperties = null }, new ItemChange { IsCurrentlyPerceived = true },
            new ItemChange { ChangedProperties = 0 });
        Add("Connection", new ConnectionChange { ChangedProperties = null },
            new ConnectionChange { Orientation = Direction.East },
            new ConnectionChange { ChangedProperties = 0 });
        Add("Ability", new AbilityChange { ChangedProperties = null }, new AbilityChange { Noise = 7 },
            new AbilityChange { ChangedProperties = 0 });
        Add("Effect", new EffectChange { ChangedProperties = null }, new EffectChange { InForce = true },
            new EffectChange { ChangedProperties = 0 });
        Add("Log", new LogEntryChange { ChangedProperties = null }, new LogEntryChange { Ticks = 7 });
        Add("RoomMap", new RoomMapChanges { ChangedProperties = null },
            new RoomMapChanges { VisibleTerrainChanges = [(1, 2)] });

        Assert.Equal(
            [
                "Player.Full=DC0011C0A000000000C0C0C0C000000000000000",
                "Player.Sparse=92CE0001000107",
                "Room.Full=9CC0C0C0C0C0C0A00000000000",
                "Room.Sparse=92CD080107",
                "Actor.Full=9DC0000000A0A0C20000000000C0",
                "Actor.Sparse=92CD10019500A0000000",
                "Actor.Deleted=9100",
                "Item.Full=97C0000000A0A0C2",
                "Item.Sparse=9241C3",
                "Item.Deleted=9100",
                "Connection.Full=94C0000000",
                "Connection.Sparse=920900",
                "Connection.Deleted=9100",
                "Ability.Full=9FC0A000C0C0C0000000C00000C0C0C0",
                "Ability.Sparse=92CD400107",
                "Ability.Deleted=9100",
                "Effect.Full=95C0000000C2",
                "Effect.Sparse=9211C3",
                "Effect.Deleted=9100",
                "Log.Full=93C0A000",
                "Log.Sparse=920507",
                "RoomMap.Full=94C0C0C0C0",
                "RoomMap.Sparse=920991920102"
            ],
            lines);

        void Add<T>(string name, T full, T sparse, T? deleted = null)
            where T : class
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            lines.Add($"{name}.Full={Convert.ToHexString(MessagePackSerializer.Serialize(full, Options, cancellationToken))}");
            lines.Add($"{name}.Sparse={Convert.ToHexString(MessagePackSerializer.Serialize(sparse, Options, cancellationToken))}");
            if (deleted != null)
            {
                lines.Add($"{name}.Deleted={Convert.ToHexString(MessagePackSerializer.Serialize(deleted, Options, cancellationToken))}");
            }
        }
    }
}

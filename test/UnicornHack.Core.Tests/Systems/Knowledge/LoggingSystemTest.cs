using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Data.Items;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Time;

namespace UnicornHack.Systems.Knowledge;

public class LoggingSystemTest
{
    // TODO: Test explosion

    private const string PickupMap = @"
.....
.....
.....
.....
.....";

    [Fact]
    public void AttackEvent()
    {
        var level = TestHelper.BuildRoom(@"
...
...
...");
        var wizard = CreatureData.WizardOfYendor.Instantiate(level, new Point(0, 0));
        TestHelper.AddSpellAbility(wizard);

        var undine = CreatureData.Undine.Instantiate(level, new Point(0, 1));

        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        playerEntity.Position!.Heading = Direction.West;
        var manager = playerEntity.Manager;

        manager.Queue.ProcessQueue(manager);

        ItemData.LongSword.Instantiate(playerEntity);

        var player2Entity = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(2, 0));
        player2Entity.Position!.Heading = Direction.North;

        manager.Queue.ProcessQueue(manager);

        var swordEntity = playerEntity.Being!.Items
            .Single(e => e.Item!.TemplateName == ItemData.LongSword.Name);
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = playerEntity;
        equipMessage.ItemEntity = swordEntity;
        equipMessage.Slot = EquipmentSlot.GraspBothMelee;

        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();
        player2Entity.Player!.LogEntries.Clear();

        var wizardPunch = wizard.ActiveEffects!.Abilities
            .Single(a => a.Ability!.Action == AbilityAction.Punch);

        var activateMessage = ActivateAbilityMessage.Create(manager);
        activateMessage.AbilityEntity = wizardPunch;
        activateMessage.ActivatorEntity = wizard;
        activateMessage.TargetEntity = undine;

        var stats = manager.AbilityActivationSystem.GetAttackStats(activateMessage);
        manager.ReturnMessage(activateMessage);

        Assert.Equal(97, stats.SubAttacks
            .Aggregate(0,
                (sum, a) => sum + manager.EffectApplicationSystem.GetExpectedDamage(a.Effects, wizard, undine)));

        Verify(wizard, undine, playerEntity, player2Entity,
            wizardPunch, success: true,
            "The Wizard of Yendor punches the undine. (<damage 9='97'>97</damage> pts.)",
            "The Wizard of Yendor punches something.",
            manager);

        Verify(wizard, undine, playerEntity, player2Entity,
            wizardPunch, success: false,
            "The Wizard of Yendor tries to punch the undine, but misses.",
            "The Wizard of Yendor tries to punch something, but misses.",
            manager);

        Verify(playerEntity, wizard, playerEntity, player2Entity,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot], success: false,
            "You slash the Wizard of Yendor. (<damage 9='90'>90</damage> pts.)",
            "Dudley slashes the Wizard of Yendor. (<damage 9='90'>90</damage> pts.)",
            manager);

        Verify(playerEntity, wizard, playerEntity, player2Entity,
            playerEntity.Being.SlottedAbilities[AbilitySlottingSystem.DefaultMeleeAttackSlot], success: true,
            "You try to slash the Wizard of Yendor, but miss.",
            "Dudley tries to slash the Wizard of Yendor, but misses.",
            manager);

        var wizardSpell = wizard.ActiveEffects!.Abilities
            .Single(a => a.Ability!.Action == AbilityAction.Spell);
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

        VerifyCreatureAttack(
            CreatureData.Firefly, "fire sting", success: true,
            "The firefly stings you! [<damage 4='11'>11</damage> pts.]",
            "The firefly stings Dudley. (<damage 4='11'>11</damage> pts.)",
            "The firefly sets you on fire.",
            "The firefly sets Dudley on fire.");
        VerifyCreatureAttack(
            CreatureData.Undine, "undine ability #1", success: true,
            "The undine punches you! [<damage 9='238'>238</damage> pts.]",
            "The undine punches Dudley. (<damage 9='238'>238</damage> pts.)");
        VerifyCreatureAttack(
            CreatureData.GiantSpider, "web", success: false,
            "The giant spider tries to spit at you, but misses.",
            "The giant spider tries to spit at Dudley, but misses.");
        VerifyCreatureAttack(
            CreatureData.Anaconda, "anaconda ability #2", success: true,
            "The anaconda squeezes you! [<damage 9='70'>70</damage> pts.]",
            "The anaconda squeezes Dudley. (<damage 9='70'>70</damage> pts.)");
        VerifyCreatureAttack(
            CreatureData.Antlar, "antlar ability #1", success: true,
            "The antlar claws you! [<damage 9='98'>98</damage> pts.]",
            "The antlar claws Dudley. (<damage 9='98'>98</damage> pts.)");
        VerifyCreatureAttack(
            CreatureData.CentaurWarrior, "centaur warrior ability #2", success: true,
            "The centaur warrior kicks you! [<damage 9='56'>56</damage> pts.]",
            "The centaur warrior kicks Dudley. (<damage 9='56'>56</damage> pts.)");
        VerifyCreatureAttack(
            CreatureData.MyconidSentry, "myconid sentry ability #1", success: false,
            "The myconid sentry tries to scream at you, but misses.",
            "The myconid sentry tries to scream at Dudley, but misses.");

        VerifyRangedWeaponAttack(
            ItemData.Shortbow, EquipmentSlot.GraspBothRanged,
            "You hit the undine. (<damage 9='40'>40</damage> pts.)");
        VerifyRangedWeaponAttack(
            ItemData.ThrowingKnives, EquipmentSlot.GraspPrimaryRanged,
            "You hit the undine. (<damage 9='37'>37</damage> pts.)");
    }

    [Fact]
    public void Revealed_room_logs_a_creature_intercepting_an_attack()
    {
        var game = TestHelper.CreateGame(seed: 1u);
        game.Options.RevealRooms = true;
        var room = TestHelper.BuildRoom(".....", game);
        var attacker = CreatureData.RedDragon.Instantiate(room, new Point(0, 0));
        var interceptor = CreatureData.Firefly.Instantiate(room, new Point(2, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(4, 0));
        player.Position!.Heading = Direction.East;
        var manager = game.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            10000, nameof(BeingComponent.HitPointMaximum), "test health", interceptor);
        manager.Queue.ProcessQueue(manager);
        interceptor.Being!.HitPoints = interceptor.Being.HitPointMaximum;
        var interceptorHitPoints = interceptor.Being.HitPoints;
        var playerHitPoints = player.Being!.HitPoints;
        player.Player!.LogEntries.Clear();

        var breath = attacker.ActiveEffects!.Abilities.Single(
            ability => ability.Ability!.Name == "red dragon ability #1");
        breath.Ability!.Slot ??= 9;
        var activation = ActivateAbilityMessage.Create(manager);
        activation.AbilityEntity = breath;
        activation.ActivatorEntity = attacker;
        activation.TargetEntity = player;
        activation.TargetCell = player.Position.Cell;
        manager.Enqueue(activation);
        ((TestRandom)manager.Game.Random).EnqueueNextBool(false);
        manager.Queue.ProcessQueue(manager);

        Assert.True(interceptor.Being.HitPoints < interceptorHitPoints);
        Assert.Equal(playerHitPoints, player.Being.HitPoints);
        Assert.Contains(
            player.Player.LogEntries,
            entry => entry.Message.Contains("red dragon", StringComparison.OrdinalIgnoreCase)
                     && entry.Message.Contains("firefly", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Lingering_attack_effect_is_announced_before_its_later_damage()
    {
        var level = TestHelper.BuildRoom("..");
        var firefly = CreatureData.Firefly.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        RemovePhysicalDefense(player, manager);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            10000, nameof(BeingComponent.HitPointMaximum), "test health", player);
        manager.Queue.ProcessQueue(manager);
        player.Being!.HitPoints = player.Being.HitPointMaximum;
        player.Player!.LogEntries.Clear();

        var ability = firefly.ActiveEffects!.Abilities.Single(
            candidate => candidate.Ability!.Name == "fire sting");
        var activation = ActivateAbilityMessage.Create(manager);
        activation.AbilityEntity = ability;
        activation.ActivatorEntity = firefly;
        activation.TargetEntity = player;
        manager.Enqueue(activation);
        ((TestRandom)manager.Game.Random).EnqueueNextBool(false);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            [
                "The firefly stings you! [<damage 4='11'>11</damage> pts.]",
                "The firefly sets you on fire."
            ],
            player.Player.LogEntries.Select(entry => entry.Message));

        player.Player.LogEntries.Clear();
        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            player, TimeSystem.DefaultActionDelay);
        manager.Queue.ProcessQueue(manager);

        AssertLog(player, "You take <damage 4='11'>11</damage> damage.");
    }

    private static void VerifyRangedWeaponAttack(
        Item weaponTemplate, EquipmentSlot slot, string expectedMessage)
    {
        var level = TestHelper.BuildRoom("...");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var target = CreatureData.Undine.Instantiate(level, new Point(2, 0));
        var manager = player.Manager;
        weaponTemplate.Instantiate(player);
        manager.Queue.ProcessQueue(manager);

        var weapon = player.Being!.Items.Single(
            item => item.Item!.TemplateName == weaponTemplate.Name);
        var equipMessage = EquipItemMessage.Create(manager);
        equipMessage.ActorEntity = player;
        equipMessage.ItemEntity = weapon;
        equipMessage.Slot = slot;
        manager.Enqueue(equipMessage);
        manager.Queue.ProcessQueue(manager);

        RemovePhysicalDefense(target, manager);
        player.Player!.LogEntries.Clear();
        var attackName = slot == EquipmentSlot.GraspBothRanged
            ? AbilityData.TwoHandedRangedAttack.Name
            : AbilityData.OneHandedRangedAttack.Name;
        var attack = manager.AffectableAbilitiesIndex[(player.Id, attackName)]!;
        var activation = ActivateAbilityMessage.Create(manager);
        activation.AbilityEntity = attack;
        activation.ActivatorEntity = player;
        activation.TargetCell = target.Position!.Cell;
        manager.Enqueue(activation);
        ((TestRandom)manager.Game.Random).EnqueueNextBool(true);
        manager.Queue.ProcessQueue(manager);

        AssertLog(player, expectedMessage);
    }

    private static void VerifyCreatureAttack(
        Creature attackerTemplate, string abilityName, bool success,
        string expectedMessage, string expectedObserverMessage,
        string? expectedLingeringMessage = null, string? expectedObserverLingeringMessage = null)
    {
        var level = TestHelper.BuildRoom("...");
        var attacker = attackerTemplate.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(1, 0));
        var observer = PlayerRace.InstantiatePlayer("Cudley", Sex.Female, level, new Point(2, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        RemovePhysicalDefense(player, manager);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            10000, nameof(BeingComponent.HitPointMaximum), "test health", player);
        manager.Queue.ProcessQueue(manager);
        player.Being!.HitPoints = player.Being.HitPointMaximum;
        var ability = attacker.ActiveEffects!.Abilities.Single(
            candidate => candidate.Ability!.Name == abilityName);
        ability.Ability!.Slot ??= 9;
        Verify(
            attacker, player, player, observer, ability, success,
            expectedMessage, expectedObserverMessage, manager,
            expectedLingeringMessage: expectedLingeringMessage,
            expectedObserverLingeringMessage: expectedObserverLingeringMessage);
    }

    private static void RemovePhysicalDefense(GameEntity target, GameManager manager)
    {
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -target.Being!.Armor, nameof(BeingComponent.Armor), "test armor", target);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            -target.Being.PhysicalResistance, nameof(BeingComponent.PhysicalResistance),
            "test physical resistance", target);
        manager.Queue.ProcessQueue(manager);
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
        GameManager manager,
        bool enqueueOutcome = true,
        string? expectedLingeringMessage = null,
        string? expectedObserverLingeringMessage = null)
    {
        player1.Player!.LogEntries.Clear();
        player2.Player!.LogEntries.Clear();

        var activationMessage = ActivateAbilityMessage.Create(manager);
        activationMessage.AbilityEntity = ability;
        activationMessage.ActivatorEntity = attacker;
        activationMessage.TargetEntity = victim;
        ability.Ability!.CooldownTick = null;

        manager.Enqueue(activationMessage);

        if (enqueueOutcome)
        {
            ((TestRandom)manager.Game.Random).EnqueueNextBool(!success);
        }
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            expectedLingeringMessage == null
                ? [expectedMessage1]
                : [expectedMessage1, expectedLingeringMessage],
            player1.Player.LogEntries.Select(entry => entry.Message));
        player1.Player.LogEntries.Clear();
        Assert.Equal(
            expectedObserverLingeringMessage == null
                ? [expectedMessage2]
                : [expectedMessage2, expectedObserverLingeringMessage],
            player2.Player!.LogEntries.Select(entry => entry.Message));
        player2.Player.LogEntries.Clear();
    }

    [Fact]
    public void DeathEvent()
    {
        var level = TestHelper.BuildRoom();
        var blob = CreatureData.AcidBlob.Instantiate(level, new Point(0, 1));
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 2));
        player.Position!.Heading = Direction.North;
        var manager = level.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();

        DiedMessage.Enqueue(blob, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            ["The acid blob dies."],
            player.Player.LogEntries.Select(entry => entry.Message));

        player.Player.LogEntries.Clear();
        DiedMessage.Enqueue(player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            ["You die!"],
            player.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Status_taking_hold_is_logged()
    {
        var level = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var blob = CreatureData.AcidBlob.Instantiate(level, new Point(1, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();

        // A full-strength stun takes hold at once; the moment it does is announced to the afflicted player.
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stun, playerEntity, amount: 200));
        manager.Queue.ProcessQueue(manager);

        Assert.True(EffectApplicationSystem.GetAbilityLevel(playerEntity, "stunned") > 0);
        var message = Assert.Single(playerEntity.Player.LogEntries).Message;
        Assert.StartsWith("You are stunned!", message);

        playerEntity.Player.LogEntries.Clear();
        manager.EffectApplicationSystem.ApplyStatus(EffectApplicationContext.CreateStatus(
            EffectType.Stun, blob, amount: 200));
        manager.Queue.ProcessQueue(manager);

        AssertLog(playerEntity, "The acid blob is stunned.");
    }

    [Fact]
    public void Instant_damage_does_not_log_a_resource_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);
        playerEntity.Player!.LogEntries.Clear();

        EffectTestHelper.ApplyInstantDamageEffect(
            playerEntity, EffectType.PhysicalDamage, amount: 10, manager);

        Assert.Empty(playerEntity.Player.LogEntries);
    }

    [Fact]
    public void Damage_over_time_effects_are_logged_once_per_turn()
    {
        var level = TestHelper.BuildRoom("..");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", playerEntity);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.EnergyPointMaximum), "test", playerEntity);
        manager.Queue.ProcessQueue(manager);
        being.HitPoints = 1000;
        being.EnergyPoints = 1000;

        var effects = playerEntity.ActiveEffects!.PropertyEffects;
        AddDamageOverTimeEffect(EffectType.BleedDamage, 5);
        AddDamageOverTimeEffect(EffectType.FireDamage, 3);
        AddDamageOverTimeEffect(EffectType.FireDamage, 2);
        AddDamageOverTimeEffect(EffectType.ToxinDamage, 7);
        AddDamageOverTimeEffect(EffectType.AcidDamage, 3);
        AddDamageOverTimeEffect(EffectType.ShockDamage, 4);
        AddDamageOverTimeEffect(EffectType.Wither, 4);
        AddDamageOverTimeEffect(EffectType.DrainEnergy, 2);
        AddDamageOverTimeEffect(EffectType.Heal, 10, ValueCombinationFunction.Sum);
        AddDamageOverTimeEffect(EffectType.Recharge, 2, ValueCombinationFunction.Sum);
        AddDamageOverTimeEffect(EffectType.GainXP, 10, ValueCombinationFunction.Sum);
        being.HitPoints = 900;
        being.EnergyPoints = 900;
        playerEntity.Player!.LogEntries.Clear();

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            playerEntity, TimeSystem.DefaultActionDelay);
        manager.Queue.ProcessQueue(manager);

        var message = Assert.Single(playerEntity.Player.LogEntries).Message;
        Assert.Equal(
            "You take <damage 2='5' 4='5' 3='7' 1='3' 11='4'>24</damage> damage"
            + " and lose <damage 14='4' 5='2'>6</damage> energy"
            + " and restore 10 health and restore 2 energy and gain 10 experience.",
            message);

        void AddDamageOverTimeEffect(
            EffectType effectType, int amount,
            ValueCombinationFunction combinationFunction = ValueCombinationFunction.Subtract)
            => effects.Add(CreatePropertyEffect(playerEntity.ActiveEffects!,
                0, 0, effects.Count, effectType, targetName: null,
                combinationFunction: combinationFunction,
                magnitudePrimary: amount));
    }

    [Fact]
    public void Damage_over_time_to_another_actor_is_logged_in_the_third_person()
    {
        var level = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var blob = CreatureData.AcidBlob.Instantiate(level, new Point(1, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        blob.ActiveEffects!.PropertyEffects.Add(CreatePropertyEffect(blob.ActiveEffects,
            0, 0, blob.ActiveEffects.PropertyEffects.Count, EffectType.ToxinDamage, targetName: null,
            combinationFunction: ValueCombinationFunction.Subtract,
            magnitudePrimary: 7));
        player.Player!.LogEntries.Clear();

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            blob, TimeSystem.DefaultActionDelay);
        manager.Queue.ProcessQueue(manager);

        AssertLog(player, "The acid blob takes <damage 3='7'>7</damage> damage.");
    }

    [Fact]
    public void Instant_and_periodic_healing_are_logged()
    {
        var level = TestHelper.BuildRoom(".");
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var being = playerEntity.Being!;
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test", playerEntity);
        manager.Queue.ProcessQueue(manager);
        being.HitPoints = 1;
        playerEntity.Player!.LogEntries.Clear();

        using (var effectReference = manager.CreateEntity())
        {
            var effectEntity = effectReference.Referenced;
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "20";
            effectEntity.Effect = effect;

            TestHelper.ApplyEffect(effectEntity, playerEntity, playerEntity, manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(
            ["You restore 20 health."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));

        playerEntity.Player.LogEntries.Clear();
        playerEntity.ActiveEffects!.PropertyEffects.Add(CreatePropertyEffect(playerEntity.ActiveEffects,
            0, 0, playerEntity.ActiveEffects.PropertyEffects.Count, EffectType.Heal, null,
            magnitudePrimary: 20));

        manager.EffectApplicationSystem.ApplyLingeringResourceEffects(
            playerEntity, TimeSystem.DefaultActionDelay);

        Assert.Equal(
            ["You restore 20 health."],
            playerEntity.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Instant_inverted_healing_does_not_log_a_resource_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            1000, nameof(BeingComponent.HitPointMaximum), "test maximum", player);
        manager.EffectApplicationSystem.UpdateOrAddPropertyEffect(
            150, nameof(BeingComponent.HealingResistance), "test healing resistance", player);
        manager.Queue.ProcessQueue(manager);
        player.Being!.HitPoints = 100;
        player.Player!.LogEntries.Clear();

        using (var effectReference = manager.CreateEntity())
        {
            var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
            effect.EffectType = EffectType.Heal;
            effect.Duration = EffectDuration.Instant;
            effect.Amount = "20";
            effectReference.Referenced.Effect = effect;
            TestHelper.ApplyEffect(effectReference.Referenced, player, player, manager);
            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(90, player.Being.HitPoints);
        Assert.Empty(player.Player.LogEntries);
    }

    [Fact]
    public void Item_equipment_and_unequipment_are_logged()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        ItemData.LongSword.Instantiate(player);
        var manager = level.Entity.Manager;
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();
        var sword = player.Being!.Items.Single(item => item.Item!.TemplateName == ItemData.LongSword.Name);

        var equipped = EquipItemMessage.Create(manager);
        equipped.ActorEntity = player;
        equipped.ItemEntity = sword;
        equipped.Slot = EquipmentSlot.GraspPrimaryMelee;
        manager.Enqueue(equipped);
        manager.Queue.ProcessQueue(manager);
        AssertLog(player, "You equip the long sword in the main hand for melee.");

        var unequipped = EquipItemMessage.Create(manager);
        unequipped.ActorEntity = player;
        unequipped.ItemEntity = sword;
        unequipped.Slot = EquipmentSlot.None;
        manager.Enqueue(unequipped);
        manager.Queue.ProcessQueue(manager);
        AssertLog(player, "You unequip the long sword.");
    }

    [Fact]
    public void Npc_item_activation_is_logged()
    {
        var level = TestHelper.BuildRoom("..");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var blob = CreatureData.AcidBlob.Instantiate(level, new Point(1, 0));
        var manager = player.Manager;
        Assert.True(ItemData.FlaskOfHealing.Instantiate(blob));
        manager.Queue.ProcessQueue(manager);
        player.Player!.LogEntries.Clear();

        TestHelper.ActivateAbility(ItemData.FlaskOfHealing.Name + ": Drink", blob, manager);
        manager.Queue.ProcessQueue(manager);

        AssertLog(player, "The acid blob drinks from a flask of healing.");
    }

    [Fact]
    public void Consumed_item_ability_is_logged_without_an_activate_effect()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.True(ItemData.PotionOfExperience.Instantiate(player, 13));
        manager.Queue.ProcessQueue(manager);
        Assert.Contains(player.Being!.Items,
            item => item.Item!.TemplateName == ItemData.PotionOfExperience.Name);
        Assert.Contains(player.ActiveEffects!.Abilities,
            ability => ability.Ability!.Name == ItemData.PotionOfExperience.Name + ": Drink");
        player.Player!.LogEntries.Clear();

        TestHelper.ActivateAbility(ItemData.PotionOfExperience.Name + ": Drink", player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            ["You drink the potion of experience."],
            player.Player.LogEntries.Select(entry => entry.Message));
    }

    [Fact]
    public void Activate_target_resolution_overrides_attack_fallback_for_item_abilities()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        Assert.True(ItemData.FlaskOfHealing.Instantiate(player));
        manager.Queue.ProcessQueue(manager);
        var flask = player.Being!.Items.Single(
            item => item.Item!.TemplateName == ItemData.FlaskOfHealing.Name);
        flask.Item!.Abilities.Single(
            ability => ability.Ability!.Name == "Drink").Ability!.Action = AbilityAction.Slash;
        player.Being.HitPoints = player.Being.HitPointMaximum;
        player.Player!.LogEntries.Clear();

        TestHelper.ActivateAbility(ItemData.FlaskOfHealing.Name + ": Drink", player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(
            ["You drink from the flask of healing."],
            player.Player.LogEntries.Select(entry => entry.Message));
    }

    [Theory]
    [InlineData(AbilityAction.Default, 0)]
    [InlineData(AbilityAction.Spell, 1)]
    public void Change_property_does_not_classify_the_activation_result(
        AbilityAction action, int expectedLogCount)
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);

        using var abilityReference = manager.CreateEntity();
        var abilityEntity = abilityReference.Referenced;
        var ability = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
        ability.Name = "test property change";
        ability.Activation = ActivationType.Manual;
        ability.Action = action;
        ability.OwnerEntity = player;
        abilityEntity.Ability = ability;

        using var effectReference = manager.CreateEntity();
        var effect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
        effect.EffectType = EffectType.ChangeProperty;
        effect.Duration = EffectDuration.UntilTimeout;
        effect.DurationAmount = "100";
        effect.TargetName = nameof(BeingComponent.Armor);
        effect.Amount = "1";
        effect.ContainingAbilityId = abilityEntity.Id;
        effectReference.Referenced.Effect = effect;

        player.Player!.LogEntries.Clear();
        TestHelper.ApplyEffect(effectReference.Referenced, player, player, manager);
        manager.Queue.ProcessQueue(manager);

        Assert.Equal(expectedLogCount, player.Player.LogEntries.Count);
    }

    /// <summary>
    ///     Regression: an item picked up in a room the player can't perceive must not be
    ///     logged. The initial-cell visibility check used to index the player's own room
    ///     visibility map with a cell from the picker's room, spuriously reporting the item
    ///     as seen and logging "Something picks up X".
    /// </summary>
    [Fact]
    public void Pickup_in_another_room_is_not_logged()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var playerRoom = TestHelper.BuildRoom(PickupMap, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, playerRoom, new Point(0, 0));
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        // A second, separate room (different level/branch) where an NPC picks up an item.
        var otherRoom = TestHelper.BuildRoom(PickupMap, game, depth: 1);
        var npc = CreatureData.HumanWarrior.Instantiate(otherRoom, new Point(0, 0));
        ItemData.GoldCoin.Instantiate(otherRoom, new Point(0, 0));
        manager.Queue.ProcessQueue(manager);

        var goldEntity = otherRoom.Items[new Point(0, 0)];
        playerEntity.Player!.LogEntries.Clear();

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = goldEntity;
        moveMessage.TargetContainerEntity = npc;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        Assert.Empty(playerEntity.Player.LogEntries);
    }

    /// <summary>
    ///     Guards against over-suppression: a pickup the player can perceive (same room, in
    ///     view) is still logged.
    /// </summary>
    [Fact]
    public void Pickup_in_view_is_logged()
    {
        var game = TestHelper.CreateGame(seed: 1u);

        var room = TestHelper.BuildRoom(PickupMap, game);
        var playerEntity = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, room, new Point(2, 2));
        playerEntity.Position!.Heading = Direction.North;
        var manager = playerEntity.Manager;
        manager.Queue.ProcessQueue(manager);

        var npc = CreatureData.AcidBlob.Instantiate(room, new Point(2, 1));
        ItemData.GoldCoin.Instantiate(room, new Point(2, 1), quantity: 11);
        manager.Queue.ProcessQueue(manager);

        var goldEntity = room.Items[new Point(2, 1)];

        // Sanity: the player actually perceives the pickup cell, so a logged message is expected.
        Assert.NotEqual(SenseType.None, manager.SensorySystem.CanSense(playerEntity, npc));

        playerEntity.Player!.LogEntries.Clear();

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = goldEntity;
        moveMessage.TargetContainerEntity = npc;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        AssertLog(playerEntity, "The acid blob picks up 11 gold coins.");

        moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = goldEntity;
        moveMessage.TargetRoomEntity = room.Entity;
        moveMessage.TargetCell = npc.Position!.Cell;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);

        AssertLog(playerEntity, "The acid blob drops 11 gold coins.");
    }

    [Fact]
    public void Player_item_pickup_and_drop_are_logged()
    {
        var level = TestHelper.BuildRoom(".");
        var player = PlayerRace.InstantiatePlayer("Dudley", Sex.Male, level, new Point(0, 0));
        var manager = level.Entity.Manager;
        Assert.True(ItemData.GoldCoin.Instantiate(level, player.Position!.Cell!.Value, quantity: 11));
        manager.Queue.ProcessQueue(manager);
        var coins = level.Items[player.Position.Cell.Value];
        player.Player!.LogEntries.Clear();

        var moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = coins;
        moveMessage.TargetContainerEntity = player;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);
        AssertLog(player, "You pick up 11 gold coins.");

        moveMessage = MoveItemMessage.Create(manager);
        moveMessage.ItemEntity = coins;
        moveMessage.TargetRoomEntity = level.Entity;
        moveMessage.TargetCell = player.Position.Cell;
        manager.Enqueue(moveMessage);
        manager.Queue.ProcessQueue(manager);
        AssertLog(player, "You drop 11 gold coins.");
    }

    private static void AssertLog(GameEntity playerEntity, string expectedMessage)
    {
        Assert.Equal(
            [expectedMessage],
            playerEntity.Player!.LogEntries.Select(entry => entry.Message));
        playerEntity.Player.LogEntries.Clear();
    }
}

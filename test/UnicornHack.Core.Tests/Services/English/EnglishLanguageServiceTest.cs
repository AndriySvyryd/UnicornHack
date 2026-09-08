using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnicornHack.Data.Abilities;
using UnicornHack.Data.Creatures;
using UnicornHack.Generation.Effects;
using UnicornHack.Services.English;
using UnicornHack.Services.LogEvents;
using UnicornHack.Systems.Beings;

namespace UnicornHack.Services;

public class EnglishLanguageServiceTest
{
    [Fact]
    public void Every_property_description_has_a_tooltip()
    {
        var language = new EnglishLanguageService();
        foreach (var property in PropertyDescription.Loader.GetAll())
        {
            var description = language.GetDescription(
                property.Name, DescriptionCategory.Property);

            Assert.False(
                string.IsNullOrEmpty(description),
                $"Property '{property.Name}' has no tooltip description.");
        }
    }

    [Fact]
    public void Every_effect_type_has_a_tooltip_and_valid_status_ability()
    {
        var language = new EnglishLanguageService();
        foreach (var effectType in Enum.GetValues<EffectType>().Where(type => type != EffectType.Default))
        {
            var description = language.GetDescription(
                effectType.ToString(), DescriptionCategory.Effect);
            Assert.False(
                string.IsNullOrWhiteSpace(description),
                $"Effect '{effectType}' has no player-facing tooltip description.");

            var metadata = Effect.GetMetadata(effectType);
            if (metadata.StatusAbilityName is not { } abilityName)
            {
                continue;
            }

            var statusAbility = Ability.Loader.Find(abilityName);
            Assert.NotNull(statusAbility);
            Assert.True(
                statusAbility.Type.IsStatusLike(),
                $"Effect '{effectType}' references non-status ability '{abilityName}'.");
        }
    }

    [Fact]
    public void Every_lingering_resource_effect_has_a_custom_application_message()
    {
        var expectedMessages = new Dictionary<EffectType, string>
        {
            [EffectType.AcidDamage] = "The firefly coats you in acid.",
            [EffectType.BleedDamage] = "The firefly lacerates you.",
            [EffectType.ToxinDamage] = "The firefly poisons you.",
            [EffectType.FireDamage] = "The firefly sets you on fire.",
            [EffectType.DrainEnergy] = "The firefly starts draining energy from you.",
            [EffectType.DrainLife] = "The firefly starts draining life from you.",
            [EffectType.ColdDamage] = "The firefly wraps you in frost.",
            [EffectType.LightDamage] = "The firefly surrounds you with radiance.",
            [EffectType.PhysicalDamage] = "The firefly crushes you.",
            [EffectType.PsychicDamage] = "The firefly fills you with anguish.",
            [EffectType.ShockDamage] = "The firefly shocks you.",
            [EffectType.WaterDamage] = "The firefly soaks you.",
            [EffectType.SonicDamage] = "The firefly rattles you.",
            [EffectType.Wither] = "The firefly causes you to wither.",
            [EffectType.GainXP] = "The firefly awakens new insight in you.",
            [EffectType.Heal] = "The firefly restores you.",
            [EffectType.Recharge] = "The firefly invigorates you."
        };
        var resourceEffects = Enum.GetValues<EffectType>()
            .Where(effectType => effectType != EffectType.Default
                                 && Effect.GetMetadata(effectType).ResourceProperty != null)
            .ToHashSet();
        Assert.Equal(expectedMessages.Keys.ToHashSet(), resourceEffects);

        var level = TestHelper.BuildRoom("..");
        var firefly = CreatureData.Firefly.Instantiate(level, new Point(0, 0));
        var player = PlayerRace.InstantiatePlayer(
            "Dudley", Sex.Male, level, new Point(1, 0));
        var manager = player.Manager;
        manager.Queue.ProcessQueue(manager);
        var language = new EnglishLanguageService();

        foreach (var (effectType, expectedMessage) in expectedMessages)
        {
            var message = language.GetString(new LingeringAttackEvent(
                player, firefly, player, SenseType.Sight, SenseType.Sight,
                new Dictionary<EffectType, int> { [effectType] = 1 }));
            Assert.Equal(expectedMessage, message);
        }

        var fallbackMessage = language.GetString(new LingeringAttackEvent(
            player, firefly, player, SenseType.Sight, SenseType.Sight,
            new Dictionary<EffectType, int> { [EffectType.Stun] = 1 }));
        Assert.Equal("The firefly applies continuous stun to you.", fallbackMessage);
    }

    [Fact]
    public void Every_reflected_component_preserves_public_properties_for_native_aot()
    {
        var loaderField = typeof(PropertyDescription).GetField(nameof(PropertyDescription.Loader))!;
        var rootedTypes = loaderField.GetCustomAttributes<DynamicDependencyAttribute>()
            .Where(attribute => (attribute.MemberTypes & DynamicallyAccessedMemberTypes.PublicProperties) != 0)
            .Select(attribute => attribute.Type)
            .ToHashSet();
        var componentTypes = typeof(GameComponent).Assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<ComponentAttribute>(inherit: true) != null
                           && type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                               .Any(property => property.GetCustomAttribute<PropertyAttribute>(inherit: true) != null));
        var loadedProperties = PropertyDescription.Loader.GetAll()
            .Select(property => (property.ComponentId, property.Name))
            .ToHashSet();

        foreach (var componentType in componentTypes)
        {
            Assert.Contains(componentType, rootedTypes);
            var componentId = componentType.GetCustomAttribute<ComponentAttribute>(inherit: true)!.Id;
            foreach (var property in componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(property => property.GetCustomAttribute<PropertyAttribute>(inherit: true) != null))
            {
                Assert.Contains((componentId, property.Name), loadedProperties);
            }
        }
    }

    [Fact]
    public void Status_descriptions_include_annotated_level_effects()
    {
        var description = new EnglishLanguageService().GetDescription(
            AbilityData.Chilled.Name, DescriptionCategory.Ability);

        Assert.Contains("Level 1:", description);
        Assert.Contains("Level 3:", description);
        Assert.DoesNotContain("includes earlier levels", description);
        Assert.Contains("<tooltip id='Speed'", description);
        Assert.Contains("</tooltip> -2", description);
        Assert.Contains("<tooltip id='Hindrance'", description);
        Assert.Contains("</tooltip> +30", description);
    }

    [Fact]
    public void Warded_description_annotates_status_level_suppression()
    {
        var description = new EnglishLanguageService().GetDescription(
            AbilityData.Warded.Name, DescriptionCategory.Ability);

        Assert.Contains("Level 1:", description);
        Assert.Contains("each Debuff ability level -1", description);
        Assert.Contains("Level 3:", description);
        Assert.DoesNotContain("includes earlier levels", description);
    }

    [Fact]
    public void Non_cumulative_ability_description_marks_replacement_levels()
    {
        var description = new EnglishLanguageService().GetDescription(
            AbilityData.Unstable.Name, DescriptionCategory.Ability);

        Assert.Contains("Level 1:", description);
        Assert.Contains("Level 2 (replaces earlier levels):", description);
    }

    [Fact]
    public void Resistance_descriptions_link_to_their_effects()
    {
        var language = new EnglishLanguageService();

        var acid = language.GetDescription(
            nameof(BeingComponent.AcidResistance), DescriptionCategory.Property);
        Assert.Contains("<tooltip id='AcidDamage' category='5'>acid damage</tooltip>", acid);
        Assert.Contains("<tooltip id='Corrode' category='5'>corrosion</tooltip>", acid);

        var disorient = language.GetDescription(
            nameof(BeingComponent.DisorientResistance), DescriptionCategory.Property);
        Assert.Contains("<tooltip id='Confuse' category='5'>being confused</tooltip>", disorient);
    }

    [Fact]
    public void Status_specific_behaviors_are_described_without_synthetic_properties()
    {
        var description = new EnglishLanguageService().GetDescription(
            AbilityData.Frightened.Name, DescriptionCategory.Ability);

        Assert.Contains("Level 1:", description);
        Assert.Contains("Accuracy against the source is halved", description);
        Assert.Contains("Cannot move closer to a perceived source", description);
        Assert.Contains("Must make a random legal escape move away from the source", description);

        description = new EnglishLanguageService().GetDescription(
            AbilityData.Taunted.Name, DescriptionCategory.Ability);
        Assert.Contains("Cannot move farther from a perceived source", description);
        Assert.Contains("Attacks are redirected to the source", description);
        Assert.Contains("Movement must approach the source and non-offensive actions are spent", description);

        description = new EnglishLanguageService().GetDescription(
            AbilityData.Muted.Name, DescriptionCategory.Ability);
        Assert.Contains("Spell energy costs are doubled", description);
        Assert.Contains("Verbal and divine ability energy costs are doubled", description);
    }

    [Fact]
    public void Welcome_includes_the_branch_and_player_name()
    {
        var level = TestHelper.BuildRoom();
        var player = PlayerRace.InstantiatePlayer(
            "Conan the Barbarian", Sex.Male, level, new Point(0, 0));

        var message = level.Entity.Manager.Game.Services.Language.Welcome(player);

        Assert.Equal($"Welcome to the {level.Level.BranchName}, Conan the Barbarian!", message);
    }

}
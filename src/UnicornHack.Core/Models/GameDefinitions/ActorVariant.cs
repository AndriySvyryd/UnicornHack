using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnicornHack.Utils;
using static UnicornHack.Models.GameDefinitions.ActorNoiseType;
using static UnicornHack.Models.GameDefinitions.AttackEffect;
using static UnicornHack.Models.GameDefinitions.AttackType;
using static UnicornHack.Models.GameDefinitions.Frequency;
using static UnicornHack.Models.GameDefinitions.GenerationFlags;
using static UnicornHack.Models.GameDefinitions.MonsterBehavior;
using static UnicornHack.Models.GameDefinitions.SimpleActorPropertyType;
using static UnicornHack.Models.GameDefinitions.Size;
using static UnicornHack.Models.GameDefinitions.Species;
using static UnicornHack.Models.GameDefinitions.SpeciesClass;
using static UnicornHack.Models.GameDefinitions.ValuedActorPropertyType;

namespace UnicornHack.Models.GameDefinitions
{
    public class ActorVariant
    {
        // TODO: pregenerate the indices
        private static readonly Dictionary<string, ActorVariant> NameLookup =
            new Dictionary<string, ActorVariant>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<Species, List<ActorVariant>> SpeciesLookup =
            new Dictionary<Species, List<ActorVariant>>();

        private static readonly Dictionary<SpeciesClass, List<ActorVariant>> SpeciesCategoryLookup
            = new Dictionary<SpeciesClass, List<ActorVariant>>();

        public static readonly ActorVariant None = new ActorVariant(
            name: "NONE", species: Species.Human,
            initialLevel: 1, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new Attack[0], properties: null,
            size: Medium, weight: 0, nutrition: 0, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly ActorVariant Human = new ActorVariant(
            name: "human", species: Species.Human, noise: Speach,
            initialLevel: 1, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Hobbit = new ActorVariant(
            name: "hobbit", species: Species.Hobbit, noise: Speach,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 10, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 500, nutrition: 250,
            behavior: AlignmentAware | WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Commonly);

        public static readonly ActorVariant Dwarf = new ActorVariant(
            name: "dwarf", species: Species.Dwarf, noise: Speach,
            initialLevel: 2, movementRate: 6, armorClass: 10, magicResistance: 10, alignment: 4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Sometimes);

        public static readonly ActorVariant DwarfLord = new ActorVariant(
            name: "dwarf lord", species: Species.Dwarf, noise: Speach, previousStage: Dwarf,
            initialLevel: 4, movementRate: 6, armorClass: 10, magicResistance: 10, alignment: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly ActorVariant DwarfKing = new ActorVariant(
            name: "dwarf king", species: Species.Dwarf, noise: Speach, previousStage: DwarfLord,
            initialLevel: 6, movementRate: 6, armorClass: 10, magicResistance: 20, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable | Entourage, generationFrequency: Rarely);

        public static readonly ActorVariant Elf = new ActorVariant(
            name: "elf", species: Species.Elf, noise: Speach,
            initialLevel: 4, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant ElfWoodland = new ActorVariant(
            name: "woodland-elf", species: Species.Elf, noise: Speach,
            initialLevel: 4, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant ElfGreen = new ActorVariant(
            name: "green-elf", species: Species.Elf, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant ElfGrey = new ActorVariant(
            name: "grey-elf", species: Species.Elf, noise: Speach,
            initialLevel: 6, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant ElfLord = new ActorVariant(
            name: "elf-lord", species: Species.Elf, noise: Speach,
            initialLevel: 8, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant ElfKing = new ActorVariant(
            name: "Elvenking", species: Species.Elf, noise: Speach,
            initialLevel: 9, movementRate: 12, armorClass: 10, magicResistance: 25, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: Entourage, generationFrequency: Rarely);

        public static readonly ActorVariant ElfDrow = new ActorVariant(
            name: "drow", species: Species.Elf, noise: Speach,
            initialLevel: 6, movementRate: 12, armorClass: 10, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Touch, Sleep, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant ElfDrowWarrior = new ActorVariant(
            name: "drow warrior", species: Species.Elf, noise: Speach,
            initialLevel: 7, movementRate: 12, armorClass: 10, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Touch, Sleep, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant Bugbear = new ActorVariant(
            name: "bugbear", species: Species.Goblin, noise: Growl,
            initialLevel: 3, movementRate: 9, armorClass: 5, magicResistance: 0, alignment: -6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1250, nutrition: 250,
            behavior: WeaponCollector, generationFrequency: Commonly);

        public static readonly ActorVariant Goblin = new ActorVariant(
            name: "goblin", species: Species.Goblin, noise: Grunt,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 400, nutrition: 100,
            behavior: WeaponCollector, generationFrequency: Commonly);

        public static readonly ActorVariant Hobgoblin = new ActorVariant(
            name: "hobgoblin", species: Species.Goblin, noise: Grunt,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFrequency: Commonly);

        public static readonly ActorVariant Orc = new ActorVariant(
            name: "orc", species: Species.Orc, noise: Grunt,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant OrcHill = new ActorVariant(
            name: "hill orc", species: Species.Orc, noise: Grunt,
            initialLevel: 2, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly ActorVariant OrcMordor = new ActorVariant(
            name: "Mordor orc", species: Species.Orc, noise: Grunt,
            initialLevel: 3, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1100, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly ActorVariant OrcShaman = new ActorVariant(
            name: "orc shaman", species: Species.Orc, noise: Grunt,
            initialLevel: 3, movementRate: 9, armorClass: 10, magicResistance: 10, alignment: -5,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: MagicUser | GoldCollector, generationFrequency: Commonly);

        public static readonly ActorVariant OrcUruk = new ActorVariant(
            name: "uruk-hai", species: Species.Orc, noise: Grunt,
            initialLevel: 4, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1300, nutrition: 300,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly ActorVariant OrcCaptain = new ActorVariant(
            name: "orc captain", species: Species.Orc, noise: Grunt, previousStage: Orc,
            initialLevel: 5, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1350, nutrition: 350,
            behavior: WeaponCollector | GoldCollector, generationFlags: Entourage, generationFrequency: Sometimes);

        public static readonly ActorVariant KoboldMedium = new ActorVariant(
            name: "kobold", species: Kobold, noise: Grunt,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            properties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 400, nutrition: 100,
            behavior: WeaponCollector, generationFrequency: Often);

        public static readonly ActorVariant KoboldLarge = new ActorVariant(
            name: "large kobold", species: Kobold, noise: Grunt, previousStage: KoboldMedium,
            initialLevel: 2, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            properties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 450, nutrition: 150,
            behavior: WeaponCollector, generationFrequency: Usually);

        public static readonly ActorVariant KoboldLord = new ActorVariant(
            name: "kobold lord", species: Kobold, noise: Grunt, previousStage: KoboldLarge,
            initialLevel: 3, movementRate: 6, armorClass: 6, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            properties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Small, weight: 500, nutrition: 200,
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant KoboldShaman = new ActorVariant(
            name: "kobold shaman", species: Kobold, noise: Grunt,
            initialLevel: 3, movementRate: 6, armorClass: 8, magicResistance: 10, alignment: -4,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            properties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Small, weight: 450, nutrition: 150,
            behavior: WeaponCollector | MagicUser, generationFrequency: Sometimes);

        public static readonly ActorVariant MindFlayer = new ActorVariant(
            name: "mind flayer", species: Illithid, noise: Gurgle,
            initialLevel: 9, movementRate: 12, armorClass: 5, magicResistance: 80, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                Levitation, InvisibilityDetection, Telepathy, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1200, nutrition: 300,
            behavior: GemCollector | GoldCollector | WeaponCollector, generationFrequency: Commonly);

        public static readonly ActorVariant MindFlayerMaster = new ActorVariant(
            name: "master mind flayer", species: Illithid, noise: Gurgle,
            previousStage: MindFlayer,
            initialLevel: 13, movementRate: 12, armorClass: 0, magicResistance: 90, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                Levitation, InvisibilityDetection, Telepathy, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1200, nutrition: 300,
            behavior: GemCollector | GoldCollector | WeaponCollector, generationFrequency: Commonly);

        public static readonly ActorVariant AntGiant = new ActorVariant(
            name: "giant ant", species: Ant, speciesClass: Insect,
            initialLevel: 2, movementRate: 18, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(AnimalBody, Stealthiness, Handlessness, Carnivorism, Asexuality),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: null,
            generationFlags: SmallGroup, generationFrequency: Commonly, behavior: NonWandering);

        public static readonly ActorVariant AntSoldier = new ActorVariant(
            name: "soldier ant", species: Ant, speciesClass: Insect,
            initialLevel: 3, movementRate: 18, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Sting, VenomDamage, diceCount: 3, diceSides: 4, frequency: Sometimes),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(PoisonResistance, AnimalBody, Stealthiness, Handlessness, Carnivorism, Asexuality),
            size: Tiny, weight: 20, nutrition: 5,
            generationFlags: SmallGroup, generationFrequency: Commonly, behavior: NonWandering);

        public static readonly ActorVariant AntFire = new ActorVariant(
            name: "fire ant", species: Ant, speciesClass: Insect,
            initialLevel: 3, movementRate: 18, armorClass: 3, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, FireDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, FireDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                FireResistance, Stealthiness, Infravisibility, SlimingResistance, AnimalBody, Handlessness, Carnivorism,
                Asexuality),
            size: Tiny, weight: 30, nutrition: 10,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Occasionally)},
            generationFlags: SmallGroup, generationFrequency: Commonly, behavior: NonWandering);

        public static readonly ActorVariant AntQueen = new ActorVariant(
            name: "ant queen", species: Ant, speciesClass: Insect,
            initialLevel: 9, movementRate: 18, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 9)
            },
            properties: Has(
                PoisonResistance, AnimalBody, Stealthiness, Handlessness, Carnivorism, Femaleness, Oviparity),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: null,
            generationFlags: Entourage, generationFrequency: Rarely, behavior: NonWandering);

        public static readonly ActorVariant BeeKiller = new ActorVariant(
            name: "killer bee", species: Bee, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 18, armorClass: -1, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(PoisonResistance, Flight, AnimalBody, Handlessness, Femaleness),
            size: Tiny, weight: 5, nutrition: 5,
            generationFlags: LargeGroup, generationFrequency: Commonly, behavior: NonWandering);

        public static readonly ActorVariant BeeQueen = new ActorVariant(
            name: "queen bee", species: Bee, speciesClass: Insect, noise: Buzz,
            initialLevel: 9, movementRate: 24, armorClass: -4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 15)
            },
            properties: Has(PoisonResistance, Flight, AnimalBody, Handlessness, Femaleness),
            size: Tiny, weight: 5, nutrition: 5,
            generationFlags: Entourage, generationFrequency: Rarely, behavior: NonWandering);

        public static readonly ActorVariant BeetleGiant = new ActorVariant(
            name: "giant beetle", species: Beetle, speciesClass: Insect,
            initialLevel: 5, movementRate: 6, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(PoisonResistance, AnimalBody, Handlessness, Carnivorism),
            size: Small, weight: 10, nutrition: 10,
            generationFrequency: Sometimes);

        public static readonly ActorVariant SpiderCave = new ActorVariant(
            name: "cave spider", species: Spider, speciesClass: Insect,
            initialLevel: 1, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Concealment, Clinginess, AnimalBody, Handlessness, Carnivorism,
                Oviparity),
            size: Tiny, weight: 50, nutrition: 25,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly ActorVariant SpiderGiant = new ActorVariant(
            name: "giant spider", species: Spider, speciesClass: Insect,
            initialLevel: 5, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, DrainStrength, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Clinginess, AnimalBody, Handlessness, Carnivorism, Oviparity),
            size: Medium, weight: 150, nutrition: 50,
            generationFrequency: Usually);

        // TODO: add more spiders

        public static readonly ActorVariant Centipede = new ActorVariant(
            name: "centipede", species: Species.Centipede, speciesClass: Insect,
            initialLevel: 2, movementRate: 4, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Concealment, Clinginess, AnimalBody, Handlessness, Carnivorism,
                Oviparity),
            size: Tiny, weight: 50, nutrition: 25,
            generationFrequency: Usually);

        public static readonly ActorVariant ScorpionLarge = new ActorVariant(
            name: "large scorpion", species: Scorpion, speciesClass: Insect,
            initialLevel: 5, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Concealment, AnimalBody, Handlessness, Carnivorism, Oviparity),
            size: Small, weight: 150, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Sometimes)},
            generationFrequency: Commonly);

        public static readonly ActorVariant BugLighting = new ActorVariant(
            name: "lightning bug", species: Beetle, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, ElectricityDamage, diceCount: 1, diceSides: 1),
                new Attack(OnConsumption, ElectricityDamage, diceCount: 1, diceSides: 1)
            },
            properties: Has(ElectricityResistance, Flight, AnimalBody, Handlessness, Herbivorism),
            size: Tiny, weight: 10, nutrition: 10,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant FireFly = new ActorVariant(
            name: "firefly", species: Beetle, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, FireDamage, diceCount: 1, diceSides: 1),
                new Attack(OnConsumption, FireDamage, diceCount: 1, diceSides: 1)
            },
            properties: Has(Flight, Infravisibility, AnimalBody, Handlessness, Herbivorism),
            size: Tiny, weight: 10, nutrition: 10,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant Xan = new ActorVariant(
            name: "xan", species: Beetle, speciesClass: Insect, noise: Buzz,
            initialLevel: 7, movementRate: 18, armorClass: -2, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Sting, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Sting, DamageLeg),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(PoisonResistance, Flight, AnimalBody, Handlessness),
            size: Tiny, weight: 1, nutrition: 1,
            generationFrequency: Sometimes);

        public static readonly ActorVariant WormLongBaby = new ActorVariant(
            name: "baby long worm", species: Worm,
            initialLevel: 2, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties:
                Has(PoisonResistance, SerpentlikeBody, Stealthiness, Eyelessness, Limblessness, Carnivorism,
                    NoInventory),
            size: Medium, weight: 600, nutrition: 250,
            generationFrequency: Commonly);

        public static readonly ActorVariant WormLong = new ActorVariant(
            name: "long worm", species: Worm,
            initialLevel: 9, movementRate: 3, armorClass: 5, magicResistance: 10, previousStage: WormLongBaby,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties:
                Has(PoisonResistance, SerpentlikeBody, Eyelessness, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Gigantic, weight: 1500, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly ActorVariant WormPurpleBaby = new ActorVariant(
            name: "baby purple worm", species: Worm,
            initialLevel: 4, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties:
                Has(PoisonResistance, SerpentlikeBody, Stealthiness, Eyelessness, Limblessness, Carnivorism,
                    NoInventory),
            size: Medium, weight: 600, nutrition: 250,
            generationFrequency: Commonly);

        public static readonly ActorVariant WormPurple = new ActorVariant(
            name: "purple worm", species: Worm,
            initialLevel: 15, movementRate: 9, armorClass: 5, magicResistance: 20, previousStage: WormPurpleBaby,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, Engulf, diceCount: 2, diceSides: 6),
                new Attack(Digestion, AcidDamage, diceCount: 1, diceSides: 10)
            },
            properties:
                Has(PoisonResistance, SerpentlikeBody, Eyelessness, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Gigantic, weight: 1500, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly ActorVariant Bat = new ActorVariant(
            name: "bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek,
            initialLevel: 1, movementRate: 22, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties:
                Has(PoisonResistance, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness, Carnivorism),
            size: Tiny, weight: 50, nutrition: 20,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly ActorVariant BatGiant = new ActorVariant(
            name: "giant bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek, previousStage: Bat,
            initialLevel: 2, movementRate: 22, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 5)
            },
            properties:
                Has(PoisonResistance, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness, Carnivorism),
            size: Tiny, weight: 100, nutrition: 40,
            generationFrequency: Commonly);

        public static readonly ActorVariant BatVampire = new ActorVariant(
            name: "vampire bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek, previousStage: BatGiant,
            initialLevel: 5, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, DrainStrength, diceCount: 1, diceSides: 1)
            },
            properties:
                Has(PoisonResistance, Regeneration, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness,
                    Carnivorism),
            size: Tiny, weight: 100, nutrition: 40,
            generationFrequency: Commonly);

        public static readonly ActorVariant Magpie = new ActorVariant(
            name: "magpie", species: Crow, speciesClass: Bird, noise: Squawk,
            initialLevel: 2, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(PoisonResistance, Flight, Infravisibility, AnimalBody, Handlessness, Omnivorism),
            size: Tiny, weight: 50, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            behavior: GemCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant Raven = new ActorVariant(
            name: "raven", species: Crow, speciesClass: Bird, noise: Squawk,
            initialLevel: 4, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, Blind, diceCount: 3, diceSides: 8)
            },
            properties: Has(PoisonResistance, Flight, Infravisibility, AnimalBody, Handlessness, Omnivorism),
            size: Tiny, weight: 100, nutrition: 40,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            generationFrequency: Sometimes);

        public static readonly ActorVariant Fox = new ActorVariant(
            name: "fox", species: Species.Fox, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 15, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 300, nutrition: 250,
            generationFrequency: Often);

        public static readonly ActorVariant Coyote = new ActorVariant(
            name: "coyote", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 300, nutrition: 250,
            generationFrequency: Usually);

        public static readonly ActorVariant Jackal = new ActorVariant(
            name: "jackal", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 300, nutrition: 250,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly ActorVariant Jackalwere = new ActorVariant(
            name: "jackalwere", species: Dog, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 2, movementRate: 12, armorClass: 7, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties:
                Has(PoisonResistance, AnimalBody, Regeneration, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 300, nutrition: 250, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Werejackal = new ActorVariant(
            name: "werejackal", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 2, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, Jackalwere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant DogSmall = new ActorVariant(
            name: "little dog", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 150, nutrition: 100,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant DogMedium = new ActorVariant(
            name: "dog", species: Dog, speciesClass: Canine, noise: Bark, previousStage: DogSmall,
            initialLevel: 4, movementRate: 16, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 300,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant DogLarge = new ActorVariant(
            name: "large dog", species: Dog, speciesClass: Canine, noise: Bark, previousStage: DogMedium,
            initialLevel: 6, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 600, nutrition: 400,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant Dingo = new ActorVariant(
            name: "dingo", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 4, movementRate: 16, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 200,
            generationFrequency: Usually);

        public static readonly ActorVariant Barghest = new ActorVariant(
            name: "barghest", species: Dog, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 9, movementRate: 16, armorClass: 2, magicResistance: 20, alignment: -6,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 1200, nutrition: 500,
            behavior: Mountable | AlignmentAware, generationFrequency: Uncommonly);

        public static readonly ActorVariant HellHoundCub = new ActorVariant(
            name: "hell hound pup", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Breath, FireDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 250, nutrition: 200,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Commonly)},
            generationFlags: HellOnly | SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant HellHound = new ActorVariant(
            name: "hell hound", species: Dog, speciesClass: Canine, noise: Bark, previousStage: HellHoundCub,
            initialLevel: 12, movementRate: 14, armorClass: 2, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Breath, FireDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 700, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Commonly)},
            generationFlags: HellOnly, generationFrequency: Usually);

        public static readonly ActorVariant Cerberus = new ActorVariant(
            name: "Cerberus", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 13, movementRate: 10, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, Maleness),
            size: Large, weight: 1000, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Often)},
            generationFlags: HellOnly | NonPolymorphable, generationFrequency: Once);

        public static readonly ActorVariant Wolf = new ActorVariant(
            name: "wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 250,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly ActorVariant WolfDire = new ActorVariant(
            name: "dire wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 7, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 1200, nutrition: 500,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant Warg = new ActorVariant(
            name: "warg", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 8, movementRate: 12, armorClass: 3, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Large, weight: 1400, nutrition: 600,
            behavior: Mountable, generationFrequency: Commonly);

        public static readonly ActorVariant Wolfwere = new ActorVariant(
            name: "wolfwere", species: Species.Wolf, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties:
                Has(PoisonResistance, Regeneration, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 250, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Werewolf = new ActorVariant(
            name: "werewolf", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, Wolfwere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant WolfWinterCub = new ActorVariant(
            name: "winter wolf cub", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Breath, ColdDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(ColdResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 250, nutrition: 200,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly ActorVariant WolfWinter = new ActorVariant(
            name: "winter wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark, previousStage: WolfWinterCub,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Breath, ColdDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(ColdResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 700, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly ActorVariant CatSmall = new ActorVariant(
            name: "kitten", species: Cat, speciesClass: Feline, noise: Mew,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 150, nutrition: 100,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant CatMedium = new ActorVariant(
            name: "housecat", species: Cat, speciesClass: Feline, noise: Mew, previousStage: CatSmall,
            initialLevel: 4, movementRate: 16, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 200, nutrition: 150,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant CatLarge = new ActorVariant(
            name: "large cat", species: Cat, speciesClass: Feline, noise: Bark, previousStage: CatMedium,
            initialLevel: 6, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 250, nutrition: 200,
            behavior: Domesticable, generationFrequency: Often);

        public static readonly ActorVariant Lynx = new ActorVariant(
            name: "lynx", species: Cat, speciesClass: Feline, noise: Growl,
            initialLevel: 5, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 200,
            generationFrequency: Commonly);

        public static readonly ActorVariant Jaguar = new ActorVariant(
            name: "jaguar", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 4, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly ActorVariant Panther = new ActorVariant(
            name: "panther", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 6, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly ActorVariant Tiger = new ActorVariant(
            name: "tiger", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 6, movementRate: 14, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly ActorVariant Pony = new ActorVariant(
            name: "pony", species: Species.Horse, speciesClass: Equine, noise: Neigh,
            initialLevel: 3, movementRate: 16, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Medium, weight: 1300, nutrition: 900,
            behavior: Domesticable | Mountable, generationFrequency: Often);

        public static readonly ActorVariant Horse = new ActorVariant(
            name: "horse", species: Species.Horse, speciesClass: Equine, noise: Neigh, previousStage: Pony,
            initialLevel: 5, movementRate: 20, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1500, nutrition: 1100,
            behavior: Domesticable | Mountable, generationFrequency: Often);

        public static readonly ActorVariant Warhorse = new ActorVariant(
            name: "warhorse", species: Species.Horse, speciesClass: Equine, noise: Neigh, previousStage: Horse,
            initialLevel: 7, movementRate: 24, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 10)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1800, nutrition: 1300,
            behavior: Domesticable | Mountable, generationFrequency: Often);

        public static readonly ActorVariant UnicornWhite = new ActorVariant(
            name: "white unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh, alignment: 7,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful, generationFrequency: Usually);

        public static readonly ActorVariant UnicornGray = new ActorVariant(
            name: "gray unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful, generationFrequency: Usually);

        public static readonly ActorVariant UnicornBlack = new ActorVariant(
            name: "black unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh, alignment: -7,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful, generationFrequency: Usually);

        public static readonly ActorVariant Rothe = new ActorVariant(
            name: "rothe", species: Quadruped, noise: Bleat,
            initialLevel: 2, movementRate: 9, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(Blindness, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Medium, weight: 600, nutrition: 400,
            behavior: NonWandering, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant Mumak = new ActorVariant(
            name: "mumak", species: Quadruped, noise: Roar,
            initialLevel: 5, movementRate: 9, armorClass: 0, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 2, diceSides: 12),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, ThickHide, Handlessness, Herbivorism),
            size: Large, weight: 2500, nutrition: 1000,
            generationFrequency: Sometimes);

        public static readonly ActorVariant Leocrotta = new ActorVariant(
            name: "leocrotta", species: Quadruped, noise: Imitate,
            initialLevel: 6, movementRate: 18, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Omnivorism),
            size: Large, weight: 1200, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly ActorVariant Wumpus = new ActorVariant(
            name: "wumpus", species: Quadruped, noise: Burble,
            initialLevel: 8, movementRate: 3, armorClass: 2, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(Clinginess, AnimalBody, Infravisibility, Handlessness, Omnivorism),
            size: Large, weight: 2500, nutrition: 500,
            generationFrequency: Usually);

        public static readonly ActorVariant Brontotheres = new ActorVariant(
            name: "brontotheres", species: Quadruped, noise: Roar,
            initialLevel: 12, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 2650, nutrition: 650,
            generationFrequency: Commonly);

        public static readonly ActorVariant Baluchitherium = new ActorVariant(
            name: "baluchitherium", species: Quadruped, noise: Roar,
            initialLevel: 14, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 5, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 5, diceSides: 4)
            },
            properties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 3800, nutrition: 800,
            generationFrequency: Commonly);

        public static readonly ActorVariant Mastodon = new ActorVariant(
            name: "mastodon", species: Quadruped, noise: Roar,
            initialLevel: 20, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Headbutt, PhysicalDamage, diceCount: 4, diceSides: 8)
            },
            properties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 3800, nutrition: 800,
            generationFrequency: Usually);

        public static readonly ActorVariant RatSewer = new ActorVariant(
            name: "sewer rat", species: Rat, speciesClass: Rodent, noise: Sqeek,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 100, nutrition: 50,
            generationFlags: SmallGroup, generationFrequency: Often);

        public static readonly ActorVariant RatGiant = new ActorVariant(
            name: "giant rat", species: Rat, speciesClass: Rodent, noise: Sqeek, previousStage: RatSewer,
            initialLevel: 2, movementRate: 10, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 150, nutrition: 75,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly ActorVariant RatRabid = new ActorVariant(
            name: "rabid rat", species: Rat, speciesClass: Rodent, noise: Sqeek,
            initialLevel: 3, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, DrainConstitution, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 150, nutrition: 50,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly ActorVariant RatWere = new ActorVariant(
            name: "ratwere", species: Rat, speciesClass: Rodent | ShapeChanger, noise: Sqeek,
            initialLevel: 3, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties: Has(
                PoisonResistance, Regeneration, AnimalBody, Infravisibility, Handlessness, Carnivorism),
            size: Small, weight: 150, nutrition: 50, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Wererat = new ActorVariant(
            name: "wererat", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 3, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            properties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, RatWere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant MoleRock = new ActorVariant(
            name: "rock mole", species: Mole, speciesClass: Rodent,
            initialLevel: 3, movementRate: 3, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Tunneling, AnimalBody, Infravisibility, Handlessness, Metallivorism),
            size: Small, weight: 100, nutrition: 50,
            behavior: GoldCollector | GemCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant Woodchuck = new ActorVariant(
            name: "woodchuck", species: Species.Woodchuck, speciesClass: Rodent,
            initialLevel: 3, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Swimming, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Small, weight: 100, nutrition: 50,
            behavior: GoldCollector | GemCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant BlobAcid = new ActorVariant(
            name: "acid blob", species: Blob,
            initialLevel: 1, movementRate: 3, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 8)
            },
            properties:
                Has(SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance,
                    DecayResistance, Stealthiness, Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness,
                    Headlessness, Mindlessness, Asexuality, Metallivorism),
            size: Tiny, weight: 30, nutrition: 1,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            generationFrequency: Usually);

        public static readonly ActorVariant BlobQuivering = new ActorVariant(
            name: "quivering blob", species: Blob,
            initialLevel: 5, movementRate: 1, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality),
            size: Small, weight: 200, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            generationFrequency: Usually);

        public static readonly ActorVariant CubeGelatinous = new ActorVariant(
            name: "gelatinous cube", species: Blob,
            initialLevel: 6, movementRate: 6, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Paralyze, diceCount: 1, diceSides: 8),
                new Attack(Touch, Engulf, diceCount: 3, diceSides: 4, frequency: Uncommonly),
                new Attack(Digestion, PoisonDamage, diceCount: 1, diceSides: 2),
                new Attack(Digestion, AcidDamage, diceCount: 1, diceSides: 2),
                new Attack(OnMeleeHit, Paralyze, diceCount: 1, diceSides: 8)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, PoisonResistance, VenomResistance, AcidResistance,
                StoningResistance, SleepResistance, DecayResistance, Stealthiness, Breathlessness, NonAnimal,
                Eyelessness, Limblessness, Headlessness, Mindlessness, Omnivorism, Asexuality),
            size: Large, weight: 600, nutrition: 150,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Uncommonly)},
            generationFrequency: Commonly, behavior: WeaponCollector);

        public static readonly ActorVariant OozeGray = new ActorVariant(
            name: "gray ooze", species: Ooze,
            initialLevel: 3, movementRate: 1, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 8),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(
                FireResistance, ColdResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism),
            size: Medium, weight: 500, nutrition: 250, corpse: None, consumptionProperties: new[]
            {
                WhenConsumedAdd(FireResistance, Uncommonly), WhenConsumedAdd(ColdResistance, Uncommonly),
                WhenConsumedAdd(VenomResistance, Uncommonly), WhenConsumedAdd(AcidResistance, Uncommonly),
                WhenConsumedAdd(SleepResistance, Uncommonly), WhenConsumedAdd(PoisonResistance, Uncommonly)
            },
            generationFrequency: Uncommonly);

        public static readonly ActorVariant SlimeGreen = new ActorVariant(
            name: "green slime", species: Ooze,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Slime),
                new Attack(OnMeleeHit, Slime),
                new Attack(OnConsumption, Slime)
            },
            properties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism),
            size: Medium, weight: 400, nutrition: 150, corpse: None,
            generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly ActorVariant PuddingBrown = new ActorVariant(
            name: "brown pudding", species: Ooze,
            initialLevel: 5, movementRate: 3, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, VenomDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, VenomDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, VenomDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism, Reanimation),
            size: Medium, weight: 512, nutrition: 256, consumptionProperties: new[]
            {
                WhenConsumedAdd(ColdResistance, Uncommonly), WhenConsumedAdd(ElectricityResistance, Uncommonly),
                WhenConsumedAdd(DecayResistance, Uncommonly)
            },
            generationFrequency: Rarely);

        public static readonly ActorVariant PuddingBlack = new ActorVariant(
            name: "black pudding", species: Ooze,
            initialLevel: 10, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, AcidDamage, diceCount: 3, diceSides: 8),
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 3, diceSides: 8)
            },
            properties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism, Reanimation),
            size: Medium, weight: 512, nutrition: 256, consumptionProperties: new[]
            {
                WhenConsumedAdd(ColdResistance, Uncommonly), WhenConsumedAdd(ElectricityResistance, Uncommonly),
                WhenConsumedAdd(AcidResistance, Uncommonly)
            },
            generationFrequency: Rarely);

        public static readonly ActorVariant JellyBlue = new ActorVariant(
            name: "blue jelly", species: Jelly,
            initialLevel: 4, movementRate: 0, armorClass: 8, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, ColdDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, ColdResistance, Stealthiness, Breathlessness,
                Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Commonly);

        public static readonly ActorVariant JellySpotted = new ActorVariant(
            name: "spotted jelly", species: Jelly,
            initialLevel: 5, movementRate: 0, armorClass: 8, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance, Stealthiness,
                Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Commonly);

        public static readonly ActorVariant JellyOchre = new ActorVariant(
            name: "ochre jelly", species: Jelly,
            initialLevel: 6, movementRate: 3, armorClass: 8, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 3),
                new Attack(Digestion, AcidDamage, diceCount: 3, diceSides: 6),
                new Attack(OnMeleeHit, AcidDamage, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance, Stealthiness,
                Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            generationFrequency: Commonly);

        public static readonly ActorVariant Lichen = new ActorVariant(
            name: "lichen", species: Fungus,
            initialLevel: 1, movementRate: 1, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Stick),
                new Attack(OnMeleeHit, Stick)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 20, nutrition: 100,
            behavior: NonWandering, generationFrequency: Commonly);

        public static readonly ActorVariant MoldBrown = new ActorVariant(
            name: "brown mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, ColdDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness,
                Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant MoldYellow = new ActorVariant(
            name: "yellow mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, Stun, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant MoldGreen = new ActorVariant(
            name: "green mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                AcidResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, DecayResistance,
                Stealthiness, Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant MoldRed = new ActorVariant(
            name: "red mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, FireDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                FireResistance, SleepResistance, PoisonResistance, VenomResistance, Stealthiness, DecayResistance,
                Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant Shrieker = new ActorVariant(
            name: "shrieker", species: Fungus,
            initialLevel: 3, movementRate: 1, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Scream, Deafen, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Breathlessness, NonAnimal, Eyelessness, Limblessness,
                Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Rarely)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant FungusViolet = new ActorVariant(
            name: "violet fungus", species: Fungus,
            initialLevel: 3, movementRate: 1, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PoisonDamage, diceCount: 1, diceSides: 6),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Stealthiness, Breathlessness, NonAnimal, Eyelessness,
                Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Rarely)},
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant CloudFog = new ActorVariant(
            name: "fog cloud", species: Cloud,
            initialLevel: 3, movementRate: 1, armorClass: 0, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 6),
                new Attack(Digestion, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Digestion, WaterDamage, diceCount: 1, diceSides: 2),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance, AcidResistance,
                SicknessResistance, Flight, Stealthiness, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Huge, weight: 1, nutrition: 0, corpse: None,
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly ActorVariant VortexDust = new ActorVariant(
            name: "dust vortex", species: Vortex,
            initialLevel: 4, movementRate: 20, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 6),
                new Attack(Digestion, Blind, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                WaterWeakness, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, NonAnimal, NonSolidBody, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly ActorVariant VortexIce = new ActorVariant(
            name: "ice vortex", species: Vortex,
            initialLevel: 5, movementRate: 20, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnRangedHit, ColdDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly ActorVariant VortexEnergy = new ActorVariant(
            name: "energy vortex", species: Vortex,
            initialLevel: 6, movementRate: 20, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, ElectricityDamage, diceCount: 1, diceSides: 6),
                new Attack(Digestion, DrainEnergy, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, ElectricityDamage, diceCount: 1, diceSides: 6),
                new Attack(OnRangedHit, ElectricityDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ElectricityResistance, DisintegrationResistance, SleepResistance, PoisonResistance, VenomResistance,
                StoningResistance, SlimingResistance, SicknessResistance, Flight, NonAnimal, NonSolidBody,
                Breathlessness, Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly ActorVariant VortexFire = new ActorVariant(
            name: "fire vortex", species: Vortex,
            initialLevel: 8, movementRate: 22, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, FireDamage, diceCount: 1, diceSides: 10),
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 10),
                new Attack(OnRangedHit, FireDamage, diceCount: 1, diceSides: 10)
            },
            properties: Has(
                FireResistance, SleepResistance, AcidResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly ActorVariant LightYellow = new ActorVariant(
            name: "yellow light", species: FloatingSphere,
            initialLevel: 3, movementRate: 15, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, Blind, diceCount: 5, diceSides: 10)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly ActorVariant LightBlack = new ActorVariant(
            name: "black light", species: FloatingSphere,
            initialLevel: 5, movementRate: 15, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, Hallucinate, diceCount: 5, diceSides: 10)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly ActorVariant LightWisp = new ActorVariant(
            name: "will o' wisp", species: FloatingSphere,
            initialLevel: 7, movementRate: 15, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Explosion, Confuse, diceCount: 5, diceSides: 10)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly ActorVariant SporeGas = new ActorVariant(
            name: "gas spore", species: FloatingSphere,
            initialLevel: 1, movementRate: 3, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Explosion, Deafen, diceCount: 5, diceSides: 10)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, NonAnimal, Breathlessness, Limblessness, Eyelessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            generationFrequency: Sometimes);

        public static readonly ActorVariant EyeFloating = new ActorVariant(
            name: "floating eye", species: FloatingSphere,
            initialLevel: 2, movementRate: 1, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, Paralyze, diceCount: 1, diceSides: 70)
            },
            properties: Has(
                Flight, Stealthiness, Infravision, Infravisibility, NonAnimal, Breathlessness, Limblessness,
                Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, consumptionProperties: new[] {WhenConsumedAdd(Telepathy, Always)},
            generationFrequency: Occasionally);

        public static readonly ActorVariant SphereFreezing = new ActorVariant(
            name: "freezing sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, ColdDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                ColdResistance, Flight, Infravisibility, NonAnimal, Breathlessness, Limblessness, Headlessness,
                Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Always)},
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly ActorVariant SphereFlaming = new ActorVariant(
            name: "flaming sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, FireDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                FireResistance, Flight, Infravisibility, NonAnimal, Breathlessness, Limblessness, Headlessness,
                Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Always)},
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly ActorVariant SphereShocking = new ActorVariant(
            name: "shocking sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, ElectricityDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                ElectricityResistance, Flight, NonAnimal, Breathlessness, Limblessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Always)},
            generationFrequency: Sometimes);

        public static readonly ActorVariant Beholder = new ActorVariant(
            name: "beholder", species: FloatingSphere,
            initialLevel: 8, movementRate: 4, armorClass: 4, magicResistance: 35, alignment: -10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Gaze, Disintegrate, diceCount: 2, diceSides: 4,
                    frequency: Uncommonly),
                new Attack(Gaze, Slow, diceCount: 2, diceSides: 25, frequency: Uncommonly),
                new Attack(Gaze, Sleep, diceCount: 2, diceSides: 25, frequency: Uncommonly),
                new Attack(Gaze, Confuse, diceCount: 2, diceSides: 25, frequency: Uncommonly),
                new Attack(Gaze, Stone, frequency: Uncommonly),
                new Attack(Gaze, Disenchant, frequency: Uncommonly)
            },
            properties: Has(
                ColdResistance, Levitation, DangerAwareness, Infravision, Infravisibility, Stealthiness, Breathlessness,
                Limblessness, Headlessness, Asexuality, NoInventory),
            size: Medium, weight: 250, nutrition: 50,
            consumptionProperties:
                new[] {WhenConsumedAdd(ColdResistance, Occasionally), WhenConsumedAdd(SleepResistance, Sometimes)},
            generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly ActorVariant Homunculus = new ActorVariant(
            name: "homunculus", species: Species.Homunculus,
            initialLevel: 2, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Bite, Sleep, diceCount: 1, diceSides: 3)
            },
            properties: Has(
                SleepResistance, PoisonResistance, Regeneration, Infravision, Infravisibility, Mindlessness, Asexuality),
            size: Small, weight: 60, nutrition: 60,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Sometimes);

        public static readonly ActorVariant Mane = new ActorVariant(
            name: "mane", species: Species.Homunculus, speciesClass: Demon, noise: Hiss,
            initialLevel: 3, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(SleepResistance, PoisonResistance, Infravision, Infravisibility),
            size: Medium, weight: 500, nutrition: 200, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            generationFlags: LargeGroup, behavior: Stalking, generationFrequency: Sometimes);

        public static readonly ActorVariant Lemure = new ActorVariant(
            name: "lemure", species: Species.Homunculus, speciesClass: Demon, noise: Hiss,
            initialLevel: 3, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(SleepResistance, PoisonResistance, Regeneration, Infravision, Infravisibility),
            size: Medium, weight: 500, nutrition: 200, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Sometimes);

        public static readonly ActorVariant Imp = new ActorVariant(
            name: "imp", species: Species.Imp, speciesClass: Demon, noise: Cuss,
            initialLevel: 3, movementRate: 12, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(Regeneration, Flight, Infravision, Infravisibility),
            size: Tiny, weight: 100, nutrition: 50, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Often);

        public static readonly ActorVariant Quasit = new ActorVariant(
            name: "quasit", species: Species.Imp, speciesClass: Demon, noise: Cuss,
            initialLevel: 3, movementRate: 15, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, DrainDexterity, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            properties: Has(PoisonResistance, Regeneration, Infravision, Infravisibility),
            size: Small, weight: 200, nutrition: 100, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Often);

        // TODO: fire, ice imps

        public static readonly ActorVariant Leprechaun = new ActorVariant(
            name: "leprechaun", species: Species.Leprechaun, noise: Laugh, speciesClass: Fey,
            initialLevel: 5, movementRate: 15, armorClass: 8, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Claw, StealGold)
            },
            properties: Has(Teleportation, Infravisibility, Omnivorism),
            size: Tiny, weight: 60, nutrition: 30,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(Teleportation, Sometimes), WhenConsumedAdd(Luck, value: 1, frequency: Rarely)
            },
            behavior: GoldCollector, generationFrequency: Occasionally);

        public static readonly ActorVariant NymphWood = new ActorVariant(
            name: "wood nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            properties: Has(Teleportation, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant NymphWater = new ActorVariant(
            name: "water nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            properties: Has(Teleportation, Swimming, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant NymphMountain = new ActorVariant(
            name: "mountain nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            properties: Has(Teleportation, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly ActorVariant Gremlin = new ActorVariant(
            name: "gremlin", species: Species.Gremlin, noise: Laugh,
            initialLevel: 5, movementRate: 12, armorClass: 2, magicResistance: 25, alignment: -5,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, Curse),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(PoisonResistance, Humanoidness, Swimming, Infravisibility, Omnivorism),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Commonly);

        public static readonly ActorVariant Gargoyle = new ActorVariant(
            name: "gargoyle", species: Species.Gargoyle, noise: Grunt,
            initialLevel: 6, movementRate: 10, armorClass: -4, magicResistance: 0, alignment: -9,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(StoningResistance, Humanoidness, ThickHide, Breathlessness),
            size: Medium, weight: 1000, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            behavior: NonWandering, generationFrequency: Commonly);

        public static readonly ActorVariant WingedGargoyle = new ActorVariant(
            name: "winged gargoyle", species: Species.Gargoyle, noise: Grunt, previousStage: Gargoyle,
            initialLevel: 9, movementRate: 15, armorClass: -4, magicResistance: 0, alignment: -12,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            properties: Has(StoningResistance, Flight, Humanoidness, ThickHide, Breathlessness, Oviparity),
            size: Medium, weight: 1200, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            behavior: NonWandering | MagicUser, generationFrequency: Commonly);

        public static readonly ActorVariant Chickatrice = new ActorVariant(
            name: "chickatrice", species: Species.Cockatrice, speciesClass: Hybrid, noise: Hiss,
            initialLevel: 4, movementRate: 4, armorClass: 8, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Touch, Stone, frequency: Sometimes),
                new Attack(OnMeleeHit, Stone),
                new Attack(OnConsumption, Stone)
            },
            properties: Has(
                PoisonResistance, StoningResistance, AnimalBody, Infravisibility, Handlessness, Omnivorism, Asexuality),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Sometimes),
                WhenConsumedAdd(StoningResistance, Sometimes)
            },
            generationFlags: SmallGroup, generationFrequency: Commonly, behavior: NonWandering);

        public static readonly ActorVariant Cockatrice = new ActorVariant(
            name: "cockatrice", species: Species.Cockatrice, speciesClass: Hybrid, noise: Hiss,
            previousStage: Chickatrice,
            initialLevel: 5, movementRate: 6, armorClass: 6, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Touch, Stone),
                new Attack(OnMeleeHit, Stone),
                new Attack(OnConsumption, Stone)
            },
            properties: Has(
                PoisonResistance, StoningResistance, AnimalBody, Infravisibility, Handlessness, Omnivorism, Oviparity),
            size: Small, weight: 30, nutrition: 30, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Sometimes),
                WhenConsumedAdd(StoningResistance, Sometimes)
            },
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant Pyrolisk = new ActorVariant(
            name: "pyrolisk", species: Species.Cockatrice, speciesClass: Hybrid, noise: Hiss,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Gaze, FireDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, FireResistance, AnimalBody, Infravisibility, Handlessness, Omnivorism, Oviparity),
            size: Small, weight: 30, nutrition: 30, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Uncommonly),
                WhenConsumedAdd(FireResistance, Sometimes)
            }, generationFrequency: Sometimes);

        public static readonly ActorVariant Tengu = new ActorVariant(
            name: "tengu", species: Species.Tengu, speciesClass: ShapeChanger, noise: Squawk,
            initialLevel: 6, movementRate: 13, armorClass: 5, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, Teleport)
            },
            properties: Has(PoisonResistance, Teleportation, TeleportationControl, Infravisibility, Infravision),
            size: Small, weight: 300, nutrition: 150, consumptionProperties: new[]
            {
                WhenConsumedAdd(Teleportation, Sometimes),
                WhenConsumedAdd(TeleportationControl, Uncommonly)
            }, behavior: NonWandering | Stalking, generationFrequency: Occasionally);

        public static readonly ActorVariant MimicSmall = new ActorVariant(
            name: "small mimic", species: Mimic, speciesClass: ShapeChanger,
            initialLevel: 7, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            properties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Small, weight: 300, nutrition: 200,
            behavior: NonWandering, generationFrequency: Occasionally);

        public static readonly ActorVariant MimicLarge = new ActorVariant(
            name: "large mimic", species: Mimic, speciesClass: ShapeChanger, previousStage: MimicSmall,
            initialLevel: 8, movementRate: 3, armorClass: 7, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 5),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            properties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Medium, weight: 600, nutrition: 400,
            behavior: NonWandering, generationFrequency: Occasionally);

        public static readonly ActorVariant MimicGiant = new ActorVariant(
            name: "giant mimic", species: Mimic, speciesClass: ShapeChanger, previousStage: MimicLarge,
            initialLevel: 9, movementRate: 3, armorClass: 7, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            properties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Large, weight: 800, nutrition: 500,
            behavior: NonWandering, generationFrequency: Occasionally);

        public static readonly ActorVariant PiercerRock = new ActorVariant(
            name: "rock piercer", species: Piercer,
            initialLevel: 3, movementRate: 1, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism, NoInventory),
            size: Small, weight: 200, nutrition: 100,
            generationFrequency: Occasionally);

        public static readonly ActorVariant PiercerIron = new ActorVariant(
            name: "iron piercer", species: Piercer,
            initialLevel: 5, movementRate: 1, armorClass: 2, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(
                Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism, NoInventory),
            size: Small, weight: 300, nutrition: 150,
            generationFrequency: Occasionally);

        public static readonly ActorVariant PiercerGlass = new ActorVariant(
            name: "glass piercer", species: Piercer,
            initialLevel: 7, movementRate: 1, armorClass: 1, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                AcidResistance, Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism,
                NoInventory),
            size: Small, weight: 400, nutrition: 200,
            generationFrequency: Occasionally);

        public static readonly ActorVariant LurkerAbove = new ActorVariant(
            name: "lurker above", species: Species.Trapper,
            initialLevel: 10, movementRate: 3, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, Suffocate)
            },
            properties: Has(
                Flight, Camouflage, AnimalBody, Stealthiness, Eyelessness, Headlessness, Limblessness, Clinginess,
                Carnivorism),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly ActorVariant Trapper = new ActorVariant(
            name: "trapper", species: Species.Trapper,
            initialLevel: 12, movementRate: 3, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 4),
                new Attack(Digestion, Suffocate)
            },
            properties: Has(
                Camouflage, AnimalBody, InvisibilityDetection, Stealthiness, Eyelessness, Headlessness, Limblessness,
                Carnivorism),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly ActorVariant Couatl = new ActorVariant(
            name: "couatl", species: WingedSnake, speciesClass: DivineBeing, noise: Hiss,
            initialLevel: 8, movementRate: 10, armorClass: 5, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Hug, Bind)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Flight, Infravision, SerpentlikeBody),
            size: Large, weight: 900, nutrition: 400, corpse: None,
            behavior: Stalking | AlignmentAware, generationFlags: NoHell | SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant Kirin = new ActorVariant(
            name: "ki-rin", species: Species.Kirin, speciesClass: DivineBeing, noise: Neigh,
            initialLevel: 16, movementRate: 18, armorClass: -5, magicResistance: 90, alignment: 15,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Kick, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Headbutt, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Flight, AnimalBody, ThickHide,
                Infravisibility, Infravision, InvisibilityDetection, Handlessness),
            size: Large, weight: 1300, nutrition: 600, corpse: None,
            behavior: Stalking | AlignmentAware,
            generationFlags: NoHell | NonPolymorphable, generationFrequency: Sometimes);

        public static readonly ActorVariant Aleax = new ActorVariant(
            name: "aleax", species: Species.Angel, speciesClass: DivineBeing, noise: Imitate,
            initialLevel: 10, movementRate: 8, armorClass: 0, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None, consumptionProperties: new[]
            {
                WhenConsumedAdd(SleepResistance, Usually),
                WhenConsumedAdd(PoisonResistance, Usually),
                WhenConsumedAdd(VenomResistance, Usually)
            },
            behavior: Stalking | AlignmentAware | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly ActorVariant Angel = new ActorVariant(
            name: "angel", species: Species.Angel, speciesClass: DivineBeing, noise: Speach, previousStage: Aleax,
            initialLevel: 14, movementRate: 10, armorClass: -4, magicResistance: 55, alignment: 12,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None, consumptionProperties: new[]
            {
                WhenConsumedAdd(SleepResistance, Usually),
                WhenConsumedAdd(PoisonResistance, Usually),
                WhenConsumedAdd(VenomResistance, Usually)
            },
            behavior: Stalking | AlignmentAware | WeaponCollector | MagicUser,
            generationFlags: NoHell | NonPolymorphable, generationFrequency: Sometimes);

        public static readonly ActorVariant Archon = new ActorVariant(
            name: "archon", species: Species.Angel, speciesClass: DivineBeing, noise: Speach, previousStage: Angel,
            initialLevel: 19, movementRate: 16, armorClass: -6, magicResistance: 80, alignment: 15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, Regeneration, Flight, Infravisibility, Infravision, InvisibilityDetection,
                Humanoidness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None, consumptionProperties: new[]
            {
                WhenConsumedAdd(SleepResistance, Usually),
                WhenConsumedAdd(PoisonResistance, Usually),
                WhenConsumedAdd(VenomResistance, Usually)
            },
            behavior: Stalking | AlignmentAware | WeaponCollector | MagicUser,
            generationFlags: NoHell | NonPolymorphable, generationFrequency: Sometimes);

        public static readonly ActorVariant CentaurPlains = new ActorVariant(
            name: "plains centaur", species: Centaur, noise: Speach,
            initialLevel: 4, movementRate: 18, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly ActorVariant CentaurForest = new ActorVariant(
            name: "forest centaur", species: Centaur, noise: Speach,
            initialLevel: 5, movementRate: 18, armorClass: 3, magicResistance: 10, alignment: -1,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly ActorVariant CentaurMountain = new ActorVariant(
            name: "mountain centaur", species: Centaur, noise: Speach,
            initialLevel: 6, movementRate: 20, armorClass: 2, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly ActorVariant NagaRedHatchling = new ActorVariant(
            name: "red naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                FireResistance, PoisonResistance, VenomResistance, SlimingResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(FireResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaRed = new ActorVariant(
            name: "red naga", species: Naga, noise: Hiss,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 0, alignment: -4,
            previousStage: NagaRedHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Spit, FireDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                FireResistance, PoisonResistance, VenomResistance, SlimingResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(FireResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaBlackHatchling = new ActorVariant(
            name: "black naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaBlack = new ActorVariant(
            name: "black naga", species: Naga, noise: Hiss,
            initialLevel: 8, movementRate: 14, armorClass: 2, magicResistance: 10, alignment: 4,
            previousStage: NagaBlackHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Spit, AcidDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaGoldenHatchling = new ActorVariant(
            name: "golden naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, VenomResistance, InvisibilityDetection, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(InvisibilityDetection, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaGolden = new ActorVariant(
            name: "golden naga", species: Naga, noise: Hiss,
            initialLevel: 10, movementRate: 14, armorClass: 2, magicResistance: 70, alignment: 5,
            previousStage: NagaGoldenHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaGuardianHatchling = new ActorVariant(
            name: "guardian naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, VenomResistance, InvisibilityDetection, Infravision, ThickHide, SerpentlikeBody,
                Limblessness, Carnivorism),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(VenomResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant NagaGuardian = new ActorVariant(
            name: "guardian naga", species: Naga, noise: Hiss,
            initialLevel: 12, movementRate: 16, armorClass: 0, magicResistance: 50, alignment: 7,
            previousStage: NagaGuardianHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spit, VenomDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Infravision, ThickHide, SerpentlikeBody, Limblessness, Carnivorism,
                Oviparity),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(VenomResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        // TODO: Add yuan-ti

        public static readonly ActorVariant DragonGrayBaby = new ActorVariant(
            name: "baby gray dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 50, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonGray = new ActorVariant(
            name: "gray dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 70, alignment: 4,
            previousStage: DragonGrayBaby,
            attacks: new[]
            {
                new Attack(Breath, MagicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision, DangerAwareness,
                AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(InvisibilityDetection, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonSilverBaby = new ActorVariant(
            name: "baby silver dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                Reflection, PoisonResistance, Flight, Infravisibility, AnimalBody, ThickHide, Handlessness, Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonSilver = new ActorVariant(
            name: "silver dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonSilverBaby,
            attacks: new[]
            {
                new Attack(Breath, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                Reflection, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DangerAwareness, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonShimmeringBaby = new ActorVariant(
            name: "baby shimmering dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: -5, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonShimmering = new ActorVariant(
            name: "shimmering dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -8, magicResistance: 20, alignment: 4,
            previousStage: DragonShimmeringBaby,
            attacks: new[]
            {
                new Attack(Breath, Confuse, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(Infravision, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonGreenBaby = new ActorVariant(
            name: "baby green dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonGreen = new ActorVariant(
            name: "green dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonGreenBaby,
            attacks: new[]
            {
                new Attack(Breath, PoisonDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonPurpleBaby = new ActorVariant(
            name: "baby purple dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                VenomResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonPurple = new ActorVariant(
            name: "purple dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonPurpleBaby,
            attacks: new[]
            {
                new Attack(Breath, VenomDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                VenomResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonOrangeBaby = new ActorVariant(
            name: "baby orange dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonOrange = new ActorVariant(
            name: "orange dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonOrangeBaby,
            attacks: new[]
            {
                new Attack(Breath, Sleep, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                SleepResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonRedBaby = new ActorVariant(
            name: "baby red dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                FireResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonRed = new ActorVariant(
            name: "red dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonRedBaby,
            attacks: new[]
            {
                new Attack(Breath, FireDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                FireResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonWhiteBaby = new ActorVariant(
            name: "baby white dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                ColdResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonWhite = new ActorVariant(
            name: "white dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonWhiteBaby,
            attacks: new[]
            {
                new Attack(Breath, ColdDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                ColdResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonBlueBaby = new ActorVariant(
            name: "baby blue dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                ElectricityResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonBlue = new ActorVariant(
            name: "blue dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonBlueBaby,
            attacks: new[]
            {
                new Attack(Breath, ElectricityDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                ElectricityResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonYellowBaby = new ActorVariant(
            name: "baby yellow dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                AcidResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonYellow = new ActorVariant(
            name: "yellow dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonYellowBaby,
            attacks: new[]
            {
                new Attack(Breath, AcidDamage, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                AcidResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonBlackBaby = new ActorVariant(
            name: "baby black dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                DisintegrationResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonBlack = new ActorVariant(
            name: "black dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonBlackBaby,
            attacks: new[]
            {
                new Attack(Breath, Disintegrate, diceCount: 4, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                DisintegrationResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DisintegrationResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonFairyBaby = new ActorVariant(
            name: "baby fairy dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, Flight, Invisibility, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonFairy = new ActorVariant(
            name: "fairy dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonFairyBaby,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision, Invisibility,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DisintegrationResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonDeepBaby = new ActorVariant(
            name: "baby deep dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                DrainResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DragonDeep = new ActorVariant(
            name: "deep dragon", species: Dragon, noise: Roar,
            initialLevel: 15, movementRate: 9, armorClass: -1, magicResistance: 20, alignment: 4,
            previousStage: DragonDeepBaby,
            attacks: new[]
            {
                new Attack(Breath, DrainLife, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                DrainResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DrainResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly ActorVariant Stalker = new ActorVariant(
            name: "stalker", species: Elemental,
            initialLevel: 8, movementRate: 12, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            properties: Has(
                Flight, Invisibility, InvisibilityDetection, Infravision, AnimalBody, Stealthiness),
            size: Large, weight: 900, nutrition: 400,
            consumptionProperties:
                new[] {WhenConsumedAdd(Invisibility, Uncommonly), WhenConsumedAdd(InvisibilityDetection, Uncommonly)},
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly ActorVariant ElementalAir = new ActorVariant(
            name: "air elemental", species: Elemental,
            initialLevel: 8, movementRate: 36, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 4),
                new Attack(Digestion, Deafen, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, Invisibility, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant ElementalFire = new ActorVariant(
            name: "fire elemental", species: Elemental,
            initialLevel: 8, movementRate: 12, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, FireDamage, diceCount: 3, diceSides: 6),
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                WaterWeakness, FireResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant ElementalWater = new ActorVariant(
            name: "water elemental", species: Elemental,
            initialLevel: 8, movementRate: 6, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 5, diceSides: 6),
                new Attack(Punch, WaterDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Swimming, NonAnimal, NonSolidBody, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 2500, nutrition: 0, corpse: None,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant ElementalEarth = new ActorVariant(
            name: "earth elemental", species: Elemental,
            initialLevel: 8, movementRate: 6, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Punch, Stun, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                FireResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Phasing, ThickHide, NonAnimal, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 2500, nutrition: 0, corpse: None,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant Gnome = new ActorVariant(
            name: "gnome", species: Species.Gnome, noise: Speach,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 650, nutrition: 200,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Rarely);

        public static readonly ActorVariant GnomeLord = new ActorVariant(
            name: "gnome lord", species: Species.Gnome, noise: Speach, previousStage: Gnome,
            initialLevel: 3, movementRate: 8, armorClass: 10, magicResistance: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 700, nutrition: 250,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Uncommonly);

        public static readonly ActorVariant GnomeWizard = new ActorVariant(
            name: "gnomish wizard", species: Species.Gnome, noise: Speach,
            initialLevel: 3, movementRate: 8, armorClass: 10, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 700, nutrition: 250,
            behavior: WeaponCollector | GoldCollector | MagicUser,
            generationFlags: NonPolymorphable, generationFrequency: Uncommonly);

        public static readonly ActorVariant GnomeKing = new ActorVariant(
            name: "gnome king", species: Species.Gnome, noise: Speach, previousStage: GnomeLord,
            initialLevel: 5, movementRate: 10, armorClass: 10, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 750, nutrition: 300,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | Entourage, generationFrequency: Uncommonly);

        public static readonly ActorVariant Ogre = new ActorVariant(
            name: "ogre", species: Species.Ogre, noise: Grunt,
            initialLevel: 5, movementRate: 10, armorClass: 5, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 5)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism),
            size: Large, weight: 1600, nutrition: 500,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly ActorVariant OgreLord = new ActorVariant(
            name: "ogre lord", species: Species.Ogre, noise: Grunt, previousStage: Ogre,
            initialLevel: 7, movementRate: 12, armorClass: 3, magicResistance: 30, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism, Maleness),
            size: Large, weight: 1650, nutrition: 550,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFrequency: Occasionally);

        public static readonly ActorVariant OgreKing = new ActorVariant(
            name: "ogre king", species: Species.Ogre, noise: Grunt, previousStage: OgreLord,
            initialLevel: 9, movementRate: 14, armorClass: 4, magicResistance: 60, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 5)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism, Maleness),
            size: Large, weight: 1700, nutrition: 600,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFlags: Entourage, generationFrequency: Occasionally);

        public static readonly ActorVariant Giant = new ActorVariant(
            name: "giant", species: Species.Giant, noise: Boast,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant GiantStone = new ActorVariant(
            name: "stone giant", species: Species.Giant, noise: Boast,
            initialLevel: 6, movementRate: 6, armorClass: 0, magicResistance: 0, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant GiantHill = new ActorVariant(
            name: "hill giant", species: Species.Giant, noise: Boast,
            initialLevel: 8, movementRate: 10, armorClass: 6, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant GiantFire = new ActorVariant(
            name: "fire giant", species: Species.Giant, noise: Boast,
            initialLevel: 9, movementRate: 12, armorClass: 4, magicResistance: 5, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            properties: Has(FireResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant GiantFrost = new ActorVariant(
            name: "frost giant", species: Species.Giant, noise: Boast,
            initialLevel: 10, movementRate: 12, armorClass: 3, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            properties: Has(ColdResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup | NoHell,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Ettin = new ActorVariant(
            name: "ettin", species: Species.Giant, noise: Boast,
            initialLevel: 10, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFrequency: Uncommonly);

        public static readonly ActorVariant GiantStorm = new ActorVariant(
            name: "storm giant", species: Species.Giant, noise: Boast,
            initialLevel: 16, movementRate: 12, armorClass: 3, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 12)
            },
            properties: Has(ElectricityResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Titan = new ActorVariant(
            name: "titan", species: Species.Giant, noise: Boast,
            initialLevel: 16, movementRate: 18, armorClass: -3, magicResistance: 70, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(Flight, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Gigantic, weight: 3000, nutrition: 900,
            behavior: WeaponCollector | GemCollector | MagicUser,
            generationFlags: NonGenocidable, generationFrequency: Rarely);

        public static readonly ActorVariant Minotaur = new ActorVariant(
            name: "minotaur", species: Species.Minotaur, noise: Roar,
            initialLevel: 15, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Headbutt, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1500, nutrition: 600,
            behavior: WeaponCollector | GemCollector, generationFrequency: Rarely);

        public static readonly ActorVariant Jabberwock = new ActorVariant(
            name: "jabberwock", species: Species.Jabberwock, noise: Burble,
            initialLevel: 15, movementRate: 12, armorClass: -2, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            properties: Has(Flight, Infravision, Infravisibility, AnimalBody, Carnivorism),
            size: Large, weight: 1300, nutrition: 400,
            generationFrequency: Rarely);

        public static readonly ActorVariant Vampire = new ActorVariant(
            name: "vampire", species: Species.Vampire, speciesClass: Undead | ShapeChanger, noise: Speach,
            initialLevel: 10, movementRate: 12, armorClass: 1, magicResistance: 25, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant VampireLord = new ActorVariant(
            name: "vampire lord", species: Species.Vampire, speciesClass: Undead | ShapeChanger,
            noise: ActorNoiseType.Vampire,
            initialLevel: 12, movementRate: 14, armorClass: 0, magicResistance: 40, alignment: -9,
            previousStage: Vampire,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking, generationFrequency: Rarely);

        public static readonly ActorVariant VampireMage = new ActorVariant(
            name: "vampire mage", species: Species.Vampire, speciesClass: Undead | ShapeChanger,
            noise: ActorNoiseType.Vampire,
            initialLevel: 20, movementRate: 14, armorClass: -4, magicResistance: 60, alignment: -9,
            previousStage: VampireLord,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 8),
                new Attack(Spell, MagicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Invisibility,
                InvisibilityDetection, Infravision, Humanoidness, Breathlessness)
                .With(ActorProperty.Remove(MaxHP, value: 20)),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | MagicUser | Stalking, generationFrequency: Rarely);

        public static readonly ActorVariant VampireVlad = new ActorVariant(
            name: "Vlad the Impaler", species: Species.Vampire, speciesClass: Undead | ShapeChanger,
            initialLevel: 14, movementRate: 18, armorClass: -3, magicResistance: 80, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 10),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking | Covetous | RangedPeaceful,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Lich = new ActorVariant(
            name: "lich", species: Species.Lich, speciesClass: Undead, noise: Mumble,
            initialLevel: 11, movementRate: 6, armorClass: 0, magicResistance: 30, alignment: -9,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 1, diceSides: 10),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(
                ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance, Regeneration,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            behavior: MagicUser, generationFrequency: Rarely);

        public static readonly ActorVariant LichDemi = new ActorVariant(
            name: "demilich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: Lich,
            initialLevel: 14, movementRate: 9, armorClass: -2, magicResistance: 60, alignment: -12,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 3, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            properties: Has(
                ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance, Regeneration,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None, consumptionProperties:
                new[] {WhenConsumedAdd(ColdResistance, Commonly), WhenConsumedAdd(EnergyRegeneration, Commonly)},
            behavior: MagicUser, generationFrequency: Rarely);

        public static readonly ActorVariant LichMaster = new ActorVariant(
            name: "master lich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: LichDemi,
            initialLevel: 17, movementRate: 9, armorClass: -4, magicResistance: 90, alignment: -15,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                FireResistance, ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance,
                Regeneration, Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(FireResistance, Commonly), WhenConsumedAdd(ColdResistance, Commonly),
                WhenConsumedAdd(EnergyRegeneration, Commonly)
            },
            behavior: MagicUser | Covetous, generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly ActorVariant LichArch = new ActorVariant(
            name: "arch-lich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: LichMaster,
            initialLevel: 25, movementRate: 9, armorClass: -6, magicResistance: 90, alignment: -15,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 5, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 8)
            },
            properties: Has(
                FireResistance, ColdResistance, ElectricityResistance, SleepResistance, SicknessResistance,
                PoisonResistance, VenomResistance, Regeneration, Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(FireResistance, Commonly), WhenConsumedAdd(ColdResistance, Commonly),
                WhenConsumedAdd(ElectricityResistance, Commonly), WhenConsumedAdd(EnergyRegeneration, Commonly)
            },
            behavior: MagicUser | Covetous, generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly ActorVariant ZombieKobold = new ActorVariant(
            name: "kobold zombie", species: Kobold, speciesClass: Undead, noise: Moan,
            initialLevel: 1, movementRate: 6, armorClass: 9, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 400, nutrition: 50, corpse: KoboldMedium,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant MummyKobold = new ActorVariant(
            name: "kobold mummy", species: Kobold, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 6, armorClass: 6, magicResistance: 20, alignment: -2,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 400, nutrition: 50, corpse: KoboldMedium,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant ZombieGnome = new ActorVariant(
            name: "gnome zombie", species: Species.Gnome, speciesClass: Undead, noise: Moan,
            initialLevel: 2, movementRate: 6, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Small, weight: 650, nutrition: 100, corpse: Gnome,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant MummyGnome = new ActorVariant(
            name: "gnome mummy", species: Species.Gnome, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 6, armorClass: 6, magicResistance: 20, alignment: -3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 650, nutrition: 100, corpse: Gnome,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant ZombieOrc = new ActorVariant(
            name: "orc zombie", species: Species.Orc, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 9, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 1000, nutrition: 100, corpse: Orc,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant MummyOrc = new ActorVariant(
            name: "orc mummy", species: Species.Orc, speciesClass: Undead, noise: Moan,
            initialLevel: 5, movementRate: 9, armorClass: 5, magicResistance: 20, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 100, corpse: Orc,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant ZombieDwarf = new ActorVariant(
            name: "dwarf zombie", species: Species.Dwarf, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 6, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 900, nutrition: 200, corpse: Dwarf,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant MummyDwarf = new ActorVariant(
            name: "dwarf mummy", species: Species.Dwarf, speciesClass: Undead, noise: Moan,
            initialLevel: 5, movementRate: 6, armorClass: 5, magicResistance: 30, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 900, nutrition: 200, corpse: Dwarf,
            behavior: NonWandering, generationFrequency: Sometimes);

        public static readonly ActorVariant ZombieElf = new ActorVariant(
            name: "elf zombie", species: Species.Elf, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 12, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, InvisibilityDetection,
                Infravision, Humanoidness, Breathlessness, Mindlessness),
            size: Medium, weight: 800, nutrition: 150, corpse: Elf,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant MummyElf = new ActorVariant(
            name: "elf mummy", species: Species.Elf, speciesClass: Undead, noise: Moan,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 30, alignment: -3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, InvisibilityDetection,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 800, nutrition: 150, corpse: Elf,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: NonWandering, generationFrequency: Sometimes);

        public static readonly ActorVariant ZombieHuman = new ActorVariant(
            name: "human zombie", species: Species.Human, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 1000, nutrition: 200, corpse: Human,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly ActorVariant MummyHuman = new ActorVariant(
            name: "human mummy", species: Species.Human, speciesClass: Undead, noise: Moan,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 200, corpse: Human,
            behavior: NonWandering, generationFrequency: Sometimes);

        public static readonly ActorVariant ZombieGiant = new ActorVariant(
            name: "giant zombie", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 8, movementRate: 6, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 10),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Giant,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant MummyGiant = new ActorVariant(
            name: "giant mummy", species: Species.Giant, speciesClass: Undead, noise: Moan, alignment: -7,
            initialLevel: 10, movementRate: 6, armorClass: 3, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Giant,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant ZombieEttin = new ActorVariant(
            name: "ettin zombie", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 10, movementRate: 12, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Ettin,
            behavior: Stalking | NonWandering, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant MummyEttin = new ActorVariant(
            name: "ettin mummy", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 12, movementRate: 12, armorClass: 1, magicResistance: 20, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Ettin,
            behavior: NonWandering, generationFrequency: Uncommonly);

        public static readonly ActorVariant Skeleton = new ActorVariant(
            name: "skeleton", species: Species.Skeleton, speciesClass: Undead,
            initialLevel: 3, movementRate: 6, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, StoningResistance, SicknessResistance,
                VenomResistance, ThickHide, Infravision, Humanoidness, Breathlessness, Mindlessness),
            size: Medium, weight: 300, nutrition: 5, corpse: None,
            behavior: WeaponCollector, generationFlags: SmallGroup, generationFrequency: Occasionally);

        //TODO: Add more skeletons

        public static readonly ActorVariant Ghoul = new ActorVariant(
            name: "ghoul", species: Species.Ghoul, speciesClass: Undead, noise: Growl,
            initialLevel: 12, movementRate: 8, armorClass: 4, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, Slow, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 50, corpse: None,
            generationFrequency: Occasionally);

        public static readonly ActorVariant Ghast = new ActorVariant(
            name: "ghast", species: Species.Ghoul, speciesClass: Undead, noise: Growl,
            initialLevel: 15, movementRate: 8, armorClass: 2, magicResistance: 0, alignment: -3, previousStage: Ghoul,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, Paralyze, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 50, corpse: None,
            generationFrequency: Occasionally);

        public static readonly ActorVariant Ghost = new ActorVariant(
            name: "ghost", species: Species.Ghost, speciesClass: Undead,
            initialLevel: 10, movementRate: 3, armorClass: -5, magicResistance: 15, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 1, diceSides: 1)
            },
            properties: Has(
                ColdResistance, DisintegrationResistance, SleepResistance, PoisonResistance, SicknessResistance,
                StoningResistance, SlimingResistance, Flight, Phasing, Infravision, NonSolidBody, Humanoidness,
                Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking | NonWandering, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly ActorVariant Shade = new ActorVariant(
            name: "shade", species: Species.Ghost, speciesClass: Undead, noise: Howl, previousStage: Ghost,
            initialLevel: 12, movementRate: 10, armorClass: 10, magicResistance: 25, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, Paralyze, diceCount: 2, diceSides: 6),
                new Attack(Touch, Slow, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                ColdResistance, DisintegrationResistance, SleepResistance, PoisonResistance, SicknessResistance,
                StoningResistance, SlimingResistance, Flight, Phasing, Infravision, InvisibilityDetection, NonSolidBody,
                Humanoidness,
                Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking, generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant WightBarrow = new ActorVariant(
            name: "barrow wight", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 3, movementRate: 12, armorClass: 5, magicResistance: 5, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1200, nutrition: 0, corpse: None,
            behavior: Stalking | WeaponCollector | MagicUser, generationFrequency: Uncommonly);

        public static readonly ActorVariant Wraith = new ActorVariant(
            name: "wraith", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 15, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(OnMeleeHit, DrainLife, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, DrainLife, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, StoningResistance,
                SlimingResistance, Flight, Infravision, NonSolidBody, Humanoidness, Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly ActorVariant Nazgul = new ActorVariant(
            name: "nazgul", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 13, movementRate: 12, armorClass: 0, magicResistance: 25, alignment: -17,
            attacks: new[]
            {
                new Attack(Weapon, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(Breath, Sleep, diceCount: 2, diceSides: 25),
                new Attack(OnConsumption, DrainLife, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, StoningResistance,
                SlimingResistance, Flight, Infravision, NonSolidBody, Humanoidness, Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant RustMonster = new ActorVariant(
            name: "rust monster", species: Species.RustMonster,
            initialLevel: 5, movementRate: 18, armorClass: 2, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 6),
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 6),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(Swimming, Infravisibility, AnimalBody, Handlessness, Metallivorism),
            size: Medium, weight: 1000, nutrition: 300,
            behavior: NonWandering, generationFrequency: Occasionally);

        public static readonly ActorVariant Disenchanter = new ActorVariant(
            name: "disenchanter", species: Species.Disenchanter, noise: Growl,
            initialLevel: 12, movementRate: 12, armorClass: -10, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Disenchant),
                new Attack(OnMeleeHit, Disenchant)
            },
            properties: Has(Infravisibility, AnimalBody, Handlessness, Metallivorism),
            size: Medium, weight: 750, nutrition: 200,
            behavior: NonWandering, generationFrequency: Occasionally);

        public static readonly ActorVariant SnakeGarter = new ActorVariant(
            name: "garter snake", species: Species.Snake, noise: Hiss,
            initialLevel: 1, movementRate: 8, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness, Carnivorism, Oviparity, NoInventory),
            size: Tiny, weight: 50, nutrition: 25,
            generationFlags: LargeGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant Snake = new ActorVariant(
            name: "snake", species: Species.Snake, noise: Hiss,
            initialLevel: 4, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Carnivorism, Oviparity, NoInventory),
            size: Small, weight: 100, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant WaterMoccasin = new ActorVariant(
            name: "water moccasin", species: Species.Snake, noise: Hiss,
            initialLevel: 4, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Carnivorism, Oviparity, NoInventory),
            size: Small, weight: 150, nutrition: 75,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFlags: LargeGroup, generationFrequency: Never);

        public static readonly ActorVariant Python = new ActorVariant(
            name: "python", species: Species.Snake, noise: Hiss,
            initialLevel: 6, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Hug, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Hug, Bind, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                Swimming, Infravision, SerpentlikeBody, Limblessness, Carnivorism, Oviparity, NoInventory),
            size: Medium, weight: 250, nutrition: 125,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant PitViper = new ActorVariant(
            name: "pit viper", species: Species.Snake, noise: Hiss,
            initialLevel: 6, movementRate: 15, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Carnivorism, Oviparity, NoInventory),
            size: Medium, weight: 100, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Cobra = new ActorVariant(
            name: "cobra", species: Species.Snake, noise: Hiss,
            initialLevel: 7, movementRate: 18, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(Spit, Blind, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Carnivorism, Oviparity, NoInventory),
            size: Medium, weight: 250, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Troll = new ActorVariant(
            name: "troll", species: Species.Troll, noise: Grunt,
            initialLevel: 5, movementRate: 10, armorClass: 5, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFrequency: Occasionally);

        public static readonly ActorVariant TrollIce = new ActorVariant(
            name: "ice troll", species: Species.Troll, noise: Grunt,
            initialLevel: 9, movementRate: 10, armorClass: 2, magicResistance: 20, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                ColdResistance, Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1000, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Sometimes)},
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Occasionally);

        public static readonly ActorVariant TrollRock = new ActorVariant(
            name: "rock troll", species: Species.Troll, noise: Grunt,
            initialLevel: 9, movementRate: 12, armorClass: 0, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1200, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Uncommonly);

        public static readonly ActorVariant TrollWater = new ActorVariant(
            name: "water troll", species: Species.Troll, noise: Grunt,
            initialLevel: 11, movementRate: 14, armorClass: 4, magicResistance: 40, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                Regeneration, Swimming, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1200, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Rarely);

        public static readonly ActorVariant TrollHai = new ActorVariant(
            name: "olog-hai", species: Species.Troll, noise: Grunt,
            initialLevel: 13, movementRate: 12, armorClass: -4, magicResistance: 40, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(
                Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1500, nutrition: 400,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Rarely);

        public static readonly ActorVariant HulkUmber = new ActorVariant(
            name: "umber hulk", species: Hulk, speciesClass: Hybrid,
            initialLevel: 9, movementRate: 6, armorClass: 2, magicResistance: 25,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 5),
                new Attack(Gaze, Confuse, diceCount: 1, diceSides: 8)
            },
            properties: Has(Tunneling, AnimalBody, ThickHide, Infravision, Infravisibility, Carnivorism),
            size: Large, weight: 1300, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Xorn = new ActorVariant(
            name: "xorn", species: Species.Xorn, noise: Roar,
            initialLevel: 8, movementRate: 9, armorClass: -2, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(
                FireResistance, ColdResistance, PoisonResistance, VenomResistance, SicknessResistance, StoningResistance,
                SlimingResistance, ThickHide, Phasing, Breathlessness, Metallivorism),
            size: Medium, weight: 1200, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Occasionally)},
            behavior: GoldCollector | GemCollector, generationFrequency: Occasionally);

        public static readonly ActorVariant Monkey = new ActorVariant(
            name: "monkey", species: Simian, noise: Growl,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, StealItem)
            },
            properties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 100, nutrition: 50,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Ape = new ActorVariant(
            name: "ape", species: Simian, noise: Growl,
            initialLevel: 4, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1100, nutrition: 500,
            generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly ActorVariant Owlbear = new ActorVariant(
            name: "owlbear", species: Simian, speciesClass: Hybrid, noise: Roar,
            initialLevel: 5, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Hug, Bind, diceCount: 1, diceSides: 4)
            },
            properties: Has(AnimalBody, Infravisibility, Humanoidness, Carnivorism),
            size: Large, weight: 1700, nutrition: 700,
            generationFrequency: Occasionally);

        public static readonly ActorVariant Yeti = new ActorVariant(
            name: "yeti", species: Simian, noise: Growl,
            initialLevel: 5, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(ColdResistance, AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1600, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly ActorVariant ApeCarnivorous = new ActorVariant(
            name: "carnivorous ape", species: Simian, noise: Growl, previousStage: Ape,
            initialLevel: 6, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(AnimalBody, Infravisibility, Humanoidness, Carnivorism),
            size: Medium, weight: 1250, nutrition: 550,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Sasquatch = new ActorVariant(
            name: "sasquatch", species: Simian, noise: Growl,
            initialLevel: 7, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1550, nutrition: 650,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemPaper = new ActorVariant(
            name: "paper golem", species: Golem,
            initialLevel: 2, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            properties: Has(
                WaterWeakness, ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 20)),
            size: Large, weight: 400, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemStraw = new ActorVariant(
            name: "straw golem", species: Golem,
            initialLevel: 3, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 20)),
            size: Large, weight: 400, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemRope = new ActorVariant(
            name: "rope golem", species: Golem,
            initialLevel: 4, movementRate: 12, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Hug, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 30)),
            size: Large, weight: 450, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemGold = new ActorVariant(
            name: "gold golem", species: Golem,
            initialLevel: 5, movementRate: 9, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(
                AcidResistance, ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness, Humanoidness,
                Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Medium, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemLeather = new ActorVariant(
            name: "leather golem", species: Golem,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Breathlessness, Mindlessness,
                Humanoidness, Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Large, weight: 800, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemWood = new ActorVariant(
            name: "wood golem", species: Golem,
            initialLevel: 7, movementRate: 3, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            properties: Has(
                SleepResistance, PoisonResistance, VenomResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness,
                Humanoidness, Asexuality).With(ActorProperty.Set(MaxHP, value: 50)),
            size: Large, weight: 1000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemFlesh = new ActorVariant(
            name: "flesh golem", species: Golem,
            initialLevel: 9, movementRate: 8, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties:
                Has(SleepResistance, PoisonResistance, Regeneration, Breathlessness, Mindlessness, Humanoidness,
                    Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Large, weight: 1400, nutrition: 600,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemClay = new ActorVariant(
            name: "clay golem", species: Golem,
            initialLevel: 11, movementRate: 7, armorClass: 7, magicResistance: 40,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10)
            },
            properties: Has(
                ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance, SicknessResistance,
                SlimingResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 50)),
            size: Large, weight: 1500, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemStone = new ActorVariant(
            name: "stone golem", species: Golem,
            initialLevel: 14, movementRate: 6, armorClass: 4, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10)
            },
            properties: Has(
                ColdResistance, FireResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide, NonAnimal,
                Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 60)),
            size: Large, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemGlass = new ActorVariant(
            name: "glass golem", species: Golem,
            initialLevel: 16, movementRate: 6, armorClass: 4, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            properties: Has(
                Reflection, AcidResistance, ColdResistance, FireResistance, ElectricityResistance, SleepResistance,
                PoisonResistance, VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide,
                NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 60)),
            size: Large, weight: 1800, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant GolemIron = new ActorVariant(
            name: "iron golem", species: Golem,
            initialLevel: 18, movementRate: 6, armorClass: 3, magicResistance: 60,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Breath, PoisonDamage, diceCount: 4, diceSides: 6, frequency: Sometimes)
            },
            properties: Has(
                WaterWeakness, ColdResistance, FireResistance, SleepResistance,
                PoisonResistance, VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide,
                NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 80)),
            size: Large, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly ActorVariant Doppelganger = new ActorVariant(
            name: "doppelganger", species: Species.Human, speciesClass: ShapeChanger, noise: Imitate,
            initialLevel: 9, movementRate: 12, armorClass: 5, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 12)
            },
            properties: Has(SleepResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PolymorphControl, Rarely)},
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Shopkeeper = new ActorVariant(
            name: "shopkeeper", species: Species.Human, noise: Sell,
            initialLevel: 12, movementRate: 18, armorClass: 0, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            properties: Has(SleepResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Doctor = new ActorVariant(
            name: "doctor", species: Species.Human, noise: ActorNoiseType.Doctor,
            initialLevel: 11, movementRate: 6, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Heal, diceCount: 2, diceSides: 6)
            },
            properties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful,
            generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly ActorVariant Priest = new ActorVariant(
            name: "aligned priest", species: Species.Human, noise: ActorNoiseType.Priest,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Spell, DivineSpell)
            },
            properties: Has(ElectricityResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant PriestHigh = new ActorVariant(
            name: "high priest", species: Species.Human, noise: ActorNoiseType.Priest,
            initialLevel: 25, movementRate: 16, armorClass: 7, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Spell, DivineSpell),
                new Attack(Spell, DivineSpell)
            },
            properties: Has(
                FireResistance, ElectricityResistance, SleepResistance, PoisonResistance, InvisibilityDetection,
                Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Prisoner = new ActorVariant(
            name: "prisoner", species: Species.Human, noise: ActorNoiseType.Prisoner,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Watchman = new ActorVariant(
            name: "watchman", species: Species.Human, noise: ActorNoiseType.Soldier,
            initialLevel: 6, movementRate: 10, armorClass: 10, magicResistance: 0, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant WatchCaptain = new ActorVariant(
            name: "watch captain", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Watchman,
            initialLevel: 10, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Guard = new ActorVariant(
            name: "guard", species: Species.Human, noise: ActorNoiseType.Guard,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 40, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Soldier = new ActorVariant(
            name: "soldier", species: Species.Human, noise: ActorNoiseType.Soldier,
            initialLevel: 6, movementRate: 10, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: SmallGroup | NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Sergeant = new ActorVariant(
            name: "sergeant", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Soldier,
            initialLevel: 8, movementRate: 10, armorClass: 10, magicResistance: 5, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: SmallGroup | NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Lieutenant = new ActorVariant(
            name: "lieutenant", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Sergeant,
            initialLevel: 10, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Captain = new ActorVariant(
            name: "captain", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Lieutenant,
            initialLevel: 12, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            properties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: Entourage | NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Oracle = new ActorVariant(
            name: "Oracle", species: Species.Human, noise: ActorNoiseType.Oracle,
            initialLevel: 12, movementRate: 0, armorClass: 0, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 1)
            },
            properties: Has(Infravisibility, Humanoidness, Femaleness),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: NonWandering | Peaceful,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Medusa = new ActorVariant(
            name: "Medusa", species: Species.Human, noise: Hiss,
            initialLevel: 20, movementRate: 12, armorClass: 2, magicResistance: 50, alignment: -15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 6),
                new Attack(Gaze, Stone),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(
                PoisonResistance, VenomResistance, StoningResistance, Flight, Amphibiousness, Infravisibility,
                Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: NonWandering | RangedPeaceful,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Rodney = new ActorVariant(
            name: "Wizard of Yendor", species: Species.Human, noise: Cuss,
            initialLevel: 30, movementRate: 12, armorClass: -8, magicResistance: 100,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 12),
                new Attack(Punch, StealAmulet),
                new Attack(Spell, ArcaneSpell)
            },
            properties: Has(
                FireResistance, PoisonResistance, Regeneration, EnergyRegeneration, Flight, Teleportation,
                TeleportationControl, MagicalBreathing, Infravisibility, InvisibilityDetection, Telepathy, Humanoidness,
                Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: NonWandering | RangedPeaceful | Covetous | MagicUser,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly ActorVariant Croesus = new ActorVariant(
            name: "Croesus", species: Species.Human, noise: ActorNoiseType.Guard,
            initialLevel: 20, movementRate: 15, armorClass: 0, magicResistance: 40, alignment: 15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10)
            },
            properties: Has(Infravisibility, InvisibilityDetection, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: NonWandering | Stalking | GoldCollector | GemCollector | WeaponCollector | MagicUser,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Sandestin = new ActorVariant(
            name: "sandestin", species: Species.Sandestin, speciesClass: ShapeChanger, noise: Cuss,
            initialLevel: 13, movementRate: 12, armorClass: 4, magicResistance: 60, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 3)
            },
            properties: Has(StoningResistance, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1500, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PolymorphControl, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector | AlignmentAware,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Rarely);

        public static readonly ActorVariant Djinni = new ActorVariant(
            name: "djinni", species: Species.Djinni, speciesClass: Demon, noise: ActorNoiseType.Djinni,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(StoningResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Humanoidness),
            size: Medium, weight: 1400, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly ActorVariant DemonWater = new ActorVariant(
            name: "water demon", species: DemonMajor, speciesClass: Demon, noise: ActorNoiseType.Djinni,
            initialLevel: 8, movementRate: 12, armorClass: -4, magicResistance: 30, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Swimming, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        // TODO: incubus
        public static readonly ActorVariant Succubus = new ActorVariant(
            name: "succubus", species: Species.Succubus, speciesClass: Demon, noise: Seduction,
            initialLevel: 6, movementRate: 12, armorClass: 0, magicResistance: 70, alignment: -9,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1400, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly ActorVariant DevilHorned = new ActorVariant(
            name: "horned devil", species: DemonMajor, speciesClass: Demon,
            initialLevel: 6, movementRate: 9, armorClass: -5, magicResistance: 50, alignment: -11,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 3),
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, ThickHide, Infravision, Infravisibility),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Hezrou = new ActorVariant(
            name: "hezrou", species: DemonMajor, speciesClass: Demon,
            initialLevel: 7, movementRate: 6, armorClass: -2, magicResistance: 55, alignment: -10,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility,
                Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Erinyes = new ActorVariant(
            name: "erinyes", species: DemonMajor, speciesClass: Demon,
            initialLevel: 7, movementRate: 12, armorClass: 2, magicResistance: 30, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility,
                Humanoidness),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DevilBarbed = new ActorVariant(
            name: "barbed devil", species: DemonMajor, speciesClass: Demon, previousStage: DevilHorned,
            initialLevel: 8, movementRate: 12, armorClass: 0, magicResistance: 35, alignment: -8,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Sting, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties:
                Has(FireResistance, PoisonResistance, SicknessResistance, ThickHide, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1200, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking,
            generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Vrock = new ActorVariant(
            name: "vrock", species: DemonMajor, speciesClass: Demon, previousStage: Erinyes,
            initialLevel: 8, movementRate: 12, armorClass: 0, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision,
                Infravisibility, Humanoidness),
            size: Large, weight: 1200, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Marilith = new ActorVariant(
            name: "marilith", species: DemonMajor, speciesClass: Demon, noise: Cuss, previousStage: Hezrou,
            initialLevel: 9, movementRate: 12, armorClass: -6, magicResistance: 80, alignment: -12,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision,
                InvisibilityDetection, Infravisibility, HumanoidTorso, SerpentlikeBody, Femaleness),
            size: Large, weight: 1200, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector,
            generationFlags: HellOnly | NonGenocidable, generationFrequency: Rarely);

        public static readonly ActorVariant DevilBone = new ActorVariant(
            name: "bone devil", species: DemonMajor, speciesClass: Demon,
            initialLevel: 9, movementRate: 15, armorClass: -1, magicResistance: 40, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 3),
                new Attack(Sting, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility,
                Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant DevilIce = new ActorVariant(
            name: "ice devil", species: DemonMajor, speciesClass: Demon, previousStage: DevilBone,
            initialLevel: 11, movementRate: 6, armorClass: -4, magicResistance: 55, alignment: -12,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Sting, ColdDamage, diceCount: 3, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(FireResistance, ColdResistance, PoisonResistance, SicknessResistance, Infravision,
                InvisibilityDetection, Infravisibility, Humanoidness),
            size: Large, weight: 1800, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Nalfeshnee = new ActorVariant(
            name: "nalfeshnee", species: DemonMajor, speciesClass: Demon, noise: Cast, previousStage: DevilBarbed,
            initialLevel: 11, movementRate: 9, armorClass: -4, magicResistance: 65, alignment: -11,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 8)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision,
                InvisibilityDetection, Infravisibility, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly ActorVariant FiendPit = new ActorVariant(
            name: "pit fiend", species: DemonMajor, speciesClass: Demon, noise: Growl, previousStage: Vrock,
            initialLevel: 13, movementRate: 6, armorClass: -3, magicResistance: 65, alignment: -13,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 2),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 2),
                new Attack(Hug, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 3)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision,
                InvisibilityDetection, Infravisibility, Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly ActorVariant Balrog = new ActorVariant(
            name: "balrog", species: DemonMajor, speciesClass: Demon, noise: Growl, previousStage: FiendPit,
            initialLevel: 16, movementRate: 5, armorClass: -2, magicResistance: 75, alignment: -14,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 8, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision,
                InvisibilityDetection, Infravisibility, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly ActorVariant Juiblex = new ActorVariant(
            name: "Juiblex", species: DemonMajor, speciesClass: Demon, noise: Gurgle,
            initialLevel: 50, movementRate: 3, armorClass: -7, magicResistance: 65, alignment: -15,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 8, diceSides: 4),
                new Attack(Digestion, AcidDamage, diceCount: 4, diceSides: 10),
                new Attack(Spit, AcidDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, StoningResistance, AcidResistance,
                Amphibiousness, Flight, Amorphism, Headlessness, Infravision, InvisibilityDetection, Maleness),
            size: Large, weight: 1500, nutrition: 400, consumptionProperties:
                new[] {WhenConsumedAdd(PoisonResistance, Sometimes), WhenConsumedAdd(AcidResistance, Sometimes)},
            corpse: None, behavior: NonWandering | Stalking | RangedPeaceful | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Yeenoghu = new ActorVariant(
            name: "Yeenoghu", species: DemonMajor, speciesClass: Demon, noise: Gurgle,
            initialLevel: 56, movementRate: 18, armorClass: -5, magicResistance: 80, alignment: -15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, Confuse, diceCount: 2, diceSides: 8),
                new Attack(Bite, Paralyze, diceCount: 1, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Infravision, InvisibilityDetection,
                Infravisibility, Maleness),
            size: Large, weight: 1500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking | WeaponCollector | MagicUser | Covetous ,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Orcus = new ActorVariant(
            name: "Orcus", species: DemonMajor, speciesClass: Demon, noise: Grunt,
            initialLevel: 66, movementRate: 9, armorClass: -6, magicResistance: 85, alignment: -20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Headbutt, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(Spell, ArcaneSpell, diceCount: 8, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8),
                new Attack(OnConsumption, Infect, diceCount: 3, diceSides: 8)
            },
            properties: Has(FireResistance, PoisonResistance, VenomResistance, SicknessResistance, Flight,
                Infravisibility,  Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | RangedPeaceful | Stalking | WeaponCollector | MagicUser | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Geryon = new ActorVariant(
            name: "Geryon", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 72, movementRate: 3, armorClass: -3, magicResistance: 75, alignment: -15,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Sting, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, SerpentlikeBody, HumanoidTorso, Maleness),
            size: Huge, weight: 2500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Dispater = new ActorVariant(
            name: "Dispater", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 78, movementRate: 15, armorClass: -2, magicResistance: 80, alignment: -15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, ArcaneSpell, diceCount: 6, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | Bribeable | Stalking | Covetous | WeaponCollector | MagicUser,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Baalzebub = new ActorVariant(
            name: "Baalzebub", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 89, movementRate: 9, armorClass: -5, magicResistance: 85, alignment: -20,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 6),
                new Attack(Gaze, Stun, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 6)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | RangedPeaceful | Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Asmodeus = new ActorVariant(
            name: "Asmodeus", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 105, movementRate: 12, armorClass: -7, magicResistance: 90, alignment: -20,
            attacks: new[]
            {
                new Attack(Claw, VenomDamage, diceCount: 4, diceSides: 4),
                new Attack(Spell, ColdDamage, diceCount: 6, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 8)
            },
            properties: Has(FireResistance, ColdResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] { WhenConsumedAdd(PoisonResistance, Sometimes) }, corpse: None,
            behavior: NonWandering | RangedPeaceful | Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Demogorgon = new ActorVariant(
            name: "Demogorgon", species: DemonMajor, speciesClass: Demon, noise: Growl,
            initialLevel: 106, movementRate: 15, armorClass: -8, magicResistance: 95, alignment: -20,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell, diceCount: 8, diceSides: 6),
                new Attack(Sting, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(Claw, Infect, diceCount: 1, diceSides: 6),
                new Attack(Claw, Infect, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 8),
                new Attack(OnConsumption, Infect, diceCount: 1, diceSides: 8)
            },
            properties: Has(FireResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Handlessness, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: NonWandering | Stalking | Covetous, generationFlags: HellOnly | NonGenocidable | NonPolymorphable,
            generationFrequency: Never);

        public static readonly ActorVariant Death = new ActorVariant(
            name: "Death", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Death, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Death, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Death, diceCount: 8, diceSides: 8)
            },
            properties: Has(AcidResistance, FireResistance, ColdResistance, ElectricityResistance,
                SleepResistance, PoisonResistance, VenomResistance, SicknessResistance, DecayResistance,
                StoningResistance, SlimingResistance, Breathlessness, Reanimation, Regeneration, Flight,
                TeleportationControl, PolymorphControl, Infravisibility, Infravision, InvisibilityDetection,
                Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, behavior: NonWandering | Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Pestilence = new ActorVariant(
            name: "Pestilence", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Pestilence, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Pestilence, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Pestilence, diceCount: 8, diceSides: 8)
            },
            properties: Has(AcidResistance, FireResistance, ColdResistance, ElectricityResistance,
                SleepResistance, PoisonResistance, VenomResistance, SicknessResistance, DecayResistance,
                StoningResistance, SlimingResistance, Breathlessness, Reanimation, Regeneration, Flight,
                TeleportationControl, PolymorphControl, Infravisibility, Infravision, InvisibilityDetection,
                Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, behavior: NonWandering | Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly ActorVariant Famine = new ActorVariant(
            name: "Famine", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Famine, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Famine, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Famine, diceCount: 8, diceSides: 8)
            },
            properties: Has(AcidResistance, FireResistance, ColdResistance, ElectricityResistance,
                SleepResistance, PoisonResistance, VenomResistance, SicknessResistance, DecayResistance,
                StoningResistance, SlimingResistance, Breathlessness, Reanimation, Regeneration, Flight,
                TeleportationControl, PolymorphControl, Infravisibility, Infravision, InvisibilityDetection,
                Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: -5000, behavior: NonWandering | Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        /*
     struct permonst {const char *name, char symbol, schar level, schar actionRate, schar ac, schar mr, schar alignment, ushort geno,
            struct attack attack[NATTK],
            ushort weight, ushort nutrition, uchar sound, uchar size, uchar resists, uchar conveys;
            unsigned long mflags1, unsigned long mflags2, unsigned long mflags3, uchar mcolor };
    Look for G_GENO, M2_WANDER, M2_HOSTILE,

        */

        protected ActorVariant(
            string name,
            Species species,
            byte initialLevel,
            byte movementRate,
            sbyte armorClass,
            byte magicResistance,
            IReadOnlyList<Attack> attacks,
            List<ActorProperty> properties,
            Size size,
            short weight,
            short nutrition,
            Frequency generationFrequency,
            ActorNoiseType noise = Silent,
            IReadOnlyList<Tuple<ActorProperty, Frequency>> consumptionProperties = null,
            SpeciesClass speciesClass = SpeciesClass.None,
            ActorVariant previousStage = null,
            ActorVariant corpse = null,
            sbyte alignment = 0,
            GenerationFlags generationFlags = GenerationFlags.None,
            MonsterBehavior behavior = MonsterBehavior.None)
        {
            Name = name;
            Species = species;
            SpeciesClass = speciesClass;
            PreviousStage = previousStage;
            if (previousStage != null)
            {
                Debug.Assert(previousStage.NextStage == null);
                Debug.Assert(previousStage != this);
                previousStage.NextStage = this;
            }
            Corpse = corpse;
            Alignment = alignment;
            Noise = noise;
            InitialLevel = initialLevel;
            MovementRate = movementRate;
            ArmorClass = armorClass;
            MagicResistance = magicResistance;
            Attacks = attacks;
            InnateProperties = properties;
            Weight = weight;
            Size = size;
            Nutrition = nutrition;
            ConsumptionProperties = consumptionProperties ?? new Tuple<ActorProperty, Frequency>[0];
            GenerationFlags = generationFlags;
            GenerationFrequency = generationFrequency;
            Behavior = behavior;

            Debug.Assert(!NameLookup.ContainsKey(name));
            NameLookup[name] = this;

            List<ActorVariant> actorTypes;
            if (!SpeciesLookup.TryGetValue(species, out actorTypes))
            {
                actorTypes = new List<ActorVariant>();
                SpeciesLookup[species] = actorTypes;
            }
            actorTypes.Add(this);

            if (!SpeciesCategoryLookup.TryGetValue(speciesClass, out actorTypes))
            {
                actorTypes = new List<ActorVariant>();
                SpeciesCategoryLookup[speciesClass] = actorTypes;
            }
            actorTypes.Add(this);
        }

        // Taxonomy
        public Species Species { get; }
        public SpeciesClass SpeciesClass { get; }
        public string Name { get; }
        public sbyte Alignment { get; }
        public ActorNoiseType Noise { get; }
        public ActorVariant PreviousStage { get; private set; }
        public ActorVariant NextStage { get; private set; }
        public ActorVariant Corpse { get; private set; }

        // Physical attributes
        public Size Size { get; }

        /// <summary> 100g units </summary>
        public short Weight { get; }

        public short Nutrition { get; }
        public IReadOnlyList<Tuple<ActorProperty, Frequency>> ConsumptionProperties { get; }

        // Combat properties
        public byte InitialLevel { get; }
        public byte MovementRate { get; }
        public sbyte ArmorClass { get; }

        /// <summary>
        ///     Base chance to avoid a magic attack
        /// </summary>
        public byte MagicResistance { get; }

        public IReadOnlyList<Attack> Attacks { get; }
        public IReadOnlyList<ActorProperty> InnateProperties { get; }

        // Generation
        public GenerationFlags GenerationFlags { get; }
        public Frequency GenerationFrequency { get; }

        public MonsterBehavior Behavior { get; }

        private static List<ActorProperty> Has(params SimpleActorPropertyType[] propertyTypes)
        {
            Debug.Assert(propertyTypes.Length > 0);

            var actorProperties = new List<ActorProperty>();
            foreach (var simpleActorPropertyType in propertyTypes)
            {
                actorProperties.Add(ActorProperty.Add(simpleActorPropertyType));
            }

            return actorProperties;
        }

        private static Tuple<ActorProperty, Frequency> WhenConsumedAdd(
            SimpleActorPropertyType propertyType, Frequency frequency)
        {
            return Tuple.Create(ActorProperty.Add(propertyType), frequency);
        }

        private static Tuple<ActorProperty, Frequency> WhenConsumedAdd<T>(
            ValuedActorPropertyType propertyType, T value, Frequency frequency)
        {
            return Tuple.Create(ActorProperty.Add(propertyType, value), frequency);
        }

        public static ActorVariant Get(string name)
        {
            return NameLookup[name];
        }

        public static IEnumerable<ActorVariant> Get(Species species)
        {
            return SpeciesLookup[species];
        }

        public static IEnumerable<ActorVariant> Get(SpeciesClass @class)
        {
            return SpeciesCategoryLookup[@class];
        }
    }
}
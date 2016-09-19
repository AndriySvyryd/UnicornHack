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
    public class MonsterVariant : ActorVariant
    {
        public static readonly MonsterVariant None = new MonsterVariant(
            name: "NONE", species: Species.Default,
            initialLevel: 1, movementRate: 0, armorClass: 0, magicResistance: 0,
            attacks: new Attack[0], innateProperties: null,
            size: Medium, weight: 0, nutrition: 0, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly MonsterVariant Lichen = new MonsterVariant(
            name: "lichen", species: Fungus,
            initialLevel: 1, movementRate: 1, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Stick),
                new Attack(OnMeleeHit, Stick)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 20, nutrition: 100,
            generationFrequency: Commonly);

        public static readonly MonsterVariant MoldBrown = new MonsterVariant(
            name: "brown mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, ColdDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness,
                Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Uncommonly)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant MoldYellow = new MonsterVariant(
            name: "yellow mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, Stun, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant MoldGreen = new MonsterVariant(
            name: "green mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                AcidResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, DecayResistance,
                Stealthiness, Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant MoldRed = new MonsterVariant(
            name: "red mold", species: Fungus,
            initialLevel: 1, movementRate: 0, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, FireDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, SleepResistance, PoisonResistance, VenomResistance, Stealthiness, DecayResistance,
                Breathlessness, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 50, nutrition: 30,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Uncommonly)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Shrieker = new MonsterVariant(
            name: "shrieker", species: Fungus,
            initialLevel: 3, movementRate: 1, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Scream, Deafen, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Breathlessness, NonAnimal, Eyelessness, Limblessness,
                Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Rarely)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant FungusViolet = new MonsterVariant(
            name: "violet fungus", species: Fungus,
            initialLevel: 3, movementRate: 1, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PoisonDamage, diceCount: 1, diceSides: 6),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Stealthiness, Breathlessness, NonAnimal, Eyelessness,
                Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Rarely)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant BlobAcid = new MonsterVariant(
            name: "acid blob", species: Blob,
            initialLevel: 1, movementRate: 3, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties:
                Has(SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance,
                    DecayResistance, Stealthiness, Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness,
                    Headlessness, Mindlessness, Asexuality, Metallivorism),
            size: Tiny, weight: 30, nutrition: 1,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            behavior: Wandering, generationFrequency: Usually);

        public static readonly MonsterVariant BlobQuivering = new MonsterVariant(
            name: "quivering blob", species: Blob,
            initialLevel: 5, movementRate: 1, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, DecayResistance, Stealthiness, Breathlessness,
                Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality),
            size: Small, weight: 200, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            behavior: Wandering, generationFrequency: Usually);

        public static readonly MonsterVariant CubeGelatinous = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, ColdResistance, ElectricityResistance, PoisonResistance, VenomResistance, AcidResistance,
                StoningResistance, SleepResistance, DecayResistance, Stealthiness, Breathlessness, NonAnimal,
                Eyelessness, Limblessness, Headlessness, Mindlessness, Omnivorism, Asexuality),
            size: Large, weight: 600, nutrition: 150,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Uncommonly)},
            generationFrequency: Commonly, behavior: WeaponCollector | Wandering);

        public static readonly MonsterVariant OozeGray = new MonsterVariant(
            name: "gray ooze", species: Ooze,
            initialLevel: 3, movementRate: 1, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 8),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(
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

        public static readonly MonsterVariant SlimeGreen = new MonsterVariant(
            name: "green slime", species: Ooze,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Slime),
                new Attack(OnMeleeHit, Slime),
                new Attack(OnConsumption, Slime)
            },
            innateProperties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism),
            size: Medium, weight: 400, nutrition: 150, corpse: None,
            generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly MonsterVariant PuddingBrown = new MonsterVariant(
            name: "brown pudding", species: Pudding,
            initialLevel: 5, movementRate: 3, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, VenomDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, VenomDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, VenomDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism, Reanimation),
            size: Medium, weight: 512, nutrition: 256, consumptionProperties: new[]
            {
                WhenConsumedAdd(ColdResistance, Uncommonly), WhenConsumedAdd(ElectricityResistance, Uncommonly),
                WhenConsumedAdd(DecayResistance, Uncommonly)
            },
            generationFrequency: Rarely);

        public static readonly MonsterVariant PuddingBlack = new MonsterVariant(
            name: "black pudding", species: Pudding,
            initialLevel: 10, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, AcidDamage, diceCount: 3, diceSides: 8),
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 3, diceSides: 8)
            },
            innateProperties: Has(
                ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance,
                AcidResistance, StoningResistance, DecayResistance, Stealthiness, Breathlessness, Amorphism,
                NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, Omnivorism, Reanimation),
            size: Medium, weight: 512, nutrition: 256, consumptionProperties: new[]
            {
                WhenConsumedAdd(ColdResistance, Uncommonly), WhenConsumedAdd(ElectricityResistance, Uncommonly),
                WhenConsumedAdd(AcidResistance, Uncommonly)
            },
            generationFrequency: Rarely);

        public static readonly MonsterVariant JellyBlue = new MonsterVariant(
            name: "blue jelly", species: Jelly,
            initialLevel: 4, movementRate: 0, armorClass: 8, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, ColdDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, ColdResistance, Stealthiness, Breathlessness,
                Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Uncommonly)},
            generationFrequency: Commonly);

        public static readonly MonsterVariant JellySpotted = new MonsterVariant(
            name: "spotted jelly", species: Jelly,
            initialLevel: 5, movementRate: 0, armorClass: 8, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, AcidDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance, Stealthiness,
                Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            generationFrequency: Commonly);

        public static readonly MonsterVariant JellyOchre = new MonsterVariant(
            name: "ochre jelly", species: Jelly,
            initialLevel: 6, movementRate: 3, armorClass: 8, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 3),
                new Attack(Digestion, AcidDamage, diceCount: 3, diceSides: 6),
                new Attack(OnMeleeHit, AcidDamage, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, AcidDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, AcidResistance, StoningResistance, Stealthiness,
                Breathlessness, Amorphism, NonAnimal, Eyelessness, Limblessness, Headlessness, Mindlessness, Asexuality,
                NoInventory),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Uncommonly)},
            generationFrequency: Commonly);

        public static readonly MonsterVariant WormLongBaby = new MonsterVariant(
            name: "baby long worm", species: Worm,
            initialLevel: 2, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties:
                Has(PoisonResistance, SerpentlikeBody, Stealthiness, Eyelessness, Limblessness, Carnivorism,
                    NoInventory),
            size: Medium, weight: 600, nutrition: 250,
            generationFrequency: Commonly);

        public static readonly MonsterVariant WormLong = new MonsterVariant(
            name: "long worm", species: Worm,
            initialLevel: 9, movementRate: 3, armorClass: 5, magicResistance: 10, previousStage: WormLongBaby,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties:
                Has(PoisonResistance, SerpentlikeBody, Eyelessness, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Gigantic, weight: 1500, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly MonsterVariant WormLongTail = new MonsterVariant(
            name: "long worm tail", species: Worm,
            initialLevel: 1, movementRate: 0, armorClass: 0, magicResistance: 0,
            attacks: new List<Attack>(),
            innateProperties: new List<ActorProperty>(),
            size: Gigantic, weight: 0, nutrition: 0,
            generationFlags: NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant WormPurpleBaby = new MonsterVariant(
            name: "baby purple worm", species: Worm,
            initialLevel: 4, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(PoisonResistance, SerpentlikeBody, Stealthiness, Eyelessness, Limblessness, Carnivorism,
                    NoInventory),
            size: Medium, weight: 600, nutrition: 250,
            generationFrequency: Commonly);

        public static readonly MonsterVariant WormPurple = new MonsterVariant(
            name: "purple worm", species: Worm,
            initialLevel: 15, movementRate: 9, armorClass: 5, magicResistance: 20, previousStage: WormPurpleBaby,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, Engulf, diceCount: 2, diceSides: 6),
                new Attack(Digestion, AcidDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties:
                Has(PoisonResistance, SerpentlikeBody, Eyelessness, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Gigantic, weight: 1500, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly MonsterVariant BugLighting = new MonsterVariant(
            name: "lightning bug", species: Beetle, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, ElectricityDamage, diceCount: 1, diceSides: 1),
                new Attack(OnConsumption, ElectricityDamage, diceCount: 1, diceSides: 1)
            },
            innateProperties: Has(ElectricityResistance, Flight, AnimalBody, Handlessness, Herbivorism),
            size: Tiny, weight: 10, nutrition: 10,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant FireFly = new MonsterVariant(
            name: "firefly", species: Beetle, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, FireDamage, diceCount: 1, diceSides: 1),
                new Attack(OnConsumption, FireDamage, diceCount: 1, diceSides: 1)
            },
            innateProperties: Has(Flight, Infravisibility, AnimalBody, Handlessness, Herbivorism),
            size: Tiny, weight: 10, nutrition: 10,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant BeetleGiant = new MonsterVariant(
            name: "giant beetle", species: Beetle, speciesClass: Insect,
            initialLevel: 5, movementRate: 6, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(PoisonResistance, AnimalBody, Handlessness, Carnivorism),
            size: Tiny, weight: 10, nutrition: 10,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant SpiderCave = new MonsterVariant(
            name: "cave spider", species: Spider, speciesClass: Insect,
            initialLevel: 1, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Concealment, Clinginess, AnimalBody, Handlessness, Carnivorism,
                Oviparity),
            size: Small, weight: 50, nutrition: 25,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly MonsterVariant SpiderGiant = new MonsterVariant(
            name: "giant spider", species: Spider, speciesClass: Insect,
            initialLevel: 5, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, DrainStrength, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Clinginess, AnimalBody, Handlessness, Carnivorism, Oviparity),
            size: Medium, weight: 150, nutrition: 50,
            generationFrequency: Usually);

        // TODO: add more spiders

        public static readonly MonsterVariant Centipede = new MonsterVariant(
            name: "centipede", species: Species.Centipede, speciesClass: Insect,
            initialLevel: 2, movementRate: 4, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Concealment, Clinginess, AnimalBody, Handlessness, Carnivorism,
                Oviparity),
            size: Tiny, weight: 50, nutrition: 25,
            generationFrequency: Usually);

        public static readonly MonsterVariant ScorpionLarge = new MonsterVariant(
            name: "large scorpion", species: Scorpion, speciesClass: Insect,
            initialLevel: 5, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Concealment, AnimalBody, Handlessness, Carnivorism, Oviparity),
            size: Small, weight: 150, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Sometimes)},
            generationFrequency: Commonly);

        public static readonly MonsterVariant AntGiant = new MonsterVariant(
            name: "giant ant", species: Ant, speciesClass: Insect,
            initialLevel: 2, movementRate: 18, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Stealthiness, Handlessness, Carnivorism, Asexuality),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: null,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant AntSoldier = new MonsterVariant(
            name: "soldier ant", species: Ant, speciesClass: Insect,
            initialLevel: 3, movementRate: 18, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Sting, VenomDamage, diceCount: 3, diceSides: 4, frequency: Sometimes),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(PoisonResistance, AnimalBody, Stealthiness, Handlessness, Carnivorism, Asexuality),
            size: Tiny, weight: 20, nutrition: 5,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant AntFire = new MonsterVariant(
            name: "fire ant", species: Ant, speciesClass: Insect,
            initialLevel: 3, movementRate: 18, armorClass: 3, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, FireDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, FireDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, Stealthiness, Infravisibility, SlimingResistance, AnimalBody, Handlessness, Carnivorism,
                Asexuality),
            size: Tiny, weight: 30, nutrition: 10,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Occasionally)},
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant AntQueen = new MonsterVariant(
            name: "ant queen", species: Ant, speciesClass: Insect,
            initialLevel: 9, movementRate: 18, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 9)
            },
            innateProperties: Has(
                PoisonResistance, AnimalBody, Stealthiness, Handlessness, Carnivorism, Femaleness, Oviparity),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: null,
            generationFlags: Entourage, generationFrequency: Rarely);

        public static readonly MonsterVariant BeeKiller = new MonsterVariant(
            name: "killer bee", species: Bee, speciesClass: Insect, noise: Buzz,
            initialLevel: 1, movementRate: 18, armorClass: -1, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(PoisonResistance, Flight, AnimalBody, Handlessness, Femaleness),
            size: Tiny, weight: 5, nutrition: 5,
            generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant BeeQueen = new MonsterVariant(
            name: "queen bee", species: Bee, speciesClass: Insect, noise: Buzz,
            initialLevel: 9, movementRate: 24, armorClass: -4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Sting, VenomDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 15)
            },
            innateProperties: Has(PoisonResistance, Flight, AnimalBody, Handlessness, Femaleness),
            size: Tiny, weight: 5, nutrition: 5,
            generationFlags: Entourage, generationFrequency: Rarely);

        public static readonly MonsterVariant Xan = new MonsterVariant(
            name: "xan", species: Species.Xan, speciesClass: Insect, noise: Buzz,
            initialLevel: 7, movementRate: 18, armorClass: -2, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Sting, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Sting, DamageLeg),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(PoisonResistance, Flight, AnimalBody, Handlessness),
            size: Tiny, weight: 1, nutrition: 1,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Jellyfish = new MonsterVariant(
            name: "jellyfish", species: Species.Jellyfish,
            initialLevel: 3, movementRate: 3, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Sting, VenomDamage, diceCount: 3, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(Swimming, WaterBreathing, Limblessness, NoInventory),
            size: Small, weight: 80, nutrition: 20,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Kraken = new MonsterVariant(
            name: "kraken", species: Squid,
            initialLevel: 20, movementRate: 3, armorClass: 6, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 5, diceSides: 4),
                new Attack(Hug, Bind, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Swimming, WaterBreathing, Limblessness, Oviparity, Carnivorism),
            size: Huge, weight: 2000, nutrition: 1000,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Piranha = new MonsterVariant(
            name: "piranha", species: Fish,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Swimming, WaterBreathing, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Tiny, weight: 60, nutrition: 30,
            generationFlags: SmallGroup, generationFrequency: Never);

        public static readonly MonsterVariant Shark = new MonsterVariant(
            name: "shark", species: Fish,
            initialLevel: 7, movementRate: 12, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 5, diceSides: 6)
            },
            innateProperties:
                Has(Swimming, ThickHide, WaterBreathing, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Large, weight: 1000, nutrition: 400,
            generationFrequency: Never);

        public static readonly MonsterVariant EelGiant = new MonsterVariant(
            name: "giant eel", species: Eel,
            initialLevel: 5, movementRate: 9, armorClass: -1, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Hug, Bind, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Swimming, WaterBreathing, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Never);

        public static readonly MonsterVariant EelElectric = new MonsterVariant(
            name: "electric eel", species: Eel, previousStage: EelGiant,
            initialLevel: 7, movementRate: 10, armorClass: -3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, ElectricityDamage, diceCount: 4, diceSides: 6),
                new Attack(Hug, Bind, diceCount: 3, diceSides: 6)
            },
            innateProperties:
                Has(ElectricityResistance, Swimming, WaterBreathing, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Large, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Sometimes)},
            generationFrequency: Never);

        public static readonly MonsterVariant SnakeGarter = new MonsterVariant(
            name: "garter snake", species: Species.Snake, noise: Hiss,
            initialLevel: 1, movementRate: 8, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Tiny, weight: 50, nutrition: 25,
            generationFlags: LargeGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant Snake = new MonsterVariant(
            name: "snake", species: Species.Snake, noise: Hiss,
            initialLevel: 4, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Oviparity, Carnivorism, NoInventory),
            size: Small, weight: 100, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant WaterMoccasin = new MonsterVariant(
            name: "water moccasin", species: Species.Snake, noise: Hiss,
            initialLevel: 4, movementRate: 15, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Oviparity, Carnivorism, NoInventory),
            size: Small, weight: 150, nutrition: 75,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFlags: LargeGroup, generationFrequency: Never);

        public static readonly MonsterVariant Python = new MonsterVariant(
            name: "python", species: Species.Snake, noise: Hiss,
            initialLevel: 6, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Hug, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Hug, Bind, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                Swimming, Infravision, SerpentlikeBody, Limblessness, Oviparity, Carnivorism, NoInventory),
            size: Medium, weight: 250, nutrition: 125,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant PitViper = new MonsterVariant(
            name: "pit viper", species: Species.Snake, noise: Hiss,
            initialLevel: 6, movementRate: 15, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Oviparity, Carnivorism, NoInventory),
            size: Medium, weight: 100, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Cobra = new MonsterVariant(
            name: "cobra", species: Species.Snake, noise: Hiss,
            initialLevel: 7, movementRate: 18, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(Spit, Blind, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Swimming, Concealment, Infravision, SerpentlikeBody, Limblessness,
                Oviparity, Carnivorism, NoInventory),
            size: Medium, weight: 250, nutrition: 100,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Newt = new MonsterVariant(
            name: "newt", species: Species.Lizard,
            initialLevel: 1, movementRate: 6, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                Swimming, Amphibiousness, Handlessness, Oviparity, Carnivorism, SingularInventory),
            size: Tiny, weight: 10, nutrition: 10,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Gecko = new MonsterVariant(
            name: "gecko", species: Species.Lizard,
            initialLevel: 2, movementRate: 6, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(Handlessness, Oviparity, Carnivorism, SingularInventory),
            size: Tiny, weight: 15, nutrition: 15,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Iguana = new MonsterVariant(
            name: "iguana", species: Species.Lizard,
            initialLevel: 3, movementRate: 6, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(Handlessness, Oviparity, Carnivorism, SingularInventory),
            size: Small, weight: 50, nutrition: 50,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Lizard = new MonsterVariant(
            name: "lizard", species: Species.Lizard,
            initialLevel: 4, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Handlessness, Oviparity, Carnivorism, SingularInventory),
            size: Small, weight: 50, nutrition: 50,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Chameleon = new MonsterVariant(
            name: "chameleon", species: Species.Lizard, speciesClass: ShapeChanger,
            initialLevel: 5, movementRate: 6, armorClass: 6, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 2)
            },
            innateProperties: Has(Handlessness, PolymorphControl, Oviparity, Carnivorism, SingularInventory),
            size: Small, weight: 50, nutrition: 50,
            generationFlags: NonPolymorphable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant CrocodileBaby = new MonsterVariant(
            name: "baby crocodile", species: Species.Crocodile,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 2)
            },
            innateProperties: Has(Swimming, Amphibiousness, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 200, nutrition: 200,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Crocodile = new MonsterVariant(
            name: "crocodile", species: Species.Crocodile, previousStage: CrocodileBaby,
            initialLevel: 8, movementRate: 9, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties:
                Has(Swimming, Amphibiousness, ThickHide, Handlessness, Oviparity, Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Chickatrice = new MonsterVariant(
            name: "chickatrice", species: Species.Cockatrice, speciesClass: Hybrid, noise: Hiss,
            initialLevel: 4, movementRate: 4, armorClass: 8, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Touch, Stone, frequency: Sometimes),
                new Attack(OnMeleeHit, Stone),
                new Attack(OnConsumption, Stone)
            },
            innateProperties: Has(
                PoisonResistance, StoningResistance, AnimalBody, Infravisibility, Handlessness, Omnivorism,
                SingularInventory),
            size: Tiny, weight: 10, nutrition: 10, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Sometimes),
                WhenConsumedAdd(StoningResistance, Sometimes)
            },
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant Cockatrice = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, StoningResistance, AnimalBody, Infravisibility, Handlessness, Oviparity, Omnivorism,
                SingularInventory),
            size: Small, weight: 30, nutrition: 30, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Sometimes),
                WhenConsumedAdd(StoningResistance, Sometimes)
            },
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant Pyrolisk = new MonsterVariant(
            name: "pyrolisk", species: Species.Cockatrice, speciesClass: Hybrid, noise: Hiss,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Gaze, FireDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, FireResistance, AnimalBody, Infravisibility, Handlessness, Oviparity, Omnivorism,
                SingularInventory),
            size: Small, weight: 30, nutrition: 30, consumptionProperties: new[]
            {
                WhenConsumedAdd(PoisonResistance, Uncommonly),
                WhenConsumedAdd(FireResistance, Sometimes)
            }, generationFrequency: Sometimes);

        public static readonly MonsterVariant Magpie = new MonsterVariant(
            name: "magpie", species: Crow, speciesClass: Bird, noise: Squawk,
            initialLevel: 2, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Infravisibility, AnimalBody, Handlessness, Oviparity, Omnivorism,
                SingularInventory),
            size: Tiny, weight: 50, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            behavior: GemCollector | Wandering, generationFrequency: Sometimes);

        public static readonly MonsterVariant Raven = new MonsterVariant(
            name: "raven", species: Crow, speciesClass: Bird, noise: Squawk,
            initialLevel: 4, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, Blind, diceCount: 3, diceSides: 8)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Infravisibility, AnimalBody, Handlessness, Oviparity, Omnivorism,
                SingularInventory),
            size: Tiny, weight: 100, nutrition: 40,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Uncommonly)},
            behavior: Wandering, generationFrequency: Sometimes);

        public static readonly MonsterVariant Bat = new MonsterVariant(
            name: "bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek,
            initialLevel: 1, movementRate: 22, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties:
                Has(PoisonResistance, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness, Carnivorism,
                    SingularInventory),
            size: Tiny, weight: 50, nutrition: 20,
            behavior: Wandering, generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant BatGiant = new MonsterVariant(
            name: "giant bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek, previousStage: Bat,
            initialLevel: 2, movementRate: 22, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 5)
            },
            innateProperties:
                Has(PoisonResistance, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness, Carnivorism,
                    SingularInventory),
            size: Tiny, weight: 100, nutrition: 40,
            behavior: Wandering, generationFrequency: Commonly);

        public static readonly MonsterVariant BatVampire = new MonsterVariant(
            name: "vampire bat", species: Species.Bat, speciesClass: Bird, noise: Sqeek, previousStage: BatGiant,
            initialLevel: 5, movementRate: 20, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, DrainStrength, diceCount: 1, diceSides: 1)
            },
            innateProperties:
                Has(PoisonResistance, Regeneration, Flight, Stealthiness, Infravisibility, AnimalBody, Handlessness,
                    Carnivorism, SingularInventory),
            size: Tiny, weight: 100, nutrition: 40,
            behavior: Wandering, generationFrequency: Commonly);

        public static readonly MonsterVariant RatSewer = new MonsterVariant(
            name: "sewer rat", species: Rat, speciesClass: Rodent, noise: Sqeek,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 100, nutrition: 50,
            generationFlags: SmallGroup, generationFrequency: Often);

        public static readonly MonsterVariant RatGiant = new MonsterVariant(
            name: "giant rat", species: Rat, speciesClass: Rodent, noise: Sqeek, previousStage: RatSewer,
            initialLevel: 2, movementRate: 10, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 150, nutrition: 75,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly MonsterVariant RatRabid = new MonsterVariant(
            name: "rabid rat", species: Rat, speciesClass: Rodent, noise: Sqeek,
            initialLevel: 3, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, DrainConstitution, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 150, nutrition: 50,
            generationFlags: SmallGroup, generationFrequency: Usually);

        public static readonly MonsterVariant RatWere = new MonsterVariant(
            name: "ratwere", species: Rat, speciesClass: Rodent | ShapeChanger, noise: Sqeek,
            initialLevel: 3, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties: Has(
                PoisonResistance, Regeneration, AnimalBody, Infravisibility, Handlessness, Carnivorism,
                SingularInventory),
            size: Small, weight: 150, nutrition: 50, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Wererat = new MonsterVariant(
            name: "wererat", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 3, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, RatWere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant MoleRock = new MonsterVariant(
            name: "rock mole", species: Mole, speciesClass: Rodent,
            initialLevel: 3, movementRate: 3, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(Tunneling, AnimalBody, Infravisibility, Handlessness, Metallivorism, SingularInventory),
            size: Small, weight: 100, nutrition: 50,
            behavior: GoldCollector | GemCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant Woodchuck = new MonsterVariant(
            name: "woodchuck", species: Species.Woodchuck, speciesClass: Rodent,
            initialLevel: 3, movementRate: 3, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Swimming, AnimalBody, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Small, weight: 100, nutrition: 50,
            behavior: GoldCollector | GemCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant CatSmall = new MonsterVariant(
            name: "kitten", species: Cat, speciesClass: Feline, noise: Mew,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 150, nutrition: 100,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant CatMedium = new MonsterVariant(
            name: "housecat", species: Cat, speciesClass: Feline, noise: Mew, previousStage: CatSmall,
            initialLevel: 4, movementRate: 16, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 200, nutrition: 150,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant CatLarge = new MonsterVariant(
            name: "large cat", species: Cat, speciesClass: Feline, noise: Bark, previousStage: CatMedium,
            initialLevel: 6, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 250, nutrition: 200,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant Lynx = new MonsterVariant(
            name: "lynx", species: Cat, speciesClass: Feline, noise: Growl,
            initialLevel: 5, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 400, nutrition: 200,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Jaguar = new MonsterVariant(
            name: "jaguar", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 4, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Panther = new MonsterVariant(
            name: "panther", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 6, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Tiger = new MonsterVariant(
            name: "tiger", species: BigCat, speciesClass: Feline, noise: Growl,
            initialLevel: 6, movementRate: 14, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Large, weight: 600, nutrition: 300,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Fox = new MonsterVariant(
            name: "fox", species: Species.Fox, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 15, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 300, nutrition: 250,
            generationFrequency: Often);

        public static readonly MonsterVariant Coyote = new MonsterVariant(
            name: "coyote", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 300, nutrition: 250,
            generationFrequency: Usually);

        public static readonly MonsterVariant Jackal = new MonsterVariant(
            name: "jackal", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 1, movementRate: 12, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 300, nutrition: 250,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant Jackalwere = new MonsterVariant(
            name: "jackalwere", species: Dog, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 2, movementRate: 12, armorClass: 7, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties:
                Has(PoisonResistance, AnimalBody, Regeneration, Infravisibility, Handlessness, Carnivorism,
                    SingularInventory),
            size: Small, weight: 300, nutrition: 250, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Werejackal = new MonsterVariant(
            name: "werejackal", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 2, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, Jackalwere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant DogSmall = new MonsterVariant(
            name: "little dog", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 150, nutrition: 100,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant DogMedium = new MonsterVariant(
            name: "dog", species: Dog, speciesClass: Canine, noise: Bark, previousStage: DogSmall,
            initialLevel: 4, movementRate: 16, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 400, nutrition: 300,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant DogLarge = new MonsterVariant(
            name: "large dog", species: Dog, speciesClass: Canine, noise: Bark, previousStage: DogMedium,
            initialLevel: 6, movementRate: 15, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 600, nutrition: 400,
            behavior: Domesticable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant Dingo = new MonsterVariant(
            name: "dingo", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 4, movementRate: 16, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 400, nutrition: 200,
            generationFrequency: Usually);

        public static readonly MonsterVariant Barghest = new MonsterVariant(
            name: "barghest", species: Dog, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 9, movementRate: 16, armorClass: 2, magicResistance: 20, alignment: -6,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 1200, nutrition: 500,
            behavior: Mountable | AlignmentAware, generationFrequency: Uncommonly);

        public static readonly MonsterVariant HellHoundCub = new MonsterVariant(
            name: "hell hound pup", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Breath, FireDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties:
                Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 250, nutrition: 200,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Commonly)},
            generationFlags: HellOnly | SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant HellHound = new MonsterVariant(
            name: "hell hound", species: Dog, speciesClass: Canine, noise: Bark, previousStage: HellHoundCub,
            initialLevel: 12, movementRate: 14, armorClass: 2, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Breath, FireDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties:
                Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 700, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Commonly)},
            generationFlags: HellOnly, generationFrequency: Usually);

        public static readonly MonsterVariant Cerberus = new MonsterVariant(
            name: "Cerberus", species: Dog, speciesClass: Canine, noise: Bark,
            initialLevel: 13, movementRate: 10, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties:
                Has(FireResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, Maleness, SingularInventory),
            size: Large, weight: 1000, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Often)},
            generationFlags: HellOnly | NonPolymorphable, generationFrequency: Once);

        public static readonly MonsterVariant Wolf = new MonsterVariant(
            name: "wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 500, nutrition: 250,
            generationFlags: SmallGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant WolfDire = new MonsterVariant(
            name: "dire wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 7, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 1200, nutrition: 500,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant Warg = new MonsterVariant(
            name: "warg", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 8, movementRate: 12, armorClass: 3, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Large, weight: 1400, nutrition: 600,
            behavior: Mountable, generationFrequency: Commonly);

        public static readonly MonsterVariant Wolfwere = new MonsterVariant(
            name: "wolfwere", species: Species.Wolf, speciesClass: Canine | ShapeChanger, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties:
                Has(PoisonResistance, Regeneration, AnimalBody, Infravisibility, Handlessness, Carnivorism,
                    SingularInventory),
            size: Medium, weight: 500, nutrition: 250, corpse: None,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Werewolf = new MonsterVariant(
            name: "werewolf", species: Species.Human, speciesClass: ShapeChanger, noise: Lycanthrope,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, ConferLycanthropy, frequency: Sometimes),
                new Attack(OnConsumption, ConferLycanthropy)
            },
            innateProperties: Has(PoisonResistance, Regeneration, Infravisibility, Humanoidness, Omnivorism)
                .With(ActorProperty.Add(Lycanthropy, Wolfwere.Name)),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant WolfWinterCub = new MonsterVariant(
            name: "winter wolf cub", species: Species.Wolf, speciesClass: Canine, noise: Bark,
            initialLevel: 5, movementRate: 12, armorClass: 4, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Breath, ColdDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties:
                Has(ColdResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Small, weight: 250, nutrition: 200,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly MonsterVariant WolfWinter = new MonsterVariant(
            name: "winter wolf", species: Species.Wolf, speciesClass: Canine, noise: Bark, previousStage: WolfWinterCub,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Breath, ColdDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties:
                Has(ColdResistance, AnimalBody, Infravisibility, Handlessness, Carnivorism, SingularInventory),
            size: Medium, weight: 700, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly MonsterVariant Pony = new MonsterVariant(
            name: "pony", species: Species.Horse, speciesClass: Equine, noise: Neigh,
            initialLevel: 3, movementRate: 16, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Medium, weight: 1300, nutrition: 900,
            behavior: Domesticable | Mountable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant Horse = new MonsterVariant(
            name: "horse", species: Species.Horse, speciesClass: Equine, noise: Neigh, previousStage: Pony,
            initialLevel: 5, movementRate: 20, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 1100,
            behavior: Domesticable | Mountable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant Warhorse = new MonsterVariant(
            name: "warhorse", species: Species.Horse, speciesClass: Equine, noise: Neigh, previousStage: Horse,
            initialLevel: 7, movementRate: 24, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 1800, nutrition: 1300,
            behavior: Domesticable | Mountable | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant UnicornWhite = new MonsterVariant(
            name: "white unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh, alignment: 7,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful | Wandering, generationFrequency: Usually);

        public static readonly MonsterVariant UnicornGray = new MonsterVariant(
            name: "gray unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful | Wandering, generationFrequency: Usually);

        public static readonly MonsterVariant UnicornBlack = new MonsterVariant(
            name: "black unicorn", species: Unicorn, speciesClass: Equine, noise: Neigh, alignment: -7,
            initialLevel: 4, movementRate: 24, armorClass: 2, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 12),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(PoisonResistance, VenomResistance, AnimalBody, Infravisibility, Handlessness, Herbivorism),
            size: Large, weight: 1300, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: GemCollector | AlignmentAware | RangedPeaceful | Wandering, generationFrequency: Usually);

        public static readonly MonsterVariant Rothe = new MonsterVariant(
            name: "rothe", species: Quadruped, noise: Bleat,
            initialLevel: 2, movementRate: 9, armorClass: 7, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(Blindness, AnimalBody, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Medium, weight: 600, nutrition: 400,
            generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant Mumak = new MonsterVariant(
            name: "mumak", species: Quadruped, noise: Roar,
            initialLevel: 5, movementRate: 9, armorClass: 0, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 2, diceSides: 12),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, ThickHide, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 2500, nutrition: 1000,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Leocrotta = new MonsterVariant(
            name: "leocrotta", species: Quadruped, noise: Imitate,
            initialLevel: 6, movementRate: 18, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Handlessness, Omnivorism, SingularInventory),
            size: Large, weight: 1200, nutrition: 500,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Wumpus = new MonsterVariant(
            name: "wumpus", species: Quadruped, noise: Burble,
            initialLevel: 8, movementRate: 3, armorClass: 2, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties: Has(Clinginess, AnimalBody, Infravisibility, Handlessness, Omnivorism, SingularInventory),
            size: Large, weight: 2500, nutrition: 500,
            generationFrequency: Usually);

        public static readonly MonsterVariant Brontotheres = new MonsterVariant(
            name: "brontotheres", species: Quadruped, noise: Roar,
            initialLevel: 12, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 2650, nutrition: 650,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Baluchitherium = new MonsterVariant(
            name: "baluchitherium", species: Quadruped, noise: Roar,
            initialLevel: 14, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 5, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 5, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 3800, nutrition: 800,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Mastodon = new MonsterVariant(
            name: "mastodon", species: Quadruped, noise: Roar,
            initialLevel: 20, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Headbutt, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Headbutt, PhysicalDamage, diceCount: 4, diceSides: 8)
            },
            innateProperties: Has(AnimalBody, ThickHide, Infravisibility, Handlessness, Herbivorism, SingularInventory),
            size: Large, weight: 3800, nutrition: 800,
            generationFrequency: Usually);

        public static readonly MonsterVariant Monkey = new MonsterVariant(
            name: "monkey", species: Simian, noise: Growl,
            initialLevel: 2, movementRate: 18, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, StealItem)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 100, nutrition: 50,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Ape = new MonsterVariant(
            name: "ape", species: Simian, noise: Growl,
            initialLevel: 4, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1100, nutrition: 500,
            generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant Owlbear = new MonsterVariant(
            name: "owlbear", species: Simian, speciesClass: Hybrid, noise: Roar,
            initialLevel: 5, movementRate: 12, armorClass: 5, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Hug, Bind, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Humanoidness, Carnivorism),
            size: Large, weight: 1700, nutrition: 700,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant Yeti = new MonsterVariant(
            name: "yeti", species: Simian, noise: Growl,
            initialLevel: 5, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(ColdResistance, AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1600, nutrition: 700,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ApeCarnivorous = new MonsterVariant(
            name: "carnivorous ape", species: Simian, noise: Growl, previousStage: Ape,
            initialLevel: 6, movementRate: 12, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Humanoidness, Carnivorism),
            size: Medium, weight: 1250, nutrition: 550,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Sasquatch = new MonsterVariant(
            name: "sasquatch", species: Simian, noise: Growl,
            initialLevel: 7, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(AnimalBody, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1550, nutrition: 650,
            generationFrequency: Rarely);

        public static readonly MonsterVariant Xorn = new MonsterVariant(
            name: "xorn", species: Species.Xorn, noise: Roar,
            initialLevel: 8, movementRate: 9, armorClass: -2, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, PoisonResistance, VenomResistance, SicknessResistance, StoningResistance,
                SlimingResistance, ThickHide, Phasing, Breathlessness, Metallivorism),
            size: Medium, weight: 1200, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Occasionally)},
            behavior: GoldCollector | GemCollector, generationFrequency: Occasionally);

        public static readonly MonsterVariant LurkerAbove = new MonsterVariant(
            name: "lurker above", species: Species.Trapper,
            initialLevel: 10, movementRate: 3, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, Suffocate)
            },
            innateProperties: Has(
                Flight, Camouflage, AnimalBody, Stealthiness, Eyelessness, Headlessness, Limblessness, Clinginess,
                Carnivorism),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly MonsterVariant Trapper = new MonsterVariant(
            name: "trapper", species: Species.Trapper,
            initialLevel: 12, movementRate: 3, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 4),
                new Attack(Digestion, Suffocate)
            },
            innateProperties: Has(
                Camouflage, AnimalBody, InvisibilityDetection, Stealthiness, Eyelessness, Headlessness, Limblessness,
                Carnivorism),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly MonsterVariant MimicSmall = new MonsterVariant(
            name: "small mimic", species: Mimic, speciesClass: ShapeChanger,
            initialLevel: 7, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            innateProperties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Small, weight: 300, nutrition: 200,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant MimicLarge = new MonsterVariant(
            name: "large mimic", species: Mimic, speciesClass: ShapeChanger, previousStage: MimicSmall,
            initialLevel: 8, movementRate: 3, armorClass: 7, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 5),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            innateProperties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Medium, weight: 600, nutrition: 400,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant MimicGiant = new MonsterVariant(
            name: "giant mimic", species: Mimic, speciesClass: ShapeChanger, previousStage: MimicLarge,
            initialLevel: 9, movementRate: 3, armorClass: 7, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Touch, Stick),
                new Attack(OnConsumption, Polymorph)
            },
            innateProperties: Has(
                AcidResistance, Infravisibility, Camouflage, Stealthiness, Eyelessness, Headlessness, Breathlessness,
                Limblessness, ThickHide, Clinginess, Amorphism, PolymorphControl, Carnivorism),
            size: Large, weight: 800, nutrition: 500,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant PiercerRock = new MonsterVariant(
            name: "rock piercer", species: Piercer,
            initialLevel: 3, movementRate: 1, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism, NoInventory),
            size: Small, weight: 200, nutrition: 100,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant PiercerIron = new MonsterVariant(
            name: "iron piercer", species: Piercer,
            initialLevel: 5, movementRate: 1, armorClass: 2, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties: Has(
                Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism, NoInventory),
            size: Small, weight: 300, nutrition: 150,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant PiercerGlass = new MonsterVariant(
            name: "glass piercer", species: Piercer,
            initialLevel: 7, movementRate: 1, armorClass: 1, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                AcidResistance, Camouflage, Stealthiness, Eyelessness, Limblessness, Clinginess, Carnivorism,
                NoInventory),
            size: Small, weight: 400, nutrition: 200,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant RustMonster = new MonsterVariant(
            name: "rust monster", species: Species.RustMonster,
            initialLevel: 5, movementRate: 18, armorClass: 2, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 6),
                new Attack(Touch, WaterDamage, diceCount: 2, diceSides: 6),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties: Has(Swimming, Infravisibility, AnimalBody, Handlessness, Metallivorism, SingularInventory),
            size: Medium, weight: 1000, nutrition: 300,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant Disenchanter = new MonsterVariant(
            name: "disenchanter", species: Species.Disenchanter, noise: Growl,
            initialLevel: 12, movementRate: 12, armorClass: -10, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Disenchant),
                new Attack(OnMeleeHit, Disenchant)
            },
            innateProperties: Has(Infravisibility, AnimalBody, Handlessness, Metallivorism, SingularInventory),
            size: Medium, weight: 750, nutrition: 200,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant Jabberwock = new MonsterVariant(
            name: "jabberwock", species: Species.Jabberwock, noise: Burble,
            initialLevel: 15, movementRate: 12, armorClass: -2, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            innateProperties: Has(Flight, Infravision, Infravisibility, AnimalBody, Carnivorism, SingularInventory),
            size: Large, weight: 1300, nutrition: 400,
            generationFrequency: Rarely);

        public static readonly MonsterVariant Couatl = new MonsterVariant(
            name: "couatl", species: WingedSnake, speciesClass: DivineBeing, noise: Hiss,
            initialLevel: 8, movementRate: 10, armorClass: 5, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Hug, Bind, diceCount: 3, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Flight, Infravision, SerpentlikeBody, SingularInventory),
            size: Large, weight: 900, nutrition: 400, corpse: None,
            behavior: Stalking | AlignmentAware, generationFlags: NoHell | SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant Kirin = new MonsterVariant(
            name: "ki-rin", species: Species.Kirin, speciesClass: DivineBeing, noise: Neigh,
            initialLevel: 16, movementRate: 18, armorClass: -5, magicResistance: 90, alignment: 15,
            attacks: new[]
            {
                new Attack(Kick, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Kick, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Headbutt, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Flight, AnimalBody, ThickHide,
                Infravisibility, Infravision, InvisibilityDetection, Handlessness, SingularInventory),
            size: Large, weight: 1300, nutrition: 600, corpse: None,
            behavior: Stalking | AlignmentAware,
            generationFlags: NoHell | NonPolymorphable, generationFrequency: Sometimes);

        public static readonly MonsterVariant DragonGrayBaby = new MonsterVariant(
            name: "baby gray dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 50, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism,
                SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonGray = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision, DangerAwareness,
                AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(InvisibilityDetection, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonSilverBaby = new MonsterVariant(
            name: "baby silver dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                Reflection, PoisonResistance, Flight, Infravisibility, AnimalBody, ThickHide, Handlessness, Carnivorism,
                SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonSilver = new MonsterVariant(
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
            innateProperties: Has(
                Reflection, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DangerAwareness, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonShimmeringBaby = new MonsterVariant(
            name: "baby shimmering dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: -5, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism,
                SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonShimmering = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(Infravision, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonGreenBaby = new MonsterVariant(
            name: "baby green dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness, Carnivorism,
                SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonGreen = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonPurpleBaby = new MonsterVariant(
            name: "baby purple dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                VenomResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonPurple = new MonsterVariant(
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
            innateProperties: Has(
                VenomResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonOrangeBaby = new MonsterVariant(
            name: "baby orange dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonOrange = new MonsterVariant(
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
            innateProperties: Has(
                SleepResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonRedBaby = new MonsterVariant(
            name: "baby red dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonRed = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonWhiteBaby = new MonsterVariant(
            name: "baby white dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonWhite = new MonsterVariant(
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
            innateProperties: Has(
                ColdResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonBlueBaby = new MonsterVariant(
            name: "baby blue dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                ElectricityResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonBlue = new MonsterVariant(
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
            innateProperties: Has(
                ElectricityResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonYellowBaby = new MonsterVariant(
            name: "baby yellow dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                AcidResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonYellow = new MonsterVariant(
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
            innateProperties: Has(
                AcidResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(AcidResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonBlackBaby = new MonsterVariant(
            name: "baby black dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                DisintegrationResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonBlack = new MonsterVariant(
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
            innateProperties: Has(
                DisintegrationResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DisintegrationResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonFairyBaby = new MonsterVariant(
            name: "baby fairy dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, Flight, Invisibility, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonFairy = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, Flight, InvisibilityDetection, Infravision, Invisibility,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DisintegrationResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonDeepBaby = new MonsterVariant(
            name: "baby deep dragon", species: Dragon, noise: Roar,
            initialLevel: 12, movementRate: 9, armorClass: 2, magicResistance: 10, alignment: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                DrainResistance, PoisonResistance, Flight, Infravision, AnimalBody, ThickHide, Handlessness,
                Carnivorism, SingularInventory),
            size: Large, weight: 1500, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DragonDeep = new MonsterVariant(
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
            innateProperties: Has(
                DrainResistance, PoisonResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Carnivorism, Oviparity, SingularInventory),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(DrainResistance, Often)},
            behavior: GoldCollector | GemCollector | Mountable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant LightYellow = new MonsterVariant(
            name: "yellow light", species: FloatingSphere,
            initialLevel: 3, movementRate: 15, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, Blind, diceCount: 5, diceSides: 10)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant LightBlack = new MonsterVariant(
            name: "black light", species: FloatingSphere,
            initialLevel: 5, movementRate: 15, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, Hallucinate, diceCount: 5, diceSides: 10)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant LightWisp = new MonsterVariant(
            name: "will o' wisp", species: FloatingSphere,
            initialLevel: 7, movementRate: 15, armorClass: 0, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Explosion, Confuse, diceCount: 5, diceSides: 10)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, ElectricityResistance, AcidResistance, DisintegrationResistance,
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, Infravisibility, InvisibilityDetection, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant SporeGas = new MonsterVariant(
            name: "gas spore", species: FloatingSphere,
            initialLevel: 1, movementRate: 3, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Explosion, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Explosion, Deafen, diceCount: 5, diceSides: 10)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, SlimingResistance, SicknessResistance, Flight,
                Stealthiness, NonAnimal, Breathlessness, Limblessness, Eyelessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant EyeFloating = new MonsterVariant(
            name: "floating eye", species: FloatingSphere,
            initialLevel: 2, movementRate: 1, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(OnMeleeHit, Paralyze, diceCount: 1, diceSides: 70)
            },
            innateProperties: Has(
                Flight, Stealthiness, Infravision, Infravisibility, NonAnimal, Breathlessness, Limblessness,
                Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, consumptionProperties: new[] {WhenConsumedAdd(Telepathy, Always)},
            behavior: Wandering, generationFrequency: Occasionally);

        public static readonly MonsterVariant SphereFreezing = new MonsterVariant(
            name: "freezing sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, ColdDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, Flight, Infravisibility, NonAnimal, Breathlessness, Limblessness, Headlessness,
                Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Always)},
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly MonsterVariant SphereFlaming = new MonsterVariant(
            name: "flaming sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, FireDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, Flight, Infravisibility, NonAnimal, Breathlessness, Limblessness, Headlessness,
                Mindlessness, Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Always)},
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly MonsterVariant SphereShocking = new MonsterVariant(
            name: "shocking sphere", species: FloatingSphere,
            initialLevel: 6, movementRate: 13, armorClass: 4, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Explosion, ElectricityDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                ElectricityResistance, Flight, NonAnimal, Breathlessness, Limblessness, Headlessness, Mindlessness,
                Asexuality, NoInventory),
            size: Small, weight: 10, nutrition: 10, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Always)},
            generationFrequency: Sometimes);

        public static readonly MonsterVariant Beholder = new MonsterVariant(
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
            innateProperties: Has(
                ColdResistance, Levitation, DangerAwareness, Infravision, Infravisibility, Stealthiness, Breathlessness,
                Limblessness, Headlessness, Asexuality, NoInventory),
            size: Medium, weight: 250, nutrition: 50,
            consumptionProperties:
                new[] {WhenConsumedAdd(ColdResistance, Occasionally), WhenConsumedAdd(SleepResistance, Sometimes)},
            behavior: Wandering, generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly MonsterVariant CloudFog = new MonsterVariant(
            name: "fog cloud", species: Cloud,
            initialLevel: 3, movementRate: 1, armorClass: 0, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 6),
                new Attack(Digestion, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Digestion, WaterDamage, diceCount: 1, diceSides: 2),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance, AcidResistance,
                SicknessResistance, Flight, Stealthiness, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality, NoInventory),
            size: Huge, weight: 1, nutrition: 0, corpse: None,
            generationFlags: NoHell, generationFrequency: Sometimes);

        public static readonly MonsterVariant VortexDust = new MonsterVariant(
            name: "dust vortex", species: Vortex,
            initialLevel: 4, movementRate: 20, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 6),
                new Attack(Digestion, Blind, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                WaterWeakness, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, NonAnimal, NonSolidBody, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly MonsterVariant VortexIce = new MonsterVariant(
            name: "ice vortex", species: Vortex,
            initialLevel: 5, movementRate: 20, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, ColdDamage, diceCount: 1, diceSides: 6),
                new Attack(OnRangedHit, ColdDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFlags: NoHell, generationFrequency: Commonly);

        public static readonly MonsterVariant VortexEnergy = new MonsterVariant(
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
            innateProperties: Has(
                ElectricityResistance, DisintegrationResistance, SleepResistance, PoisonResistance, VenomResistance,
                StoningResistance, SlimingResistance, SicknessResistance, Flight, NonAnimal, NonSolidBody,
                Breathlessness, Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly MonsterVariant VortexFire = new MonsterVariant(
            name: "fire vortex", species: Vortex,
            initialLevel: 8, movementRate: 22, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 1, diceSides: 8),
                new Attack(Digestion, FireDamage, diceCount: 1, diceSides: 10),
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 10),
                new Attack(OnRangedHit, FireDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties: Has(
                FireResistance, SleepResistance, AcidResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Commonly);

        public static readonly MonsterVariant Stalker = new MonsterVariant(
            name: "stalker", species: Elemental,
            initialLevel: 8, movementRate: 12, armorClass: 3, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            innateProperties: Has(
                Flight, Invisibility, InvisibilityDetection, Infravision, AnimalBody, Stealthiness),
            size: Large, weight: 900, nutrition: 400,
            consumptionProperties:
                new[] {WhenConsumedAdd(Invisibility, Uncommonly), WhenConsumedAdd(InvisibilityDetection, Uncommonly)},
            behavior: Stalking | Wandering, generationFrequency: Occasionally);

        public static readonly MonsterVariant ElementalAir = new MonsterVariant(
            name: "air elemental", species: Elemental,
            initialLevel: 8, movementRate: 36, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Touch, Engulf, diceCount: 2, diceSides: 4),
                new Attack(Digestion, Deafen, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Flight, Invisibility, NonAnimal, NonSolidBody, Breathlessness, Limblessness,
                Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ElementalFire = new MonsterVariant(
            name: "fire elemental", species: Elemental,
            initialLevel: 8, movementRate: 12, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, FireDamage, diceCount: 3, diceSides: 6),
                new Attack(OnMeleeHit, FireDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                WaterWeakness, FireResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, Flight, Infravisibility, NonAnimal, NonSolidBody, Breathlessness,
                Limblessness, Eyelessness, Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 0, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ElementalWater = new MonsterVariant(
            name: "water elemental", species: Elemental,
            initialLevel: 8, movementRate: 6, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 5, diceSides: 6),
                new Attack(Punch, WaterDamage, diceCount: 1, diceSides: 6),
                new Attack(OnMeleeHit, WaterDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Swimming, NonAnimal, NonSolidBody, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 2500, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ElementalEarth = new MonsterVariant(
            name: "earth elemental", species: Elemental,
            initialLevel: 8, movementRate: 6, armorClass: 2, magicResistance: 30,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Punch, Stun, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance, SlimingResistance,
                SicknessResistance, Phasing, ThickHide, NonAnimal, Breathlessness, Limblessness, Eyelessness,
                Headlessness, Mindlessness, Asexuality),
            size: Huge, weight: 2500, nutrition: 0, corpse: None,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant GolemPaper = new MonsterVariant(
            name: "paper golem", species: Golem,
            initialLevel: 2, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(
                WaterWeakness, ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 20)),
            size: Large, weight: 400, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemStraw = new MonsterVariant(
            name: "straw golem", species: Golem,
            initialLevel: 3, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 20)),
            size: Large, weight: 400, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemRope = new MonsterVariant(
            name: "rope golem", species: Golem,
            initialLevel: 4, movementRate: 12, armorClass: 8, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Hug, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SicknessResistance, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 30)),
            size: Large, weight: 450, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemGold = new MonsterVariant(
            name: "gold golem", species: Golem,
            initialLevel: 5, movementRate: 9, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                AcidResistance, ColdResistance, SleepResistance, PoisonResistance, VenomResistance, StoningResistance,
                SlimingResistance, SicknessResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness, Humanoidness,
                Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Medium, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemLeather = new MonsterVariant(
            name: "leather golem", species: Golem,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, Breathlessness, Mindlessness,
                Humanoidness, Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Large, weight: 800, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemWood = new MonsterVariant(
            name: "wood golem", species: Golem,
            initialLevel: 7, movementRate: 3, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, VenomResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness,
                Humanoidness, Asexuality).With(ActorProperty.Set(MaxHP, value: 50)),
            size: Large, weight: 1000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemFlesh = new MonsterVariant(
            name: "flesh golem", species: Golem,
            initialLevel: 9, movementRate: 8, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties:
                Has(SleepResistance, PoisonResistance, Regeneration, Breathlessness, Mindlessness, Humanoidness,
                    Asexuality).With(ActorProperty.Set(MaxHP, value: 40)),
            size: Large, weight: 1400, nutrition: 600,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemClay = new MonsterVariant(
            name: "clay golem", species: Golem,
            initialLevel: 11, movementRate: 7, armorClass: 7, magicResistance: 40,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10)
            },
            innateProperties: Has(
                ElectricityResistance, SleepResistance, PoisonResistance, VenomResistance, SicknessResistance,
                SlimingResistance, ThickHide, NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 50)),
            size: Large, weight: 1500, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemStone = new MonsterVariant(
            name: "stone golem", species: Golem,
            initialLevel: 14, movementRate: 6, armorClass: 4, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10)
            },
            innateProperties: Has(
                ColdResistance, FireResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide, NonAnimal,
                Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 60)),
            size: Large, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemGlass = new MonsterVariant(
            name: "glass golem", species: Golem,
            initialLevel: 16, movementRate: 6, armorClass: 4, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(
                Reflection, AcidResistance, ColdResistance, FireResistance, ElectricityResistance, SleepResistance,
                PoisonResistance, VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide,
                NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 60)),
            size: Large, weight: 1800, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant GolemIron = new MonsterVariant(
            name: "iron golem", species: Golem,
            initialLevel: 18, movementRate: 6, armorClass: 3, magicResistance: 60,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Breath, PoisonDamage, diceCount: 4, diceSides: 6, frequency: Sometimes)
            },
            innateProperties: Has(
                WaterWeakness, ColdResistance, FireResistance, SleepResistance,
                PoisonResistance, VenomResistance, SicknessResistance, StoningResistance, SlimingResistance, ThickHide,
                NonAnimal, Breathlessness, Mindlessness, Humanoidness, Asexuality)
                .With(ActorProperty.Set(MaxHP, value: 80)),
            size: Large, weight: 2000, nutrition: 0, corpse: None,
            generationFrequency: Rarely);

        public static readonly MonsterVariant Gargoyle = new MonsterVariant(
            name: "gargoyle", species: Species.Gargoyle, noise: Grunt,
            initialLevel: 6, movementRate: 10, armorClass: -4, magicResistance: 0, alignment: -9,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(StoningResistance, Humanoidness, ThickHide, Breathlessness),
            size: Medium, weight: 1000, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            generationFrequency: Commonly);

        public static readonly MonsterVariant WingedGargoyle = new MonsterVariant(
            name: "winged gargoyle", species: Species.Gargoyle, noise: Grunt, previousStage: Gargoyle,
            initialLevel: 9, movementRate: 15, armorClass: -4, magicResistance: 0, alignment: -12,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            innateProperties: Has(StoningResistance, Flight, Humanoidness, ThickHide, Breathlessness, Oviparity),
            size: Medium, weight: 1200, nutrition: 50,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Rarely)},
            behavior: MagicUser, generationFrequency: Commonly);

        public static readonly MonsterVariant HulkUmber = new MonsterVariant(
            name: "umber hulk", species: Hulk, speciesClass: Hybrid,
            initialLevel: 9, movementRate: 6, armorClass: 2, magicResistance: 25,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 5),
                new Attack(Gaze, Confuse, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Tunneling, AnimalBody, ThickHide, Infravision, Infravisibility, Carnivorism),
            size: Large, weight: 1300, nutrition: 500,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Minotaur = new MonsterVariant(
            name: "minotaur", species: Species.Minotaur, noise: Roar,
            initialLevel: 15, movementRate: 15, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Headbutt, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1500, nutrition: 600,
            behavior: WeaponCollector | GemCollector, generationFrequency: Rarely);

        public static readonly MonsterVariant Bugbear = new MonsterVariant(
            name: "bugbear", species: Species.Bugbear, noise: Growl,
            initialLevel: 3, movementRate: 9, armorClass: 5, magicResistance: 0, alignment: -6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1250, nutrition: 250,
            behavior: WeaponCollector, generationFrequency: Commonly);

        public static readonly MonsterVariant CentaurPlains = new MonsterVariant(
            name: "plains centaur", species: Centaur, noise: Speach,
            initialLevel: 4, movementRate: 18, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly MonsterVariant CentaurForest = new MonsterVariant(
            name: "forest centaur", species: Centaur, noise: Speach,
            initialLevel: 5, movementRate: 18, armorClass: 3, magicResistance: 10, alignment: -1,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly MonsterVariant CentaurMountain = new MonsterVariant(
            name: "mountain centaur", species: Centaur, noise: Speach,
            initialLevel: 6, movementRate: 20, armorClass: 2, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, HumanoidTorso, Omnivorism),
            size: Large, weight: 2000, nutrition: 800,
            behavior: WeaponCollector | GoldCollector,
            generationFrequency: Commonly);

        public static readonly MonsterVariant NymphWood = new MonsterVariant(
            name: "wood nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            innateProperties: Has(Teleportation, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant NymphWater = new MonsterVariant(
            name: "water nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            innateProperties: Has(Teleportation, Swimming, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant NymphMountain = new MonsterVariant(
            name: "mountain nymph", species: Nymph, noise: Seduction, speciesClass: Fey,
            initialLevel: 3, movementRate: 12, armorClass: 9, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Touch, StealItem)
            },
            innateProperties: Has(Teleportation, Humanoidness, Infravisibility, Femaleness),
            size: Medium, weight: 600, nutrition: 300,
            consumptionProperties: new[] {WhenConsumedAdd(Teleportation, Sometimes)},
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant NagaRedHatchling = new MonsterVariant(
            name: "red naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, VenomResistance, SlimingResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, SingularInventory),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(FireResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaRed = new MonsterVariant(
            name: "red naga", species: Naga, noise: Hiss,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 0, alignment: -4,
            previousStage: NagaRedHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Spit, FireDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, VenomResistance, SlimingResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity, SingularInventory),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(FireResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaBlackHatchling = new MonsterVariant(
            name: "black naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, SingularInventory),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaBlack = new MonsterVariant(
            name: "black naga", species: Naga, noise: Hiss,
            initialLevel: 8, movementRate: 14, armorClass: 2, magicResistance: 10, alignment: 4,
            previousStage: NagaBlackHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Spit, AcidDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity, SingularInventory),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaGoldenHatchling = new MonsterVariant(
            name: "golden naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, InvisibilityDetection, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, SingularInventory),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(InvisibilityDetection, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaGolden = new MonsterVariant(
            name: "golden naga", species: Naga, noise: Hiss,
            initialLevel: 10, movementRate: 14, armorClass: 2, magicResistance: 70, alignment: 5,
            previousStage: NagaGoldenHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                AcidResistance, PoisonResistance, VenomResistance, StoningResistance, Infravision, ThickHide,
                SerpentlikeBody, Limblessness, Carnivorism, Oviparity, SingularInventory),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(AcidResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaGuardianHatchling = new MonsterVariant(
            name: "guardian naga hatchling", species: Naga, noise: Hiss,
            initialLevel: 3, movementRate: 10, armorClass: 6, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, InvisibilityDetection, Infravision, ThickHide, SerpentlikeBody,
                Limblessness, Carnivorism, SingularInventory),
            size: Medium, weight: 500, nutrition: 200, consumptionProperties:
                new[] {WhenConsumedAdd(VenomResistance, Rarely), WhenConsumedAdd(PoisonResistance, Occasionally)},
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant NagaGuardian = new MonsterVariant(
            name: "guardian naga", species: Naga, noise: Hiss,
            initialLevel: 12, movementRate: 16, armorClass: 0, magicResistance: 50, alignment: 7,
            previousStage: NagaGuardianHatchling,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spit, VenomDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, Infravision, ThickHide, SerpentlikeBody, Limblessness, Carnivorism,
                Oviparity, SingularInventory),
            size: Huge, weight: 1500, nutrition: 600, consumptionProperties:
                new[] {WhenConsumedAdd(VenomResistance, Occasionally), WhenConsumedAdd(PoisonResistance, Sometimes)},
            generationFrequency: Uncommonly);

        // TODO: Add yuan-ti, lizardmen

        public static readonly MonsterVariant Salamander = new MonsterVariant(
            name: "salamander", species: Species.Salamander, noise: Mumble,
            initialLevel: 10, movementRate: 12, armorClass: -1, magicResistance: 0, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Touch, FireDamage, diceCount: 1, diceSides: 6),
                new Attack(Hug, FireDamage, diceCount: 2, diceSides: 6),
                new Attack(Hug, FireDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, FireDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SleepResistance, SlimingResistance, Infravision, Infravisibility,
                Humanoidness, ThickHide, SerpentlikeBody, Limblessness),
            size: Large, weight: 1500, nutrition: 400, consumptionProperties:
                new[] {WhenConsumedAdd(FireResistance, Occasionally)},
            behavior: Stalking | WeaponCollector | MagicUser,
            generationFlags: HellOnly, generationFrequency: Uncommonly);

        public static readonly MonsterVariant KoboldMedium = new MonsterVariant(
            name: "kobold", species: Kobold, noise: Grunt,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 400, nutrition: 100,
            behavior: WeaponCollector, generationFrequency: Often);

        public static readonly MonsterVariant KoboldLarge = new MonsterVariant(
            name: "large kobold", species: Kobold, noise: Grunt, previousStage: KoboldMedium,
            initialLevel: 2, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            innateProperties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 450, nutrition: 150,
            behavior: WeaponCollector, generationFrequency: Usually);

        public static readonly MonsterVariant KoboldLord = new MonsterVariant(
            name: "kobold lord", species: Kobold, noise: Grunt, previousStage: KoboldLarge,
            initialLevel: 3, movementRate: 6, armorClass: 6, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            innateProperties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Small, weight: 500, nutrition: 200,
            behavior: WeaponCollector, generationFrequency: Sometimes);

        public static readonly MonsterVariant KoboldShaman = new MonsterVariant(
            name: "kobold shaman", species: Kobold, noise: Grunt,
            initialLevel: 3, movementRate: 6, armorClass: 8, magicResistance: 10, alignment: -4,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 5)
            },
            innateProperties: Has(PoisonResistance, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Small, weight: 450, nutrition: 150,
            behavior: WeaponCollector | MagicUser, generationFrequency: Sometimes);

        public static readonly MonsterVariant Goblin = new MonsterVariant(
            name: "goblin", species: Species.Goblin, noise: Grunt,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 400, nutrition: 100,
            behavior: WeaponCollector, generationFrequency: Commonly);

        public static readonly MonsterVariant Hobgoblin = new MonsterVariant(
            name: "hobgoblin", species: Species.Goblin, noise: Grunt,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFrequency: Commonly);

        // TODO: Add more goblins

        public static readonly MonsterVariant Gremlin = new MonsterVariant(
            name: "gremlin", species: Species.Gremlin, noise: Laugh,
            initialLevel: 5, movementRate: 12, armorClass: 2, magicResistance: 25, alignment: -5,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Claw, Curse),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Humanoidness, Swimming, Infravisibility, Omnivorism),
            size: Small, weight: 100, nutrition: 20,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Commonly);

        public static readonly MonsterVariant Leprechaun = new MonsterVariant(
            name: "leprechaun", species: Species.Leprechaun, noise: Laugh, speciesClass: Fey,
            initialLevel: 5, movementRate: 15, armorClass: 8, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Claw, StealGold)
            },
            innateProperties: Has(Teleportation, Infravisibility, Omnivorism),
            size: Tiny, weight: 60, nutrition: 30,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(Teleportation, Sometimes), WhenConsumedAdd(Luck, value: 1, frequency: Rarely)
            },
            behavior: GoldCollector, generationFrequency: Occasionally);

        public static readonly MonsterVariant Gnome = new MonsterVariant(
            name: "gnome", species: Species.Gnome, noise: Speach,
            initialLevel: 1, movementRate: 6, armorClass: 10, magicResistance: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Small, weight: 650, nutrition: 200,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Rarely);

        public static readonly MonsterVariant GnomeLord = new MonsterVariant(
            name: "gnome lord", species: Species.Gnome, noise: Speach, previousStage: Gnome,
            initialLevel: 3, movementRate: 8, armorClass: 10, magicResistance: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 700, nutrition: 250,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GnomeWizard = new MonsterVariant(
            name: "gnomish wizard", species: Species.Gnome, noise: Speach,
            initialLevel: 3, movementRate: 8, armorClass: 10, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 700, nutrition: 250,
            behavior: WeaponCollector | GoldCollector | MagicUser,
            generationFlags: NonPolymorphable, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GnomeKing = new MonsterVariant(
            name: "gnome king", species: Species.Gnome, noise: Speach, previousStage: GnomeLord,
            initialLevel: 5, movementRate: 10, armorClass: 10, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Small, weight: 750, nutrition: 300,
            behavior: WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | Entourage, generationFrequency: Uncommonly);

        public static readonly MonsterVariant Hobbit = new MonsterVariant(
            name: "hobbit", species: Species.Hobbit, noise: Speach,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 10, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 500, nutrition: 250,
            behavior: AlignmentAware | WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Commonly);

        public static readonly MonsterVariant Dwarf = new MonsterVariant(
            name: "dwarf", species: Species.Dwarf, noise: Speach,
            initialLevel: 2, movementRate: 6, armorClass: 10, magicResistance: 10, alignment: 4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Sometimes);

        public static readonly MonsterVariant DwarfLord = new MonsterVariant(
            name: "dwarf lord", species: Species.Dwarf, noise: Speach, previousStage: Dwarf,
            initialLevel: 4, movementRate: 6, armorClass: 10, magicResistance: 10, alignment: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly MonsterVariant DwarfKing = new MonsterVariant(
            name: "dwarf king", species: Species.Dwarf, noise: Speach, previousStage: DwarfLord,
            initialLevel: 6, movementRate: 6, armorClass: 10, magicResistance: 20, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(ToolTunneling, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 900, nutrition: 400,
            behavior: AlignmentAware | WeaponCollector | GemCollector | GoldCollector,
            generationFlags: NonPolymorphable | Entourage, generationFrequency: Rarely);

        public static readonly MonsterVariant Elf = new MonsterVariant(
            name: "elf", species: Species.Elf, noise: Speach,
            initialLevel: 4, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant ElfWoodland = new MonsterVariant(
            name: "woodland-elf", species: Species.Elf, noise: Speach,
            initialLevel: 4, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant ElfGreen = new MonsterVariant(
            name: "green-elf", species: Species.Elf, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant ElfGrey = new MonsterVariant(
            name: "grey-elf", species: Species.Elf, noise: Speach,
            initialLevel: 6, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant ElfLord = new MonsterVariant(
            name: "elf-lord", species: Species.Elf, noise: Speach,
            initialLevel: 8, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant ElfKing = new MonsterVariant(
            name: "Elvenking", species: Species.Elf, noise: Speach,
            initialLevel: 9, movementRate: 12, armorClass: 10, magicResistance: 25, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism, Maleness),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: Entourage, generationFrequency: Rarely);

        public static readonly MonsterVariant ElfDrow = new MonsterVariant(
            name: "drow", species: Species.Elf, noise: Speach,
            initialLevel: 6, movementRate: 12, armorClass: 10, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Touch, Sleep, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant ElfDrowWarrior = new MonsterVariant(
            name: "drow warrior", species: Species.Elf, noise: Speach,
            initialLevel: 7, movementRate: 12, armorClass: 10, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Touch, Sleep, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                SleepResistance, InvisibilityDetection, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 800, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: AlignmentAware | WeaponCollector,
            generationFlags: NonPolymorphable | SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant Human = new MonsterVariant(
            name: "human", species: Species.Human, noise: Speach,
            initialLevel: 1, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Shopkeeper = new MonsterVariant(
            name: "shopkeeper", species: Species.Human, noise: Sell,
            initialLevel: 12, movementRate: 18, armorClass: 0, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            innateProperties: Has(SleepResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Doctor = new MonsterVariant(
            name: "doctor", species: Species.Human, noise: ActorNoiseType.Doctor,
            initialLevel: 11, movementRate: 6, armorClass: 0, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Touch, Heal, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful,
            generationFlags: NonPolymorphable, generationFrequency: Occasionally);

        public static readonly MonsterVariant Priest = new MonsterVariant(
            name: "aligned priest", species: Species.Human, noise: ActorNoiseType.Priest,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Spell, DivineSpell)
            },
            innateProperties: Has(ElectricityResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant PriestHigh = new MonsterVariant(
            name: "high priest", species: Species.Human, noise: ActorNoiseType.Priest,
            initialLevel: 25, movementRate: 16, armorClass: 7, magicResistance: 70,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10),
                new Attack(Spell, DivineSpell),
                new Attack(Spell, DivineSpell)
            },
            innateProperties: Has(
                FireResistance, ElectricityResistance, SleepResistance, PoisonResistance, InvisibilityDetection,
                Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Displacing | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Prisoner = new MonsterVariant(
            name: "prisoner", species: Species.Human, noise: ActorNoiseType.Prisoner,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Watchman = new MonsterVariant(
            name: "watchman", species: Species.Human, noise: ActorNoiseType.Soldier,
            initialLevel: 6, movementRate: 10, armorClass: 10, magicResistance: 0, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant WatchCaptain = new MonsterVariant(
            name: "watch captain", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Watchman,
            initialLevel: 10, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Guard = new MonsterVariant(
            name: "guard", species: Species.Human, noise: ActorNoiseType.Guard,
            initialLevel: 12, movementRate: 12, armorClass: 10, magicResistance: 40, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | Bribeable | Stalking | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Soldier = new MonsterVariant(
            name: "soldier", species: Species.Human, noise: ActorNoiseType.Soldier,
            initialLevel: 6, movementRate: 10, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: SmallGroup | NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Sergeant = new MonsterVariant(
            name: "sergeant", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Soldier,
            initialLevel: 8, movementRate: 10, armorClass: 10, magicResistance: 5, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: SmallGroup | NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Lieutenant = new MonsterVariant(
            name: "lieutenant", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Sergeant,
            initialLevel: 10, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Captain = new MonsterVariant(
            name: "captain", species: Species.Human, noise: ActorNoiseType.Soldier, previousStage: Lieutenant,
            initialLevel: 12, movementRate: 10, armorClass: 10, magicResistance: 15, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Bribeable | Stalking | Wandering | Displacing | WeaponCollector | GoldCollector,
            generationFlags: Entourage | NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Student = new MonsterVariant(
            name: "student", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(ToolTunneling, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Chieftain = new MonsterVariant(
            name: "chieftain", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Neanderthal = new MonsterVariant(
            name: "neanderthal", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Attendant = new MonsterVariant(
            name: "attendant", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Page = new MonsterVariant(
            name: "page", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 6, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Abbot = new MonsterVariant(
            name: "abbot", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Acolyte = new MonsterVariant(
            name: "acolyte", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Hunter = new MonsterVariant(
            name: "hunter", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(Perception, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Thug = new MonsterVariant(
            name: "thug", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Stealthiness, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector | GemCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Ninja = new MonsterVariant(
            name: "ninja", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Guide = new MonsterVariant(
            name: "guide", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Warrior = new MonsterVariant(
            name: "warrior", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -1,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(ColdResistance, Infravisibility, Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Apprentice = new MonsterVariant(
            name: "apprentice", species: Species.Human, noise: Speach,
            initialLevel: 5, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Archeologist = new MonsterVariant(
            name: "archeologist", species: Species.Human, noise: Speach, previousStage: Student,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(ToolTunneling, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Barbarian = new MonsterVariant(
            name: "barbarian", species: Species.Human, noise: Speach, previousStage: Chieftain,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Caveman = new MonsterVariant(
            name: "caveman", species: Species.Human, noise: Speach, previousStage: Neanderthal,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Healer = new MonsterVariant(
            name: "healer", species: Species.Human, noise: Speach, previousStage: Attendant,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Spell, DivineSpell, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Knight = new MonsterVariant(
            name: "knight", species: Species.Human, noise: Speach, previousStage: Page,
            initialLevel: 10, movementRate: 12, armorClass: 6, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Monk = new MonsterVariant(
            name: "monk", species: Species.Human, noise: Speach, previousStage: Abbot,
            initialLevel: 10, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Cleric = new MonsterVariant(
            name: "cleric", species: Species.Human, noise: Speach, previousStage: Acolyte,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 10, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Spell, DivineSpell, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Ranger = new MonsterVariant(
            name: "ranger", species: Species.Human, noise: Speach, previousStage: Hunter,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Perception, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Rogue = new MonsterVariant(
            name: "rogue", species: Species.Human, noise: Speach, previousStage: Thug,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Stealthiness, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector | GemCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Samurai = new MonsterVariant(
            name: "samurai", species: Species.Human, noise: Speach, previousStage: Ninja,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Tourist = new MonsterVariant(
            name: "tourist", species: Species.Human, noise: Speach, previousStage: Guide,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Valkyrie = new MonsterVariant(
            name: "valkyrie", species: Species.Human, noise: Speach, previousStage: Warrior,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -1,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(ColdResistance, Infravisibility, Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Wizard = new MonsterVariant(
            name: "wizard", species: Species.Human, noise: Speach, previousStage: Apprentice,
            initialLevel: 10, movementRate: 12, armorClass: 10, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(PoisonResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant LordCarnarvon = new MonsterVariant(
            name: "Lord Carnarvon", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: 20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, ToolTunneling, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Pelias = new MonsterVariant(
            name: "Pelias", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant ShamanKarnov = new MonsterVariant(
            name: "Shaman Karnov", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 10, magicResistance: 30, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Hippocrates = new MonsterVariant(
            name: "Hippocrates", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 40, alignment: 0,
            attacks: new[]
            {
                new Attack(Spell, DivineSpell, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, Infravisibility, Humanoidness, Maleness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant KingArthur = new MonsterVariant(
            name: "King Arthur", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 40, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant GrandMaster = new MonsterVariant(
            name: "Grand Master", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 6, magicResistance: 50, alignment: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Kick, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Maleness, Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant ArchPriest = new MonsterVariant(
            name: "Arch Priest", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 7, magicResistance: 60, alignment: 3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Spell, DivineSpell, diceCount: 1, diceSides: 4)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Orion = new MonsterVariant(
            name: "Orion", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Perception, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant ThievesMaster = new MonsterVariant(
            name: "Master of Thieves", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 10, magicResistance: 0, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(StoningResistance, Stealthiness, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector | GemCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant LordSato = new MonsterVariant(
            name: "Lord Sato", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: 10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Twoflower = new MonsterVariant(
            name: "Twoflower", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 8, magicResistance: 20, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Norn = new MonsterVariant(
            name: "Norn", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 2, magicResistance: 40, alignment: 6,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties:
                Has(StoningResistance, ColdResistance, Infravisibility, Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1200, nutrition: 400,
            behavior: Peaceful | WeaponCollector | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Neferet = new MonsterVariant(
            name: "Neferet the Green", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 10, magicResistance: 70, alignment: 0,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell, diceCount: 1, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, Infravisibility, Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful | MagicUser | GoldCollector,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant HuehueteotlMinion = new MonsterVariant(
            name: "Minion of Huehueteotl", species: DemonMajor, speciesClass: Demon, noise: Growl,
            initialLevel: 16, movementRate: 12, armorClass: -2, magicResistance: 75, alignment: -14,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 8, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, StoningResistance, Flight, Infravision,
                InvisibilityDetection, Infravisibility, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | Covetous,
            generationFlags: NonGenocidable | NonPolymorphable,
            generationFrequency: Rarely);

        public static readonly MonsterVariant ThothAmon = new MonsterVariant(
            name: "Thoth Amon", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: -14,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | GoldCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant DragonChromatic = new MonsterVariant(
            name: "Chromatic Dragon", species: Dragon, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: -14,
            attacks: new[]
            {
                new Attack(Breath, ElementalDamage, diceCount: 6, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, SleepResistance, ElectricityResistance, PoisonResistance, AcidResistance,
                StoningResistance, Flight, InvisibilityDetection, Infravision, DangerAwareness, AnimalBody, ThickHide,
                Handlessness, Femaleness, Carnivorism),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties:
                new[]
                {
                    WhenConsumedAdd(FireResistance, Usually), WhenConsumedAdd(ColdResistance, Usually),
                    WhenConsumedAdd(ElectricityResistance, Usually), WhenConsumedAdd(PoisonResistance, Usually),
                    WhenConsumedAdd(AcidResistance, Usually)
                },
            behavior: RangedPeaceful | Stalking | GoldCollector | GemCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Cyclops = new MonsterVariant(
            name: "Cyclops", species: Species.Giant, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 0, alignment: -15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, Flight, Infravision, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Huge, weight: 2200, nutrition: 800,
            behavior: RangedPeaceful | Stalking | WeaponCollector | GemCollector | MagicUser | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Ixoth = new MonsterVariant(
            name: "Ixoth", species: Dragon, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: -1, magicResistance: 20, alignment: -14,
            attacks: new[]
            {
                new Attack(Breath, FireDamage, diceCount: 6, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 4, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 4, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, StoningResistance, Flight, InvisibilityDetection, Infravision,
                DangerAwareness, AnimalBody, ThickHide, Handlessness, Maleness, Carnivorism),
            size: Gigantic, weight: 4500, nutrition: 1500,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Usually)},
            behavior: RangedPeaceful | Stalking | GoldCollector | GemCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant MasterKaen = new MonsterVariant(
            name: "Master Kaen", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 10, alignment: -16,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 16, diceSides: 2),
                new Attack(Spell, DivineSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties:
                Has(StoningResistance, PoisonResistance, InvisibilityDetection, Infravisibility, Humanoidness, Maleness,
                    Herbivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful | Stalking | MagicUser | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Nalzok = new MonsterVariant(
            name: "Nalzok", species: DemonMajor, speciesClass: Demon, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: -2, magicResistance: 75, alignment: -16,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 8, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, StoningResistance, Flight, Infravision,
                InvisibilityDetection, Infravisibility, Maleness, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | Covetous,
            generationFlags: NonGenocidable | NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Scorpius = new MonsterVariant(
            name: "Scorpius", species: Scorpion, speciesClass: Insect, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 3, magicResistance: 0, alignment: -15,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, StealAmulet, diceCount: 2, diceSides: 6),
                new Attack(Sting, Infect, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                PoisonResistance, VenomResistance, StoningResistance, AnimalBody, Handlessness, Maleness, Carnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(VenomResistance, Sometimes)},
            behavior: RangedPeaceful | Stalking | Covetous,
            generationFlags: NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant AssassinMaster = new MonsterVariant(
            name: "Master Assassin", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 30, alignment: 16,
            attacks: new[]
            {
                new Attack(Weapon, VenomDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, VenomDamage, diceCount: 2, diceSides: 6),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Stealthiness, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | GoldCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant AshikagaTakauji = new MonsterVariant(
            name: "Ashikaga Takauji", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 40, alignment: -13,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | GoldCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant LordSurtur = new MonsterVariant(
            name: "Lord Surtur", species: Species.Giant, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 2, magicResistance: 40, alignment: 12,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 10),
                new Attack(Touch, StealAmulet, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, StoningResistance, Flight, Infravision, Infravisibility, Humanoidness, Maleness,
                Omnivorism),
            size: Huge, weight: 2200, nutrition: 800,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Usually)},
            behavior: RangedPeaceful | Stalking | WeaponCollector | GemCollector | MagicUser | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant DarkOne = new MonsterVariant(
            name: "Dark One", species: Species.Human, noise: Quest,
            initialLevel: 16, movementRate: 12, armorClass: 0, magicResistance: 60, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(Touch, StealAmulet, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(StoningResistance, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | GoldCollector | Covetous,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Oracle = new MonsterVariant(
            name: "Oracle", species: Species.Human, noise: ActorNoiseType.Oracle,
            initialLevel: 12, movementRate: 0, armorClass: 0, magicResistance: 50,
            attacks: new[]
            {
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 1)
            },
            innateProperties: Has(Infravisibility, Humanoidness, Femaleness),
            size: Medium, weight: 1000, nutrition: 400,
            behavior: Peaceful, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Medusa = new MonsterVariant(
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
            innateProperties: Has(
                PoisonResistance, VenomResistance, StoningResistance, Flight, Amphibiousness, Infravisibility,
                Humanoidness, Femaleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: RangedPeaceful, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Rodney = new MonsterVariant(
            name: "Wizard of Yendor", species: Species.Human, noise: Cuss,
            initialLevel: 30, movementRate: 12, armorClass: -8, magicResistance: 100,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 12),
                new Attack(Punch, StealAmulet),
                new Attack(Spell, ArcaneSpell)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, Regeneration, EnergyRegeneration, Flight, Teleportation,
                TeleportationControl, MagicalBreathing, Infravisibility, InvisibilityDetection, Telepathy, Humanoidness,
                Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: RangedPeaceful | Covetous | MagicUser,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Croesus = new MonsterVariant(
            name: "Croesus", species: Species.Human, noise: ActorNoiseType.Guard,
            initialLevel: 20, movementRate: 15, armorClass: 0, magicResistance: 40, alignment: 15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 10)
            },
            innateProperties: Has(Infravisibility, InvisibilityDetection, Humanoidness, Maleness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(StoningResistance, Often)},
            behavior: Stalking | GoldCollector | GemCollector | WeaponCollector | MagicUser,
            generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Doppelganger = new MonsterVariant(
            name: "doppelganger", species: Species.Doppelganger, speciesClass: ShapeChanger, noise: Imitate,
            initialLevel: 9, movementRate: 12, armorClass: 5, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 12)
            },
            innateProperties: Has(SleepResistance, PolymorphControl, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PolymorphControl, Rarely)},
            behavior: WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Orc = new MonsterVariant(
            name: "orc", species: Species.Orc, noise: Grunt,
            initialLevel: 1, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant OrcHill = new MonsterVariant(
            name: "hill orc", species: Species.Orc, noise: Grunt,
            initialLevel: 2, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -4,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant OrcMordor = new MonsterVariant(
            name: "Mordor orc", species: Species.Orc, noise: Grunt,
            initialLevel: 3, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1100, nutrition: 200,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant OrcShaman = new MonsterVariant(
            name: "orc shaman", species: Species.Orc, noise: Grunt,
            initialLevel: 3, movementRate: 9, armorClass: 10, magicResistance: 10, alignment: -5,
            attacks: new[]
            {
                new Attack(Spell, ArcaneSpell)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1000, nutrition: 200,
            behavior: MagicUser | GoldCollector, generationFrequency: Commonly);

        public static readonly MonsterVariant OrcUruk = new MonsterVariant(
            name: "uruk-hai", species: Species.Orc, noise: Grunt,
            initialLevel: 4, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1300, nutrition: 300,
            behavior: WeaponCollector | GoldCollector, generationFlags: LargeGroup, generationFrequency: Commonly);

        public static readonly MonsterVariant OrcCaptain = new MonsterVariant(
            name: "orc captain", species: Species.Orc, noise: Grunt, previousStage: Orc,
            initialLevel: 5, movementRate: 9, armorClass: 10, magicResistance: 0, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Medium, weight: 1350, nutrition: 350,
            behavior: WeaponCollector | GoldCollector, generationFlags: Entourage, generationFrequency: Sometimes);

        public static readonly MonsterVariant Ogre = new MonsterVariant(
            name: "ogre", species: Species.Ogre, noise: Grunt,
            initialLevel: 5, movementRate: 10, armorClass: 5, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 5)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism),
            size: Large, weight: 1600, nutrition: 500,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFlags: SmallGroup, generationFrequency: Occasionally);

        public static readonly MonsterVariant OgreLord = new MonsterVariant(
            name: "ogre lord", species: Species.Ogre, noise: Grunt, previousStage: Ogre,
            initialLevel: 7, movementRate: 12, armorClass: 3, magicResistance: 30, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism, Maleness),
            size: Large, weight: 1650, nutrition: 550,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant OgreKing = new MonsterVariant(
            name: "ogre king", species: Species.Ogre, noise: Grunt, previousStage: OgreLord,
            initialLevel: 9, movementRate: 14, armorClass: 4, magicResistance: 60, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 5)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Carnivorism, Maleness),
            size: Large, weight: 1700, nutrition: 600,
            behavior: WeaponCollector | GemCollector | GoldCollector,
            generationFlags: Entourage, generationFrequency: Occasionally);

        public static readonly MonsterVariant Troll = new MonsterVariant(
            name: "troll", species: Species.Troll, noise: Grunt,
            initialLevel: 5, movementRate: 10, armorClass: 5, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 800, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant TrollIce = new MonsterVariant(
            name: "ice troll", species: Species.Troll, noise: Grunt,
            initialLevel: 9, movementRate: 10, armorClass: 2, magicResistance: 20, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1000, nutrition: 350,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Sometimes)},
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Occasionally);

        public static readonly MonsterVariant TrollRock = new MonsterVariant(
            name: "rock troll", species: Species.Troll, noise: Grunt,
            initialLevel: 9, movementRate: 12, armorClass: 0, magicResistance: 0, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1200, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Uncommonly);

        public static readonly MonsterVariant TrollWater = new MonsterVariant(
            name: "water troll", species: Species.Troll, noise: Grunt,
            initialLevel: 11, movementRate: 14, armorClass: 4, magicResistance: 40, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                Regeneration, Swimming, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1200, nutrition: 350,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Rarely);

        public static readonly MonsterVariant TrollHai = new MonsterVariant(
            name: "olog-hai", species: Species.Troll, noise: Grunt,
            initialLevel: 13, movementRate: 12, armorClass: -4, magicResistance: 40, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
                Regeneration, Infravision, Infravisibility, Humanoidness, Carnivorism, Reanimation),
            size: Large, weight: 1500, nutrition: 400,
            behavior: Stalking | WeaponCollector,
            generationFlags: NoHell, generationFrequency: Rarely);

        public static readonly MonsterVariant Giant = new MonsterVariant(
            name: "giant", species: Species.Giant, noise: Boast,
            initialLevel: 6, movementRate: 6, armorClass: 6, magicResistance: 0, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant GiantStone = new MonsterVariant(
            name: "stone giant", species: Species.Giant, noise: Boast,
            initialLevel: 6, movementRate: 6, armorClass: 0, magicResistance: 0, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GiantHill = new MonsterVariant(
            name: "hill giant", species: Species.Giant, noise: Boast,
            initialLevel: 8, movementRate: 10, armorClass: 6, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GiantFire = new MonsterVariant(
            name: "fire giant", species: Species.Giant, noise: Boast,
            initialLevel: 9, movementRate: 12, armorClass: 4, magicResistance: 5, alignment: 2,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            innateProperties: Has(FireResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(FireResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GiantFrost = new MonsterVariant(
            name: "frost giant", species: Species.Giant, noise: Boast,
            initialLevel: 10, movementRate: 12, armorClass: 3, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 10)
            },
            innateProperties: Has(ColdResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup | NoHell,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Ettin = new MonsterVariant(
            name: "ettin", species: Species.Giant, noise: Boast,
            initialLevel: 10, movementRate: 12, armorClass: 3, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            behavior: WeaponCollector | GemCollector, generationFrequency: Uncommonly);

        public static readonly MonsterVariant GiantStorm = new MonsterVariant(
            name: "storm giant", species: Species.Giant, noise: Boast,
            initialLevel: 16, movementRate: 12, armorClass: 3, magicResistance: 10, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 12)
            },
            innateProperties: Has(ElectricityResistance, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Huge, weight: 2250, nutrition: 750,
            consumptionProperties: new[] {WhenConsumedAdd(ElectricityResistance, Occasionally)},
            behavior: WeaponCollector | GemCollector, generationFlags: SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Titan = new MonsterVariant(
            name: "titan", species: Species.Giant, noise: Boast,
            initialLevel: 16, movementRate: 18, armorClass: -3, magicResistance: 70, alignment: 9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 8)
            },
            innateProperties: Has(Flight, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Gigantic, weight: 3000, nutrition: 900,
            behavior: WeaponCollector | GemCollector | MagicUser,
            generationFlags: NonGenocidable, generationFrequency: Rarely);

        public static readonly MonsterVariant Skeleton = new MonsterVariant(
            name: "skeleton", species: Species.Skeleton, speciesClass: Undead,
            initialLevel: 3, movementRate: 6, armorClass: 10, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, StoningResistance, SicknessResistance,
                VenomResistance, ThickHide, Infravision, Humanoidness, Breathlessness, Mindlessness),
            size: Medium, weight: 300, nutrition: 5, corpse: None,
            behavior: WeaponCollector, generationFlags: SmallGroup, generationFrequency: Occasionally);

        //TODO: Add more skeletons

        public static readonly MonsterVariant ZombieKobold = new MonsterVariant(
            name: "kobold zombie", species: Kobold, speciesClass: Undead, noise: Moan,
            initialLevel: 1, movementRate: 6, armorClass: 9, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 400, nutrition: 50, corpse: KoboldMedium,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant MummyKobold = new MonsterVariant(
            name: "kobold mummy", species: Kobold, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 6, armorClass: 6, magicResistance: 20, alignment: -2,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 10)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 400, nutrition: 50, corpse: KoboldMedium,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ZombieGnome = new MonsterVariant(
            name: "gnome zombie", species: Species.Gnome, speciesClass: Undead, noise: Moan,
            initialLevel: 2, movementRate: 6, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Small, weight: 650, nutrition: 100, corpse: Gnome,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant MummyGnome = new MonsterVariant(
            name: "gnome mummy", species: Species.Gnome, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 6, armorClass: 6, magicResistance: 20, alignment: -3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Small, weight: 650, nutrition: 100, corpse: Gnome,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ZombieOrc = new MonsterVariant(
            name: "orc zombie", species: Species.Orc, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 9, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 1000, nutrition: 100, corpse: Orc,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant MummyOrc = new MonsterVariant(
            name: "orc mummy", species: Species.Orc, speciesClass: Undead, noise: Moan,
            initialLevel: 5, movementRate: 9, armorClass: 5, magicResistance: 20, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 100, corpse: Orc,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ZombieDwarf = new MonsterVariant(
            name: "dwarf zombie", species: Species.Dwarf, speciesClass: Undead, noise: Moan,
            initialLevel: 3, movementRate: 6, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 900, nutrition: 200, corpse: Dwarf,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant MummyDwarf = new MonsterVariant(
            name: "dwarf mummy", species: Species.Dwarf, speciesClass: Undead, noise: Moan,
            initialLevel: 5, movementRate: 6, armorClass: 5, magicResistance: 30, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 900, nutrition: 200, corpse: Dwarf,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant ZombieElf = new MonsterVariant(
            name: "elf zombie", species: Species.Elf, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 12, armorClass: 9, magicResistance: 10,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, InvisibilityDetection,
                Infravision, Humanoidness, Breathlessness, Mindlessness),
            size: Medium, weight: 800, nutrition: 150, corpse: Elf,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant MummyElf = new MonsterVariant(
            name: "elf mummy", species: Species.Elf, speciesClass: Undead, noise: Moan,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 30, alignment: -3,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, InvisibilityDetection,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 800, nutrition: 150, corpse: Elf,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Uncommonly)},
            generationFrequency: Sometimes);

        public static readonly MonsterVariant ZombieHuman = new MonsterVariant(
            name: "human zombie", species: Species.Human, speciesClass: Undead, noise: Moan,
            initialLevel: 4, movementRate: 12, armorClass: 9, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Medium, weight: 1000, nutrition: 200, corpse: Human,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Sometimes);

        public static readonly MonsterVariant MummyHuman = new MonsterVariant(
            name: "human mummy", species: Species.Human, speciesClass: Undead, noise: Moan,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 20, alignment: -5,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 200, corpse: Human,
            generationFrequency: Sometimes);

        public static readonly MonsterVariant ZombieGiant = new MonsterVariant(
            name: "giant zombie", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 8, movementRate: 6, armorClass: 4, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 10),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Giant,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant MummyGiant = new MonsterVariant(
            name: "giant mummy", species: Species.Giant, speciesClass: Undead, noise: Moan, alignment: -7,
            initialLevel: 10, movementRate: 6, armorClass: 3, magicResistance: 20,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Giant,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant ZombieEttin = new MonsterVariant(
            name: "ettin zombie", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 10, movementRate: 12, armorClass: 2, magicResistance: 0,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Mindlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Ettin,
            behavior: Stalking, generationFlags: SmallGroup, generationFrequency: Uncommonly);

        public static readonly MonsterVariant MummyEttin = new MonsterVariant(
            name: "ettin mummy", species: Species.Giant, speciesClass: Undead, noise: Moan,
            initialLevel: 12, movementRate: 12, armorClass: 1, magicResistance: 20, alignment: -4,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Punch, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Huge, weight: 2250, nutrition: 350, corpse: Ettin,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Ghoul = new MonsterVariant(
            name: "ghoul", species: Species.Ghoul, speciesClass: Undead, noise: Growl,
            initialLevel: 12, movementRate: 8, armorClass: 4, magicResistance: 0, alignment: -2,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, Slow, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 50, corpse: None,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant Ghast = new MonsterVariant(
            name: "ghast", species: Species.Ghoul, speciesClass: Undead, noise: Growl,
            initialLevel: 15, movementRate: 8, armorClass: 2, magicResistance: 0, alignment: -3, previousStage: Ghoul,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, Paralyze, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness, Carnivorism),
            size: Medium, weight: 400, nutrition: 50, corpse: None,
            generationFrequency: Occasionally);

        public static readonly MonsterVariant Ghost = new MonsterVariant(
            name: "ghost", species: Species.Ghost, speciesClass: Undead,
            initialLevel: 10, movementRate: 3, armorClass: -5, magicResistance: 15, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, PhysicalDamage, diceCount: 1, diceSides: 1)
            },
            innateProperties: Has(
                ColdResistance, DisintegrationResistance, SleepResistance, PoisonResistance, SicknessResistance,
                StoningResistance, SlimingResistance, Flight, Phasing, Infravision, NonSolidBody, Humanoidness,
                Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking | Wandering, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly MonsterVariant Shade = new MonsterVariant(
            name: "shade", species: Species.Ghost, speciesClass: Undead, noise: Howl, previousStage: Ghost,
            initialLevel: 12, movementRate: 10, armorClass: 10, magicResistance: 25, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, Paralyze, diceCount: 2, diceSides: 6),
                new Attack(Touch, Slow, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, DisintegrationResistance, SleepResistance, PoisonResistance, SicknessResistance,
                StoningResistance, SlimingResistance, Flight, Phasing, Infravision, InvisibilityDetection, NonSolidBody,
                Humanoidness, Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking | Wandering, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly MonsterVariant WightBarrow = new MonsterVariant(
            name: "barrow wight", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 3, movementRate: 12, armorClass: 5, magicResistance: 5, alignment: -3,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Claw, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1200, nutrition: 0, corpse: None,
            behavior: Stalking | WeaponCollector | MagicUser, generationFrequency: Uncommonly);

        public static readonly MonsterVariant Wraith = new MonsterVariant(
            name: "wraith", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 6, movementRate: 12, armorClass: 4, magicResistance: 15, alignment: -6,
            attacks: new[]
            {
                new Attack(Touch, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(OnMeleeHit, DrainLife, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, DrainLife, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, StoningResistance,
                SlimingResistance, Flight, Infravision, NonSolidBody, Humanoidness, Breathlessness, NoInventory),
            size: Medium, weight: 0, nutrition: 0, corpse: None,
            behavior: Stalking, generationFrequency: Occasionally);

        public static readonly MonsterVariant Nazgul = new MonsterVariant(
            name: "nazgul", species: Species.Wraith, speciesClass: Undead, noise: Howl,
            initialLevel: 13, movementRate: 12, armorClass: 0, magicResistance: 25, alignment: -17,
            attacks: new[]
            {
                new Attack(Weapon, DrainLife, diceCount: 1, diceSides: 4),
                new Attack(Breath, Sleep, diceCount: 2, diceSides: 25),
                new Attack(OnConsumption, DrainLife, diceCount: 1, diceSides: 2)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, PoisonResistance, SicknessResistance, StoningResistance,
                SlimingResistance, Flight, Infravision, NonSolidBody, Humanoidness, Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Vampire = new MonsterVariant(
            name: "vampire", species: Species.Vampire, speciesClass: Undead | ShapeChanger, noise: Speach,
            initialLevel: 10, movementRate: 12, armorClass: 1, magicResistance: 25, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking, generationFlags: NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant VampireLord = new MonsterVariant(
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
            innateProperties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking, generationFrequency: Rarely);

        public static readonly MonsterVariant VampireMage = new MonsterVariant(
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
            innateProperties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Invisibility,
                InvisibilityDetection, Infravision, Humanoidness, Breathlessness)
                .With(ActorProperty.Remove(MaxHP, value: 20)),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | MagicUser | Stalking, generationFrequency: Rarely);

        public static readonly MonsterVariant VampireVlad = new MonsterVariant(
            name: "Vlad the Impaler", species: Species.Vampire, speciesClass: Undead | ShapeChanger,
            initialLevel: 14, movementRate: 18, armorClass: -3, magicResistance: 80, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 10),
                new Attack(Bite, DrainLife, diceCount: 1, diceSides: 10),
                new Attack(OnConsumption, Infect)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, SicknessResistance, Regeneration, Flight, Infravision, Humanoidness,
                Breathlessness, Maleness),
            size: Medium, weight: 1000, nutrition: 400, corpse: None,
            behavior: WeaponCollector | Stalking | Covetous | RangedPeaceful,
            generationFlags: NonPolymorphable | NonGenocidable, generationFrequency: Never);

        public static readonly MonsterVariant Lich = new MonsterVariant(
            name: "lich", species: Species.Lich, speciesClass: Undead, noise: Mumble,
            initialLevel: 11, movementRate: 6, armorClass: 0, magicResistance: 30, alignment: -9,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 1, diceSides: 10),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 6)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance, Regeneration,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(ColdResistance, Commonly)},
            behavior: MagicUser, generationFrequency: Rarely);

        public static readonly MonsterVariant LichDemi = new MonsterVariant(
            name: "demilich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: Lich,
            initialLevel: 14, movementRate: 9, armorClass: -2, magicResistance: 60, alignment: -12,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 3, diceSides: 4),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            innateProperties: Has(
                ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance, Regeneration,
                Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None, consumptionProperties:
                new[] {WhenConsumedAdd(ColdResistance, Commonly), WhenConsumedAdd(EnergyRegeneration, Commonly)},
            behavior: MagicUser, generationFrequency: Rarely);

        public static readonly MonsterVariant LichMaster = new MonsterVariant(
            name: "master lich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: LichDemi,
            initialLevel: 17, movementRate: 9, armorClass: -4, magicResistance: 90, alignment: -15,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, SleepResistance, SicknessResistance, PoisonResistance, VenomResistance,
                Regeneration, Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(FireResistance, Commonly), WhenConsumedAdd(ColdResistance, Commonly),
                WhenConsumedAdd(EnergyRegeneration, Commonly)
            },
            behavior: MagicUser | Covetous, generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly MonsterVariant LichArch = new MonsterVariant(
            name: "arch-lich", species: Species.Lich, speciesClass: Undead, noise: Mumble, previousStage: LichMaster,
            initialLevel: 25, movementRate: 9, armorClass: -6, magicResistance: 90, alignment: -15,
            attacks: new[]
            {
                new Attack(Touch, ColdDamage, diceCount: 5, diceSides: 6),
                new Attack(Spell, ArcaneSpell),
                new Attack(OnConsumption, Infect),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 8)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, ElectricityResistance, SleepResistance, SicknessResistance,
                PoisonResistance, VenomResistance, Regeneration, Infravision, Humanoidness, Breathlessness),
            size: Medium, weight: 600, nutrition: 50, corpse: None,
            consumptionProperties: new[]
            {
                WhenConsumedAdd(FireResistance, Commonly), WhenConsumedAdd(ColdResistance, Commonly),
                WhenConsumedAdd(ElectricityResistance, Commonly), WhenConsumedAdd(EnergyRegeneration, Commonly)
            },
            behavior: MagicUser | Covetous, generationFlags: HellOnly, generationFrequency: Rarely);

        public static readonly MonsterVariant MindFlayer = new MonsterVariant(
            name: "mind flayer", species: Illithid, noise: Gurgle,
            initialLevel: 9, movementRate: 12, armorClass: 5, magicResistance: 80, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 4),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                Levitation, InvisibilityDetection, Telepathy, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1200, nutrition: 300,
            behavior: GemCollector | GoldCollector | WeaponCollector, generationFrequency: Commonly);

        public static readonly MonsterVariant MindFlayerMaster = new MonsterVariant(
            name: "master mind flayer", species: Illithid, noise: Gurgle,
            previousStage: MindFlayer,
            initialLevel: 13, movementRate: 12, armorClass: 0, magicResistance: 90, alignment: -8,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4),
                new Attack(Suck, DrainIntelligence, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                Levitation, InvisibilityDetection, Telepathy, Infravision, Infravisibility, Humanoidness, Omnivorism),
            size: Large, weight: 1200, nutrition: 300,
            behavior: GemCollector | GoldCollector | WeaponCollector, generationFrequency: Commonly);

        public static readonly MonsterVariant Aleax = new MonsterVariant(
            name: "aleax", species: Species.Angel, speciesClass: DivineBeing, noise: Imitate,
            initialLevel: 10, movementRate: 8, armorClass: 0, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
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

        public static readonly MonsterVariant Angel = new MonsterVariant(
            name: "angel", species: Species.Angel, speciesClass: DivineBeing, noise: Speach, previousStage: Aleax,
            initialLevel: 14, movementRate: 10, armorClass: -4, magicResistance: 55, alignment: 12,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(Spell, MagicalDamage, diceCount: 2, diceSides: 6)
            },
            innateProperties: Has(
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

        public static readonly MonsterVariant Archon = new MonsterVariant(
            name: "archon", species: Species.Angel, speciesClass: DivineBeing, noise: Speach, previousStage: Angel,
            initialLevel: 19, movementRate: 16, armorClass: -6, magicResistance: 80, alignment: 15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Spell, MagicalDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
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

        // TODO: Add solars, planetars

        public static readonly MonsterVariant Homunculus = new MonsterVariant(
            name: "homunculus", species: Species.Homunculus,
            initialLevel: 2, movementRate: 12, armorClass: 6, magicResistance: 10, alignment: -7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 2),
                new Attack(Bite, Sleep, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(
                SleepResistance, PoisonResistance, Regeneration, Infravision, Infravisibility, Mindlessness, Asexuality),
            size: Small, weight: 60, nutrition: 60,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Sometimes);

        public static readonly MonsterVariant Mane = new MonsterVariant(
            name: "mane", species: Species.Homunculus, speciesClass: Demon, noise: Hiss,
            initialLevel: 3, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(SleepResistance, PoisonResistance, Infravision, Infravisibility),
            size: Medium, weight: 500, nutrition: 200, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            generationFlags: LargeGroup, behavior: Stalking, generationFrequency: Sometimes);

        public static readonly MonsterVariant Lemure = new MonsterVariant(
            name: "lemure", species: Species.Homunculus, speciesClass: Demon, noise: Hiss,
            initialLevel: 3, movementRate: 3, armorClass: 7, magicResistance: 0, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(SleepResistance, PoisonResistance, Regeneration, Infravision, Infravisibility),
            size: Medium, weight: 500, nutrition: 200, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(SleepResistance, Sometimes)},
            behavior: Stalking, generationFrequency: Sometimes);

        public static readonly MonsterVariant Imp = new MonsterVariant(
            name: "imp", species: Species.Imp, speciesClass: Demon, noise: Cuss,
            initialLevel: 3, movementRate: 12, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 3)
            },
            innateProperties: Has(Regeneration, Flight, Infravision, Infravisibility),
            size: Tiny, weight: 100, nutrition: 50, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking | Wandering, generationFrequency: Often);

        public static readonly MonsterVariant Quasit = new MonsterVariant(
            name: "quasit", species: Species.Imp, speciesClass: Demon, noise: Cuss,
            initialLevel: 3, movementRate: 15, armorClass: 2, magicResistance: 20, alignment: -7,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, DrainDexterity, diceCount: 1, diceSides: 2),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            innateProperties: Has(PoisonResistance, Regeneration, Infravision, Infravisibility),
            size: Small, weight: 200, nutrition: 100, corpse: None,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)},
            behavior: Stalking | Wandering, generationFrequency: Often);

        // TODO: fire, ice imps

        public static readonly MonsterVariant Tengu = new MonsterVariant(
            name: "tengu", species: Species.Tengu, speciesClass: ShapeChanger, noise: Squawk,
            initialLevel: 6, movementRate: 13, armorClass: 5, magicResistance: 30, alignment: 7,
            attacks: new[]
            {
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4),
                new Attack(OnConsumption, Teleport)
            },
            innateProperties: Has(PoisonResistance, Teleportation, TeleportationControl, Infravisibility, Infravision),
            size: Small, weight: 300, nutrition: 150, consumptionProperties: new[]
            {
                WhenConsumedAdd(Teleportation, Sometimes),
                WhenConsumedAdd(TeleportationControl, Uncommonly)
            }, behavior: Stalking, generationFrequency: Occasionally);

        public static readonly MonsterVariant Sandestin = new MonsterVariant(
            name: "sandestin", species: Species.Sandestin, speciesClass: ShapeChanger, noise: Cuss,
            initialLevel: 13, movementRate: 12, armorClass: 4, magicResistance: 60, alignment: -5,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 3)
            },
            innateProperties: Has(StoningResistance, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PolymorphControl, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector | AlignmentAware,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Rarely);

        public static readonly MonsterVariant Djinni = new MonsterVariant(
            name: "djinni", species: Species.Djinni, speciesClass: Demon, noise: ActorNoiseType.Djinni,
            initialLevel: 7, movementRate: 12, armorClass: 4, magicResistance: 30, alignment: 0,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 2, diceSides: 8),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                StoningResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Humanoidness),
            size: Medium, weight: 1400, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly MonsterVariant Succubus = new MonsterVariant(
            name: "succubus", species: Species.Succubus, speciesClass: Demon, noise: Seduction,
            initialLevel: 6, movementRate: 12, armorClass: 0, magicResistance: 70, alignment: -9,
            attacks: new[]
            {
                new Attack(Touch, Seduce),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Punch, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1400, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly MonsterVariant DemonWater = new MonsterVariant(
            name: "water demon", species: DemonMajor, speciesClass: Demon, noise: ActorNoiseType.Djinni,
            initialLevel: 8, movementRate: 12, armorClass: -4, magicResistance: 30, alignment: -7,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Swimming, Infravision, Infravisibility,
                Humanoidness),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: NonPolymorphable | NonGenocidable,
            generationFrequency: Never);

        public static readonly MonsterVariant DevilHorned = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, ThickHide, Infravision, Infravisibility),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Hezrou = new MonsterVariant(
            name: "hezrou", species: DemonMajor, speciesClass: Demon,
            initialLevel: 7, movementRate: 6, armorClass: -2, magicResistance: 55, alignment: -10,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Claw, PhysicalDamage, diceCount: 1, diceSides: 3),
                new Attack(Bite, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility, Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Erinyes = new MonsterVariant(
            name: "erinyes", species: DemonMajor, speciesClass: Demon,
            initialLevel: 7, movementRate: 12, armorClass: 2, magicResistance: 30, alignment: -10,
            attacks: new[]
            {
                new Attack(Weapon, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 5)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility, Humanoidness),
            size: Medium, weight: 1000, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DevilBarbed = new MonsterVariant(
            name: "barbed devil", species: DemonMajor, speciesClass: Demon, previousStage: DevilHorned,
            initialLevel: 8, movementRate: 12, armorClass: 0, magicResistance: 35, alignment: -8,
            attacks: new[]
            {
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Punch, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(Sting, PhysicalDamage, diceCount: 3, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, ThickHide, Infravision, Infravisibility,
                Humanoidness),
            size: Medium, weight: 1200, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking,
            generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Vrock = new MonsterVariant(
            name: "vrock", species: DemonMajor, speciesClass: Demon, previousStage: Erinyes,
            initialLevel: 8, movementRate: 12, armorClass: 0, magicResistance: 50, alignment: -9,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 2, diceSides: 6),
                new Attack(Bite, PhysicalDamage, diceCount: 1, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision, Infravisibility, Humanoidness),
            size: Large, weight: 1200, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Marilith = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, InvisibilityDetection,
                Infravisibility, HumanoidTorso, SerpentlikeBody, Femaleness),
            size: Large, weight: 1200, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector,
            generationFlags: HellOnly | NonGenocidable, generationFrequency: Rarely);

        public static readonly MonsterVariant DevilBone = new MonsterVariant(
            name: "bone devil", species: DemonMajor, speciesClass: Demon,
            initialLevel: 9, movementRate: 15, armorClass: -1, magicResistance: 40, alignment: -9,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 3),
                new Attack(Sting, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 1, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, Infravisibility, Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector, generationFlags: HellOnly | NonGenocidable | SmallGroup,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant DevilIce = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, ColdResistance, PoisonResistance, SicknessResistance, Infravision, InvisibilityDetection,
                Infravisibility, Humanoidness),
            size: Large, weight: 1800, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Nalfeshnee = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, InvisibilityDetection,
                Infravisibility, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly MonsterVariant FiendPit = new MonsterVariant(
            name: "pit fiend", species: DemonMajor, speciesClass: Demon, noise: Growl, previousStage: Vrock,
            initialLevel: 13, movementRate: 6, armorClass: -3, magicResistance: 65, alignment: -13,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 2),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 2),
                new Attack(Hug, PhysicalDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 3)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision, InvisibilityDetection,
                Infravisibility, Humanoidness),
            size: Large, weight: 1600, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Uncommonly);

        public static readonly MonsterVariant Balrog = new MonsterVariant(
            name: "balrog", species: DemonMajor, speciesClass: Demon, noise: Growl, previousStage: FiendPit,
            initialLevel: 16, movementRate: 5, armorClass: -2, magicResistance: 75, alignment: -14,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 8, diceSides: 4),
                new Attack(Weapon, PhysicalDamage, diceCount: 4, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 2, diceSides: 4)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravision, InvisibilityDetection,
                Infravisibility, Humanoidness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector | MagicUser, generationFlags: HellOnly | NonGenocidable,
            generationFrequency: Rarely);

        public static readonly MonsterVariant Juiblex = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, StoningResistance, AcidResistance,
                Amphibiousness, Flight, Amorphism, Headlessness, Infravision, InvisibilityDetection, Maleness),
            size: Large, weight: 1500, nutrition: 400, consumptionProperties:
                new[] {WhenConsumedAdd(PoisonResistance, Sometimes), WhenConsumedAdd(AcidResistance, Sometimes)},
            corpse: None, behavior: Stalking | RangedPeaceful | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Yeenoghu = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Infravision, InvisibilityDetection,
                Infravisibility, Maleness),
            size: Large, weight: 1500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | WeaponCollector | MagicUser | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Orcus = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, VenomResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Huge, weight: 2500, nutrition: 400,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: RangedPeaceful | Stalking | WeaponCollector | MagicUser | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Geryon = new MonsterVariant(
            name: "Geryon", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 72, movementRate: 3, armorClass: -3, magicResistance: 75, alignment: -15,
            attacks: new[]
            {
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Claw, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Sting, VenomDamage, diceCount: 2, diceSides: 4),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Infravision,
                InvisibilityDetection, SerpentlikeBody, HumanoidTorso, Maleness),
            size: Huge, weight: 2500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Dispater = new MonsterVariant(
            name: "Dispater", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 78, movementRate: 15, armorClass: -2, magicResistance: 80, alignment: -15,
            attacks: new[]
            {
                new Attack(Weapon, PhysicalDamage, diceCount: 3, diceSides: 6),
                new Attack(Spell, ArcaneSpell, diceCount: 6, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 3, diceSides: 8)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Infravision,
                InvisibilityDetection, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Bribeable | Stalking | Covetous | WeaponCollector | MagicUser,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Baalzebub = new MonsterVariant(
            name: "Baalzebub", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 89, movementRate: 9, armorClass: -5, magicResistance: 85, alignment: -20,
            attacks: new[]
            {
                new Attack(Bite, VenomDamage, diceCount: 2, diceSides: 6),
                new Attack(Gaze, Stun, diceCount: 2, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 6)
            },
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Infravision,
                InvisibilityDetection, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: RangedPeaceful | Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Asmodeus = new MonsterVariant(
            name: "Asmodeus", species: DemonMajor, speciesClass: Demon, noise: Bribe,
            initialLevel: 105, movementRate: 12, armorClass: -7, magicResistance: 90, alignment: -20,
            attacks: new[]
            {
                new Attack(Claw, VenomDamage, diceCount: 4, diceSides: 4),
                new Attack(Spell, ColdDamage, diceCount: 6, diceSides: 6),
                new Attack(OnConsumption, PoisonDamage, diceCount: 4, diceSides: 8)
            },
            innateProperties: Has(
                FireResistance, ColdResistance, PoisonResistance, SicknessResistance, Flight,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: RangedPeaceful | Bribeable | Stalking | Covetous,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Demogorgon = new MonsterVariant(
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
            innateProperties: Has(
                FireResistance, PoisonResistance, SicknessResistance, Flight, Infravisibility, Infravision,
                InvisibilityDetection, Handlessness, Humanoidness, Maleness),
            size: Large, weight: 1500, nutrition: 500,
            consumptionProperties: new[] {WhenConsumedAdd(PoisonResistance, Sometimes)}, corpse: None,
            behavior: Stalking | Covetous, generationFlags: HellOnly | NonGenocidable | NonPolymorphable,
            generationFrequency: Never);

        public static readonly MonsterVariant Death = new MonsterVariant(
            name: "Death", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Death, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Death, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Death, diceCount: 8, diceSides: 8)
            },
            innateProperties: Has(
                AcidResistance, FireResistance, ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, SicknessResistance, DecayResistance, StoningResistance, SlimingResistance,
                Breathlessness, Reanimation, Regeneration, Flight, TeleportationControl, PolymorphControl,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, behavior: Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Pestilence = new MonsterVariant(
            name: "Pestilence", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Pestilence, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Pestilence, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Pestilence, diceCount: 8, diceSides: 8)
            },
            innateProperties: Has(
                AcidResistance, FireResistance, ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, SicknessResistance, DecayResistance, StoningResistance, SlimingResistance,
                Breathlessness, Reanimation, Regeneration, Flight, TeleportationControl, PolymorphControl,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: 0, behavior: Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        public static readonly MonsterVariant Famine = new MonsterVariant(
            name: "Famine", species: Horseman, noise: Rider,
            initialLevel: 30, movementRate: 12, armorClass: -5, magicResistance: 100, alignment: 0,
            attacks: new[]
            {
                new Attack(Touch, AttackEffect.Famine, diceCount: 8, diceSides: 8),
                new Attack(Touch, AttackEffect.Famine, diceCount: 8, diceSides: 8),
                new Attack(OnConsumption, AttackEffect.Famine, diceCount: 8, diceSides: 8)
            },
            innateProperties: Has(
                AcidResistance, FireResistance, ColdResistance, ElectricityResistance, SleepResistance, PoisonResistance,
                VenomResistance, SicknessResistance, DecayResistance, StoningResistance, SlimingResistance,
                Breathlessness, Reanimation, Regeneration, Flight, TeleportationControl, PolymorphControl,
                Infravisibility, Infravision, InvisibilityDetection, Humanoidness, Maleness),
            size: Medium, weight: 1000, nutrition: -5000, behavior: Stalking | Displacing,
            generationFlags: HellOnly | NonGenocidable | NonPolymorphable, generationFrequency: Never);

        protected MonsterVariant(
            string name,
            Species species,
            byte initialLevel,
            byte movementRate,
            sbyte armorClass,
            byte magicResistance,
            IReadOnlyList<Attack> attacks,
            List<ActorProperty> innateProperties,
            Size size,
            short weight,
            short nutrition,
            Frequency generationFrequency,
            ActorNoiseType noise = Silent,
            IReadOnlyList<Tuple<ActorProperty, Frequency>> consumptionProperties = null,
            SpeciesClass speciesClass = SpeciesClass.None,
            MonsterVariant previousStage = null,
            MonsterVariant corpse = null,
            sbyte alignment = 0,
            GenerationFlags generationFlags = GenerationFlags.None,
            MonsterBehavior behavior = MonsterBehavior.None)
            : base(
                name, species, innateProperties, movementRate, size, weight, nutrition, consumptionProperties,
                speciesClass)
        {
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
            ArmorClass = armorClass;
            MagicResistance = magicResistance;
            Attacks = attacks;
            GenerationFlags = generationFlags;
            GenerationFrequency = generationFrequency;
            Behavior = behavior;
        }

        // Taxonomy
        public sbyte Alignment { get; }
        public ActorNoiseType Noise { get; }
        public MonsterVariant PreviousStage { get; private set; }
        public MonsterVariant NextStage { get; private set; }
        public MonsterVariant Corpse { get; private set; }

        // Combat properties
        public byte InitialLevel { get; }
        public sbyte ArmorClass { get; }

        /// <summary>
        ///     Base chance to avoid a magic attack
        /// </summary>
        public byte MagicResistance { get; }

        public IReadOnlyList<Attack> Attacks { get; }

        // Generation
        public GenerationFlags GenerationFlags { get; }
        public Frequency GenerationFrequency { get; }

        public MonsterBehavior Behavior { get; }

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
    }
}